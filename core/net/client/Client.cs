using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Casanova.core.net.server;
using Godot;

namespace Casanova.core.net.client
{
    public class Client
    {
        public static int dataBufferSize = 4096;

        public static string ip = "127.0.0.1";
        public static int port = 6969;
        public static int myId = 0;
        public static TCP tcp;
        public static UDP udp;
        
        private static bool isConnected = false;
        private delegate void PacketHandler(Packet _packet);
        private static Dictionary<int, PacketHandler> packetHandlers;

        public static void ConnectToServer(string _ip, int _port)
        {
            GD.Print($"Attempting connection to {_ip}:{_port}");
            try
            {
                ip = _ip;
                port = _port;

                tcp = new TCP();
                udp = new UDP();

                InitializeClientData();

                isConnected = true;
                tcp.Connect();
            }
            catch (Exception e)
            {
                DisconnectAndDispose();
                throw new Exception("An error occured while connecting to the server: " + e.Message);
            }
        }

        public static void DisconnectAndDispose()
        {
            try
            {
                tcp.Disconnect();
                udp.Disconnect();
            }
            catch (Exception)
            {
                // ignored
            }

            isConnected = false;
            tcp = null;
            udp = null;
        }

        public class TCP
        {
            public TcpClient socket;

            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public void Connect()
            {
                socket = new TcpClient
                {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize
                };

                receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(ip, port, ConnectCallback, socket);
            }

            private void ConnectCallback(IAsyncResult _result)
            {
                try
                {
                    socket.EndConnect(_result);
                }
                catch (Exception e)
                {
                    DisconnectAndDispose();
                }

                if (!socket.Connected)
                {
                    return;
                }

                GD.Print("Connection to server established");

                stream = socket.GetStream();

                receivedData = new Packet();

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception _ex)
                {
                    GD.Print($"Error sending data to server via TCP: {_ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0)
                    {
                        Disconnect();
                        return;
                    }

                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);
                    
                    receivedData.Reset(HandleData(_data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    GD.Print($"Error receiving TCP data; {e}");
                    Disconnect();
                }
            }
            
            private bool HandleData(byte[] _data)
            {
                int _packetLength = 0;

                receivedData.SetBytes(_data);

                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
                {
                    byte[] _packetBytes = receivedData.ReadBytes(_packetLength);

                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetId = _packet.ReadInt();
                            packetHandlers[_packetId](_packet); // Call appropriate method to handle the packet
                        }

                        // TODO: end of thread manager
                    });

                    _packetLength = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        _packetLength = receivedData.ReadInt();
                        if (_packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (_packetLength <= 1)
                {
                    return true;
                }

                // malformed/not-understood packet, drop it
                return false;
            }

            public void Disconnect()
            {
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        public class UDP
        {
            public UdpClient socket;
            public IPEndPoint endPoint;

            public UDP()
            {
                endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            }

            public void Connect(int _localPort)
            {
                socket = new UdpClient(_localPort);

                socket.Connect(endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                using (Packet _packet = new Packet())
                {
                    SendData(_packet);
                }
            }
            
            public void SendData(Packet _packet)
            {
                try
                {
                    _packet.InsertInt(myId);
                    if (socket != null)
                    {
                        socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                    }
                }
                catch (Exception _ex)
                {
                    GD.Print($"Error sending data to server via UDP: {_ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    byte[] _data = socket.EndReceive(_result, ref endPoint);
                    socket.BeginReceive(ReceiveCallback, null);

                    if (_data.Length < 4)
                    {
                        Disconnect();
                        return;
                    }

                    HandleData(_data);
                }
                catch
                {
                    Disconnect();
                }
            }

            private void HandleData(byte[] _data)
            {
                using (Packet _packet = new Packet(_data))
                {
                    int _packetLength = _packet.ReadInt();
                    _data = _packet.ReadBytes(_packetLength);
                }

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_data))
                    {
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
                });
            }
            
            public void Disconnect()
            {
                endPoint = null;
                socket = null;
            }
        }
        
        public static void SendTCPData(Packet _packet)
        {
            _packet.WriteLength();
            tcp.SendData(_packet);
        }

        public static void SendUDPData(Packet _packet)
        {
            _packet.WriteLength();
            udp.SendData(_packet);
        }

        private static void InitializeClientData()
        {
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ServerPackets.welcome, Packets.ClientHandle.Receive.Welcome },
                { (int)ServerPackets.spawnPlayer, Packets.ClientHandle.Receive.SpawnPlayer },
                { (int)ServerPackets.playerMovement, Packets.ClientHandle.Receive.PlayerMovement },
                { (int)ServerPackets.disconnectPlayer, Packets.ClientHandle.Receive.PlayerDisconnect },
                { (int)ServerPackets.chatMessage, Packets.ClientHandle.Receive.ChatMessage }
            };
        }

        private void Disconnect()
        {
            if (isConnected)
            {
                isConnected = false;
                tcp.socket.Close();
                udp.socket.Close();
                
                GD.Print("Disconnected from server.");
            }
        }
    }
}