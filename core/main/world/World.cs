using System;
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
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                throw new Exception("Can't instance multiple worlds!");
            }
        }

        public void StartServer()
        {
            GetNode<ServerHandler>("Server").Start();
        }

        public void StartClient()
        {
            GetNode<ClientHandler>("Client").ConnectToServer(Vars.PersistentData.ip);
        }
        public Unit SpawnPlayer(Unit unit)
        {
            GetNode<Node2D>("Units").AddChild(unit);
            return unit;
        }
    }
}
