using Casanova.core.types;
using Godot;
using Godot.Collections;

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

            explorer = new UnitType
            {
                Name = "Explorer",
                Description = "Starting bot equipped with a low-power building Projector.",
                MaxSpeed = 140f,
                Acceleration = 900f,
                Deceleration = 600f,
                RotationSpeed = 5f,
                Health = 100f,
                Height = 0.01f,
                ShadowBlur = 1.1f,
                MovementType = Enums.MovementType.Ground,
                
                SkidMarks = new System.Collections.Generic.Dictionary<Vector2, float>
                {
                    {new Vector2(-4, 4), 4},
                    {new Vector2(4, 4), 4}
                },
                SkidLength = 300,
                SkidOpacity = 10,
                SkidCurve = wheelSkidCurve
            };
            
            crimson = new UnitType
            {
                Name = "Crimson",
                Description = "A big drone equipped with a Mk1 Mining laser. Flies at high altitudes making it untrackable to missiles.",
                MaxSpeed = 180f,
                Acceleration = 700f,
                RotationSpeed = 3.5f,
                Deceleration = 400f,
                Health = 500f,
                Height = 20f,
                ShadowBlur = 3f,
                MovementType = Enums.MovementType.Air,
                
                SkidMarks = new System.Collections.Generic.Dictionary<Vector2, float>
                {
                    {new Vector2(6, 8.7f), 6},
                    {new Vector2(-6, 8.7f), 6}
                },
                SkidLength = 10,
                SkidColor = new Color(255 / 255f, 254 / 255f, 152 / 255f),
                SkidOpacity = 50,
                SkidCurve = rocketEngineCurve
            };
        }
    }
}
