using System;
using System.Net;
using System.Net.Sockets;
using Casanova.ui;
using Godot;

namespace Casanova.core.net.client
{
    public class Client
    {
        public static int dataBufferSize = 4096;

        public static string ip = "127.0.0.1";
        public static int port = 6969;
        public static short myId = 0;
        public static TCP tcp;
        public static UDP udp;

        public static bool isConnected;

        
        // Connects to a server & runs the "post" Action with a bool representing whether the connection was successful
        public static void ConnectToServer(string _ip, int _port, Action<bool> post)
        {
            GD.Print($"Attempting connection to {_ip}:{_port}");
            try
            {
                ip = _ip;
                port = _port;

                tcp = new TCP();
                udp = new UDP();
                
                tcp.Connect();
                post.Invoke(true);
            }
            catch (Exception e)
            {
                Disconnect();
                post.Invoke(false);
                throw new Exception("An error occured while connecting to the server: " + e.Message);
            }
        }

        public static void SendTCPData(Packet _packet)
        {
            if (!isConnected)
                return;

            _packet.WriteLength();
            tcp.SendData(_packet);
        }

        public static void SendUDPData(Packet _packet)
        {
            if (!isConnected)
                return;

            _packet.WriteLength();
            udp.SendData(_packet);
        }

        public static void Disconnect()
        {
            if (isConnected)
            {
                isConnected = false;
                tcp.socket.Close();
                udp.socket.Close();
            }
        }

        public class TCP
        {
            private byte[] receiveBuffer;
            private Packet receivedData;
            public TcpClient socket;

            private NetworkStream stream;

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
                    if (!socket.Connected) return;
                    isConnected = true;

                    receivedData = new Packet();

                    stream = socket.GetStream();
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Interface.Utils.CreateInformalMessage(e.Message, 10);
                    Disconnect();
                }
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    if (socket != null) stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
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
                    var _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0)
                    {
                        Disconnect();
                        return;
                    }

                    var _data = new byte[_byteLength];
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
                var _packetLength = 0;

                receivedData.SetBytes(_data);

                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadShort();
                    if (_packetLength <= 0) return true;
                }

                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
                {
                    var _packetBytes = receivedData.ReadBytes(_packetLength);
                    if (!isConnected)
                        continue;

                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (var _packet = new Packet(_packetBytes))
                        {
                            var _packetId = _packet.ReadByte();
                            Packets.handlers[_packetId](_packet); // Call appropriate method to handle the packet
                        }

                        // TODO: end of thread manager
                    });

                    _packetLength = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        _packetLength = receivedData.ReadShort();
                        if (_packetLength <= 0) return true;
                    }
                }

                if (_packetLength <= 1) return true;

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
            public IPEndPoint endPoint;
            public UdpClient socket;

            public UDP()
            {
                endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            }

            public void Connect(int _localPort)
            {
                socket = new UdpClient(_localPort);

                socket.Connect(endPoint);
                socket.BeginReceive(ReceiveCallback, null);
                
                using (var _packet = new Packet((int) Packets.ServerPackets.ChatMessage))
                {
                    _packet.Write("RegisterUDP bootleg fix");
                    SendData(_packet);
                }
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    _packet.InsertShort(myId);
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
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
                    var _data = socket.EndReceive(_result, ref endPoint);
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
                using (var _packet = new Packet(_data))
                {
                    var _packetLength = _packet.ReadShort();
                    _data = _packet.ReadBytes(_packetLength);
                }

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    if (!isConnected)
                        return;

                    using (var _packet = new Packet(_data))
                    {
                        var _packetId = _packet.ReadByte();
                        Packets.handlers[_packetId](_packet);
                    }
                });
            }

            public void Disconnect()
            {
                endPoint = null;
                socket = null;
            }
        }
    }
}