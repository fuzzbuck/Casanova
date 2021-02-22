using System;
using Casanova.core.utils;
using Godot;

namespace Casanova.core.types.bodies
{
    public class Air : Body
    {
        protected override void ApplyRotation(float delta, Physics2DDirectBodyState state)
        {
            if (Vel.Length() > 0f || RotateBy != 0)
            {
                Transform2D xform = state.Transform.Rotated(MathU.LerpAngle(state.Transform.Rotation, RotateBy == 0 ? LinearVelocity.Angle() + Mathf.Deg2Rad(90) : RotateBy, 
                                                                Type.RotationSpeed * delta * (Speed / Type.MaxSpeed) + Mathf.Abs(RotateBy / Mathf.Tau)) - state.Transform.Rotation);
                
                state.Transform = xform;
            }
        }
    }
}