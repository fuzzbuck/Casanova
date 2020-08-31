using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Casanova.core.net.client;
using Godot;

namespace Casanova.core.net.server
{
    class Server
    {
        public static int MaxClients { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, Client> Clients = new Dictionary<int, Client>();
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;
        private static UdpClient udpListener;

        
        // is server currently running and doing stuff
        public static bool IsRunning = false;
        
        // is currently hosting a map/world and playing on it
        public static bool IsHosting = false;
        public static bool IsDedicated = false;
        public static void Start(int _maxClients, int _port, bool _IsDedicated = false)
        {
            MaxClients = _maxClients;
            Port = _port;
            IsDedicated = _IsDedicated;
            
            GD.Print("Starting server...");
            InitializeServerData();
            
            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            
            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);
            
            GD.Print($"Server started on {tcpListener.LocalEndpoint}:{Port}.");
            IsRunning = true;
            IsHosting = true;
        }

        public static void Stop()
        {
            GD.Print($"Server stopped on port {Port}.");
            IsRunning = true;
            IsHosting = true;
        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            GD.Print($"{_client.Client.RemoteEndPoint} is attempting to connect.");

            for (int i = 1; i < MaxClients; i++)
            {
                if (Clients[i].tcp.socket == null) // no client is assigned to this id
                {
                    Clients[i].tcp.Connect(_client);
                    GD.Print($"{_client.Client.RemoteEndPoint} connected as id {i}.");
                    return;
                }
            }
            // no free ids, server reached MaxClients
            GD.Print($"{_client.Client.RemoteEndPoint} failed to connect! Max Clients reached.");
        }
        
        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (_data.Length < 4)
                {
                    return;
                }

                using (Packet _packet = new Packet(_data))
                {
                    int _clientId = _packet.ReadInt();

                    if (_clientId == 0)
                    {
                        return;
                    }

                    if (Clients[_clientId].udp.endPoint == null)
                    {
                        Clients[_clientId].udp.Connect(_clientEndPoint);
                        return;
                    }

                    if (Clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                    {
                        Clients[_clientId].udp.HandleData(_packet);
                    }
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
                {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
            }
        }
        private static void InitializeServerData()
        {
            for (int i = 1; i < MaxClients+1; i++)
            {
                Clients.Add(i, new Client(i));
            }
            
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, Packets.ServerHandle.Receive.WelcomeConfirmation },
                { (int)ClientPackets.playerMovement, Packets.ServerHandle.Receive.PlayerMovement }
            };
        }
    }
}