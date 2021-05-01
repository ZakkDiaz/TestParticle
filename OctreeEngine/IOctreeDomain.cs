using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OctreeEngine
{
    public interface IOctreeDomain
    {
        ReadOnlyDictionary<IntPtr, Octree> Octrees { get; }
    }
}