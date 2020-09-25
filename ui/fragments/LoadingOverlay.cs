using Godot;

namespace Casanova.ui.fragments
{
    public class LoadingOverlay : Overlay
    {
        public int percent;
        public Label title;
        public TextureProgress tp;

        public int Percent
        {
            get => percent;
            set
            {
                percent = value;
                tp.Value = value;
            }
        }

        public override void _Ready()
        {
            tp = (TextureProgress) GetNode("CenterContainer").GetNode("ProgressBar");
            title = (Label) GetNode("Title");
        }
    }
}