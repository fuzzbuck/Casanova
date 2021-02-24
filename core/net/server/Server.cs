using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Casanova.core.content;
using Casanova.core.main.units;
using Casanova.core.main.world;
using Casanova.core.net.types;
using Casanova.core.types;
using Casanova.core.utils;
using Godot;
using static Casanova.core.Vars;

namespace Casanova.core.net.server
{
    internal class Server
    {
        public delegate void PacketHandler(short _fromClient, Packet _packet);

        public static Dictionary<short, Client> Clients = new Dictionary<short, Client>();
        public static Dictionary<int, PacketHandler> handlers;
        
        public static CommandHandler clientCommands;

        private static TcpListener tcpListener;
        private static UdpClient udpListener;


        public static bool IsHosting;
        public static int MaxClients { get; private set; }
        public static int Port { get; private set; }

        public static void Start(int _maxClients, int _port)
        {
            MaxClients = _maxClients;
            Port = _port;

            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if(log_server)
                GD.Print($"{serv_string} started on {tcpListener.LocalEndpoint}");

            IsHosting = true;
        }

        public static void Stop()
        {
            GD.Print($"Server stopped on port {Port}.");

            tcpListener.Stop();
            udpListener.Close();

            Clients.Clear();
            IsHosting = false;
        }

        private static void TcpConnectCallback(IAsyncResult _result)
        {
            try
            {
                var _client = tcpListener.EndAcceptTcpClient(_result);
                tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);

                for (short i = 1; i < MaxClients; i++)
                    if (Clients[i].tcp.socket == null) // no client is assigned to this id
                    {
                        Clients[i].tcp.Connect(_client);
                        return;
                    }

                // no free ids, server reached MaxClients
                if(log_server)
                    GD.Print($"{serv_string} {_client.Client.RemoteEndPoint} failed to connect! Max Clients reached.");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to accept connection from: {e}");
            }
        }

        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                var _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (_data.Length < 4) return;

