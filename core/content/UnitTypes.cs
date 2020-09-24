using System.Collections.Generic;
using Casanova.core.types;
using Godot;

namespace Casanova.core.content
{
    public class UnitTypes
    {
        public static UnitType crimson, ulysses;
        
        public static void Init()
        {
            crimson = new UnitType
            {
                Name = "Crimson",
                Description = "Starting bot equipped with a high powered Projector.",
                MaxSpeed = 190f,
                Acceleration = 1200f,
                Deceleration = 900f,
                RotationSpeed = 6f,
                Health = 100f,
                MovementType = Enums.MovementType.Ground,
                
                SkidMarks = new Dictionary<Vector2, float>
                {
                    {new Vector2(-12, 0), 7},
                    {new Vector2(12, 0), 7}
                },
                SkidLength = 600,
                SkidOpacity = 2
            };
            ulysses = new UnitType
            {
                Name = "Ulysses",
                Description = "A big drone equipped with a Mk1 Mining laser.",
                MaxSpeed = 180f,
                Acceleration = 700f,
                RotationSpeed = 3.5f,
                Deceleration = 400f,
                Health = 500f,
                MovementType = Enums.MovementType.Air,
                
                SkidMarks = new Dictionary<Vector2, float>
                {
                    {new Vector2(7, 8), 5},
                    {new Vector2(-7, 8), 5}
                },
                SkidLength = 8,
                SkidColor = new Color(255 / 255f, 254 / 255f, 152 / 255f),
                SkidOpacity = 50
            };
        }
    }
}
