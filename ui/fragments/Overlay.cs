using Godot;

namespace Casanova.ui.fragments
{
    public class Overlay : Control
    {
        private AnimationPlayer anim;
        public override void _Ready()
        {
            var bb = GetNode("Title").GetNode<Button>("BackButton");
            anim = GetNode<AnimationPlayer>("Animation");
            bb.Connect("pressed", this, "_onButtonPress");
            
            // enter
            anim.Play("Enter");
            anim.Connect("animation_finished", this, "_onAnimationFinish");
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
            }
        }

        public void _onAnimationFinish(string name)
        {
            if (name == "Leave")
            {
                QueueFree();
            }
        }
    }
}
