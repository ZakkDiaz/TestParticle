using System.Buffers;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ParticleLib.Modern.Models;

/// <summary>
/// A modern implementation of a static octree with Morton codes for O(1) sector lookups.
/// </summary>
public sealed class Octree : IDisposable
{
    // Constants
    private const int DefaultCapacity = 1024;
    private const int MaxPointsPerNode = 8;
    private const int MaxDepth = 21; // Maximum theoretical depth with 64-bit Morton codes (21 levels * 3 bits per level = 63 bits)
    
    // Octree bounds
    private readonly AAABBB _bounds;
    
    // Node storage
    private ConcurrentDictionary<ulong, int> _nodeIndices = new();
    private OctreeNode[] _nodes;
    private int _nodeCount;
    
    // Child node storage (8 children per node)
    private int[] _childrenIndices;
    private int _childrenIndicesCount;
    
    // Point storage
    private readonly ConcurrentDictionary<ulong, List<Point3D>> _pointsByNode = new();
    private int _pointCount = 0; // Track total point count
    
    // Thread synchronization
    private readonly object _nodeLock = new object();
    
    /// <summary>
    /// Gets the bounds of the octree.
    /// </summary>
    public AAABBB Bounds => _bounds;
    
    /// <summary>
    /// Creates a new octree with the specified bounds.
    /// </summary>
    public Octree(Point3D min, Point3D max, int initialCapacity = DefaultCapacity)
    {
        _bounds = new AAABBB(min, max);
        
        // Initialize node storage
        _nodes = new OctreeNode[initialCapacity];
        _childrenIndices = new int[initialCapacity * 8]; // 8 children per node
        
        // Create root node
        _nodes[0] = new OctreeNode(
            0, // Root has Morton code 0
            NodeType.Empty,
            _bounds,
            0, // Depth 0
            0, // No quadrant
            -1 // No children yet
        );
        _nodeCount = 1;
        _nodeIndices[0] = 0; // Root node
    }
    
    /// <summary>
    /// Gets the total number of points in the octree.
    /// </summary>
    public int PointCount => _pointCount;
    
    /// <summary>
    /// Gets the total number of nodes in the octree.
    /// </summary>
    public int NodeCount => _nodeCount;
    
    /// <summary>
    /// Gets the maximum depth of the octree.
    /// </summary>
    public int Depth
    {
        get
        {
            int maxDepth = 0;
            foreach (var node in _nodes.Take(_nodeCount))
            {
                if (node.Depth > maxDepth)
                {
                    maxDepth = node.Depth;
                }
            }
            return maxDepth;
        }
    }
    
    /// <summary>
    /// Adds a point to the octree.
    /// </summary>
    public void Add(float x, float y, float z)
    {
        Add(new Point3D(x, y, z));
    }
    
