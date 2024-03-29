﻿using System.Security.Cryptography;
using Casanova.core.main.units;
using Casanova.core.main.world;
using Casanova.core.net;
using Casanova.core.net.server;
using Casanova.core.net.types;
using Casanova.core.utils;
using Casanova.ui;
using Godot;
using Camera = Casanova.core.main.units.Camera;
using Client = Casanova.core.net.client.Client;

namespace Casanova.core.main
{
    public class PlayerController : Node
    {
        public static Unit LocalUnit;
        public static Player LocalPlayer;

        public static Camera LocalCamera;

        public static Node Focus;
        public static Vector2 Axis;
        public static void Init()
        {
            LocalCamera?.Dispose();
            LocalCamera = (Camera) References.main_camera.Instance();
        }

        public override void _Process(float delta)
        {
            if (LocalUnit?.Body != null)
            {
                if (!Vars.PersistentData.isMobile)
                    ProcessMovement();
                else
                    ProcessMobileMovement();
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            if (LocalUnit?.Body != null && Client.IsConnected)
                Packets.ClientHandle.Send.UnitMovement(LocalUnit.netId, LocalUnit.Body.InWorldPosition,
                    LocalUnit.Body.Axis, LocalUnit.Body.Vel, LocalUnit.Body.Transform.Rotation);
        }

        public void ProcessMovement()
        {
            if (Focus == null) LocalUnit.Body.Axis = Axis;
            if(!Vars.PersistentData.isMobile)
                LocalCamera.Position = LocalUnit.Body.Position;
        }

        public void ProcessMobileMovement()
        {
            if (Camera.instance != null)
            {
                // todo: check if player flew too far away and move the camera there

                var p1 = LocalUnit.Body.InWorldPosition;
                var p2 = Camera.instance.GlobalPosition;

                if (p1.DistanceTo(p2) > Vars.PlayerCamera.mobile_cam_distance_treshold)
                    LocalUnit.Body.Axis = p1.DirectionTo(p2);
                else
                    LocalUnit.Body.Axis = Vector2.Zero;
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

        
        public override void _UnhandledInput(InputEvent @event) //todo :remove this and replace with better thing
        {
            if (@event is InputEventMouseButton eventMouse && @event.IsPressed())
            {
                if (eventMouse.ButtonIndex == (int) ButtonList.Left)
                {
                    
                }
            }
        }

        public static void TakeOwnership(Unit unit)
        {
            ThreadManager.ExecuteOnMainThread(() =>
            {
                if(LocalCamera.GetParent() != null)
                    LocalCamera.GetParent().RemoveChild(LocalCamera);   // wow godot very nice

                if(!Vars.PersistentData.isMobile)
                    unit.AddChild(LocalCamera);
                else
                {
                    unit.AddChild(LocalCamera);
                }
                
                LocalUnit = unit;
                
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    LocalCamera.GlobalPosition = unit.GlobalPosition;
                    LocalCamera.Offset = Vector2.Zero;
                    LocalCamera.Position = Vector2.Zero;
                });
            });
        }
        
        public static void VoidOwnership()
        {
            if (LocalUnit != null)
            {
                if(LocalCamera.GetParent() != null)
                    LocalCamera.GetParent().RemoveChild(LocalCamera);

                LocalUnit = null;
            }
        }
    }
}