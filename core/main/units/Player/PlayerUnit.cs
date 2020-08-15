using Godot;

namespace Casanova.core.main.units.Player
{
	public class PlayerUnit : Unit
	{
		public int id;
		public PlayerUnit()
		{}

		/*
		public override Vector2 _GetInputAxis()
		{
			var axis = new Vector2();
			axis.x = Input.GetActionStrength("right") - Input.GetActionStrength("left");
			axis.y = Input.GetActionStrength("down") - Input.GetActionStrength("up");
			return axis;
		}
		*/
	}
}
