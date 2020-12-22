using Casanova.core;
using Casanova.core.utils;
using Godot;
using Godot.Collections;

namespace Casanova.ui.fragments
{
    public class MenuButtonsContainer : VBoxContainer
    {
        private readonly Array<Godot.Button> buttons = new Array<Godot.Button>();

        public override void _Ready()
        {
            buttons.Add(GetNode("Play") as Godot.Button);
            buttons.Add(GetNode("Settings") as Godot.Button);

            buttons.Add(GetNode("About") as Godot.Button);
            buttons.Add(GetNode("Exit") as Godot.Button);

            for (var i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];

                button.Connect("button_down", this, "_onButtonPress", new Array(new[] {i}));
                Interface.ButtonGroup.Add(button);
            }
        }

        public void _onButtonPress(int index)
        {
            if (index == 3) // 3 = exit button index
            {
                Vars.Unload();
                return;
            }

            if (Interface.CurrentSelected == index) // cards selected, close them
            {
                Interface.CurrentSelected = -1;
                Interface.Cards.Close();
                return;
            }

            if (Interface.CurrentSelected == -1) // if nothing is displayed
            {
                Interface.MainMenu.IndexBindings[index].DynamicInvoke();
                Interface.CurrentSelected = index;
                return;
            }

            if (Interface.CurrentSelected != -1
            ) // if something is already displayed, but we want to display something else
            {
                Interface.MainMenu.CloseAll();
                Interface.MainMenu.IndexBindings[index].DynamicInvoke();
                Interface.CurrentSelected = index;
            }
        }
    }
}