using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ParticleLib.Models.Entities
{
    //public class ParticleEntity : ITimesteppableLocationEntity
    //{
    //    public (float, float) pos() => (dimenisons[0].pos, dimenisons[1].pos);

    //    void ITimesteppableEntity.ProcessTimestep(float diff, (float, float) focus, (int, int) BOUNDS)
    //    {
    //        ParticleEntityExtensions.ProcessTimestep(this, diff, focus, BOUNDS);
    //    }

    //    public List<DimensionProperty> dimenisons;
    //    //not to be confused with "quantum"
    //    public List<Tuple<DimensionProperty, DimensionProperty>> dimenisonsEntanglements;
    //    public float mass { get; internal set; }
    //    public float radius { get; internal set; }
    //    public bool isSticky { get; internal set; }
    //    public bool isExploding { get; internal set; }
    //    public bool isGhosting { get; internal set; }
    //    public int ghostCount { get; internal set; }
    //    public bool isAsorb { get; internal set; }
    //    public bool isReflect { get; internal set; }
    //    public float duration { get; internal set; }
    //    public bool isGravitational { get; internal set; }
    //    public bool isEvaporating { get; internal set; }
    //    public float _deltaStep { get; internal set; }
    //    public bool isSeeking { get; internal set; }
    //    public bool isDead { get; set; }
    //    public int splitCount { get; internal set; }
    //    //public BaseEntity<ITimesteppableLocationEntity> parentRef { get; set; }
    //    public RectangleF Rect => new RectangleF(pos().Item1, pos().Item2, (float)Math.Sqrt(mass), (float)Math.Sqrt(mass));
    //}

    //public static class ParticleEntityExtensions
    //{
    //    public static void ProcessTimestep(this ParticleEntity entity, float diff, (float, float) focus, (int, int) BOUNDS)
    //    {
    //        foreach (var p in entity.dimenisons)
    //        {
    //            p.ProcessTimestep(entity._deltaStep * diff, entity.mass, BOUNDS);
    //        }
    //        entity.duration -= entity._deltaStep * diff;

    //        foreach (var e in entity.dimenisonsEntanglements)
    //        {
    //            e.Item2.ProcessEntanglements(entity._deltaStep * diff, e.Item1, focus, entity.isSeeking);
    //        }

    //        if (entity.isEvaporating)
    //            entity.mass -= (entity._deltaStep * diff) / 10;
    //    }

    //    public static void AccelToPoint(this ParticleEntity entity, (float X, float Y) fromLocation, (float X, float Y) toLocation, float amt = 1)
    //    {
    //        var angle = MathExtensions.AngleFor((fromLocation.X, fromLocation.Y), (toLocation.X, toLocation.Y));
    //        var x = Math.Cos(angle);
    //        var y = Math.Sin(angle);
    //        var _breaks = 5f / amt;
    //        entity.dimenisons[0].AddAccel((float)x / _breaks);
    //        entity.dimenisons[1].AddAccel((float)y / _breaks);
    //    }

    //    public static void AddAccel(this ParticleEntity entity, int dimension, float acc)
    //    {
    //        entity.dimenisons[dimension].AddAccel(acc);
    //    }

    //    internal static float DistanceFrom(this ParticleEntity entity, ParticleEntity p2)
    //    {
    //        return Math.Abs((p2.dimenisons[0].pos - entity.dimenisons[0].pos) + (p2.dimenisons[1].pos - entity.dimenisons[1].pos));
    //    }

    //    internal static void AddForce(this ParticleEntity entity, float fAngle, float forceMult)
    //    {
    //        var cos = Math.Cos(fAngle);
    //        var sin = Math.Sin(fAngle);

    //        entity.dimenisons[0].AddVel((float)(cos * forceMult));
    //        entity.dimenisons[1].AddVel((float)(sin * forceMult));

    //    }

    //    internal static bool Intersects(this ParticleEntity entity, ParticleEntity p2)
    //    {
    //        var shellMult = 10;
    //        var dist = entity.DistanceFrom(p2) / shellMult;
    //        return (dist < entity.Rect.Width || dist < p2.Rect.Width);
    //    }

    //    internal static void SetSpeed(this ParticleEntity entity, (float, float) vel)
    //    {
    //        entity.dimenisons[0].SetVel(vel.Item1);
    //        entity.dimenisons[1].SetVel(vel.Item2);
    //    }

    //    internal static (float X, float Y) vel(this ParticleEntity entity)
    //    {
    //        return (entity.dimenisons[0].vel, entity.dimenisons[1].vel);
    //    }

    //    internal static float angle(this ParticleEntity entity)
    //    {
    //        return (float)(Math.Atan2(entity.dimenisons[1].vel, entity.dimenisons[0].vel));
    //    }

    //    internal static void AddSpeed(this ParticleEntity entity, (float, float) vel)
    //    {
    //        entity.dimenisons[0].AddVel(vel.Item1);
    //        entity.dimenisons[1].AddVel(vel.Item2);
    //    }

    //    public static void ParticleInit(this ParticleEntity entity, float dStep, float _mass, float posX = 0, float posY = 0, float _duration = 1000, float rotX = 0, float rotY = 0, bool isEvap = false, bool isSeek = false, int _splitCount = 0, float velX = 0, float velY = 0, bool _isAsorb = false)
    //    {
    //        entity._deltaStep = dStep;
    //        entity.mass = _mass;
    //        entity.radius = 0;
    //        entity.isSticky = false;
    //        entity.isExploding = false;
    //        entity.isGhosting = false;
    //        entity.ghostCount = 0;
    //        entity.isAsorb = _isAsorb;
    //        entity.isReflect = false;
    //        entity.duration = _duration;
    //        entity.isEvaporating = isEvap;
    //        entity.isGravitational = true;
    //        entity.isSeeking = isSeek;
    //        entity.splitCount = _splitCount;
    //        entity.isDead = false;
    //        entity.dimenisons = new List<DimensionProperty>();
    //        var x = new DimensionProperty(0, posX, rotX, velX);
    //        var y = new DimensionProperty(1, posY, rotY, velY);
    //        entity.dimenisons.Add(x);
    //        entity.dimenisons.Add(y);
    //        entity.dimenisonsEntanglements = new List<Tuple<DimensionProperty, DimensionProperty>>();
    //        entity.dimenisonsEntanglements.Add(new Tuple<DimensionProperty, DimensionProperty>(x, y));
    //    }
    //    public static float AngleFor(this ParticleEntity entity, (float X, float Y) relativePoint)
    //    {
    //        return MathExtensions.AngleFor((relativePoint.Y - entity.pos().Item2), (relativePoint.X - entity.pos().Item1));
    //    }

    //    internal static void Interact<T>(this ParticleEntity entity, IEnumerable<T> p2, ConcurrentBag<T> toRemove, ConcurrentBag<T> toAdd, float diff) where T : BaseEntity<ITimesteppableLocationEntity>
    //    {
    //        foreach(T entity2 in p2)
    //        {
    //            if(entity2.Entity is ParticleEntity)
    //            {
    //                DoParticleInteraction(entity, (ParticleEntity)(entity2.Entity), toRemove, toAdd, diff);
    //            }
    //        }
            
    //    }

    //    private static void DoParticleInteraction<T>(ParticleEntity entity, ParticleEntity p2, ConcurrentBag<T> toRemove, ConcurrentBag<T> toAdd, float diff) where T : BaseEntity<ITimesteppableLocationEntity>
    //    {

    //        if (entity != null && p2 != null)
    //            if (entity != p2)
    //            {
    //                var angleRads = entity.AngleFor(p2.pos());
    //                var dist = entity.DistanceFrom(p2);
    //                var mt = (entity.mass + p2.mass);
    //                if (entity.Intersects(p2))
    //                {
    //                    var vTot = (float)Math.Sqrt(Math.Pow(Math.Abs(entity.vel().X * entity.mass / mt), 2) + Math.Pow(Math.Abs(entity.vel().Y * entity.mass / mt), 2));
    //                    var v2Tot = (float)Math.Sqrt(Math.Pow(Math.Abs(p2.vel().X * entity.mass / mt), 2) + Math.Pow(Math.Abs(p2.vel().Y * entity.mass / mt), 2));
    //                    if (entity.isAsorb)
    //                    {
    //                        var np = new ParticleEntity();
    //                        np.ParticleInit(10f, mt, (entity.pos().Item1 + p2.pos().Item1) / 2, (entity.pos().Item2 + p2.pos().Item1) / 2, entity.duration + p2.duration, 0, 0, false, entity.isSeeking, 0, vTot, v2Tot);
    //                        toRemove.Add((T)(object)(entity.parentRef));
    //                        toRemove.Add((T)(object)(p2.parentRef));
    //                        toAdd.Add((T)(object)(np));
    //                    }
    //                    else
    //                    {
    //                        //var absV = vTot + v2Tot;
    //                        //var vt_ratio = vTot / absV;
    //                        //var vt2_ratio = v2Tot / absV;

    //                        //var v1Ang = p.angle();
    //                        //var v2Ang = p2.angle();

    //                        //var p1Dif = v1Ang - angleRads;
    //                        //var p2Dif = v2Ang - angleRads;


    //                        //var p1Vals = ((float)Math.Cos(p1Dif) * (p2.mass), (float)Math.Sin(p1Dif) * (p2.mass));
    //                        //var p2Vals = ((float)Math.Sin(p2Dif) * (p.mass), (float)Math.Cos(p2Dif) * (p.mass));

    //                        //var vTot_r = Math.Sqrt(Math.Pow(Math.Abs(p1Vals.Item1), 2) + Math.Pow(Math.Abs(p1Vals.Item2), 2));
    //                        //var v2Tot_r = Math.Sqrt(Math.Pow(Math.Abs(p2Vals.Item1), 2) + Math.Pow(Math.Abs(p2Vals.Item2), 2));
    //                        //var absV_r = vTot_r + v2Tot_r;

    //                        //var p1Scaled = ((float)(p1Vals.Item1 * (vTot / absV_r) * vt_ratio), (float)(p1Vals.Item2 * (vTot / absV_r) * vt_ratio));
    //                        //var p2Scaled = ((float)(p2Vals.Item1 * (v2Tot_r / absV_r) * vt_ratio), (float)(p2Vals.Item2 * (v2Tot_r / absV_r) * vt_ratio));

    //                        //p.SetSpeed(p1Scaled);
    //                        //p2.SetSpeed(p2Scaled);

    //                        //p.ProcessTimestep(diff, focus);
    //                        //p2.ProcessTimestep(diff, focus);
    //                    }

    //                }
    //                else if (entity.isGravitational && p2.isGravitational)
    //                {
    //                    var g = 1;
    //                    float forceMult = (float)(((entity.mass * p2.mass)) / Math.Pow(dist, 1.4)) * g;
    //                    entity.AddForce((float)angleRads, -forceMult * (p2.mass / mt) * diff * entity._deltaStep);
    //                    p2.AddForce((float)angleRads, (forceMult * (entity.mass / mt) * diff * p2._deltaStep));
    //                }
    //            }
    //    }
    //}
}
