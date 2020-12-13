using System;
using System.Runtime.InteropServices;
using Casanova.core.types.effects;
using Casanova.core.utils;
using Godot;
using Godot.Collections;

namespace Casanova.core.types.bodies
{
    public abstract class Body : RigidBody2D
    {
        public float Acceleration;
        public Vector2 Axis;

        // Declared as a fraction of 1.0f
        protected float Bounciness = 0.98f;

        public CollisionPolygon2D CollisionHitbox;
        public Vector2[] CollisionHull;
        
        public float Decelleration;
        public Vector2 InWorldPosition;
        public float MaxSpeed;
        public float RotationSpeed;
        public Shadow Shadow;

        public float Speed;

        public UnitType Type;
        public Sprite Sprite;

        public Vector2 Vel;

        public void Init(UnitType type)
        {
            MaxSpeed = type.MaxSpeed;
            RotationSpeed = type.RotationSpeed;
            Acceleration = type.Acceleration;
            Decelleration = type.Deceleration;
            Mass = type.Mass;

            Sprite = GetNode<Sprite>("Sprite");
            Shadow = GetNode<Shadow>("Shadow");
            CollisionHitbox = GetNode<CollisionPolygon2D>("CollisionPolygon2D");

            CollisionHull = type.CollisionShape;
            CollisionHitbox.Polygon = CollisionHull;
            
            Sprite.Texture = type.SpriteTexture;
            
            Shadow.Texture = type.ShadowTexture;
            Shadow.ShadowOffset = type.ShadowOffset;
        }

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

        protected virtual void ApplyRotation(float delta)
        {
            if (Axis.Length() > 0f)
            {
                float deltaAngle = MathU.DeltaAngle(RotationDegrees, Mathf.Rad2Deg(Axis.Angle()) + 270);
                AngularVelocity = (deltaAngle > 0 ? Math.Max(deltaAngle, 10) : Math.Min(deltaAngle, -10)) * RotationSpeed * delta;
            }
        }

        protected virtual void ApplyRotationFriction(float delta)
        {
            AngularVelocity = Mathf.Lerp(AngularVelocity, 0, (RotationSpeed) * delta);
        }

        protected virtual void ProcessMovement(float delta)
        {
            if (Axis == Vector2.Zero)
            {
                ApplyFriction(Decelleration * delta);
            }
            else
            {
                ApplyMovement(Axis * delta * Acceleration);
            }


            Speed = Vel.Length();
            InWorldPosition = Position;
        }

        public override void _IntegrateForces(Physics2DDirectBodyState state)
        {
            ProcessMovement(state.Step);
            state.LinearVelocity = Vel;
            
            if(Vel.Length() > 0f)
                ApplyRotation(state.Step);
            else
                ApplyRotationFriction(state.Step);
        }
    }
}