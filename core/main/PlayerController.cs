using System;
using Casanova.core.main.units;
using Casanova.core.net;
using Casanova.core.net.client;
using Casanova.core.net.server;
using Casanova.core.net.types;
using Godot;

namespace Casanova.core.main
{
    public class PlayerController : Node
    {
        
        public static Unit localUnit;
        public static Player localPlayer;

        public override void _Process(float delta)
        {
            ThreadManager.UpdateMain();
            
            if (localUnit != null)
            {
                var axis = new Vector2();
                axis.x = Input.GetActionStrength("right") - Input.GetActionStrength("left");
                axis.y = Input.GetActionStrength("down") - Input.GetActionStrength("up");
                localUnit.Axis = axis;
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            if (localUnit != null)
            {
                Packets.ClientHandle.Send.PlayerMovement(localUnit.kinematicBody.Position, localUnit.Axis, localUnit.Speed, localUnit.kinematicBody.Rotation);
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