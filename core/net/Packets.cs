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
using Casanova.core.utils;
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
            PlayerCreate,
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
            {(int) ServerPackets.PlayerCreate, ClientHandle.Receive.PlayerConnect},
            {(int) ServerPackets.UnitCreate, ClientHandle.Receive.UnitCreate},
            {(int) ServerPackets.UnitMovement, ClientHandle.Receive.UnitMovement},
            {(int) ServerPackets.UnitOwnership, ClientHandle.Receive.UnitOwnership},
            {(int) ServerPackets.UnitRemove, ClientHandle.Receive.UnitRemove},
            {(int) ServerPackets.ChatMessage, ClientHandle.Receive.ChatMessage},
            {(int) ServerPackets.InformalMessage, ClientHandle.Receive.InformalMessage}
        };
        
        public class ClientHandle
        {
            // handles packets sent from the server to the client
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
                    var vel = _packet.ReadVector2();
                    var rotation = _packet.ReadFloat();

                    var unit = NetworkManager.UnitsGroup[id];
                    var unitBody = unit.Body;
                    
                    if (unitBody != null)
                    {
                        unitBody.Axis = axis;
                        unitBody.DesiredPosition = pos;
                        unitBody.RotateBy = rotation;

                        /* Rotation Interpolation is done automatically */
                        unitBody.ApplyNetworkPosition();
                    }
                }
                
                public static void UnitOwnership(Packet _packet)
                {
                    var unit = _packet.ReadUnit();
                    var owner = _packet.ReadPlayer();
                    
                    NetworkManager.UnitOwnership(NetworkManager.loc.CLIENT, unit, owner);
                }
                
                public static void UnitRemove(Packet _packet)
                {
                    var _unit = _packet.ReadUnit();
                    
                    if(_unit != null)
                        NetworkManager.DestroyUnit(NetworkManager.loc.CLIENT, _unit);
                } 

                public static void PlayerDisconnect(Packet _packet)
                {
                    var _plr = _packet.ReadPlayer();
                    
                    NetworkManager.DestroyPlayer(NetworkManager.loc.CLIENT, _plr);
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

            
            // sends packets from the client to the server
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
                public static void UnitMovement(int _id, Vector2 _position, Vector2 _axis, Vector2 _velocity, float _rotation)
                {
                    using (var _packet = new Packet((int) ClientPackets.UnitMovement))
                    {
                        _packet.Write(_id);
                        _packet.Write(_position);
                        _packet.Write(_axis);
                        _packet.Write(_velocity);
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
                
                // handles packets sent to the server from the client/s
                public static void WelcomeConfirmation(short _fromClient, Packet _packet)
                {
                    var _clientIdCheck = _packet.ReadShort();
                    var _username = _packet.ReadString();

                    GD.Print(
                        $"{Server.Clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
                    if (_fromClient != _clientIdCheck)
                        GD.PrintErr(
                            $"[player \"{_username}\"] (ID: {_fromClient}) has assumed the wrong [client ID ({_clientIdCheck})]!");
                    
                    
                    var player = NetworkManager.CreatePlayer(NetworkManager.loc.SERVER, _clientIdCheck, _username);
                    
                    
                    // LOAD WORLD DATA FOR NEW PLAYERS

                    // notify this client of already existing units
                    foreach (Unit u in NetworkManager.UnitsGroup.Values)
                    {
                        Send.UnitCreate(u.netId, u.Type, u.Body.GlobalPosition, u.Body.GlobalRotation);
                    }
                    
                    // notify other clients that this player has connected & about connections of other players
                    foreach(Player p in NetworkManager.PlayersGroup.Values)
                    {
                        // notify other clients that this player joined
                        Send.PlayerCreate(player, p.netId);
                        
                        // notify this client of other players that joined previously
                        Send.PlayerCreate(p, player.netId);
                        
                        // notify this client of unit ownerships
                        if(p.Unit != null)
                            Send.UnitOwnership(p.Unit, p, player.netId);
                    }

                    // send chat message
                    NetworkManager.SendMessage(NetworkManager.loc.SERVER,$"[color=#edc774]{_username} has connected.[/color]");
                    
                    // create unit for this player with id of 0 (auto-assign new id)
                    var unit = NetworkManager.CreateUnit(NetworkManager.loc.SERVER, 0, UnitTypes.crimson);
                    
                    // this player will take ownership of this unit
                    NetworkManager.UnitOwnership(NetworkManager.loc.SERVER, unit, player);
                }

                public static void UnitMovement(short _fromClient, Packet _packet)
                {
                    var unitNetId = _packet.ReadInt();
                    
                    /* Check if player can control this unit */
                    if (!NetworkManager.UnitsGroup.ContainsKey(unitNetId) ||
                        NetworkManager.PlayersGroup[_fromClient].Unit == null ||
                        NetworkManager.PlayersGroup[_fromClient].Unit.netId != unitNetId)
                    {
                        GD.PrintErr($"[player {_fromClient}] unit check failed for [unit {unitNetId}]");
                        return;
                    }


                    var pos = _packet.ReadVector2();
                    var axis = _packet.ReadVector2();
                    var velocity = _packet.ReadVector2();
                    var rotation = _packet.ReadFloat();
                    
                    var _plr = NetworkManager.PlayersGroup[_fromClient];
                    var unitBody = _plr.Unit.Body;


                    /* Do not update server-side if running a localserver (client = server shenanigans) */
                    if (_plr.netId != Client.myId)
                    {
                        unitBody.Axis = axis;
                        unitBody.Vel = velocity;
                        unitBody.Position = pos;
                        unitBody.DesiredPosition = pos;
                        unitBody.RotateBy = rotation;
                    }


                    Send.UnitMovement(_plr.Unit, _plr);
                }

                public static void ChatMessage(short _fromClient, Packet _packet)
                {
                    var message = _packet.ReadString();

                    var (cmdResp, cmd) = Server.clientCommands.handle(NetworkManager.PlayersGroup[_fromClient], message);
                    if(cmdResp == HandleResponse.NonExistent || cmdResp == HandleResponse.BadPrefix)
                        Send.ChatMessage(_fromClient, message);
                    else if(cmdResp == HandleResponse.BadArguments)
                        Send.ChatMessage(_fromClient, 0, $"[color=#e64b40]Invalid arguments supplied. Required arguments:[/color] {cmd.textparam}");
                }
            }

            
            // sends packets from the server to the client/s
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
                
                public static void PlayerCreate(Player player, short _toClient)
                {
                    using (var _packet = new Packet((int) ServerPackets.PlayerCreate))
                    {
                        _packet.Write(player.netId);
                        _packet.Write(player.Username);
                        
                        Server.SendTCPData(_toClient, _packet);
                    }
                }
                
                public static void PlayerDisconnect(Player _plr)
                {
                    using (var _packet = new Packet((int) ServerPackets.PlayerDisconnect))
                    {
                        _packet.Write(_plr);
                        
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
                        _packet.Write(unitBody.Vel);
                        _packet.Write(unitBody.Transform.Rotation);

                        if (controller != null)
                            Server.SendUDPDataToAll(controller.netId, _packet);
                        else
                        {
                            Server.SendUDPDataToAll(_packet);
                        }
                    }
                }
                
                public static void UnitOwnership(Unit unit, Player owner, short to=0)
                {
                    using (var _packet = new Packet((int) ServerPackets.UnitOwnership))
                    {
                        _packet.Write(unit);
                        _packet.Write(owner);
                        
                        if(to == 0)
                            Server.SendTCPDataToAll(_packet);
                        else
                            Server.SendTCPData(to, _packet);
                    }
                }
                
                public static void UnitRemove(Unit unit)
                {
                    using (var _packet = new Packet((int) ServerPackets.UnitRemove))
                    {
                        _packet.Write(unit);

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
                
                public static void ChatMessage(short _to, short _id, string message)
                {
                    using (var _packet = new Packet((int) ServerPackets.ChatMessage))
                    {
                        _packet.Write(_id);
                        _packet.Write(message);

                        Server.SendTCPData(_to, _packet);
                    }
                }
            }
        }
    }
}