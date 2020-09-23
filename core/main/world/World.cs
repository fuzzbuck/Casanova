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
        public static SceneTree tree;

        public override void _Ready()
        {
            instance = this;
            tree = GetTree();
        }

        /*
        public void StartServer()
        {
            GetNode<ServerHandler>("Server").Start();
        }

        public void StartClient()
        {
            GetNode<ClientHandler>("Client").ConnectToServer(Vars.PersistentData.ip);
        }
        
        */
        
        public PlayerUnit SpawnPlayer(PlayerUnit playerUnit)
        {
            GetNode<Node2D>("Units").AddChild(playerUnit);
            return playerUnit;
        }
    }
}