                using (var _packet = new Packet(_data))
                {
                    var _clientId = _packet.ReadShort();

                    if (_clientId == 0) return;

                    if (Clients[_clientId].udp.endPoint == null)
                    {
                        Clients[_clientId].udp.Connect(_clientEndPoint);
                        return;
                    }

                    if (Clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                        Clients[_clientId].udp.HandleData(_packet);
                }
            }
            catch (Exception _ex)
            {
                GD.PrintErr($"Error receiving UDP data: {_ex}");
            }
        }

        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                if (_clientEndPoint != null)
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
            catch (Exception _ex)
            {
                GD.PrintErr($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
            }
        }
        
        public static void SendTCPData(short _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Clients[_toClient].tcp.SendData(_packet);
        }

        public static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (short i = 1; i <= MaxClients; i++) Clients[i].tcp.SendData(_packet);
        }

        public static void SendTCPDataToAll(short[] _exceptClients, Packet _packet)
        {
            _packet.WriteLength();
            for (short i = 1; i <= MaxClients; i++)
                if (!_exceptClients.Contains(i))
                    Clients[i].tcp.SendData(_packet);
        }


        public static void SendUDPData(short _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Clients[_toClient].udp.SendData(_packet);
        }

        public static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (short i = 1; i <= MaxClients; i++) Clients[i].udp.SendData(_packet);
        }

        public static void SendUDPDataToAll(short[] _exceptClients, Packet _packet)
        {
            _packet.WriteLength();
            for (short i = 1; i <= MaxClients; i++)
                if (!_exceptClients.Contains(i))
                    Clients[i].udp.SendData(_packet);
        }

        private static void InitializeServerData()
        {
            for (short i = 1; i < MaxClients + 1; i++) Clients.Add(i, new Client(i));

            handlers = new Dictionary<int, PacketHandler>
            {
                {(int) Packets.ClientPackets.WelcomeReceived, Packets.ServerHandle.Receive.WelcomeConfirmation},
                {(int) Packets.ClientPackets.UnitMovement, Packets.ServerHandle.Receive.PlayerUnitMovement},
                {(int) Packets.ClientPackets.ChatMessage, Packets.ServerHandle.Receive.ChatMessage}
            };

            clientCommands = new CommandHandler("/");
            clientCommands.register(new Command("help", "", "Display all available commands", (player, args) =>
            {
                string final = String.Empty;
                foreach (var cmd in clientCommands.commands)
                {
                    final = final + $"[color={Funcs.ColorToHex(Pals.command)}]{cmd.Value.name}[/color] [color={Funcs.ColorToHex(Pals.unimportant)}]{cmd.Value.textparam}[/color] - {cmd.Value.desc}\n";
                }
                NetworkManager.SendMessage(NetworkManager.loc.SERVER, final.Substring(0, final.Length-1), player); // remove trailing \n
            }));
            
            clientCommands.register(new Command("spawn", "[amount] [typeid]", "Spawns an amount of units at the player's position",
                (player, args) =>
                {
                    int amt;
                    UnitType type;

                    try
                    {
                        amt = int.Parse((string) args[0]);
                        type = Enums.UnitTypes[int.Parse((string) args[1])];
                    }
                    catch (Exception e)
                    {
                        NetworkManager.SendMessage(NetworkManager.loc.SERVER, $"[color=#e64b40]Error parsing command arguments:[/color] {e.Message}", player);
                        return;
                    }

                    if (player.Unit?.Body != null)
                    {
                        var pos = player.Unit.Body.GlobalPosition;
                        var rnd = new Random();
                        pos.x += rnd.Next(-20000, 20000) / 300f;
                        pos.y += rnd.Next(-20000, 20000) / 300f;
                        
                        for (int i = 0; i < amt; i++)
                        {
                            NetworkManager.CreateUnit(NetworkManager.loc.SERVER, 0, type, pos, 0);
                        }
                        NetworkManager.SendMessage(NetworkManager.loc.SERVER, $"Spawned {amt} {type.Name}s", player);
                        return;
                    }
                    NetworkManager.SendMessage(NetworkManager.loc.SERVER, $"You need to be alive to use this command.", player);
                }));
            
            clientCommands.register(new Command("ownership", "[unitid]", "Take the ownership of the specified unit",
                (player, args) =>
                {
                    Unit unit;

                    try
                    {
                        unit = NetworkManager.UnitsGroup[int.Parse((string) args[0])];
                    }
                    catch (Exception e)
                    {
                        NetworkManager.SendMessage(NetworkManager.loc.SERVER, $"[color=#e64b40]Error parsing command arguments:[/color] {e.Message}", player);
                        return;
                    }
                    
                    Packets.ServerHandle.Send.UnitOwnership(unit, player);
                    NetworkManager.SendMessage(NetworkManager.loc.SERVER, $"Taken ownership of unit id {unit.netId}", player);
                }));
            
            clientCommands.register(new Command("sleep", "", "Sleep all units' collision systems (debug purpose)",
                (player, args) =>
                {
                    foreach (Unit unit in NetworkManager.UnitsGroup.Values)
                    {
                        unit.Body.Sleeping = true;
                    }
                    
                    NetworkManager.SendMessage(NetworkManager.loc.SERVER, $"Slept all units.", player);
                }));
            clientCommands.register(new Command("destroy", "[unitid]", "Destroy the specified unit", (player, args) =>
            {
                Unit unit;

                try
                {
                    unit = NetworkManager.UnitsGroup[int.Parse((string) args[0])];
                }
                catch (Exception e)
                {
                    NetworkManager.SendMessage(NetworkManager.loc.SERVER, $"[color=#e64b40]Error parsing command arguments:[/color] {e.Message}", player);
                    return;
                }
                
                NetworkManager.DestroyUnit(NetworkManager.loc.SERVER, unit);
                NetworkManager.SendMessage(NetworkManager.loc.SERVER, $"Removed unit id {unit.netId}", player);
            }));
            clientCommands.register(new Command("players", "", "List all players",
                (player, args) =>
                {
                    var msg = String.Empty;
                    foreach (Player plr in NetworkManager.PlayersGroup.Values)
                    {
                        msg = msg + "id:" + plr.netId + " -> " + plr.Username + (plr.IsHost ? $" [color={Funcs.ColorToHex(Pals.unimportant)}](local)[/color] " : " ") + "\n";
                    }

                    msg = msg.Substring(0, msg.Length - 1);
                    
                    NetworkManager.SendMessage(NetworkManager.loc.SERVER, msg, player);
                }));
        }
    }
}