using Casanova.core.net.server;
using Godot;
using Godot.Collections;
using static Casanova.core.Vars.Networking;

namespace Casanova.core.net.headless_server
{
    public class ServerLauncher : Node
    {
        public override void _Ready()
        {
            isHeadless = OS.HasFeature("Server") || new Array(OS.GetCmdlineArgs()).Contains("server");
            Server.IsDedicated = true;

            GD.Print($"isHeadless: {isHeadless}");
            if (!isHeadless)
                return;

            Server.Start(128, defaultPort);

            var tree = GetTree();
            tree.ChangeSceneTo(ResourceLoader.Load<PackedScene>(Vars.path_world + "/World.tscn"));
        }
    }
}