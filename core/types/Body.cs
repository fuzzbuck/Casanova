using Godot;

namespace Casanova.core.types
{
    public class Body : KinematicBody2D
    {
        private float MaxSpeed = 15f;
        private float RotationSpeed = 15f;
        
        public float Speed;
        public Vector2 InWorldPosition;

        private Vector2 Vel;
        public Vector2 Axis;

        private void ProcessMovement(float delta)
        {
            if (Axis == Vector2.Zero)
                Vel = Vector2.Zero;
            else
            {
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
