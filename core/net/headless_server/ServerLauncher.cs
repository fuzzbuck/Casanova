using Casanova.core.net.server;
using Godot;
using World = Casanova.core.main.world.World;
using static Casanova.core.Vars.Networking;

namespace Casanova.core.net.headless_server
{
    public class ServerLauncher : Node
    {
        public override void _Ready()
        {
            GD.Print($"isHeadless: {isHeadless}");
            if (!isHeadless)
                return;
            
            isHeadless = OS.HasFeature("Server");
            Server.IsDedicated = true;

            var tree = GetTree();
            tree.ChangeSceneTo(ResourceLoader.Load<PackedScene>(Vars.path_world + "/World.tscn"));
						
            ThreadManager.ExecuteOnMainThread(() =>
            {
                World world = (World) tree.CurrentScene;
							
                world.StartServer();
            });
        }
    }
}
