using System.Collections.Concurrent;
using System.Diagnostics;

namespace PerformanceTests
{
    public class Program
    {
        private const int Width = 1000;
        private const int Height = 1000;
        private const int Depth = 1000;
        private const int NumParticles = 1_000_000;
        private const int TestBatchSize = 100_000;
        private const int NumIterations = 3;
        private const int AccuracyTestSize = 10_000;

        public static void Main(string[] args)
        {
            Console.WriteLine("ParticleLib Performance Test with 1 Million Particles");
            Console.WriteLine("====================================================");
            
            // Create test points with the same values for both implementations
            Console.WriteLine("Generating 1 million test particles...");
            var random = new Random(42); // Fixed seed for reproducibility
            var originalTestPoints = new List<ParticleLib.Models._3D.Point3D>();
            var modernTestPoints = new List<ParticleLib.Modern.Models.Point3D>();
            
            for (int i = 0; i < NumParticles; i++)
            {
                float x = (float)random.NextDouble() * Width;
                float y = (float)random.NextDouble() * Height;
                float z = (float)random.NextDouble() * Depth;
                
                originalTestPoints.Add(new ParticleLib.Models._3D.Point3D(x, y, z));
                modernTestPoints.Add(new ParticleLib.Modern.Models.Point3D(x, y, z));
            }
            
            Console.WriteLine("Particle generation complete.");
            
            // Run accuracy tests first
            Console.WriteLine("\nRunning accuracy tests...");
            RunAccuracyTests(originalTestPoints.Take(AccuracyTestSize).ToList(), 
                            modernTestPoints.Take(AccuracyTestSize).ToList());
            
            // Run the performance tests
            Console.WriteLine("\nRunning performance tests...");
            
            // Warmup
            Console.WriteLine("\nWarmup...");
            RunOriginalConstruction();
            RunModernConstruction();
            
            // Construction tests
            Console.WriteLine("\nConstruction Performance:");
            RunPerformanceTest("Original", () => RunOriginalConstruction());
            RunPerformanceTest("Modern", () => RunModernConstruction());
            
            // Add points tests with throughput measurement
            Console.WriteLine($"\nAdding {TestBatchSize:N0} Points Performance (Particles/Second):");
            MeasureOriginalParticleAdditionRate(originalTestPoints.Take(TestBatchSize).ToList());
            MeasureModernParticleAdditionRate(modernTestPoints.Take(TestBatchSize).ToList());
            
            // Get points tests
            Console.WriteLine("\nGet Points Performance (after adding 100,000 points):");
            var originalOctree = RunOriginalAddPoints(originalTestPoints.Take(TestBatchSize).ToList());
            var modernOctree = RunModernAddPoints(modernTestPoints.Take(TestBatchSize).ToList());
            
            RunPerformanceTest("Original", () => originalOctree.GetPointCloud());
            RunPerformanceTest("Modern", () => modernOctree.GetAllPoints().ToArray());
            
            // Depth calculation tests
            Console.WriteLine("\nDepth Calculation Performance:");
            RunPerformanceTest("Original", () => originalOctree.Depth());
            RunPerformanceTest("Modern", () => modernOctree.Depth);
            
            Console.WriteLine("\nTests complete. Press any key to exit.");
            Console.ReadKey();
        }
        
