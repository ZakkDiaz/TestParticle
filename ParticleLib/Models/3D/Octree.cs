using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ParticleLib.Models._3D
{
    public class Octree : AAABBB
    {
        //public static Dictionary<IntPtr, Dictionary<ulong, NodeCollection>> OctreeHeaps = new Dictionary<IntPtr, Dictionary<ulong, NodeCollection>>();
        public static ConcurrentBag<KeyValuePair<IntPtr, KeyValuePair<OctreeNode, NodeCollection>>> _octreeMerges = new ConcurrentBag<KeyValuePair<IntPtr, KeyValuePair<OctreeNode, NodeCollection>>>();


        object taskLock = new object();
        List<Task> taskQueue = new List<Task>();
        Thread _taskThread;
        Thread _mergeThread;
        public OctreeNode OctreeNode { get; set; }
        //private ConcurrentDictionary<ulong, NodeCollection> _octreeHeap = new ConcurrentDictionary<ulong, NodeCollection>();

        //private object _objRefsLock = new object();
        //private List<NodeTypeLocation3D> _objRefs = new List<NodeTypeLocation3D>();
        //private ConcurrentDictionary<IntPtr, NodeTypeLayer3D> _locationRefs = new ConcurrentDictionary<IntPtr, NodeTypeLayer3D>();

        //public void ProcessParticles(IParticleProcessor particleProcessor)
        //{
        //    particleProcessor.Process(OctreeNode, ref _locationRefs, ref _octreeHeap);
        //}

        public Point3D[] GetPointCloud()
        {
            return GetPointCloudFromPtr(OctreeNode).ToArray();
        }

        public unsafe IEnumerable<Point3D> GetPointCloudFromPtr(OctreeNode node)
        {
            var ptr = node.ObjPtr;
            var handle = GCHandle.FromIntPtr(ptr);
            var layer = (NodeTypeLayer3D)handle.Target;
            var list = layer.ChildLocationItems.Select(s => s.Location).ToList();
            

            if (node.Children->_000.HasValue)
            {
                list.AddRange(GetPointCloudFromPtr(node.Children->_000.Value));
            }
            if (node.Children->_001.HasValue)
            {
                list.AddRange(GetPointCloudFromPtr(node.Children->_001.Value));
            }
            if (node.Children->_010.HasValue)
            {
                list.AddRange(GetPointCloudFromPtr(node.Children->_010.Value));
            }
            if (node.Children->_011.HasValue)
            {
                list.AddRange(GetPointCloudFromPtr(node.Children->_011.Value));
            }
            if (node.Children->_100.HasValue)
            {
                list.AddRange(GetPointCloudFromPtr(node.Children->_100.Value));
            }
            if (node.Children->_101.HasValue)
            {
                list.AddRange(GetPointCloudFromPtr(node.Children->_101.Value));
            }
            if (node.Children->_110.HasValue)
            {
                list.AddRange(GetPointCloudFromPtr(node.Children->_110.Value));
            }
            if (node.Children->_111.HasValue)
            {
                list.AddRange(GetPointCloudFromPtr(node.Children->_111.Value));
            }

            return list;
        }

        public bool AnyToAdd()
        {
            lock(taskLock)
                return taskQueue.Any();
        }

        //public AAABBB[] GetBoxCloud()
        //{
        //    //return _locationRefs.Select(s => s.Value).ToArray();
        //}

        //Font font = new Font("Arial", 20);
        //private ConcurrentBag<NodeTypeLocation3D> _locationObjects = new ConcurrentBag<NodeTypeLocation3D>();

        //public int Depth()
        //{
        //    var depth = 1;

        //    var collObj = &(OctreeNode.Children);
        //    var _depth = GetDepth(collObj);

        //    return depth + _depth;
        //}

        //private int GetDepth(NodeCollection collObj)
        //{
        //    var depth = 1;

        //    var maxDepth = 0;
        //    if (collObj._000.HasValue)
        //    {
        //        var cD = GetDepth(_octreeHeap[collObj._000.Value.LocCode]);
        //        if (cD > maxDepth)
        //            maxDepth = cD;
        //    }
        //    if (collObj._001.HasValue)
        //    {
        //        var cD = GetDepth(_octreeHeap[collObj._001.Value.LocCode]);
        //        if (cD > maxDepth)
        //            maxDepth = cD;
        //    }
        //    if (collObj._010.HasValue)
        //    {
        //        var cD = GetDepth(_octreeHeap[collObj._010.Value.LocCode]);
        //        if (cD > maxDepth)
        //            maxDepth = cD;
        //    }
        //    if (collObj._011.HasValue)
        //    {
        //        var cD = GetDepth(_octreeHeap[collObj._011.Value.LocCode]);
        //        if (cD > maxDepth)
        //            maxDepth = cD;
        //    }
        //    if (collObj._100.HasValue)
        //    {
        //        var cD = GetDepth(_octreeHeap[collObj._100.Value.LocCode]);
        //        if (cD > maxDepth)
        //            maxDepth = cD;
        //    }
        //    if (collObj._101.HasValue)
        //    {
        //        var cD = GetDepth(_octreeHeap[collObj._101.Value.LocCode]);
        //        if (cD > maxDepth)
        //            maxDepth = cD;
        //    }
        //    if (collObj._110.HasValue)
        //    {
        //        var cD = GetDepth(_octreeHeap[collObj._110.Value.LocCode]);
        //        if (cD > maxDepth)
        //            maxDepth = cD;
        //    }
        //    if (collObj._111.HasValue)
        //    {
        //        var cD = GetDepth(_octreeHeap[collObj._111.Value.LocCode]);
        //        if (cD > maxDepth)
        //            maxDepth = cD;
        //    }

        //    return depth + maxDepth;
        //}

        internal static void MergeHeap(KeyValuePair<IntPtr, KeyValuePair<OctreeNode, NodeCollection>> merge)
        {
            _octreeMerges.Add(new KeyValuePair<IntPtr, KeyValuePair<OctreeNode, NodeCollection>>(merge.Key, merge.Value));
        }

        //public void Draw(Graphics g)
        //{
        //    var head = OctreeNode;
        //    int _drawcount = 0;
        //    Draw(g, head, ref _drawcount, 1);
        //}

        //private void Draw(Graphics g, OctreeNode node, ref int drawCount, int depth)
        //{
        //    var center = (_to - _from) / 2;
        //    var locCode = node.LocCode;
        //    var parentNode = _locationRefs[node.ObjPtr];
        //    //if (depth > 10)
        //    {
        //        //if (!parentNode.IsLayer)
        //        //    g.FillEllipse(Brushes.Black, new Rectangle((int)parentNode.Location.Item1, (int)parentNode.Location.Item2, 10, 10));
        //        //else
        //        //{
        //        //var layerSize = _to * ((float)(1 / Math.Pow(2, depth)));
        //        Pen pen = Pens.Black;
        //        switch (depth)
        //        {
        //            case 2:
        //                pen = Pens.DarkBlue;
        //                break;
        //            case 3:
        //                pen = Pens.DarkCyan;
        //                break;
        //            case 4:
        //                pen = Pens.DarkGreen;
        //                break;
        //            case 5:
        //                pen = Pens.Blue;
        //                break;
        //            case 6:
        //                pen = Pens.Cyan;
        //                break;
        //            case 7:
        //                pen = Pens.Green;
        //                break;
        //            case 8:
        //                pen = Pens.DarkOrange;
        //                break;
        //            case 9:
        //                pen = Pens.DarkSalmon;
        //                break;
        //            case 10:
        //                pen = Pens.DarkRed;
        //                break;
        //            case 11:
        //                pen = Pens.Yellow;
        //                break;
        //            case 12:
        //                pen = Pens.OrangeRed;
        //                break;
        //            case 13:
        //                pen = Pens.Red;
        //                break;
        //            case 14:
        //                pen = Pens.MediumPurple;
        //                break;
        //            case 15:
        //                pen = Pens.Violet;
        //                break;
        //            case 16:
        //                pen = Pens.Pink;
        //                break;
        //        }

        //        g.DrawRectangle(pen, new Rectangle((int)parentNode._from.X, (int)parentNode._from.Y, (int)parentNode._to.X, (int)parentNode._to.Y));


        //        foreach (var cnode in parentNode.ChildLocationItems)
        //            DrawNode(g, cnode, parentNode, ref drawCount);
        //    }
        //    var collectionItem = _octreeHeap[locCode];
        //    if (collectionItem._000.HasValue)
        //    {
        //        //var _obj = _objRefs[collectionItem._000.Value.ObjPtr] as NodeTypeLocation3D;
        //        //g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
        //        //g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
        //        //g.DrawString("_000 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
        //        drawCount++;


        //        Draw(g, collectionItem._000.Value, ref drawCount, depth + 1);
        //    }
        //    if (collectionItem._001.HasValue)
        //    {
        //        //var _obj = _objRefs[collectionItem._001.Value.ObjPtr] as NodeTypeLocation3D;
        //        //g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
        //        //g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
        //        //g.DrawString("_001 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
        //        drawCount++;
        //        Draw(g, collectionItem._001.Value, ref drawCount, depth + 1);
        //    }
        //    if (collectionItem._010.HasValue)
        //    {
        //        //var _obj = _objRefs[collectionItem._010.Value.ObjPtr] as NodeTypeLocation3D;
        //        //g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
        //        //g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
        //        //g.DrawString("_010 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
        //        drawCount++;
        //        Draw(g, collectionItem._010.Value, ref drawCount, depth + 1);
        //    }
        //    if (collectionItem._011.HasValue)
        //    {
        //        //var _obj = _objRefs[collectionItem._011.Value.ObjPtr] as NodeTypeLocation3D;
        //        //g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
        //        //g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
        //        //g.DrawString("_011 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
        //        drawCount++;
        //        Draw(g, collectionItem._011.Value, ref drawCount, depth + 1);
        //    }
        //    if (collectionItem._100.HasValue)
        //    {
        //        //var _obj = _objRefs[collectionItem._100.Value.ObjPtr] as NodeTypeLocation3D;
        //        //g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
        //        //g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
        //        //g.DrawString("_100 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
        //        drawCount++;
        //        Draw(g, collectionItem._100.Value, ref drawCount, depth + 1);
        //    }
        //    if (collectionItem._101.HasValue)
        //    {
        //        //var _obj = _objRefs[collectionItem._101.Value.ObjPtr] as NodeTypeLocation3D;
        //        //g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
        //        //g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
        //        //g.DrawString("_101 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
        //        drawCount++;
        //        Draw(g, collectionItem._101.Value, ref drawCount, depth + 1);
        //    }
        //    if (collectionItem._110.HasValue)
        //    {
        //        //var _obj = _objRefs[collectionItem._110.Value.ObjPtr] as NodeTypeLocation3D;
        //        //g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
        //        //g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
        //        //g.DrawString("_110 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
        //        drawCount++;
        //        Draw(g, collectionItem._110.Value, ref drawCount, depth + 1);
        //    }
        //    if (collectionItem._111.HasValue)
        //    {
        //        //var _obj = _objRefs[collectionItem._111.Value.ObjPtr] as NodeTypeLocation3D;
        //        //g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
        //        //g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
        //        //g.DrawString("_111 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
        //        drawCount++;
        //        Draw(g, collectionItem._111.Value, ref drawCount, depth + 1);
        //    }
        //}

        //private void DrawNode(Graphics g, NodeTypeLocation3D cnode, NodeTypeLayer3D parent, ref int drawCount)
        //{
        //    var center = (parent._to - parent._from) / 2;
        //    g.FillEllipse(Brushes.Black, new Rectangle((int)cnode.Location.Item1, (int)cnode.Location.Item2, 10, 10));
        //    g.DrawLine(Pens.Black, new Point((int)center.X, (int)center.Y), new Point((int)center.X, (int)center.Y));
        //    drawCount++;
        //}

        public int Size()
        {
            return -1;
            //return _locationRefs.SelectMany(s => s.Value.ChildLocationItems).Count();
        }

        unsafe public Octree(Point3D from, Point3D to) : base(from, to)
        {
            var octPtr = (IntPtr)GCHandle.Alloc(this);

            var _locationNode = new NodeTypeLayer3D(from, to, octPtr);
            var ptr = (IntPtr)GCHandle.Alloc(_locationNode);
            //_locationRefs.TryAdd(ptr, _locationNode);


            var defaultNodeCollection = new NodeCollection();
            var nodeCollectionPtr  =  &defaultNodeCollection;
            OctreeNode = new OctreeNode(nodeCollectionPtr, null, 0b1000, 0, NodeType.Location, ptr, 0);
            //_octreeHeap.TryAdd();
            _octreeMerges.Add(new KeyValuePair<IntPtr, KeyValuePair<OctreeNode, NodeCollection>> (octPtr, new KeyValuePair<OctreeNode, NodeCollection>(OctreeNode, defaultNodeCollection)));
            //_octreeNodes.Add(OctreeNode);
            _taskThread = new Thread(ProcessStack);
            _taskThread.Start();
            _mergeThread = new Thread(ProcessMerges);
            _mergeThread.Start();
        }

        private unsafe void ProcessMerges()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(10);
                
                List<KeyValuePair<IntPtr, KeyValuePair<OctreeNode, NodeCollection>>> copy = new List<KeyValuePair<IntPtr, KeyValuePair<OctreeNode, NodeCollection>>>(); ;
                if (_octreeMerges.Any())
                lock (_octreeMerges)
                {
                        copy = _octreeMerges.ToList();
                        _octreeMerges = new ConcurrentBag<KeyValuePair<IntPtr, KeyValuePair<OctreeNode, NodeCollection>>>();
                        
                }
                //foreach (var itm in copy)
                //{
                //    mergeList.Add(new KeyValuePair<IntPtr, Dictionary<ulong, NodeCollection>>(itm.Key, itm.Value.ToDictionary(k => k.Key, v => v.Value)));
                //}
                var groupList = copy.GroupBy(k => k.Key);
                foreach (var grouping in groupList)
                {
                    var ptr = grouping.Key;
                    var list = grouping.Select(l => l.Value).ToList();

                    Dictionary<ulong, NodeCollection> mergedDictionary = new Dictionary<ulong, NodeCollection>();
                    foreach(var item in list)
                    {
                        var key = item.Key.LocCode;
                        if (!mergedDictionary.ContainsKey(key))
                        {
                            var srcColl = item.Key.Children;
                            mergedDictionary.Add(key, *srcColl);
                        }

                        var currCollection = item.Value;
                        var collection = mergedDictionary[key];

                        //MergeCollection(currCollection._000, collection._000);
                        collection._000 = MergeCollection(collection._000, currCollection._000 ?? null);
                        collection._100 = MergeCollection(collection._100, currCollection._100 ?? null);
                        collection._010 = MergeCollection(collection._010, currCollection._010 ?? null);
                        collection._110 = MergeCollection(collection._110, currCollection._110 ?? null);
                        collection._001 = MergeCollection(collection._001, currCollection._001 ?? null);
                        collection._101 = MergeCollection(collection._101, currCollection._101 ?? null);
                        collection._011 = MergeCollection(collection._011, currCollection._011 ?? null);
                        collection._111 = MergeCollection(collection._111, currCollection._111 ?? null);
                        mergedDictionary[key] = collection;
                        
                    }

                    //Dictionary<ulong, NodeCollection> octPtr = null;
                    //foreach (var item in mergedDictionary)
                    //{
                    //    if(octPtr == null)
                    //        if (OctreeHeaps.ContainsKey(ptr))
                    //            octPtr = OctreeHeaps[ptr];
                    //        else
                    //        {
                    //            octPtr = new Dictionary<ulong, NodeCollection>();
                    //            OctreeHeaps.Add(ptr, octPtr);
                    //        }
                    //    if (octPtr.ContainsKey(item.Key))
                    //        octPtr[item.Key] = item.Value;
                    //    else
                    //        octPtr.Add(item.Key, item.Value);

                    //}
                    //OctreeHeaps[ptr] = mergedDictionary;
                }
            }
        }

        //private OctreeNode GetOctreeNodeByLocation(ulong key)
        //{
        //    if (OctreeNode.LocCode == key)
        //        return OctreeNode;
            

        //}

        private unsafe OctreeNode? MergeCollection(OctreeNode? v1, OctreeNode? v2)
        {
            if (!v1.HasValue)
                return v2;
            if (!v2.HasValue)
                return v1;

            var handle = GCHandle.FromIntPtr(v1.Value.ObjPtr);
            var handle2 = GCHandle.FromIntPtr(v2.Value.ObjPtr);
            var to = (NodeTypeLayer3D)handle.Target;
            var from = (NodeTypeLayer3D)handle2.Target;
            var cList = from.ClearChildren();
            foreach (var item in cList)
                to.Add(item);
            return v1;
               
        }

        private void ProcessStack()
        {
            while(true)
            {
                List<Task> toRun = new List<Task>();
                lock (taskLock)
                    if (taskQueue.Any())
                    {
                        toRun.AddRange(taskQueue.ToList());
                        taskQueue = new List<Task>();
                    }
                toRun.ForEach(task => task.Start());
                //Task.Factory.StartNew(() => );

                //var procThread = new Thread(() => {
                //    var ts = DateTime.UtcNow.Ticks;
                //    Parallel.ForEach(toRun, new ParallelOptions() { MaxDegreeOfParallelism = 10000 }, (task) => { task.Start(); });
                //    var e_ts = DateTime.UtcNow.Ticks;
                //    var diff = (e_ts - ts) / TimeSpan.TicksPerMillisecond;
                //    Console.WriteLine($"O:{diff}:{toRun.Count}");
                //});
                //procThread.Start();
                //foreach(var task in toRun)
                //{
                //    task.Start();
                //}
            }

        }

        public void AddAsync(float x, float y, float z)
        {
            lock (taskLock)
                taskQueue.Add(new Task(() => { Add(x, y, z); }));
        }
        private void Add(float x, float y, float z)
        {
            var location = new NodeTypeLocation3D(x, y, z, false);
            Add(OctreeNode, location);
        }

        private void AddAsyncRec(OctreeNode parentNode, NodeTypeLocation3D pointLocation, int depth = 4)
        {
            lock (taskLock)
                taskQueue.Add(new Task(() => { 
                    Add(parentNode, pointLocation, depth);
                    //var ptr = (IntPtr)GCHandle.Alloc(pointLocation);
                    //lock (_objRefsLock)
                    //    _objRefs.Add(pointLocation);
                }));
        }

        unsafe private void Add(OctreeNode parentNode, NodeTypeLocation3D pointLocation, int depth = 4)
        {
            if (pointLocation == null)
                return;

            var handle = GCHandle.FromIntPtr(parentNode.ObjPtr);
            var _compareTo = (NodeTypeLayer3D)handle.Target;

            _compareTo.LoadTask(_compareTo.AddAsyncTask(parentNode, pointLocation, depth));
            //_compareTo.Add(parentNode, pointLocation, depth)

        
        }

    }
}
