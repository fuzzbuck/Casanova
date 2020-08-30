using System;
using System.Runtime.InteropServices;
using Godot;
using Godot.Collections;
using static Casanova.core.Vars.PlayerCamera;

namespace Casanova.core.main.units
{
	public class Camera : Camera2D
	{
		
		Dictionary<int, Vector2> events = new Dictionary<int, Vector2>();
		private float last_drag_distance = 0;
		
		public override void _Ready()
		{
			Rotating = rotates_with_player;

			if (Vars.PersistentData.isMobile)
			{
				Zoom = new Vector2(min_zoom_distance * mobile_zoom_offset_multiplier,
					min_zoom_distance * mobile_zoom_offset_multiplier);
			}
			else
			{
				Zoom = new Vector2(min_zoom_distance, min_zoom_distance);
			}
		}
		
		public override void _Input(InputEvent @event){
			if (@event is InputEventMouseButton){
				InputEventMouseButton emb = (InputEventMouseButton) @event;
				
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

			if (@event is InputEventScreenTouch)
			{
				InputEventScreenTouch ev = (InputEventScreenTouch) @event;
				if (@event.IsPressed())
				{
					events[ev.Index] = ev.Position;
				}
				else
				{
					events.Remove(ev.Index);
				}
			}

			if (@event is InputEventScreenDrag)
			{
				InputEventScreenDrag drag = (InputEventScreenDrag) @event;
				MoveLocalX(-drag.Relative.x * drag_sensitivity);
				MoveLocalY(-drag.Relative.y * drag_sensitivity);

				events[drag.Index] = drag.Position;
				if (events.Count == 2)
				{
					var drag_distance = events[0].DistanceTo(events[1]);
					if (Mathf.Abs(drag_distance - last_drag_distance) > zoom_sensitivity)
					{
						var new_zoom = drag_distance < last_drag_distance ? 1 + zoom_speed : 1 - zoom_speed;
						new_zoom = Mathf.Clamp(Zoom.x * new_zoom, min_zoom_distance * mobile_zoom_offset_multiplier, max_zoom_distance * mobile_zoom_offset_multiplier);
						Zoom = Vector2.One * new_zoom;
						last_drag_distance = drag_distance;
					}
				}
			}

			// todo: implement zooming/pinching & dragging movement for mobile
		}
	}
}
