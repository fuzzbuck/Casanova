using System;
using Casanova.core.utils;
using Godot;

namespace Casanova.core.types.bodies
{
    public class Air : Body
    {
        protected override void ApplyRotation(float delta, Physics2DDirectBodyState state)
        {
            float to = LinearVelocity.Angle() + Mathf.Deg2Rad(90);
            Transform2D xform = state.Transform.Rotated(MathU.LerpAngle(state.Transform.Rotation, to, 
                Type.RotationSpeed * delta * (Speed/Type.MaxSpeed)) - state.Transform.Rotation);

            state.Transform = xform;
        }
    }
}