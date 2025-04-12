using ParticleLib.Modern.Models;
using ParticleLib.Modern.Rendering;
using System.Numerics;

namespace ParticleLib.Modern.Examples
{
    /// <summary>
    /// A simple console-based renderer for particle simulations.
    /// </summary>
    public class ConsoleParticleRenderer : IParticleRenderer
    {
        private int _particleCount = 0;
        private int _nodeCount = 0;
        private int _maxDepth = 0;
        private int _framesRendered = 0;
        private DateTime _lastFrameTime = DateTime.UtcNow;
        private readonly List<double> _frameRates = new();
        
        /// <summary>
        /// Begins a new rendering frame.
        /// </summary>
        public void BeginFrame()
        {
            _particleCount = 0;
            _nodeCount = 0;
            _maxDepth = 0;
        }
        
        /// <summary>
        /// Renders a particle at the specified position with the given velocity.
        /// </summary>
        public void RenderParticle(Point3D position, Vector3 velocity, Vector4? color = null)
        {
            _particleCount++;
        }
        
        /// <summary>
        /// Renders an octree node's bounding box.
        /// </summary>
        public void RenderOctreeNode(AAABBB bounds, int depth, bool hasParticles)
        {
            _nodeCount++;
            _maxDepth = Math.Max(_maxDepth, depth);
        }
        
        /// <summary>
        /// Ends the current rendering frame.
        /// </summary>
        public void EndFrame()
        {
            _framesRendered++;
            
            // Calculate frame rate
            DateTime now = DateTime.UtcNow;
            double frameTime = (now - _lastFrameTime).TotalSeconds;
            _lastFrameTime = now;
            
            if (frameTime > 0)
            {
                _frameRates.Add(1.0 / frameTime);
                
                // Keep only the last 60 frame rates for averaging
                if (_frameRates.Count > 60)
                {
                    _frameRates.RemoveAt(0);
                }
            }
            
            // Only update the console every 30 frames to avoid flickering
            if (_framesRendered % 30 == 0)
            {
                Console.Clear();
                Console.WriteLine("Particle Simulation Statistics");
                Console.WriteLine("==============================");
                Console.WriteLine($"Particles: {_particleCount}");
                Console.WriteLine($"Octree Nodes: {_nodeCount}");
                Console.WriteLine($"Max Depth: {_maxDepth}");
                Console.WriteLine($"FPS: {_frameRates.Average():F1}");
                Console.WriteLine("\nPress Escape to exit");
            }
        }
    }
}
