using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ParticleLib.Modern.Models._3D
{
    /// <summary>Represents a node in the octree.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct OctreeNode : IEquatable<OctreeNode>
    {
        public readonly ulong MortonCode;
        public readonly byte Depth;
        public readonly AAABBB BoundingBox;
        public readonly byte Octant;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OctreeNode(ulong morton, byte depth, AAABBB box, byte octant)
        {
            MortonCode = morton;
            Depth = depth;
            BoundingBox = box;
            Octant = octant;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OctreeNode CreateRoot(AAABBB box) => new OctreeNode(0, 0, box, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OctreeNode CreateChild(byte octant)
        {
            var octants = BoundingBox.Split();
            byte newDepth = (byte)(Depth + 1);

            // shift left, add octant ...
            ulong childCode = (MortonCode << 3) | octant;

            // ... then mark this depth
            childCode |= 1UL << (3 * newDepth);

            return new OctreeNode(childCode, newDepth, octants[octant], octant);
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetChildOctantForPoint(Point3D p) => BoundingBox.GetOctant(p);

        public bool Equals(OctreeNode other) => MortonCode == other.MortonCode;
        public override bool Equals(object obj) => obj is OctreeNode o && Equals(o);
        public override int GetHashCode() => MortonCode.GetHashCode();
        public override string ToString() => $"Node[Morton={MortonCode}, Depth={Depth}, Octant={Octant}]";
    }

    /// <summary>Node categories for bookkeeping.</summary>
    [Flags]
    public enum NodeType
    {
        None = 0,
        Internal = 1,
        Leaf = 2
    }
}
