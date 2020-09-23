using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Messaging;
using Casanova.core.main.units;
using Casanova.core.net;
using Casanova.core.net.server;
using Casanova.core.net.types;
using Casanova.ui;
using Godot;
using Camera = Casanova.core.main.units.Camera;
using Client = Casanova.core.net.client.Client;

namespace Casanova.core.main.world
{
    public class NetworkManager
    {
        public static Dictionary<int, Player> PlayersGroup = new Dictionary<int, Player>();
        public static Player HostPlayer;

        public static PlayerUnit CreatePlayerInstance()
        {
            var scene = (PackedScene) ResourceLoader.Load(Vars.path_main + "/units/PlayerUnit.tscn");
            return (PlayerUnit) scene.Instance();
        }

        public static void ConfirmConnect()
        {
            Vars.CurrentState = Vars.State.World;
            Interface.tree.ChangeScene(Vars.path_world + "/World.tscn");
        }

        public static void DisconnectToMenu()
        {
            Client.Disconnect();
            ThreadManager.ExecuteOnMainThread(() =>
            {
                if (Server.IsHosting)
                {
                    Server.Stop();
                }
            });

            Vars.Reload();
        }

        public enum loc
        {
            SERVER, CLIENT
        }

        // SERVER & CLIENT
        public static void DestroyPlayer(int _id)
        {
            if (PlayersGroup.ContainsKey(_id))
            {
                PlayersGroup[_id].PlayerUnit?.QueueFree();
                PlayersGroup.Remove(_id);
            }
        }

        // TODO: add unit type parameter
        public static Player CreatePlayer(loc loc, int _id, string _username)
        {
            /*
             TODO:
             1. check if player is not spawned
             2. spawn player if doesnt exist
             3. tell the client which player instance to control
             */
            GD.Print($"Spawning unit for player {_id}");
            
            PlayerUnit instance = World.instance.SpawnPlayer(CreatePlayerInstance());
            instance.Tag = _username;

            bool willBeLocal = false;
            if (loc == loc.CLIENT && Server.IsHosting)
            {
                willBeLocal = true;
            }
            else
            {
                if (loc == loc.SERVER && !Server.IsDedicated && HostPlayer == null)
                {
                    willBeLocal = true;
                }
            }
            
            Player player = new Player(_id, _username, instance, willBeLocal);
            player.PlayerUnit = instance;
            PlayersGroup[_id] = player;
            
            if (HostPlayer == null && loc == loc.SERVER)
                HostPlayer = player;

            ThreadManager.ExecuteOnMainThread(() =>
            {
                try
                {
                    if (_id == Client.myId)
                    {
                        // make camera
                        Camera cam = (Camera) ResourceLoader.Load<PackedScene>(Vars.path_main + "/units/Camera.tscn").Instance();
                        
                        instance.Body.AddChild(cam);
                        cam.GlobalPosition = instance.Position;

                        PlayerController.LocalPlayer = player;
                        PlayerController.LocalPlayerUnit = instance;
                    }
                }
                catch (Exception e)
                {
                    GD.PrintErr(e);
                    // this is not my unit
                }

            });
            

            return player;
        }
    }
}