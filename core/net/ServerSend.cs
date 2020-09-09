using System.Linq;
using Casanova.core.main.world;
using Casanova.core.net.server;
using Godot;

namespace Casanova.core.net
{
    public class ServerSend
    {
        public static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            GD.Print($"Sending a packet to client {_toClient}");
            Server.Clients[_toClient].tcp.SendData(_packet);
        }

        public static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxClients; i++)
            {
                Server.Clients[i].tcp.SendData(_packet);
            }
        }
        public static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxClients; i++)
            {
                if (i != _exceptClient)
                {
                    Server.Clients[i].tcp.SendData(_packet);
                }
            }
        }


        public static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.Clients[_toClient].udp.SendData(_packet);
        }
        
        public static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxClients; i++)
            {
                Server.Clients[i].udp.SendData(_packet);
            }
        }
        public static void SendUDPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxClients; i++)
            {
                if (i != _exceptClient)
                {
                    Server.Clients[i].udp.SendData(_packet);
                }
            }
        }
    }
}