using System;
using System.Collections.Generic;
using System.Linq;
using Casanova.core.content;
using Casanova.core.main.units;
using Casanova.core.net;
using Casanova.core.net.server;
using Casanova.core.net.types;
using Casanova.core.types;
using Casanova.core.utils;
using Casanova.ui;
using Godot;
using static Casanova.core.Vars;
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
        
        public static List<int> availUnitIds = Enumerable.Range(1, 1000).ToList();

        public static Player HostPlayer = new Player(0, "server", true);


        public static bool IsLocal()
        {
            return Client.IsConnected && Server.IsHosting && !Networking.IsHeadless;
        }
        public static Unit CreateUnitInstance()
        {
            var instance = (Unit) References.base_unit.Instance();
            instance.ToSignal(instance, "ready").GetResult();

            return instance;
        }

        public static void ConfirmConnect()
        {
            CurrentState = State.World;
            Interface.tree.ChangeScene(path_world + "/World.tscn");
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

            Reload();
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
        
        // called from server only
        public static void DestroyUnit(loc loc, Unit unit) // destroys & removes the unit
        {
            if(loc == loc.SERVER)
                Packets.ServerHandle.Send.UnitRemove(unit);
            
            /* Nullify controller player's reference for this unit */
            if (unit.Controller != null)
                unit.Controller.Unit = null;
            
            /* Server host shenanigans */
            if(IsLocal() && unit.netId == Client.myId)
                PlayerController.VoidOwnership();
            
            ThreadManager.ExecuteOnMainThread(() => { RemoveUnit(unit.netId); });
        }

        public static void DiminishUnitOwnership(Unit unit, Player owner)
        {
            unit.Controller = null;
        }
        
        public static void UnitOwnership(loc loc, Unit unit, Player newOwner)
        {
            if(newOwner.Unit != null)
                DiminishUnitOwnership(unit, newOwner);
            
            newOwner.Unit = unit;
            newOwner.Unit.Controller = newOwner;

            if (loc == loc.SERVER)
            {
                Packets.ServerHandle.Send.UnitOwnership(unit, newOwner);
            }
            else
            {
                if (newOwner.netId == Client.myId)
                    PlayerController.TakeOwnership(unit);
            }
        }

        public static void RemovePlayer(short _id)
        {
            if (PlayersGroup.ContainsKey(_id))
            {
                PlayersGroup.Remove(_id);
            }
        }

        public static void DestroyPlayer(loc loc, Player player)
        {
            if(player.Unit != null)
                DestroyUnit(loc, player.Unit);
            
            if(loc == loc.SERVER)
                SendMessage(loc.SERVER,$"[color=#edc774]{player.Username} has disconnected.[/color]");
            
            RemovePlayer(player.netId);
        }
        
        public static Player CreatePlayer(loc loc, short _id, string _username)
        {
            var isHost = false;
            if (loc == loc.CLIENT && Server.IsHosting)
            {
                isHost = true;
            }
            else
            {
                if (loc == loc.SERVER && !Networking.IsHeadless && HostPlayer.netId == 0)
                {
                    isHost = true;
                }
            }

            var player = new Player(_id, _username, isHost);
            PlayersGroup[_id] = player;

            if (HostPlayer.netId == 0 && loc == loc.SERVER)
                HostPlayer = player;
            
            GD.Print($"HostPlayer: {HostPlayer.Username}:{HostPlayer.IsHost} -> willBeLocal -> {isHost} -> loc -> {loc.ToString()}");
            
            return player;
        }

        public static void SendMessage(loc loc, string message, Player player = null)
        {
            if (loc == loc.SERVER)
            {
                if(player != null)
                    Packets.ServerHandle.Send.ChatMessage(player.netId, 0, message);
                else
                    Packets.ServerHandle.Send.ChatMessage(0, message);
            }
        }
    }
}