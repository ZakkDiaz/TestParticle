using UnityEngine;

namespace ParticleLib.Models.Entities
{
    public interface ITimesteppableEntity
    {
        internal void ProcessTimestep(float diff, (float, float) focus, (int, int, int) BOUNDS);
        Vector3 Location { get; set; }
    }
}