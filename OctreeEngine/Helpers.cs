using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OctreeEngine
{
    public static class Helpers
    {
        public static int GetDepth(ulong key)
        {
            int depth = 0;
            var _cDepth = key;
            while (_cDepth > 0)
            {
                _cDepth = _cDepth >> 4;
                depth += 1;
            }
            return depth;
        }

        internal static byte GetQuad(ulong newLocation)
        {
            ulong curVal = newLocation;
            ulong nextVal = curVal;

            ulong _last = ulong.MaxValue;
            while (nextVal != 0)
            {
                _last = nextVal;
                nextVal = nextVal >> 4;
            }
            return (byte)_last;
        }
    }
}
