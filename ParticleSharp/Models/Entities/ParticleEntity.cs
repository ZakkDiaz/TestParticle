using ParticleSharp.Models;
using ParticleSharp.Models.Entities;
using System.Numerics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using ParticleLib.Models._3D;

namespace ParticleSharp.Models.Entities
{
    public class ParticleEntity : ITimesteppableLocationEntity
    {
        public Vector3 pos() => new Vector3(dimenisons[0].pos, dimenisons[1].pos, dimenisons[2].pos);

        void ITimesteppableEntity.ProcessTimestep(float diff, Vector3 focus, AAABBB BOUNDS)
        {
            this.ProcessTimestep(diff, focus, BOUNDS);
        }

        public List<DimensionProperty> dimenisons;
        //not to be confused with "quantum"
        public List<Tuple<DimensionProperty, DimensionProperty, DimensionProperty>> dimenisonsEntanglements;
        public float density { get; set; }
        public float mass { get; set; }
        public float radius { get; internal set; }
        public bool isSticky { get; internal set; }
        public bool isExploding { get; internal set; }
        public bool isGhosting { get; internal set; }
        public int ghostCount { get; internal set; }
        public bool isAsorb { get; internal set; }
        public bool isReflect { get; internal set; }
        public float duration { get; internal set; }
        public bool isGravitational { get; internal set; }
        public bool isEvaporating { get; internal set; }
        public float _deltaStep { get; internal set; }
        public bool isSeeking { get; internal set; }
        public bool isDead { get; set; }
        public int splitCount { get; internal set; }
        //public BaseEntity<ITimesteppableLocationEntity> parentRef { get; set; }
        public Vector3 Location { get; set; }
    }

    public static class ParticleEntityExtensions
    {
        public static void ProcessTimestep(this ParticleEntity entity, float diff, Vector3 focus, AAABBB BOUNDS)
        {
            for (var i = 0; i < entity.dimenisons.Count; i++)
            {
                var dim = entity.dimenisons[i];
                dim = DimensionPropertyExtensions.ProcessTimestep(dim, entity._deltaStep * diff, entity.mass, BOUNDS);
                entity.dimenisons[i] = dim;
            }
            entity.duration -= entity._deltaStep * diff;

            foreach (var e in entity.dimenisonsEntanglements)
            {
                e.Item1.ProcessEntanglements(entity._deltaStep * diff, e.Item2, e.Item3, focus, entity.isSeeking);
            }

            if (entity.isEvaporating)
                entity.mass -= entity._deltaStep * diff / 10;

            var epos = entity.pos();
            entity.Location = epos;
        }

        public static void AccelToPoint(this ParticleEntity entity, Vector3 fromLocation, Vector3 toLocation, float amt = 1)
        {
            var angleX = MathExtensions.AngleFor(new Vector3(fromLocation.X, 0, 0), new Vector3(toLocation.X, 0, 0));
            var angleY = MathExtensions.AngleFor(new Vector3(0, fromLocation.Y, 0), new Vector3(0, toLocation.Y, 0));
            var angleZ = MathExtensions.AngleFor(new Vector3(0, 0, fromLocation.Z), new Vector3(0, 0, toLocation.Z));
            //var x = Math.Cos(angle);
            //var y = Math.Sin(angle);
            var _breaks = 5f / amt;

            var refx = entity.dimenisons[0];
            refx.AddAccel((float)angleX.X / _breaks);
            entity.dimenisons[0] = refx;
            var refy = entity.dimenisons[1];
            refy.AddAccel((float)angleY.Y / _breaks);
            entity.dimenisons[1] = refx;
            var refz = entity.dimenisons[2];
            refz.AddAccel((float)angleZ.Z / _breaks);
            entity.dimenisons[2] = refz;
        }

        //    public static void AddAccel(this ParticleEntity entity, int dimension, float acc)
        //    {
        //        entity.dimenisons[dimension].AddAccel(acc);
        //    }

        internal static float DistanceFrom(this ParticleEntity entity, ParticleEntity p2)
        {
            //var calculatedDiff = Math.Abs((p2.dimenisons[0].pos - entity.dimenisons[0].pos) + (p2.dimenisons[1].pos - entity.dimenisons[1].pos) + (p2.dimenisons[2].pos - entity.dimenisons[2].pos));
            var diff = p2.pos() - entity.pos();
            return diff.Length();
        }

