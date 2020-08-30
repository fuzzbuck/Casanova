using Casanova.core.main.units;
using Casanova.core.net;
using Casanova.core.net.types;
using Godot;
using Camera = Casanova.core.main.units.Camera;

namespace Casanova.core.main
{
    public class PlayerController : Node
    {
        
        public static Unit localUnit;
        public static Player localPlayer;

        public override void _Process(float delta)
        {
            ThreadManager.UpdateMain();

            if (!Vars.PersistentData.isMobile)
            {
                ProcessMovement();
            }
            else
            {
                ProcessMobileMovement();
            }
        }

        public void ProcessMovement()
        {
            if (localUnit != null)
            {
                var axis = new Vector2();
                axis.x = Input.GetActionStrength("right") - Input.GetActionStrength("left");
                axis.y = Input.GetActionStrength("down") - Input.GetActionStrength("up");
                localUnit.Axis = axis;
            }
        }

        public void ProcessMobileMovement()
        {
            if (localUnit != null && Camera.instance != null)
            {
                var p1 = localUnit.InWorldPosition;
                var p2 = Camera.instance.GlobalPosition;

                if (p1.DistanceTo(p2) > Vars.PlayerCamera.mobile_cam_distance_treshold)
                {
                    localUnit.Axis = p1.DirectionTo(p2);
                }
                else
                {
                    localUnit.Axis = Vector2.Zero;
                }
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            if (localUnit != null && !localPlayer.isLocal)
            {
                Packets.ClientHandle.Send.PlayerMovement(localUnit.InWorldPosition, localUnit.Axis, localUnit.Speed, localUnit.kinematicBody.Rotation);
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventKey eventKey)
            {
                if (eventKey.Pressed)
                {
                    if (eventKey.Scancode == (int) KeyList.F11)
                        OS.WindowFullscreen = !OS.WindowFullscreen;
                }
            }
        }
    }
}