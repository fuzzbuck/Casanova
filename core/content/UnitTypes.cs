using Casanova.core.types;

namespace Casanova.core.content
{
    public class UnitTypes
    {
        public static UnitType groundUnit, airUnit;
        
        public static void Init()
        {
            groundUnit = new UnitType
            {
                Name = "GroundUnit",
                Description = "A Default unit used for testing.",
                MaxSpeed = 9f,
                RotationSpeed = 4f,
                Health = 100f,
                MovementType = Enums.MovementType.Ground
            };
            airUnit = new UnitType
            {
                Name = "AirUnit",
                Description = "It flies!",
                MaxSpeed = 180f,
                Acceleration = 700f,
                RotationSpeed = 3.5f,
                Deceleration = 400f,
                Health = 50f,
                MovementType = Enums.MovementType.Air
            };
        }
    }
}
