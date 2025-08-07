using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;           


namespace ParticleLib.Modern.Models._3D
{
    /// <summary>
    /// A high-performance octree using Morton codes for fast spatial queries.
    /// </summary>
    public sealed class Octree : IDisposable
    {
        // Core data
        private readonly OctreeNode _root;
        private readonly ConcurrentDictionary<ulong, int> _nodeIndices = new();
        private readonly ConcurrentDictionary<int, List<int>> _leafParticles = new();
        private readonly List<OctreeNode> _nodes = new();
        private readonly List<Point3D> _particles = new();

        // Configuration
        private readonly int _maxParticlesPerLeaf;
        private readonly int _maxDepth;

        // Sync
        private readonly ReaderWriterLockSlim _treeLock = new();
        private readonly SemaphoreSlim _particleAddSemaphore = new(1, 1);

        // Re-flow support
        private readonly ConcurrentQueue<int> _particlesToReflow = new();
        private readonly ConcurrentDictionary<int, NodeType> _particleNodeTypes = new();

        public AAABBB Bounds => _root.BoundingBox;
        public int ParticleCount => _particles.Count;
        public int NodeCount => _nodes.Count;

        public Octree(AAABBB bounds, int maxParticlesPerLeaf = 16, int maxDepth = 8)
        {
            _maxParticlesPerLeaf = maxParticlesPerLeaf;
            _maxDepth = maxDepth;

            _root = OctreeNode.CreateRoot(bounds);
            _nodes.Add(_root);
            _nodeIndices[_root.MortonCode] = 0;

            _leafParticles[0] = new List<int>(); 
        }


        #region Public API -----------------------------------------------------

        public int AddParticle(Point3D position)
        {
            if (!Bounds.Contains(position))
                throw new ArgumentOutOfRangeException(nameof(position), "Particle is outside octree bounds.");

            _treeLock.EnterWriteLock();
            try
            {
                int index = _particles.Count;
                _particles.Add(position);
                InsertParticle(index, position);
                return index;
            }
            finally { _treeLock.ExitWriteLock(); }
        }

        public int[] AddParticles(IEnumerable<Point3D> positions)
        {
            var list = positions.ToList();
            var indices = new int[list.Count];

            // Sequential insert is faster than parallel under the global write-lock.
            for (int i = 0; i < list.Count; i++)
                indices[i] = AddParticle(list[i]);

            ProcessParticleReflow();
            return indices;
        }

        public void UpdateParticle(int particleIndex, Point3D newPosition)
        {
            if (!Bounds.Contains(newPosition))
                throw new ArgumentOutOfRangeException(nameof(newPosition), "New position is outside octree bounds.");

            _treeLock.EnterWriteLock();
            try
            {
                Point3D oldPos = _particles[particleIndex];
                _particles[particleIndex] = newPosition;

                if (!IsInSameLeaf(oldPos, newPosition))
                    _particlesToReflow.Enqueue(particleIndex);
            }
            finally { _treeLock.ExitWriteLock(); }
        }

        public void RemoveParticle(int particleIndex)
        {
            _treeLock.EnterWriteLock();
            try
            {
                RemoveParticleFromLeaves(particleIndex);
                _particles[particleIndex] = new Point3D(float.NaN, float.NaN, float.NaN);
                _particleNodeTypes.TryRemove(particleIndex, out _);
            }
            finally { _treeLock.ExitWriteLock(); }
        }

        public Point3D[] GetAllParticles()
        {
            _treeLock.EnterReadLock();
            try { return _particles.ToArray(); }
            finally { _treeLock.ExitReadLock(); }
        }

        public int[] GetParticlesInRadius(Point3D center, float radius)
        {
            var result = new List<int>();
            float r2 = radius * radius;

            _treeLock.EnterReadLock();
            try
            {
                var box = new AAABBB(
                    new Point3D(center.X - radius, center.Y - radius, center.Z - radius),
                    new Point3D(center.X + radius, center.Y + radius, center.Z + radius));

                var leaves = new List<int>();
                FindIntersectingLeaves(_root, box, leaves);

                foreach (var leafIndex in leaves)
                {
                    if (!_leafParticles.TryGetValue(leafIndex, out var parts)) continue;
                    foreach (var pi in parts)
                    {
                        var p = _particles[pi];
                        if (float.IsNaN(p.X)) continue;
                        if (Point3D.DistanceSquared(center, p) <= r2) result.Add(pi);
                    }
                }
            }
            finally { _treeLock.ExitReadLock(); }

            return result.ToArray();
        }

        public int[] GetParticlesInBox(AAABBB box)
        {
            var result = new List<int>();

            _treeLock.EnterReadLock();
            try
            {
                var leaves = new List<int>();
                FindIntersectingLeaves(_root, box, leaves);

                foreach (var leafIndex in leaves)
                {
                    if (!_leafParticles.TryGetValue(leafIndex, out var parts)) continue;
                    foreach (var pi in parts)
                    {
                        var p = _particles[pi];
                        if (float.IsNaN(p.X)) continue;
                        if (box.Contains(p)) result.Add(pi);
                    }
                }
            }
            finally { _treeLock.ExitReadLock(); }

            return result.ToArray();
        }

        public int GetDepth() { _treeLock.EnterReadLock(); try { return _nodes.Max(n => n.Depth); } finally { _treeLock.ExitReadLock(); } }
        public AAABBB[] GetBoxCloud() { _treeLock.EnterReadLock(); try { return _nodes.Select(n => n.BoundingBox).ToArray(); } finally { _treeLock.ExitReadLock(); } }

