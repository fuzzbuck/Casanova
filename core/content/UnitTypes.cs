using Casanova.core.types;

namespace Casanova.core.content
{
    public class UnitTypes
    {
        public static UnitType crimson, airUnit;
        
        public static void Init()
        {
            crimson = new UnitType
            {
                Name = "Crimson",
                Description = "Starting bot equipped with a high powered Projector.",
                MaxSpeed = 12f,
                RotationSpeed = 6f,
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
