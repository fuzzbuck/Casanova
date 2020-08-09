using Godot;
using System;
using System.Security.Principal;
using Casanova.core;

public class PlayerUnit : RigidBody2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	public static float accel = 5f;
	public static float decell = 0.1f;
	
	public static float max_speed = 300f;
	public static Vector2 vel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	public static void process_input()
	{
		if (Input.IsActionPressed("right") && vel.x < max_speed)
			vel.x += accel;
		if (Input.IsActionPressed("left") && -vel.x < max_speed)
			vel.x -= accel;
		if (Input.IsActionPressed("up") && -vel.y < max_speed)
			vel.y -= accel;
		if (Input.IsActionPressed("down") && vel.y < max_speed)
			vel.y += accel;
		
	}

	public override void _PhysicsProcess(float delta)
	{
		process_input();
		AngularVelocity = 3f;
		LinearVelocity = vel;
		GD.Print(vel);
	}

	public override void _Process(float delta)
	{
		
	}
}
