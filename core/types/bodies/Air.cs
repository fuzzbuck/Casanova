using Godot;

namespace Casanova.core.types.bodies
{
    public class Air : Body
    {
        // Declared as a fraction of 1.0f
        protected float Bounciness = 0.7f;

        private void ApplyFriction(float amt)
        {
            if (Vel.Length() > amt)
                Vel -= Vel.Normalized() * amt;
            else
                Vel = Vector2.Zero;
        }

        private void ApplyMovement(Vector2 amt)
        {
            Vel += amt;
            Vel = Vel.Clamped(MaxSpeed);
        }

        public override void ProcessMovement(float delta)
        {
            if (Axis == Vector2.Zero)
                ApplyFriction(Decelleration * delta);
            else
            {
                ApplyMovement(Axis * delta * Acceleration);
                Rotation = Mathf.LerpAngle(Rotation, Axis.Angle() + Mathf.Deg2Rad(90), RotationSpeed * delta);
            }
			
			
            var collision = MoveAndCollide(Vel * delta);
            if (collision != null)
            {
                Vel = Vel.Bounce(collision.Normal * Bounciness);
            }
			
			
            Speed = Vel.Length();
            InWorldPosition = Position;
        }
    }
}