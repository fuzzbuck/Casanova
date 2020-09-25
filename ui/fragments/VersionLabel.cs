using Casanova.core;
using Godot;

namespace Casanova.ui.fragments
{
    public class VersionLabel : Label
    {
        public override void _Ready()
        {
            Text = Vars.ver;
        }
    }
}