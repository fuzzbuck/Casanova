using Godot;

namespace Casanova.ui.fragments
{
    public class LoadingOverlay : Overlay
    {
        public int percent;
        public TextureProgress tp;
        public Label title;

        public override void _Ready()
        {
            tp = (TextureProgress) GetNode("CenterContainer").GetNode("ProgressBar");
            title = (Label) GetNode("Title");
        }

        public int Percent
        {
            get => percent;
            set
            {
                percent = value;
                tp.Value = value;
            }
        }
    }
}
