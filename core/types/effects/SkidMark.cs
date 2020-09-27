using Godot;

namespace Casanova.core.types.bodies.effects
{
    public class Skid
    {
        public Color Color;
        public Curve Curve;
        public int Length;
        public float Opacity;
        public float Width;
    }

    public abstract class SkidMark : Line2D
    {
        public Skid Info;
        public Vector2 Pos;
    }
}