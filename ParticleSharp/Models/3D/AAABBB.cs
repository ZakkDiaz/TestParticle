using System;
using System.Collections.Generic;
using System.Text;

namespace ParticleLib.Models._3D
{
    public class AAABBB
    {
        public Point3D From { get; set; }
        public Point3D To { get; set; }

        public AAABBB(Point3D from, Point3D to)
        {
            To = to;
            From = from;
        }
    }
}
