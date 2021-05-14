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
            //ulong curVal = newLocation;
            //ulong nextVal = curVal;

            //ulong _last = ulong.MaxValue;

            //while (nextVal != 0)
            //{
            //    _last = nextVal;
            //    nextVal = nextVal >> 0b100;
            //}
            return (byte)(newLocation & 0b1111);
            //return (byte)_new;
            //return (byte)_last;
        }
        internal static ulong GetNextKey(ulong _location, ulong key)
        {
            var toDepth = Helpers.GetDepth(key);
            var nextDepth = Helpers.GetDepth(_location) + 1;
            var diff = toDepth - nextDepth;
            ulong _key = key;
            for (var i = 0; i < diff; i++)
            {
                _key = _key >> 4;
            }

            return _key;
        }
        internal unsafe static void NavigateTo(OctreeCell fromCell, ulong key, out OctreeCell outCell)
        {
            ulong _key = Helpers.GetNextKey(fromCell._location, key);
            var quad = Helpers.GetQuad(_key);
            var fromDepth = Helpers.GetDepth(fromCell._location);
            var toDepth = Helpers.GetDepth(key);
            var diff = toDepth - fromDepth;
            if (diff == 0)
            {
                outCell = fromCell;
                return;
            }
            if (diff > 1)
            {
                switch (quad)
                {
                    case unchecked((byte)0b1111):
                        NavigateTo(fromCell._111, key, out outCell);
                        return;
                    case unchecked((byte)0b1110):
                        NavigateTo(fromCell._110, key, out outCell);
                        return;
                    case unchecked((byte)0b1101):
                        NavigateTo(fromCell._101, key, out outCell);
                        return;
                    case unchecked((byte)0b1100):
                        NavigateTo(fromCell._100, key, out outCell);
                        return;
                    case unchecked((byte)0b1011):
                        NavigateTo(fromCell._011, key, out outCell);
                        return;
                    case unchecked((byte)0b1010):
                        NavigateTo(fromCell._010, key, out outCell);
                        return;
                    case unchecked((byte)0b1001):
                        NavigateTo(fromCell._001, key, out outCell);
                        return;
                    case unchecked((byte)0b1000):
                        NavigateTo(fromCell._000, key, out outCell);
                        return;
                    default:
                        throw new Exception("Attempt to navigate to unallocated cell");
                }
            }

            switch (quad)
            {
                case unchecked((byte)0b1111):
                    outCell = fromCell._111;
                    return;
                case unchecked((byte)0b1110):
                    outCell = fromCell._110;
                    return;
                case unchecked((byte)0b1101):
                    outCell = fromCell._101;
                    return;
                case unchecked((byte)0b1100):
                    outCell = fromCell._100;
                    return;
                case unchecked((byte)0b1011):
                    outCell = fromCell._011;
                    return;
                case unchecked((byte)0b1010):
                    outCell = fromCell._010;
                    return;
                case unchecked((byte)0b1001):
                    outCell = fromCell._001;
                    return;
                case unchecked((byte)0b1000):
                    outCell = fromCell._000;
                    return;
                default:
                    throw new Exception("Attempt to navigate to unallocated cell");
            }
            throw new Exception("Attempt to navigate to unallocated cell");



            //ulong curVal = key;
            //ulong nextVal = curVal;
            //int _valDepth = 0;
            //int curDepth = Helpers.GetDepth(_location);
            //int nextDepth = Helpers.GetDepth(key);
            //ulong _last = ulong.MaxValue;
            //ulong _last2 = _last;
            //for(var i = 0; i < nextDepth - curDepth; i++)
            //{
            //    _last2 = _last;
            //    _last = nextVal;
            //    nextVal = nextVal >> 4;
            //}

            //return NavigateOrCreateCellByByte(_location, key, (byte)(nextByte));
        }
    }
}
