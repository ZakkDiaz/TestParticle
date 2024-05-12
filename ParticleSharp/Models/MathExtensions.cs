using System.Numerics;

namespace ParticleSharp.Models
{
    public static class MathExtensions
    {
        public static Vector3 AngleFor(Vector3 from, Vector3 to)
        {
            var x = (float)Math.Atan2(to.X, from.X);
            var y = (float)Math.Atan2(to.Y, from.Y);
            var z = (float)Math.Atan2(to.Z, from.Z);
            return new Vector3(x, y, z);
            //return Vector3.Angle(from, to);
        }
        //public static float AngleFor(Vector2 from, Vector2 to)
        //{
        //    return Vector2.Angle(from, to);
        //}
    }
}
