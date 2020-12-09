using System;
using System.Net;
using Casanova.core.main.world;
using Casanova.core.net.server;
using Casanova.core.net.types;
using Casanova.core.types;
using Casanova.ui;
using Casanova.ui.fragments;
using Godot;
using Godot.Collections;
using Client = Casanova.core.net.client.Client;

namespace Casanova.core.net
{
    public class Packets
    {
        /// <summary>Sent from server to client.</summary>
        public enum ServerPackets
        {
            Welcome = 1,
            PlayerJoin,
            PlayerDisconnect,
            PlayerMovement,
            ChatMessage,
            InformalMessage
        }

        /// <summary>Sent from client to server.</summary>
        public enum ClientPackets
        {
            WelcomeReceived = 1,
            PlayerMovement,
            ChatMessage
        }

        public delegate void PacketHandler(Packet _packet);
        public static Dictionary<int, PacketHandler> handlers = new Dictionary<int, PacketHandler>
        {
            {(int) ServerPackets.Welcome, ClientHandle.Receive.Welcome},
            {(int) ServerPackets.PlayerJoin, ClientHandle.Receive.PlayerJoin},
            {(int) ServerPackets.PlayerDisconnect, ClientHandle.Receive.PlayerDisconnect},
            {(int) ServerPackets.PlayerMovement, ClientHandle.Receive.UnitMovement},
            {(int) ServerPackets.ChatMessage, ClientHandle.Receive.ChatMessage},
            {(int) ServerPackets.InformalMessage, ClientHandle.Receive.InformalMessage}
        };
        public class ClientHandle
        {
            public class Receive
            {
                public static void Welcome(Packet _packet)
                {
                    var _msg = _packet.ReadString();
                    var _myId = _packet.ReadShort();

                    Client.myId = _myId;
                    
                    Client.udp.Connect(((IPEndPoint) Client.tcp.socket.Client.LocalEndPoint).Port);
                    
                    Send.WelcomeConfirmation(Vars.PersistentData.username);
                    // create world, etc.
                    NetworkManager.ConfirmConnect();
                }

                public static void InformalMessage(Packet _packet)
                {
                    var _msg = _packet.ReadString();
                    if(_msg != String.Empty)
                        Interface.Utils.CreateInformalMessage($"{_msg}", 10);
                }

                public static void PlayerJoin(Packet _packet)
                {
                    var _id = _packet.ReadShort();
                    var _username = _packet.ReadString();

                    if (Server.IsHosting)
                        return;

                    NetworkManager.CreatePlayer(NetworkManager.loc.CLIENT, _id, _username);
                }

                public static void UnitMovement(Packet _packet)
                {
                    var id = _packet.ReadShort();

                    if (!NetworkManager.UnitsGroup.ContainsKey(id))
                        return;

                    var pos = _packet.ReadVector2();
                    var axis = _packet.ReadVector2();
                    var speed = _packet.ReadFloat();
                    var rotation = _packet.ReadFloat();

                    var unit = NetworkManager.UnitsGroup[id];
                    var unitBody = unit.Body;
                    
                    if (unitBody != null)
                    {
                        unitBody.Axis = axis;
                        if (unitBody.Position.DistanceTo(pos) > Vars.Networking.unit_desync_treshold)
                        {
                            unitBody.CollisionHitbox.Disabled = true;
                            unitBody.Position =
                                unitBody.Position.LinearInterpolate(pos, Vars.Networking.unit_desync_interpolation);
                        }
                        else
                        {
                            unitBody.CollisionHitbox.Disabled = false;
                        }
                    }
                }

                public static void PlayerDisconnect(Packet _packet)
                {
                    var _id = _packet.ReadShort();

                    if (!NetworkManager.PlayersGroup.ContainsKey(_id))
                        return;
                    
                    // todo: handle player disconnect
                }

