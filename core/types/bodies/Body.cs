using Casanova.core.types.bodies.effects;
using Godot;

namespace Casanova.core.types.bodies
{
    public abstract class Body : KinematicBody2D
    {
        public float Acceleration;
        public Vector2 Axis;

        // Declared as a fraction of 1.0f
        protected float Bounciness = 0.98f;

        public CollisionPolygon2D CollisionHitbox;
        
        public float Decelleration;
        public Vector2 InWorldPosition;
        public float MaxSpeed;
        public float RotationSpeed;
        public Shadow Shadow;

        public float Speed;

        public UnitType Type;
        public Sprite Sprite;

        protected Vector2 Vel;

        public void Init(UnitType type)
        {
            MaxSpeed = type.MaxSpeed;
            RotationSpeed = type.RotationSpeed;
            Acceleration = type.Acceleration;
            Decelleration = type.Deceleration;

            Sprite = GetNode<Sprite>("Sprite");
            Shadow = GetNode<Shadow>("Shadow");
            CollisionHitbox = GetNode<CollisionPolygon2D>("CollisionPolygon2D");
            
            Sprite.Texture = type.SpriteTexture;
            
            Shadow.Texture = type.ShadowTexture;
            Shadow.Offset = type.ShadowOffset;
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
            if(Axis != Vector2.Zero)
                Rotation = Mathf.LerpAngle(Rotation, Axis.Angle() + Mathf.Deg2Rad(90), RotationSpeed * delta);
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
            
            if(Vel.Length() > 0f)
                ApplyRotation(delta);

            var collision = MoveAndCollide(Vel * delta);
            if (collision != null) Vel = Vel.Slide(collision.Normal) * Bounciness;


            Speed = Vel.Length();
            InWorldPosition = Position;
        }

        public override void _Process(float delta)
        {
            ProcessMovement(delta);
        }
    }
}