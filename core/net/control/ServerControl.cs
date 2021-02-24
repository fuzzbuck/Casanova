using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Casanova.core.content;
using Casanova.core.main;
using Casanova.core.main.units;
using Casanova.core.main.world;
using Casanova.core.net.server;
using Casanova.core.net.types;
using Casanova.core.utils;
using Godot;
using static Casanova.core.Vars;
using Thread = System.Threading.Thread;

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

        
        /* Server only commands */
        public static CommandHandler handler = new CommandHandler("");
        public static void InitCommands()
        {
            handler.register(new Command("status", "", "Display the status of the server", (_, args) =>
            {
                GD.Print($"Listening on port {Server.Port}\n - {Engine.IterationsPerSecond} fps\n - {NetworkManager.PlayersGroup.Count} players\n - {NetworkManager.UnitsGroup.Count} units\n - Host => {(Networking.IsHeadless ? "HEADLESS SERVER" : NetworkManager.HostPlayer.ToString())}");
            }));
            
            handler.register(new Command("players", "", "List all players", (_, args) =>
            {
                var msg = $"{NetworkManager.PlayersGroup.Count} players:\n";
                foreach (Player plr in NetworkManager.PlayersGroup.Values)
                {
                    msg = msg + " - " + plr + "\n";
                }
                msg = msg.Substring(0, msg.Length - 1);
                    
                GD.Print(msg);
            }));
            
            handler.register(new Command("help", "", "Display all available commands", (_, args) =>
            {
                string final = $"{handler.commands.Count} commands:\n";
                foreach (var cmd in handler.commands)
                {
                    final = final + $" - {cmd.Value.name} {cmd.Value.textparam} - {cmd.Value.desc}\n";
                }
                
                GD.Print(final);
            }));
            
            handler.register(new Command("admin", "[id]", "Give or remove (toggle) the specified player id's 'host' (administrative powers)", (_, args) =>
            {
                short id;
                if (!short.TryParse((string) args[0], out id))
                {
                    GD.PrintErr($"That argument is not a {typeof(short)}!");
                    return;
                }
                var player = NetworkManager.FindPlayer(id);
                if (player != null)
                {
                    player.IsHost = !player.IsHost;
                    GD.Print($"{player} => {(player.IsHost ? "has been promoted" : "has been demoted")}");
                    NetworkManager.SendMessage(NetworkManager.loc.SERVER, player.IsHost ? $"[color={Funcs.ColorToHex(Pals.command)}]You have been promoted![/color]" : $"[color={Funcs.ColorToHex(Pals.unimportant)}]You have been demoted.[/color]");
                }
                else
                {
                    GD.PrintErr($"Can't find a player with id '{args[0]}'!");
                }
            }));
        }

        public static void ReadCmd()
        {
            new Thread(() =>
            {
                while (true)
                {
                    var line = Console.ReadLine();
                    var (cmdResp, cmd) = handler.handle(line);
                    if(cmdResp == HandleResponse.BadArguments)
                        GD.PrintErr($"Invalid arguments supplied. Required arguments: {cmd.textparam}");
                }
            }).Start();
        }

        public override void _Ready()
        {
            InitEvents();
            InitCommands();
#pragma warning disable 4014
            ReadCmd();
#pragma warning restore 4014
        }
    }
}