        internal static void AddForce(this ParticleEntity entity, Vector3 angle, float forceMult)
        {
            var xref = entity.dimenisons[0];
            xref.AddVel((float)(angle.X * forceMult / entity.mass));
            entity.dimenisons[0] = xref;

            var yref = entity.dimenisons[1];
            yref.AddVel((float)(angle.Y * forceMult / entity.mass));
            entity.dimenisons[1] = yref;

            var zref = entity.dimenisons[2];
            zref.AddVel((float)(angle.Z * forceMult / entity.mass));
            entity.dimenisons[2] = zref;
        }

        internal static bool Intersects(this ParticleEntity entity, ParticleEntity p2)
        {
            var shellMult = 10;
            var dist = entity.DistanceFrom(p2) / shellMult;
            return dist < Math.Sqrt(entity.mass) || dist < Math.Sqrt(entity.mass);
        }

        //internal static void SetSpeed(this ParticleEntity entity, Vector3 vel)
        //{
        //    entity.dimenisons[0].SetVel(vel.x);
        //    entity.dimenisons[1].SetVel(vel.y);
        //    entity.dimenisons[2].SetVel(vel.z);
        //}

        internal static Vector3 vel(this ParticleEntity entity)
        {
            return new Vector3(entity.dimenisons[0].vel, entity.dimenisons[1].vel, entity.dimenisons[2].vel);
        }

        //internal static void AddSpeed(this ParticleEntity entity, Vector3 vel)
        //{
        //    entity.dimenisons[0].AddVel(vel.x);
        //    entity.dimenisons[1].AddVel(vel.y);
        //    entity.dimenisons[2].AddVel(vel.z);
        //}

        public static void ParticleInit(this ParticleEntity entity, float dStep, float _mass, float posX = 0, float posY = 0, float posZ = 0, float _duration = 1000, float rotX = 0, float rotY = 0, float rotZ = 0, bool isEvap = false, bool isSeek = false, int _splitCount = 0, float velX = 0, float velY = 0, float velZ = 0, bool _isAsorb = false, float density = 1)
        {
            entity._deltaStep = dStep;
            entity.mass = _mass;
            entity.radius = 0;
            entity.density = density;
            entity.isSticky = false;
            entity.isExploding = false;
            entity.isGhosting = false;
            entity.ghostCount = 0;
            entity.isAsorb = _isAsorb;
            entity.isReflect = false;
            entity.duration = _duration;
            entity.isEvaporating = isEvap;
            entity.isGravitational = true;
            entity.isSeeking = isSeek;
            entity.splitCount = _splitCount;
            entity.isDead = false;
            entity.dimenisons = new List<DimensionProperty>();
            var x = new DimensionProperty(0, posX, rotX, velX);
            var y = new DimensionProperty(1, posY, rotY, velY);
            var z = new DimensionProperty(2, posZ, rotZ, velZ);
            entity.dimenisons.Add(x);
            entity.dimenisons.Add(y);
            entity.dimenisons.Add(z);
            entity.dimenisonsEntanglements = new List<Tuple<DimensionProperty, DimensionProperty, DimensionProperty>>();
            entity.dimenisonsEntanglements.Add(new Tuple<DimensionProperty, DimensionProperty, DimensionProperty>(x, y, z));
            //entity.dimenisonsEntanglements.Add(new Tuple<DimensionProperty, DimensionProperty>(y, z));
            //entity.dimenisonsEntanglements.Add(new Tuple<DimensionProperty, DimensionProperty>(x, z));

            var epos = entity.pos();
            entity.Location = epos;
        }
        public static Vector3 AngleFor(this ParticleEntity entity, Vector3 relativePoint)
        {
            return MathExtensions.AngleFor(entity.Location, relativePoint);
        }

        internal static void Interact(this ParticleEntity entity, IEnumerable<ParticleEntity> p2, ConcurrentBag<ParticleEntity> toRemove, ConcurrentBag<ParticleEntity> toAdd, float diff)
        {
            foreach (var entity2 in p2)
            {
                if (entity == entity2)
                    continue;
                DoParticleInteraction(entity, entity2, toRemove, toAdd, diff);
            }

        }

