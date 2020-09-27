using Godot;

namespace Casanova.core.types.bodies
{
    public class Air : Body
    {
        protected override void ApplyRotation(float delta)
        {
            Rotation = Mathf.LerpAngle(Rotation, Vel.Angle() + Mathf.Deg2Rad(90), RotationSpeed * delta);
        }
    }
}