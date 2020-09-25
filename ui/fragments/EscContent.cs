using Casanova.core;
using Casanova.core.main.world;
using Casanova.ui.elements;
using Godot;
using Godot.Collections;

namespace Casanova.ui.fragments
{
    public class EscContent : MarginContainer
    {
        public override void _Ready()
        {
            var vbox = GetNode("VBox");
            var hbox = vbox.GetNode("HBox");

            var all = vbox.GetChildren();
            foreach (var c in hbox.GetChildren())
                all.Add(c);

            foreach (Node node in all)
                if (node is FillingButton button)
                    button.Connect("pressed", this, "_onButtonPressed", new Array {button.Name});
        }

        public void _onButtonPressed(string name)
        {
            GD.Print(name);
            switch (name)
            {
                case "Exit":
                    Vars.Unload();
                    break;
                case "Leave":
                    if (Vars.CurrentState != Vars.State.Menu)
                        NetworkManager.DisconnectToMenu();
                    break;
            }
        }
    }
}