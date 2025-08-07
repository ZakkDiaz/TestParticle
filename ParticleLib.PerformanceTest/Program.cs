using System;
using System.Collections.Generic;
using System.Diagnostics;
using ParticleLib.Models._3D;
using ModernOctree = ParticleLib.Modern.Models._3D.Octree;
using ModernPoint3D = ParticleLib.Modern.Models._3D.Point3D;
using ModernAAABBB = ParticleLib.Modern.Models._3D.AAABBB;
using OriginalOctree = ParticleLib.Models._3D.Octree;
using OriginalPoint3D = ParticleLib.Models._3D.Point3D;

namespace ParticleLib.PerformanceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Octree Performance Test");
            Console.WriteLine("=======================");

            // Configuration
            int particleCount = 10000;
            int iterations = 10;
            var random = new Random(42); // Fixed seed for reproducibility

            // Generate test data
            Console.WriteLine($"Generating {particleCount} random particles...");
            var originalPoints = new List<OriginalPoint3D>();
            var modernPoints = new List<ModernPoint3D>();

            for (int i = 0; i < particleCount; i++)
            {
                float x = (float)(random.NextDouble() * 200 - 100);
                float y = (float)(random.NextDouble() * 200 - 100);
                float z = (float)(random.NextDouble() * 200 - 100);

                originalPoints.Add(new OriginalPoint3D(x, y, z));
                modernPoints.Add(new ModernPoint3D(x, y, z));
            }

            Console.WriteLine("Running performance tests...");

            // Test results
            long originalConstructionTime = 0;
            long modernConstructionTime = 0;
            long originalAddTime = 0;
            long modernAddTime = 0;
            long originalGetPointsTime = 0;
            long modernGetPointsTime = 0;
            long originalDepthTime = 0;
            long modernDepthTime = 0;

            // Run multiple iterations for more stable results
            for (int i = 0; i < iterations; i++)
            {
                Console.WriteLine($"Iteration {i + 1}/{iterations}");

                // Test original octree construction
                var originalStopwatch = Stopwatch.StartNew();
                var originalOctree = new OriginalOctree(
                    new OriginalPoint3D(-100, -100, -100),
                    new OriginalPoint3D(100, 100, 100)
                );
                originalStopwatch.Stop();
                originalConstructionTime += originalStopwatch.ElapsedMilliseconds;

                // Test modern octree construction
                var modernStopwatch = Stopwatch.StartNew();
                var modernOctree = new ModernOctree(
                    new ModernAAABBB(
                        new ModernPoint3D(-100, -100, -100),
                        new ModernPoint3D(100, 100, 100)
                    )
                );
                modernStopwatch.Stop();
                modernConstructionTime += modernStopwatch.ElapsedMilliseconds;

                // Test adding particles to original octree
                originalStopwatch.Restart();
                foreach (var point in originalPoints)
                {
                    originalOctree.Add(point.X, point.Y, point.Z);
                }
                originalStopwatch.Stop();
                originalAddTime += originalStopwatch.ElapsedMilliseconds;

                // Test adding particles to modern octree
                modernStopwatch.Restart();
                modernOctree.AddParticles(modernPoints);
                modernStopwatch.Stop();
                modernAddTime += modernStopwatch.ElapsedMilliseconds;

                // Test getting points from original octree
                originalStopwatch.Restart();
                var originalPointCloud = originalOctree.GetPointCloud();
                originalStopwatch.Stop();
                originalGetPointsTime += originalStopwatch.ElapsedMilliseconds;

                // Test getting points from modern octree
                modernStopwatch.Restart();
                var modernPointCloud = modernOctree.GetAllParticles();
                modernStopwatch.Stop();
                modernGetPointsTime += modernStopwatch.ElapsedMilliseconds;

                // Test depth calculation for original octree
                originalStopwatch.Restart();
                var originalDepth = originalOctree.Depth();
                originalStopwatch.Stop();
                originalDepthTime += originalStopwatch.ElapsedMilliseconds;

                // Test depth calculation for modern octree
                modernStopwatch.Restart();
                var modernDepth = modernOctree.GetDepth();
                modernStopwatch.Stop();
                modernDepthTime += modernStopwatch.ElapsedMilliseconds;
            }

            // Calculate averages
            double originalConstructionAvg = originalConstructionTime / (double)iterations;
            double modernConstructionAvg = modernConstructionTime / (double)iterations;
            double originalAddAvg = originalAddTime / (double)iterations;
            double modernAddAvg = modernAddTime / (double)iterations;
            double originalGetPointsAvg = originalGetPointsTime / (double)iterations;
            double modernGetPointsAvg = modernGetPointsTime / (double)iterations;
            double originalDepthAvg = originalDepthTime / (double)iterations;
            double modernDepthAvg = modernDepthTime / (double)iterations;

            // Calculate speedup factors
            double constructionSpeedup = originalConstructionAvg / Math.Max(0.0000001, modernConstructionAvg);
            double addSpeedup = originalAddAvg / Math.Max(0.0000001, modernAddAvg);
            double getPointsSpeedup = originalGetPointsAvg / Math.Max(0.0000001, modernGetPointsAvg);
            double depthSpeedup = originalDepthAvg / Math.Max(0.0000001, modernDepthAvg);

            // Output results
            Console.WriteLine("\nResults:");
            Console.WriteLine("-----------------------------------------------------------");
            Console.WriteLine($"Construction:      Original: {originalConstructionAvg:F2}ms, Modern: {modernConstructionAvg:F2}ms, Speedup: {constructionSpeedup:F2}x");
            Console.WriteLine($"Adding particles:  Original: {originalAddAvg:F2}ms, Modern: {modernAddAvg:F2}ms, Speedup: {addSpeedup:F2}x");
            Console.WriteLine($"Getting points:    Original: {originalGetPointsAvg:F2}ms, Modern: {modernGetPointsAvg:F2}ms, Speedup: {getPointsSpeedup:F2}x");
            Console.WriteLine($"Depth calculation: Original: {originalDepthAvg:F2}ms, Modern: {modernDepthAvg:F2}ms, Speedup: {depthSpeedup:F2}x");
            Console.WriteLine("-----------------------------------------------------------");
            Console.WriteLine($"Overall speedup: {(constructionSpeedup + addSpeedup + getPointsSpeedup + depthSpeedup) / 4:F2}x");

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