    /// <summary>
    /// Adds a point to the octree.
    /// </summary>
    public void Add(Point3D point)
    {
        // Check if the point is within the octree bounds
        if (!_bounds.Contains(point))
        {
            throw new ArgumentOutOfRangeException(nameof(point), "Point is outside the octree bounds");
        }
        
        lock (_nodeLock)
        {
            // Start at the root node
            ulong mortonCode = 0;
            
            // If we don't have any nodes yet, create the root node
            if (_nodeCount == 0)
            {
                _nodes[0] = new OctreeNode(0, NodeType.Leaf, _bounds, 0, 0);
                _nodeIndices[0] = 0;
                _nodeCount = 1;
            }
            
            // Find or create the leaf node for this point
            bool added = false;
            int depth = 0;
            
            while (!added && depth < MaxDepth)
            {
                // Get the node index
                if (!_nodeIndices.TryGetValue(mortonCode, out int nodeIndex))
                {
                    // This shouldn't happen, but just in case
                    break;
                }
                
                var node = _nodes[nodeIndex];
                
                // If this is an internal node, traverse down to the appropriate child
                if (node.Type == NodeType.Internal && node.ChildrenIndex >= 0)
                {
                    // Determine which child this point belongs to
                    byte quadrant = DetermineQuadrant(point, node.BoundingBox);
                    
                    // Calculate the child's Morton code
                    ulong childCode = CalculateChildMortonCode(mortonCode, quadrant, node.Depth);
                    
                    // Check if the child node exists
                    int childIndex = -1;
                    if (node.ChildrenIndex >= 0)
                    {
                        childIndex = _childrenIndices[node.ChildrenIndex + quadrant];
                    }
                    
                    // If the child node doesn't exist, create it
                    if (childIndex < 0)
                    {
                        // Create the child node
                        childIndex = CreateChildNode(mortonCode, nodeIndex, quadrant);
                    }
                    
                    // Update the current node to the child
                    mortonCode = childCode;
                    depth++;
                }
                else
                {
                    // Get or create the points list for this node
                    if (!_pointsByNode.TryGetValue(mortonCode, out var points))
                    {
                        points = new List<Point3D>();
                        _pointsByNode[mortonCode] = points;
                    }
                    
                    // Add the point to the list
                    points.Add(point);
                    added = true;
                    
                    // Check if we need to subdivide this node
                    if (points.Count > MaxPointsPerNode && depth < MaxDepth - 1)
                    {
                        // Subdivide the node - this will create child nodes and redistribute points
                        SubdivideNode(mortonCode, nodeIndex, node);
                    }
                    
                    // Increment point count
                    Interlocked.Increment(ref _pointCount);
                }
            }
            
            // If we couldn't add the point to a leaf node, add it to the last node we found
            if (!added)
            {
                // Get or create the points list for this node
                if (!_pointsByNode.TryGetValue(mortonCode, out var points))
                {
                    points = new List<Point3D>();
                    _pointsByNode[mortonCode] = points;
                }
                
                // Add the point to the list
                points.Add(point);
                
                // Increment point count
                Interlocked.Increment(ref _pointCount);
            }
        }
    }
    
    /// <summary>
    /// Adds a point to a node.
    /// </summary>
    private void AddPointToNode(ulong mortonCode, Point3D point)
    {
        // Get or create the points list for this node
        if (!_pointsByNode.TryGetValue(mortonCode, out var points))
        {
            points = new List<Point3D>();
            _pointsByNode[mortonCode] = points;
        }
        
        // Add the point to the list
        points.Add(point);
    }
    
    /// <summary>
    /// Subdivides a node into 8 children.
    /// </summary>
    private void SubdivideNode(ulong mortonCode, int nodeIndex, OctreeNode node)
    {
        // Get the points in this node
        if (!_pointsByNode.TryGetValue(mortonCode, out var points) || points.Count <= MaxPointsPerNode)
        {
            return; // No points to redistribute or not enough points to trigger subdivision
        }
        
        // Check if we've reached the maximum depth
        if (node.Depth >= MaxDepth - 1)
        {
            return; // Don't subdivide further
        }
        
        // Allocate children indices
        int childrenIndex = _childrenIndicesCount;
        _childrenIndicesCount += 8;
        
        // Resize the children indices array if needed
        if (_childrenIndicesCount > _childrenIndices.Length)
        {
            int newSize = Math.Max(_childrenIndicesCount, _childrenIndices.Length * 2);
            Array.Resize(ref _childrenIndices, newSize);
        }
        
        // Initialize all children to -1 (no child)
        for (int i = 0; i < 8; i++)
        {
            _childrenIndices[childrenIndex + i] = -1;
        }
        
        // Convert this node to an internal node
        _nodes[nodeIndex] = new OctreeNode(
            node.MortonCode,
            NodeType.Internal,
            node.BoundingBox,
            node.Depth,
            node.Quadrant,
            childrenIndex
        );
        
        // Create a copy of the points to redistribute
        var pointsToRedistribute = new List<Point3D>(points);
        
        // Clear the points list for this node
        points.Clear();
        
        // Group points by quadrant
        var pointsByQuadrant = new Dictionary<byte, List<Point3D>>();
        
        // First pass: group points by quadrant
        foreach (var p in pointsToRedistribute)
        {
            byte quadrant = DetermineQuadrant(p, node.BoundingBox);
            
            if (!pointsByQuadrant.TryGetValue(quadrant, out var quadrantPoints))
            {
                quadrantPoints = new List<Point3D>();
                pointsByQuadrant[quadrant] = quadrantPoints;
            }
            
            quadrantPoints.Add(p);
        }
        
        // Second pass: create child nodes and distribute points
        foreach (var kvp in pointsByQuadrant)
        {
            byte quadrant = kvp.Key;
            var quadrantPoints = kvp.Value;
            
            // Calculate the child's Morton code
            ulong childCode = CalculateChildMortonCode(mortonCode, quadrant, node.Depth);
            
            // Create the child node
            int childIndex = CreateChildNode(mortonCode, nodeIndex, quadrant);
            
            // Get or create the points list for the child
            if (!_pointsByNode.TryGetValue(childCode, out var childPoints))
            {
                childPoints = new List<Point3D>();
                _pointsByNode[childCode] = childPoints;
            }
            
            // Add all points to the child
            childPoints.AddRange(quadrantPoints);
            
            // Check if we need to subdivide the child
            if (childPoints.Count > MaxPointsPerNode && node.Depth + 1 < MaxDepth - 1)
            {
                SubdivideNode(childCode, childIndex, _nodes[childIndex]);
            }
        }
    }
    
