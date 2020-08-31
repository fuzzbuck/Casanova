using Godot;

namespace Casanova.core.main.units
{
    public class Tag : Node2D
    {
        private KinematicBody2D _kinematicBody2D;
        public override void _Ready()
        {
            _kinematicBody2D = GetParent<KinematicBody2D>();
        }

        public override void _Process(float delta)
        {
            GlobalRotation = 0f;
            GlobalPosition = _kinematicBody2D.GlobalPosition + new Vector2(0, 10);
        }
    }
}
