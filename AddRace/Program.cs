using OctreeEngine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace AddRace
{
    class Program
    {
        static OctreeEngine.Octree octree;
        static void Main(string[] args)
        {
            octree = new OctreeEngine.Octree(new OctreeEngine.Point3D(0, 0, 0), new OctreeEngine.Point3D(100, 100, 100));

            octree.Start();
            Random r = new Random(0);

            for (var i = 0; i < 10; i++)
            {
                octree.AddMany(new List<Particle>() {
                    new Particle(new Point3D((float)(r.NextDouble() * 5) + 45, (float)(r.NextDouble() * 5) + 45, (float)(r.NextDouble() * 5) + 45)),
                    new Particle(new Point3D((float)(r.NextDouble() * 5) + 45, (float)(r.NextDouble() * 5) + 45, (float)(r.NextDouble() * 5) + 55)),
                    new Particle(new Point3D((float)(r.NextDouble() * 5) + 45, (float)(r.NextDouble() * 5) + 55, (float)(r.NextDouble() * 5) + 45)),
                    new Particle(new Point3D((float)(r.NextDouble() * 5) + 45, (float)(r.NextDouble() * 5) + 55, (float)(r.NextDouble() * 5) + 55)),
                    new Particle(new Point3D((float)(r.NextDouble() * 5) + 55, (float)(r.NextDouble() * 5) + 45, (float)(r.NextDouble() * 5) + 45)),
                    new Particle(new Point3D((float)(r.NextDouble() * 5) + 55, (float)(r.NextDouble() * 5) + 45, (float)(r.NextDouble() * 5) + 55)),
                    new Particle(new Point3D((float)(r.NextDouble() * 5) + 55, (float)(r.NextDouble() * 5) + 55, (float)(r.NextDouble() * 5) + 45)),
                    new Particle(new Point3D((float)(r.NextDouble() * 5) + 55, (float)(r.NextDouble() * 5) + 55, (float)(r.NextDouble() * 5) + 55)),
                });
            }
        }
    }
}
