using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticleLib.Models._3D
{
    public class GravityParticleProcessor : IParticleProcessor
    {
        public void Process(OctreeNode octreeNode, ref ConcurrentDictionary<IntPtr, NodeTypeLayer3D> locationRefs, ref ConcurrentDictionary<ulong, NodeCollection> octreeHeap)
        {
            var startLocation = locationRefs[octreeNode.ObjPtr];

            if (octreeHeap.ContainsKey(octreeNode.LocCode))
            {
                var startCollection = octreeHeap[octreeNode.LocCode];
                ProcessCollection(startCollection, startLocation);
            }
            else
            {
                ProcessLocationForces(startLocation);
            }


        }

        private void ProcessLocationForces(NodeTypeLayer3D startLocation)
        {
            throw new NotImplementedException();
        }

        private void ProcessCollection(NodeCollection startCollection, NodeTypeLayer3D startLocation)
        {
            throw new NotImplementedException();
        }
    }
}
