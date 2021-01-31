using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ParticleLib.Models.ParticleSpace2D;

namespace ParticleLib.Models
{
    internal class QuadTree<T> where T : Particle
    {
        (float X, float Y) from;
        (float X, float Y) to;
        int layer;

        QuadTree<T> nw;
        QuadTree<T> ne;
        QuadTree<T> sw;
        QuadTree<T> se;

        object _nwl = new object();
        object _nel = new object();
        object _swl = new object();
        object _sel = new object();

        T t;

        List<Particle> t_l = null;

        QuadTree<T> parent;
        Quad pQuad;
        public QuadTree(QuadTree<T> _parent, Quad _pQuad)
        {
            parent = _parent;
            pQuad = _pQuad;
        }
        
        private int _minResolution = 5;
        public void Init((float, float) _from, (float, float) _to, int _layer, IEnumerable<T> elements)
        {
            if (_from == _to)
                return;
            //var bs = (int)(Math.Pow(2, layer));
            from = _from;
            to = _to;
            layer = _layer;

            //var __tdX = (to.X - from.X) / 2;
            //var __tdY = (to.Y - from.Y) / 2;
            //var __toX = ((to.X + from.X)/2);
            //var __toY = ((to.Y + from.Y)/2);

            //var mx = (__toX);
            //var lx = __toX - __tdX;
            //var rx = (__toX + __tdX);

            //var my = (__toY);
            //var ty = __toY - __tdY;
            //var by = (__toY + __tdY);

            //var TLF = (lx, ty);
            //var TLT = (mx, my);

            //var TRF = (mx, ty);
            //var TRT = (rx, my);

            //var BRF = TLT;
            //var BRT = (rx, by);

            //var BLF = (lx, my);
            //var BLT = (mx, by);

            //var nwE = elements.Where(e => Contains(e.pos(), Quad.TopLeft));
            //var neE = elements.Where(e => Contains(e.pos(), Quad.TopRight));
            //var swE = elements.Where(e => Contains(e.pos(), Quad.BottomLeft));
            //var seE = elements.Where(e => Contains(e.pos(), Quad.BottomRight));

            //if (nwE.Any())
            //{
            //    lock (_nwl)
            //    {
            //        nw = new QuadTree<T>(this, Quad.TopLeft);
            //        nw.Init(TLF, TLT, layer + 1, nwE);
            //    }
            //}
            //if (neE.Any())
            //{
            //    lock (_nel)
            //    {
            //        ne = new QuadTree<T>(this, Quad.TopRight);
            //        ne.Init(TRF, TRT, layer + 1, neE);
            //    }
            //}
            //if (swE.Any())
            //{
            //    lock (_swl)
            //    {
            //        sw = new QuadTree<T>(this, Quad.BottomLeft);
            //        sw.Init(BLF, BLT, layer + 1, swE);
            //    }
            //}
            //if (seE.Any())
            //{
            //    lock (_sel)
            //    {
            //        se = new QuadTree<T>(this, Quad.BottomRight);
            //        se.Init(BRF, BRT, layer + 1, seE);
            //    }
            //}
            

        }

        internal void ProcessTimestep(long diff, (float, float) focus, Point BOUNDS)
        {
            var hasDied = t?.isDead ?? false;

            List<Particle> _t_l = null;
            if (t_l != null)
                _t_l = t_l.Where(t => t?.isDead ?? false).ToList();


            var ptsk = new List<Task>();
            ptsk.Add(Task.Run(() => t?.ProcessTimestep(diff, focus, BOUNDS)));
            ptsk.Add(Task.Run(() => nw?.ProcessTimestep(diff, focus, BOUNDS)));
            ptsk.Add(Task.Run(() => ne?.ProcessTimestep(diff, focus, BOUNDS)));
            ptsk.Add(Task.Run(() => sw?.ProcessTimestep(diff, focus, BOUNDS)));
            ptsk.Add(Task.Run(() => se?.ProcessTimestep(diff, focus, BOUNDS)));

            if(t != null)
            {
                if (hasDied)
                    t = null;
                else if (!Contains(t.pos()) && parent != null)
                {
                    parent.Add(t);
                    t = null;
                }
            }

            if(t_l != null)
            foreach(var p in t_l)
            {
                if (_t_l.Contains(p))
                    t_l.Remove(p);
                else if(!Contains(p.pos()) && parent != null)
                {
                    parent.Add(t);
                    t = null;
                }
            }

            lock (_nwl)
            {
                if (nw?.Empty() ?? true)
                    nw = null;
            }
            lock (_nel)
            {
                if (ne?.Empty() ?? true)
                    ne = null;
            }
            lock (_swl)
            {
                if (sw?.Empty() ?? true)
                    sw = null;
            }
            lock (_sel)
            {
                if (se?.Empty() ?? true)
                    se = null;
            }


            Task.WaitAll(ptsk.ToArray());
        }

