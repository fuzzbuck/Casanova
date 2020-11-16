using System;
using System.Net;
using System.Net.Sockets;
using Casanova.core.main.world;
using Casanova.core.net.types;
using Casanova.core.types;
using Godot;

namespace Casanova.core.net.server
{
    internal class Client
    {
        public static int dataBufferSize = 4096;

        public int id;
        public Player player;
        public TCP tcp;
        public UDP udp;

        public Client(int _clientId)
        {
            id = _clientId;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public void SendIntoGame(string _username, UnitType type)
        {
            player = NetworkManager.CreatePlayer(NetworkManager.loc.SERVER, id, _username);

            // send info to all clients except ours that we spawned
            foreach (var _client in Server.Clients.Values)
                if (_client.player != null)
                    if (_client.id != id)
                        Packets.ServerHandle.Send.PlayerJoin(id, _client.player);

            // send info from all clients except ours about others existance
            foreach (var _client in Server.Clients.Values)
                if (_client.player != null && !_client.player.IsLocal)
                    Packets.ServerHandle.Send.PlayerJoin(_client.id, player);
        }

        private void Disconnect()
        {
            GD.Print($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

            if (Server.IsHosting)
            {
                Packets.ServerHandle.Send.PlayerDisconnect(id);
                player = null;
            }

            tcp.Disconnect();
            udp.Disconnect();
        }

        public class TCP
        {
            private readonly int id;
            private byte[] receiveBuffer;
            private Packet receivedData;
            public TcpClient socket;
            private NetworkStream stream;

            public TCP(int _id)
            {
                id = _id;
            }

            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                Packets.ServerHandle.Send.Welcome(id, "Welcome to the testing branch.");
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    if (socket != null)
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null,
                            null); // Send data to appropriate client
                }
                catch (Exception _ex)
                {
                    Console.WriteLine($"Error sending data to player {id} via TCP: {_ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    var _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0)
                    {
                        Server.Clients[id].Disconnect();
                        return;
                    }

                    var _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    receivedData.Reset(HandleData(_data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception _ex)
                {
                    Console.WriteLine($"Error receiving TCP data: {_ex}, disconnecting.");
                    Server.Clients[id].Disconnect();
                }
            }

            private bool HandleData(byte[] _data)
            {
                var _packetLength = 0;

                receivedData.SetBytes(_data);

                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0) return true;
                }

                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
                {
                    var _packetBytes = receivedData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (var _packet = new Packet(_packetBytes))
                        {
                            var _packetId = _packet.ReadInt();
                            Server.packetHandlers[_packetId](id, _packet);
                        }
                    });

                    _packetLength = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        _packetLength = receivedData.ReadInt();
                        if (_packetLength <= 0) return true;
                    }
                }

                if (_packetLength <= 1) return true;

                return false;
            }

            public void Disconnect()
            {
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        public class UDP
        {
            public IPEndPoint endPoint;

            private readonly int id;

            public UDP(int _id)
            {
                id = _id;
            }

            public void Connect(IPEndPoint _endPoint)
            {
                endPoint = _endPoint;
            }

            public void SendData(Packet _packet)
            {
                Server.SendUDPData(endPoint, _packet);
            }

            public void HandleData(Packet _packetData)
            {
                if (!Server.IsHosting)
                    return;

                var _packetLength = _packetData.ReadInt();
                var _packetBytes = _packetData.ReadBytes(_packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (var _packet = new Packet(_packetBytes))
                    {
                        var _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id, _packet);
                    }
                });
            }

            public void Disconnect()
            {
                endPoint = null;
            }
        }
    }
}