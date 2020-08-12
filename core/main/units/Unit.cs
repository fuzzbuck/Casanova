using Godot;

namespace Casanova.core.main.units
{
	public class Unit : KinematicBody2D
	{
		public Unit()
		{
		}

		// These variables are declared in units/second
		float Acceleration = 1800f;
		float Decellaration = 800f;
		float MaxSpeed = 215f;
		protected float Speed = 0f;
		
		// Declared in lerp fraction/delta
		float RotationSpeed = 6.5f;
		
		// Declared as a fraction of 1.0f
		float Bounciness = 0.85f;
		float Lubrication = 0.7f;
		
		Vector2 Vel;
		
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
		
		// todo: make it use pathfinder or something later
		public virtual Vector2 _GetInputAxis()
		{
			return new Vector2(0, 0);
		}
		public void ProcessMovement(float delta)
		{
			var axis = _GetInputAxis();
			if (axis == Vector2.Zero)
				ApplyFriction(Decellaration * delta);
			else
			{
				ApplyMovement(axis * Acceleration * delta);
				Rotation = Mathf.LerpAngle(Rotation, axis.Angle(), RotationSpeed * delta);
			}
			var collision = MoveAndCollide(Vel * delta);
			
			
			if (collision != null)
			{
				//Vel = Vel.Bounce(collision.Normal * bounciness);
				Vel = Vel.Slide(collision.Normal * Lubrication);
			}
			
			
			Speed = Vel.Length();
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