    /// <summary>
    /// Calculates the Morton code for a child node.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong CalculateChildMortonCode(ulong parentMortonCode, byte quadrant, int parentDepth)
    {
        // Shift the quadrant to the appropriate position based on the parent's depth
        return parentMortonCode | ((ulong)quadrant << (3 * parentDepth));
    }
    
    /// <summary>
    /// Creates a child node for the specified parent.
    /// </summary>
    private int CreateChildNode(ulong mortonCode, int parentIndex, byte quadrant)
    {
        // Get the parent node
        var parentNode = _nodes[parentIndex];
        
        // Create the child node
        int nodeIndex = _nodeCount++;
        
        // Ensure capacity
        if (nodeIndex >= _nodes.Length)
        {
            Array.Resize(ref _nodes, _nodes.Length * 2);
        }
        
        // Calculate the child's Morton code
        ulong childMortonCode = CalculateChildMortonCode(mortonCode, quadrant, parentNode.Depth);
        
        // Calculate the child's bounding box
        AAABBB childBox = CalculateChildBoundingBox(parentNode.BoundingBox, quadrant);
        
        // Create the child node
        _nodes[nodeIndex] = new OctreeNode(
            childMortonCode,
            NodeType.Leaf,
            childBox,
            parentNode.Depth + 1,
            quadrant,
            -1 // No children yet
        );
        
        // If the parent doesn't have a children index yet, allocate one
        if (parentNode.ChildrenIndex < 0)
        {
            // Allocate children indices
            int childrenIndex = _childrenIndicesCount;
            _childrenIndicesCount += 8;
            
            // Resize the children indices array if needed
            if (_childrenIndicesCount > _childrenIndices.Length)
            {
                int newSize = Math.Max(_childrenIndicesCount, _childrenIndices.Length * 2);
                Array.Resize(ref _childrenIndices, newSize);
            }
            
            // Initialize all children to -1 (no child)
            for (int i = 0; i < 8; i++)
            {
                _childrenIndices[childrenIndex + i] = -1;
            }
            
            // Update the parent node with the new children index
            _nodes[parentIndex] = new OctreeNode(
                parentNode.MortonCode,
                parentNode.Type,
                parentNode.BoundingBox,
                parentNode.Depth,
                parentNode.Quadrant,
                childrenIndex
            );
            
            // Update the parent node reference
            parentNode = _nodes[parentIndex];
        }
        
        // Update the child index in the parent's children array
        _childrenIndices[parentNode.ChildrenIndex + quadrant] = nodeIndex;
        
        // Add the node to the index lookup
        _nodeIndices[childMortonCode] = nodeIndex;
        
        return nodeIndex;
    }
    
