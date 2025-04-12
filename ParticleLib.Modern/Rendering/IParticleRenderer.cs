using ParticleLib.Modern.Models;
using System.Numerics;

namespace ParticleLib.Modern.Rendering
{
    /// <summary>
    /// Interface for rendering particles and octree structure.
    /// </summary>
    public interface IParticleRenderer
    {
        /// <summary>
        /// Renders a particle at the specified position with the given velocity.
        /// </summary>
        /// <param name="position">The position of the particle.</param>
        /// <param name="velocity">The velocity of the particle.</param>
        /// <param name="color">Optional color for the particle.</param>
        void RenderParticle(Point3D position, Vector3 velocity, Vector4? color = null);
        
        /// <summary>
        /// Renders an octree node's bounding box.
        /// </summary>
        /// <param name="bounds">The bounding box of the node.</param>
        /// <param name="depth">The depth of the node in the octree.</param>
        /// <param name="hasParticles">Whether the node contains particles.</param>
        void RenderOctreeNode(AAABBB bounds, int depth, bool hasParticles);
        
        /// <summary>
        /// Begins a new rendering frame.
        /// </summary>
        void BeginFrame();
        
        /// <summary>
        /// Ends the current rendering frame.
        /// </summary>
        void EndFrame();
    }
}
