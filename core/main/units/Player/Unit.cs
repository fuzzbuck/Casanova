using Godot;

namespace Casanova.core.main.units
{
	public class Unit : KinematicBody2D
	{
		// These variables are declared in units/second
		protected float Acceleration = 1800f;
		protected float Decellaration = 800f;
		protected float MaxSpeed = 215f;
		public float Speed = 0f;
		protected Vector2 InWorldPosition;
		
		// Declared in lerp fraction/delta
		protected float RotationSpeed = 6.5f;
		
		// Declared as a fraction of 1.0f
		protected float Bounciness = 0.85f;
		protected float Lubrication = 0.7f;
		
		public Vector2 Vel;
		public Vector2 Axis;

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
				ApplyMovement(Axis * Acceleration * delta);
				Rotation = Mathf.LerpAngle(Rotation, Axis.Angle(), RotationSpeed * delta);
			}
			
			
			var collision = MoveAndCollide(Vel * delta);
			
			if (collision != null)
			{
				Vel = Vel.Slide(collision.Normal * Lubrication);
			}
			
			Speed = Vel.Length();
			InWorldPosition = Position;
		}

		public override void _Process(float delta)
		{
			ProcessMovement(delta);
		}
		

		public override void _Ready()
		{
		
		}
	}
}
