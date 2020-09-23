using System;
using System.Diagnostics;
using System.Net;
using Casanova.core.main;
using Casanova.core.main.world;
using Casanova.core.net.server;
using Casanova.core.net.types;
using Casanova.ui.fragments;
using Godot;
using Client = Casanova.core.net.client.Client;

namespace Casanova.core.net
{
    public class Packets
    {
        public class ClientHandle
        {
            public class Receive
            {
                public static void Welcome(Packet _packet)
                {
                    string _msg = _packet.ReadString();
                    int _myId = _packet.ReadInt();

                    GD.Print($"Message from server: {_msg}");
                    Client.myId = _myId;
                    Send.WelcomeConfirmation(Vars.PersistentData.username);
                    
                    // create world, etc.
                    NetworkManager.ConfirmConnect();

                    Client.udp.Connect(((IPEndPoint)Client.tcp.socket.Client.LocalEndPoint).Port);
                }

                public static void SpawnPlayer(Packet _packet)
                {
                    int _id = _packet.ReadInt();
                    string _username = _packet.ReadString();

                    if (Server.IsHosting)
                        return;

                    GD.Print($"Received spawn packet from server for {_username} with id {_id}");
                    NetworkManager.CreatePlayer(NetworkManager.loc.CLIENT, _id, _username);
                }
                
                public static void PlayerMovement(Packet _packet)
                {
                    int id = _packet.ReadInt();

                    if (!NetworkManager.PlayersGroup.ContainsKey(id))
                        return;
                    
                    Vector2 pos = _packet.ReadVector2();
                    Vector2 axis = _packet.ReadVector2();
                    float speed = _packet.ReadFloat();
                    float rotation = _packet.ReadFloat();
                    // todo: use UnitType for rotation speed, prediction & etc..

                    var player = NetworkManager.PlayersGroup[id];
                    var unitBody = player?.PlayerUnit.Body;
                    if (unitBody != null && !player.IsLocal)
                    {
                        unitBody.Axis = axis;
                        //unit.Speed = speed;

                        if (unitBody.Position.DistanceTo(pos) > Vars.Networking.unit_desync_treshold)
                            unitBody.Position = unitBody.Position.LinearInterpolate(pos, Vars.Networking.unit_desync_interpolation);

                    }
                }

                public static void PlayerDisconnect(Packet _packet)
                {
                    int _id = _packet.ReadInt();
                    
                    if (!NetworkManager.PlayersGroup.ContainsKey(_id))
                        return;

                    NetworkManager.DestroyPlayer(_id);
                }

                public static void ChatMessage(Packet _packet)
                {
                    int _id = _packet.ReadInt();
                    
                    if (_id != 0 && !NetworkManager.PlayersGroup.ContainsKey(_id))
                        return;
                    
                    string message = _packet.ReadString();
                    
                    GD.Print($"Received chat message from {_id}: {message}");
                    
                    Chat.instance?.SendMessage(message, _id == 0 ? new Player(0, "server", null, false) : NetworkManager.PlayersGroup[_id]);
                }
                
            }

            public class Send
            {
                public static void WelcomeConfirmation(string _username)
                {
                    using (Packet _packet = new Packet((int) ClientPackets.welcomeReceived))
                    {
                        _packet.Write(Client.myId);
                        _packet.Write(_username);

                        Client.SendTCPData(_packet);
                    }
                }

                public static void PlayerMovement(Vector2 _position, Vector2 _axis, float _speed, float _rotation)
                {
                    using (Packet _packet = new Packet((int) ClientPackets.playerMovement))
                    {
                        
                        _packet.Write(_position);
                        _packet.Write(_axis);
                        _packet.Write(_speed);
                        _packet.Write(_rotation);

                        Client.SendUDPData(_packet);
                    }
                }

                public static void ChatMessage(string _message)
                {
                    using (Packet _packet = new Packet((int) ClientPackets.chatMessage))
                    {
                        _packet.Write(_message);

                        Client.SendTCPData(_packet);
                    }
                }
            }
        }

        public class ServerHandle
        {
            public class Receive
            {
                public static void WelcomeConfirmation(int _fromClient, Packet _packet)
                {
                    int _clientIdCheck = _packet.ReadInt();
                    string _username = _packet.ReadString();

                    GD.Print($"{Server.Clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
                    if (_fromClient != _clientIdCheck)
                    {
                        GD.Print($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
                    }
                    // TODO: send player into game
                    Server.Clients[_fromClient].SendIntoGame(_username);
                }

                public static void PlayerMovement(int _fromClient, Packet _packet)
                {
                    Vector2 pos = _packet.ReadVector2();
                    Vector2 axis = _packet.ReadVector2();
                    float speed = _packet.ReadFloat();
                    float rotation = _packet.ReadFloat();
                    // todo: use UnitType for rotation speed, prediction & etc..

                    var _plr = Server.Clients[_fromClient].player;
                    var unitBody = _plr.PlayerUnit.Body;
                    if (!_plr.IsLocal && unitBody != null)
                    {
                        unitBody.Axis = axis;
                        unitBody.Speed = speed;
                        unitBody.Rotation = rotation;
                        unitBody.Position = pos;
                    }
                    if (unitBody != null)
                        Send.PlayerMovement(_plr);
                }

                public static void ChatMessage(int _fromClient, Packet _packet)
                {
                    String message = _packet.ReadString();
                    
                    Send.ChatMessage(_fromClient, message);
                }
            }

            public class Send
            {
                public static void Welcome(int _toClient, string _msg)
                {
                    using (Packet _packet = new Packet((int)ServerPackets.welcome))
                    {
                        _packet.Write(_msg);
                        _packet.Write(_toClient);

                        ServerSend.SendTCPData(_toClient, _packet);
                    }
                }

                public static void SpawnPlayer(int _toClient, Player _player)
                {
                    using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
                    {
                        _packet.Write(_player.Id);
                        _packet.Write(_player.Username);
                        _packet.Write(_toClient);

                        ServerSend.SendTCPData(_toClient, _packet);
                    }
                }

                public static void PlayerMovement(Player _player)
                {
                    using (Packet _packet = new Packet((int)ServerPackets.playerMovement))
                    {
                        var unitBody = _player.PlayerUnit.Body;
                        
                        _packet.Write(_player.Id);
                        _packet.Write(unitBody.Position);
                        _packet.Write(unitBody.Axis);
                        _packet.Write(unitBody.Speed);
                        _packet.Write(unitBody.Rotation);

                        ServerSend.SendUDPDataToAll(_player.Id, _packet);
                    }
                }
                
                public static void PlayerDisconnect(int _id)
                {
                    using (Packet _packet = new Packet((int)ServerPackets.disconnectPlayer))
                    {
                        _packet.Write(_id);
                        
                        // replicate to all clients
                        ServerSend.SendTCPDataToAll(_packet);
                    }
                    
                    // destroy player server-side
                    NetworkManager.DestroyPlayer(_id);
                }
                
                public static void ChatMessage(int _id, string message)
                {
                    using (Packet _packet = new Packet((int)ServerPackets.chatMessage))
                    {
                        _packet.Write(_id);
                        _packet.Write(message);
                        
                        ServerSend.SendTCPDataToAll(_packet);
                    }
                }
            }
        }
    }
}