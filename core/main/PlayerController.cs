using Casanova.core.main.units;
using Casanova.core.main.units.Player;
using Casanova.core.net;
using Casanova.core.net.types;
using Godot;

namespace Casanova.core.main
{
    public class PlayerController : Node
    {
        
        public static PlayerUnit localUnit;
        public static Player localPlayer;

        public override void _Process(float delta)
        {
            if (localUnit != null)
            {
                var axis = new Vector2();
                axis.x = Input.GetActionStrength("right") - Input.GetActionStrength("left");
                axis.y = Input.GetActionStrength("down") - Input.GetActionStrength("up");
                localUnit.Axis = axis;
            }
        }
        
        // 60 tps, great for servers :)
        public override void _PhysicsProcess(float delta)
        {
            if (localUnit != null)
            {
                Packets.ClientHandle.Send.PlayerMovement(localUnit.instance.Position, localUnit.Axis, localUnit.Speed, localUnit.instance.Rotation);
            }
        }


        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventKey eventKey)
                if (eventKey.Pressed)
                {
                    if (eventKey.Scancode == (int) KeyList.F11)
                        OS.WindowFullscreen = !OS.WindowFullscreen;
                }
        }
        
    }
}