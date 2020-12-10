using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Casanova.core.content;
using Casanova.core.main;
using Casanova.core.main.units;
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
            PlayerDisconnect,
            PlayerConnect,
            UnitCreate,
            UnitMovement,
            UnitOwnership,
            UnitRemove,
            ChatMessage,
            InformalMessage
        }

        /// <summary>Sent from client to server.</summary>
        public enum ClientPackets
        {
            WelcomeReceived = 1,
            UnitMovement,
            ChatMessage
        }

        public delegate void PacketHandler(Packet _packet);
        public static Dictionary<int, PacketHandler> handlers = new Dictionary<int, PacketHandler>
        {
            {(int) ServerPackets.Welcome, ClientHandle.Receive.Welcome},
            {(int) ServerPackets.PlayerDisconnect, ClientHandle.Receive.PlayerDisconnect},
            {(int) ServerPackets.PlayerConnect, ClientHandle.Receive.PlayerConnect},
            {(int) ServerPackets.UnitCreate, ClientHandle.Receive.UnitCreate},
            {(int) ServerPackets.UnitMovement, ClientHandle.Receive.UnitMovement},
            {(int) ServerPackets.UnitOwnership, ClientHandle.Receive.UnitOwnership},
            {(int) ServerPackets.UnitRemove, ClientHandle.Receive.UnitRemove},
            {(int) ServerPackets.ChatMessage, ClientHandle.Receive.ChatMessage},
            {(int) ServerPackets.InformalMessage, ClientHandle.Receive.InformalMessage}
        };
        public class ClientHandle
        {
            public class Receive
            {
                public static void Welcome(Packet _packet)
                {
                    var _myId = _packet.ReadShort();

                    Client.myId = _myId;
                    Client.udp.Connect(((IPEndPoint) Client.tcp.socket.Client.LocalEndPoint).Port);

                    // create world, etc.
                    NetworkManager.ConfirmConnect();
                    Send.WelcomeConfirmation(Vars.PersistentData.username);
                }

                public static void InformalMessage(Packet _packet)
                {
                    var _msg = _packet.ReadString();
                    if(_msg != String.Empty)
                        Interface.Utils.CreateInformalMessage($"{_msg}", 10);
                }

                public static void UnitCreate(Packet _packet)
                {
                    var id = _packet.ReadInt();
                    var type = _packet.ReadUnitType();
                    var pos = _packet.ReadVector2();
                    var rotation = _packet.ReadFloat();

                    NetworkManager.CreateUnit(NetworkManager.loc.CLIENT, id, type, pos, rotation);
                }

                public static void UnitMovement(Packet _packet)
                {
                    var id = _packet.ReadInt();

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
                
                public static void UnitOwnership(Packet _packet)
                {
                    var unit = _packet.ReadUnit();
                    var ownerId = _packet.ReadShort();

                    if (ownerId == Client.myId)
                    {
                        PlayerController.TakeOwnership(unit);
                    }
                    else
                    {
                        // todo: register ownership to another player
                    }
                }
                
                public static void UnitRemove(Packet _packet)
                {
                    var _id = _packet.ReadInt();
                    NetworkManager.RemoveUnit(_id);
                } 

                public static void PlayerDisconnect(Packet _packet)
                {
                    var _id = _packet.ReadShort();
                    
                    NetworkManager.RemovePlayer(_id);
                }
                
                public static void PlayerConnect(Packet _packet)
                {
                    var _id = _packet.ReadShort();
                    var _name = _packet.ReadString();

                    NetworkManager.CreatePlayer(NetworkManager.loc.CLIENT, _id, _name);
                    if (_id == Client.myId)
                        PlayerController.LocalPlayer = NetworkManager.PlayersGroup[_id];
                }

                public static void ChatMessage(Packet _packet)
                {
                    var _id = _packet.ReadShort();
                    var message = _packet.ReadString();

                    Chat.instance?.SendMessage(message,
                        _id == 0 ? null : NetworkManager.PlayersGroup[_id]);
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
                public static void UnitMovement(int _id, Vector2 _position, Vector2 _axis, float _speed, float _rotation)
                {
                    using (var _packet = new Packet((int) ClientPackets.UnitMovement))
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

                        Client.SendUDPData(_packet);
                    }
                }
            }
        }

        public class ServerHandle
        {
            public class Receive
            {
                public static void WelcomeConfirmation(short _fromClient, Packet _packet)
                {
                    var _clientIdCheck = _packet.ReadShort();
                    var _username = _packet.ReadString();

                    GD.Print(
                        $"{Server.Clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
                    if (_fromClient != _clientIdCheck)
                        GD.Print(
                            $"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
                    
                    var player = NetworkManager.CreatePlayer(NetworkManager.loc.SERVER, _clientIdCheck, _username);
                    
                    
                    
                    // notify other clients that this player has connected
                    Send.PlayerConnect(player);
                    
                    // send chat message
                    Send.ChatMessage(0, $"[color=#edc774]{_username} has connected.[/color]");
                    
                    // create unit for this player
                    var unit = NetworkManager.CreateUnit(NetworkManager.loc.SERVER, 0, UnitTypes.crimson);
                    
                    // notify clients that this player will take ownership of this unit
                    Send.UnitOwnership(unit, player);
                }

                public static void UnitMovement(short _fromClient, Packet _packet)
                {
                    var unitNetId = _packet.ReadInt();
                    
                    // check if player can control this unit
                    if (!(NetworkManager.UnitsGroup.ContainsKey(unitNetId) &&
                          NetworkManager.PlayersGroup[_fromClient].Unit != null &&
                          NetworkManager.PlayersGroup[_fromClient].Unit.netId == unitNetId))
                        return;


                    var pos = _packet.ReadVector2();
                    var axis = _packet.ReadVector2();
                    var speed = _packet.ReadFloat();
                    var rotation = _packet.ReadFloat();
                    
                    // todo: more smooth client side prediction
                    var _plr = NetworkManager.PlayersGroup[_fromClient];
                    var unitBody = _plr.Unit.Body;
                    if (!_plr.isLocal && unitBody != null)
                    {
                        unitBody.Axis = axis;
                        unitBody.Speed = speed;
                        unitBody.Rotation = rotation;
                        unitBody.Position = pos;
                    }

                    if (unitBody != null)
                        Send.UnitMovement(_plr.Unit, _plr);
                }

                public static void ChatMessage(short _fromClient, Packet _packet)
                {
                    var message = _packet.ReadString();

                    Send.ChatMessage(_fromClient, message);
                }
            }

            public class Send
            {
                public static void Welcome(short _toClient)
                {
                    using (var _packet = new Packet((int) ServerPackets.Welcome))
                    {
                        _packet.Write(_toClient);

                        Server.SendTCPData(_toClient, _packet);
                    }
                }
                
                public static void PlayerConnect(Player player)
                {
                    using (var _packet = new Packet((int) ServerPackets.PlayerConnect))
                    {
                        _packet.Write(player.netId);
                        _packet.Write(player.Username);
                        
                        Server.SendTCPDataToAll(_packet);
                    }
                }
                
                public static void PlayerDisconnect(int _id)
                {
                    using (var _packet = new Packet((int) ServerPackets.PlayerDisconnect))
                    {
                        _packet.Write(_id);
                        
                        Server.SendTCPDataToAll(_packet);
                    }
                }

                public static void UnitCreate(int netId, UnitType type, Vector2 position, float rotation)
                {
                    using (var _packet = new Packet((int) ServerPackets.UnitCreate))
                    {
                        _packet.Write(netId);
                        _packet.Write(type);
                        _packet.Write(position);
                        _packet.Write(rotation);
                        
                        Server.SendTCPDataToAll(NetworkManager.HostPlayer.netId, _packet);
                    }
                }

                public static void UnitMovement(Unit unit, Player controller=null)
                {
                    using (var _packet = new Packet((int) ServerPackets.UnitMovement))
                    {
                        var unitBody = unit.Body;

                        _packet.Write(unit.netId);
                        _packet.Write(unitBody.Position);
                        _packet.Write(unitBody.Axis);
                        _packet.Write(unitBody.Speed);
                        _packet.Write(unitBody.Rotation);

                        if (controller != null)
                            Server.SendUDPDataToAll(controller.netId, _packet);
                        else
                        {
                            Server.SendUDPDataToAll(_packet);
                        }
                    }
                }
                
                public static void UnitOwnership(Unit unit, Player owner)
                {
                    using (var _packet = new Packet((int) ServerPackets.UnitOwnership))
                    {
                        _packet.Write(unit);
                        _packet.Write(owner.netId);
                        
                        Server.SendTCPDataToAll(_packet);
                    }
                }
                
                public static void UnitRemove(int id)
                {
                    using (var _packet = new Packet((int) ServerPackets.UnitRemove))
                    {
                        _packet.Write(id);

                        Server.SendTCPDataToAll(_packet);
                    }
                }

                public static void ChatMessage(short _id, string message)
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