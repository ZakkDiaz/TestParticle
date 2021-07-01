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
        private object particleLock = new object();
        private PointOctree<ParticleEntity> particles;
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
            lock (particleLock)
            {
                particles.DrawAllBounds(); // Draw node boundaries
            }
        }

        public List<ParticleEntity> GetParticles() {
            lock (particleLock)
                return particles.GetAll().ToList(); 
        }

        public void AddParticle(ParticleEntity particle)
        {
            lock (particleLock)
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
            //var items = new List<ParticleEntity>();

            lock (particleLock)
            {
                //p.Invoke(particles.GetNearby(location, 250).ToList());
                p.Invoke(particles.GetAll().ToList());
            }
        }

        internal void Remove(ParticleEntity p)
        {
            lock (particleLock)
                particles.Remove(p);
        }

        internal void Add(ParticleEntity p)
        {
            lock (particleLock)
                particles.Add(p, p.Location);
        }

        internal void Move(ParticleEntity p)
        {
            lock (particleLock) {  
                particles.Remove(p);
                particles.Add(p, p.Location);
            }
        }
    }
}
