using Godot;
using System;

public class CardPanel : Panel
{
    private Panel infoPanel;
    private TextureRect iconRect;
    public override void _Ready()
    {
        infoPanel = Get("Info") as Panel;
        iconRect = Get("Icon") as TextureRect;
    }

    public void _onMouseEnter()
    {
        
    }
}
