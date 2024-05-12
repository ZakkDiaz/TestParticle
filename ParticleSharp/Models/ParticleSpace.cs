using ParticleSharp.Models;
using ParticleSharp.Models.Entities;
using System.Numerics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParticleLib.Models._3D;

namespace ParticleSharp.Models
{

    public static class ParticleSpace2DExtensions
    {
        public static bool ProcessTimestep(this ParticleSpace3D space, float diff, Vector3 focus, AAABBB BOUNDS)
        {

            ConcurrentBag<ParticleEntity> toRemove = new ConcurrentBag<ParticleEntity>();
            ConcurrentBag<ParticleEntity> toAdd = new ConcurrentBag<ParticleEntity>();
            var particles = space.GetParticles();
            Parallel.ForEach(particles, (p) =>
            {

                if(p.pos().X < BOUNDS.From.X || p.pos().X > BOUNDS.To.X || p.pos().Y < BOUNDS.From.Y || p.pos().Y > BOUNDS.To.Y || p.pos().Z < BOUNDS.From.Z || p.pos().Z > BOUNDS.To.Z)
                {
                    toRemove.Add(p);
                    return;
                }

                ProcessEntity(p, diff, focus, BOUNDS);
            });

            space.ProcessEntityStates(BOUNDS, particles, toRemove, toAdd, diff);


            foreach (var p in toRemove)
                space.Remove(p);

            foreach (var p in toAdd)
                space.Add(p);

            return toRemove.Any() || toAdd.Any();
        }

        private static void ProcessEntityStates(this ParticleSpace3D space, AAABBB BOUNDS, List<ParticleEntity> pList, ConcurrentBag<ParticleEntity> toRemove, ConcurrentBag<ParticleEntity> toAdd, float diff)
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

        private static void ProcessEntity(ParticleEntity p, float diff, Vector3 focus, AAABBB BOUNDS) => p.ProcessTimestep(diff, focus, BOUNDS);
    }
}
