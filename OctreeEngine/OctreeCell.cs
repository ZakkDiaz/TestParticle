using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OctreeEngine
{
    public unsafe struct OctreeCell
    {
        public byte _qaud { get; set; }
        public ulong _location { get; set; }
        public OctreeCell?* _parent { get; set; }
        public OctreeCell?* _111 { get; set; }
        public OctreeCell?* _110 { get; set; }
        public OctreeCell?* _101 { get; set; }
        public OctreeCell?* _100 { get; set; }
        public OctreeCell?* _011 { get; set; }
        public OctreeCell?* _010 { get; set; }
        public OctreeCell?* _001 { get; set; }
        public OctreeCell?* _000 { get; set; }
    }
}
