using Casanova.core.types;
using Casanova.core.types.effects;
using Godot;
using Godot.Collections;
using static Casanova.core.Vars.Enums;

namespace Casanova.core.content
{
    public class UnitTypes
    {
        // ground units
        public static UnitType explorer;

        // air units
        public static UnitType crimson;

        public static void Init()
        {
            
            #region effects
            
            var rocketEngineCurve = new Curve();
            rocketEngineCurve.AddPoint(new Vector2(1, 1f));
            rocketEngineCurve.AddPoint(new Vector2(0.5f, 1.5f));
            rocketEngineCurve.AddPoint(new Vector2(0, 0.2f));

            var wheelSkidCurve = new Curve();
            var rng = new RandomNumberGenerator();
            for (var i = 0; i < 100; i++)
                wheelSkidCurve.AddPoint(new Vector2(i / 100f, rng.RandiRange(900, 1100) / 1000f));

            var explorerSkid = new Skid
            {
                Length = 300,
                Width = 4,
                Opacity = 10,
                Curve = wheelSkidCurve,
                Color = new Color(0, 0, 0)
            };

            var explorerSmoke = new ParticleInfo
            {
                Direction = new Vector2(0, 1),
                Velocity = 50f,
                Amount = 64
            };
            
            var crimsonSkid = new Skid
            {
                Length = 15,
                Width = 5,
                Color = new Color(255 / 255f, 254 / 255f, 152 / 255f),
                Opacity = 75,
                Curve = rocketEngineCurve
            };
            
            #endregion

            explorer = new UnitType("explorer.png")
            {
                Name = "Explorer",
                Description = "Starting bot equipped with a low-power building Projector.",
                MaxSpeed = 140f,
                Acceleration = 600f,
                Deceleration = 400f,
                RotationSpeed = 8f,
                Health = 100f,
                CollisionShape = new []
                {
                    new Vector2(26, 22),
                    new Vector2(26, -25),
                    new Vector2(-26, -25),
                    new Vector2(-26, -21)
                },
                ShadowBlur = 4,
                ShadowOffset = new Vector2(0, 0.2f),
                MovementType = MovementType.Ground,

                SkidMarks = new System.Collections.Generic.Dictionary<Vector2, Skid>
                {
                    {new Vector2(-4, 4), explorerSkid},
                    {new Vector2(4, 4), explorerSkid}
                },

                ParticleEffects = new System.Collections.Generic.Dictionary<Vector2, ParticleInfo>
                {
                    {new Vector2(0, 4), explorerSmoke}
                }
            };

            crimson = new UnitType("crimson.png")
            {
                Name = "Crimson",
                Description = "A big drone equipped with a Mk1 Mining laser. Flies at high altitudes.",
                MaxSpeed = 90f,
                Acceleration = 170f,
                RotationSpeed = 3.5f,
                Deceleration = 50f,
                Health = 500f,
                CollisionShape = new []
                {
                    new Vector2(24, 32),
                    new Vector2(24, -8),
                    new Vector2(0, -32),
                    new Vector2(-24, -8),
                    new Vector2(-24, -32),
                    new Vector2(0, 12)
                },
                ShadowOffset = new Vector2(-15, 20),
                MovementType = MovementType.Air,

                SkidMarks = new System.Collections.Generic.Dictionary<Vector2, Skid>
                {
                    {new Vector2(6, 8.7f), crimsonSkid},
                    {new Vector2(-6, 8.7f), crimsonSkid}
                }
            };
        }

        public static void Load()
        {
            // load shadows
            foreach (UnitType type in Vars.Enums.UnitTypes.Values)
            {
                if (type.ShadowBlur > 0)
                {
                    type.ShadowTexture = Funcs.BlurTexture(type.SpriteTexture, type.ShadowBlur);
                }
                else
                {
                    type.ShadowTexture = type.SpriteTexture;
                }
            }
        }
    }
}