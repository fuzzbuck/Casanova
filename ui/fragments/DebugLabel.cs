using Godot;
using System;
using System.Threading.Tasks;

public class DebugLabel : Godot.Label
{
	public override void _Ready()
	{
		new System.Threading.Thread(() =>
		{
			while (true)
			{
				Text = Engine.GetFramesPerSecond() + " fps\n" + Engine.TargetFps + " cap\n" + Engine.IterationsPerSecond + " ips";
				if (Input.IsActionJustPressed("toggle_fullscreen"))
					OS.WindowFullscreen = !OS.WindowFullscreen;
				//Task.Delay(1000).Wait();
			}
		}).Start();
	}
	
}
