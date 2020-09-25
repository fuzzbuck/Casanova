using Casanova.core.main.units;
using Casanova.core.net;
using Casanova.core.net.server;
using Casanova.core.net.types;
using Casanova.ui;
using Godot;
using Camera = Casanova.core.main.units.Camera;
using Client = Casanova.core.net.client.Client;

namespace Casanova.core.main
{
    public class PlayerController : Node
    {
        public static PlayerUnit LocalPlayerUnit;
        public static Player LocalPlayer;

        public static Node Focus;
        public static Vector2 Axis;

        public override void _Process(float delta)
        {
            if (LocalPlayerUnit?.Body != null)
            {
                if (!Vars.PersistentData.isMobile)
                    ProcessMovement();
                else
                    ProcessMobileMovement();
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            if (LocalPlayerUnit != null && !Server.IsHosting && Client.isConnected)
                Packets.ClientHandle.Send.PlayerMovement(LocalPlayerUnit.Body.InWorldPosition,
                    LocalPlayerUnit.Body.Axis, LocalPlayerUnit.Body.Speed, LocalPlayerUnit.Rotation);
        }

        public void ProcessMovement()
        {
            if (Focus == null) LocalPlayerUnit.Body.Axis = Axis;
        }

        public void ProcessMobileMovement()
        {
            if (Camera.instance != null)
            {
                // todo: check if player flew too far away and move the camera there

                var p1 = LocalPlayerUnit.Body.InWorldPosition;
                var p2 = Camera.instance.GlobalPosition;

                if (p1.DistanceTo(p2) > Vars.PlayerCamera.mobile_cam_distance_treshold)
                    LocalPlayerUnit.Body.Axis = p1.DirectionTo(p2);
                else
                    LocalPlayerUnit.Body.Axis = Vector2.Zero;
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey eventKey)
            {
                Axis.x = Input.GetActionStrength("right") - Input.GetActionStrength("left");
                Axis.y = Input.GetActionStrength("down") - Input.GetActionStrength("up");

                if (eventKey.Pressed)
                {
                    if (eventKey.Scancode == (int) KeyList.F11)
                        OS.WindowFullscreen = !OS.WindowFullscreen;
                    if (eventKey.Scancode == (int) KeyList.Escape)
                        if (Vars.CurrentState != Vars.State.Menu)
                            Interface.Utils.SpawnOverlayFragment("EscOverlay");
                }
            }
        }
    }
}