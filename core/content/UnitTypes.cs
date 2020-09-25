using Casanova.core.types;
using Godot;
using Godot.Collections;
using Particles = Casanova.core.types.Particles;

namespace Casanova.core.content
{
	public class UnitTypes
	{
		public static UnitType explorer, crimson;

		public static void Init()
		{
			var rocketEngineCurve = new Curve();
			rocketEngineCurve.AddPoint(new Vector2(1, 1f));
			rocketEngineCurve.AddPoint(new Vector2(0.5f, 1.5f));
			rocketEngineCurve.AddPoint(new Vector2(0, 0.2f));
			
			var wheelSkidCurve = new Curve();
			var rng = new RandomNumberGenerator();
			for (int i = 0; i < 100; i++)
			{
				wheelSkidCurve.AddPoint(new Vector2(i/100f, rng.RandiRange(900, 1100)/1000f));
			}

			var explorerSkid = new Skid
			{
				Length = 300,
				Width = 4,
				Opacity = 5,
				Curve = wheelSkidCurve,
				Color = new Color(0, 0, 0),
			};

			var explorerSmoke = new ParticleInfo
			{
				Direction = new Vector2(0, 1),
				Velocity = 50f,
				Amount = 64
			};
			
			var explorerBoost = new Skid
			{
				Length = 5,
				Width = 6,
				Color = new Color(255 / 255f, 254 / 255f, 152 / 255f),
				Opacity = 75,
				Curve = rocketEngineCurve,
				Boost = true
			};
			
			explorer = new UnitType
			{
				Name = "Explorer",
				Description = "Starting bot equipped with a low-power building Projector.",
				Acceleration = 25f,
				Friction = 0.8f,
				RotationSpeed = 16f,
				Health = 100f,
				MovementType = Enums.MovementType.Ground,
				BoostAmount = 2f,
				SkidMarks = new System.Collections.Generic.Dictionary<Vector2, Skid>
				{
					{new Vector2(-4, 3), explorerSkid},
					{new Vector2(4, 3), explorerSkid},
					{new Vector2(0, 3), explorerBoost}
				},
				
				ParticleEffects = new System.Collections.Generic.Dictionary<Vector2, ParticleInfo>
				{
					{new Vector2(-4, 3), explorerSmoke},
					{new Vector2(4, 3), explorerSmoke}
				}
			};

			var crimsonSkid = new Skid
			{
				Length = 10,
				Width = 6,
				Color = new Color(255 / 255f, 254 / 255f, 152 / 255f),
				Opacity = 75,
				Curve = rocketEngineCurve
			};
				
			crimson = new UnitType
			{
				Name = "Crimson",
				Description = "A big drone equipped with a Mk1 Mining laser. Flies at high altitudes making it untrackable to missiles.",
				Acceleration = 10f,
				RotationSpeed = 3.5f,
				Friction = 0.95f,
				Health = 500f,
				Height = 20f,
				ShadowBlur = 3f,
				MovementType = Enums.MovementType.Air,
				
				SkidMarks = new System.Collections.Generic.Dictionary<Vector2, Skid>
				{
					{new Vector2(6, 8.7f), crimsonSkid},
					{new Vector2(-6, 8.7f), crimsonSkid}
				}
			};
		}
	}
}
