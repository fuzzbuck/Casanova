using Godot;
using LineEdit = Casanova.ui.elements.LineEdit;

namespace Casanova.ui.fragments
{
    public class MobileTextInput : Control
    {
        public TextEdit texteditor;
        public Godot.Button button;
        public RichTextLabel preview;
        
        // label to change real-time
        public LineEdit label;

        public override void _Ready()
        {
            texteditor = GetNode<TextEdit>("TextEdit");
            button = texteditor.GetNode<Godot.Button>("Button");

            button.Connect("pressed", this, "_onButton");
            texteditor.Connect("text_changed", this, "_onTextEditChange");

            preview = GetNode("HBox").GetNode<RichTextLabel>("Preview");
        }

        private void _onTextEditChange()
        {
            preview.BbcodeText = texteditor.Text;
            if (label != null)
            {
                label.Text = texteditor.Text;
                label.EmitSignal("text_changed", texteditor.Text);
            }
        }

        private void _onButton()
        {
            QueueFree();
        }
    }
}