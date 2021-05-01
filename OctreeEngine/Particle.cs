using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OctreeEngine
{
    public class Particle
    {
        public Point3D Location;
        public Particle(Point3D location)
        {
            Location = location;
        }
    }
}
