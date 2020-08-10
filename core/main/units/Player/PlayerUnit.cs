using Godot;

namespace Casanova.core.main.units.Player
{
	public class PlayerUnit : Unit
	{
		// These variables are declared in units/second
		public static float acceleration = 1800f;
		public static float decellaration = 800f;
		public static float max_speed = 215f;
		public static float speed = 0f;
		
		// Declared in lerp fraction/delta
		public static float rotation_speed = 6.5f;
		
		// Declared as a fraction of 1.0f
		public static float bounciness = 0.85f;
		public static float lubrication = 0.7f;
		
		public static Vector2 Vel;
		public Vector2 GetInputAxis()
		{
			var axis = new Vector2();
			axis.x = Input.GetActionStrength("right") - Input.GetActionStrength("left");
			axis.y = Input.GetActionStrength("down") - Input.GetActionStrength("up");
			return axis;
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
			Vel = Vel.Clamped(max_speed);
		}

		public override void _PhysicsProcess(float delta)
		{
			// makes movement look janky on 60hz+ monitors, will use _Process instead which is bound to FPS
		}

		public override void _Process(float delta)
		{
			var axis = GetInputAxis();
			if (axis == Vector2.Zero)
				ApplyFriction(decellaration * delta);
			else
			{
				ApplyMovement(axis * acceleration * delta);
				Rotation = Mathf.LerpAngle(Rotation, axis.Angle(), rotation_speed * delta);
			}
			var collision = MoveAndCollide(Vel * delta);
			
			
			if (collision != null)
			{
				//Vel = Vel.Bounce(collision.Normal * bounciness);
				Vel = Vel.Slide(collision.Normal * lubrication);
			}
			
			
			speed = Vel.Length();
		}
	}
}
