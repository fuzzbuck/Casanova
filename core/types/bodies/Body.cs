using System;
using Godot;

namespace Casanova.core.types.bodies
{
    public abstract class Body : KinematicBody2D
    {
        public void Init(float maxSpeed, float rotationSpeed, float acceleration, float decelleration)
        {
            MaxSpeed = maxSpeed;
            RotationSpeed = rotationSpeed;
            Acceleration = acceleration;
            Decelleration = decelleration;
        }

        public float MaxSpeed;
        public float RotationSpeed;
        
        public float Acceleration;
        public float Decelleration;
        public float DiagonalLimit = 0.72f;
        
        public float Speed;
        public Vector2 InWorldPosition;

        protected Vector2 Vel;
        public Vector2 Axis;

        public virtual void ProcessMovement(float delta)
        {
            if (Axis == Vector2.Zero)
                Vel = Vector2.Zero;
            else
            {
                if (Axis.x != 0 && Axis.y != 0)
                    Axis *= DiagonalLimit;
                
                Vel = Axis * MaxSpeed;
                Rotation = Mathf.LerpAngle(Rotation, Axis.Angle() + Mathf.Deg2Rad(90), RotationSpeed * delta);
            }

            MoveAndCollide(Vel * MaxSpeed * delta);
            Speed = Vel.Length();
            InWorldPosition = Position;
        }
        
        public override void _Process(float delta)
        {
            ProcessMovement(delta);
        }
    }
}
