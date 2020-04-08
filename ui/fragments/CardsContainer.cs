using System.Linq;
using Casanova.core;
using Godot;
using Godot.Collections;

namespace Casanova.ui.fragments
{
	public class CardsContainer : MarginContainer
	{
		private HBoxContainer container;
		public override void _Ready()
		{
			container = GetNode<HBoxContainer>("Container");

			for (int i = 0; i < container.GetChildCount(); i++)
			{
				Panel cardPanel = container.GetChildren()[i] as Panel;
				//todo: Connect mouse_enter & mouse_exit events to cardPanel & animate
				
				VBoxContainer infoBox = cardPanel.GetNode<Panel>("Info").GetNode<VBoxContainer>("VBoxContainer");
				
				GD.Print(infoBox.GetChildren()[0]);
				GD.Print(infoBox.GetChildren()[1]);
				// add 2 first children, which are usually Title & Description
				Vars.uiHandler.labelGroup.Add(infoBox.GetChildren()[0] as Label);
				Vars.uiHandler.labelGroup.Add(infoBox.GetChildren()[1] as Label);
			}
			
			Vars.load();
		}

	}
}
