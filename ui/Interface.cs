using Godot;
using Godot.Collections;

namespace Casanova.ui
{
	public class Interface : Node
	{
		public Array<Button> buttonGroup = new Array<Button>();
		public Array<Label> labelGroup = new Array<Label>();
		public Array<AnimationPlayer> cardAnimationGroup = new Array<AnimationPlayer>();
		public Interface()
		{
			
		}
	}
}
