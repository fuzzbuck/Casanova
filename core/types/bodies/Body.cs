using System;
using Godot;

namespace Casanova.core.types.bodies
{
    public abstract class Body : KinematicBody2D
    {
        public void Init(float maxSpeed, float rotationSpeed, float acceleration, float deceleration, float shadowHeight, float shadowBlur)
        {
            MaxSpeed = maxSpeed;
            RotationSpeed = rotationSpeed;
            Acceleration = acceleration;
            Decelleration = deceleration;
            
            Sprite = GetNode<Sprite>("Sprite");
            Shadow = GetNode<Shadow>("Shadow");

            Shadow.Heigth = shadowHeight;
            Shadow.Blur = shadowBlur;

            // todo: give unit different sprites based on name
        }

        public Sprite Sprite;
        public Shadow Shadow;

        public float MaxSpeed;
        public float RotationSpeed;
        
        public float Acceleration;
        public float Decelleration;

        public float Speed;
        public Vector2 InWorldPosition;

        protected Vector2 Vel;
        public Vector2 Axis;

        // Declared as a fraction of 1.0f
        protected float Bounciness = 0.98f;

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

        public void ProcessMovement(float delta)
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
                Vel = Vel.Slide(collision.Normal) * Bounciness;
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
