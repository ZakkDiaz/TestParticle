using ParticleLib.Models._3D;
using ParticleSharp.Models.Entities;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ParticleSharp.Models
{
    public partial class ParticleSpace3D
    {
        public Vector3 from;
        public Vector3 to;
        //private object particleLock = new object();
        private ConcurrentOctree particles;
        //QuadTreeRect<T> particles;

        public ParticleSpace3D(Vector3 _from, Vector3 _to)
        {
            from = _from;
            to = _to;
            var diff = to - from;
            var center = (to + from) / 2;
            particles = new ConcurrentOctree(new Point3D(), new Point3D(diff.X, diff.Y, diff.Z));

            //particles = new Octree(15, Vector3.zero, .001f);
            //particles = new QuadTreeRect<T>(from.X, from.Y, to.X - from.X, to.Y - from.Y);
        }

        //public void DrawAll()
        //{
        //    lock (particleLock)
        //    {
        //        particles.DrawBounds(); // Draw node boundaries
        //    }
        //}

        public List<ParticleEntity> GetParticles()
        {
            return particles.GetPointCloud().ToList();
        }

        public void AddParticle(ParticleEntity particle)
        {
                particles.Add(particle);
        }

        //public List<T> GetParticles()
        //{
        //    return particles.Add().ToList();
        //}
        //public Bounds[] GetRects()
        //{
        //    return particles.nodes.Select(n => n.Bounds).ToArray();
        //}

        internal void ProcessEntityState(Vector3 location, Action<List<ParticleEntity>> p)
        {
            //var items = new List<ParticleEntity>();

            p.Invoke(particles.GetPointCloud().ToList());
        }

        internal void Remove(ParticleEntity p)
        {
                particles.Remove(p);
        }

        internal void Add(ParticleEntity p)
        {
                particles.Add(p);
        }

        internal void Move(ParticleEntity p)
        {
                particles.Move(p);
            
        }
    }
}
