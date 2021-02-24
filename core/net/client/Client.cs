using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Casanova.core.utils;
using Casanova.ui;
using Godot;

namespace Casanova.core.net.client
{
    public class Client
    {
        public static int ConnectTimeout = 3000;
        public static int dataBufferSize = 4096;

        public static string Hostname = "127.0.0.1";
        public static int Port = 375;
        public static short MyId = 0;
        public static TCP Tcp;
        public static UDP Udp;

        public static bool IsConnected;

        
        // Connects to a server & runs the "post" Action with a bool representing whether the connection was successful
        public static void ConnectToServer(string _hostname, int _port, Action<bool> post)
        {
            if(Vars.log_log)
                GD.Print($"{Vars.client_string} attempting connection to {_hostname}:{_port}");
            try
            {
                Hostname = _hostname;
                Port = _port;

                Tcp = new TCP();
                Udp = new UDP();
                
                Tcp.Connect();
                post.Invoke(true);
            }
            catch (Exception e)
            {
                Disconnect();
                post.Invoke(false);
                throw new Exception(e.Message);
            }
        }

        public static void SendTCPData(Packet _packet)
        {
            if (!IsConnected)
                return;

            _packet.WriteLength();
            Tcp.SendData(_packet);
        }

        public static void SendUDPData(Packet _packet)
        {
            if (!IsConnected)
                return;

            _packet.WriteLength();
            Udp.SendData(_packet);
        }

        public static void Disconnect()
        {
            if (IsConnected)
            {
                IsConnected = false;
                Tcp.socket.Close();
                Udp.socket.Close();
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
                
                Task result = socket.ConnectAsync(Hostname, Port);
                Task.WaitAny(new[] {result}, ConnectTimeout);

                if (!socket.Connected)
                {
                    Disconnect();
                    throw new Exception("Timeout! the server might be down or overloaded.");
                }
                
                IsConnected = true;
                receivedData = new Packet();

                stream = socket.GetStream();
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }

            /*
            private void ConnectCallback(IAsyncResult _result)
            {
                try
                {
                    socket.EndConnect(_result);
                    if (!socket.Connected) return;
                    IsConnected = true;

                    receivedData = new Packet();

                    stream = socket.GetStream();
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Disconnect();
                }
            }
            */

            public void SendData(Packet _packet)
            {
                try
                {
                    if (socket != null) stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
                catch (Exception)
                {
                    // ignored (?) could need to disconnect so [todo]
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
                catch (Exception)
                {
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
                    if (!IsConnected)
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
                endPoint = Funcs.HostToIp(Hostname, Port);
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
                    _packet.InsertShort(MyId);
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
                    if (!IsConnected)
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