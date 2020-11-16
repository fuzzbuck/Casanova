using System.Net;
using Casanova.core.main.world;
using Casanova.core.net.server;
using Casanova.core.net.types;
using Casanova.core.types;
using Casanova.ui;
using Casanova.ui.fragments;
using Godot;
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
            ChatMessage
        }

        /// <summary>Sent from client to server.</summary>
        public enum ClientPackets
        {
            WelcomeReceived = 1,
            PlayerMovement,
            ChatMessage
        }
        public class ClientHandle
        {
            public class Receive
            {
                public static void Welcome(Packet _packet)
                {
                    var _msg = _packet.ReadString();
                    var _myId = _packet.ReadInt();

                    Client.myId = _myId;
                    Send.WelcomeConfirmation(Vars.PersistentData.username, Vars.PersistentData.UnitType);

                    // create world, etc.
                    NetworkManager.ConfirmConnect();
                    Client.udp.Connect(((IPEndPoint) Client.tcp.socket.Client.LocalEndPoint).Port);

                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        Interface.Utils.CreateInformalMessage($"Server: {_msg}", 10);
                    });
                }

                public static void PlayerJoin(Packet _packet)
                {
                    var _id = _packet.ReadInt();
                    var _username = _packet.ReadString();

                    if (Server.IsHosting)
                        return;

                    NetworkManager.CreatePlayer(NetworkManager.loc.CLIENT, _id, _username);
                }

                public static void PlayerMovement(Packet _packet)
                {
                    var id = _packet.ReadInt();

                    if (!NetworkManager.PlayersGroup.ContainsKey(id))
                        return;

                    var pos = _packet.ReadVector2();
                    var axis = _packet.ReadVector2();
                    var speed = _packet.ReadFloat();
                    var rotation = _packet.ReadFloat();

                    var player = NetworkManager.PlayersGroup[id];
                    var unitBody = player?.Unit.Body;
                    if (unitBody != null && !player.IsLocal)
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
                    var _id = _packet.ReadInt();

                    if (!NetworkManager.PlayersGroup.ContainsKey(_id))
                        return;
                    
                    // todo: handle player disconnect
                }

                public static void ChatMessage(Packet _packet)
                {
                    var _id = _packet.ReadInt();

                    if (_id != 0 && !NetworkManager.PlayersGroup.ContainsKey(_id))
                        return;

                    var message = _packet.ReadString();

                    Chat.instance?.SendMessage(message,
                        _id == 0 ? new Player(0, "server", null) : NetworkManager.PlayersGroup[_id]);
                }
            }

            public class Send
            {
                public static void WelcomeConfirmation(string _username, UnitType _type)
                {
                    using (var _packet = new Packet((int) ClientPackets.WelcomeReceived))
                    {
                        _packet.Write(Client.myId);
                        _packet.Write(_username);
                        _packet.Write(_type);

                        Client.SendTCPData(_packet);
                    }
                }

                public static void PlayerMovement(Vector2 _position, Vector2 _axis, float _speed, float _rotation)
                {
                    using (var _packet = new Packet((int) ClientPackets.PlayerMovement))
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
                    var _clientIdCheck = _packet.ReadInt();
                    var _username = _packet.ReadString();
                    var _type = _packet.ReadUnitType();

                    GD.Print(
                        $"{Server.Clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient} with type {_type.Name}.");
                    if (_fromClient != _clientIdCheck)
                        GD.Print(
                            $"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");

                    Server.Clients[_fromClient].SendIntoGame(_username, _type);
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