using System.Linq;
using Godot;
using Godot.Collections;
using Array = System.Array;

namespace Casanova.@interface.fragments
{
	public class ButtonsContainer : VBoxContainer
	{
		private HBoxContainer _topButtons;
		private HBoxContainer _botButtons;
		
		private Array<Button> buttons = new Array<Button>();

		public override void _Ready()
		{
			_topButtons = GetNode("TopButtons") as HBoxContainer;
			_botButtons = GetNode("BottomButtons") as HBoxContainer;
			
			buttons.Add(_topButtons.Get("Play") as Button);
			buttons.Add(_topButtons.Get("Settings") as Button);
			
			buttons.Add(_botButtons.Get("About") as Button);
			buttons.Add(_botButtons.Get("Exit") as Button);
			
			for(int x=1; x < buttons.Count; x++)
			{
				Button button = buttons[x];
			}
		}
	}
}
