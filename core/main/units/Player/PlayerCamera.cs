using Godot;
using static Casanova.core.Vars.PlayerCamera;

namespace Casanova.core.main.units.Player
{
	public class PlayerCamera : Camera2D
	{
		public override void _Ready()
		{
			Rotating = rotates_with_player;
		}
		
		public override void _Input(InputEvent @event){
			if (@event is InputEventMouseButton){
				InputEventMouseButton emb = (InputEventMouseButton)@event;
				
				// leave these brackets for future keybinds
				{
					if (emb.ButtonIndex == (int)ButtonList.WheelUp && Zoom.Length() > min_zoom_distance)
					{
						Zoom = Zoom - new Vector2(smoothness, smoothness);
					}
					if (emb.ButtonIndex == (int)ButtonList.WheelDown && Zoom.Length() < max_zoom_distance){
						Zoom = Zoom + new Vector2(smoothness, smoothness);
					}
				}
			}
		
			// todo: implement zooming/pinching & dragging movement for mobile
		}
	}
}
