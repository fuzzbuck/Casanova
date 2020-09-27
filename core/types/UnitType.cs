using System.Collections.Generic;
using Casanova.core.types.bodies.effects;
using Godot;
using static Casanova.core.Vars.Enums;

namespace Casanova.core.types
{
    
    public class UnitType
    {
        public int Id;
        
        public Texture SpriteTexture;
        public Texture ShadowTexture;

        public float Acceleration;
        public float Deceleration;
        public string Description = "Attacks.";

        public float Health = 100f;
        public float Height;

        public float MaxSpeed;

        public MovementType MovementType = MovementType.Ground;

        public string Name = "Attacker";
        public Dictionary<Vector2, ParticleInfo> ParticleEffects;
        public float RotationSpeed;
        
        public int ShadowBlur;
        public Vector2 ShadowOffset = Vector2.Zero;

        public Dictionary<Vector2, Skid> SkidMarks; // <Vector2> offset, <Skid> skid info

        public UnitType(string spriteName)
        {
            SpriteTexture = ResourceLoader.Load<Texture>(Vars.path_sprites + $"/units/{spriteName}");
            Id = UnitTypes.Count;

            UnitTypes.Add(UnitTypes.Count, this);
        }
    }
}