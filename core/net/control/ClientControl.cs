using Casanova.core.main;
using Casanova.core.main.world;
using Casanova.core.net.client;
using Godot;
using static Casanova.core.Vars;

namespace Casanova.core.net.control
{
    public class ClientControl : Node
    {
        public static void InitEvents()
        {
            Events.PlayerJoin += (loc, player) =>
            {
                if (loc != NetworkManager.loc.CLIENT)
                    return;
                
                if(log_client)
                    GD.Print($"{client_string} [player {player.netId}:{player.Username}] joined the server.");
            };
            Events.PlayerConnect += (loc, id, name, ip) =>
            {
                if (loc != NetworkManager.loc.CLIENT)
                    return;
                
                NetworkManager.CreatePlayer(NetworkManager.loc.CLIENT, id, name);
                
                if(log_client)
                    GD.Print($"{client_string} creating [player {id}:{name}]");
                if (id == Client.MyId)
                {
                    if(log_client)
                        GD.Print($"{client_string} [player {id}:{name}] is us (local)");
                    PlayerController.LocalPlayer = NetworkManager.PlayersGroup[id];
                }
            };
        }

        public override void _Ready()
        {
            InitEvents();
        }
    }
}
