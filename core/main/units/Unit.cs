using System;
using Godot;

namespace Casanova.core.main.units
{
	public class Unit : Node
	{
		// These variables are declared in units/second
		protected float Acceleration = 1200f;
		protected float Decellaration = 600f;
		protected float MaxSpeed = 185f;
		public float Speed = 0f;
		public Vector2 InWorldPosition;
		
		// Declared in lerp fraction/delta
		protected float RotationSpeed = 6.5f;
		
		// Declared as a fraction of 1.0f
		protected float Bounciness = 0.7f;
		protected float Lubrication = 0.8f;
		
		public Vector2 Vel;
		public Vector2 Axis;

		public KinematicBody2D kinematicBody;

		public Node2D tagNode;
		public RichTextLabel tagLabel;
		
		private string tag;

		public string Tag
		{
			get => tag;
			set
			{
				tag = value;
				tagLabel.BbcodeText = $"[center]{tag}[/center]";
				if (value == String.Empty)
				{
					tagNode.Visible = false;
				}
				else
				{
					tagNode.Visible = true;
				}
			}
		}

		public override void _Ready()
		{
			kinematicBody = GetNode<KinematicBody2D>("Unit");
			tagNode = kinematicBody.GetNode<Node2D>("Tag");
			tagLabel = tagNode.GetNode("Panel").GetNode<RichTextLabel>("Name");
		}

		public void ApplyFriction(float amt)
		{
			if (Vel.Length() > amt)
				Vel -= Vel.Normalized() * amt;
			else
				Vel = Vector2.Zero;
		}

		public void ApplyMovement(Vector2 amt)
		{
			Vel += amt;
			Vel = Vel.Clamped(MaxSpeed);
		}
		
		public void ProcessMovement(float delta)
		{
			if (Axis == Vector2.Zero)
				ApplyFriction(Decellaration * delta);
			else
			{
				ApplyMovement(Axis * delta * Acceleration);
				kinematicBody.Rotation = Mathf.LerpAngle(kinematicBody.Rotation, Axis.Angle(), RotationSpeed * delta);
			}
			
			
			var collision = kinematicBody.MoveAndCollide(Vel * delta);
			
			
			if (collision != null)
			{
				Vel = Vel.Slide(collision.Normal * Lubrication);
				//Vel = Vel.Bounce(collision.Normal * Bounciness);
			}
			
			
			Speed = Vel.Length();
			InWorldPosition = kinematicBody.Position;
		}

		public override void _Process(float delta)
		{
			ProcessMovement(delta);
		}
	}
}
