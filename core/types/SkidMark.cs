using Godot;

namespace Casanova.core.types
{
	public class Skid
	{
		public int Length;
		public float Width;
		public Curve Curve;
		public Color Color;
		public float Opacity;
		public bool Boost;
	}
	public abstract class SkidMark : Line2D
	{
		public Skid Info;
		public Vector2 Pos;
	}
}