        private static void RunAccuracyTests(List<ParticleLib.Models._3D.Point3D> originalPoints, 
                                           List<ParticleLib.Modern.Models.Point3D> modernPoints)
        {
            Console.WriteLine($"Running accuracy tests with {originalPoints.Count:N0} particles...");
            
            // Create the octrees
            var originalOctree = new ParticleLib.Models._3D.Octree(
                new ParticleLib.Models._3D.Point3D(), 
                new ParticleLib.Models._3D.Point3D(Width, Height, Depth)
            );
            
            var modernOctree = new ParticleLib.Modern.Models.Octree(
                ParticleLib.Modern.Models.Point3D.Origin,
                new ParticleLib.Modern.Models.Point3D(Width, Height, Depth)
            );
            
            // Add points to both octrees
            for (int i = 0; i < originalPoints.Count; i++)
            {
                originalOctree.Add(originalPoints[i].X, originalPoints[i].Y, originalPoints[i].Z);
                modernOctree.Add(modernPoints[i]);
            }
            
            // Test 1: Point Count
            var originalPointCloud = originalOctree.GetPointCloud();
            var modernPointCloud = modernOctree.GetAllPoints().ToArray();
            
            bool pointCountMatch = originalPointCloud.Length == modernPointCloud.Length;
            Console.WriteLine($"Point Count Test: {(pointCountMatch ? "PASSED" : "FAILED")}");
            Console.WriteLine($"  Original: {originalPointCloud.Length:N0} points");
            Console.WriteLine($"  Modern:   {modernPointCloud.Length:N0} points");
            
            // Test 2: Depth
            int originalDepth = originalOctree.Depth();
            int modernDepth = modernOctree.Depth;
            
            bool depthMatch = originalDepth == modernDepth;
            Console.WriteLine($"Depth Test: {(depthMatch ? "PASSED" : "FAILED")}");
            Console.WriteLine($"  Original: {originalDepth}");
            Console.WriteLine($"  Modern:   {modernDepth}");
            
            // Test 3: Point Position Accuracy (check a few random points)
            if (originalPointCloud.Length > 0 && modernPointCloud.Length > 0)
            {
                // Sort points by X coordinate for comparison
                var sortedOriginalPoints = originalPointCloud
                    .OrderBy(p => p.X)
                    .ThenBy(p => p.Y)
                    .ThenBy(p => p.Z)
                    .ToArray();
                
                var sortedModernPoints = modernPointCloud
                    .OrderBy(p => p.X)
                    .ThenBy(p => p.Y)
                    .ThenBy(p => p.Z)
                    .ToArray();
                
                // Check a sample of points (first, middle, last)
                bool positionAccuracyPassed = true;
                int samplesToCheck = Math.Min(10, sortedOriginalPoints.Length);
                
                for (int i = 0; i < samplesToCheck; i++)
                {
                    int index = (i * sortedOriginalPoints.Length) / samplesToCheck;
                    if (index >= sortedOriginalPoints.Length) index = sortedOriginalPoints.Length - 1;
                    
                    var originalPoint = sortedOriginalPoints[index];
                    var modernPoint = sortedModernPoints[index];
                    
                    // Check if points are within a small epsilon of each other
                    const float epsilon = 0.001f;
                    bool xMatch = Math.Abs(originalPoint.X - modernPoint.X) < epsilon;
                    bool yMatch = Math.Abs(originalPoint.Y - modernPoint.Y) < epsilon;
                    bool zMatch = Math.Abs(originalPoint.Z - modernPoint.Z) < epsilon;
                    
                    if (!xMatch || !yMatch || !zMatch)
                    {
                        positionAccuracyPassed = false;
                        Console.WriteLine($"  Position mismatch at index {index}:");
                        Console.WriteLine($"    Original: ({originalPoint.X}, {originalPoint.Y}, {originalPoint.Z})");
                        Console.WriteLine($"    Modern:   ({modernPoint.X}, {modernPoint.Y}, {modernPoint.Z})");
                    }
                }
                
                Console.WriteLine($"Position Accuracy Test: {(positionAccuracyPassed ? "PASSED" : "FAILED")}");
            }
            
            // Overall accuracy assessment
            bool allTestsPassed = pointCountMatch && depthMatch;
            Console.WriteLine($"\nOverall Accuracy Assessment: {(allTestsPassed ? "PASSED" : "FAILED")}");
        }
        
        private static void MeasureOriginalParticleAdditionRate(List<ParticleLib.Models._3D.Point3D> points)
        {
            var times = new List<long>();
            
            for (int i = 0; i < NumIterations; i++)
            {
                var sw = Stopwatch.StartNew();
                
                var octree = new ParticleLib.Models._3D.Octree(
                    new ParticleLib.Models._3D.Point3D(), 
                    new ParticleLib.Models._3D.Point3D(Width, Height, Depth)
                );
                
                foreach (var point in points)
                {
                    octree.Add(point.X, point.Y, point.Z);
                }
                
                sw.Stop();
                times.Add(sw.ElapsedMilliseconds);
            }
            
            double avgTime = times.Average();
            double minTime = times.Min();
            double maxTime = times.Max();
            
            double avgParticlesPerSecond = TestBatchSize / (avgTime / 1000.0);
            double maxParticlesPerSecond = TestBatchSize / (minTime / 1000.0);
            
            Console.WriteLine($"Original: Avg={avgTime:F2}ms, Min={minTime:F2}ms, Max={maxTime:F2}ms");
            Console.WriteLine($"Original: Avg={avgParticlesPerSecond:N0} particles/sec, Max={maxParticlesPerSecond:N0} particles/sec");
        }
        
