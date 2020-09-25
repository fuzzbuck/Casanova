using System;
using System.Collections.Generic;
using System.Security.Permissions;
using Casanova.core.content;
using Casanova.core.types;
using Casanova.core.types.bodies;
using Godot;
using Godot.Collections;
using Particles = Casanova.core.types.Particles;

namespace Casanova.core.main.units
{
	public static class Utils
	{
		public static Body CreateBody(UnitType type)
		{
			string bodyType = String.Empty;
			switch (type.MovementType)
			{
				case Enums.MovementType.Air:
					bodyType = "Air";
					break;
				case Enums.MovementType.Ground:
					bodyType = "Body";
					break;
			}
			
			Body body = (Body) ResourceLoader.Load<PackedScene>(Vars.path_types + $"/bodies/{bodyType}.tscn").Instance();
			body.Init(type.RotationSpeed, type.Acceleration, type.Friction, type.Height, type.ShadowBlur, type.BoostAmount);
			
			return body;
		}

		public static Node CreateTypeEffect(string name)
		{
			return ResourceLoader.Load<PackedScene>(Vars.path_types + $"/{name}.tscn").Instance();
		}
	}
	
	public class Unit : Node2D
	{
		public UnitType Type;
		public Node2D Content;
		public Body Body;
		private Array<SkidMark> SkidMarks = new Array<SkidMark>();
		private Array<Particles> Particles = new Array<Particles>();
		
		public override void _Ready()
		{
			Content = GetNode<Node2D>("Content");
			Body = Utils.CreateBody(Type);

			if (Type.ParticleEffects != null)
			{
				foreach (KeyValuePair<Vector2, ParticleInfo> kvp in Type.ParticleEffects)
				{
					
					var pos = kvp.Key;
					var particlesInfo = kvp.Value;
					var particles = (Particles) Utils.CreateTypeEffect("Particles");

					particles.Info = particlesInfo;
					particles.Pos = pos;

					particles.InitialVelocity = particlesInfo.Velocity;
					particles.Direction = particlesInfo.Direction;
					particles.Amount = particlesInfo.Amount;
					
					Particles.Add(particles);
					Content.AddChild(particles);

				}
			}

			if (Type.SkidMarks != null)
			{
				foreach (KeyValuePair<Vector2, Skid> kvp in Type.SkidMarks)
				{
					var pos = kvp.Key;
					var skid = kvp.Value;
					var skidMark = (SkidMark) Utils.CreateTypeEffect("SkidMark");
					
					var grad = new Gradient();

					var fade = skid.Color;
					fade.a = 0;
					
					grad.SetColor(1, skid.Color);
					grad.SetColor(0, fade);
					
					
					skidMark.Width = skid.Width;
					skidMark.Gradient = grad;
					skidMark.Pos = pos;
					skidMark.Info = skid;
					skidMark.Modulate = new Color(1, 1, 1, skid.Opacity / 100f);
					
					if (skid.Curve != null) 
						skidMark.WidthCurve = skid.Curve;
					
					SkidMarks.Add(skidMark);
					Content.AddChild(skidMark);
					
				}
			}
			
			AddChild(Body);
		}
		public override void _PhysicsProcess(float delta)
		{
			if (SkidMarks != null)
			{
				foreach (SkidMark skid in SkidMarks)
				{
					skid.AddPoint(Body.InWorldPosition + skid.Pos.Rotated(Body.Rotation));
					if (skid.Points.Length > skid.Info.Length)
						skid.RemovePoint(0);
					skid.Visible = !skid.Info.Boost | Body.Boosting;
				}
			}

			if (Particles != null)
			{
				foreach (Particles particles in Particles)
				{
					particles.GlobalPosition = Body.InWorldPosition + particles.Pos.Rotated(Body.Rotation);
					particles.GlobalRotation = Body.Rotation;
					particles.Visible = !particles.Info.Boost | Body.Boosting;
				}
			}
		}
	}
}
