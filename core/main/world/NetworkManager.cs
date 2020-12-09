using System;
using System.Collections.Generic;
using System.Linq;
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

        public static Dictionary<short, Player> PlayersGroup = new Dictionary<short, Player>();
        public static Dictionary<int, Unit> UnitsGroup = new Dictionary<int, Unit>();
        
        public static List<int> availUnitIds = Enumerable.Range(1, 50000).ToList();

        public static Player HostPlayer;

        public static Unit CreateUnitInstance()
        {
            var scene = (PackedScene) ResourceLoader.Load(Vars.path_main + $"/units/Unit.tscn");
            var instance = (Unit) scene.Instance();
            
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

        public static void RemoveUnit(int _id)
        {
            if (UnitsGroup.ContainsKey(_id))
            {
                UnitsGroup[_id].QueueFree();
                UnitsGroup.Remove(_id);

                availUnitIds.Add(_id);
            }
        }

        
        // called from server or client
        public static Unit CreateUnit(loc loc, UnitType type, Vector2 position = new Vector2(), float rotation=0)
        {
            // no available unit id, do not create
            if (availUnitIds.Count == 0)
                throw new Exception($"No ID to assign to unit type {type.Name}");
            
            int id = availUnitIds.First();
            availUnitIds.Remove(id);
            
            var instance = CreateUnitInstance();
            
            instance.netId = id;
            instance.Type = type;
            instance.GlobalPosition = position;
            instance.Body.RotationDegrees = rotation;
            
            UnitsGroup[id] = instance;

            World.instance.SpawnUnit(instance);
            if(loc == loc.SERVER)
                Packets.ServerHandle.Send.UnitCreate(id, type, position, rotation);
            
            return instance;
        }

        public static void RemovePlayer(short _id)
        {
            if (PlayersGroup.ContainsKey(_id))
            {
                PlayersGroup.Remove(_id);
            }
        }
        
        public static Player CreatePlayer(loc loc, short _id, string _username)
        {
            GD.Print($"Creating player with username: {_username}");
            if(PlayersGroup.ContainsKey(_id))
                RemovePlayer(_id);

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