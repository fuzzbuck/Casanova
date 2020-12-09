using System.Collections.Generic;
using Casanova.core.content;
using Casanova.core.main.units;
using Casanova.core.net;
using Casanova.core.net.server;
using Casanova.core.net.types;
using Casanova.core.types;
using Casanova.ui;
using Godot;
using Camera = Casanova.core.main.units.Camera;
using Client = Casanova.core.net.client.Client;

namespace Casanova.core.main.world
{
    public class NetworkManager
    {
        public enum loc
        {
            SERVER,
            CLIENT
        }

        public static Dictionary<int, Player> PlayersGroup = new Dictionary<int, Player>();
        public static Dictionary<int, Unit> UnitsGroup = new Dictionary<int, Unit>();

        public static Player HostPlayer;

        public static Unit CreateUnitInstance(short netId)
        {
            var scene = (PackedScene) ResourceLoader.Load(Vars.path_main + $"/units/Unit.tscn");
            var instance = (Unit) scene.Instance();

            instance.netId = netId;
            return instance;
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

        public static void DestroyUnit(int _id)
        {
            if (UnitsGroup.ContainsKey(_id))
            {
                UnitsGroup[_id].QueueFree();
                UnitsGroup.Remove(_id);
            }
        }

        public static Unit CreateUnit(short _id, UnitType type, Vector2 position = new Vector2())
        {
            if (UnitsGroup.ContainsKey(_id))
                DestroyUnit(_id);

            var instance = CreateUnitInstance(_id);
            instance.Type = type;
            instance.GlobalPosition = position;

            UnitsGroup[_id] = instance;

            World.instance.SpawnUnit(instance);
            return instance;
        }
        
        public static Player CreatePlayer(loc loc, int _id, string _username)
        {
            GD.Print($"Creating player with username: {_username}");

            var willBeLocal = false;
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

            var player = new Player(_id, _username, null, willBeLocal);
            PlayersGroup[_id] = player;

            if (HostPlayer == null && loc == loc.SERVER)
                HostPlayer = player;
            
            /*
            ThreadManager.ExecuteOnMainThread(() =>
            {
                if (_id == Client.myId)
                {
                    var cam = (Camera) ResourceLoader.Load<PackedScene>(Vars.path_main + "/units/Camera.tscn")
                        .Instance();

                    cam.GlobalPosition = instance.GlobalPosition;
                    
                    if(!Vars.PersistentData.isMobile)
                        instance.Body.AddChild(cam);
                    else
                    {
                        instance.AddChild(cam);
                    }

                    PlayerController.LocalPlayer = player;
                    PlayerController.LocalUnit = instance;
                }
            });
            */

            return player;
        }
    }
}