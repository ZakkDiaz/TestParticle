using ParticleLib.Models.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ParticleLib.Models
{
    public struct DimensionProperty
    {
        public DimensionProperty(int index, float _pos = 0, float _rot = 0, float _vel = 0, bool _repel = false)
        {
            dimensionIndex = index;
            pos = _pos;
            vel = _vel;
            rot = _rot;
            acc = 0;
            rac = 0;
            rep = _repel;
            bias = false;
        }
        public int dimensionIndex { get; set; }
        public float pos { get; internal set; }
        public float vel { get; internal set; }
        public float rot { get; internal set; }
        public float acc { get; internal set; }
        public float rac { get; internal set; }
        public bool rep { get; internal set; }
        public bool bias { get; internal set; }

        public void AddAccel(float val)
        {
            acc += val;
        }

        internal void AddVel(float v)
        {
            vel += v;
        }

        internal void SetVel(float v)
        {
            vel = v;
        }
    }
    public static class DimensionPropertyExtensions
    {
        public static void ProcessTimestep(this DimensionProperty entity, float stepSize, float mass, (int, int) BOUNDS)
        {
            entity.ProcessRotation(stepSize, mass);
            entity.ProcessPosition(stepSize, mass, BOUNDS);
        }

        private static void ProcessRotation(this DimensionProperty entity, float stepSize, float mass)
        {
            if (entity.rac != 0)
            {
                var mltSs = stepSize * 10;
                var p = entity.rac > 0 ? 1 : -1;
                var amt = (Math.Abs(entity.rac) > mltSs ? mltSs : Math.Abs(entity.rac)) * p;
                entity.rot += amt / mass;
                entity.rac -= amt;
            }
        }

        private static void ProcessPosition(this DimensionProperty entity, float stepSize, float mass, (int, int) BOUNDS)
        {
            if (entity.acc != 0)
            {

                var p = entity.acc > 0 ? 1 : -1;
                var amt = (Math.Abs(entity.acc) > stepSize ? stepSize : Math.Abs(entity.acc)) * p;
                entity.vel += (amt) / mass;
                entity.acc -= amt;
            }

            entity.pos += entity.vel;
            if (entity.pos > BOUNDS.Item1)
            {
                entity.pos = 0;
            }
            if (entity.pos < 0)
            {
                entity.pos = BOUNDS.Item2;
            }
        }

        internal static void AddRot(this DimensionProperty entity, float y)
        {
            entity.rac += y;
        }
        internal static void SetRot(this DimensionProperty entity, float y)
        {
            entity.rot = 0;
            entity.rac = y;
        }
        internal static void ProcessEntanglements(this DimensionProperty entity, float stepSize, ref DimensionProperty item2, (float, float) target, bool isSeeking = false)
        {
            var velocity = Math.Sqrt(Math.Pow(item2.vel, 2) + Math.Pow(entity.vel, 2));
            var angle = Math.Atan2(item2.vel, entity.vel);
            angle += (entity.rot + item2.rot) * stepSize;
            var na1 = velocity * Math.Cos(angle);
            var na2 = velocity * Math.Sin(angle);


            entity.vel = (float)na1;
            item2.vel = (float)na2;

            if (isSeeking)
            {
                var angleToTarg = item2.AngleFor(entity, target);
                var angDiff = angle - angleToTarg - (45 * Math.PI / 180);
                var ac1 = Math.Cos(angDiff);
                var ac2 = Math.Sin(angDiff);
                var forceMult = 1;
                entity.SetRot((float)(-ac1 * forceMult));
                item2.SetRot((float)(-ac2 * forceMult));
            }
        }

        internal static float AngleFor(this DimensionProperty entity, DimensionProperty other, (float X, float Y) to)
        {
            return MathExtensions.AngleFor((to.Y - other.pos), (to.X - entity.pos));
        }

    }
}
