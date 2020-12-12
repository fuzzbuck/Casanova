using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Casanova.core.utils;
using Godot;

namespace Casanova.core.net.server
{
    internal class Server
    {
        public delegate void PacketHandler(short _fromClient, Packet _packet);

        public static Dictionary<short, Client> Clients = new Dictionary<short, Client>();
        public static Dictionary<int, PacketHandler> handlers;
        
        public static CommandHandler clientCommands = new CommandHandler("/");

        private static TcpListener tcpListener;
        private static UdpClient udpListener;


        public static bool IsHosting;
        public static bool IsDedicated = false;
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

            GD.Print($"Server started on {tcpListener.LocalEndpoint}:");
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

                GD.Print($"{_client.Client.RemoteEndPoint} is attempting to connect.");

                for (short i = 1; i < MaxClients; i++)
                    if (Clients[i].tcp.socket == null) // no client is assigned to this id
                    {
                        Clients[i].tcp.Connect(_client);
                        GD.Print($"{_client.Client.RemoteEndPoint} connected as id {i}.");
                        return;
                    }

                // no free ids, server reached MaxClients
                GD.Print($"{_client.Client.RemoteEndPoint} failed to connect! Max Clients reached.");
            }
            catch (Exception e)
            {
                GD.Print($"Failed to accept connection: {e}");
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
                Console.WriteLine($"Error receiving UDP data: {_ex}");
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
                Console.WriteLine($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
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

        public static void SendTCPDataToAll(short _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (short i = 1; i <= MaxClients; i++)
                if (i != _exceptClient)
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

        public static void SendUDPDataToAll(short _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (short i = 1; i <= MaxClients; i++)
                if (i != _exceptClient)
                    Clients[i].udp.SendData(_packet);
        }

        private static void InitializeServerData()
        {
            for (short i = 1; i < MaxClients + 1; i++) Clients.Add(i, new Client(i));

            handlers = new Dictionary<int, PacketHandler>
            {
                {(int) Packets.ClientPackets.WelcomeReceived, Packets.ServerHandle.Receive.WelcomeConfirmation},
                {(int) Packets.ClientPackets.UnitMovement, Packets.ServerHandle.Receive.UnitMovement},
                {(int) Packets.ClientPackets.ChatMessage, Packets.ServerHandle.Receive.ChatMessage}
            };
            
            clientCommands.register(new Command("spawn", "Spawns a specified amount of specified units at the origin player units position",
                (player, args) =>
                {
                    Packets.ServerHandle.Send.ChatMessage(player.netId, 0, "hi thank u for executing the spawn command :) unfortunately it doesnt work yet sorry");
                }));
        }
    }
}