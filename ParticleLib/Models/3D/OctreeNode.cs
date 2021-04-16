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
        public DimensionProperty ForceX { get; set; }
        public DimensionProperty ForceY { get; set; }
        public DimensionProperty ForceZ { get; set; }

        unsafe public OctreeNode(NodeCollection* children, OctreeNode?* parent, UInt32 locCode, Byte childExists, NodeType nodeType, IntPtr objPtr, Byte quadrant)
        {
            Children = children;
            Parent = parent;
            LocCode = locCode;
            ChildExists = childExists;
            NodeType = nodeType;
            ObjPtr = objPtr;
            Quadrant = quadrant;
            ForceX = new DimensionProperty(0);
            ForceY = new DimensionProperty(1);
            ForceZ = new DimensionProperty(2);
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

    public class NodeTypeLayer3D : AAABBB
    {
        public ForceContainer ForceContainer;
        public NodeTypeLayer3D(Point3D from, Point3D to) : base(from, to)
        { 
            ChildLocationItems = new List<NodeTypeLocation3D>();
            ForceContainer = new ForceContainer();
        }
        private static int _maxSize = 10;
        private bool isoverflow = false;
        public static NodeType Identity { get; set; } = NodeType.Location;
        public List<NodeTypeLocation3D> ChildLocationItems { get; set; }
        public void Add(NodeTypeLocation3D item)
        {
            ChildLocationItems.Add(item);
        }

        public bool Full()
        {
            return isoverflow || ChildLocationItems.Count >= _maxSize;
        }

        internal void ClearChildren()
        {
            ChildLocationItems = new List<NodeTypeLocation3D>();
            isoverflow = true;
        }
    }

    public class NodeTypeLocation3D
    {
        public Point3D Location;
        public NodeTypeLocation3D(float x, float y, float z, bool isLayer)
        {
            Location = new Point3D(x, y, z);
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
