using ParticleLib.Modern.Models;
using System.Collections.Concurrent;

namespace ParticleLib.Modern.Rendering
{
    /// <summary>
    /// Represents a snapshot of the simulation state for rendering purposes.
    /// </summary>
    public class SimulationSnapshot
    {
        /// <summary>
        /// Gets the particles in the simulation at the time of the snapshot.
        /// </summary>
        public IReadOnlyList<Particle> Particles { get; }
        
        /// <summary>
        /// Gets the octree node information at the time of the snapshot.
        /// </summary>
        public IReadOnlyList<OctreeNodeInfo> Nodes { get; }
        
        /// <summary>
        /// Gets the timestamp of when this snapshot was taken.
        /// </summary>
        public DateTime Timestamp { get; }
        
        /// <summary>
        /// Initializes a new instance of the SimulationSnapshot class.
        /// </summary>
        public SimulationSnapshot(IEnumerable<Particle> particles, IEnumerable<OctreeNodeInfo> nodes)
        {
            Particles = particles.ToList();
            Nodes = nodes.ToList();
            Timestamp = DateTime.UtcNow;
        }
    }
    
    /// <summary>
    /// Contains information about an octree node for rendering purposes.
    /// </summary>
    public class OctreeNodeInfo
    {
        /// <summary>
        /// Gets the bounding box of the node.
        /// </summary>
        public AAABBB BoundingBox { get; }
        
        /// <summary>
        /// Gets the depth of the node in the octree.
        /// </summary>
        public int Depth { get; }
        
        /// <summary>
        /// Gets whether the node contains particles.
        /// </summary>
        public bool HasParticles { get; }
        
        /// <summary>
        /// Gets the Morton code of the node.
        /// </summary>
        public ulong MortonCode { get; }
        
        /// <summary>
        /// Initializes a new instance of the OctreeNodeInfo class.
        /// </summary>
        public OctreeNodeInfo(AAABBB boundingBox, int depth, bool hasParticles, ulong mortonCode)
        {
            BoundingBox = boundingBox;
            Depth = depth;
            HasParticles = hasParticles;
            MortonCode = mortonCode;
        }
    }
}
