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
        private ConcurrentQueue<Tuple<ulong, OctreeCellCollection>> _collectionUpdates = new ConcurrentQueue<Tuple<ulong, OctreeCellCollection>>();
        private ConcurrentQueue<IEnumerable<Particle>> _addParticles = new ConcurrentQueue<IEnumerable<Particle>>();
        private ConcurrentQueue<IEnumerable<Tuple<ulong, byte, Particle>>> _flowParticles = new ConcurrentQueue<IEnumerable<Tuple<ulong, byte, Particle>>>();

        private Thread _octreeCollectionThread;

        public Octree(Point3D from, Point3D to) : base(from, to)
        {
            _octreeCollectionThread = new Thread(RunOctreeLoop);
            _octreeCollectionThread.Start();

            _octreeCollections = new Dictionary<ulong, OctreeCellCollection>();

            var headLoc = (ulong)0x1000b;
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
                List<Particle> particles = new List<Particle>();
                while(!_addParticles.IsEmpty)
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

                while (!_collectionUpdates.IsEmpty)
                {
                    _mergeList.Clear();
                    if (_collectionUpdates.TryDequeue(out Tuple<ulong, OctreeCellCollection> collection))
                    {
                        _mergeList.Add(collection);
                    }
                }
                
                foreach(var ml in _mergeList)
                {
                    if (!_octreeCollections.ContainsKey(ml.Item1))
                        _octreeCollections.Add(ml.Item1, ml.Item2);
                    else
                    {
                        var flowList = _octreeCollections[ml.Item1].AddAll(ml.Item2.Flush());
                        if(flowList.Any())
                            _flowParticles.Enqueue(flowList);
                    }
                }

                while(!_flowParticles.IsEmpty)
                {
                    var flowParticles = new List<Tuple<ulong, byte, Particle>>();
                    if(_flowParticles.TryDequeue(out var flowResult))
                    {
                        flowParticles.AddRange(flowResult);
                    }
                    var cellGroups = flowParticles.GroupBy(b => b.Item1);
                    foreach(var group in cellGroups)
                    {
                        MapCellGroupToOctree(group.Key, group.ToList(), head);
                    }
                }

                System.Threading.Thread.Sleep(100);
            }
        }

        private void MapCellGroupToOctree(ulong key, IEnumerable<Tuple<ulong, byte, Particle>> quadGrp, OctreeCell head)
        {
            OctreeCell current = head;
            ulong _location = current._location;

            while(_location < key)
            {
                var loc = _octreeCollections[_location];
                var half = (loc.To - loc.From) / 2;
                var center = loc.From + half;
                OctreeCellCollection.GetQuad()
            }

            foreach (var qg in quadGrp)
            {
                MapQuadGroupToOctree(qg, head);
            }
        }

        private void MapQuadGroupToOctree(IGrouping<byte, Tuple<ulong, byte, Particle>> qg, OctreeCell head)
        {

            switch (qg.Key)
            {
                case unchecked((byte)0b1111):

                    break;
                case unchecked((byte)0b1110):
                    break;
                case unchecked((byte)0b1101):
                    break;
                case unchecked((byte)0b1100):
                    break;
                case unchecked((byte)0b1011):
                    break;
                case unchecked((byte)0b1010):
                    break;
                case unchecked((byte)0b1001):
                    break;
                case unchecked((byte)0b1000):
                    break;
            }
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
