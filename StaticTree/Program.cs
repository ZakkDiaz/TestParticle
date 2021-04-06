using System;
using System.Drawing;
using ParticleLib.Models._3D;
namespace StaticTree
{
    class Program
    {
        unsafe static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var width = 1000;
            var height = 1000;
            var depth = 1000;
            var octree = new Octree(new Point3D(), new Point3D(width, height, depth));
            Random r = new Random();
            var font = new Font("Arial", 20);
            var z = 0;
            //var interval = 300;
            //for (var x = 0; x < width; x += interval)
            //{
            //    for(var y = 0; y < height; y += interval)
            //    {
            //        var locationToAdd = new NodeTypeLocation3D((float)(r.NextDouble()*width), (float)(r.NextDouble() * height), z);
            //        octree.Add(locationToAdd);
            //        //for(var z = 0; z < depth; z += 100)
            //        //{
            //        //    var locationToAdd = new NodeTypeLocation3D(x, y, z);
            //        //    octree.Add(locationToAdd);
            //        //}
            //    }
            //}

            var count = 100;
            for (var i = 0; i < count; i++)
            {
                var locationToAdd = new NodeTypeLocation3D((float)(r.NextDouble() * width), (float)(r.NextDouble() * height), (float)(r.NextDouble() * depth));
                octree.Add(locationToAdd);
            }

            var otSize = octree.Size();
            var otDepth = octree.Depth();

            Bitmap bmp = new Bitmap(width*2, height * 2);
            using var g = Graphics.FromImage(bmp);
            octree.Draw(g);
            g.DrawString($"Size: {otSize} Depth: {otDepth}", font, Brushes.Black, new PointF(50, 50));
            bmp.Save("_output.png");
        }
    }
}
