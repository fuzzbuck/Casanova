using Casanova.core.types.effects;
using Godot;
using Godot.Collections;
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
        public float Mass;
        public string Description = "Attacks.";
        public string Body = "Body";

        public float Health = 100f;
        public float Height;

        public float MaxSpeed;

        public string Name = "Attacker";
        public System.Collections.Generic.Dictionary<Vector2, ParticleInfo> ParticleEffects;
        public float RotationSpeed;
        
        public int ShadowBlur;
        public Vector2 ShadowOffset = Vector2.Zero;
        
        public Vector2[] CollisionShape;

        public System.Collections.Generic.Dictionary<Vector2, Skid> SkidMarks; // <Vector2> offset, <Skid> skid info

        public UnitType(string spriteName)
        {
            // this can stay here instead of References.cs since it is loaded on launch time
            SpriteTexture = ResourceLoader.Load<Texture>(Vars.path_sprites + $"/units/{spriteName}");
            Id = UnitTypes.Count;

            UnitTypes.Add(UnitTypes.Count, this);
        }
    }
}