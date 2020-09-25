using Godot;

namespace Casanova.core.types
{
    public class ParticleInfo
    {
        public int Amount;
        public Vector2 Direction;
        public float Size;
        public Curve SizeCurve;
        public Texture Texture;
        public float Velocity;
    }

    public abstract class Particles : CPUParticles2D
    {
        public ParticleInfo Info;
        public Vector2 Pos;
    }
}