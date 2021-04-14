using System;
using System.Collections.Generic;
using System.Text;

namespace ParticleLib.Models._3D
{
    public unsafe struct OctreeNode
    {
        public NodeCollection* Children;
        public OctreeNode?* Parent; // optional
        public ulong LocCode;
        public Byte ChildExists;
        public NodeType NodeType { get; set; }
        public IntPtr ObjPtr { get; set; }
        public Byte Quadrant { get; set; }

        unsafe public OctreeNode(NodeCollection* children, OctreeNode?* parent, UInt32 locCode, Byte childExists, NodeType nodeType, IntPtr objPtr, Byte quadrant)
        {
            Children = children;
            Parent = parent;
            LocCode = locCode;
            ChildExists = childExists;
            NodeType = nodeType;
            ObjPtr = objPtr;
            Quadrant = quadrant;
        }

        internal void Add(OctreeNode node)
        {
            throw new NotImplementedException();
        }
    };

    [Flags]
    public enum NodeType
    {
        None = 0,
        Location = 1,
        Layer = 2
    }

    public struct NodeCollection
    {
        public OctreeNode? _111 { get; set; }
        public OctreeNode? _110 { get; set; }
        public OctreeNode? _101 { get; set; }
        public OctreeNode? _100 { get; set; }
        public OctreeNode? _011 { get; set; }
        public OctreeNode? _010 { get; set; }
        public OctreeNode? _001 { get; set; }
        public OctreeNode? _000 { get; set; }
    }

    public class NodeTypeLocation3D
    {
        private bool islayer = false;
        private bool isoverflow = false;
        private static int _maxSize = 10;
        public (float, float, float) Location;
        public static NodeType Identity { get; set; } = NodeType.Location;
        public List<NodeTypeLocation3D> ChildLocationItems { get; set; }
        public bool IsLayer => islayer;

        public NodeTypeLocation3D(float x, float y, float z, bool isLayer)
        {
            islayer = isLayer;
            Location = (x, y, z);
            if (isLayer)
            { ChildLocationItems = new List<NodeTypeLocation3D>(); }
        }

        public void Add(NodeTypeLocation3D item)
        {
            ChildLocationItems.Add(item);
        }

        public bool Full()
        {
            return isoverflow || !this.islayer || ChildLocationItems.Count >= _maxSize;
        }

        internal void ClearChildren()
        {
            ChildLocationItems = new List<NodeTypeLocation3D>();
            isoverflow = true;
        }
    }

    [Flags]
    public enum Quadrants : byte
    {
        None = 0,
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2
    }
}
