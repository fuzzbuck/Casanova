using Godot;

namespace Casanova.ui.fragments
{
    public class Overlay : Control
    {
        private AnimationPlayer anim;

        public override void _Ready()
        {
            var content = GetNode("MarginContainer").GetNode("MarginContainer").GetNode("VBox");

            var top = content.GetNode("Top");
            var header = top.GetNode("Header");
            
            var bb = header.GetNode<Godot.Button>("Back");
            anim = GetNode<AnimationPlayer>("Animation");
            bb.Connect("pressed", this, "_onButtonPress");

            // enter
            anim.Play("Enter");
            anim.Connect("animation_finished", this, "_onAnimationFinish");
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey eventKey)
                if (eventKey.Pressed)
                    if (eventKey.Scancode == (int) KeyList.Escape)
                        Clear();
        }

        public void _onButtonPress()
        {
            Clear();
        }

        public void Clear()
        {
            if (anim.AssignedAnimation != "Leave")
            {
                anim.Stop();
                anim.Play("Leave");
                GetTree().SetInputAsHandled();
            }
        }

        public void _onAnimationFinish(string name)
        {
            if (name == "Leave") QueueFree();
        }
    }
}