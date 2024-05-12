using System.Numerics;

namespace ParticleLib.Models.Entities
{
    public interface ITimesteppableEntity
    {
        internal void ProcessTimestep(float diff, Vector3 focus, Vector3 BOUNDS);
        Vector3 Location { get; }
    }
}