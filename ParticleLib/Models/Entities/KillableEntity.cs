using QuadTrees.QTreeRect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ParticleLib.Models.Entities
{
    public class KillableEntity : ITimesteppableLocationEntity
    {
        public List<DimensionProperty> dimensions;
        public RectangleF Rect => new RectangleF(dimensions[0].pos, dimensions[1].pos, 10, 10);
        public float mass = 10;
        public float deltaStep = .001f;
        void ITimesteppableEntity.ProcessTimestep(float diff, (float, float) focus, (int, int) BOUNDS)
        {
            foreach (var p in dimensions)
            {
                p.ProcessTimestep(deltaStep * diff, mass, BOUNDS);
            }
        }
    }
}
