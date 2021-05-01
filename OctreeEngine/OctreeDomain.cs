using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OctreeEngine
{
    public class OctreeDomain : IOctreeDomain
    {
        public Dictionary<IntPtr, Octree> _octrees = new Dictionary<IntPtr, Octree>();
        public ReadOnlyDictionary<IntPtr, Octree> Octrees => new ReadOnlyDictionary<IntPtr, Octree>(_octrees);
    }
}