        private static void DoParticleInteraction(ParticleEntity entity, ParticleEntity p2, ConcurrentBag<ParticleEntity> toRemove, ConcurrentBag<ParticleEntity> toAdd, float diff)
        {

            if (entity != null && p2 != null)
                if (entity != p2)
                {
                    var g = 5f;
                    var posDiff = p2.Location - entity.Location;// entity.AngleFor(p2.Location);
                    var dist = entity.DistanceFrom(p2);
                    if (dist < 25)
                        dist = 25;
                    //if(dist == float.NaN)
                    //{

                    //}
                    var mt = entity.mass + p2.mass;
                    //if (entity.Intersects(p2))
                    //{
                    //    //var vTot = (float)Math.Sqrt(Math.Pow(Math.Abs(entity.vel().x * entity.mass / mt), 2) + Math.Pow(Math.Abs(entity.vel().y * entity.mass / mt), 2));
                    //    //var v2Tot = (float)Math.Sqrt(Math.Pow(Math.Abs(p2.vel().x * entity.mass / mt), 2) + Math.Pow(Math.Abs(p2.vel().y * entity.mass / mt), 2));
                    //    //if (entity.isAsorb)
                    //    //{
                    //    //    var np = new ParticleEntity();
                    //    //    np.ParticleInit(10f, mt, (entity.pos().Item1 + p2.pos().Item1) / 2, (entity.pos().Item2 + p2.pos().Item2) / 2, (entity.pos().Item3 + p2.pos().Item3) / 2, entity.duration + p2.duration, 0, 0, 0, false, entity.isSeeking, 0, vTot, v2Tot, v3Tot);
                    //    //    toRemove.Add((T)(object)(entity.parentRef));
                    //    //    toRemove.Add((T)(object)(p2.parentRef));
                    //    //    toAdd.Add((T)(object)(np));
                    //    //}
                    //    //else
                    //    //{
                    //    //    //var absV = vTot + v2Tot;
                    //    //    //var vt_ratio = vTot / absV;
                    //    //    //var vt2_ratio = v2Tot / absV;

                    //    //    //var v1Ang = p.angle();
                    //    //    //var v2Ang = p2.angle();

                    //    //    //var p1Dif = v1Ang - angleRads;
                    //    //    //var p2Dif = v2Ang - angleRads;


                    //    //    //var p1Vals = ((float)Math.Cos(p1Dif) * (p2.mass), (float)Math.Sin(p1Dif) * (p2.mass));
                    //    //    //var p2Vals = ((float)Math.Sin(p2Dif) * (p.mass), (float)Math.Cos(p2Dif) * (p.mass));

                    //    //    //var vTot_r = Math.Sqrt(Math.Pow(Math.Abs(p1Vals.Item1), 2) + Math.Pow(Math.Abs(p1Vals.Item2), 2));
                    //    //    //var v2Tot_r = Math.Sqrt(Math.Pow(Math.Abs(p2Vals.Item1), 2) + Math.Pow(Math.Abs(p2Vals.Item2), 2));
                    //    //    //var absV_r = vTot_r + v2Tot_r;

                    //    //    //var p1Scaled = ((float)(p1Vals.Item1 * (vTot / absV_r) * vt_ratio), (float)(p1Vals.Item2 * (vTot / absV_r) * vt_ratio));
                    //    //    //var p2Scaled = ((float)(p2Vals.Item1 * (v2Tot_r / absV_r) * vt_ratio), (float)(p2Vals.Item2 * (v2Tot_r / absV_r) * vt_ratio));

                    //    //    //p.SetSpeed(p1Scaled);
                    //    //    //p2.SetSpeed(p2Scaled);

                    //    //    //p.ProcessTimestep(diff, focus);
                    //    //    //p2.ProcessTimestep(diff, focus);
                    //    //}

                    //}
                    //else



                    if (entity.isGravitational && p2.isGravitational)
                    {
                        float forceMult = -(float)(entity.mass * p2.mass / Math.Pow(dist, 2)) * g;
                        entity.AddForce(posDiff, -forceMult * (p2.mass / mt) * diff * entity._deltaStep);
                        p2.AddForce(posDiff, forceMult * (entity.mass / mt) * diff * p2._deltaStep);
                    }

                    //if (entity.isGravitational && p2.isGravitational)
                    //{
                    //    //dist = dist / 1000f;
                    //    if (dist < 5)
                    //        dist = 5;
                    //    float forceMult = -((float)(g * ((entity.mass * p2.mass))
                    //         /Math.Pow(dist, 1.01)
                    //        ));
                    //    entity.AddForce(angleRads, (-forceMult  * diff * entity._deltaStep));
                    //    p2.AddForce(angleRads, (forceMult * diff * p2._deltaStep));
                    //}
                }
        }
    }
}
