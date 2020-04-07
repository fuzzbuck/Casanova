using Casanova.core;
using Godot;
using Godot.Collections;

namespace Casanova.ui.fragments
{
	public class ButtonsContainer : VBoxContainer
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
		
			for (var i = 0; i < buttons.Count; i++)
			{
				Button button = buttons[i];
				Vars.uiHandler.buttonGroup.Add(button);
			}
		}
	
	}
}
