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
        //private static List<BaseEntity<ITimesteppableLocationEntity>> particles = new List<BaseEntity<ITimesteppableLocationEntity>>();

        //public static List<BaseEntity<ITimesteppableLocationEntity>> Particles { get { lock (particleLockObj) return new List<BaseEntity<ITimesteppableLocationEntity>>(particles); } }

        public ParticleEntity EmitParticle(ref ParticleSpace3D pspace, Vector3 relativePoint, Vector3 rotation, Vector3 velocity, Bounds BOUNDS, int split = 0, bool isChained = false, float stepSize = 10f, float particleSize = 1)
        {
            if (relativePoint.x > BOUNDS.min.x && relativePoint.x < BOUNDS.max.x && relativePoint.y > BOUNDS.min.y && relativePoint.y < BOUNDS.max.y && relativePoint.z > BOUNDS.min.z && relativePoint.z < BOUNDS.max.z)
            {
                var newParticle = new ParticleEntity();
                newParticle.ParticleInit(stepSize, particleSize, relativePoint.x, relativePoint.y, relativePoint.z, ThreadSafeRandom.Next_s() * defaultLifespan, rotation.x, rotation.y, rotation.z, isEvap, isSeek, split, velocity.x, velocity.y, velocity.z);
                newParticle.AddForce(ThreadSafeRandom.Next_v3(), 1f);
                //var be = new BaseEntity<ITimesteppableLocationEntity>(newParticle);
                //newParticle.parentRef = be;                pspace.AddParticle(newParticle);
                pspace.Add(newParticle);
                return newParticle;
            }
            return null;
        }

        public static void AccelToPoint(Vector3 fromLocation, Vector3 toLocation, ParticleEntity newParticle)
        {
            newParticle.AccelToPoint(fromLocation, toLocation);
        }

        public void SetLocation(float x, float y, float z)
        {
            location = new Vector3(x, y, z);
        }
    }
}
