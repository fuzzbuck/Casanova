﻿using System;
using Godot;

namespace Casanova.core.utils
{
    // MathU - "Math Utils"
    public class MathU
    {
        // Loops the value t, so that it is never larger than length and never smaller than 0.
        public static float Repeat(float t, float length)
        {
            return Mathf.Clamp(t - Mathf.Floor(t / length) * length, 0.0f, length);
        }
        
        public static float DeltaAngle(float a1, float a2)
        {
            float delta = 180 - Repeat((a1 - a2), 360.0F);
            if (delta > 180.0F)
                delta -= 360.0F;

            return delta;
        }
    }
}