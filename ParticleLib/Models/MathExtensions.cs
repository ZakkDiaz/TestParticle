using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ParticleLib.Models
{
    public static class MathExtensions
    {
        public static float AngleFor(Vector3 from, Vector3 to)
        {
            return Vector3.Angle(from, to);
        }
        public static float AngleFor(Vector2 from, Vector2 to)
        {
            return Vector2.Angle(from, to);
        }
        //public static float AngleFor(float x, float y, float z)
        //{
        //    return (float)(Math.Atan2(y, x));
        //}
    }
}
