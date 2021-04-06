using System;
using System.Collections.Generic;
using System.Text;

namespace ParticleLib.Models._3D
{
    public class Point3D
    {
        public Point3D()
        {

        }

        public Point3D(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Point3D operator -(Point3D to, Point3D from)
        {
            return new Point3D(to.X - from.X, to.Y - from.Y, to.Z - from.Z);
        }

        public static Point3D operator /(Point3D point, float scalar)
        {
            return new Point3D(point.X / scalar, point.Y / scalar, point.Z / scalar);
        }

        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;
        public float Z { get; set; } = 0;
    }
}
