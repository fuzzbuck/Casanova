using Casanova.core.main.units.Player;
using Casanova.core.net.client;
using Casanova.core.net.server;
using Godot;

namespace Casanova.core.main.world
{
    public class World : Node2D
    {
        public static World instance;
        public override void _Ready()
        {
            // awake NetworkManager
            new NetworkManager().Awake();
            
            // singleton
            instance = this;
        }

        public void SpawnPlayer(PlayerUnit unit)
        {
            GetNode<Node2D>("Units").AddChild(unit);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventKey eventKey)
                if (eventKey.Pressed)
                {
                    if (eventKey.Scancode == (int) KeyList.F10)
                    {
                        ServerHandler server = GetNode<ServerHandler>("Server");
                        server.Start();
                    }
                    if (eventKey.Scancode == (int) KeyList.F9)
                    {
                        ClientHandler client = GetNode<ClientHandler>("Client");
                        client.ConnectToServer();
                    }
                }
        }
    }
}
