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
        public static ConcurrentBag<KeyValuePair<IntPtr, KeyValuePair<OctreeNode, NodeCollection>>> _octreeMerges = new ConcurrentBag<KeyValuePair<IntPtr, KeyValuePair<OctreeNode, NodeCollection>>>();
        //public static ConcurrentQueue<Tuple<IntPtr, NodeTypeLayer3D>> _addLayers = new ConcurrentQueue<Tuple<IntPtr, NodeTypeLayer3D>>();

        public static Dictionary<ulong, NodeTypeLayer3D> _layers = new Dictionary<ulong, NodeTypeLayer3D>();
        public static Dictionary<ulong, OctreeNode> _nodes = new Dictionary<ulong, OctreeNode>();

        object taskLock = new object();
        List<Task> taskQueue = new List<Task>();
        Thread _taskThread;
        Thread _mergeThread;
        public OctreeNode OctreeNode { get; set; }

        public Point3D[] GetPointCloud()
        {
            return GetPointCloudFromPtr(OctreeNode).ToArray();
        }

        public unsafe IEnumerable<Point3D> GetPointCloudFromPtr(OctreeNode node)
        {
            //var ptr = node.ObjPtr;
            //var handle = GCHandle.FromIntPtr(ptr);
            //var layer = (NodeTypeLayer3D)handle.Target;
            var layer = _layers[node.LocCode];
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

        internal static void MergeHeap(KeyValuePair<IntPtr, KeyValuePair<OctreeNode, NodeCollection>> merge)
        {
            _octreeMerges.Add(new KeyValuePair<IntPtr, KeyValuePair<OctreeNode, NodeCollection>>(merge.Key, merge.Value));
        }
        public int Size()
        {
            return -1;
        }

        unsafe public Octree(Point3D from, Point3D to) : base(from, to)
        {
            var octPtr = (IntPtr)GCHandle.Alloc(this);

            ulong location = (0b1000);

            var _locationNode = new NodeTypeLayer3D(from, to, location);
            var ptr = (IntPtr)GCHandle.Alloc(_locationNode);

            _layers[location] = _locationNode;
            //_addLayers.Enqueue(new Tuple<IntPtr, NodeTypeLayer3D>(ptr, _locationNode));

            var defaultNodeCollection = new NodeCollection();
            var nodeCollectionPtr  =  &defaultNodeCollection;
            OctreeNode = new OctreeNode(nodeCollectionPtr, null, location, 0, NodeType.Location, 0, octPtr);
            _octreeMerges.Add(new KeyValuePair<IntPtr, KeyValuePair<OctreeNode, NodeCollection>> (octPtr, new KeyValuePair<OctreeNode, NodeCollection>(OctreeNode, defaultNodeCollection)));
            _taskThread = new Thread(ProcessStack);
            _taskThread.Start();
            _mergeThread = new Thread(ProcessMerges);
            _mergeThread.Start();
        }

        private unsafe void ProcessMerges()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(100);
                
                List<KeyValuePair<IntPtr, KeyValuePair<OctreeNode, NodeCollection>>> copy = new List<KeyValuePair<IntPtr, KeyValuePair<OctreeNode, NodeCollection>>>(); ;
                if (_octreeMerges.Any())
                lock (_octreeMerges)
                {
                        copy = _octreeMerges.ToList();
                        _octreeMerges = new ConcurrentBag<KeyValuePair<IntPtr, KeyValuePair<OctreeNode, NodeCollection>>>();
                        
                }

                //MergeLists();

                var groupList = copy.GroupBy(k => k.Key);
                foreach (var grouping in groupList)
                {
                    var ptr = grouping.Key;
                    var list = grouping.Select(l => l.Value).ToList();

                    Dictionary<ulong, NodeCollection> mergedDictionary = new Dictionary<ulong, NodeCollection>();
                    foreach(var item in list)
                    {
                        if (!_nodes.ContainsKey(item.Key.LocCode))
                            _nodes.Add(item.Key.LocCode, item.Key);
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

                        //NodeCollection* colPtr = &collection;
                        //if (item.Key.Parent != null && item.Key.Parent->HasValue)
                        //{
                        //    var parent = item.Key.Parent->Value;
                        //    var pColl = *parent.Children;
                        //    OctreeNode toRepoint;
                        //    switch (item.Key.Quadrant)
                        //    {
                        //        case unchecked((byte)0b1111):
                        //            {
                        //                toRepoint = pColl._111.Value;
                        //            }
                        //            break;
                        //        case unchecked((byte)0b1110):
                        //            {
                        //                toRepoint = pColl._110.Value;
                        //            }
                        //            break;
                        //        case unchecked((byte)0b1101):
                        //            {
                        //                toRepoint = pColl._101.Value;
                        //            }
                        //            break;
                        //        case unchecked((byte)0b1100):
                        //            {
                        //                toRepoint = pColl._100.Value;
                        //            }
                        //            break;
                        //        case unchecked((byte)0b1011):
                        //            {
                        //                toRepoint = pColl._011.Value;
                        //            }
                        //            break;
                        //        case unchecked((byte)0b1010):
                        //            {
                        //                toRepoint = pColl._010.Value;
                        //            }
                        //            break;
                        //        case unchecked((byte)0b1001):
                        //            {
                        //                toRepoint = pColl._001.Value;
                        //            }
                        //            break;
                        //        case unchecked((byte)0b1000):
                        //            {
                        //                toRepoint = pColl._000.Value;
                        //            }
                        //            break;
                        //    }
                        //    toRepoint.Children = colPtr;
                        //} else
                        //{
                        //    item.Key.Children = colPtr;
                        //}

                        //foreach(var layer in _layers)
                        //{
                        //    if(layer.Key == item.Key.LocCode)
                        //    {
                        //        _layers[layer.Key].
                        //    }
                        //}
                    }

                    foreach (var m in mergedDictionary)
                    {
                        var obj = _nodes[m.Key];
                        var coll = m.Value;
                        var colPtr = &coll;
                        obj.Children = colPtr;

                        if (obj.Parent != null && obj.Parent->HasValue)
                        {
                            var parent = obj.Parent->Value;
                            var pColl = *parent.Children;
                            OctreeNode toRepoint;
                            switch (obj.Quadrant)
                            {
                                case unchecked((byte)0b1111):
                                    {
                                        toRepoint = pColl._111.Value;
                                    }
                                    break;
                                case unchecked((byte)0b1110):
                                    {
                                        toRepoint = pColl._110.Value;
                                    }
                                    break;
                                case unchecked((byte)0b1101):
                                    {
                                        toRepoint = pColl._101.Value;
                                    }
                                    break;
                                case unchecked((byte)0b1100):
                                    {
                                        toRepoint = pColl._100.Value;
                                    }
                                    break;
                                case unchecked((byte)0b1011):
                                    {
                                        toRepoint = pColl._011.Value;
                                    }
                                    break;
                                case unchecked((byte)0b1010):
                                    {
                                        toRepoint = pColl._010.Value;
                                    }
                                    break;
                                case unchecked((byte)0b1001):
                                    {
                                        toRepoint = pColl._001.Value;
                                    }
                                    break;
                                case unchecked((byte)0b1000):
                                    {
                                        toRepoint = pColl._000.Value;
                                    }
                                    break;
                            }
                            toRepoint.Children = colPtr;
                        }
                    }
                    //Octree.UpsertMerge(ptr, mergedDictionary);




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

        private static void UpsertMerge(IntPtr ptr, Dictionary<ulong, NodeCollection> mergedDictionary)
        {
            throw new NotImplementedException();
        }



        //private void MergeLists()
        //{
        //    while (!_addLayers.IsEmpty)
        //    {
        //        if (_addLayers.TryDequeue(out Tuple<IntPtr, NodeTypeLayer3D> res))
        //        {
        //            _layers[res.Item1] = res.Item2;
        //        }
        //    }
        //}

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

            //var handle = GCHandle.FromIntPtr(v1.Value.ObjPtr);
            //var handle2 = GCHandle.FromIntPtr(v2.Value.ObjPtr);
            //var to = (NodeTypeLayer3D)handle.Target;
            //var from = (NodeTypeLayer3D)handle2.Target;

            int _count = 0;

            while(_count < 10)
            {
                if (!_layers.ContainsKey(v1.Value.LocCode))
                    System.Threading.Thread.Sleep(100);
                if (!_layers.ContainsKey(v2.Value.LocCode))
                    System.Threading.Thread.Sleep(100);
                _count++;
                //MergeLists();
            }

            if (!_layers.ContainsKey(v1.Value.LocCode))
                return v2;
            if (!_layers.ContainsKey(v2.Value.LocCode))
                return v1;
            //MergeLists();
            var from = _layers[v1.Value.LocCode];
            var to = _layers[v2.Value.LocCode];
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
                System.Threading.Thread.Sleep(100);
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

            //var handle = GCHandle.FromIntPtr(parentNode.ObjPtr);
            //var _compareTo = (NodeTypeLayer3D)handle.Target;

            var pl = _layers[parentNode.LocCode];
            pl.LoadTask(pl.AddAsyncTask(parentNode, pointLocation, depth));

            //_compareTo.LoadTask(_compareTo.AddAsyncTask(parentNode, pointLocation, depth));
            //_compareTo.Add(parentNode, pointLocation, depth)

        
        }

    }
}
