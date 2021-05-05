using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OctreeEngine
{
    public class OctreeCellCollection : AAABBB
    {
        private static int _particleMaxCount = 10;
        public List<Particle> particles = new List<Particle>();

        public bool _overflow = false;
        public bool Overflow => (_overflow || (_overflow = particles.Count() >= _particleMaxCount));
        public ulong Location { get; }
        public OctreeCellCollection(Point3D from, Point3D to, ulong location) : base(from, to)
        {
            Location = location;
        }

        internal List<Particle> Flush()
        {
            var ret = particles.ToList();
            particles.Clear();
            return ret;
        }

        internal IEnumerable<Tuple<ulong, byte, Particle>> AddAll(List<Particle> lists)
        {
            if(!Overflow)
                particles.AddRange(lists);
            else
            {
                return FlowParticles(lists);
            }
            return new List<Tuple<ulong, byte, Particle>>();
        }

        private IEnumerable<Tuple<ulong, byte, Particle>> FlowParticles(IEnumerable<Particle> particle)
        {
            var depth = Helpers.GetDepth(Location);
            var toFlow = particle.Select((p) => GenerateFlow(p, depth));
            return toFlow;
        }

        private Tuple<ulong, byte, Particle> GenerateFlow(Particle particle, int depth)
        {
            var pLoc = particle.Location;
            var half = (this.To - this.From) / 2;
            var center = this.From + half;
            var quad = GetQuad(pLoc, center);
            //var location = parentNode.LocCode | (quad << depth);
            var location = (Location << 4) | (quad);
            return new Tuple<ulong, byte, Particle>(location, (byte)quad, particle);
        }


        //removed math.abs from center...
        public static ulong GetQuad(Point3D pLoc, Point3D center) =>
                (ulong)(
                    (pLoc.X > center.X ? 0b1001 : 0b1000)
                    |
                    (pLoc.Y > center.Y ? 0b1010 : 0b1000)
                    |
                    (pLoc.Z > center.Z ? 0b1100 : 0b1000)
                );
    }
}