                public static void ChatMessage(Packet _packet)
                {
                    var _id = _packet.ReadShort();

                    if (_id != 0 && !NetworkManager.PlayersGroup.ContainsKey(_id))
                        return;

                    var message = _packet.ReadString();

                    Chat.instance?.SendMessage(message,
                        _id == 0 ? new Player(0, "server", null) : NetworkManager.PlayersGroup[_id]);
                }
            }

            public class Send
            {
                public static void WelcomeConfirmation(string _username)
                {
                    using (var _packet = new Packet((int) ClientPackets.WelcomeReceived))
                    {
                        _packet.Write(Client.myId);
                        _packet.Write(_username);

                        Client.SendUDPData(_packet);
                    }
                }
                
                
                /// <param name="_id">The id of the unit.</param>
                public static void UnitMovement(short _id, Vector2 _position, Vector2 _axis, float _speed, float _rotation)
                {
                    using (var _packet = new Packet((int) ClientPackets.PlayerMovement))
                    {
                        _packet.Write(_id);
                        _packet.Write(_position);
                        _packet.Write(_axis);
                        _packet.Write(_speed);
                        _packet.Write(_rotation);

                        Client.SendUDPData(_packet);
                    }
                }

                public static void ChatMessage(string _message)
                {
                    using (var _packet = new Packet((int) ClientPackets.ChatMessage))
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
                    var _clientIdCheck = _packet.ReadShort();
                    var _username = _packet.ReadString();

                    GD.Print(
                        $"{Server.Clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
                    if (_fromClient != _clientIdCheck)
                        GD.Print(
                            $"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");

                    Server.Clients[_fromClient].SendIntoGame(_username);
                }

                public static void PlayerMovement(int _fromClient, Packet _packet)
                {
                    if (!Server.Clients.ContainsKey(_fromClient))
                        return;

                    var pos = _packet.ReadVector2();
                    var axis = _packet.ReadVector2();
                    var speed = _packet.ReadFloat();
                    var rotation = _packet.ReadFloat();
                    // todo: use UnitType for rotation speed, prediction & etc..

                    var _plr = Server.Clients[_fromClient].player;
                    var unitBody = _plr.Unit.Body;
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
                    var message = _packet.ReadString();

                    Send.ChatMessage(_fromClient, message);
                }
            }

            public class Send
            {
                public static void Welcome(int _toClient, string _msg)
                {
                    using (var _packet = new Packet((int) ServerPackets.Welcome))
                    {
                        _packet.Write(_msg);
                        _packet.Write(_toClient);

                        Server.SendTCPData(_toClient, _packet);
                    }
                }

                public static void PlayerJoin(int _toClient, Player _player)
                {
                    using (var _packet = new Packet((int) ServerPackets.PlayerJoin))
                    {
                        _packet.Write(_player.Id);
                        _packet.Write(_player.Username);
                        _packet.Write(_toClient);

                        Server.SendTCPData(_toClient, _packet);
                    }
                }

                public static void PlayerMovement(Player _player)
                {
                    using (var _packet = new Packet((int) ServerPackets.PlayerMovement))
                    {
                        var unitBody = _player.Unit.Body;

                        _packet.Write(_player.Id);
                        _packet.Write(unitBody.Position);
                        _packet.Write(unitBody.Axis);
                        _packet.Write(unitBody.Speed);
                        _packet.Write(unitBody.Rotation);

                        Server.SendUDPDataToAll(_player.Id, _packet);
                    }
                }

                public static void PlayerDisconnect(int _id)
                {
                    using (var _packet = new Packet((int) ServerPackets.PlayerDisconnect))
                    {
                        _packet.Write(_id);

                        // replicate to all clients
                        Server.SendTCPDataToAll(_packet);
                    }
                }

                public static void ChatMessage(int _id, string message)
                {
                    using (var _packet = new Packet((int) ServerPackets.ChatMessage))
                    {
                        _packet.Write(_id);
                        _packet.Write(message);

                        Server.SendTCPDataToAll(_packet);
                    }
                }
            }
        }
    }
}