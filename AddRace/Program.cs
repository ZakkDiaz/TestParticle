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

            octree.AddMany(new List<Particle>() {
                new Particle(new Point3D(49, 49, 49)),
                new Particle(new Point3D(49, 49, 51)),
                new Particle(new Point3D(49, 51, 49)),
                new Particle(new Point3D(49, 51, 51)),
                new Particle(new Point3D(51, 49, 49)),
                new Particle(new Point3D(51, 49, 51)),
                new Particle(new Point3D(51, 51, 49)),
                new Particle(new Point3D(51, 51, 51)),
                new Particle(new Point3D(51, 49, 49)),
                new Particle(new Point3D(49, 49, 51)),
            });
        }
    }
}
