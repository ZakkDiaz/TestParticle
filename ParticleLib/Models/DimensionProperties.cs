using ParticleLib.Models.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UnityEngine;

namespace ParticleLib.Models
{
    public class DimensionProperty
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
        public static void ProcessTimestep(this DimensionProperty entity, float stepSize, float mass, Vector3 BOUNDS)
        {
            entity.ProcessRotation(stepSize, mass);
            entity.ProcessPosition(stepSize, mass, BOUNDS);
        }

        private static void ProcessRotation(this DimensionProperty entity, float stepSize, float mass)
        {
            //if (entity.rac != 0)
            //{
            //    var mltSs = stepSize * 10;
            //    var p = entity.rac > 0 ? 1 : -1;
            //    var amt = (Math.Abs(entity.rac) > mltSs ? mltSs : Math.Abs(entity.rac)) * p;
            //    entity.rot += amt / mass;
            //    entity.rac -= amt;
            //}
        }

        private static void ProcessPosition(this DimensionProperty entity, float stepSize, float mass, Vector2 BOUNDS)
        {
            //if (entity.vel == float.NaN)
            //    entity.vel = 0;
            //var curAc = entity.acc;
            //if (curAc != 0)
            //{

            //    var p = curAc > 0 ? 1 : -1;
            //    var amt = (Math.Abs(curAc) > stepSize ? stepSize * p : Math.Abs(entity.acc)) * p;
            //    entity.vel += (amt) / mass;
            //    entity.acc -= amt;
            //}

            entity.pos += (entity.vel * stepSize);
            //if (entity.pos > BOUNDS.x)
            //{
            //    entity.pos = 0;
            //}
            //if (entity.pos < 0)
            //{
            //    entity.pos = BOUNDS.y;
            //}
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
        internal static void ProcessEntanglements(this DimensionProperty entity, float stepSize, DimensionProperty item2, DimensionProperty item3, Vector2 target, bool isSeeking = false)
        {
            //Vector3 rotations = new Vector3(entity.rot, item2.rot, item3.rot).normalized;


            ////var velocity = Math.Sqrt(Math.Pow(item2.vel, 2) + Math.Pow(entity.vel, 2));
            ////var angle = Math.Atan2(item2.vel, entity.vel);
            ////angle += (entity.rot + item2.rot) * stepSize;

            //var na1 = entity.vel * rotations.x;
            //var na2 = item2.vel * rotations.y;
            //var na3 = item3.vel * rotations.z;


            //entity.vel = (float)na1;
            //item2.vel = (float)na2;
            //item3.vel = (float)na3;

            //if (isSeeking)
            //{
            //    var angleToTarg = item2.AngleFor(entity, target);
            //    var angDiff = angle - angleToTarg - (45 * Math.PI / 180);
            //    var ac1 = Math.Cos(angDiff);
            //    var ac2 = Math.Sin(angDiff);
            //    var forceMult = 1;
            //    entity.SetRot((float)(-ac1 * forceMult));
            //    item2.SetRot((float)(-ac2 * forceMult));
            //}
        }

    }
}
