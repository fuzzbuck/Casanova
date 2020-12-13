using Casanova.core.utils;
using Godot;

namespace Casanova.core.types.bodies
{
    public class Air : Body
    {
        protected override void ApplyRotation(float delta)
        {
            AngularVelocity = MathU.DeltaAngle(RotationDegrees, Mathf.Rad2Deg(LinearVelocity.Angle()) + 270) * RotationSpeed * delta;
        }
    }
}