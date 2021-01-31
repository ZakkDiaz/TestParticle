using ParticleLib.Models.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ParticleLib.Models
{
    public class ParticleEmitter
    {
        public PointF location { get; set; }
        int defaultLifespan = 50;

        public bool isEvap = false;
        public bool isSeek = true;

        public ParticleEntity EmitParticle(ref ParticleSpace2D<BaseEntity<ITimesteppableLocationEntity>> pspace, PointF relativePoint, PointF rotation, PointF velocity, Point BOUNDS, int split = 0, bool isChained = false, float stepSize = 10f, float particleSize = 1)
        {
            if (relativePoint.X > 0 && relativePoint.X < BOUNDS.X && relativePoint.Y > 0 && relativePoint.Y < BOUNDS.Y)
            {
                var spawnLoc = isChained ? relativePoint : location;

                var newParticle = new ParticleEntity();
                newParticle.ParticleInit(stepSize, particleSize, spawnLoc.X, spawnLoc.Y, ThreadSafeRandom.Next_s() * defaultLifespan, rotation.X, rotation.Y, isEvap, isSeek, split, velocity.X, velocity.Y);
                newParticle.AddForce(ThreadSafeRandom.Next_a(), 1f);
                var be = new BaseEntity<ITimesteppableLocationEntity>(newParticle);
                newParticle.parentRef = be;
                pspace.AddParticle(be);
                return newParticle;
            }
            return null;
        }

        public static void AccelToPoint(PointF fromLocation, PointF toLocation, ParticleEntity newParticle)
        {
            newParticle.AccelToPoint((fromLocation.X, fromLocation.Y), (toLocation.X, toLocation.Y));
        }

        public void SetLocation(float x, float y)
        {
            location = new PointF(x, y);
        }
    }
}
