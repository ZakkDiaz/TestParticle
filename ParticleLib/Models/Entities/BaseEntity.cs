using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UnityEngine;

namespace ParticleLib.Models.Entities
{
    public class BaseEntity<T> where T : ITimesteppableLocationEntity
        //: IRectQuadStorable where T : ITimesteppableLocationEntity
        
    {
        public T Entity { get; }
        public string EntityName => nameof(T);

        public Bounds AABB => Entity.Location;

        public BaseEntity(T entity)
        {
            Entity = entity;
        }

        internal void InteractWith<T>(IEnumerable<T> p2, ConcurrentBag<T> toRemove, ConcurrentBag<T> toAdd, float diff) where T : BaseEntity<ITimesteppableLocationEntity>
        {
            if (Entity is ParticleEntity)
                ((ParticleEntity)(object)(Entity)).Interact(p2, toRemove, toAdd, diff);
        }
    }
}
