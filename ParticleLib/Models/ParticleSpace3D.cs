using ParticleLib.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ParticleLib.Models
{
    public partial class ParticleSpace3D<T> where T : BaseEntity<ITimesteppableLocationEntity>
    {
        private UnityEngine.Vector3 from;
        private UnityEngine.Vector3 to;
        PointOctree<T> particles;
        //QuadTreeRect<T> particles;

        public ParticleSpace3D(Vector3 _from, Vector3 _to)
        {
            from = _from;
            to = _to;
            var diff = (to - from);
            var center = (to + from) / 2;
            particles = new PointOctree<T>(15, Vector3.zero, .001f);
            //particles = new QuadTreeRect<T>(from.X, from.Y, to.X - from.X, to.Y - from.Y);
        }

        public void AddParticle(T particle)
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

        internal void ProcessEntityState(Vector3 location, Action<List<T>> p)
        {
            var items = new List<T>();
            particles.GetNearby(new Ray(location, Vector3.zero), 100);
            p.Invoke(items);
        }

        internal void Remove(T p)
        {
            particles.Remove(p);
        }

        internal void Add(T p)
        {
            particles.Add(p, p.Location);
        }

        internal void Move(T p)
        {
            particles.Remove(p);
            particles.Add(p, p.Location);
        }
    }
}
