using Godot;

namespace Casanova.core.types
{
    public class FlyingBody : KinematicBody2D
    {
        // These variables are declared in units/second
        private float Acceleration = 29800f;
        private readonly float Decellaration = 29800f;
        private float MaxSpeed = 5f;
        public float Speed;
        public Vector2 InWorldPosition;
		
        // Declared in lerp fraction/delta
        private float RotationSpeed = 9f;
		
        // Declared as a fraction of 1.0f
        protected float Bounciness = 0.7f;
        private float Lubrication = 0.2f;
        
        
        private Vector2 Vel;
        public Vector2 Axis;
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

        private void ProcessMovement(float delta)
        {
            if (Axis == Vector2.Zero)
                ApplyFriction(Decellaration * delta);
            else
            {
                ApplyMovement(Axis * delta * Acceleration);
                Rotation = Mathf.LerpAngle(Rotation, Axis.Angle() + Mathf.Deg2Rad(90), RotationSpeed * delta);
            }
			
			
            var collision = MoveAndCollide(Vel * delta);

            if (collision != null)
            {
                Vel = Vel.Slide(collision.Normal * Lubrication);
                //Vel = Vel.Bounce(collision.Normal * Bounciness);
            }
			
			
            Speed = Vel.Length();
            InWorldPosition = Position;
        }
        
        public override void _Process(float delta)
        {
            ProcessMovement(delta);
        }
    }
}