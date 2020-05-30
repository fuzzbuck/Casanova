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
				VBoxContainer infoBox = cardPanel.GetNode<Panel>("Info").GetNode<VBoxContainer>("VBoxContainer");
				
				// add 2 first children, which are usually Title & Description
				Interface.labelGroup.Add(infoBox.GetChildren()[0] as Label);
				Interface.labelGroup.Add(infoBox.GetChildren()[1] as Label);
				
				// add the animation player for this card panel
				Interface.cardAnimationGroup.Add(cardPanel.GetNode<AnimationPlayer>("AnimationPlayer"));
			}
			
			Vars.load();
		}

		public void _cardHover(Array args)
		{
			GD.Print(args);
			Panel cardPanel = args[0] as Panel;
			
			// find the animation for this card panel
			AnimationPlayer animation = null;
			for (var i = 0; i < Interface.cardAnimationGroup.Count; i++)
			{
				AnimationPlayer anim = Interface.cardAnimationGroup[i];
				if (anim.GetParent() == cardPanel)
				{
					animation = anim;
				}
			}
			
			animation.Play("hover");
		}

		private void _on_Detector_mouse_entered(int index)
		{
			// play hover animation
			AnimationPlayer animation = Interface.cardAnimationGroup[index];
			animation.Stop();
			animation.Play("hover");
		}
		
		private void _on_Detector_mouse_exited(int index)
		{
			AnimationPlayer animation = Interface.cardAnimationGroup[index];
			animation.Stop();
			animation.Play("unhover");
		}

	}
}
