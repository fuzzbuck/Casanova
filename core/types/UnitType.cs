using System.Collections.Generic;
using Godot;

namespace Casanova.core.types
{
	public class Enums
	{
		public enum MovementType
		{
			Ground,
			Air
		}

		public static Dictionary<int, UnitType> UnitTypes = new Dictionary<int, UnitType>();
	}
	public class UnitType
	{
		public UnitType()
		{
			Enums.UnitTypes.Add(Enums.UnitTypes.Count, this);
		}
		
		public string Name = "Attacker";
		public string Description = "Attacks.";
		
		public float Health = 100f;
		public float Height;
		public float ShadowBlur;
		
		public float Acceleration;
		public float Friction;
		public float RotationSpeed;
		
		public float BoostAmount = 1f;
		
		public Dictionary<Vector2, Skid> SkidMarks; // <Vector2> offset, <Skid> skid info
		public Dictionary<Vector2, ParticleInfo> ParticleEffects;

		public Enums.MovementType MovementType = Enums.MovementType.Ground;
	}
}
