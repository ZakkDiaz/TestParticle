using ParticleLib.Models.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UnityEngine;

namespace ParticleLib.Models
{
    public class ParticleEmitter
    {
        public Vector3 location { get; set; }
        int defaultLifespan = 50;

        public bool isEvap = false;
        public bool isSeek = true;

        private static object particleLockObj = new object();
        private static List<BaseEntity<ITimesteppableLocationEntity>> particles = new List<BaseEntity<ITimesteppableLocationEntity>>();

        public static List<BaseEntity<ITimesteppableLocationEntity>> Particles { get { lock (particleLockObj) return new List<BaseEntity<ITimesteppableLocationEntity>>(particles); } }

        public ParticleEntity EmitParticle(ref ParticleSpace3D<BaseEntity<ITimesteppableLocationEntity>> pspace, Vector3 relativePoint, Vector3 rotation, Vector3 velocity, Bounds BOUNDS, int split = 0, bool isChained = false, float stepSize = 10f, float particleSize = 1)
        {
            if (relativePoint.x > BOUNDS.min.x && relativePoint.x < BOUNDS.max.x && relativePoint.y > BOUNDS.min.y && relativePoint.y < BOUNDS.max.y && relativePoint.z > BOUNDS.min.z && relativePoint.z < BOUNDS.max.z)
            {
                var spawnLoc = isChained ? relativePoint : location;

                var newParticle = new ParticleEntity();
                newParticle.ParticleInit(stepSize, particleSize, spawnLoc.x, spawnLoc.y, ThreadSafeRandom.Next_s() * defaultLifespan, rotation.x, rotation.y, isEvap, isSeek, split, velocity.x, velocity.y);
                newParticle.AddForce(ThreadSafeRandom.Next_a(), 1f);
                var be = new BaseEntity<ITimesteppableLocationEntity>(newParticle);
                newParticle.parentRef = be;
                pspace.AddParticle(be);
                particles.Add(be);
                return newParticle;
            }
            return null;
        }

        public static void AccelToPoint(PointF fromLocation, PointF toLocation, ParticleEntity newParticle)
        {
            newParticle.AccelToPoint((fromLocation.X, fromLocation.Y), (toLocation.X, toLocation.Y));
        }

        public void SetLocation(float x, float y, float z)
        {
            location = new Vector3(x, y, z);
        }
    }
}