    /// <summary>
    /// Calculates the bounding box for a child node.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AAABBB CalculateChildBoundingBox(AAABBB parentBox, byte quadrant)
    {
        // Calculate the center of the parent box
        Point3D center = parentBox.Center;
        
        // Calculate the min and max points based on the quadrant
        float minX, minY, minZ, maxX, maxY, maxZ;
        
        // X dimension (bit 0)
        if ((quadrant & 0b001) == 0)
        {
            minX = parentBox.Min.X;
            maxX = center.X;
        }
        else
        {
            minX = center.X;
            maxX = parentBox.Max.X;
        }
        
        // Y dimension (bit 1)
        if ((quadrant & 0b010) == 0)
        {
            minY = parentBox.Min.Y;
            maxY = center.Y;
        }
        else
        {
            minY = center.Y;
            maxY = parentBox.Max.Y;
        }
        
        // Z dimension (bit 2)
        if ((quadrant & 0b100) == 0)
        {
            minZ = parentBox.Min.Z;
            maxZ = center.Z;
        }
        else
        {
            minZ = center.Z;
            maxZ = parentBox.Max.Z;
        }
        
        return new AAABBB(
            new Point3D(minX, minY, minZ),
            new Point3D(maxX, maxY, maxZ)
        );
    }
    
    /// <summary>
    /// Removes a point from the octree.
    /// </summary>
    public bool Remove(Point3D point)
    {
        // Check if the point is within the octree bounds
        if (!_bounds.Contains(point))
        {
            return false;
        }
        
        lock (_nodeLock)
        {
            // Start at the root node
            ulong mortonCode = 0;
            bool removed = false;
            
            // Use a stack for depth-first traversal
            var stack = new Stack<ulong>();
            stack.Push(mortonCode);
            
            while (stack.Count > 0 && !removed)
            {
                ulong currentCode = stack.Pop();
                
                // Check if this node has points
                if (_pointsByNode.TryGetValue(currentCode, out var points))
                {
                    // Try to remove the point from this node
                    removed = RemovePointFromList(points, point);
                    
                    if (removed)
                    {
                        // Decrement the point count
                        _pointCount--;
                        
                        // If this node is now empty and it's not the root, clean it up
                        if (points.Count == 0 && currentCode != 0)
                        {
                            CleanupEmptyNodeNonRecursive(currentCode);
                        }
                        
                        // If all points are removed, reset to just the root node
                        if (PointCount == 0)
                        {
                            ResetToRootOnly();
                        }
                        
                        return true;
                    }
                }
                
                // If this node has children, add them to the stack
                if (_nodeIndices.TryGetValue(currentCode, out int nodeIndex))
                {
                    var node = _nodes[nodeIndex];
                    
                    if (node.Type == NodeType.Internal && node.ChildrenIndex >= 0)
                    {
                        // Add all existing children to the stack
                        for (byte i = 0; i < 8; i++)
                        {
                            int childIndex = _childrenIndices[node.ChildrenIndex + i];
                            if (childIndex >= 0)
                            {
                                ulong childCode = CalculateChildMortonCode(currentCode, i, node.Depth);
                                stack.Push(childCode);
                            }
                        }
                    }
                }
            }
            
            // If we couldn't find the point using traversal, try a direct search as a fallback
            if (!removed)
            {
                foreach (var kvp in _pointsByNode.ToArray())
                {
                    removed = RemovePointFromList(kvp.Value, point);
                    if (removed)
                    {
                        // Decrement the point count
                        _pointCount--;
                        
                        // If this node is now empty and it's not the root, clean it up
                        if (kvp.Value.Count == 0 && kvp.Key != 0)
                        {
                            CleanupEmptyNodeNonRecursive(kvp.Key);
                        }
                        
                        // If all points are removed, reset to just the root node
                        if (PointCount == 0)
                        {
                            ResetToRootOnly();
                        }
                        
                        return true;
                    }
                }
            }
            
            return removed;
        }
    }
    
