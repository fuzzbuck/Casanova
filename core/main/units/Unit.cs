using Casanova.core.types;
using Godot;

namespace Casanova.core.main.units
{
    public class Unit : Node2D
    {
        public Body Body;
        public override void _Ready()
        {
            Body = GetNode<Body>("Body");
        }
    }
}