        internal bool Add(T ele)
        {
            if (Empty())
                t = ele;
            else
            {
                var added = false;
                added = AddEleToQuadIfExists(ele, Quad.TopLeft);
                if (!added)
                    added = AddEleToQuadIfExists(ele, Quad.TopRight);
                if (!added)
                    added = AddEleToQuadIfExists(ele, Quad.BottomLeft);
                if (!added)
                    added = AddEleToQuadIfExists(ele, Quad.BottomRight);
                if(!added)
                {
                    parent.Add(ele);
                }
                else if (added && t != null)
                {
                    var added2 = false;
                    if (!added2)
                        added2 = AddEleToQuadIfExists(t, Quad.TopLeft);
                    if (!added2)
                        added2 = AddEleToQuadIfExists(t, Quad.TopRight);
                    if (!added2)
                        added2 = AddEleToQuadIfExists(t, Quad.BottomLeft);
                    if (!added2)
                        added2 = AddEleToQuadIfExists(t, Quad.BottomRight);
                    if (!added2)
                        parent.Add(t);
                    t = null;
                }
                return added;
            }
            return true;
        }

        private bool Empty()
        {
            return t == null && (nw?.Empty() ?? true) && (nw?.Empty() ?? true) && (sw?.Empty() ?? true) && (se?.Empty() ?? true);
        }

        private bool AddEleToQuadIfExists(T ele, Quad quad)
        {
            if (ele == null)
                return true;
            if(layer >= 8)
            {
                if (t_l == null)
                    t_l = new List<Particle>();
                t_l.Add(ele);
                return true;
            }

            var __tdX = (to.X - from.X) / 2;
            var __tdY = (to.Y - from.Y) / 2;
            var __toX = ((to.X + from.X) / 2);
            var __toY = ((to.Y + from.Y) / 2);

            var mx = (__toX);
            var lx = __toX - __tdX;
            var rx = (__toX + __tdX);

            var my = (__toY);
            var ty = __toY - __tdY;
            var by = (__toY + __tdY);

            var TLF = (lx, ty);
            var TLT = (mx, my);

            var TRF = (mx, ty);
            var TRT = (rx, my);

            var BRF = TLT;
            var BRT = (rx, by);

            var BLF = (lx, my);
            var BLT = (mx, by);
            var epos = ele.pos();
            var x = epos.Item1;
            var y = epos.Item2;
            bool isInQuad = false;
            switch (quad)
            {
                case Quad.TopLeft:
                    isInQuad = x >= lx && x <= mx && y >= ty && y <= my;
                    break;
                case Quad.TopRight:
                    isInQuad = x > mx && x <= rx && y >= ty && y <= my;
                    break;
                case Quad.BottomLeft:
                    isInQuad = x >= lx && x <= mx && y > my && y <= by;
                    break;
                case Quad.BottomRight:
                    isInQuad = x > mx && x <= rx && y > my && y <= by;
                    break;
                case Quad.Global:
                    isInQuad = x >= lx && x <= rx && y >= ty && y <= by;
                    break;
                case Quad.NONE:
                    isInQuad = x < lx || x > rx || y < by || y > ty;
                    break;
            }
            if (!isInQuad)
                return false;


            switch (quad)
            {
                case Quad.TopLeft:
                    lock (_nwl)
                    {
                        if (nw == null)
                        {
                            nw = new QuadTree<T>(this, quad);
                            nw.Init(TLF, TLT, layer + 1, new List<T>() {  });
                            nw.Add(ele);
                        }
                        else
                        {
                            nw.Add(ele);
                        }
                    }
                    break;
                case Quad.TopRight:
                    lock (_nel)
                    {
                        if (ne == null)
                        {
                            ne = new QuadTree<T>(this, quad);
                            ne.Init(TRF, TRT, layer + 1, new List<T>() {  });
                            ne.Add(ele);
                        }
                        else
                        {
                            ne.Add(ele);
                        }
                    }
                    break;
                case Quad.BottomLeft:
                    lock (_swl)
                    {
                        if (sw == null)
                        {
                            sw = new QuadTree<T>(this, quad);
                            sw.Init(BLF, BLT, layer + 1, new List<T>() {  });
                            sw.Add(ele);
                        }
                        else
                        {
                            sw.Add(ele);
                        }
                    }
                    break;
                case Quad.BottomRight:
                    lock (_sel)
                    {
                        if (se == null)
                        {
                            se = new QuadTree<T>(this, quad);
                            se.Init(BRF, BRT, layer + 1, new List<T>() {  });
                            se.Add(ele);
                        }
                        else
                        {
                            se.Add(ele);
                        }
                    }
                    break;
            }
            return true;
        }

