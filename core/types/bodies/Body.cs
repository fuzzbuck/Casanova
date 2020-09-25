using System;
using Godot;

namespace Casanova.core.types.bodies
{
	public abstract class Body : KinematicBody2D
	{
		public void Init(float rotationSpeed, float acceleration, float friction, float shadowHeight, float shadowBlur, float boostamount)
		{
			RotationSpeed = rotationSpeed;
			Acceleration = acceleration;
			Friction = friction;
			
			Sprite = GetNode<Sprite>("Sprite");
			Shadow = GetNode<Shadow>("Shadow");
			CollisionHitbox = GetNode<CollisionPolygon2D>("CollisionPolygon2D");

			Shadow.Heigth = shadowHeight;
			Shadow.Blur = shadowBlur;

			BoostAmount = boostamount;
			// todo: give unit different sprites based on name
		}

		public Sprite Sprite;
		public Shadow Shadow;

		public CollisionPolygon2D CollisionHitbox;
		public float MaxSpeed;
		public float RotationSpeed;
		
		public float Acceleration;
		public float Friction;

		public float Speed;
		public Vector2 InWorldPosition;

		protected Vector2 Vel;
		public Vector2 Axis;
		
		public bool Boosting;
		public float BoostAmount;
		// Declared as a fraction of 1.0f
		protected float Bounciness = 0.98f;

		public void ProcessMovement(float delta)
		{
			Vel *= Friction;
			Vel += Axis.Normalized() * (Boosting ? Acceleration * BoostAmount : Acceleration);
			
			if (Axis != Vector2.Zero) {
				Rotation = Mathf.LerpAngle(Rotation, Axis.Angle() + Mathf.Deg2Rad(90), RotationSpeed * delta);
			}
			
			var collision = MoveAndCollide(Vel * delta);
			if (collision != null)
			{
				Vel = Vel.Slide(collision.Normal) * Bounciness;
			}
			
			
			Speed = Vel.Length();

			InWorldPosition = Position;
		}
		
		public override void _Process(float delta)
		{
			ProcessMovement(delta);
		}
	}
}
