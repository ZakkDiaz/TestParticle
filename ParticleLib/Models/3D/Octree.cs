using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ParticleLib.Models._3D
{
    public class Octree : AAABBB
    {
        public OctreeNode OctreeNode { get; set; }
        private Dictionary<ulong, NodeCollection> _octreeHeap = new Dictionary<ulong, NodeCollection>();
        private Dictionary<IntPtr, NodeTypeLocation3D> _objRefs = new Dictionary<IntPtr, NodeTypeLocation3D>();
        private Dictionary<IntPtr, NodeTypeLayer3D> _locationRefs = new Dictionary<IntPtr, NodeTypeLayer3D>();

        //Font font = new Font("Arial", 20);
        //private ConcurrentBag<NodeTypeLocation3D> _locationObjects = new ConcurrentBag<NodeTypeLocation3D>();

        public int Depth()
        {
            var depth = 1;

            var collObj = _octreeHeap[OctreeNode.LocCode];
            var _depth = GetDepth(collObj);

            return depth + _depth;
        }

        private int GetDepth(NodeCollection collObj)
        {
            var depth = 1;

            var maxDepth = 0;
            if (collObj._000.HasValue)
            {
                var cD = GetDepth(_octreeHeap[collObj._000.Value.LocCode]);
                if (cD > maxDepth)
                    maxDepth = cD;
            }
            if (collObj._001.HasValue)
            {
                var cD = GetDepth(_octreeHeap[collObj._001.Value.LocCode]);
                if (cD > maxDepth)
                    maxDepth = cD;
            }
            if (collObj._010.HasValue)
            {
                var cD = GetDepth(_octreeHeap[collObj._010.Value.LocCode]);
                if (cD > maxDepth)
                    maxDepth = cD;
            }
            if (collObj._011.HasValue)
            {
                var cD = GetDepth(_octreeHeap[collObj._011.Value.LocCode]);
                if (cD > maxDepth)
                    maxDepth = cD;
            }
            if (collObj._100.HasValue)
            {
                var cD = GetDepth(_octreeHeap[collObj._100.Value.LocCode]);
                if (cD > maxDepth)
                    maxDepth = cD;
            }
            if (collObj._101.HasValue)
            {
                var cD = GetDepth(_octreeHeap[collObj._101.Value.LocCode]);
                if (cD > maxDepth)
                    maxDepth = cD;
            }
            if (collObj._110.HasValue)
            {
                var cD = GetDepth(_octreeHeap[collObj._110.Value.LocCode]);
                if (cD > maxDepth)
                    maxDepth = cD;
            }
            if (collObj._111.HasValue)
            {
                var cD = GetDepth(_octreeHeap[collObj._111.Value.LocCode]);
                if (cD > maxDepth)
                    maxDepth = cD;
            }

            return depth + maxDepth;
        }

        public void Draw(Graphics g)
        {
            var head = OctreeNode;
            int _drawcount = 0;
            Draw(g, head, ref _drawcount, 1);
        }

        private void Draw(Graphics g, OctreeNode node, ref int drawCount, int depth)
        {
            var center = (_to - _from) / 2;
            var locCode = node.LocCode;
            var parentNode = _locationRefs[node.ObjPtr];
            //if (depth > 10)
            {
                //if (!parentNode.IsLayer)
                //    g.FillEllipse(Brushes.Black, new Rectangle((int)parentNode.Location.Item1, (int)parentNode.Location.Item2, 10, 10));
                //else
                //{
                //var layerSize = _to * ((float)(1 / Math.Pow(2, depth)));
                Pen pen = Pens.Black;
                switch (depth)
                {
                    case 2:
                        pen = Pens.DarkBlue;
                        break;
                    case 3:
                        pen = Pens.DarkCyan;
                        break;
                    case 4:
                        pen = Pens.DarkGreen;
                        break;
                    case 5:
                        pen = Pens.Blue;
                        break;
                    case 6:
                        pen = Pens.Cyan;
                        break;
                    case 7:
                        pen = Pens.Green;
                        break;
                    case 8:
                        pen = Pens.DarkOrange;
                        break;
                    case 9:
                        pen = Pens.DarkSalmon;
                        break;
                    case 10:
                        pen = Pens.DarkRed;
                        break;
                    case 11:
                        pen = Pens.Yellow;
                        break;
                    case 12:
                        pen = Pens.OrangeRed;
                        break;
                    case 13:
                        pen = Pens.Red;
                        break;
                    case 14:
                        pen = Pens.MediumPurple;
                        break;
                    case 15:
                        pen = Pens.Violet;
                        break;
                    case 16:
                        pen = Pens.Pink;
                        break;
                }
                   
                g.DrawRectangle(pen, new Rectangle((int)parentNode._from.X, (int)parentNode._from.Y, (int)parentNode._to.X, (int)parentNode._to.Y));
          

                foreach (var cnode in parentNode.ChildLocationItems)
                    DrawNode(g, cnode, parentNode, ref drawCount);
            }
            var collectionItem = _octreeHeap[locCode];
            if (collectionItem._000.HasValue)
            {
                //var _obj = _objRefs[collectionItem._000.Value.ObjPtr] as NodeTypeLocation3D;
                //g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
                //g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
                //g.DrawString("_000 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
                drawCount++;


                Draw(g, collectionItem._000.Value, ref drawCount, depth + 1);
            }
            if (collectionItem._001.HasValue)
            {
                //var _obj = _objRefs[collectionItem._001.Value.ObjPtr] as NodeTypeLocation3D;
                //g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
                //g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
                //g.DrawString("_001 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
                drawCount++;
                Draw(g, collectionItem._001.Value, ref drawCount, depth + 1);
            }
            if (collectionItem._010.HasValue)
            {
                //var _obj = _objRefs[collectionItem._010.Value.ObjPtr] as NodeTypeLocation3D;
                //g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
                //g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
                //g.DrawString("_010 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
                drawCount++;
                Draw(g, collectionItem._010.Value, ref drawCount, depth + 1);
            }
            if (collectionItem._011.HasValue)
            {
                //var _obj = _objRefs[collectionItem._011.Value.ObjPtr] as NodeTypeLocation3D;
                //g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
                //g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
                //g.DrawString("_011 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
                drawCount++;
                Draw(g, collectionItem._011.Value, ref drawCount, depth + 1);
            }
            if (collectionItem._100.HasValue)
            {
                //var _obj = _objRefs[collectionItem._100.Value.ObjPtr] as NodeTypeLocation3D;
                //g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
                //g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
                //g.DrawString("_100 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
                drawCount++;
                Draw(g, collectionItem._100.Value, ref drawCount, depth + 1);
            }
            if (collectionItem._101.HasValue)
            {
                //var _obj = _objRefs[collectionItem._101.Value.ObjPtr] as NodeTypeLocation3D;
                //g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
                //g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
                //g.DrawString("_101 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
                drawCount++;
                Draw(g, collectionItem._101.Value, ref drawCount, depth + 1);
            }
            if (collectionItem._110.HasValue)
            {
                //var _obj = _objRefs[collectionItem._110.Value.ObjPtr] as NodeTypeLocation3D;
                //g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
                //g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
                //g.DrawString("_110 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
                drawCount++;
                Draw(g, collectionItem._110.Value, ref drawCount, depth + 1);
            }
            if (collectionItem._111.HasValue)
            {
                //var _obj = _objRefs[collectionItem._111.Value.ObjPtr] as NodeTypeLocation3D;
                //g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
                //g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
                //g.DrawString("_111 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
                drawCount++;
                Draw(g, collectionItem._111.Value, ref drawCount, depth + 1);
            }
        }

        private void DrawNode(Graphics g, NodeTypeLocation3D cnode, NodeTypeLayer3D parent, ref int drawCount)
        {
            var center = (parent._to - parent._from) / 2;
            g.FillEllipse(Brushes.Black, new Rectangle((int)cnode.Location.Item1, (int)cnode.Location.Item2, 10, 10));
            g.DrawLine(Pens.Black, new Point((int)center.X, (int)center.Y), new Point((int)center.X, (int)center.Y));
            drawCount++;
        }

        public int Size()
        {
            return _objRefs.Count;
        }

        unsafe public Octree(Point3D from, Point3D to) : base(from, to)
        {
            var _locationNode = new NodeTypeLayer3D(from, to);
            TypedReference tr = __makeref(_locationNode);
            IntPtr ptr = **(IntPtr**)(&tr);
            _locationRefs.Add(ptr, _locationNode);

            var defaultNodeCollection = new NodeCollection();
            var nodeCollectionPtr  =  &defaultNodeCollection;
            OctreeNode = new OctreeNode(nodeCollectionPtr, null, 0b1000, 0, NodeType.Location, ptr, 0);
            _octreeHeap.Add(OctreeNode.LocCode, defaultNodeCollection);
            //_octreeNodes.Add(OctreeNode);
        }

        unsafe public void Add(NodeTypeLocation3D pointLocation)
        {
            TypedReference tr = __makeref(pointLocation);
            IntPtr ptr = **(IntPtr**)(&tr);
            while(_objRefs.ContainsKey(ptr))
            {
                pointLocation = new NodeTypeLocation3D(pointLocation.Location.Item1, pointLocation.Location.Item2, pointLocation.Location.Item3, false);
                tr = __makeref(pointLocation);
                ptr = **(IntPtr**)(&tr);
            }
            _objRefs.Add(ptr, pointLocation);

            Add(OctreeNode, pointLocation);
        }

        unsafe private void Add(OctreeNode parentNode, NodeTypeLocation3D pointLocation, int depth = 4)
        {
            if (depth == 64)
                return;


            var _compareTo = _locationRefs[parentNode.ObjPtr];
            if (!_compareTo.Full())
            {
                _compareTo.Add(pointLocation);
                return;
            }

            if (depth >= 12)
                ;
            var half = ((_compareTo._to - _compareTo._from) / 2);
            var center = _compareTo._from + half;
            var x = pointLocation.Location.Item1 > Math.Abs(center.X);
            var y = pointLocation.Location.Item2 > Math.Abs(center.Y);
            var z = pointLocation.Location.Item3 > Math.Abs(center.Z);

            ulong quad =
                (ulong)(
                    (x ? 0b1001 : 0b1000)
                    |
                    (y ? 0b1010 : 0b1000)
                    |
                    (z ? 0b1100 : 0b1000)
                );


            var nodeCollection = _octreeHeap[parentNode.LocCode];
            var defaultNodeCollection = new NodeCollection();
            var nodeCollectionPtr = &defaultNodeCollection;

            //var layerSize = _to * ((float)(1 / Math.Pow(2, depth/4)));
            //g.DrawRectangle(Pens.Black, new Rectangle((int)parentNode.Location.Item1 - (int)layerSize.X, (int)parentNode.Location.Item2 - (int)layerSize.Y, 2 * (int)layerSize.X, 2 * (int)layerSize.Y));

            //if (x && (pointLocation.Location.Item1 - layerSize.X) > _compareTo.Location.Item1)
            //    ;

            switch (quad)
            {
                case unchecked((byte)0b1111):
                    if (nodeCollection._111.HasValue)
                    {
                        Add(nodeCollection._111.Value, pointLocation, depth + 4);
                    }
                    else
                    {
                        var fromOff = new Point3D(half.X, half.Y, half.Z);
                        var _fr = _compareTo._from + fromOff;
                        var _to = half + _fr;
                        OctreeNode layerONode = GenerateLayerOctreeNode(parentNode, depth, _compareTo, quad, nodeCollectionPtr, _fr, _to);

                        nodeCollection._111 = layerONode;
                        _octreeHeap.Add(layerONode.LocCode, defaultNodeCollection);
                        Add(layerONode, pointLocation, depth + 4);
                    }
                    break;
                case unchecked((byte)0b1110):
                    if (nodeCollection._110.HasValue) { 
                        Add(nodeCollection._110.Value, pointLocation, depth + 4);
                    }
                    else
                    {
                        var fromOff = new Point3D(0, half.Y, half.Z);
                        var _fr = _compareTo._from + fromOff;
                        var _to = half + _fr;
                        OctreeNode layerONode = GenerateLayerOctreeNode(parentNode, depth, _compareTo, quad, nodeCollectionPtr, _fr, _to);
                        nodeCollection._110 = layerONode;
                        _octreeHeap.Add(layerONode.LocCode, defaultNodeCollection);
                        Add(layerONode, pointLocation, depth + 4);
                    }
                    break;
                case unchecked((byte)0b1101):
                    if (nodeCollection._101.HasValue) { 
                        Add(nodeCollection._101.Value, pointLocation, depth + 4);
                    }
                    else
                    {
                        var fromOff = new Point3D(half.X, 0, half.Z);
                        var _fr = _compareTo._from + fromOff;
                        var _to = half + _fr;
                        OctreeNode layerONode = GenerateLayerOctreeNode(parentNode, depth, _compareTo, quad, nodeCollectionPtr, _fr, _to);
                        nodeCollection._101 = layerONode;
                        _octreeHeap.Add(layerONode.LocCode, defaultNodeCollection);
                        Add(layerONode, pointLocation, depth + 4);
                    }
                    break;
                case unchecked((byte)0b1100):
                    if (nodeCollection._100.HasValue) { 
                        Add(nodeCollection._100.Value, pointLocation, depth + 4);
                    }
                    else
                    {
                        var fromOff = new Point3D(0, 0, half.Z);
                        var _fr = _compareTo._from + fromOff;
                        var _to = half + _fr;
                        OctreeNode layerONode = GenerateLayerOctreeNode(parentNode, depth, _compareTo, quad, nodeCollectionPtr, _fr, _to);
                        nodeCollection._100 = layerONode;
                        _octreeHeap.Add(layerONode.LocCode, defaultNodeCollection);
                        Add(layerONode, pointLocation, depth + 4);
                    }
                    break;
                case unchecked((byte)0b1011):
                    if (nodeCollection._011.HasValue) { 
                        Add(nodeCollection._011.Value, pointLocation, depth + 4);
                    }
                    else
                    {
                        var fromOff = new Point3D(half.X, half.Y, 0);
                        var _fr = _compareTo._from + fromOff;
                        var _to = half + _fr;
                        OctreeNode layerONode = GenerateLayerOctreeNode(parentNode, depth, _compareTo, quad, nodeCollectionPtr, _fr, _to);
                        nodeCollection._011 = layerONode;
                        _octreeHeap.Add(layerONode.LocCode, defaultNodeCollection);
                        Add(layerONode, pointLocation, depth + 4);
                    }
                    break;
                case unchecked((byte)0b1010):
                    if (nodeCollection._010.HasValue) { 
                        Add(nodeCollection._010.Value, pointLocation, depth + 4);
                    }
                    else
                    {
                        var fromOff = new Point3D(0, half.Y, 0);
                        var _fr = _compareTo._from + fromOff;
                        var _to = half + _fr;
                        OctreeNode layerONode = GenerateLayerOctreeNode(parentNode, depth, _compareTo, quad, nodeCollectionPtr, _fr, _to);
                        nodeCollection._010 = layerONode;
                        _octreeHeap.Add(layerONode.LocCode, defaultNodeCollection);
                        Add(layerONode, pointLocation, depth + 4);
                    }
                    break;
                case unchecked((byte)0b1001):
                    if (nodeCollection._001.HasValue) { 
                        Add(nodeCollection._001.Value, pointLocation, depth + 4);
                    }
                    else
                    {
                        var fromOff = new Point3D(half.X, 0, 0);
                        var _fr = _compareTo._from + fromOff;
                        var _to = half + _fr;
                        OctreeNode layerONode = GenerateLayerOctreeNode(parentNode, depth, _compareTo, quad, nodeCollectionPtr, _fr, _to);
                        nodeCollection._001 = layerONode;
                        _octreeHeap.Add(layerONode.LocCode, defaultNodeCollection);
                        Add(layerONode, pointLocation, depth + 4);
                    }
                    break;
                case unchecked((byte)0b1000):
                    if (nodeCollection._000.HasValue) { 
                        Add(nodeCollection._000.Value, pointLocation, depth + 4);
                    }
                    else
                    {
                        var fromOff = new Point3D(0, 0, 0);
                        var _fr = _compareTo._from + fromOff;
                        var _to = half + _fr;
                        OctreeNode layerONode = GenerateLayerOctreeNode(parentNode, depth, _compareTo, quad, nodeCollectionPtr, _fr, _to);
                        nodeCollection._000 = layerONode;
                        _octreeHeap.Add(layerONode.LocCode, defaultNodeCollection);
                        Add(layerONode, pointLocation, depth + 4);
                    }
                    break;
            }
            _octreeHeap[parentNode.LocCode] = nodeCollection;

            var toReflow = _compareTo.ChildLocationItems.ToList();
            _compareTo.ClearChildren();
            foreach (var node in toReflow)
            {
                Add(OctreeNode, node);
            }
        }

        private unsafe OctreeNode GenerateLayerOctreeNode(OctreeNode parentNode, int depth, NodeTypeLayer3D _compareTo, ulong quad, NodeCollection* nodeCollectionPtr, Point3D quadFrom, Point3D quadTo)
        {
            var layerPtr = CreateLayerLocation(_compareTo, quadFrom, quadTo);
            var layerONode = new OctreeNode()
            {
                NodeType = NodeType.Layer,
                ObjPtr = layerPtr,
                ChildExists = 0
            };
            layerONode.Children = nodeCollectionPtr;
            layerONode.LocCode = (parentNode.LocCode | (quad << depth));
            return layerONode;
        }

        private unsafe IntPtr CreateLayerLocation(NodeTypeLayer3D parentLayer, Point3D quadFrom, Point3D quadTo)
        {
            //var center = (parentLayer._to - parentLayer._from) / 2;
            //var from = center + quadOff;
            //var to = from + center;
            //var absOff = new Point3D(Math.Abs(centerOff.X), Math.Abs(centerOff.Y), Math.Abs(centerOff.Z));
            //var to = absOff + quadOff;
            //var xMin = Math.Min(center.X, quadOff.X);
            //var yMin = Math.Min(center.Y, quadOff.Y);
            //var zMin = Math.Min(center.Z, quadOff.Z);
            //var xMax = Math.Max(center.X, quadOff.X);
            //var yMax = Math.Max(center.Y, quadOff.Y);
            //var zMax = Math.Max(center.Z, quadOff.Z);

            //var from = new Point3D(xMin, yMin, zMin);
            //var to = new Point3D(xMax, yMax, zMax);
            var layerNode = new NodeTypeLayer3D(quadFrom, quadTo);

            TypedReference tr = __makeref(layerNode);
            IntPtr layer_ptr = **(IntPtr**)(&tr);
            while (_locationRefs.ContainsKey(layer_ptr))
            {
                layerNode = new NodeTypeLayer3D(quadFrom, quadTo);
                tr = __makeref(layerNode);
                layer_ptr = **(IntPtr**)(&tr);
            }

            //Remove this and add another ref to a point collection
            //Then from quad calculate the point location relative to the point referenced (head = center, 000 = center/2, 111 = center * 2)


            _locationRefs.Add(layer_ptr, layerNode);
            return layer_ptr;
        }
    }
}
