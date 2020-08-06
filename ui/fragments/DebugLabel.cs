using Godot;
using System;
using System.Threading.Tasks;

public class DebugLabel : Godot.Label
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		new System.Threading.Thread(() =>
		{
			while (true)
			{
				Text = Engine.GetFramesPerSecond() + " fps\n" + Engine.TargetFps + " cap\n" + Engine.IterationsPerSecond + " ips";
				//Task.Delay(1000).Wait();
			}
		}).Start();
	}
	
}
