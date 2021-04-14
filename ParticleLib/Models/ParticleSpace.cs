using ParticleLib.Models.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticleLib.Models
{
    //public partial class ParticleSpace2D<T> where T : BaseEntity<ITimesteppableLocationEntity>
    //{
    //    private PointF from;
    //    private PointF to;

    //    public ParticleSpace2D(PointF _from, PointF _to)
    //    {
    //        from = _from;
    //        to = _to;
    //    }

    //    public void AddParticle(T particle)
    //    {
    //        particles.Add(particle);
    //    }

    //    public List<T> GetParticles()
    //    {
    //        return particles.GetAllObjects().ToList();
    //    }
    //    public RectangleF[] GetRects()
    //    {
    //        List<RectangleF> rectArs = new List<RectangleF>();
    //        rectArs.Add(particles.QuadRect);
    //        return rectArs.ToArray();
    //    }

    //    internal void ProcessEntityState(RectangleF rectangleF, Action<List<T>> p)
    //    {
    //        p.Invoke(particles.GetObjects(rectangleF));
    //    }

    //    internal void Remove(T p)
    //    {
    //        particles.Remove(p);
    //    }

    //    internal void Add(T p)
    //    {
    //        particles.Add(p);
    //    }

    //    internal void Move(T p)
    //    {
    //        particles.Move(p);
    //    }
    //}

    //public static class ParticleSpace2DExtensions
    //{
    //    public static void ProcessTimestep<T>(this ParticleSpace2D<T> space, float diff, (float, float) focus, (int, int) BOUNDS) where T : BaseEntity<ITimesteppableLocationEntity>
    //    {
    //        Parallel.ForEach<T>(space.GetParticles(), (p) =>
    //        {
    //            ProcessEntity(p, diff, focus, BOUNDS);
    //        });

    //        var pList = space.GetParticles().ToList();
    //        ConcurrentBag<T> toRemove = new ConcurrentBag<T>();
    //        ConcurrentBag<T> toAdd = new ConcurrentBag<T>();
    //        ProcessEntityStates(space, BOUNDS, pList, toRemove, toAdd, diff);


    //        foreach (var p in toRemove)
    //            space.Remove(p);

    //        foreach (var p in toAdd)
    //            space.Add(p);
    //    }

    //    private static void ProcessEntityStates<T>(this ParticleSpace2D<T> space, (int, int) BOUNDS, List<T> pList, ConcurrentBag<T> toRemove, ConcurrentBag<T> toAdd, float diff) where T : BaseEntity<ITimesteppableLocationEntity>
    //    {
    //        int dist = BOUNDS.Item1 / 2;
    //        foreach (var p in pList)
    //        {
    //            space.ProcessEntityState(new RectangleF(p.Rect.X, p.Rect.Y, dist, dist), (p2) =>
    //            {
    //                p.InteractWith(p2, toRemove, toAdd, diff);
    //            });
    //        }


    //        foreach (var p in pList)
    //        {
    //            space.Move(p);
    //        }
    //    }

    //    private static void ProcessEntity<T>(T p, float diff, (float, float) focus, (int, int) BOUNDS) where T : BaseEntity<ITimesteppableLocationEntity> => p.Entity.ProcessTimestep(diff, focus, BOUNDS);
    //}
}
