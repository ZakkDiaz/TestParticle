using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OctreeEngine
{
    public unsafe class OctreeCell
    {
        public byte _qaud { get; set; }
        public ulong _location { get; set; }
        public OctreeCell _parent { get; set; }
        public OctreeCell _111 { get; set; }
        public OctreeCell _110 { get; set; }
        public OctreeCell _101 { get; set; }
        public OctreeCell _100 { get; set; }
        public OctreeCell _011 { get; set; }
        public OctreeCell _010 { get; set; }
        public OctreeCell _001 { get; set; }
        public OctreeCell _000 { get; set; }

        internal void CreateCellByByte(ulong key)
        {
            ulong _key = Helpers.GetNextKey(_location, key);
            var quad = Helpers.GetQuad(_key);
            var fromDepth = Helpers.GetDepth(_location);
            var toDepth = Helpers.GetDepth(key);
            var diff = toDepth - fromDepth;
            if (diff == 0)
                return;
            if (diff > 1)
            {
                Helpers.NavigateTo(this, _key, out OctreeCell nextCell);
                nextCell.CreateCellByByte(key);
            }



            OctreeCell assign;
            switch (quad)
            {
                case unchecked((byte)0b1111):
                    //Helpers.NavigateTo(ref this._111, _key, out OctreeCell nextCell);
                    assign = GetOrCreateOctreeCell(this, key, quad, this._111);
                    this._111 = assign;
                    break;
                case unchecked((byte)0b1110):
                    assign = GetOrCreateOctreeCell(this, key, quad, this._110);
                    this._110 = assign;
                    break;
                case unchecked((byte)0b1101):
                    assign = GetOrCreateOctreeCell(this, key, quad, this._101);
                    this._101 = assign;
                    break;
                case unchecked((byte)0b1100):
                    assign = GetOrCreateOctreeCell(this, key, quad, this._100);
                    this._100 = assign;
                    break;
                case unchecked((byte)0b1011):
                    assign = GetOrCreateOctreeCell(this, key, quad, this._011);
                    this._011 = assign;
                    break;
                case unchecked((byte)0b1010):
                    assign = GetOrCreateOctreeCell(this, key, quad, this._010);
                    this._010 = assign;
                    break;
                case unchecked((byte)0b1001):
                    assign = GetOrCreateOctreeCell(this, key, quad, this._001);
                    this._001 = assign;
                    break;
                case unchecked((byte)0b1000):
                    assign = GetOrCreateOctreeCell(this, key, quad, this._000);
                    this._000 = assign;
                    break;
                default:
                    throw new Exception("Unrecognized bytecode");
                    break;
            }
            //return this;
        }

        private OctreeCell GetOrCreateOctreeCell(OctreeCell? parent, ulong key, byte quad, OctreeCell cell)
        {
            if (cell != null)
                return cell;

            OctreeCell _cell = new OctreeCell();

            _cell._location = key;
            _cell._parent = parent;
            _cell._qaud = quad;
            

            return _cell;
        }
    }
}
