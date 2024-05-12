using ParticleLib.Models._3D;
using ParticleSharp.Models.Entities;
using System.Numerics;

namespace ParticleSharp.Models
{
    public class ParticleEmitter
    {
        public Vector3 location { get; set; }
        int defaultLifespan = 50;

        public bool isEvap = false;
        public bool isSeek = false;

        private static object particleLockObj = new object();
        //private static List<BaseEntity<ITimesteppableLocationEntity>> particles = new List<BaseEntity<ITimesteppableLocationEntity>>();

        //public static List<BaseEntity<ITimesteppableLocationEntity>> Particles { get { lock (particleLockObj) return new List<BaseEntity<ITimesteppableLocationEntity>>(particles); } }

        public ParticleEntity EmitParticle(ref ParticleSpace3D pspace, Vector3 relativePoint, Vector3 rotation, Vector3 velocity, AAABBB BOUNDS, int split = 0, bool isChained = false, float stepSize = 10f, float particleSize = 1, float emissionSpeed = 1, float density = 1)
        {
            //if (relativePoint.x > BOUNDS.min.x && relativePoint.x < BOUNDS.max.x && relativePoint.y > BOUNDS.min.y && relativePoint.y < BOUNDS.max.y && relativePoint.z > BOUNDS.min.z && relativePoint.z < BOUNDS.max.z)
            {
                var newParticle = new ParticleEntity();
                newParticle.ParticleInit(stepSize, particleSize, relativePoint.X, relativePoint.Y, relativePoint.Z, ThreadSafeRandom.Next_s() * defaultLifespan, rotation.X, rotation.Y, rotation.Z, isEvap, isSeek, split, velocity.X, velocity.Y, velocity.Z, density: density);
                newParticle.AddForce(ThreadSafeRandom.Next_v3f() - new Vector3(.5f, .5f, .5f), emissionSpeed);
                //var be = new BaseEntity<ITimesteppableLocationEntity>(newParticle);
                //newParticle.parentRef = be;                pspace.AddParticle(newParticle);
                pspace.Add(newParticle);
                return newParticle;
            }
            return null;
        }

        public static void AccelToPoint(Vector3 fromLocation, Vector3 toLocation, ParticleEntity newParticle)
        {
            newParticle.AccelToPoint(fromLocation, toLocation);
        }

        public void SetLocation(float x, float y, float z)
        {
            location = new Vector3(x, y, z);
        }
    }
}
