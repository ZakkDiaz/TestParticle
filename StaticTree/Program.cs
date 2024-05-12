using System;
using System.Drawing;
using System.Threading.Tasks;
using ParticleLib.Models._3D;
namespace StaticTree
{
    class Program
    {
        public static ConcurrentOctree octree { get; set; }
        unsafe static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");

            var width = 1000;
            var height = 1000;
            var depth = 1000;
            octree = new ConcurrentOctree(new Point3D(), new Point3D(width, height, depth));
            //Random r = new Random();
            //var font = new Font("Arial", 20);
            //var z = 0;
            var interval = 10;
            int lastCount = 0;
        _start:
            var start = System.DateTime.UtcNow.Ticks;
            Emitter.AddEmitter();
            Emitter.AddEmitter();
            Emitter.AddEmitter();
            Emitter.AddEmitter();
            Emitter.AddEmitter();
            Emitter.AddEmitter();
            Emitter.AddEmitter();
            Emitter.AddEmitter();
            Emitter.AddEmitter();
            Emitter.AddEmitter();
            System.Threading.Thread.Sleep(1000);
            //Parallel.For(0, width, (x) =>
            //{
            //    Parallel.For(0, height, (y) =>
            //    {
            //        Parallel.For(0, depth, (z) =>
            //        {
            //            octree.AddAsync(ParticleLib.Models.ThreadSafeRandom.Next_s() + x, ParticleLib.Models.ThreadSafeRandom.Next_s() + y, ParticleLib.Models.ThreadSafeRandom.Next_s() + z);

            //            //var end = System.DateTime.UtcNow.Ticks;
            //            //var elapsedTicks = end - start;
            //            //var ms = elapsedTicks / TimeSpan.TicksPerSecond;
            //            //var rs = octree.Size();
            //            //var size = rs - lastCount;
            //            //var rate = size / (double)ms;
            //            //Console.WriteLine($"Elapsed: {ms} {size} {rate}/s {rs}");
            //        });
            //    });
            //});
            //for (var x = 0; x < width; x += interval)
            //{
            //    for (var y = 0; y < height; y += interval)
            //    {
            //        for (var z = 0; z < depth; z += interval)
            //        {
            //        }
            //    }
            //    var end = System.DateTime.UtcNow.Ticks;
            //    var elapsedTicks = end - start;
            //    var ms = elapsedTicks / TimeSpan.TicksPerSecond;
            //    var rs = octree.Size();
            //    var size = rs - lastCount;
            //    var rate = size / (double)ms;
            //    Console.WriteLine($"Elapsed: {ms} {size} {rate}/s {rs}");
            //}
            while (true)
            {
                var end = System.DateTime.UtcNow.Ticks;
                var elapsedTicks = end - start;
                var ms = elapsedTicks / TimeSpan.TicksPerSecond;
                var rs = octree.Size();
                var size = rs - lastCount;
                var rate = size / (double)ms;
                Console.WriteLine($"Elapsed: {ms} {size} {rate}/s {rs}");
                System.Threading.Thread.Sleep(1000);
            }
            var end2 = System.DateTime.UtcNow.Ticks;
            var elapsedTicks2 = end2 - start;
            var ms2 = elapsedTicks2 / TimeSpan.TicksPerSecond;
            var rs2 = octree.Size();
            var size2 = rs2 - lastCount;
            var rate2 = size2 / (double)ms2;
            lastCount = size2;
            Console.WriteLine($"Elapsed: {ms2} {size2} {rate2}/s {rs2} FIN");
            Console.ReadLine();
            goto _start;
            //var count = 100;
            //for (var i = 0; i < count; i++)
            //{
            //    //var locationToAdd = new NodeTypeLocation3D(, false);
            //    octree.Add((float)(r.NextDouble() * width), (float)(r.NextDouble() * height), (float)(r.NextDouble() * depth));
            //}

            //var otSize = octree.Size();
            //var otDepth = octree.Depth();

            //Bitmap bmp = new Bitmap(width*2, height * 2);
            //using var g = Graphics.FromImage(bmp);
            ////octree.Draw(g);
            //g.DrawString($"Size: {otSize} Depth: {otDepth}", font, Brushes.Black, new PointF(50, 50));
            //bmp.Save("_output.png");
        }
    }
}
