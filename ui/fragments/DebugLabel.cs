using Casanova.core.main.world;
using Godot;
using Label = Casanova.ui.elements.Label;

namespace Casanova.ui.fragments
{
    public class DebugLabel : Label
    {
        private bool UnitLog = false;
        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventKey eventKey)
                if (eventKey.Pressed)
                {
                    if (eventKey.Scancode == (int) KeyList.F3)
                        Visible = !Visible;
                    if (eventKey.Scancode == (int) KeyList.F4)
                        UnitLog = !UnitLog;
                }

        }

        public override void _Process(float delta)
        {
            var utext = "";
            foreach (var kvp in NetworkManager.UnitsGroup)
            {
                utext = utext + kvp.Value.netId + ": " + kvp.Value.Body.Position + " -> " + kvp.Value.Body.MoveBy + ", rotation: " + kvp.Value.Body.Transform.Rotation + " -> " + kvp.Value.Body.RotateBy + "\n";
            }
            Text = Engine.GetFramesPerSecond() + " fps\n" + Engine.TargetFps + " cap\n" + Engine.IterationsPerSecond +
                   " ips" + "\n" + (UnitLog ? utext : "");
        }
    }
}