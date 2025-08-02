using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ParticleLib.Modern.Models._3D
{
    /// <summary>
    /// A high-performance octree implementation for spatial partitioning and queries
    /// using Morton codes (Z-order curves) for efficient O(1) sector lookups.
    /// </summary>
    public sealed class Octree : IDisposable
    {
        // Core octree data structures
        private readonly OctreeNode _root;
        private readonly ConcurrentDictionary<ulong, int> _nodeIndices = new();
        private readonly ConcurrentDictionary<int, List<int>> _leafParticles = new();
        private readonly List<OctreeNode> _nodes = new();
        private readonly List<Point3D> _particles = new();
        
        // Configuration
        private readonly int _maxParticlesPerLeaf;
        private readonly int _maxDepth;
        
        // Thread synchronization
        private readonly ReaderWriterLockSlim _treeLock = new();
        private readonly SemaphoreSlim _particleAddSemaphore = new(1, 1);
        
        // Particle reflow tracking
        private readonly ConcurrentQueue<int> _particlesToReflow = new();
        private readonly ConcurrentDictionary<int, NodeType> _particleNodeTypes = new();

        /// <summary>
        /// Gets the bounds of this octree
        /// </summary>
        public AAABBB Bounds => _root.BoundingBox;

        /// <summary>
        /// Gets the number of particles in this octree
        /// </summary>
        public int ParticleCount => _particles.Count;

        /// <summary>
        /// Gets the number of nodes in this octree
        /// </summary>
        public int NodeCount => _nodes.Count;

        /// <summary>
        /// Creates a new octree with the specified bounds
        /// </summary>
        /// <param name="bounds">The bounds of the octree</param>
        /// <param name="maxParticlesPerLeaf">Maximum particles per leaf node before splitting</param>
        /// <param name="maxDepth">Maximum depth of the octree</param>
        public Octree(AAABBB bounds, int maxParticlesPerLeaf = 16, int maxDepth = 8)
        {
            _maxParticlesPerLeaf = maxParticlesPerLeaf;
            _maxDepth = maxDepth;
            
            // Create the root node
            _root = OctreeNode.CreateRoot(bounds);
            _nodes.Add(_root);
            _nodeIndices[_root.MortonCode] = 0;
        }

        /// <summary>
        /// Adds a particle to the octree
        /// </summary>
        /// <param name="position">The position of the particle</param>
        /// <returns>The index of the added particle</returns>
        public int AddParticle(Point3D position)
        {
            // Check if the particle is within bounds
            if (!Bounds.Contains(position))
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Particle position is outside the octree bounds");
            }

            // Add the particle to the octree
            _treeLock.EnterWriteLock();
            try
            {
                // Add the particle to the list
                int particleIndex = _particles.Count;
                _particles.Add(position);

                // Insert the particle into the tree
                InsertParticle(particleIndex, position);

                return particleIndex;
            }
            finally
            {
                _treeLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Adds multiple particles to the octree in parallel
        /// </summary>
        /// <param name="positions">The positions of the particles</param>
        /// <returns>The indices of the added particles</returns>
        public int[] AddParticles(IEnumerable<Point3D> positions)
        {
            var positionsList = positions.ToList();
            var indices = new int[positionsList.Count];

            // Add particles in parallel
            Parallel.For(0, positionsList.Count, i =>
            {
                indices[i] = AddParticle(positionsList[i]);
            });

            // Process any particles that need to be reflowed
            ProcessParticleReflow();

            return indices;
        }

        /// <summary>
        /// Updates the position of a particle
        /// </summary>
        /// <param name="particleIndex">The index of the particle</param>
        /// <param name="newPosition">The new position</param>
        public void UpdateParticle(int particleIndex, Point3D newPosition)
        {
            // Check if the particle is within bounds
            if (!Bounds.Contains(newPosition))
            {
                throw new ArgumentOutOfRangeException(nameof(newPosition), "New particle position is outside the octree bounds");
            }

            // Update the particle position
            _treeLock.EnterWriteLock();
            try
            {
                // Get the old position
                Point3D oldPosition = _particles[particleIndex];

                // Update the position
                _particles[particleIndex] = newPosition;

                // If the position has changed significantly, we need to reflow the particle
                if (!IsInSameLeaf(oldPosition, newPosition))
                {
                    // Queue the particle for reflow
                    _particlesToReflow.Enqueue(particleIndex);
                }
            }
            finally
            {
                _treeLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes a particle from the octree
        /// </summary>
        /// <param name="particleIndex">The index of the particle to remove</param>
        public void RemoveParticle(int particleIndex)
        {
            _treeLock.EnterWriteLock();
            try
            {
                // Find the leaf node containing the particle
                foreach (var leafEntry in _leafParticles)
                {
                    if (leafEntry.Value.Contains(particleIndex))
                    {
                        // Remove the particle from the leaf
                        leafEntry.Value.Remove(particleIndex);
                        break;
                    }
                }

                // Mark the particle as removed (we don't actually remove it from the list to avoid reindexing)
                _particles[particleIndex] = new Point3D(float.NaN, float.NaN, float.NaN);
                _particleNodeTypes.TryRemove(particleIndex, out _);
            }
            finally
            {
                _treeLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets all particles in the octree
        /// </summary>
        /// <returns>An array of particle positions</returns>
        public Point3D[] GetAllParticles()
        {
            _treeLock.EnterReadLock();
            try
            {
                return _particles.ToArray();
            }
            finally
            {
                _treeLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets all particles within the specified radius of a point
        /// </summary>
        /// <param name="center">The center point</param>
        /// <param name="radius">The radius</param>
        /// <returns>An array of particle indices within the radius</returns>
        public int[] GetParticlesInRadius(Point3D center, float radius)
        {
            var result = new List<int>();
            var radiusSquared = radius * radius;

            _treeLock.EnterReadLock();
            try
            {
                // Create a bounding box for the search area
                var searchMin = new Point3D(center.X - radius, center.Y - radius, center.Z - radius);
                var searchMax = new Point3D(center.X + radius, center.Y + radius, center.Z + radius);
                var searchBox = new AAABBB(searchMin, searchMax);

                // Find all leaf nodes that intersect with the search box
                var intersectingLeaves = new List<int>();
                FindIntersectingLeaves(_root, searchBox, intersectingLeaves);

                // Check each particle in the intersecting leaves
                foreach (var leafIndex in intersectingLeaves)
                {
                    if (_leafParticles.TryGetValue(leafIndex, out var particles))
                    {
                        foreach (var particleIndex in particles)
                        {
                            var particlePos = _particles[particleIndex];
                            
                            // Skip removed particles
                            if (float.IsNaN(particlePos.X))
                                continue;
                                
                            // Check if the particle is within the radius
                            if (Point3D.DistanceSquared(center, particlePos) <= radiusSquared)
                            {
                                result.Add(particleIndex);
                            }
                        }
                    }
                }
            }
            finally
            {
                _treeLock.ExitReadLock();
            }

            return result.ToArray();
        }

        /// <summary>
        /// Gets all particles within a bounding box
        /// </summary>
        /// <param name="box">The bounding box</param>
        /// <returns>An array of particle indices within the box</returns>
        public int[] GetParticlesInBox(AAABBB box)
        {
            var result = new List<int>();

            _treeLock.EnterReadLock();
            try
            {
                // Find all leaf nodes that intersect with the box
                var intersectingLeaves = new List<int>();
                FindIntersectingLeaves(_root, box, intersectingLeaves);

                // Check each particle in the intersecting leaves
                foreach (var leafIndex in intersectingLeaves)
                {
                    if (_leafParticles.TryGetValue(leafIndex, out var particles))
                    {
                        foreach (var particleIndex in particles)
                        {
                            var particlePos = _particles[particleIndex];
                            
                            // Skip removed particles
                            if (float.IsNaN(particlePos.X))
                                continue;
                                
                            // Check if the particle is within the box
                            if (box.Contains(particlePos))
                            {
                                result.Add(particleIndex);
                            }
                        }
                    }
                }
            }
            finally
            {
                _treeLock.ExitReadLock();
            }

            return result.ToArray();
        }

        /// <summary>
        /// Gets the depth of the octree
        /// </summary>
        /// <returns>The maximum depth of any node in the tree</returns>
        public int GetDepth()
        {
            _treeLock.EnterReadLock();
            try
            {
                return _nodes.Max(n => n.Depth);
            }
            finally
            {
                _treeLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets all the bounding boxes in the octree for visualization
        /// </summary>
        /// <returns>An array of bounding boxes</returns>
        public AAABBB[] GetBoxCloud()
        {
            _treeLock.EnterReadLock();
            try
            {
                return _nodes.Select(n => n.BoundingBox).ToArray();
            }
            finally
            {
                _treeLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Processes any particles that need to be reflowed
        /// </summary>
        public void ProcessParticleReflow()
        {
            // Process particles in the reflow queue
            while (_particlesToReflow.TryDequeue(out int particleIndex))
            {
                // Skip removed particles
                if (float.IsNaN(_particles[particleIndex].X))
                    continue;

                // Reflow the particle
                _particleAddSemaphore.Wait();
                try
                {
                    // Remove the particle from its current leaf
                    RemoveParticleFromLeaves(particleIndex);

                    // Re-insert the particle
                    InsertParticle(particleIndex, _particles[particleIndex]);
                }
                finally
                {
                    _particleAddSemaphore.Release();
                }
            }
        }

        /// <summary>
        /// Clears the octree, removing all particles
        /// </summary>
        public void Clear()
        {
            _treeLock.EnterWriteLock();
            try
            {
                // Clear all data structures
                _nodeIndices.Clear();
                _leafParticles.Clear();
                _particles.Clear();
                _nodes.Clear();
                _particlesToReflow.Clear();
                _particleNodeTypes.Clear();

                // Re-add the root node
                _nodes.Add(_root);
                _nodeIndices[_root.MortonCode] = 0;
            }
            finally
            {
                _treeLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Disposes the octree, releasing all resources
        /// </summary>
        public void Dispose()
        {
            _treeLock.Dispose();
            _particleAddSemaphore.Dispose();
        }

        #region Private Implementation

        /// <summary>
        /// Inserts a particle into the octree
        /// </summary>
        private void InsertParticle(int particleIndex, Point3D position)
        {
            // Start at the root node
            int currentNodeIndex = 0;
            OctreeNode currentNode = _nodes[currentNodeIndex];

            // Traverse the tree to find the leaf node
            while (true)
            {
                // If we've reached the maximum depth, stop here
                if (currentNode.Depth >= _maxDepth)
                {
                    AddParticleToLeaf(currentNodeIndex, particleIndex);
                    return;
                }

                // Get the octant for this particle
                byte octant = currentNode.GetChildOctantForPoint(position);

                // Check if the child node exists
                int childNodeIndex;
                if (TryGetChildNode(currentNodeIndex, octant, out childNodeIndex))
                {
                    // Continue traversal with the child node
                    currentNodeIndex = childNodeIndex;
                    currentNode = _nodes[currentNodeIndex];
                }
                else
                {
                    // Create a new child node
                    OctreeNode childNode = currentNode.CreateChild(octant);
                    childNodeIndex = AddNode(childNode);

                    // Add the particle to the new leaf
                    AddParticleToLeaf(childNodeIndex, particleIndex);
                    return;
                }

                // If this is a leaf node with particles
                if (_leafParticles.TryGetValue(currentNodeIndex, out var particles))
                {
                    // If the leaf has room for more particles, add it here
                    if (particles.Count < _maxParticlesPerLeaf)
                    {
                        AddParticleToLeaf(currentNodeIndex, particleIndex);
                        return;
                    }
                    
                    // Otherwise, we need to split this leaf
                    if (currentNode.Depth < _maxDepth)
                    {
                        // Split the leaf by redistributing its particles
                        SplitLeaf(currentNodeIndex);
                        
                        // Continue traversal to place this particle
                        continue;
                    }
                    
                    // If we've reached the maximum depth, add it to this leaf anyway
                    AddParticleToLeaf(currentNodeIndex, particleIndex);
                    return;
                }
                else
                {
                    // This is a new leaf, add the particle here
                    AddParticleToLeaf(currentNodeIndex, particleIndex);
                    return;
                }
            }
        }

        /// <summary>
        /// Adds a particle to a leaf node
        /// </summary>
        private void AddParticleToLeaf(int nodeIndex, int particleIndex)
        {
            // Get or create the particle list for this leaf
            var particles = _leafParticles.GetOrAdd(nodeIndex, _ => new List<int>());
            
            // Add the particle to the leaf
            particles.Add(particleIndex);
            
            // Track the node type for this particle
            _particleNodeTypes[particleIndex] = NodeType.Leaf;
        }

        /// <summary>
        /// Removes a particle from all leaf nodes
        /// </summary>
        private void RemoveParticleFromLeaves(int particleIndex)
        {
            foreach (var leafEntry in _leafParticles)
            {
                if (leafEntry.Value.Contains(particleIndex))
                {
                    leafEntry.Value.Remove(particleIndex);
                    break;
                }
            }
        }

        /// <summary>
        /// Splits a leaf node by redistributing its particles
        /// </summary>
        private void SplitLeaf(int nodeIndex)
        {
            // Get the particles in this leaf
            if (!_leafParticles.TryGetValue(nodeIndex, out var particles) || particles.Count == 0)
                return;

            // Get the node
            var node = _nodes[nodeIndex];

            // Create child nodes for each octant that contains particles
            var particlesByOctant = new Dictionary<byte, List<int>>();
            
            // Group particles by octant
            foreach (var particleIndex in particles)
            {
                var position = _particles[particleIndex];
                byte octant = node.GetChildOctantForPoint(position);
                
                if (!particlesByOctant.TryGetValue(octant, out var octantParticles))
                {
                    octantParticles = new List<int>();
                    particlesByOctant[octant] = octantParticles;
                }
                
                octantParticles.Add(particleIndex);
            }
            
            // Clear the particles from this node
            particles.Clear();
            
            // Create child nodes and redistribute particles
            foreach (var entry in particlesByOctant)
            {
                byte octant = entry.Key;
                var octantParticles = entry.Value;
                
                // Create the child node if it doesn't exist
                int childNodeIndex;
                if (!TryGetChildNode(nodeIndex, octant, out childNodeIndex))
                {
                    OctreeNode childNode = node.CreateChild(octant);
                    childNodeIndex = AddNode(childNode);
                }
                
                // Add the particles to the child node
                var childParticles = _leafParticles.GetOrAdd(childNodeIndex, _ => new List<int>());
                childParticles.AddRange(octantParticles);
                
                // Update particle node types
                foreach (var particleIndex in octantParticles)
                {
                    _particleNodeTypes[particleIndex] = NodeType.Leaf;
                }
            }
        }

        /// <summary>
        /// Adds a node to the octree
        /// </summary>
        private int AddNode(OctreeNode node)
        {
            // Add the node to the list
            int nodeIndex = _nodes.Count;
            _nodes.Add(node);
            
            // Add the node to the index
            _nodeIndices[node.MortonCode] = nodeIndex;
            
            return nodeIndex;
        }

        /// <summary>
        /// Tries to get a child node by octant
        /// </summary>
        private bool TryGetChildNode(int parentIndex, byte octant, out int childIndex)
        {
            // Get the parent node
            var parentNode = _nodes[parentIndex];
            
            // Calculate the child node's Morton code
            ulong childMortonCode = parentNode.MortonCode | ((ulong)octant << (3 * (21 - parentNode.Depth - 1)));
            
            // Try to get the child node index
            return _nodeIndices.TryGetValue(childMortonCode, out childIndex);
        }

        /// <summary>
        /// Finds all leaf nodes that intersect with a bounding box
        /// </summary>
        private void FindIntersectingLeaves(OctreeNode node, AAABBB box, List<int> result)
        {
            // Get the node index
            int nodeIndex = _nodeIndices[node.MortonCode];
            
            // If this node doesn't intersect the box, skip it
            if (!node.BoundingBox.Intersects(box))
                return;
            
            // If this is a leaf node with particles, add it to the result
            if (_leafParticles.ContainsKey(nodeIndex))
            {
                result.Add(nodeIndex);
            }
            
            // Check all child nodes
            for (byte octant = 0; octant < 8; octant++)
            {
                if (TryGetChildNode(nodeIndex, octant, out int childIndex))
                {
                    FindIntersectingLeaves(_nodes[childIndex], box, result);
                }
            }
        }

        /// <summary>
        /// Checks if two positions would be in the same leaf node
        /// </summary>
        private bool IsInSameLeaf(Point3D a, Point3D b)
        {
            // Calculate the Morton codes for both positions
            ulong mortonA = MortonCode.Encode(a, Bounds);
            ulong mortonB = MortonCode.Encode(b, Bounds);
            
            // Check if they have the same prefix up to the maximum depth
            int commonPrefix = MortonCode.CommonPrefixLength(mortonA, mortonB);
            return commonPrefix >= 3 * _maxDepth;
        }

        #endregion
    }
}
