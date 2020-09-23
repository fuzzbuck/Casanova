﻿namespace Casanova.core.types
{
    public class Enums
    {
        public enum MovementType
        {
            Ground,
            Air
        }
    }
    public class UnitType
    {
        public string Name = "Attacker";
        public string Description = "Attacks.";
        
        public float Health;
        
        public float MaxSpeed;
        public float Acceleration;
        public float Deceleration;
        public float RotationSpeed;

        public Enums.MovementType MovementType = Enums.MovementType.Ground;
    }
}