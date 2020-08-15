using Godot;

namespace Casanova.core.net.server
{
    public class ServerHandler : Node
    {
        public void Start()
        {
            Server.Start(32, 6969);
        }
    }
}
