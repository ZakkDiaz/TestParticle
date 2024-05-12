using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ParticleSharp.Models
{
    //public class Particle : IRectQuadStorable
    //{
    //    public (float, float) pos() => (dimenisons[0].pos, dimenisons[1].pos);
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
    //    internal float _deltaStep { get; }
    //    public bool isSeeking { get; internal set; }
    //    public bool isDead { get; set; } = false;
    //    public int splitCount { get; internal set; }

    //    public RectangleF Rect => new RectangleF(pos().Item1, pos().Item2, (float)Math.Sqrt(mass), (float)Math.Sqrt(mass));

    //    internal static float AngleFor((float X, float Y) location, (float X, float Y) relativePoint)
    //    {
    //        return (float)(Math.Atan2((relativePoint.Y - location.Y), (relativePoint.X - location.X)));
    //    }

    //    public Particle(float dStep, float _mass, float posX = 0, float posY = 0, float _duration = 1000, float rotX = 0, float rotY = 0, bool isEvap = false, bool isSeek = false, int _splitCount = 0, float velX = 0, float velY = 0, bool _isAsorb = false)
    //    {
    //        _deltaStep = dStep;
    //        mass = _mass;
    //        radius = 0;
    //        isSticky = false;
    //        isExploding = false;
    //        isGhosting = false;
    //        ghostCount = 0;
    //        isAsorb = _isAsorb;
    //        isReflect = false;
    //        duration = _duration;
    //        isEvaporating = isEvap;
    //        isGravitational = true;
    //        isSeeking = isSeek;
    //        splitCount = _splitCount;
    //        dimenisons = new List<DimensionProperty>();
    //        var x = new DimensionProperty(0, posX, rotX, velX);
    //        var y = new DimensionProperty(1, posY, rotY, velY);
    //        dimenisons.Add(x);
    //        dimenisons.Add(y);
    //        dimenisonsEntanglements = new List<Tuple<DimensionProperty, DimensionProperty>>();
    //        dimenisonsEntanglements.Add(new Tuple<DimensionProperty, DimensionProperty>(x, y));
    //    }

    //    public void ProcessTimestep(long diff, (float, float) target, Point BOUNDS)
    //    {
    //        foreach(var p in dimenisons)
    //        {
    //            p.ProcessTimestep(_deltaStep*diff, mass, BOUNDS);
    //        }
    //        duration -= _deltaStep * diff;

    //        foreach(var e in dimenisonsEntanglements)
    //        {
    //            e.Item1.ProcessEntanglements(_deltaStep * diff, e.Item2, target, isSeeking);
    //        }

    //        if (isEvaporating)
    //            mass -= (_deltaStep * diff) / 10;
    //    }

    //    public void AccelToPoint((float X, float Y) fromLocation, (float X, float Y) toLocation, float amt = 1)
    //    {
    //        var angle = Particle.AngleFor((fromLocation.X, fromLocation.Y), (toLocation.X, toLocation.Y));
    //        var x = Math.Cos(angle);
    //        var y = Math.Sin(angle);
    //        var _breaks = 5f / amt;
    //        dimenisons[0].AddAccel((float)x / _breaks);
    //        dimenisons[1].AddAccel((float)y / _breaks);
    //    }

    //    public void AddAccel(int dimension, float acc)
    //    {
    //        dimenisons[dimension].AddAccel(acc);
    //    }

    //    internal float DistanceFrom(Particle p2)
    //    {
    //        return Math.Abs((p2.dimenisons[0].pos - dimenisons[0].pos) + (p2.dimenisons[1].pos - dimenisons[1].pos));
    //    }

    //    internal void AddForce(float fAngle, float forceMult)
    //    {
    //        var cos = Math.Cos(fAngle);
    //        var sin = Math.Sin(fAngle);

    //        dimenisons[0].vel += (float)(cos * forceMult);
    //        dimenisons[1].vel += (float)(sin * forceMult);

    //    }

    //    internal bool Intersects(Particle p2)
    //    {
    //        var shellMult = 10;
    //        var dist = DistanceFrom(p2)/shellMult;
    //        return (dist < this.Rect.Width || dist < p2.Rect.Width);
    //    }

    //    internal void SetSpeed((float, float) vel)
    //    {
    //        dimenisons[0].vel = vel.Item1;
    //        dimenisons[1].vel = vel.Item2;
    //    }

    //    internal (float X, float Y) vel()
    //    {
    //        return (dimenisons[0].vel, dimenisons[1].vel);
    //    }
    //    internal float angle()
    //    {
    //        return (float)(Math.Atan2(dimenisons[1].vel, dimenisons[0].vel));
    //    }

    //    internal void AddSpeed((float, float) vel)
    //    {
    //        dimenisons[0].vel += vel.Item1;
    //        dimenisons[1].vel += vel.Item2;
    //    }
    //}
}
