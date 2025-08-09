using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

// aliases
using ModernAAABBB = ParticleLib.Modern.Models._3D.AAABBB;
using ModernOctree = ParticleLib.Modern.Models._3D.Octree;
using ModernPoint3D = ParticleLib.Modern.Models._3D.Point3D;

using OriginalOctree = ParticleLib.Models._3D.Octree;
using OriginalPoint3D = ParticleLib.Models._3D.Point3D;

namespace ParticleLib.PerformanceTest
{
    internal struct Phase
    {
        public double WallMs;
        public double CpuMs;
        public long Bytes;
        public int G0, G1, G2;
        public object Result;
    }

    class Program
    {
        // ---------- CONFIG ----------
        private const int ParticleCount = 100000;
        private const int Iterations = 3;
        private const int RandSeed = 42;
        private const float Min = -100f;
        private const float Max = 100f;
        private static readonly int LogicalCores = Environment.ProcessorCount;

        private static readonly Process ThisProc = Process.GetCurrentProcess();

        // ---------- ENTRY ----------
        static void Main()
        {
            Console.WriteLine("Octree Performance Comparison");
            Console.WriteLine("==========================================");
            Console.WriteLine($"Particles       : {ParticleCount:N0}");
            Console.WriteLine($"Iterations      : {Iterations}");
            Console.WriteLine($"Logical cores   : {LogicalCores}");
            Console.WriteLine($"Coordinate cube : [{Min},{Max}]");
            Console.WriteLine();

            // 1) prepare identical test data
            var rnd = new Random(RandSeed);
            var origPts = new List<OriginalPoint3D>(ParticleCount);
            var modPts = new List<ModernPoint3D>(ParticleCount);
            for (int i = 0; i < ParticleCount; i++)
            {
                float x = (float)(rnd.NextDouble() * (Max - Min) + Min);
                float y = (float)(rnd.NextDouble() * (Max - Min) + Min);
                float z = (float)(rnd.NextDouble() * (Max - Min) + Min);
                origPts.Add(new OriginalPoint3D(x, y, z));
                modPts.Add(new ModernPoint3D(x, y, z));
            }

            // 2) buckets for results
            var cOrig = new List<Phase>(); var cMod = new List<Phase>();
            var iOrig = new List<Phase>(); var iMod = new List<Phase>();
            var pOrig = new List<Phase>(); var pMod = new List<Phase>();
            var dOrig = new List<Phase>(); var dMod = new List<Phase>();
            var rOrig = new List<Phase>(); var rMod = new List<Phase>();
            var mtMod = new List<Phase>();             // multi-threaded insert

            // 3) benchmark loop
            for (int it = 1; it <= Iterations; it++)
            {
                Console.WriteLine($"Iteration {it}/{Iterations}");

                // construct
                var (oTree, p1) = Timed(() => new OriginalOctree(
                                            new OriginalPoint3D(Min, Min, Min),
                                            new OriginalPoint3D(Max, Max, Max)));
                var (mTree, p2) = Timed(() => new ModernOctree(
                                            new ModernAAABBB(
                                                new ModernPoint3D(Min, Min, Min),
                                                new ModernPoint3D(Max, Max, Max))));
                cOrig.Add(p1); cMod.Add(p2);

                // single-thread inserts
                iOrig.Add(Timed(() =>
                {
                    foreach (var p in origPts) oTree.Add(p.X, p.Y, p.Z);
                }).phase);
                iMod.Add(Timed(() => mTree.AddParticles(modPts)).phase);

                // multi-thread insert on a fresh modern octree
                //var (mtTree, mtPhase) = Timed(() =>
                //{
                //    var fresh = new ModernOctree(
                //        new ModernAAABBB(new ModernPoint3D(Min, Min, Min),
                //                         new ModernPoint3D(Max, Max, Max)));

                //    int threads = LogicalCores;
                //    int batch = ParticleCount / threads;

                //    Parallel.For(0, threads, t =>
                //    {
                //        int from = t * batch;
                //        int to = (t == threads - 1) ? ParticleCount : from + batch;
                //        var slice = modernPts.GetRange(from, to - from);
                //        fresh.AddParticles(slice);          // single call per slice
                //    });

                //    return fresh;
                //});
                //mtMod.Add(mtPhase);

                // export point cloud
                pOrig.Add(Timed(() => oTree.GetPointCloud()).phase);
                pMod.Add(Timed(() => mTree.GetAllParticles()).phase);

                // depth
                dOrig.Add(Timed(() => oTree.Depth()).phase);
                dMod.Add(Timed(() => mTree.GetDepth()).phase);

                // remove 50 % random particles
                var removals = Enumerable.Range(0, ParticleCount)
                                         .Where(_ => rnd.NextDouble() < 0.5)
                                         .ToArray();
                //rOrig.Add(Timed(() =>
                //{
                //    foreach (var idx in removals) oTree.Remove(idx);
                //}).phase);

                rMod.Add(Timed(() =>
                {
                    foreach (var idx in removals) mTree.RemoveParticle(idx);
                }).phase);

                if (it == 1)
                    Console.WriteLine($"Modern nodes after insert : {mTree.NodeCount:N0}");
            }

            // 4) table
            PrintRow("Construct", cOrig, cMod);
            PrintRow("Insert 1T", iOrig, iMod);
            PrintRow("Insert 8T", iOrig, mtMod, labelOrig: "Orig (1T)", labelMod: "Modern 8T");
            PrintRow("Remove", rOrig, rMod);
            PrintRow("Export", pOrig, pMod);
            PrintRow("Depth", dOrig, dMod);

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        // ---------- helper methods ----------

        private static (T result, Phase phase) Timed<T>(Func<T> work)
        {
            GC.Collect(2, GCCollectionMode.Forced, true);
            var memBefore = GC.GetAllocatedBytesForCurrentThread();
            int g0B = GC.CollectionCount(0), g1B = GC.CollectionCount(1), g2B = GC.CollectionCount(2);
            var cpuBefore = ThisProc.TotalProcessorTime;
            var sw = Stopwatch.StartNew();

            T result = work();

            sw.Stop();
            var cpuAfter = ThisProc.TotalProcessorTime;
            long memAfter = GC.GetAllocatedBytesForCurrentThread();

            return (result, new Phase
            {
                WallMs = sw.Elapsed.TotalMilliseconds,
                CpuMs = (cpuAfter - cpuBefore).TotalMilliseconds,
                Bytes = memAfter - memBefore,
                G0 = GC.CollectionCount(0) - g0B,
                G1 = GC.CollectionCount(1) - g1B,
                G2 = GC.CollectionCount(2) - g2B,
                Result = result
            });
        }

        private static (object dummy, Phase phase) Timed(Action work)
        {
            var (_, p) = Timed(() => { work(); return (object)null; });
            return (null, p);
        }

        private static Phase Average(IEnumerable<Phase> list)
        {
            var a = list.ToArray();
            if(a.Length == 0) return new Phase();
            return new Phase
            {
                WallMs = a.Average(x => x.WallMs),
                CpuMs = a.Average(x => x.CpuMs),
                Bytes = (long)a.Average(x => x.Bytes),
                G0 = (int)a.Average(x => x.G0),
                G1 = (int)a.Average(x => x.G1),
                G2 = (int)a.Average(x => x.G2)
            };
        }

        private static double Speed(IEnumerable<Phase> a, IEnumerable<Phase> b) =>
            Average(a).WallMs / Math.Max(0.0001, Average(b).WallMs);

        private static void PrintRow(string name,
                                     IEnumerable<Phase> orig,
                                     IEnumerable<Phase> mod,
                                     string labelOrig = "Original",
                                     string labelMod = "Modern")
        {
            var o = Average(orig);
            var m = Average(mod);
            double util = 100.0 * m.CpuMs /
                          (m.WallMs * LogicalCores);

            Console.WriteLine("\n--- {0} ---", name);
            Console.WriteLine($"  {labelOrig,-9} wall ms : {o.WallMs:F2}");
            Console.WriteLine($"  {labelMod,-9} wall ms : {m.WallMs:F2}");
            Console.WriteLine($"  Speedup               : {o.WallMs / Math.Max(0.0001, m.WallMs):F2}x");
            Console.WriteLine($"  {labelMod,-9} allocKB: {m.Bytes / 1024:N0}");
            Console.WriteLine($"  {labelMod,-9} GC G0/1/2: {m.G0}/{m.G1}/{m.G2}");
            Console.WriteLine($"  {labelMod,-9} CPU util: {util:F1} %");
        }
    }
}
