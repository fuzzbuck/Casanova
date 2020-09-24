using System;
using System.Collections.Generic;
using Casanova.core.content;
using Casanova.core.types;
using Casanova.core.types.bodies;
using Godot;
using Godot.Collections;

namespace Casanova.core.main.units
{
    public static class Utils
    {
        public static Body CreateBody(UnitType type)
        {
            string bodyType = String.Empty;
            switch (type.MovementType)
            {
                case Enums.MovementType.Air:
                    bodyType = "Air";
                    break;
                case Enums.MovementType.Ground:
                    bodyType = "Body";
                    break;
            }
            
            Body body = (Body) ResourceLoader.Load<PackedScene>(Vars.path_types + $"/bodies/{bodyType}.tscn").Instance();
            body.Init(type.MaxSpeed, type.RotationSpeed, type.Acceleration, type.Deceleration);
            
            return body;
        }

        public static SkidMark CreateSkidMark()
        {
            SkidMark skid = (SkidMark) ResourceLoader.Load<PackedScene>(Vars.path_types + $"/SkidMark.tscn").Instance();

            return skid;
        }
    }
    
    public class Unit : Node2D
    {
        public UnitType Type;
        public Node2D Content;
        public Body Body;
        private Array<SkidMark> SkidMarks = new Array<SkidMark>();
        public override void _Ready()
        {
            Content = GetNode<Node2D>("Content");
            Body = Utils.CreateBody(Type);

            if (Type.SkidMarks != null)
            {
                foreach (KeyValuePair<Vector2, float> kvp in Type.SkidMarks)
                {
                    var pos = kvp.Key;
                    var size = kvp.Value;
                    
                    var skid = Utils.CreateSkidMark();
                    
                    var grad = new Gradient();

                    var fade = Type.SkidColor;
                    fade.a = 0;
                    
                    grad.SetColor(1, Type.SkidColor);
                    grad.SetColor(0, fade);
                    
                    
                    skid.Width = size;
                    skid.Gradient = grad;
                    skid.Pos = pos;
                    skid.Modulate = new Color(1, 1, 1, Type.SkidOpacity / 100f);

                    SkidMarks.Add(skid);
                    GD.Print("Added skid marks");
                    
                    Content.AddChild(skid);
                }
            }
            
            AddChild(Body);
        }
        public override void _PhysicsProcess(float delta)
        {
            if (SkidMarks != null)
            {
                foreach (SkidMark skid in SkidMarks)
                {
                    skid.AddPoint(Body.InWorldPosition + skid.Pos.Rotated(Body.Rotation));
                    if (skid.Points.Length > Type.SkidLength)
                        skid.RemovePoint(0);
                }
            }
        }
    }
}
