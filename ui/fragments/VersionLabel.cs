using Casanova.core;
using Godot;
using Label = Casanova.ui.elements.Label;

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