    /// <summary>
    /// Helper method to remove a point from a list using approximate equality.
    /// </summary>
    private bool RemovePointFromList(List<Point3D> points, Point3D point)
    {
        lock (points)
        {
            // Find and remove the point (using approximate equality for floating point)
            const float epsilon = 0.0001f;
            for (int i = points.Count - 1; i >= 0; i--)
            {
                var p = points[i];
                if (Math.Abs(p.X - point.X) < epsilon && 
                    Math.Abs(p.Y - point.Y) < epsilon && 
                    Math.Abs(p.Z - point.Z) < epsilon)
                {
                    points.RemoveAt(i);
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Non-recursive version of CleanupEmptyNode to prevent stack overflow
    /// </summary>
    private void CleanupEmptyNodeNonRecursive(ulong startNodeCode)
    {
        if (startNodeCode == 0) // Don't clean up root node
            return;
            
        // Use a queue to avoid recursion
        var nodesToCheck = new Queue<ulong>();
        nodesToCheck.Enqueue(startNodeCode);
        
        // Keep track of nodes we've already processed
        var processedNodes = new HashSet<ulong>();
        
        while (nodesToCheck.Count > 0)
        {
            ulong mortonCode = nodesToCheck.Dequeue();
            
            // Skip if we've already processed this node
            if (processedNodes.Contains(mortonCode))
                continue;
                
            processedNodes.Add(mortonCode);
            
            // Only proceed if we have this node in our indices
            if (!_nodeIndices.TryGetValue(mortonCode, out int nodeIndex))
                continue;
                
            // Get the node
            var node = _nodes[nodeIndex];
            
            // Check if this node has any points
            bool hasPoints = _pointsByNode.TryGetValue(mortonCode, out var points) && points.Count > 0;
            
            // If this node has points, we can't remove it
            if (hasPoints)
                continue;
                
            // If this is an internal node with children, check if all children are empty
            if (node.Type == NodeType.Internal && node.ChildrenIndex >= 0)
            {
                bool allChildrenEmpty = true;
                var childrenToCheck = new List<(ulong code, int index)>();
                
                // Collect all children
                for (byte i = 0; i < 8; i++)
                {
                    int childIndex = _childrenIndices[node.ChildrenIndex + i];
                    
                    // Skip non-existent children
                    if (childIndex < 0)
                        continue;
                        
                    // Get the child's Morton code
                    ulong childCode = CalculateChildMortonCode(mortonCode, i, node.Depth);
                    childrenToCheck.Add((childCode, childIndex));
                    
                    // Check if the child has points
                    if (_pointsByNode.TryGetValue(childCode, out var childPoints) && childPoints.Count > 0)
                    {
                        allChildrenEmpty = false;
                        break;
                    }
                }
                
                // If not all children are empty, we can't clean up this node yet
                if (!allChildrenEmpty)
                    continue;
                
                // If all children are empty, remove them all
                foreach (var (childCode, childIndex) in childrenToCheck)
                {
                    // Remove child from indices
                    if (_nodeIndices.ContainsKey(childCode))
                    {
                        _nodeIndices.TryRemove(childCode, out _);
                        Interlocked.Decrement(ref _nodeCount);
                    }
                    
                    // Remove child's points
                    if (_pointsByNode.ContainsKey(childCode))
                    {
                        _pointsByNode.TryRemove(childCode, out _);
                    }
                    
                    // Mark child index as unused
                    byte quadrant = (byte)(MortonCode.GetQuadrant(childCode, node.Depth + 1));
                    if (node.ChildrenIndex >= 0 && node.ChildrenIndex + quadrant < _childrenIndices.Length)
                    {
                        _childrenIndices[node.ChildrenIndex + quadrant] = -1;
                    }
                }
                
                // Convert this node to a leaf node
                _nodes[nodeIndex] = new OctreeNode(
                    node.MortonCode,
                    NodeType.Leaf,
                    node.BoundingBox,
                    node.Depth,
                    node.Quadrant,
                    -1 // No children anymore
                );
            }
            
            // Remove this node's points collection
            _pointsByNode.TryRemove(mortonCode, out _);
            
            // Remove this node from the index lookup
            _nodeIndices.TryRemove(mortonCode, out _);
            Interlocked.Decrement(ref _nodeCount);
            
            // Get parent code
            ulong parentCode = MortonCode.GetParentCode(mortonCode);
            
            // If this is not the root, check if parent should be cleaned up
            if (parentCode != 0 && !processedNodes.Contains(parentCode))
            {
                nodesToCheck.Enqueue(parentCode);
            }
        }
    }
    
    /// <summary>
    /// Gets all points in the octree.
    /// </summary>
    public IEnumerable<Point3D> GetAllPoints()
    {
        lock (_nodeLock)
        {
            // Create a new list to avoid concurrent modification issues
            var result = new List<Point3D>();
            
            // Add all points from all nodes to the result list
            foreach (var pointsList in _pointsByNode.Values)
            {
                lock (pointsList)
                {
                    result.AddRange(pointsList);
                }
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// Processes all points in the octree using the specified action.
    /// </summary>
    public void ProcessPoints(Action<Point3D> action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));
            
        lock (_nodeLock)
        {
            foreach (var points in _pointsByNode.Values)
            {
                foreach (var point in points)
                {
                    action(point);
                }
            }
        }
    }
    
    /// <summary>
    /// Clears all points from the octree.
    /// </summary>
    public void Clear()
    {
        lock (_nodeLock)
        {
            // Clear all points
            foreach (var list in _pointsByNode.Values)
            {
                list.Clear();
            }
            _pointsByNode.Clear();
            
            // Reset the octree to just the root node
            ResetToRootOnly();
        }
    }
    
    /// <summary>
    /// Disposes the octree and releases all resources.
    /// </summary>
    public void Dispose()
    {
        Clear();
    }
    
    /// <summary>
    /// Normalizes a coordinate to the range [0, 1] based on the octree bounds.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float NormalizeCoordinate(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }
    
    /// <summary>
    /// Determines which octant a point belongs in.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte DetermineQuadrant(Point3D point, AAABBB bounds)
    {
        Point3D center = bounds.Center;
        
        byte quadrant = 0;
        
        if (point.X >= center.X) quadrant |= 0b001;
        if (point.Y >= center.Y) quadrant |= 0b010;
        if (point.Z >= center.Z) quadrant |= 0b100;
        
        return quadrant;
    }
    
    /// <summary>
    /// Gets information about all nodes in the octree for rendering purposes.
    /// </summary>
    public IEnumerable<(AAABBB BoundingBox, int Depth, ulong MortonCode)> GetNodeInfo()
    {
        lock (_nodeLock)
        {
            foreach (var kvp in _nodeIndices)
            {
                var nodeIndex = kvp.Value;
                var node = _nodes[nodeIndex];
                
                yield return (
                    node.BoundingBox,
                    node.Depth,
                    node.MortonCode
                );
            }
        }
    }
    
    /// <summary>
    /// Checks if a node has any points.
    /// </summary>
    public bool HasPointsAtNode(ulong mortonCode)
    {
        lock (_nodeLock)
        {
            if (_pointsByNode.TryGetValue(mortonCode, out var points))
            {
                return points.Count > 0;
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// Resets the octree to only contain the root node.
    /// </summary>
    private void ResetToRootOnly()
    {
        lock (_nodeLock)
        {
            // Clear all nodes except the root
            _nodeIndices.Clear();
            _nodeIndices[0] = 0;
            
            // Reset node count
            _nodeCount = 1;
            
            // Clear all points
            _pointsByNode.Clear();
            
            // Reset point count
            _pointCount = 0;
            
            // Reset children indices
            for (int i = 0; i < _childrenIndices.Length; i++)
            {
                _childrenIndices[i] = -1;
            }
            
            // Reset root node
            _nodes[0] = new OctreeNode(
                0, // Root has Morton code 0
                NodeType.Empty,
                _bounds,
                0, // Depth 0
                0, // No quadrant
                -1 // No children yet
            );
        }
    }
}
