using Casanova.core.net.server;
using Casanova.core.utils;
using Godot;
using Godot.Collections;
using static Casanova.core.Vars.Networking;

namespace Casanova.core.net.control
{
    public class HeadlessLauncher : Node
    {
        public override void _Ready()
        {
            IsHeadless = OS.HasFeature("Server") || new Array(OS.GetCmdlineArgs()).Contains("server");

            GD.Print($"IsHeadless: {IsHeadless}");
            if (!IsHeadless)
                return;

            Server.Start(128, defaultPort);

            var tree = GetTree();
            tree.ChangeSceneTo(References.main_world);
        }
    }
}