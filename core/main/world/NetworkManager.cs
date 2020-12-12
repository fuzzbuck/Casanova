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
using Thread = System.Threading.Thread;

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

        public static Player HostPlayer = new Player(0, "server", true);

        public static Unit CreateUnitInstance()
        {
            var scene = (PackedScene) ResourceLoader.Load(Vars.path_main + $"/units/Unit.tscn");
            var instance = (Unit) scene.Instance();
            instance.ToSignal(instance, "ready").GetResult();

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
        public static Unit CreateUnit(loc loc, int id=0, UnitType type=null, Vector2 position = new Vector2(), float rotation=0)
        {
            // no available unit id, do not create
            if (availUnitIds.Count == 0)
                throw new Exception($"No ID to assign to unit type {type.Name}");

            if (id == 0)
            {
                id = availUnitIds.First();
                availUnitIds.Remove(id);
            }

            var instance = CreateUnitInstance();
            instance.netId = id;
            instance.Type = type;
            instance.GlobalPosition = position;

            UnitsGroup[id] = instance;

            World.instance.AddUnit(instance, rotation);
            if(loc == loc.SERVER)
                Packets.ServerHandle.Send.UnitCreate(id, type, position, rotation);

            return instance;
        }

        public static void UnitOwnership(loc loc, Unit unit, Player newOwner)
        {
            if (loc == loc.SERVER)
            {
                newOwner.Unit = unit;
                Packets.ServerHandle.Send.UnitOwnership(unit, newOwner);
            }
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
            GD.Print($"{loc.ToString()}: Creating player with username: {_username}");

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

            var player = new Player(_id, _username, willBeLocal);
            PlayersGroup[_id] = player;

            if (HostPlayer.netId == 0 && loc == loc.SERVER)
                HostPlayer = player;

            return player;
        }
    }
}