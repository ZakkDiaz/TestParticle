using UnityEngine;

namespace ParticleLib.Models.Entities
{
    public interface ITimesteppableEntity
    {
        internal void ProcessTimestep(float diff, Vector2 focus, Vector2 BOUNDS);
        Vector3 Location { get; }
    }
}