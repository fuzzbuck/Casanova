using Casanova.core.net.client;
using Casanova.core.net.server;
using Casanova.core.main.units;
using Godot;

namespace Casanova.core.main.world
{
    public class World : Node2D
    {
        public static World instance;
        public override void _Ready()
        {
            // singleton
            if (instance == null)
                instance = this;
            
            // todo: change this
            // start server & client
            GetNode<ServerHandler>("Server").Start();
            GetNode<ClientHandler>("Client").ConnectToServer();
            
        }
        public Unit SpawnPlayer(Unit unit)
        {
            GetNode<Node2D>("Units").AddChild(unit);
            return unit;
        }
    }
}
