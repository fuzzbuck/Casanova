using Godot;
using System;
using Casanova.core;
using Casanova.ui;

public class LineEdit : Godot.LineEdit
{
    public override void _Ready()
    {
        if (Vars.PersistentData.isMobile)
        {
            Connect("focus_entered", this, "_onFocus");
        }
    }
    private void _onFocus()
    {
        var mte = Interface.Utils.spawnMte(Text);
        mte.label = this;
    }
}
