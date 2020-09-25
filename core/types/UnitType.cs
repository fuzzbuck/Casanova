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
        public float Acceleration;
        public float Deceleration;
        public string Description = "Attacks.";

        public float Health = 100f;
        public float Height;

        public float MaxSpeed;

        public Enums.MovementType MovementType = Enums.MovementType.Ground;

        public string Name = "Attacker";
        public Dictionary<Vector2, ParticleInfo> ParticleEffects;
        public float RotationSpeed;
        public float ShadowBlur;

        public Dictionary<Vector2, Skid> SkidMarks; // <Vector2> offset, <Skid> skid info

        public UnitType()
        {
            Enums.UnitTypes.Add(Enums.UnitTypes.Count, this);
        }
    }
}