using ParticleLib.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ParticleLib.Models
{
    public partial class ParticleSpace3D
    {
        private UnityEngine.Vector3 from;
        private UnityEngine.Vector3 to;
        PointOctree<ParticleEntity> particles;
        //QuadTreeRect<T> particles;

        public ParticleSpace3D(Vector3 _from, Vector3 _to)
        {
            from = _from;
            to = _to;
            var diff = (to - from);
            var center = (to + from) / 2;
            particles = new PointOctree<ParticleEntity>(15, Vector3.zero, .001f);
            //particles = new QuadTreeRect<T>(from.X, from.Y, to.X - from.X, to.Y - from.Y);
        }

        public void DrawAll()
        {
            particles.DrawAllBounds(); // Draw node boundaries
            particles.DrawAllObjects(); // Mark object positions
        }

        public void AddParticle(ParticleEntity particle)
        {

            particles.Add(particle, particle.Location);
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
            var items = new List<ParticleEntity>();
            particles.GetNearby(new Ray(location, Vector3.zero), 100);
            p.Invoke(items);
        }

        internal void Remove(ParticleEntity p)
        {
            particles.Remove(p);
        }

        internal void Add(ParticleEntity p)
        {
            particles.Add(p, p.Location);
        }

        internal void Move(ParticleEntity p)
        {
            particles.Remove(p);
            particles.Add(p, p.Location);
        }
    }
}
