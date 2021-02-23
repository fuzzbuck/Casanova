using Casanova.core.content;
using Casanova.core.main;
using Casanova.core.main.units;
using Casanova.core.main.world;
using Casanova.core.net.types;
using Godot;
using static Casanova.core.Vars;

namespace Casanova.core.net.control
{
    public class ServerControl : Node
    {

        public static void LoadWorldData(Player player)
        {
            // LOAD WORLD DATA FOR NEW PLAYERS
            
            // tell this player to create himself if not Host
            if(NetworkManager.HostPlayer != player)
                Packets.ServerHandle.Send.PlayerCreate(player, player.netId);

            // notify this client of already existing units
            foreach (Unit u in NetworkManager.UnitsGroup.Values)
            {
                Packets.ServerHandle.Send.UnitCreate(u.netId, u.Type, u.Body.GlobalPosition, u.Body.GlobalRotation);
            }
                    
            // notify other clients that this player has connected & about existance of other players
            foreach(Player p in NetworkManager.PlayersGroup.Values)
            {
                
                // if player in iteration is not us
                if (p.netId != player.netId)
                {
                    // notify other clients (except Host) that this player joined
                    if(p != NetworkManager.HostPlayer)
                        Packets.ServerHandle.Send.PlayerCreate(player, p.netId);

                    // notify this client of other players that joined previously
                    Packets.ServerHandle.Send.PlayerCreate(p, player.netId);
                    
                    // notify us of unit ownerships
                    if(p.Unit != null)
                        Packets.ServerHandle.Send.UnitOwnership(p.Unit, p, player.netId);
                }
                
            }

            // create unit for this player with id of 0 (id=0 means auto-assign new id)
            var unit = NetworkManager.CreateUnit(NetworkManager.loc.SERVER, 0, UnitTypes.crimson);
                    
            // this player will take ownership of this unit
            NetworkManager.UnitOwnership(NetworkManager.loc.SERVER, unit, player);
            
            // loaded, raise join event
            Events.RaisePlayerJoin(NetworkManager.loc.SERVER, player);
        }
        
        public static void InitEvents()
        {
            Events.PlayerJoin += (loc, player) =>
            {
                if (loc != NetworkManager.loc.SERVER)
                    return;
                
                if(log_server)
                    GD.Print($"{serv_string} {player} joined the server.");
                NetworkManager.SendMessage(NetworkManager.loc.SERVER,$"[color=#edc774]{player.Username} has connected.[/color]");
            };
            Events.PlayerConnect += (loc, id, username, ip) =>
            {
                if (loc != NetworkManager.loc.SERVER)
                    return;
                
                if(log_server)
                    GD.Print($"{serv_string} [connection {ip}:{id}:{username}] is joining the server.");
                
                var player = NetworkManager.CreatePlayer(NetworkManager.loc.SERVER, id, username);
                LoadWorldData(player);
            };
            Events.PlayerDisconnect += (loc, player) =>
            {
                if (loc != NetworkManager.loc.SERVER)
                    return;

                if (log_server)
                    GD.Print($"{serv_string} {player} has disconnected.");

                NetworkManager.DestroyPlayer(loc, player);
            };
            
            // todo: add more
        }

        public override void _Ready()
        {
            InitEvents();
        }
    }
}