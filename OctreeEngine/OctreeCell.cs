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
        public OctreeCell* _111 { get; set; }
        public OctreeCell* _110 { get; set; }
        public OctreeCell* _101 { get; set; }
        public OctreeCell* _100 { get; set; }
        public OctreeCell* _011 { get; set; }
        public OctreeCell* _010 { get; set; }
        public OctreeCell* _001 { get; set; }
        public OctreeCell* _000 { get; set; }

        internal OctreeCell NavigateOrCreateCellByByte(ulong from, ulong key, byte quad)
        {
            OctreeCell ret;
            switch (quad)
            {
                case unchecked((byte)0b1111):
                    if (this._111 != null)
                        return this._111->NavigateTo(from, key);
                    else
                    {
                        ret = GetOrCreateOctreeCell(this, key, quad, this._111);
                        this._111 = &ret;
                    }
                    break;
                case unchecked((byte)0b1110):
                    if (this._110 != null)
                        return this._110->NavigateTo(from, key);
                    else
                    {
                        ret = GetOrCreateOctreeCell(this, key, quad, this._110);
                        this._110 = &ret;
                    }
                    break;
                case unchecked((byte)0b1101):
                    if (this._101 != null)
                        return this._101->NavigateTo(from, key);
                    else
                    {
                        ret = GetOrCreateOctreeCell(this, key, quad, this._101);
                        this._101 = &ret;
                    }
                    break;
                case unchecked((byte)0b1100):
                    if (this._100 != null)
                        return this._100->NavigateTo(from, key);
                    else
                    {
                        ret = GetOrCreateOctreeCell(this, key, quad, this._100);
                        this._100 = &ret;
                    }
                    break;
                case unchecked((byte)0b1011):
                    if (this._011 != null)
                        return this._011->NavigateTo(from, key);
                    else
                    {
                        ret = GetOrCreateOctreeCell(this, key, quad, this._011);
                        this._011 = &ret;
                    }
                    break;
                case unchecked((byte)0b1010):
                    if (this._010 != null)
                        return this._010->NavigateTo(from, key);
                    else
                    {
                        ret = GetOrCreateOctreeCell(this, key, quad, this._010);
                        this._010 = &ret;
                    }
                    break;
                case unchecked((byte)0b1001):
                    if (this._001 != null)
                        return this._001->NavigateTo(from, key);
                    else
                    {
                        ret = GetOrCreateOctreeCell(this, key, quad, this._001);
                        this._001 = &ret;
                    }
                    break;
                case unchecked((byte)0b1000):
                    if (this._000 != null)
                        return this._000->NavigateTo(from, key);
                    else
                    {
                        ret = GetOrCreateOctreeCell(this, key, quad, this._000);
                        this._000 = &ret;
                    }
                    break;
                default:
                    throw new Exception("Unrecognized bytecode");
                    ret = new OctreeCell();
                    break;
            }
            return ret;
        }

        private OctreeCell GetOrCreateOctreeCell(OctreeCell? parent, ulong key, byte quad, OctreeCell* cell)
        {
            if (cell != null)
                return *cell;

            OctreeCell _cell = new OctreeCell();

            _cell._location = key;
            _cell._parent = &parent;
            _cell._qaud = quad;
            

            return _cell;
        }

        internal OctreeCell NavigateTo(ulong from, ulong key)
        {
            if (key == _location)
                return this;
            if (key < _location)
                return _parent->Value.NavigateTo(_location, key);

            var nextByte = Helpers.GetQuad(from);

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

            return NavigateOrCreateCellByByte(_location, key, (byte)(nextByte));
        }
    }
}
