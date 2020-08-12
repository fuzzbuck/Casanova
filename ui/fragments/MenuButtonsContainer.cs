using System;
using Casanova.core;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;
using Environment = Godot.Environment;

namespace Casanova.ui.fragments
{
	public class MenuButtonsContainer : VBoxContainer
	{
		private VBoxContainer _topButtons;
		private VBoxContainer _botButtons;
		
		private Array<Button> buttons = new Array<Button>();

		public override void _Ready()
		{
			_topButtons = GetNode<VBoxContainer>("TopButtons");
			_botButtons = GetNode<VBoxContainer>("BottomButtons");

			buttons.Add(_topButtons.GetNode("Play") as Button);
			buttons.Add(_topButtons.GetNode("Settings") as Button);
			
			buttons.Add(_botButtons.GetNode("About") as Button);
			buttons.Add(_botButtons.GetNode("Exit") as Button);

			for (int i = 0; i < buttons.Count; i++)
			{
				Button button = buttons[i];
				
				button.Connect("button_down", this, "_onButtonPress", new Array(new[] {i}));
				Interface.ButtonGroup.Add(button);
			}
		}

		public void _onButtonPress(int index)
		{
			GD.Print(index);
			if (index == 3) // 3 = exit button index
			{
				// todo: save important data, do pre-exit things
				GetTree().Quit();
				return;
			}
			if (Interface.CurrentSelected == index) // cards selected, close them
			{
				Interface.CurrentSelected = -1;
				GD.Print("Closing cards");
				Interface.Cards.Close();
				return;
			}

			if (Interface.CurrentSelected == -1) // if nothing is displayed
			{
				Interface.MainMenu.IndexBindings[index].DynamicInvoke();
				Interface.CurrentSelected = index;
				return;
			}

			if (Interface.CurrentSelected != -1) // if something is displayed, but we want to display something else
			{
				Interface.MainMenu.CloseAll();
				Interface.MainMenu.IndexBindings[index].DynamicInvoke();
				Interface.CurrentSelected = index;
			}
		}
	
	}
}
