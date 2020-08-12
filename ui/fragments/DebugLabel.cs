using Godot;

namespace Casanova.ui.fragments
{
	public class DebugLabel : Label
	{
		public override void _UnhandledInput(InputEvent @event)
		{
			if (@event is InputEventKey eventKey)
				if (eventKey.Pressed)
				{
					if (eventKey.Scancode == (int) KeyList.F3)
						Visible = !Visible;
					
					// todo: move this somewhere else
					if (eventKey.Scancode == (int) KeyList.F11)
						OS.WindowFullscreen = !OS.WindowFullscreen;
				}
		}

		public override void _Process(float delta)
		{
			Text = Engine.GetFramesPerSecond() + " fps\n" + Engine.TargetFps + " cap\n" + Engine.IterationsPerSecond + " ips";
		}
	}
}
