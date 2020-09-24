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
    }
    public class UnitType
    {
        public string Name = "Attacker";
        public string Description = "Attacks.";
        
        public float Health;
        
        public float MaxSpeed;
        public float Acceleration;
        public float Deceleration;
        public float RotationSpeed;
        
        public Dictionary<Vector2, float> SkidMarks; // <Vector2> offset, <float> width
        public int SkidLength = 200;
        public int SkidOpacity = 10;
        public Color SkidColor = new Color(0, 0, 0);

        public Enums.MovementType MovementType = Enums.MovementType.Ground;
    }
}