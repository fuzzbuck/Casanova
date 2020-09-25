using Godot;

namespace Casanova.core.main.units
{
    public class Tag : Node2D
    {
        private KinematicBody2D _kinematicBody2D;

        public Label TagFakeLabel;
        public RichTextLabel TagLabel;

        public void Init()
        {
            _kinematicBody2D = GetParent().GetNode<KinematicBody2D>("Body");

            TagFakeLabel = GetNode<Label>("FakeLabel");
            TagLabel = TagFakeLabel.GetNode<RichTextLabel>("Text");

            SetProcess(true);
        }

        public override void _Process(float delta)
        {
            GlobalRotation = 0f;
            GlobalPosition = _kinematicBody2D.GlobalPosition + new Vector2(0, 10);
        }

        public void UpdateTag(string text)
        {
            TagLabel.BbcodeText = $"[center]{text}[/center]";

            TagFakeLabel.Text = string.Empty;
            TagFakeLabel.RectPosition =
                new Vector2(-52, 72); // if you ever want to hire me please look at this and reconsider your choices
            TagFakeLabel.RectSize = Vector2.Zero;
            TagFakeLabel.Text = TagLabel.Text + ", ";


            if (text == string.Empty)
                Visible = false;
            else
                Visible = true;
        }
    }
}