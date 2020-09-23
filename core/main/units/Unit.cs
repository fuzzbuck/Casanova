using System;
using Casanova.core.content;
using Casanova.core.types;
using Casanova.core.types.bodies;
using Godot;

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
    }
    
    public class Unit : Node2D
    {
        public UnitType Type;
        public Body Body;
        public override void _Ready()
        {
            Body = Utils.CreateBody(Type);
            AddChild(Body);
        }
    }
}
