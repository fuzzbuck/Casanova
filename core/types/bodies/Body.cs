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
        public Vector2 Axis;

        public CollisionPolygon2D CollisionHitbox;
        public Vector2[] CollisionHull;
        
        public Vector2 InWorldPosition;
        public Shadow Shadow;

        public UnitType Type;
        public Sprite Sprite;

        public float Speed;
        public Vector2 Vel;

        public void Init(UnitType type)
        {
            Type = type;
            
            Sprite = GetNode<Sprite>("Sprite");
            Shadow = GetNode<Shadow>("Shadow");
            CollisionHitbox = GetNode<CollisionPolygon2D>("CollisionPolygon2D");

            CollisionHull = type.CollisionShape;
            CollisionHitbox.Polygon = CollisionHull;
            
            Sprite.Texture = type.SpriteTexture;
            
            Shadow.Texture = type.ShadowTexture;
            Shadow.ShadowOffset = type.ShadowOffset;
            
            /* Copy Type variables to RigidBody2D */
            Mass = Type.Mass;
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
            Vel = Vel.Clamped(Type.MaxSpeed);
        }

        protected virtual void ApplyRotation(float delta, Physics2DDirectBodyState state)
        {
            if (Axis == Vector2.Zero)
                return;

            Transform2D xform = state.Transform.Rotated(Mathf.LerpAngle(state.Transform.Rotation, Axis.Angle() + Mathf.Deg2Rad(90), 
                Type.RotationSpeed * delta) - state.Transform.Rotation);

            state.Transform = xform;
        }

        protected virtual void ProcessMovement(float delta)
        {
            if (Axis == Vector2.Zero)
            {
                ApplyFriction(Type.Deceleration * delta);
            }
            else
            {
                ApplyMovement(Axis * delta * Type.Acceleration);
            }


            Speed = Vel.Length();
            InWorldPosition = Position;
        }

        public override void _IntegrateForces(Physics2DDirectBodyState state)
        {
            ProcessMovement(state.Step);
            state.LinearVelocity = Vel;
            state.AngularVelocity = Mathf.Lerp(state.AngularVelocity, 0f, Type.AngularDeceleration);
            
            if(Vel.Length() > 0f)
                ApplyRotation(state.Step, state);
        }
    }
}