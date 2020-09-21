using Godot;

namespace Casanova.ui.elements
{
    public class InformalMessage : CenterContainer
    {
        private AnimationPlayer anim;
        public override void _Ready()
        {
            anim = GetNode<AnimationPlayer>("Anim");
            anim.Connect("animation_finished", this, "_onAnimFinished");
            anim.Play("Enter");
        }

        public void SetMessage(string text)
        {
            GetNode<Label>("Label").Text = text;
        }

        public void _onAnimFinished(string name)
        {
            if (name == "Leave")
            {
                QueueFree();
            }
            else
            {
                anim.Stop();
                anim.Play("Leave");
            }
        }
    }
}
