using Casanova.core.main.units;
using Casanova.core.net;
using Casanova.core.net.server;
using Casanova.core.net.types;
using Casanova.ui;
using Godot;
using Camera = Casanova.core.main.units.Camera;

namespace Casanova.core.main
{
    public class PlayerController : Node
    {
        
        public static Unit localUnit;
        public static Player localPlayer;

        public static Node focus;
        public static Vector2 axis;

        public override void _Process(float delta)
        {
            if (!Vars.PersistentData.isMobile)
            {
                ProcessMovement();
            }
            else
            {
                ProcessMobileMovement();
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            if (localUnit != null && !Server.IsHosting)
            {
                Packets.ClientHandle.Send.PlayerMovement(localUnit.InWorldPosition, localUnit.Axis, localUnit.Speed, localUnit.kinematicBody.Rotation);
            }
        }

        public void ProcessMovement()
        {
            if (localUnit != null && focus == null)
            {
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

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey eventKey)
            {
                axis.x = Input.GetActionStrength("right") - Input.GetActionStrength("left");
                axis.y = Input.GetActionStrength("down") - Input.GetActionStrength("up");
                
                if (eventKey.Pressed)
                {
                    if (eventKey.Scancode == (int) KeyList.F11)
                        OS.WindowFullscreen = !OS.WindowFullscreen;
                    if (eventKey.Scancode == (int) KeyList.Escape)
                    {
                        if(Vars.CurrentState != Vars.State.Menu)
                            Interface.Utils.SpawnOverlayFragment("EscOverlay");
                    }
                }
            }
        }
    }
}