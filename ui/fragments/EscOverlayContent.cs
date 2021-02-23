using Casanova.core;
using Godot;

namespace Casanova.ui.fragments
{
    public class EscOverlayContent : VBoxContainer
    {
        public override void _Ready()
        {
            GetNode("Leave").Connect("pressed", this, "leave");
            GetNode("Exit").Connect("pressed", this, "exit");
        }


        /* Reload & go back to main menu */
        void leave()
        {
            Vars.Reload();
        }
        
        /* Unload & exit */
        void exit()
        {
            Vars.Unload();
        }
    }
}
