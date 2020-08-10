using Godot;
using static Casanova.core.Vars.GlobalSettings.Camera;

namespace Casanova.core.main.units.Player
{
    public class Camera : Camera2D
    {
        private static Tween tween;
	
        public override void _Ready()
        {
            tween = GetNode<Tween>("Tween");
            Rotating = rotates_with_player;
		
            GD.Print(Zoom.Length());
        }

        public override void _UnhandledInput(InputEvent @event){
            if (@event is InputEventMouseButton){
                InputEventMouseButton emb = (InputEventMouseButton)@event;
                if (Input.IsActionPressed("control"))
                {
                    if (tween.IsActive())
                        tween.StopAll();
				
                    if (emb.ButtonIndex == (int)ButtonList.WheelUp)
                    {
                        tween.InterpolateProperty(this, "Zoom", Zoom,
                            Zoom + new Vector2(smoothness,
                                smoothness), tween_duration, transition_type, Tween.EaseType.In);
                    }
                    if (emb.ButtonIndex == (int)ButtonList.WheelDown){
                        tween.InterpolateProperty(this, "Zoom", Zoom,
                            Zoom - new Vector2(smoothness,
                                smoothness), tween_duration, transition_type, Tween.EaseType.Out);
                    }
				
                    GD.Print("started");
                    tween.Start();
                }
            }
		
            // todo: implement zooming/pinching & dragging movement for mobile
        }
    }
}