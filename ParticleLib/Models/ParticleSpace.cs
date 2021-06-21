using ParticleLib.Models.Entities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ParticleLib.Models
{

    public static class ParticleSpace2DExtensions
    {
        public static void ProcessTimestep(this ParticleSpace3D space, List<ParticleEntity> particles, float diff, Vector3 focus, Vector3 BOUNDS)
        {
            Parallel.ForEach(particles, (p) =>
            {
                ProcessEntity(p, diff, focus, BOUNDS);
            });

            ConcurrentBag<ParticleEntity> toRemove = new ConcurrentBag<ParticleEntity>();
            ConcurrentBag<ParticleEntity> toAdd = new ConcurrentBag<ParticleEntity>();
            ProcessEntityStates(space, BOUNDS, particles, toRemove, toAdd, diff);


            foreach (var p in toRemove)
                space.Remove(p);

            foreach (var p in toAdd)
                space.Add(p);
        }

        private static void ProcessEntityStates(this ParticleSpace3D space, Vector3 BOUNDS, List<ParticleEntity> pList, ConcurrentBag<ParticleEntity> toRemove, ConcurrentBag<ParticleEntity> toAdd, float diff)
        {
            //int dist = BOUNDS.x / 2;
            foreach (var p in pList)
            {
                space.ProcessEntityState(p.Location, (p2) =>
                {
                    p.Interact(p2, toRemove, toAdd, diff);
                });
            }


            foreach (var p in pList)
            {
                space.Move(p);
            }
        }

        private static void ProcessEntity(ParticleEntity p, float diff, Vector2 focus, Vector3 BOUNDS) => p.ProcessTimestep(diff, focus, BOUNDS);
    }
}
