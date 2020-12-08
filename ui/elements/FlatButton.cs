using Casanova.core;
using Godot;
using Microsoft.SqlServer.Server;

namespace Casanova.ui.elements
{
    public class FlatButton : Button
    {
        public string content;
        public override void _Ready()
        {
            content = Text.ToLower(); // ok then
            Text = content;
            RectScale = new Vector2(1, 0.7f);
            Connect("mouse_entered", this, "_onMouseEnter");
            Connect("mouse_exited", this, "_onMouseExit");
        }

        public void _onMouseEnter()
        {
            Text = $"- {content} -";
        }
        
        public void _onMouseExit()
        {
            Text = content;
        }
    }
}
