using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OctreeEngine
{
    public class Octree : AAABBB
    {
        OctreeCell head;
        Dictionary<ulong, OctreeCellCollection> _octreeCollections;
        public ReadOnlyDictionary<ulong, OctreeCellCollection> OctreeCollections => new ReadOnlyDictionary<ulong, OctreeCellCollection>(_octreeCollections);

        public void Start()
        {
            _octreeCollectionThread.Start();
        }

        private ConcurrentQueue<Tuple<ulong, OctreeCellCollection>> _collectionUpdates = new ConcurrentQueue<Tuple<ulong, OctreeCellCollection>>();
        private ConcurrentQueue<IEnumerable<Particle>> _addParticles = new ConcurrentQueue<IEnumerable<Particle>>();
        private ConcurrentQueue<IEnumerable<Tuple<ulong, byte, Particle>>> _flowParticles = new ConcurrentQueue<IEnumerable<Tuple<ulong, byte, Particle>>>();
        private List<Tuple<ulong, byte, Particle>> __flowParticles = new List<Tuple<ulong, byte, Particle>>();

        private Thread _octreeCollectionThread;

        public Octree(Point3D from, Point3D to) : base(from, to)
        {
            _octreeCollectionThread = new Thread(RunOctreeLoop);

            _octreeCollections = new Dictionary<ulong, OctreeCellCollection>();

            ulong headLoc = 0b1000;
            head = MakeCell(headLoc);
            AddCell(from, to, headLoc);
        }

        public void AddMany(IEnumerable<Particle> particles)
        {
            _addParticles.Enqueue(particles);
        }

        private void RunOctreeLoop()
        {
            List<Tuple<ulong, OctreeCellCollection>> _mergeList = new List<Tuple<ulong, OctreeCellCollection>>();
            while (true)
            {

                ProcessMergelist();

                List<Particle> particles = new List<Particle>();
                while (!_addParticles.IsEmpty)
                {
                    if (_addParticles.TryDequeue(out IEnumerable<Particle> list))
                        particles.AddRange(list);
                }

                OctreeCollections[head._location].particles.AddRange(particles);

                foreach(var collection in _octreeCollections)
                {
                    if(collection.Value.Overflow)
                    {
                        var allParticles = collection.Value.Flush();
                        var flowList = collection.Value.AddAll(allParticles);
                        if(flowList.Any())
                            _flowParticles.Enqueue(flowList);
                    }
                }

                //var flowParticles = new List<Tuple<ulong, byte, Particle>>();
                __flowParticles = new List<Tuple<ulong, byte, Particle>>();
                while (!_flowParticles.IsEmpty)
                {
                    if(_flowParticles.TryDequeue(out var flowResult))
                    {
                        __flowParticles.AddRange(flowResult);
                    }
                }
                var cellGroups = __flowParticles.GroupBy(b => b.Item1);
                foreach (var group in cellGroups)
                {
                    //_octreeCollections[group.Key].particles.AddRange(group.Select(g => g.Item3));
                    MapCellGroupToOctree(group.Key, head);
                    ProcessMergelist();

                    _octreeCollections[group.Key].particles.AddRange(group.Select(s => s.Item3));
                }

                System.Threading.Thread.Sleep(100);
            }
        }

        private void ProcessMergelist()
        {
            var mergeList = new List<Tuple<ulong, OctreeCellCollection>>();
            while (!_collectionUpdates.IsEmpty)
            {
                if (_collectionUpdates.TryDequeue(out Tuple<ulong, OctreeCellCollection> collection))
                {
                    mergeList.Add(collection);

                }
            }
            foreach (var ml in mergeList)
            {
                if (!_octreeCollections.ContainsKey(ml.Item1))
                    _octreeCollections.Add(ml.Item1, ml.Item2);
                else
                {
                    var flowList = _octreeCollections[ml.Item1].AddAll(ml.Item2.Flush());
                    if (flowList.Any())
                        _flowParticles.Enqueue(flowList);
                }
            }
        }

        private void MapCellGroupToOctree(ulong key, OctreeCell head)
        {
            OctreeCell current = head;
            ulong _location = current._location;
            current = current.NavigateTo(_location, key);

            if (!_octreeCollections.ContainsKey(current._location))
            {
                var depth = Helpers.GetDepth(current._location);
                AddCellForLocation(this.From, this.To, _location, current._location);
            }
        }

        private void AddCellForLocation(Point3D from, Point3D to, ulong fromLoc, ulong newLocation)
        {
            var fromDepth = Helpers.GetDepth(fromLoc);
            var nextDepth = fromDepth + 1;
            var toDepth = Helpers.GetDepth(newLocation);
            var totSize = (to - from);
            var half = (totSize) / (float)Math.Pow(2, toDepth - fromDepth);
            var center = from + half;
            var quad = Helpers.GetQuad(newLocation);

            Point3D fromOff = null;

            switch (quad)
            {
                case unchecked((byte)0b1111):
                    fromOff = new Point3D(half.X, half.Y, half.Z);
                    break;
                case unchecked((byte)0b1110):
                    fromOff = new Point3D(0, half.Y, half.Z);
                    break;
                case unchecked((byte)0b1101):
                    fromOff = new Point3D(half.X, 0, half.Z);
                    break;
                case unchecked((byte)0b1100):
                    fromOff = new Point3D(0, 0, half.Z);
                    break;
                case unchecked((byte)0b1011):
                    fromOff = new Point3D(half.X, half.Y, 0);
                    break;
                case unchecked((byte)0b1010):
                    fromOff = new Point3D(0, half.Y, 0);
                    break;
                case unchecked((byte)0b1001):
                    fromOff = new Point3D(half.X, 0, 0);
                    break;
                case unchecked((byte)0b1000):
                    fromOff = new Point3D(0, 0, 0);
                    break;
            }


            if (nextDepth == toDepth)
            {
                AddCell(from + fromOff, from + fromOff + half, newLocation);
            }
            else
            {
                AddCellForLocation(from + fromOff, from + fromOff + half, fromLoc << 4, newLocation);
            }

        }

        private void MapQuadGroupToOctree(IEnumerable<Tuple<ulong, byte, Particle>> qg, OctreeCell addTo)
        {
            //var byteGrpList = qg.GroupBy(g => g.Item2).ToList();
            //foreach (var byteGrp in byteGrpList)
            //    switch (byteGrp.Key)
            //    {
            //        case unchecked((byte)0b1111):
            //            addTo.GetCellByByte(byteGrp.Key)._location
            //            break;
            //        case unchecked((byte)0b1110):
            //            break;
            //        case unchecked((byte)0b1101):
            //            break;
            //        case unchecked((byte)0b1100):
            //            break;
            //        case unchecked((byte)0b1011):
            //            break;
            //        case unchecked((byte)0b1010):
            //            break;
            //        case unchecked((byte)0b1001):
            //            break;
            //        case unchecked((byte)0b1000):
            //            break;
            //    }
        }

        private void AddCell(Point3D from, Point3D to, ulong location)
        {
            _collectionUpdates.Enqueue(new Tuple<ulong, OctreeCellCollection>(location, new OctreeCellCollection(from, to, location)));
        }

        private OctreeCell MakeCell(ulong location)
        {
            var cell = new OctreeCell();
            cell._location = location;
            return cell;
        }
    }
}
