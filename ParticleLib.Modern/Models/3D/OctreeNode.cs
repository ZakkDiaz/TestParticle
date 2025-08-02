using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ParticleLib.Modern.Models._3D
{
    /// <summary>
    /// Represents a node in the octree structure
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct OctreeNode : IEquatable<OctreeNode>
    {
        /// <summary>
        /// The Morton code (Z-order curve) location code for this node
        /// </summary>
        public readonly ulong MortonCode;
        
        /// <summary>
        /// The depth of this node in the octree
        /// </summary>
        public readonly byte Depth;
        
        /// <summary>
        /// The bounding box of this node
        /// </summary>
        public readonly AAABBB BoundingBox;
        
        /// <summary>
        /// The octant this node belongs to (0-7)
        /// </summary>
        public readonly byte Octant;

        /// <summary>
        /// Creates a new octree node
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OctreeNode(ulong mortonCode, byte depth, AAABBB boundingBox, byte octant)
        {
            MortonCode = mortonCode;
            Depth = depth;
            BoundingBox = boundingBox;
            Octant = octant;
        }

        /// <summary>
        /// Creates a root octree node
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OctreeNode CreateRoot(AAABBB boundingBox)
        {
            return new OctreeNode(0, 0, boundingBox, 0);
        }

        /// <summary>
        /// Creates a child node for the given octant
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OctreeNode CreateChild(byte octant)
        {
            // Split the bounding box
            var octants = BoundingBox.Split();
            
            // Calculate the new Morton code by adding the octant bits at the appropriate position
            ulong childMortonCode = MortonCode | ((ulong)octant << (3 * (21 - Depth - 1)));
            
            return new OctreeNode(
                childMortonCode,
                (byte)(Depth + 1),
                octants[octant],
                octant
            );
        }

        /// <summary>
        /// Determines which child octant a point belongs to
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetChildOctantForPoint(Point3D point)
        {
            return BoundingBox.GetOctant(point);
        }

        /// <summary>
        /// Checks if this node equals another
        /// </summary>
        public bool Equals(OctreeNode other)
        {
            return MortonCode == other.MortonCode;
        }

        /// <summary>
        /// Checks if this node equals another object
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is OctreeNode other && Equals(other);
        }

        /// <summary>
        /// Gets a hash code for this node
        /// </summary>
        public override int GetHashCode()
        {
            return MortonCode.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation of this node
        /// </summary>
        public override string ToString()
        {
            return $"Node[MortonCode: {MortonCode}, Depth: {Depth}, Octant: {Octant}]";
        }
    }

    /// <summary>
    /// Represents a collection of child nodes in an octree
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NodeCollection
    {
        // Child nodes for each octant (000 to 111)
        public int _000;
        public int _001;
        public int _010;
        public int _011;
        public int _100;
        public int _101;
        public int _110;
        public int _111;

        /// <summary>
        /// Gets or sets a child node by its octant index (0-7)
        /// </summary>
        public int this[int octant]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => octant switch
            {
                0 => _000,
                1 => _001,
                2 => _010,
                3 => _011,
                4 => _100,
                5 => _101,
                6 => _110,
                7 => _111,
                _ => throw new ArgumentOutOfRangeException(nameof(octant), "Octant must be between 0 and 7")
            };
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                switch (octant)
                {
                    case 0: _000 = value; break;
                    case 1: _001 = value; break;
                    case 2: _010 = value; break;
                    case 3: _011 = value; break;
                    case 4: _100 = value; break;
                    case 5: _101 = value; break;
                    case 6: _110 = value; break;
                    case 7: _111 = value; break;
                    default: throw new ArgumentOutOfRangeException(nameof(octant), "Octant must be between 0 and 7");
                }
            }
        }

        /// <summary>
        /// Checks if any child node exists at the specified octant
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasChild(int octant)
        {
            return this[octant] != 0;
        }

        /// <summary>
        /// Gets the number of child nodes in this collection
        /// </summary>
        public int ChildCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (HasChild(i))
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Clears all child nodes
        /// </summary>
        public void Clear()
        {
            _000 = 0;
            _001 = 0;
            _010 = 0;
            _011 = 0;
            _100 = 0;
            _101 = 0;
            _110 = 0;
            _111 = 0;
        }
    }

    /// <summary>
    /// Represents a node type in the octree
    /// </summary>
    [Flags]
    public enum NodeType
    {
        None = 0,
        Internal = 1,
        Leaf = 2
    }
}
