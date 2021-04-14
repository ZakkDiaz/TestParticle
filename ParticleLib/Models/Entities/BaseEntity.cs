using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ParticleLib.Models.Entities
{
    //public class BaseEntity<T> : IRectQuadStorable where T : ITimesteppableLocationEntity
    //{
    //    public RectangleF Rect => Entity.Rect;
    //    public T Entity { get; }
    //    public string EntityName => nameof(T);
    //    public BaseEntity(T entity)
    //    {
    //        Entity = entity;
    //    }

    //    internal void InteractWith<T>(IEnumerable<T> p2, ConcurrentBag<T> toRemove, ConcurrentBag<T> toAdd, float diff) where T : BaseEntity<ITimesteppableLocationEntity>
    //    {
    //        if (Entity is ParticleEntity)
    //            ((ParticleEntity)(object)(Entity)).Interact(p2, toRemove, toAdd, diff);
    //    }
    //}
}
