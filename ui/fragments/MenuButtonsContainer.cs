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
				
				button.Connect("pressed", this, "_onButtonPress", new Array(new[] {i}));
				Interface.buttonGroup.Add(button);
			}
		}

		public void _onButtonPress(int index)
		{
			if (index == 3) // 3 = exit button index
			{
				// todo: save important data, do pre-exit things
				GetTree().Quit();
				return;
			}
			if (Interface.currentSelected == index) // we are selected, ignore
			{
				// Interface.currentSelected = -1;
				// Interface.MainMenu.closeAll();
				return;
			}

			if (Interface.currentSelected == -1) // if nothing is displayed
			{
				// todo: open the selected index
				Interface.currentSelected = index;
				return;
			}

			if (Interface.currentSelected != -1) // if something is displayed, but we want to display something else
			{
				Interface.MainMenu.closeAll();
				Interface.currentSelected = index;
			}
		}
	
	}
}
