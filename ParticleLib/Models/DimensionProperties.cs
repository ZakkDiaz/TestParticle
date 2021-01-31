using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

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

        public void ProcessTimestep(float stepSize, float mass, Point BOUNDS)
        {
            ProcessRotation(stepSize, mass);
            ProcessPosition(stepSize, mass, BOUNDS);
        }

        private void ProcessRotation(float stepSize, float mass)
        {
            if (rac != 0)
            {
                var mltSs = stepSize * 10;
                var p = rac > 0 ? 1 : -1;
                var amt = (Math.Abs(rac) > mltSs ? mltSs : Math.Abs(rac)) * p;
                rot += amt / mass;
                rac -= amt;
            }
        }

        private void ProcessPosition(float stepSize, float mass, Point BOUNDS)
        {
            if (acc != 0)
            {

                var p = acc > 0 ? 1 : -1;
                var amt = (Math.Abs(acc) > stepSize ? stepSize : Math.Abs(acc)) * p;
                vel += (amt) / mass;
                acc -= amt;
            }

            pos += vel;
            if (pos > BOUNDS.X)
            {
                pos = 0;
            }
            if (pos < 0)
            {
                pos = BOUNDS.Y;
            }
        }

        internal void ProcessEntanglements(float stepSize, DimensionProperty item2, (float, float) target, bool isSeeking = false)
        {
            var velocity = Math.Sqrt(Math.Pow(item2.vel, 2) + Math.Pow(this.vel, 2));
            var angle = Math.Atan2(item2.vel, vel);
            angle += (rot + item2.rot) * stepSize;
            var na1 = velocity * Math.Cos(angle);
            var na2 = velocity * Math.Sin(angle);


            vel = (float)na1;
            item2.vel = (float)na2;

            if (isSeeking)
            {
                var angleToTarg = Particle.AngleFor((this.pos, item2.pos), target);
                var angDiff = angle-angleToTarg - (45 * Math.PI/180);
                var ac1 = Math.Cos(angDiff);
                var ac2 = Math.Sin(angDiff);
                var forceMult = 1;
                SetRot((float)(-ac1* forceMult)); 
                item2.SetRot((float)(-ac2* forceMult));
            }
        }

        internal void AddRot(float y)
        {
            rac += y;
        }
        internal void SetRot(float y)
        {
            rot = 0;
            rac = y;
        }
    }
}