        private static void MeasureModernParticleAdditionRate(List<ParticleLib.Modern.Models.Point3D> points)
        {
            var times = new List<long>();
            
            for (int i = 0; i < NumIterations; i++)
            {
                var sw = Stopwatch.StartNew();
                
                var octree = new ParticleLib.Modern.Models.Octree(
                    ParticleLib.Modern.Models.Point3D.Origin,
                    new ParticleLib.Modern.Models.Point3D(Width, Height, Depth)
                );
                
                foreach (var point in points)
                {
                    octree.Add(point);
                }
                
                sw.Stop();
                times.Add(sw.ElapsedMilliseconds);
            }
            
            double avgTime = times.Average();
            double minTime = times.Min();
            double maxTime = times.Max();
            
            double avgParticlesPerSecond = TestBatchSize / (avgTime / 1000.0);
            double maxParticlesPerSecond = TestBatchSize / (minTime / 1000.0);
            
            Console.WriteLine($"Modern: Avg={avgTime:F2}ms, Min={minTime:F2}ms, Max={maxTime:F2}ms");
            Console.WriteLine($"Modern: Avg={avgParticlesPerSecond:N0} particles/sec, Max={maxParticlesPerSecond:N0} particles/sec");
        }
        
        private static void RunPerformanceTest(string name, Action action)
        {
            var times = new List<long>();
            
            for (int i = 0; i < NumIterations; i++)
            {
                var sw = Stopwatch.StartNew();
                action();
                sw.Stop();
                times.Add(sw.ElapsedMilliseconds);
            }
            
            double avgTime = times.Average();
            double minTime = times.Min();
            double maxTime = times.Max();
            
            Console.WriteLine($"{name}: Avg={avgTime:F2}ms, Min={minTime:F2}ms, Max={maxTime:F2}ms");
        }
        
        private static T RunPerformanceTest<T>(string name, Func<T> func)
        {
            var times = new List<long>();
            T result = default;
            
            for (int i = 0; i < NumIterations; i++)
            {
                var sw = Stopwatch.StartNew();
                result = func();
                sw.Stop();
                times.Add(sw.ElapsedMilliseconds);
            }
            
            double avgTime = times.Average();
            double minTime = times.Min();
            double maxTime = times.Max();
            
            Console.WriteLine($"{name}: Avg={avgTime:F2}ms, Min={minTime:F2}ms, Max={maxTime:F2}ms");
            
            return result;
        }
        
        private static ParticleLib.Models._3D.Octree RunOriginalConstruction()
        {
            return new ParticleLib.Models._3D.Octree(
                new ParticleLib.Models._3D.Point3D(), 
                new ParticleLib.Models._3D.Point3D(Width, Height, Depth)
            );
        }
        
        private static ParticleLib.Modern.Models.Octree RunModernConstruction()
        {
            return new ParticleLib.Modern.Models.Octree(
                ParticleLib.Modern.Models.Point3D.Origin,
                new ParticleLib.Modern.Models.Point3D(Width, Height, Depth)
            );
        }
        
        private static ParticleLib.Models._3D.Octree RunOriginalAddPoints(List<ParticleLib.Models._3D.Point3D> points)
        {
            var octree = new ParticleLib.Models._3D.Octree(
                new ParticleLib.Models._3D.Point3D(), 
                new ParticleLib.Models._3D.Point3D(Width, Height, Depth)
            );
            
            foreach (var point in points)
            {
                octree.Add(point.X, point.Y, point.Z);
            }
            
            return octree;
        }
        
        private static ParticleLib.Modern.Models.Octree RunModernAddPoints(List<ParticleLib.Modern.Models.Point3D> points)
        {
            var octree = new ParticleLib.Modern.Models.Octree(
                ParticleLib.Modern.Models.Point3D.Origin,
                new ParticleLib.Modern.Models.Point3D(Width, Height, Depth)
            );
            
            foreach (var point in points)
            {
                octree.Add(point);
            }
            
            return octree;
        }
    }
}
