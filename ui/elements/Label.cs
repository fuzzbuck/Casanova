using Godot;

namespace Casanova.ui.elements
{
    public class Label : Godot.Label
    {
        public override void _Ready()
        {
            RectScale = new Vector2(1, 0.7f);
        }
    }
}
