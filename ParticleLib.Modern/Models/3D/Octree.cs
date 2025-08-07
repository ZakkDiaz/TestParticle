using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Extensions.ObjectPool;

namespace ParticleLib.Modern.Models._3D
{
    /// <summary>
    /// High-performance octree with per-node locking and pooled leaves.
    /// </summary>
    public sealed class Octree : IDisposable
    {
        private readonly OctreeNode _root;
        private readonly ConcurrentDictionary<NodeKey, int> _nodeIndices = new();
        private readonly ConcurrentDictionary<int, List<int>> _leafParticles = new();
        private readonly List<OctreeNode> _nodes = new();
        private readonly List<Point3D> _particles = new();
        private readonly List<int> _particleLeaf = new();
        private readonly List<NodeSpinLock> _nodeLocks = new();

        private static readonly ObjectPool<List<int>> _listPool =
            new DefaultObjectPoolProvider { MaximumRetained = 1024 }.Create<List<int>>();

        private readonly object _particleArraysLock = new object();

        // --- config ---
        private readonly int _maxParticlesPerLeaf;
        private readonly int _maxDepth;

        private readonly ReaderWriterLockSlim _treeLock = new();

        private readonly ConcurrentQueue<int> _particlesToReflow = new();
        private readonly ConcurrentDictionary<int, NodeType> _particleNodeTypes = new();

        public AAABBB Bounds => _root.BoundingBox;
        public int ParticleCount => _particles.Count;
        public int NodeCount => _nodes.Count;

        public Octree(AAABBB bounds, int maxParticlesPerLeaf = 10, int maxDepth = 27)
        {
            _maxParticlesPerLeaf = maxParticlesPerLeaf;
            _maxDepth = maxDepth;

            _root = OctreeNode.CreateRoot(bounds);
            _nodes.Add(_root);
            _nodeIndices[new NodeKey(_root.MortonCode, 0)] = 0;
            _nodeLocks.Add(new NodeSpinLock());

            _leafParticles[0] = _listPool.Get();
        }

        // --------------- public API ---------------

        public int AddParticle(Point3D position)
        {
            if (!Bounds.Contains(position))
                throw new ArgumentOutOfRangeException(nameof(position));

            int idx;
            lock (_particleArraysLock)
            {
                idx = _particles.Count;
                _particles.Add(position);
                _particleLeaf.Add(-1);
            }
            InsertParticle(idx, position);
            return idx;
        }

        public int[] AddParticles(IEnumerable<Point3D> positions)
        {
            var arr = positions.ToArray();
            var outIdx = new int[arr.Length];
            for (int i = 0; i < arr.Length; i++)
                outIdx[i] = AddParticle(arr[i]);
            ProcessParticleReflow();
            return outIdx;
        }

        public void UpdateParticle(int id, Point3D pos)
        {
            if (!Bounds.Contains(pos)) throw new ArgumentOutOfRangeException(nameof(pos));
            Point3D old;
            lock (_particleArraysLock)
            {
                old = _particles[id];
                _particles[id] = pos;
            }
            if (!IsInSameLeaf(old, pos)) _particlesToReflow.Enqueue(id);
        }

        public void RemoveParticle(int id)
        {
            int leaf = _particleLeaf[id];
            _nodeLocks[leaf].Enter();
            try
            {
                if (_leafParticles.TryGetValue(leaf, out var list))
                    list.Remove(id);
            }
            finally { _nodeLocks[leaf].Exit(); }

            lock (_particleArraysLock)
            {
                _particles[id] = new Point3D(float.NaN, float.NaN, float.NaN);
            }
            _particleNodeTypes.TryRemove(id, out _);
        }

