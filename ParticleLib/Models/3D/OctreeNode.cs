using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        public List<NodeTypeLocation3D> ChildLocationItems => childLocationItems.ToList();
        private IntPtr _octPtr;
        public NodeTypeLayer3D(Point3D from, Point3D to, IntPtr octPtr) : base(from, to)
        {
            _octPtr = octPtr;
            childLocationItems = new List<NodeTypeLocation3D>();
            ForceContainer = new ForceContainer();
            layerThread = new Thread(ProcessItems);
        }

        private List<Task> _toRun { get; set; } = new List<Task>(); 
        private void ProcessItems()
        {
            while(true)
            {
                try
                {
                    List<Task> toRun = new List<Task>();
                    lock (_toRun)
                    {
                        toRun = _toRun.ToList();
                        _toRun = new List<Task>();
                    }
                    foreach (var t in toRun)
                        t.Start();
                }
                catch(Exception ex )
                {

                }
            }
        }

        private static int _maxSize = 10;
        private bool isoverflow = false;
        public static NodeType Identity { get; set; } = NodeType.Location;
        private object itemListLock { get; set; } = new object();
        private List<NodeTypeLocation3D> childLocationItems { get; set; }
        public Thread layerThread;
        public void Add(NodeTypeLocation3D item)
        {
            lock(itemListLock)
                childLocationItems.Add(item);
        }

        public bool Full()
        {
            lock (itemListLock)
                return isoverflow || childLocationItems.Count >= _maxSize;
        }

        internal void ClearChildren()
        {
            lock (itemListLock)
                childLocationItems = new List<NodeTypeLocation3D>();
            isoverflow = true;
        }

        public void LoadTask(Task task)
        {
            lock (_toRun)
                _toRun.Add(task);
        }

        public async Task AddAsyncTask(OctreeNode parentNode, NodeTypeLocation3D pointLocation, int depth)
        {
            await Task.Run(() => { AddAsync(parentNode, pointLocation, depth); });
        }

        internal unsafe void AddAsync(OctreeNode parentNode, NodeTypeLocation3D pointLocation, int depth)
        {
            if (!this.Full() || depth == 64)
            {
                this.Add(pointLocation);
                return;
            }

            //var scale = Math.Pow(2, depth / 4);
            var half = (this.To - this.From) / 2;
            var center = this.From + half;
            var x = pointLocation.Location.X > Math.Abs(center.X);
            var y = pointLocation.Location.Y > Math.Abs(center.Y);
            var z = pointLocation.Location.Z > Math.Abs(center.Z);

            ulong quad =
                (ulong)(
                    (x ? 0b1001 : 0b1000)
                    |
                    (y ? 0b1010 : 0b1000)
                    |
                    (z ? 0b1100 : 0b1000)
                );


            var _octreeHeap = Octree.OctreeHeaps[_octPtr];
            var nodeCollection = _octreeHeap[parentNode.LocCode];
            var defaultNodeCollection = new NodeCollection();
            var nodeCollectionPtr = &defaultNodeCollection;

            //var layerSize = _to * ((float)(1 / Math.Pow(2, depth/4)));
            //g.DrawRectangle(Pens.Black, new Rectangle((int)parentNode.Location.Item1 - (int)layerSize.X, (int)parentNode.Location.Item2 - (int)layerSize.Y, 2 * (int)layerSize.X, 2 * (int)layerSize.Y));

            //if (x && (pointLocation.Location.Item1 - layerSize.X) > this.Location.Item1)
            //    ;

            OctreeNode? addTo = null;
            switch (quad)
            {
                case unchecked((byte)0b1111):
                    if (nodeCollection._111.HasValue)
                    {
                        addTo = nodeCollection._111.Value;
                    }
                    else
                    {
                        var fromOff = new Point3D(half.X, half.Y, half.Z);

                        addTo = GenerateLayerNode(parentNode, depth, half, quad, nodeCollectionPtr, fromOff);

                        nodeCollection._111 = addTo;
                    }
                    break;
                case unchecked((byte)0b1110):
                    if (nodeCollection._110.HasValue)
                    {
                        addTo = nodeCollection._110.Value;
                    }
                    else
                    {
                        var fromOff = new Point3D(0, half.Y, half.Z);

                        addTo = GenerateLayerNode(parentNode, depth, half, quad, nodeCollectionPtr, fromOff);

                        nodeCollection._110 = addTo;
                    }
                    break;
                case unchecked((byte)0b1101):
                    if (nodeCollection._101.HasValue)
                    {
                        addTo = nodeCollection._101.Value;
                    }
                    else
                    {
                        var fromOff = new Point3D(half.X, 0, half.Z);

                        addTo = GenerateLayerNode(parentNode, depth, half, quad, nodeCollectionPtr, fromOff);

                        nodeCollection._101 = addTo;
                    }
                    break;
                case unchecked((byte)0b1100):
                    if (nodeCollection._100.HasValue)
                    {
                        addTo = nodeCollection._100.Value;
                    }
                    else
                    {
                        var fromOff = new Point3D(0, 0, half.Z);

                        addTo = GenerateLayerNode(parentNode, depth, half, quad, nodeCollectionPtr, fromOff);

                        nodeCollection._100 = addTo;
                    }
                    break;
                case unchecked((byte)0b1011):
                    if (nodeCollection._011.HasValue)
                    {
                        addTo = nodeCollection._011.Value;
                    }
                    else
                    {
                        var fromOff = new Point3D(half.X, half.Y, 0);

                        addTo = GenerateLayerNode(parentNode, depth, half, quad, nodeCollectionPtr, fromOff);

                        nodeCollection._011 = addTo;
                    }
                    break;
                case unchecked((byte)0b1010):
                    if (nodeCollection._010.HasValue)
                    {
                        addTo = nodeCollection._010.Value;
                    }
                    else
                    {
                        var fromOff = new Point3D(0, half.Y, 0);

                        addTo = GenerateLayerNode(parentNode, depth, half, quad, nodeCollectionPtr, fromOff);

                        nodeCollection._010 = addTo;
                    }
                    break;
                case unchecked((byte)0b1001):
                    if (nodeCollection._001.HasValue)
                    {
                        addTo = nodeCollection._001.Value;
                    }
                    else
                    {
                        var fromOff = new Point3D(half.X, 0, 0);

                        addTo = GenerateLayerNode(parentNode, depth, half, quad, nodeCollectionPtr, fromOff);

                        nodeCollection._001 = addTo;
                    }
                    break;
                case unchecked((byte)0b1000):
                    if (nodeCollection._000.HasValue)
                    {
                        addTo = nodeCollection._000.Value;
                    }
                    else
                    {
                        var fromOff = new Point3D(0, 0, 0);

                        addTo = GenerateLayerNode(parentNode, depth, half, quad, nodeCollectionPtr, fromOff);

                        nodeCollection._000 = addTo;
                    }
                    break;
            }

            var handle = GCHandle.FromIntPtr(addTo.Value.ObjPtr);
            var layer = (NodeTypeLayer3D)handle.Target;
            var parentPtr = parentNode.Parent;


            if (_octreeHeap.TryAdd(addTo.Value.LocCode, defaultNodeCollection))
            {
                    LoadTask(AddAsyncTask(addTo.Value, pointLocation, depth += 4));
            }
            else
            {

                if (parentPtr != null)
                {
                    LoadTask(AddAsyncTask(parentPtr->Value, pointLocation, depth -= 4));
                } else
                {
                    if(addTo.Value.Parent != null)
                    {
                        var prent = (addTo.Value.Parent)->Value;
                        LoadTask(AddAsyncTask(prent, pointLocation, depth));
                    }
                    //else
                    //{
                    //    LoadTask(AddAsyncTask(addTo.Value, pointLocation, depth += 4));
                    //}
                }
            }

            _octreeHeap[parentNode.LocCode] = nodeCollection;
            Octree.OctreeHeaps[_octPtr] = _octreeHeap;

            var toReflow = this.childLocationItems.ToList();
            this.ClearChildren();
            foreach (var node in toReflow)
            {
                if (parentPtr != null)
                {
                    LoadTask(AddAsyncTask(parentPtr->Value, node, depth -= 4));
                }
                else
                {
                    LoadTask(AddAsyncTask(addTo.Value, node, depth += 4));
                }
            }
        }

        private unsafe OctreeNode GenerateLayerNode(OctreeNode parentNode, int depth, Point3D half, ulong quad, NodeCollection* nodeCollectionPtr, Point3D fromOff)
        {
            var _fr = this.From + fromOff;
            var _to = _fr + half;
            OctreeNode layerONode = GenerateLayerOctreeNode(parentNode, depth, this, quad, nodeCollectionPtr, _fr, _to);
            return layerONode;
        }

        private unsafe OctreeNode GenerateLayerOctreeNode(OctreeNode? parentNode, int depth, NodeTypeLayer3D _compareTo, ulong quad, NodeCollection* nodeCollectionPtr, Point3D quadFrom, Point3D quadTo)
        {
            var layerPtr = CreateLayerLocation(_compareTo, quadFrom, quadTo);
            var layerONode = new OctreeNode()
            {
                NodeType = NodeType.Layer,
                ObjPtr = layerPtr,
                ChildExists = 0
            };
            layerONode.Children = nodeCollectionPtr;
            layerONode.LocCode = (parentNode.Value.LocCode | (quad << depth));

            OctreeNode?* parentRef;
            parentRef = &parentNode;
            layerONode.Parent = parentRef;
            return layerONode;
        }

        private unsafe IntPtr CreateLayerLocation(NodeTypeLayer3D parentLayer, Point3D quadFrom, Point3D quadTo)
        {
            //var center = (parentLayer._to - parentLayer._from) / 2;
            //var from = center + quadOff;
            //var to = from + center;
            //var absOff = new Point3D(Math.Abs(centerOff.X), Math.Abs(centerOff.Y), Math.Abs(centerOff.Z));
            //var to = absOff + quadOff;
            //var xMin = Math.Min(center.X, quadOff.X);
            //var yMin = Math.Min(center.Y, quadOff.Y);
            //var zMin = Math.Min(center.Z, quadOff.Z);
            //var xMax = Math.Max(center.X, quadOff.X);
            //var yMax = Math.Max(center.Y, quadOff.Y);
            //var zMax = Math.Max(center.Z, quadOff.Z);

            //var from = new Point3D(xMin, yMin, zMin);
            //var to = new Point3D(xMax, yMax, zMax);
            var layerNode = new NodeTypeLayer3D(quadFrom, quadTo, _octPtr);

            var ptr = (IntPtr)GCHandle.Alloc(layerNode);

            ////Remove this and add another ref to a point collection
            ////Then from quad calculate the point location relative to the point referenced (head = center, 000 = center/2, 111 = center * 2)


            //_locationRefs.TryAdd(ptr, layerNode);
            return ptr;
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
