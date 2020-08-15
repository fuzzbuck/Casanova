using System;
using System.Diagnostics;
using System.Net;
using Casanova.core.main.units.Player;
using Casanova.core.main.world;
using Casanova.core.net.server;
using Casanova.core.net.types;
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
                    Client.instance.myId = _myId;
                    Send.WelcomeConfirmation("username");

                    Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
                }

                public static void SpawnPlayer(Packet _packet)
                {
                    int _id = _packet.ReadInt();
                    string _username = _packet.ReadString();
                    
                    GD.Print($"Received spawn packet from server for {_username} with id {_id}");
                    NetworkManager.instance.SpawnPlayer(_id, _username);
                }
                
                public static void PlayerMovement(Packet _packet)
                {
                    int id = _packet.ReadInt();
                    Vector2 pos = _packet.ReadVector2();
                    Vector2 axis = _packet.ReadVector2();
                    float speed = _packet.ReadFloat();
                    float rotation = _packet.ReadFloat();
                    // todo: use UnitType for rotation speed, prediction & etc..

                    var player = NetworkManager.playersGroup.ContainsKey(id) ? NetworkManager.playersGroup[id] : null;
                    if (player != null)
                    {
                        var unit = player.unit;
                        if (unit != null)
                        {
                            unit.Axis = axis;
                            unit.Speed = speed;
                            if (unit.Position.DistanceTo(pos) > 4)
                                unit.Position = pos;
                        }
                    }
                }
                
            }

            public class Send
            {
                public static void WelcomeConfirmation(string _username)
                {
                    using (Packet _packet = new Packet((int) ClientPackets.welcomeReceived))
                    {
                        _packet.Write(Client.instance.myId);
                        _packet.Write(_username);

                        ClientSend.SendTCPData(_packet);
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

                        ClientSend.SendUDPData(_packet);
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
                    var unit = _plr.unit;
                    if (unit != null)
                    {
                        unit.Axis = axis;
                        unit.Speed = speed;
                        unit.Rotation = rotation;
                        unit.Position = pos;
                    }
                    // replicate movement to other clients
                    Send.PlayerMovement(_plr);
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
                        _packet.Write(_player.id);
                        _packet.Write(_player.username);
                        _packet.Write(_toClient);

                        ServerSend.SendTCPData(_toClient, _packet);
                    }
                }

                public static void PlayerMovement(Player _player)
                {
                    using (Packet _packet = new Packet((int)ServerPackets.playerMovement))
                    {
                        var unit = _player.unit;
                        _packet.Write(_player.id);
                        _packet.Write(unit.Position);
                        _packet.Write(unit.Axis);
                        _packet.Write(unit.Speed);
                        _packet.Write(unit.Rotation);

                        ServerSend.SendUDPDataToAll(_player.id, _packet);
                    }
                }
            }
        }
    }
}