        internal IEnumerable<Particle> GetParticles()
        {
            List<Particle> particles = new List<Particle>();
            if (t != null)
                particles.Add(t);
            if (nw != null)
                particles.AddRange(nw.GetParticles());
            if (ne != null)
                particles.AddRange(ne.GetParticles());
            if (sw != null)
                particles.AddRange(sw.GetParticles());
            if (se != null)
                particles.AddRange(se.GetParticles());

            particles.AddRange(t_l?.ToList() ?? new List<Particle>());

            return particles;
        }

        internal bool Is(T _t)
        {
            var tpos = t.pos();
            var _tpos = _t.pos();
            return tpos.Item1 == _tpos.Item1 && tpos.Item2 == _tpos.Item2;
        }

        private bool Contains((float x, float y) pos, Quad quad = Quad.Global)
        {
            var x = pos.x;
            var y = pos.y;

            var __tdX = (to.X - from.X) / 2;
            var __tdY = (to.Y - from.Y) / 2;
            var __toX = ((to.X + from.X) / 2);
            var __toY = ((to.Y + from.Y) / 2);

            var mx = (__toX);
            var lx = __toX - __tdX;
            var rx = (__toX + __tdX);

            var my = (__toY);
            var ty = __toY - __tdY;
            var by = (__toY + __tdY);
            switch (quad)
            {
                case Quad.TopLeft:
                    return x >= lx && x <= mx && y >= ty && y <= my;
                case Quad.TopRight:
                    return x >= mx && x <= rx && y >= ty && y <= my;
                case Quad.BottomLeft:
                    return x >= lx && x <= mx && y >= my && y <= by;
                case Quad.BottomRight:
                    return x >= mx && x <= rx && y >= my && y <= by;
                case Quad.Global:
                    return x >= lx && x <= rx && y >= ty && y <= by;
                case Quad.NONE:
                    return x < lx || x > rx || y < ty || y > ty;
            }
            return false;
        }

        public bool InRange((int, int) corner, int range, (int, int) point, int length)
        {
            var center = (corner.Item1 + length / 4, corner.Item2 + length / 4);
            var distY = Math.Abs(point.Item2 - center.Item2);
            var distX = Math.Abs(point.Item1 - center.Item1);
            return Math.Sqrt((Math.Pow(distY, 2) + Math.Pow(distX, 2))) < range;
        }

        internal RectangleF[] GetRectangles()
        {
            List<RectangleF> rectangles = new List<RectangleF>();
            rectangles.Add(new RectangleF(from.X, from.Y, to.X, to.Y));

            if (nw != null)
                rectangles.AddRange(nw.GetRectangles());
            if (ne != null)
                rectangles.AddRange(ne.GetRectangles());
            if (sw != null)
                rectangles.AddRange(sw.GetRectangles());
            if (se != null)
                rectangles.AddRange(se.GetRectangles());
            return rectangles.ToArray();
        }

        //public bool Contains((int x, int y) point)
        //{
        //    var bs = Math.Pow(2, key.Item3);
        //    var off = (int)(bs / 2);
        //    return Contains((key.Item1, key.Item2), (key.Item1 + off, key.Item2 + off), point);
        //}

        public static bool Contains((int, int) x1y1, int offset, (int x, int y) point)
        {
            return Contains(x1y1, (x1y1.Item1 + offset, x1y1.Item2 + offset), point);
        }

        public static bool Contains((int, int) x1y1, (int, int) x2y2, (int x, int y) point)
        {
            return point.x >= x1y1.Item1 && point.x <= x2y2.Item1
                && point.y >= x1y1.Item2 && point.y <= x2y2.Item2;
        }

        //private IEnumerator LoadChunkIntoMap(string filename, bool collision = false)
        //{
        //    if (!_currNames.Contains(filename))
        //        _currNames.Add(filename);
        //    yield return LoadDataChunk(filename, collision);
        //}
    }

    public interface ILocational
    {
        public (float, float) pos();
    }
}
