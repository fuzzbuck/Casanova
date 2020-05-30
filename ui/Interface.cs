using Godot;
using Godot.Collections;

namespace Casanova.ui
{
	public class Interface : Node
	{
		public static Array<Button> buttonGroup = new Array<Button>();
		public static Array<Label> labelGroup = new Array<Label>();
		public static Array<AnimationPlayer> cardAnimationGroup = new Array<AnimationPlayer>();
		public static int currentSelected = -1; // current button/category selected   -1 = none,  0 = play, 1 = settings, 2 = about, 3 = exit (dont select)
		
		public Interface()
		{
			
		}

		public static class Cards
		{
			public static void open()
			{
				for (var i = 0; i < cardAnimationGroup.Count; i++)
				{
					cardAnimationGroup[i].Play("enter");
				}
			}

			public static void close()
			{
				for (var i = 0; i < cardAnimationGroup.Count; i++)
				{
					if(cardAnimationGroup[i].AssignedAnimation != "exit") 
						cardAnimationGroup[i].Play("exit");
				}
			}
		}

		public static class MainMenu
		{
			public static void closeAll()
			{
				Cards.close();
			}
		}
	}
}
