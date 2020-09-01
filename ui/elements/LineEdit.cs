using Casanova.core;

namespace Casanova.ui.elements
{
    public class LineEdit : Godot.LineEdit
    {
        public bool custom_behaviour = false;
        public override void _Ready()
        {
            if (Vars.PersistentData.isMobile)
            {
                Connect("focus_entered", this, "_onFocus");
            }
        }

        private void _onFocus()
        {
            if (custom_behaviour)
                return;
            
            var mte = Interface.Utils.spawnMte(Text);
            mte.label = this;
        }
    }
}
