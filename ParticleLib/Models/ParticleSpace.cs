using ParticleLib.Models.Entities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticleLib.Models
{

    public static class ParticleSpace2DExtensions
    {
        public static void ProcessTimestep<T>(this ParticleSpace3D<T> space, List<T> particles, float diff, (float, float) focus, (int, int, int) BOUNDS) where T : BaseEntity<ITimesteppableLocationEntity>
        {
            Parallel.ForEach<T>(particles, (p) =>
            {
                ProcessEntity(p, diff, focus, BOUNDS);
            });

            ConcurrentBag<T> toRemove = new ConcurrentBag<T>();
            ConcurrentBag<T> toAdd = new ConcurrentBag<T>();
            ProcessEntityStates(space, BOUNDS, particles, toRemove, toAdd, diff);


            foreach (var p in toRemove)
                space.Remove(p);

            foreach (var p in toAdd)
                space.Add(p);
        }

        private static void ProcessEntityStates<T>(this ParticleSpace3D<T> space, (int, int, int) BOUNDS, List<T> pList, ConcurrentBag<T> toRemove, ConcurrentBag<T> toAdd, float diff) where T : BaseEntity<ITimesteppableLocationEntity>
        {
            int dist = BOUNDS.Item1 / 2;
            foreach (var p in pList)
            {
                space.ProcessEntityState(p.Location, (p2) =>
                {
                    p.InteractWith(p2, toRemove, toAdd, diff);
                });
            }


            foreach (var p in pList)
            {
                space.Move(p);
            }
        }

        private static void ProcessEntity<T>(T p, float diff, (float, float) focus, (int, int, int) BOUNDS) where T : BaseEntity<ITimesteppableLocationEntity> => p.Entity.ProcessTimestep(diff, focus, BOUNDS);
    }
}
