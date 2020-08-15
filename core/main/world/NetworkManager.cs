using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Casanova.core.main.units.Player;
using Casanova.core.net;
using Casanova.core.net.server;
using Casanova.core.net.types;
using Godot;
using Client = Casanova.core.net.client.Client;

namespace Casanova.core.main.world
{
    public class NetworkManager
    {
        public static NetworkManager instance;
        public static Dictionary<int, Player> playersGroup = new Dictionary<int, Player>();

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }else if (instance != this)
            {
                GD.Print("fatal error! network manager instance already exists.");
            }
        }

        public static PlayerUnit CreatePlayerInstance()
        {
            var scene = (PackedScene) ResourceLoader.Load(Vars.path_main + "/units/Player/PlayerUnit.tscn");
            return (PlayerUnit) scene.Instance();
        }

        // TODO: add unit type parameter
        public void SpawnPlayer(int _id, string _username)
        {
            /*
             TODO:
             1. check if player is not spawned
             2. spawn player if doesnt exist
             3. tell the client which player instance to control
             */

            PlayerUnit _instance = CreatePlayerInstance();
            
            World.instance.SpawnPlayer(_instance);
            
            GD.Print($"[CLIENT]: Spawning unit for player {_id}");

            Player player = new Player(_id, _username, _instance);
            player.unit = _instance;
            playersGroup[_id] = player;

            // if this is our player

            if (_id == Client.instance.myId)
            {
                _instance.instance.GetNode<Camera2D>("Camera").Current = true;
                PlayerController.localPlayer = player;
                PlayerController.localUnit = (PlayerUnit) _instance;
            }
        }
    }
}