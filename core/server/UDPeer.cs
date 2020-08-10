using Godot;

namespace Casanova.core.server
{
    public class UDPeer
    {
        public static PacketPeerUDP socket;
        public void init()
        {
            socket = new PacketPeerUDP();
            if (socket.Listen(6969, "127.0.0.1") != Error.Ok)
            {
                GD.Print("ERROR! Can't listen on port 6969.");
            }
            else
            {
                GD.Print("Listening on port 6969");
                Vars.Networking.isConnected = true;
            }
            
            PollServer();
        }

        public void PollServer()
        {
            while (Vars.Networking.isConnected)
            {
                if (socket.GetAvailablePacketCount() > 0)
                {
                    var data = socket.GetPacket();
                }
            }
        }
    }
}