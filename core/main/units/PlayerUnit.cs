using Godot;

namespace Casanova.core.main.units
{
	public class PlayerUnit : Unit
	{
		public static float accel = 1400f;
		public static float max_speed = 215f;
		public static float rotation_speed = 8f;
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
				ApplyFriction(accel * delta);
			else
			{
				ApplyMovement(axis * accel * delta);
				Rotation = Mathf.LerpAngle(Rotation, axis.Angle(), rotation_speed * delta);
			}
			Vel = MoveAndSlide(Vel);
		}
	}
}
