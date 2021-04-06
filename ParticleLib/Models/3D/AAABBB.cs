using System;
using System.Collections.Generic;
using System.Text;

namespace ParticleLib.Models._3D
{
    public class AAABBB
    {
        internal Point3D _from { get; set; }
        internal Point3D _to { get; set; }

        public AAABBB(Point3D from, Point3D to)
        {
            _to = to;
            _from = from;
        }
    }
}
