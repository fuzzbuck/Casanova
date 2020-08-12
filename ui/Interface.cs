using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.InteropServices;
using Godot;
using Godot.Collections;

namespace Casanova.ui
{
	public class Interface : Node
	{
		public static Array<Button> ButtonGroup = new Array<Button>();
		public static Array<Label> LabelGroup = new Array<Label>();
		public static Array<AnimationPlayer> cardAnimationGroup = new Array<AnimationPlayer>();
		public static int CurrentSelected = -1; // current button/category selected   -1 = none,  0 = play, 1 = settings, 2 = about, 3 = exit (dont select)
		
		public Interface()
		{
			
		}

		public static class Cards
		{
			public static bool IsShown;
			public static void Open()
			{
				IsShown = true;
				GD.Print("opening cards");
				for (var i = 0; i < cardAnimationGroup.Count; i++)
				{
					GD.Print("opening card " + cardAnimationGroup[i].Name);
					cardAnimationGroup[i].Play("enter");
				}
			}

			public static void Close()
			{
				IsShown = false;
				for (var i = 0; i < cardAnimationGroup.Count; i++)
				{
					if(cardAnimationGroup[i].AssignedAnimation != "exit") 
						cardAnimationGroup[i].Play("exit");
				}
			}
		}

		public static class MainMenu
		{
			public static readonly System.Collections.Generic.Dictionary<int, Action> IndexBindings = new System.Collections.Generic.Dictionary<int, Action>
			{
				{
					0, Cards.Open
				},
				{
					1, () =>
					{
					}
				},
				{
					2, () =>
					{
					}
				}
				// todo: settings.open(), about.open()
			};
			public static void CloseAll()
			{
				Cards.Close();
			}
		}
	}
}
