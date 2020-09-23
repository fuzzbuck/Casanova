using System;
using Godot;

namespace Casanova.core.main.units
{
    public class Tag : Node2D
    {
        private KinematicBody2D _kinematicBody2D;
        
        public Label TagFakeLabel;
        public RichTextLabel TagLabel;
        public override void _Ready()
        {
            _kinematicBody2D = GetParent().GetNode<KinematicBody2D>("Body");
            
            TagFakeLabel = GetNode<Label>("FakeLabel");
            TagLabel = TagFakeLabel.GetNode<RichTextLabel>("Text");
        }

        public override void _Process(float delta)
        {
            GlobalRotation = 0f;
            GlobalPosition = _kinematicBody2D.GlobalPosition + new Vector2(0, 10);
        }

        public void UpdateTag(string text)
        {
            TagLabel.BbcodeText = $"[center]{text}[/center]";
				
            TagFakeLabel.Text = String.Empty;
            TagFakeLabel.RectPosition = new Vector2(0, 67);
            TagFakeLabel.RectSize = Vector2.Zero;
            TagFakeLabel.Text = TagLabel.Text;
				

            if (text == String.Empty)
            {
                Visible = false;
            }
            else
            {
                Visible = true;
            }
        }
    }
}
