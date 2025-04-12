using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using ParticleLib.Models._3D;
using System.Runtime.CompilerServices;
using System.Globalization;
using BenchmarkDotNet.Order;
using System.Collections.Concurrent;

namespace PerformanceTests
{
    [Config(typeof(ComparisonConfig))]
    public class ComparisonBenchmarks
    {
        private Octree? _originalOctree;
        private ParticleLib.Modern.Models.Octree? _modernOctree;
        private List<ParticleLib.Models._3D.Point3D> _originalTestPoints = new();
        private List<ParticleLib.Modern.Models.Point3D> _modernTestPoints = new();
        
        private const int Width = 1000;
        private const int Height = 1000;
        private const int Depth = 1000;
        private const int NumParticles = 10000;
        
        [GlobalSetup]
        public void Setup()
        {
            // Create test points with the same values for both implementations
            var random = new Random(42); // Fixed seed for reproducibility
            for (int i = 0; i < NumParticles; i++)
            {
                float x = random.Next(Width);
                float y = random.Next(Height);
                float z = random.Next(Depth);
                
                _originalTestPoints.Add(new ParticleLib.Models._3D.Point3D(x, y, z));
                _modernTestPoints.Add(new ParticleLib.Modern.Models.Point3D(x, y, z));
            }
        }
        
        [Benchmark(Baseline = true, Description = "Original - Construction")]
        public void OriginalConstruction()
        {
            _originalOctree = new Octree(
                new ParticleLib.Models._3D.Point3D(), 
                new ParticleLib.Models._3D.Point3D(Width, Height, Depth)
            );
        }
        
        [Benchmark(Description = "Modern - Construction")]
        public void ModernConstruction()
        {
            _modernOctree = new ParticleLib.Modern.Models.Octree(
                ParticleLib.Modern.Models.Point3D.Origin,
                new ParticleLib.Modern.Models.Point3D(Width, Height, Depth)
            );
        }
        
        [Benchmark(Description = "Original - Add 10K Points")]
        public void OriginalAddPoints()
        {
            _originalOctree = new Octree(
                new ParticleLib.Models._3D.Point3D(), 
                new ParticleLib.Models._3D.Point3D(Width, Height, Depth)
            );
            
            foreach (var point in _originalTestPoints)
            {
                _originalOctree.Add(point.X, point.Y, point.Z);
            }
        }
        
        [Benchmark(Description = "Modern - Add 10K Points")]
        public void ModernAddPoints()
        {
            _modernOctree = new ParticleLib.Modern.Models.Octree(
                ParticleLib.Modern.Models.Point3D.Origin,
                new ParticleLib.Modern.Models.Point3D(Width, Height, Depth)
            );
            
            foreach (var point in _modernTestPoints)
            {
                _modernOctree.Add(point);
            }
        }
        
        [Benchmark(Description = "Original - Add 10K Points (Parallel)")]
        public void OriginalAddPointsParallel()
        {
            _originalOctree = new Octree(
                new ParticleLib.Models._3D.Point3D(), 
                new ParticleLib.Models._3D.Point3D(Width, Height, Depth)
            );
            
            Parallel.ForEach(_originalTestPoints, point =>
            {
                _originalOctree.AddAsync(point.X, point.Y, point.Z);
            });
            
            // Wait for all async operations to complete
            while (_originalOctree.AnyToAdd())
            {
                Thread.Sleep(10);
            }
        }
        
        [Benchmark(Description = "Modern - Add 10K Points (Parallel)")]
        public void ModernAddPointsParallel()
        {
            _modernOctree = new ParticleLib.Modern.Models.Octree(
                ParticleLib.Modern.Models.Point3D.Origin,
                new ParticleLib.Modern.Models.Point3D(Width, Height, Depth)
            );
            
            Parallel.ForEach(_modernTestPoints, point =>
            {
                _modernOctree.Add(point);
            });
        }
        
