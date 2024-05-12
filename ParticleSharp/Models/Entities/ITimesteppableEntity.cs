using ParticleLib.Models._3D;
using System.Numerics;

namespace ParticleSharp.Models.Entities
{
    public interface ITimesteppableEntity
    {
        internal void ProcessTimestep(float diff, Vector3 focus, AAABBB BOUNDS);
        Vector3 Location { get; }
    }
}