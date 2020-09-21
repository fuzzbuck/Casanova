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
        public static Dictionary<int, Player> playersGroup = new Dictionary<int, Player>();
        public static Player hostPlayer;

        public static Unit CreatePlayerInstance()
        {
            var scene = (PackedScene) ResourceLoader.Load(Vars.path_main + "/units/Unit.tscn");
            return (Unit) scene.Instance();
        }

        public static void ConfirmConnect()
        {
            Vars.CurrentState = Vars.State.World;
            Interface.tree.ChangeScene(Vars.path_world + "/World.tscn");
        }
        
        public enum loc
        {
            SERVER, CLIENT
        }

        // SERVER & CLIENT
        public static void DestroyPlayer(int _id)
        {
            if (playersGroup.ContainsKey(_id))
            {
                playersGroup[_id].unit?.QueueFree();
                playersGroup.Remove(_id);
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

            Unit _instance = CreatePlayerInstance();

            Unit unit = World.instance.SpawnPlayer(_instance);
            unit.Tag = _username;

            bool willBeLocal = false;
            if (loc == loc.CLIENT && Server.IsHosting)
            {
                willBeLocal = true;
            }
            else
            {
                if (loc == loc.SERVER && !Server.IsDedicated && hostPlayer == null)
                {
                    willBeLocal = true;
                }
            }
            
            Player player = new Player(_id, _username, _instance, willBeLocal);
            player.unit = _instance;
            playersGroup[_id] = player;
            
            if (hostPlayer == null && loc == loc.SERVER)
                hostPlayer = player;

            ThreadManager.ExecuteOnMainThread(() =>
            {
                try
                {
                    if (_id == Client.myId)
                    {
                        // make camera
                        Camera cam = (Camera) ResourceLoader.Load<PackedScene>(Vars.path_main + "/units/Camera.tscn").Instance();

                        if (Vars.PersistentData.isMobile)
                        {
                            _instance.AddChild(cam);
                            cam.GlobalPosition = _instance.kinematicBody.Position;
                        }
                        else
                        {
                            _instance.kinematicBody.AddChild(cam);
                        }
                        
                        PlayerController.localPlayer = player;
                        PlayerController.localUnit = _instance;
                    }
                }
                catch (Exception e)
                {
                    // this is not my unit
                }

            });
            

            return player;
        }
    }
}