        [Benchmark(Description = "Original - Get Point Cloud")]
        public ParticleLib.Models._3D.Point3D[] OriginalGetPointCloud()
        {
            // First create and populate the octree
            _originalOctree = new Octree(
                new ParticleLib.Models._3D.Point3D(), 
                new ParticleLib.Models._3D.Point3D(Width, Height, Depth)
            );
            
            foreach (var point in _originalTestPoints.Take(1000))
            {
                _originalOctree.Add(point.X, point.Y, point.Z);
            }
            
            // Then measure lookup performance by getting the point cloud
            return _originalOctree.GetPointCloud();
        }
        
        [Benchmark(Description = "Modern - Get All Points")]
        public ParticleLib.Modern.Models.Point3D[] ModernGetAllPoints()
        {
            // First create and populate the octree
            _modernOctree = new ParticleLib.Modern.Models.Octree(
                ParticleLib.Modern.Models.Point3D.Origin,
                new ParticleLib.Modern.Models.Point3D(Width, Height, Depth)
            );
            
            foreach (var point in _modernTestPoints.Take(1000))
            {
                _modernOctree.Add(point);
            }
            
            // Then measure lookup performance by getting all points
            return _modernOctree.GetAllPoints().ToArray();
        }
        
        [Benchmark(Description = "Original - Process Particles")]
        public void OriginalProcessParticles()
        {
            // First create and populate the octree
            _originalOctree = new Octree(
                new ParticleLib.Models._3D.Point3D(), 
                new ParticleLib.Models._3D.Point3D(Width, Height, Depth)
            );
            
            foreach (var point in _originalTestPoints.Take(1000))
            {
                _originalOctree.Add(point.X, point.Y, point.Z);
            }
            
            // Then measure processing performance
            var processor = new TestParticleProcessor();
            _originalOctree.ProcessParticles(processor);
        }
        
        [Benchmark(Description = "Modern - Process Points")]
        public void ModernProcessPoints()
        {
            // First create and populate the octree
            _modernOctree = new ParticleLib.Modern.Models.Octree(
                ParticleLib.Modern.Models.Point3D.Origin,
                new ParticleLib.Modern.Models.Point3D(Width, Height, Depth)
            );
            
            foreach (var point in _modernTestPoints.Take(1000))
            {
                _modernOctree.Add(point);
            }
            
            // Then measure processing performance
            _modernOctree.ProcessPoints(_ => { /* Simulate work */ });
        }
        
        [Benchmark(Description = "Original - Depth Calculation")]
        public int OriginalDepthCalculation()
        {
            // First create and populate the octree
            _originalOctree = new Octree(
                new ParticleLib.Models._3D.Point3D(), 
                new ParticleLib.Models._3D.Point3D(Width, Height, Depth)
            );
            
            foreach (var point in _originalTestPoints.Take(1000))
            {
                _originalOctree.Add(point.X, point.Y, point.Z);
            }
            
            // Then measure depth calculation performance
            return _originalOctree.Depth();
        }
        
        [Benchmark(Description = "Modern - Depth Calculation")]
        public int ModernDepthCalculation()
        {
            // First create and populate the octree
            _modernOctree = new ParticleLib.Modern.Models.Octree(
                ParticleLib.Modern.Models.Point3D.Origin,
                new ParticleLib.Modern.Models.Point3D(Width, Height, Depth)
            );
            
            foreach (var point in _modernTestPoints.Take(1000))
            {
                _modernOctree.Add(point);
            }
            
            // Then measure depth calculation performance
            return _modernOctree.Depth;
        }
    }

    internal class TestParticleProcessor : IParticleProcessor
    {
        public void Process(OctreeNode octreeNode, ref ConcurrentDictionary<nint, NodeTypeLayer3D> locationRefs, ref ConcurrentDictionary<ulong, NodeCollection> octreeHeap)
        {

        }
    }

    public class ComparisonConfig : ManualConfig
    {
        public ComparisonConfig()
        {
            AddColumn(StatisticColumn.Min);
            AddColumn(StatisticColumn.Max);
            AddColumn(StatisticColumn.Mean);
            AddColumn(StatisticColumn.Median);
            AddColumn(StatisticColumn.StdDev);
            AddColumn(
                BaselineRatioColumn.RatioMean
            );
            
            // Add memory diagnostics
            AddDiagnoser(BenchmarkDotNet.Diagnosers.MemoryDiagnoser.Default);
            
            // Set the column providers
            WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Percentage));
        }
    }
}
