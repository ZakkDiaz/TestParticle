using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ParticleLib.Modern.Models;

/// <summary>
/// Represents a node in the octree.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly struct OctreeNode : IEquatable<OctreeNode>
{
    /// <summary>
    /// The Morton code location of this node.
    /// </summary>
    public readonly ulong MortonCode { get; init; }
    
    /// <summary>
    /// The type of this node.
    /// </summary>
    public readonly NodeType Type { get; init; }
    
    /// <summary>
    /// The bounding box of this node.
    /// </summary>
    public readonly AAABBB BoundingBox { get; init; }
    
    /// <summary>
    /// The depth of this node in the octree.
    /// </summary>
    public readonly int Depth { get; init; }
    
    /// <summary>
    /// The quadrant of this node relative to its parent.
    /// </summary>
    public readonly byte Quadrant { get; init; }
    
    /// <summary>
    /// The index of this node's children in the octree's node pool.
    /// </summary>
    public readonly int ChildrenIndex { get; init; }
    
    /// <summary>
    /// Creates a new octree node.
    /// </summary>
    public OctreeNode(ulong mortonCode, NodeType type, AAABBB boundingBox, int depth, byte quadrant, int childrenIndex = -1)
    {
        MortonCode = mortonCode;
        Type = type;
        BoundingBox = boundingBox;
        Depth = depth;
        Quadrant = quadrant;
        ChildrenIndex = childrenIndex;
    }
    
    /// <summary>
    /// Gets the parent Morton code of this node.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong GetParentMortonCode() => MortonCode.GetParentCode();
    
    /// <summary>
    /// Creates a child node at the specified quadrant.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OctreeNode CreateChildNode(byte quadrant, NodeType type, int childrenIndex = -1)
    {
        // Calculate the child's Morton code
        ulong childCode = MortonCode.CreateChildCode(quadrant, Depth + 1);
        
        // Calculate the child's bounding box
        AAABBB childBox = CalculateChildBoundingBox(quadrant);
        
        return new OctreeNode(childCode, type, childBox, Depth + 1, quadrant, childrenIndex);
    }
    
    /// <summary>
    /// Calculates the bounding box for a child at the specified quadrant.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AAABBB CalculateChildBoundingBox(byte quadrant)
    {
        Point3D center = BoundingBox.Center;
        Point3D min = BoundingBox.Min;
        Point3D max = BoundingBox.Max;
        
        // Determine the child's bounding box based on the quadrant
        // Quadrants are encoded as binary: ZYX
        bool x = (quadrant & 0b001) != 0;
        bool y = (quadrant & 0b010) != 0;
        bool z = (quadrant & 0b100) != 0;
        
        Point3D childMin = new(
            x ? center.X : min.X,
            y ? center.Y : min.Y,
            z ? center.Z : min.Z
        );
        
        Point3D childMax = new(
            x ? max.X : center.X,
            y ? max.Y : center.Y,
            z ? max.Z : center.Z
        );
        
        return new AAABBB(childMin, childMax);
    }
    
    public bool Equals(OctreeNode other) => MortonCode == other.MortonCode;
    
    public override bool Equals(object? obj) => obj is OctreeNode node && Equals(node);
    
    public override int GetHashCode() => MortonCode.GetHashCode();
    
    public override string ToString() => $"Node[{MortonCode:X16}, Depth={Depth}, Type={Type}]";
}

/// <summary>
/// Represents the type of an octree node.
/// </summary>
[Flags]
public enum NodeType : byte
{
    /// <summary>
    /// An empty node.
    /// </summary>
    Empty = 0,
    
    /// <summary>
    /// A node containing a single point.
    /// </summary>
    Point = 1,
    
    /// <summary>
    /// A node containing multiple points.
    /// </summary>
    Cluster = 2,
    
    /// <summary>
    /// A leaf node that can contain points but has no children.
    /// </summary>
    Leaf = 3,
    
    /// <summary>
    /// An internal node with children.
    /// </summary>
    Internal = 4
}

/// <summary>
/// Extension methods for Morton codes.
/// </summary>
public static class MortonCodeExtensions
{
    /// <summary>
    /// Gets the parent Morton code of the specified code.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetParentCode(this ulong code)
    {
        return MortonCode.GetParentCode(code);
    }
    
    /// <summary>
    /// Creates a Morton code for a child node at the specified quadrant and level.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong CreateChildCode(this ulong parentCode, byte quadrant, int level)
    {
        return MortonCode.CreateChildCode(parentCode, quadrant, level);
    }
    
    /// <summary>
    /// Gets the depth of a Morton code (number of levels in the octree).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetDepth(this ulong code)
    {
        return MortonCode.GetDepth(code);
    }
    
    /// <summary>
    /// Gets the quadrant (octant) of a Morton code at the specified level.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetQuadrant(this ulong code, int level)
    {
        return MortonCode.GetQuadrant(code, level);
    }
    
    /// <summary>
    /// Gets the quadrant (octant) of a Morton code at its deepest level.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetQuadrant(this ulong code)
    {
        return MortonCode.GetQuadrant(code);
    }
}