        public Point3D[] GetAllParticles()
        {
            lock (_particleArraysLock)
            {
                return _particles.ToArray();
            }
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

                foreach (int leaf in leaves)
                {
                    if (!_leafParticles.TryGetValue(leaf, out var list)) continue;
                    SimdSearch.AppendHitsInRadius(center, radius, list, _particles, result);
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

                foreach (int leaf in leaves)
                {
                    if (!_leafParticles.TryGetValue(leaf, out var list)) continue;
                    foreach (int pi in list)
                    {
                        var p = _particles[pi];
                        if (!float.IsNaN(p.X) && box.Contains(p))
                            result.Add(pi);
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

                int oldLeaf = _particleLeaf[pi];

                _nodeLocks[oldLeaf].Enter();
                try
                {
                    if (_leafParticles.TryGetValue(oldLeaf, out var list))
                        list.Remove(pi);
                }
                finally { _nodeLocks[oldLeaf].Exit(); }

                InsertParticle(pi, _particles[pi]);
            }
        }

        public void Clear()
        {
            _treeLock.EnterWriteLock();
            try
            {
                // return every leaf list to the pool
                foreach (var kv in _leafParticles)
                {
                    kv.Value.Clear();
                    _listPool.Return(kv.Value);
                }
                _leafParticles.Clear();

                _nodeIndices.Clear();
                _nodes.Clear();
                _nodeLocks.Clear();
                _particles.Clear();
                _particleLeaf.Clear();
                _particlesToReflow.Clear();
                _particleNodeTypes.Clear();

                // re-add root
                _nodes.Add(_root);
                _nodeIndices[new NodeKey(_root.MortonCode, 0)] = 0;
                _nodeLocks.Add(new NodeSpinLock());
                _leafParticles[0] = _listPool.Get();
            }
            finally { _treeLock.ExitWriteLock(); }
        }


        // ------------- internal helpers -------------

        private void InsertParticle(int id, Point3D pos)
        {
            int current = 0;
            while (true)
            {
                var node = _nodes[current];
                if (node.Depth >= _maxDepth)
                {
                    _nodeLocks[current].Enter();
                    AddParticleToLeaf(current, id);
                    _nodeLocks[current].Exit();
                    return;
                }

                byte oct = node.GetChildOctantForPoint(pos);
                if (TryGetChildNode(current, oct, out int child))
                {
                    current = child;
                    continue;
                }

                _nodeLocks[current].Enter();
                try
                {
                    if (TryGetChildNode(current, oct, out child))
                    {
                        current = child;
                        continue;
                    }
                    if (_leafParticles.TryGetValue(current, out var lst) &&
                        lst.Count >= _maxParticlesPerLeaf)
                        SplitLeaf(current);

                    if (!TryGetChildNode(current, oct, out child))
                    {
                        var newChild = node.CreateChild(oct);
                        child = AddNode(newChild);
                    }
                    current = child;
                }
                finally { _nodeLocks[current].Exit(); }
            }
        }

        private void AddParticleToLeaf(int nodeIdx, int pid)
        {
            var list = _leafParticles.GetOrAdd(nodeIdx, _ => _listPool.Get());
            list.Add(pid);
            _particleLeaf[pid] = nodeIdx;
            _particleNodeTypes[pid] = NodeType.Leaf;
        }
        private void SplitLeaf(int nodeIdx)
        {
            List<int> parts;
            _nodeLocks[nodeIdx].Enter();
            try
            {
                if (!_leafParticles.TryRemove(nodeIdx, out parts)) return;
            }
            finally { _nodeLocks[nodeIdx].Exit(); }

            if (parts.Count == 0) { _listPool.Return(parts); return; }

            var parent = _nodes[nodeIdx];
            var byOct = new Dictionary<byte, List<int>>();

            foreach (int pid in parts)
            {
                Point3D p;
                lock (_particleArraysLock)            // NEW: protect _particles[pid]
                    p = _particles[pid];

                byte oct = parent.GetChildOctantForPoint(p);
                if (!byOct.TryGetValue(oct, out var list))
                    byOct[oct] = list = _listPool.Get();
                list.Add(pid);
            }
            parts.Clear(); _listPool.Return(parts);

            foreach (var kv in byOct)
            {
                byte oct = kv.Key;
                var octParts = kv.Value;

                if (!TryGetChildNode(nodeIdx, oct, out int childIdx))
                {
                    var child = parent.CreateChild(oct);
                    childIdx = AddNode(child);
                }

                var clist = _leafParticles.GetOrAdd(childIdx, _ => _listPool.Get());
                clist.AddRange(octParts);

                foreach (int pid in octParts) { _particleLeaf[pid] = childIdx; }

                octParts.Clear(); _listPool.Return(octParts);
            }
        }

        private int AddNode(OctreeNode n)
        {
            int idx = _nodes.Count;
            _nodes.Add(n);
            _nodeIndices[new NodeKey(n.MortonCode, n.Depth)] = idx;
            _nodeLocks.Add(new NodeSpinLock());
            return idx;
        }

        private bool TryGetChildNode(int parentIdx, byte oct, out int childIdx)
        {
            var parent = _nodes[parentIdx];
            ulong code = (parent.MortonCode << 3) | oct;
            var key = new NodeKey(code, (byte)(parent.Depth + 1));
            return _nodeIndices.TryGetValue(key, out childIdx);
        }

        private void FindIntersectingLeaves(OctreeNode n, AAABBB box, List<int> res)
        {
            int idx = _nodeIndices[new NodeKey(n.MortonCode, n.Depth)];
            if (!n.BoundingBox.Intersects(box)) return;

            if (_leafParticles.ContainsKey(idx)) res.Add(idx);

            for (byte o = 0; o < 8; o++)
                if (TryGetChildNode(idx, o, out int child))
                    FindIntersectingLeaves(_nodes[child], box, res);
        }

        private bool IsInSameLeaf(Point3D a, Point3D b)
        {
            ulong ma = MortonCode.Encode(a, Bounds);
            ulong mb = MortonCode.Encode(b, Bounds);
            return MortonCode.CommonPrefixLength(ma, mb) >= 3 * _maxDepth;
        }

        public void Dispose()
        {
            _treeLock.Dispose();
        }
    }

    // ---------- tiny spin lock for per-node writes ----------
    internal struct NodeSpinLock
    {
        private int _state;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter() { var s = new SpinWait(); while (Interlocked.CompareExchange(ref _state, 1, 0) != 0) s.SpinOnce(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() { Volatile.Write(ref _state, 0); }
    }
}
