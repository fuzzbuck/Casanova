using System;
using System.Data.SqlClient;
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
        
        # region Networking
        public float RotateBy;
        public Vector2 DesiredPosition;
        # endregion

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
            Weight = Mass * Vars.WeightMassMultiplier;
            Inertia = Type.Inertia;
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
        
        
        /* Rotates to desired rotation with interpolation weight being speed / max_speed,
         * Also handles Networking, if RotateBy (networking variable) is not 0, the rotation
         * needs to be interpolated to that one.
         * In that case, interpolation weight is proportional to the amount needed to rotate
         */
        protected virtual void ApplyRotation(float delta, Physics2DDirectBodyState state)
        {
            if (Vel.Length() > 0f || RotateBy != 0)
            {
                Transform2D xform = state.Transform.Rotated(MathU.LerpAngle(state.Transform.Rotation, RotateBy == 0 ? Axis.Angle() + Mathf.Deg2Rad(90) : RotateBy, 
                    Type.RotationSpeed * delta + Mathf.Abs(RotateBy / Mathf.Tau)) - state.Transform.Rotation);
                
                state.Transform = xform;
            }
        }

        # region Networking
        public virtual void ApplyNetworkPosition()
        {
            if (Position.DistanceTo(DesiredPosition) > Vars.Networking.unit_desync_treshold)
            {
                Position = DesiredPosition;
            }
        }

        # endregion

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

            ApplyRotation(state.Step, state);
        }
    }
}