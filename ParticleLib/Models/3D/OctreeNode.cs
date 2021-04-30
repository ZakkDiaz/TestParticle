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
        public Byte Quadrant { get; set; }
        public DimensionProperty ForceX { get; set; }
        public DimensionProperty ForceY { get; set; }
        public DimensionProperty ForceZ { get; set; }
        public IntPtr Octree { get; set; }

        unsafe public OctreeNode(NodeCollection* children, OctreeNode?* parent, ulong locCode, Byte childExists, NodeType nodeType, Byte quadrant, IntPtr octreePtr)
        {
            Children = children;
            Parent = parent;
            LocCode = locCode;
            ChildExists = childExists;
            NodeType = nodeType;
            Quadrant = quadrant;
            ForceX = new DimensionProperty(0);
            ForceY = new DimensionProperty(1);
            ForceZ = new DimensionProperty(2);
            Octree = octreePtr;
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
        public NodeTypeLayer3D ParentLayer;
        public ForceContainer ForceContainer;
        public List<NodeTypeLocation3D> ChildLocationItems => childLocationItems.ToList();
        private ulong _location;
        public NodeTypeLayer3D(Point3D from, Point3D to, ulong location) : base(from, to)
        {
            _location = location;
            childLocationItems = new ConcurrentBag<NodeTypeLocation3D>();
            ForceContainer = new ForceContainer();
            layerTask = new Task(() => ProcessItems());
            layerTask.Start();
        }

        private List<Task> _toRun { get; set; } = new List<Task>(); 
        private void ProcessItems()
        {
            while(true)
            {
                try
                {
                    System.Threading.Thread.Sleep(100);
                    List<Task> toRun = new List<Task>();
                    lock (_toRun)
                    {
                        toRun = _toRun.ToList();
                        _toRun = new List<Task>();
                    }
                    foreach (var t in toRun)
                    {
                        if(t.Status == TaskStatus.Created || t.Status == TaskStatus.WaitingForActivation)
                            t.Start();
                    }
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
        private ConcurrentBag<NodeTypeLocation3D> childLocationItems { get; set; }
        public Task layerTask;
        public void Add(NodeTypeLocation3D item)
        {
            childLocationItems.Add(item);
        }

        public bool Full()
        {
            lock (itemListLock)
                return isoverflow || childLocationItems.Count >= _maxSize;
        }

        internal List<NodeTypeLocation3D> ClearChildren()
        {
            isoverflow = true;
            var toRet = childLocationItems.ToList();
            childLocationItems = new ConcurrentBag<NodeTypeLocation3D>();
            return toRet;
            
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

        strt:
            //if(!Octree.OctreeHeaps.ContainsKey(_octPtr))
            //{
            //    System.Threading.Thread.Sleep(1000);
            //}
            //var _octreeHeap = Octree.OctreeHeaps[_octPtr];
            //if(!_octreeHeap.ContainsKey(parentNode.LocCode))
            //{
            //    System.Threading.Thread.Sleep(1000);
            //    goto strt;
            //}

            //while(!Octree._layers.ContainsKey(parentNode.LocCode))
            //{
            //    System.Threading.Thread.Sleep(100);
            //    var parentLayer = Octree._layers[parentNode.LocCode];
            //}

            //var parentHandle = GCHandle.FromIntPtr(parentNode.ObjPtr);
            //var parentLayer = (NodeTypeLayer3D)parentHandle.Target;
            //var parentCollection = *parentNode.Children;
            //var parentCollection = _octreeHeap[parentNode.LocCode];

            NodeCollection mergeCollection;
            var location = parentNode.LocCode | (quad << depth);
            if (Octree._nodes.ContainsKey(location))
            {
                var chldPtr = Octree._nodes[location].Children;
                mergeCollection = *(chldPtr);
            }
                
            else
                mergeCollection = new NodeCollection();
            //var defaultNodeCollection = new NodeCollection();
            var nodeCollectionPtr = &mergeCollection;

            //var layerSize = _to * ((float)(1 / Math.Pow(2, depth/4)));
            //g.DrawRectangle(Pens.Black, new Rectangle((int)parentNode.Location.Item1 - (int)layerSize.X, (int)parentNode.Location.Item2 - (int)layerSize.Y, 2 * (int)layerSize.X, 2 * (int)layerSize.Y));

            //if (x && (pointLocation.Location.Item1 - layerSize.X) > this.Location.Item1)
            //    ;

            OctreeNode? addTo = null;
            switch (quad)
            {
                case unchecked((byte)0b1111):
                    //if (parentCollection._111.HasValue)
                    //{
                    //    addTo = parentCollection._111.Value;
                    //    mergeCollection._111 = addTo;
                    //}
                    //else
                    {
                        var fromOff = new Point3D(half.X, half.Y, half.Z);

                        addTo = GenerateLayerNode(parentNode, depth, half, quad, nodeCollectionPtr, fromOff);

                        //if (_octreeHeap.ContainsKey(addTo.Value.LocCode))
                        //{
                        //    LoadTask(AddAsyncTask(parentNode, pointLocation, depth));
                        //    return;
                        //}

                        mergeCollection._111 = addTo;
                    }
                    break;
                case unchecked((byte)0b1110):
                    //if (parentCollection._110.HasValue)
                    //{
                    //    addTo = parentCollection._110.Value;
                    //    mergeCollection._110 = addTo;
                    //}
                    //else
                    {
                        var fromOff = new Point3D(0, half.Y, half.Z);

                        addTo = GenerateLayerNode(parentNode, depth, half, quad, nodeCollectionPtr, fromOff);

                        //if (_octreeHeap.ContainsKey(addTo.Value.LocCode))
                        //{
                        //    LoadTask(AddAsyncTask(parentNode, pointLocation, depth));
                        //    return;
                        //}

                        mergeCollection._110 = addTo;
                    }
                    break;
                case unchecked((byte)0b1101):
                    //if (parentCollection._101.HasValue)
                    //{
                    //    addTo = parentCollection._101.Value;
                    //    mergeCollection._101 = addTo;
                    //}
                    //else
                    {
                        var fromOff = new Point3D(half.X, 0, half.Z);

                        addTo = GenerateLayerNode(parentNode, depth, half, quad, nodeCollectionPtr, fromOff);

                        //if (_octreeHeap.ContainsKey(addTo.Value.LocCode))
                        //{
                        //    LoadTask(AddAsyncTask(parentNode, pointLocation, depth));
                        //    return;
                        //}

                        mergeCollection._101 = addTo;
                    }
                    break;
                case unchecked((byte)0b1100):
                    //if (parentCollection._100.HasValue)
                    //{
                    //    addTo = parentCollection._100.Value;
                    //    mergeCollection._100 = addTo;
                    //}
                    //else
                    {
                        var fromOff = new Point3D(0, 0, half.Z);

                        addTo = GenerateLayerNode(parentNode, depth, half, quad, nodeCollectionPtr, fromOff);

                        //if (_octreeHeap.ContainsKey(addTo.Value.LocCode))
                        //{
                        //    LoadTask(AddAsyncTask(parentNode, pointLocation, depth));
                        //    return;
                        //}

                        mergeCollection._100 = addTo;
                    }
                    break;
                case unchecked((byte)0b1011):
                    //if (parentCollection._011.HasValue)
                    //{
                    //    addTo = parentCollection._011.Value;
                    //    mergeCollection._011 = addTo;
                    //}
                    //else
                    {
                        var fromOff = new Point3D(half.X, half.Y, 0);

                        addTo = GenerateLayerNode(parentNode, depth, half, quad, nodeCollectionPtr, fromOff);

                        //if (_octreeHeap.ContainsKey(addTo.Value.LocCode))
                        //{
                        //    LoadTask(AddAsyncTask(parentNode, pointLocation, depth));
                        //    return;
                        //}

                        mergeCollection._011 = addTo;
                    }
                    break;
                case unchecked((byte)0b1010):
                    //if (parentCollection._010.HasValue)
                    //{
                    //    addTo = parentCollection._010.Value;
                    //    mergeCollection._010 = addTo;
                    //}
                    //else
                    {
                        var fromOff = new Point3D(0, half.Y, 0);

                        addTo = GenerateLayerNode(parentNode, depth, half, quad, nodeCollectionPtr, fromOff);

                        //if (_octreeHeap.ContainsKey(addTo.Value.LocCode))
                        //{
                        //    LoadTask(AddAsyncTask(parentNode, pointLocation, depth));
                        //    return;
                        //}

                        mergeCollection._010 = addTo;
                    }
                    break;
                case unchecked((byte)0b1001):
                    //if (parentCollection._001.HasValue)
                    //{
                    //    addTo = parentCollection._001.Value;
                    //    mergeCollection._001 = addTo;
                    //}
                    //else
                    {
                        var fromOff = new Point3D(half.X, 0, 0);

                        addTo = GenerateLayerNode(parentNode, depth, half, quad, nodeCollectionPtr, fromOff);

                        //if (_octreeHeap.ContainsKey(addTo.Value.LocCode))
                        //{
                        //    LoadTask(AddAsyncTask(parentNode, pointLocation, depth));
                        //    return;
                        //}

                        mergeCollection._001 = addTo;
                    }
                    break;
                case unchecked((byte)0b1000):
                    //if (parentCollection._000.HasValue)
                    //{
                    //    addTo = parentCollection._000.Value;
                    //    mergeCollection._000 = addTo;
                    //}
                    //else
                    {
                        var fromOff = new Point3D(0, 0, 0);

                        addTo = GenerateLayerNode(parentNode, depth, half, quad, nodeCollectionPtr, fromOff);

                        //if (_octreeHeap.ContainsKey(addTo.Value.LocCode))
                        //{
                        //    LoadTask(AddAsyncTask(parentNode, pointLocation, depth));
                        //    return;
                        //}

                        mergeCollection._000 = addTo;
                    }
                    break;
            }

            //var handle = GCHandle.FromIntPtr(addTo.Value.ObjPtr);
            
            //while (!handle.IsAllocated)
            //{
            //    System.Threading.Thread.Sleep(10);
            //}
            //var layer = (NodeTypeLayer3D)handle.Target;
            //layer.Add(pointLocation);
            Octree.MergeHeap(new KeyValuePair<IntPtr, KeyValuePair<OctreeNode, NodeCollection>>(parentNode.Octree, new KeyValuePair<OctreeNode, NodeCollection>(parentNode, mergeCollection)));

            var toReflow = this.ClearChildren();
            foreach (var node in toReflow)
            {
                LoadTask(AddAsyncTask(parentNode, node, depth));
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
            var layerPtr = CreateLayerLocation(_compareTo, quadFrom, quadTo, quad);
            var layerONode = new OctreeNode()
            {
                NodeType = NodeType.Layer,
                LocCode = quad,
                ChildExists = 0,
                Quadrant = (byte)quad
            };
            layerONode.Children = nodeCollectionPtr;
            layerONode.LocCode = (parentNode.Value.LocCode | (quad << depth));

            OctreeNode?* parentRef;
            parentRef = &parentNode;
            layerONode.Parent = parentRef;
            return layerONode;
        }

        private ulong CreateLayerLocation(NodeTypeLayer3D parentLayer, Point3D quadFrom, Point3D quadTo, ulong quad)
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
            var layerNode = new NodeTypeLayer3D(quadFrom, quadTo, quad);
            layerNode.ParentLayer = parentLayer;
            //var ptr = (IntPtr)GCHandle.Alloc(layerNode);
            //while(Octree._layers.ContainsKey(ptr))
            //{
            //    layerNode = new NodeTypeLayer3D(quadFrom, quadTo, quad);
            //    layerNode.ParentLayer = parentLayer;
            //    ptr = (IntPtr)GCHandle.Alloc(layerNode);
            //}
            Octree._layers[quad] = layerNode;
            //Octree._addLayers.Enqueue(new Tuple<IntPtr, NodeTypeLayer3D>(ptr, layerNode));

            ////Remove this and add another ref to a point collection
            ////Then from quad calculate the point location relative to the point referenced (head = center, 000 = center/2, 111 = center * 2)


            //_locationRefs.TryAdd(ptr, layerNode);
            return quad;
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
