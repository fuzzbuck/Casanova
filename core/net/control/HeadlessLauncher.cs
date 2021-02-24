using Casanova.core.net.server;
using Casanova.core.utils;
using Godot;
using Godot.Collections;
using static Casanova.core.Vars;
using static Casanova.core.Vars.Networking;

namespace Casanova.core.net.control
{
    public class HeadlessLauncher : Node
    {
        public override void _Ready()
        {
            var cmdArgs = new Array(OS.GetCmdlineArgs());
            IsHeadless = OS.HasFeature("Server") || cmdArgs.Contains("server");

            if(log_log)
                GD.Print($"{log_string} IsHeadless: {IsHeadless}");
            
            if (!IsHeadless)
                return;

            Server.Start(128, defaultPort);

            var tree = GetTree();
            tree.ChangeSceneTo(References.main_world);
        }
    }
}