using Godot;

namespace Casanova.core.types.effects
{
    public class Shadow : Sprite
    {
        public Vector2 Offset = Vector2.Zero;
        private Node2D Parent;

        public override void _Ready()
        {
            Parent = GetParent<Node2D>();
        }

        public override void _Process(float delta)
        {
            GlobalPosition = Parent.GlobalPosition + Offset;
        }
    }
}