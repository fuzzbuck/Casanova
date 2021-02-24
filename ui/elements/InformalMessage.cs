using System;
using Godot;

namespace Casanova.ui.elements
{
    public class InformalMessage : CenterContainer
    {
        private AnimationPlayer anim;
        private Timer timer;

        public override void _Ready()
        {
            anim = GetNode<AnimationPlayer>("Anim");
            timer = GetNode<Timer>("Timer");

            anim.Connect("animation_finished", this, "_onAnimFinished");
            timer.Connect("timeout", this, "_onTimeout");

            anim.Play("Enter");
        }

        public void SetMessage(string text)
        {
           GetNode<Godot.Label>("Label").Text = text;
        }

        public void SetTime(float time)
        {
            timer.WaitTime = time;
            timer.Start();
        }

        public void _onTimeout()
        {
            anim.Stop();
            anim.Play("Leave");
        }

        public void Skip()
        {
            _onAnimFinished("Leave");
        }

        public void _onAnimFinished(string name)
        {
            if (name == "Leave")
            {
                Interface.LatestInformalMessage = null;
                try // try for in case its already disposed
                {
                    QueueFree();
                }
                catch (Exception e)
                {
                    GD.Print(e.Message);
                }
            }
        }
    }
}