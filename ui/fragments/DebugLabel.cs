using Godot;
using Label = Casanova.ui.elements.Label;

namespace Casanova.ui.fragments
{
    public class DebugLabel : Label
    {
        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventKey eventKey)
                if (eventKey.Pressed)
                    if (eventKey.Scancode == (int) KeyList.F3)
                        Visible = !Visible;
        }

        public override void _Process(float delta)
        {
            Text = Engine.GetFramesPerSecond() + " fps\n" + Engine.TargetFps + " cap\n" + Engine.IterationsPerSecond +
                   " ips";
        }
    }
}