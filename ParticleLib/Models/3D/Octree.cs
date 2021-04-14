using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ParticleLib.Models._3D
{
    public class Octree : AAABBB
    {
        public OctreeNode OctreeNode { get; set; }
        private Dictionary<ulong, NodeCollection> _octreeHeap = new Dictionary<ulong, NodeCollection>();
        private Dictionary<IntPtr, NodeTypeLocation3D> _objRefs = new Dictionary<IntPtr, NodeTypeLocation3D>();

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
            var parentNode = _objRefs[node.ObjPtr] as NodeTypeLocation3D;
            if (!parentNode.IsLayer)
                g.FillEllipse(Brushes.Black, new Rectangle((int)parentNode.Location.Item1, (int)parentNode.Location.Item2, 10, 10));
            else {
                var layerSize = _to * ((float)(1 / Math.Pow(2, depth)));
                g.DrawRectangle(Pens.Black, new Rectangle((int)parentNode.Location.Item1 - (int)layerSize.X, (int)parentNode.Location.Item2 - (int)layerSize.Y, 2 * (int)layerSize.X, 2 * (int)layerSize.Y));
            }

            foreach (var cnode in parentNode.ChildLocationItems)
                DrawNode(g, cnode, parentNode, ref drawCount);

            var collectionItem = _octreeHeap[locCode];
            if (collectionItem._000.HasValue)
            {
                var _obj = _objRefs[collectionItem._000.Value.ObjPtr] as NodeTypeLocation3D;
                g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
                g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
                //g.DrawString("_000 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
                drawCount++;


                Draw(g, collectionItem._000.Value, ref drawCount, depth + 1);
            }
            if (collectionItem._001.HasValue)
            {
                var _obj = _objRefs[collectionItem._001.Value.ObjPtr] as NodeTypeLocation3D;
                g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
                g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
                //g.DrawString("_001 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
                drawCount++;
                Draw(g, collectionItem._001.Value, ref drawCount, depth + 1);
            }
            if (collectionItem._010.HasValue)
            {
                var _obj = _objRefs[collectionItem._010.Value.ObjPtr] as NodeTypeLocation3D;
                g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
                g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
                //g.DrawString("_010 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
                drawCount++;
                Draw(g, collectionItem._010.Value, ref drawCount, depth + 1);
            }
            if (collectionItem._011.HasValue)
            {
                var _obj = _objRefs[collectionItem._011.Value.ObjPtr] as NodeTypeLocation3D;
                g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
                g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
                //g.DrawString("_011 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
                drawCount++;
                Draw(g, collectionItem._011.Value, ref drawCount, depth + 1);
            }
            if (collectionItem._100.HasValue)
            {
                var _obj = _objRefs[collectionItem._100.Value.ObjPtr] as NodeTypeLocation3D;
                g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
                g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
                //g.DrawString("_100 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
                drawCount++;
                Draw(g, collectionItem._100.Value, ref drawCount, depth + 1);
            }
            if (collectionItem._101.HasValue)
            {
                var _obj = _objRefs[collectionItem._101.Value.ObjPtr] as NodeTypeLocation3D;
                g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
                g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
                //g.DrawString("_101 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
                drawCount++;
                Draw(g, collectionItem._101.Value, ref drawCount, depth + 1);
            }
            if (collectionItem._110.HasValue)
            {
                var _obj = _objRefs[collectionItem._110.Value.ObjPtr] as NodeTypeLocation3D;
                g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
                g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
                //g.DrawString("_110 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
                drawCount++;
                Draw(g, collectionItem._110.Value, ref drawCount, depth + 1);
            }
            if (collectionItem._111.HasValue)
            {
                var _obj = _objRefs[collectionItem._111.Value.ObjPtr] as NodeTypeLocation3D;
                g.FillEllipse(Brushes.Black, new Rectangle((int)_obj.Location.Item1, (int)_obj.Location.Item2, 10, 10));
                g.DrawLine(Pens.Black, new Point((int)parentNode.Location.Item1, (int)parentNode.Location.Item2), new Point((int)_obj.Location.Item1, (int)_obj.Location.Item2));
                //g.DrawString("_111 " + drawCount, font, Brushes.Black, new PointF(_obj.Location.Item1, _obj.Location.Item2));
                drawCount++;
                Draw(g, collectionItem._111.Value, ref drawCount, depth + 1);
            }
        }

        private void DrawNode(Graphics g, NodeTypeLocation3D cnode, NodeTypeLocation3D parent, ref int drawCount)
        {
            g.FillEllipse(Brushes.Black, new Rectangle((int)cnode.Location.Item1, (int)cnode.Location.Item2, 10, 10));
            g.DrawLine(Pens.Black, new Point((int)parent.Location.Item1, (int)parent.Location.Item2), new Point((int)cnode.Location.Item1, (int)cnode.Location.Item2));
            drawCount++;
        }

        public int Size()
        {
            return _objRefs.Count;
        }

        unsafe public Octree(Point3D from, Point3D to) : base(from, to)
        {
            var center = (to - from) / 2;
            var _locationNode = new NodeTypeLocation3D(center.X, center.Y, center.Z, true);
            TypedReference tr = __makeref(_locationNode);
            IntPtr ptr = **(IntPtr**)(&tr);
            _objRefs.Add(ptr, _locationNode);

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

            Add(OctreeNode, pointLocation, ptr);
        }

        unsafe private void Add(OctreeNode parentNode, NodeTypeLocation3D pointLocation, IntPtr ptr, int depth = 4)
        {
            if (depth == 64)
                return;

            var _compareTo = _objRefs[parentNode.ObjPtr];
            if (!_compareTo.Full())
            {
                _compareTo.Add(pointLocation);
                return;
            }

            var x = pointLocation.Location.Item1 > _compareTo.Location.Item1;
            var y = pointLocation.Location.Item2 > _compareTo.Location.Item2;
            var z = pointLocation.Location.Item3 > _compareTo.Location.Item3;

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

            var layerSize = _to * ((float)(1 / Math.Pow(2, depth/4)));
            //g.DrawRectangle(Pens.Black, new Rectangle((int)parentNode.Location.Item1 - (int)layerSize.X, (int)parentNode.Location.Item2 - (int)layerSize.Y, 2 * (int)layerSize.X, 2 * (int)layerSize.Y));

            switch (quad)
            {
                case unchecked((byte)0b1111):
                    if (nodeCollection._111.HasValue)
                    {
                        Add(nodeCollection._111.Value, pointLocation, ptr, depth + 4);
                    }
                    else
                    {
                        var loc = (_compareTo.Location.Item1 + (layerSize.X / 2), _compareTo.Location.Item2 + (layerSize.Y / 2), _compareTo.Location.Item3 + (layerSize.Z / 2));
                        OctreeNode layerONode = GenerateLayerOctreeNode(parentNode, depth, _compareTo, quad, nodeCollectionPtr, loc);

                        nodeCollection._111 = layerONode;
                        _octreeHeap.Add(layerONode.LocCode, defaultNodeCollection);
                        Add(layerONode, pointLocation, ptr, depth + 4);
                    }
                    break;
                case unchecked((byte)0b1110):
                    if (nodeCollection._110.HasValue) { 
                        Add(nodeCollection._110.Value, pointLocation, ptr, depth + 4);
                    }
                    else
                    {
                        var loc = (_compareTo.Location.Item1 - (layerSize.X / 2), _compareTo.Location.Item2 + (layerSize.Y / 2), _compareTo.Location.Item3 + (layerSize.Z / 2));
                        OctreeNode layerONode = GenerateLayerOctreeNode(parentNode, depth, _compareTo, quad, nodeCollectionPtr, loc);
                        nodeCollection._110 = layerONode;
                        _octreeHeap.Add(layerONode.LocCode, defaultNodeCollection);
                        Add(layerONode, pointLocation, ptr, depth + 4);
                    }
                    break;
                case unchecked((byte)0b1101):
                    if (nodeCollection._101.HasValue) { 
                        Add(nodeCollection._101.Value, pointLocation, ptr, depth + 4);
                    }
                    else
                    {
                        var loc = (_compareTo.Location.Item1 + (layerSize.X / 2), _compareTo.Location.Item2 - (layerSize.Y / 2), _compareTo.Location.Item3 + (layerSize.Z / 2));
                        OctreeNode layerONode = GenerateLayerOctreeNode(parentNode, depth, _compareTo, quad, nodeCollectionPtr, loc);
                        nodeCollection._101 = layerONode;
                        _octreeHeap.Add(layerONode.LocCode, defaultNodeCollection);
                        Add(layerONode, pointLocation, ptr, depth + 4);
                    }
                    break;
                case unchecked((byte)0b1100):
                    if (nodeCollection._100.HasValue) { 
                        Add(nodeCollection._100.Value, pointLocation, ptr, depth + 4);
                    }
                    else
                    {
                        var loc = (_compareTo.Location.Item1 - (layerSize.X / 2), _compareTo.Location.Item2 - (layerSize.Y / 2), _compareTo.Location.Item3 + (layerSize.Z / 2));
                        OctreeNode layerONode = GenerateLayerOctreeNode(parentNode, depth, _compareTo, quad, nodeCollectionPtr, loc);
                        nodeCollection._100 = layerONode;
                        _octreeHeap.Add(layerONode.LocCode, defaultNodeCollection);
                        Add(layerONode, pointLocation, ptr, depth + 4);
                    }
                    break;
                case unchecked((byte)0b1011):
                    if (nodeCollection._011.HasValue) { 
                        Add(nodeCollection._011.Value, pointLocation, ptr, depth + 4);
                    }
                    else
                    {
                        var loc = (_compareTo.Location.Item1 + (layerSize.X / 2), _compareTo.Location.Item2 + (layerSize.Y / 2), _compareTo.Location.Item3 - (layerSize.Z / 2));
                        OctreeNode layerONode = GenerateLayerOctreeNode(parentNode, depth, _compareTo, quad, nodeCollectionPtr, loc);
                        nodeCollection._011 = layerONode;
                        _octreeHeap.Add(layerONode.LocCode, defaultNodeCollection);
                        Add(layerONode, pointLocation, ptr, depth + 4);
                    }
                    break;
                case unchecked((byte)0b1010):
                    if (nodeCollection._010.HasValue) { 
                        Add(nodeCollection._010.Value, pointLocation, ptr, depth + 4);
                    }
                    else
                    {
                        var loc = (_compareTo.Location.Item1 - (layerSize.X / 2), _compareTo.Location.Item2 + (layerSize.Y / 2), _compareTo.Location.Item3 - (layerSize.Z / 2));
                        OctreeNode layerONode = GenerateLayerOctreeNode(parentNode, depth, _compareTo, quad, nodeCollectionPtr, loc);
                        nodeCollection._010 = layerONode;
                        _octreeHeap.Add(layerONode.LocCode, defaultNodeCollection);
                        Add(layerONode, pointLocation, ptr, depth + 4);
                    }
                    break;
                case unchecked((byte)0b1001):
                    if (nodeCollection._001.HasValue) { 
                        Add(nodeCollection._001.Value, pointLocation, ptr, depth + 4);
                    }
                    else
                    {
                        var loc = (_compareTo.Location.Item1 + (layerSize.X / 2), _compareTo.Location.Item2 - (layerSize.Y / 2), _compareTo.Location.Item3 - (layerSize.Z / 2));
                        OctreeNode layerONode = GenerateLayerOctreeNode(parentNode, depth, _compareTo, quad, nodeCollectionPtr, loc);
                        nodeCollection._001 = layerONode;
                        _octreeHeap.Add(layerONode.LocCode, defaultNodeCollection);
                        Add(layerONode, pointLocation, ptr, depth + 4);
                    }
                    break;
                case unchecked((byte)0b1000):
                    if (nodeCollection._000.HasValue) { 
                        Add(nodeCollection._000.Value, pointLocation, ptr, depth + 4);
                    }
                    else
                    {
                        var loc = (_compareTo.Location.Item1 - (layerSize.X / 2), _compareTo.Location.Item2 - (layerSize.Y / 2), _compareTo.Location.Item3 - (layerSize.Z / 2));
                        OctreeNode layerONode = GenerateLayerOctreeNode(parentNode, depth, _compareTo, quad, nodeCollectionPtr, loc);
                        nodeCollection._000 = layerONode;
                        _octreeHeap.Add(layerONode.LocCode, defaultNodeCollection);
                        Add(layerONode, pointLocation, ptr, depth + 4);
                    }
                    break;
            }
            _octreeHeap[parentNode.LocCode] = nodeCollection;

            var toReflow = _compareTo.ChildLocationItems.ToList();
            _compareTo.ClearChildren();
            foreach (var node in toReflow)
            {
                TypedReference tr = __makeref(pointLocation);
                IntPtr _ptr = **(IntPtr**)(&tr);
                Add(OctreeNode, node, _ptr);
            }
        }

        private unsafe OctreeNode GenerateLayerOctreeNode(OctreeNode parentNode, int depth, NodeTypeLocation3D _compareTo, ulong quad, NodeCollection* nodeCollectionPtr, (float, float, float) quadOff)
        {
            var layerLocation = CreateLayerLocation(_compareTo, quadOff);
            var layerONode = new OctreeNode()
            {
                NodeType = NodeType.Layer,
                ObjPtr = layerLocation.Item2,
                ChildExists = 0
            };
            layerONode.Children = nodeCollectionPtr;
            layerONode.LocCode = (parentNode.LocCode | (quad << depth));
            return layerONode;
        }

        private unsafe (NodeTypeLocation3D, IntPtr) CreateLayerLocation(NodeTypeLocation3D compareTo, (float, float, float) quadOff)
        {
            var layerNode = new NodeTypeLocation3D(quadOff.Item1, quadOff.Item2, quadOff.Item3, true);

            TypedReference tr = __makeref(layerNode);
            IntPtr layer_ptr = **(IntPtr**)(&tr);
            while (_objRefs.ContainsKey(layer_ptr))
            {
                layerNode = new NodeTypeLocation3D(compareTo.Location.Item1, compareTo.Location.Item2, compareTo.Location.Item3, true);
                tr = __makeref(layerNode);
                layer_ptr = **(IntPtr**)(&tr);
            }

            //Remove this and add another ref to a point collection
            //Then from quad calculate the point location relative to the point referenced (head = center, 000 = center/2, 111 = center * 2)


            _objRefs.Add(layer_ptr, layerNode);
            return (layerNode, layer_ptr);
        }
    }
}
