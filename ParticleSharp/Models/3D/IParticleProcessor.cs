using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ParticleLib.Models._3D
{
    public interface IParticleProcessor
    {
        void Process(OctreeNode octreeNode, ref ConcurrentDictionary<IntPtr, NodeTypeLayer3D> locationRefs, ref ConcurrentDictionary<ulong, NodeCollection> octreeHeap);
    }
}