        public void ProcessParticleReflow()
        {
            while (_particlesToReflow.TryDequeue(out int pi))
            {
                if (float.IsNaN(_particles[pi].X)) continue;

                _particleAddSemaphore.Wait();
                try
                {
                    RemoveParticleFromLeaves(pi);
                    InsertParticle(pi, _particles[pi]);
                }
                finally { _particleAddSemaphore.Release(); }
            }
        }

        public void Clear()
        {
            _treeLock.EnterWriteLock();
            try
            {
                _nodeIndices.Clear();
                _leafParticles.Clear();
                _particles.Clear();
                _nodes.Clear();
                _particlesToReflow.Clear();
                _particleNodeTypes.Clear();

                _nodes.Add(_root);
                _nodeIndices[_root.MortonCode] = 0;
                _leafParticles[0] = new List<int>();  
            }
            finally { _treeLock.ExitWriteLock(); }
        }


        public void Dispose()
        {
            _treeLock.Dispose();
            _particleAddSemaphore.Dispose();
        }

        #endregion

        #region Private implementation ----------------------------------------

        private void InsertParticle(int particleIndex, Point3D position)
        {
            var sw = OctreeDebug.Enabled ? Stopwatch.StartNew() : null;

            int currentNodeIndex = 0;
            OctreeNode currentNode = _nodes[currentNodeIndex];

            while (true)
            {
                if (currentNode.Depth >= _maxDepth)
                {
                    AddParticleToLeaf(currentNodeIndex, particleIndex);
                    return;
                }

                byte octant = currentNode.GetChildOctantForPoint(position);

                // Walk to existing child if present
                if (TryGetChildNode(currentNodeIndex, octant, out int childIdx))
                {
                    currentNodeIndex = childIdx;
                    currentNode = _nodes[childIdx];
                    continue;
                }

                // If this node is still a leaf, see if we can add directly
                if (_leafParticles.TryGetValue(currentNodeIndex, out var partList))
                {
                    if (partList.Count < _maxParticlesPerLeaf)
                    {
                        OctreeDebug.Log($"Add p{particleIndex} to leaf {currentNodeIndex}");
                        AddParticleToLeaf(currentNodeIndex, particleIndex);
                        sw?.Stop(); OctreeDebug.Log($"  - done in {sw?.ElapsedTicks ?? 0} ticks");
                        return;
                    }

                    // Need to split
                    OctreeDebug.Log($"Split leaf {currentNodeIndex} (depth {currentNode.Depth})");
                    SplitLeaf(currentNodeIndex);

                }

                // After split or if not a leaf, create child and continue
                OctreeDebug.Log($"Create child of {currentNodeIndex} in octant {octant}");
                OctreeNode child = currentNode.CreateChild(octant);
                childIdx = AddNode(child);

                currentNodeIndex = childIdx;
                currentNode = child;
            }
        }

        private void AddParticleToLeaf(int nodeIndex, int particleIndex)
        {
            var list = _leafParticles.GetOrAdd(nodeIndex, _ => new List<int>());
            list.Add(particleIndex);
            _particleNodeTypes[particleIndex] = NodeType.Leaf;
        }

        private void RemoveParticleFromLeaves(int particleIndex)
        {
            foreach (var kv in _leafParticles)
            {
                if (kv.Value.Remove(particleIndex))
                    break;
            }
        }

        private void SplitLeaf(int nodeIndex)
        {
            if (!_leafParticles.TryRemove(nodeIndex, out var particles) || particles.Count == 0)
                return;

            var node = _nodes[nodeIndex];
            var byOctant = new Dictionary<byte, List<int>>();

            foreach (var pi in particles)
            {
                byte o = node.GetChildOctantForPoint(_particles[pi]);
                if (!byOctant.TryGetValue(o, out var list))
                {
                    list = new List<int>();
                    byOctant[o] = list;
                }
                list.Add(pi);
            }

            foreach (var kv in byOctant)
            {
                byte oct = kv.Key;
                List<int> octParts = kv.Value;

                if (!TryGetChildNode(nodeIndex, oct, out int childIdx))
                {
                    var child = node.CreateChild(oct);
                    childIdx = AddNode(child);
                }

                var childList = _leafParticles.GetOrAdd(childIdx, _ => new List<int>());
                childList.AddRange(octParts);

                foreach (var pi in octParts)
                    _particleNodeTypes[pi] = NodeType.Leaf;
            }
        }

        private int AddNode(OctreeNode node)
        {
            int idx = _nodes.Count;
            _nodes.Add(node);
            _nodeIndices[node.MortonCode] = idx;
            return idx;
        }

        private bool TryGetChildNode(int parentIdx, byte octant, out int childIdx)
        {
            var parent = _nodes[parentIdx];
            byte newDepth = (byte)(parent.Depth + 1);

            ulong childCode = (parent.MortonCode << 3) | octant;
            childCode |= 1UL << (3 * newDepth);

            return _nodeIndices.TryGetValue(childCode, out childIdx);
        }



        private void FindIntersectingLeaves(OctreeNode node, AAABBB box, List<int> result)
        {
            int idx = _nodeIndices[node.MortonCode];
            if (!node.BoundingBox.Intersects(box)) return;

            if (_leafParticles.ContainsKey(idx))
                result.Add(idx);

            for (byte o = 0; o < 8; o++)
                if (TryGetChildNode(idx, o, out int child))
                    FindIntersectingLeaves(_nodes[child], box, result);
        }

        private bool IsInSameLeaf(Point3D a, Point3D b)
        {
            ulong ma = MortonCode.Encode(a, Bounds);
            ulong mb = MortonCode.Encode(b, Bounds);
            int cp = MortonCode.CommonPrefixLength(ma, mb);
            return cp >= 3 * _maxDepth;
        }

        #endregion
    }
}
