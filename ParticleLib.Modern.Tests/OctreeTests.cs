using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParticleLib.Modern.Models;
using System.Collections.Concurrent;
using System.Reflection;

namespace ParticleLib.Modern.Tests
{
    [TestClass]
    public class OctreeTests
    {
        private const float WorldSize = 1000f;
        private const float Epsilon = 0.0001f;

        #region Basic Functionality Tests

        [TestMethod]
        public void Octree_Creation_InitializesCorrectly()
        {
            // Arrange & Act
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );

            // Assert
            Assert.AreEqual(0, octree.Depth, "Initial depth should be 0");
            Assert.AreEqual(1, octree.NodeCount, "Initial node count should be 1 (root node)");
            Assert.AreEqual(0, octree.GetAllPoints().Count(), "Initial point count should be 0");
            Assert.AreEqual(Point3D.Origin, octree.Bounds.Min, "Minimum bounds should match initialization");
            Assert.AreEqual(new Point3D(WorldSize, WorldSize, WorldSize), octree.Bounds.Max, "Maximum bounds should match initialization");
        }

        [TestMethod]
        public void Octree_AddPoint_SinglePoint_AddsCorrectly()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );

            // Act
            var point = new Point3D(WorldSize / 2, WorldSize / 2, WorldSize / 2);
            octree.Add(point);

            // Assert
            Assert.AreEqual(1, octree.GetAllPoints().Count(), "Should have 1 point");
            Assert.IsTrue(octree.GetAllPoints().Contains(point), "Should contain the added point");
            Assert.AreEqual(1, octree.NodeCount, "Should still have only the root node");
        }

        [TestMethod]
        public void Octree_AddPoint_MultiplePoints_AddsCorrectly()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );

            // Act
            var points = new List<Point3D>
            {
                new Point3D(WorldSize / 4, WorldSize / 4, WorldSize / 4),
                new Point3D(WorldSize / 2, WorldSize / 2, WorldSize / 2),
                new Point3D(3 * WorldSize / 4, 3 * WorldSize / 4, 3 * WorldSize / 4)
            };

            foreach (var point in points)
            {
                octree.Add(point);
            }

            // Assert
            var allPoints = octree.GetAllPoints().ToList();
            Assert.AreEqual(points.Count, allPoints.Count, "Should have all points");
            foreach (var point in points)
            {
                Assert.IsTrue(allPoints.Any(p => 
                    Math.Abs(p.X - point.X) < Epsilon && 
                    Math.Abs(p.Y - point.Y) < Epsilon && 
                    Math.Abs(p.Z - point.Z) < Epsilon), 
                    $"Should contain point {point}");
            }
        }

        [TestMethod]
        public void Octree_RemovePoint_RemovesCorrectly()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );

            var point = new Point3D(WorldSize / 2, WorldSize / 2, WorldSize / 2);
            octree.Add(point);

            // Act
            bool removed = octree.Remove(point);

            // Assert
            Assert.IsTrue(removed, "Remove should return true for existing point");
            Assert.AreEqual(0, octree.GetAllPoints().Count(), "Should have 0 points after removal");
        }

        [TestMethod]
        public void Octree_RemovePoint_NonExistentPoint_ReturnsFalse()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );

            var point = new Point3D(WorldSize / 2, WorldSize / 2, WorldSize / 2);
            octree.Add(point);

            var nonExistentPoint = new Point3D(WorldSize / 4, WorldSize / 4, WorldSize / 4);

            // Act
            bool removed = octree.Remove(nonExistentPoint);

            // Assert
            Assert.IsFalse(removed, "Remove should return false for non-existent point");
            Assert.AreEqual(1, octree.GetAllPoints().Count(), "Should still have 1 point");
        }

        #endregion

        #region Subdivision Tests

        [TestMethod]
        public void Octree_Subdivision_TriggersAtThreshold()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );

            // The subdivision threshold is 8 (MaxPointsPerNode constant in Octree.cs)
            const int threshold = 8;

            // Add points at the same location up to threshold
            var point = new Point3D(WorldSize / 2, WorldSize / 2, WorldSize / 2);
            for (int i = 0; i < threshold; i++)
            {
                octree.Add(new Point3D(
                    point.X + i * 0.001f, // Tiny offset to make them unique
                    point.Y,
                    point.Z
                ));
            }

            // Record state before adding the point that should trigger subdivision
            int nodeCountBefore = octree.NodeCount;

            // Act - add one more point to trigger subdivision
            octree.Add(new Point3D(point.X + threshold * 0.001f, point.Y, point.Z));

            // Assert
            Assert.IsTrue(octree.NodeCount > nodeCountBefore, "Node count should increase after subdivision");
            Assert.IsTrue(octree.Depth > 0, "Depth should be greater than 0 after subdivision");
        }

        [TestMethod]
        public void Octree_Subdivision_DistributesPointsCorrectly()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );

            // Add points in different octants
            var points = new List<Point3D>();
            
            // Add points to all 8 octants of the root
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        float xPos = x * 0.75f * WorldSize + 0.125f * WorldSize;
                        float yPos = y * 0.75f * WorldSize + 0.125f * WorldSize;
                        float zPos = z * 0.75f * WorldSize + 0.125f * WorldSize;
                        
                        // Add multiple points to each octant to trigger subdivision
                        for (int i = 0; i < 10; i++)
                        {
                            var point = new Point3D(
                                xPos + i * 0.01f,
                                yPos + i * 0.01f,
                                zPos + i * 0.01f
                            );
                            points.Add(point);
                            octree.Add(point);
                        }
                    }
                }
            }

            // Assert
            Assert.IsTrue(octree.Depth > 1, "Octree should have subdivided multiple times");
            Assert.AreEqual(points.Count, octree.GetAllPoints().Count(), "All points should be in the octree");
            
            // Verify each point is still retrievable by checking if it's in the GetAllPoints result
            var allPoints = octree.GetAllPoints().ToList();
            foreach (var point in points)
            {
                Assert.IsTrue(allPoints.Any(p => 
                    Math.Abs(p.X - point.X) < Epsilon && 
                    Math.Abs(p.Y - point.Y) < Epsilon && 
                    Math.Abs(p.Z - point.Z) < Epsilon), 
                    $"Point {point} should be retrievable from the octree");
            }
        }

        [TestMethod]
        public void Octree_MaxDepth_RespectsLimit()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );

            // The max depth is 21
            const int maxDepth = 21;

            // Create a very small region to force deep subdivision
            var smallRegion = new Point3D(0.001f, 0.001f, 0.001f);

            // Add many points to the same small region to force deep subdivision
            for (int i = 0; i < 100; i++)
            {
                octree.Add(new Point3D(
                    smallRegion.X + i * 0.0000001f, // Extremely small offset
                    smallRegion.Y + i * 0.0000001f,
                    smallRegion.Z + i * 0.0000001f
                ));
            }

            // Assert
            Assert.IsTrue(octree.Depth <= maxDepth, $"Depth should not exceed max depth ({maxDepth})");
        }

        #endregion

        #region Node Cleanup Tests

        [TestMethod]
        public void Octree_NodeCleanup_RemovesEmptyNodes()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );

            // Add enough points to trigger subdivision
            var points = new List<Point3D>();
            for (int i = 0; i < 50; i++)
            {
                var point = new Point3D(
                    WorldSize / 2 + i * 0.1f,
                    WorldSize / 2 + i * 0.1f,
                    WorldSize / 2 + i * 0.1f
                );
                points.Add(point);
                octree.Add(point);
            }

            // Record state after adding points
            int nodeCountAfterAdd = octree.NodeCount;
            int depthAfterAdd = octree.Depth;

            // Act - remove all points
            foreach (var point in points)
            {
                octree.Remove(point);
            }

            // Assert
            Assert.IsTrue(octree.NodeCount < nodeCountAfterAdd, "Node count should decrease after removing all points");
            Assert.AreEqual(0, octree.GetAllPoints().Count(), "All points should be removed");
            Assert.AreEqual(1, octree.NodeCount, "Only root node should remain after cleanup");
            Assert.AreEqual(0, octree.Depth, "Depth should be 0 after cleanup");
        }

        [TestMethod]
        public void Octree_NodeCleanup_HandlesPartialRemoval()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );

            // Add points to different parts of the octree
            var pointsGroup1 = new List<Point3D>();
            var pointsGroup2 = new List<Point3D>();

            // Group 1 - upper octant
            for (int i = 0; i < 20; i++)
            {
                var point = new Point3D(
                    3 * WorldSize / 4 + i * 0.1f,
                    3 * WorldSize / 4 + i * 0.1f,
                    3 * WorldSize / 4 + i * 0.1f
                );
                pointsGroup1.Add(point);
                octree.Add(point);
            }

            // Group 2 - lower octant
            for (int i = 0; i < 20; i++)
            {
                var point = new Point3D(
                    WorldSize / 4 + i * 0.1f,
                    WorldSize / 4 + i * 0.1f,
                    WorldSize / 4 + i * 0.1f
                );
                pointsGroup2.Add(point);
                octree.Add(point);
            }

            // Record state after adding all points
            int nodeCountAfterAdd = octree.NodeCount;
            int depthAfterAdd = octree.Depth;

            // Act - remove only group 1 points
            foreach (var point in pointsGroup1)
            {
                octree.Remove(point);
            }

            // Assert
            Assert.IsTrue(octree.NodeCount < nodeCountAfterAdd, "Node count should decrease after removing group 1");
            Assert.AreEqual(pointsGroup2.Count, octree.GetAllPoints().Count(), "Only group 2 points should remain");
            
            // Verify group 2 points are still retrievable
            var allPoints = octree.GetAllPoints().ToList();
            foreach (var point in pointsGroup2)
            {
                Assert.IsTrue(allPoints.Any(p => 
                    Math.Abs(p.X - point.X) < Epsilon && 
                    Math.Abs(p.Y - point.Y) < Epsilon && 
                    Math.Abs(p.Z - point.Z) < Epsilon), 
                    $"Point {point} should still be retrievable from the octree");
            }
        }

        [TestMethod]
        public void Octree_NodeCleanup_HandlesRecursiveCleanup()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );

            // Add points to create a deep tree structure
            // We'll create a line of points that should create a deep, narrow branch
            var points = new List<Point3D>();
            var basePoint = new Point3D(WorldSize / 2, WorldSize / 2, WorldSize / 2);
            
            // Add enough points with small offsets to force deep subdivision
            for (int i = 0; i < 100; i++)
            {
                var point = new Point3D(
                    basePoint.X + i * 0.01f,
                    basePoint.Y + i * 0.01f,
                    basePoint.Z + i * 0.01f
                );
                points.Add(point);
                octree.Add(point);
            }

            // Record state after adding points
            int nodeCountAfterAdd = octree.NodeCount;
            int depthAfterAdd = octree.Depth;
            
            // Act - remove points from deepest to shallowest to test recursive cleanup
            for (int i = points.Count - 1; i >= 0; i--)
            {
                octree.Remove(points[i]);
                
                // Check intermediate state occasionally
                if (i % 10 == 0)
                {
                    Assert.IsTrue(octree.GetAllPoints().Count() == i, 
                        $"Should have {i} points remaining after removing {points.Count - i} points");
                }
            }

            // Assert
            Assert.AreEqual(0, octree.GetAllPoints().Count(), "All points should be removed");
            Assert.AreEqual(1, octree.NodeCount, "Only root node should remain after cleanup");
            Assert.AreEqual(0, octree.Depth, "Depth should be 0 after cleanup");
        }

        #endregion

        #region Boundary and Edge Case Tests

        [TestMethod]
        public void Octree_BoundaryPoints_HandlesCorrectly()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );

            // Create points exactly on the boundaries
            var boundaryPoints = new List<Point3D>
            {
                new Point3D(0, 0, 0),                                   // Min corner
                new Point3D(WorldSize, WorldSize, WorldSize),           // Max corner
                new Point3D(0, WorldSize / 2, WorldSize / 2),           // Min X edge
                new Point3D(WorldSize, WorldSize / 2, WorldSize / 2),   // Max X edge
                new Point3D(WorldSize / 2, 0, WorldSize / 2),           // Min Y edge
                new Point3D(WorldSize / 2, WorldSize, WorldSize / 2),   // Max Y edge
                new Point3D(WorldSize / 2, WorldSize / 2, 0),           // Min Z edge
                new Point3D(WorldSize / 2, WorldSize / 2, WorldSize)    // Max Z edge
            };

            // Act
            foreach (var point in boundaryPoints)
            {
                octree.Add(point);
            }

            // Assert
            Assert.AreEqual(boundaryPoints.Count, octree.GetAllPoints().Count(), "All boundary points should be added");
            
            // Verify each boundary point is retrievable
            var allPoints = octree.GetAllPoints().ToList();
            foreach (var point in boundaryPoints)
            {
                Assert.IsTrue(allPoints.Any(p => 
                    Math.Abs(p.X - point.X) < Epsilon && 
                    Math.Abs(p.Y - point.Y) < Epsilon && 
                    Math.Abs(p.Z - point.Z) < Epsilon), 
                    $"Boundary point {point} should be retrievable from the octree");
            }
        }

        [TestMethod]
        public void Octree_OutOfBoundsPoints_HandlesGracefully()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );

            // Create points outside the boundaries
            var outOfBoundsPoints = new List<Point3D>
            {
                new Point3D(-1, WorldSize / 2, WorldSize / 2),          // Outside min X
                new Point3D(WorldSize + 1, WorldSize / 2, WorldSize / 2), // Outside max X
                new Point3D(WorldSize / 2, -1, WorldSize / 2),          // Outside min Y
                new Point3D(WorldSize / 2, WorldSize + 1, WorldSize / 2), // Outside max Y
                new Point3D(WorldSize / 2, WorldSize / 2, -1),          // Outside min Z
                new Point3D(WorldSize / 2, WorldSize / 2, WorldSize + 1)  // Outside max Z
            };

            // Act & Assert - should not throw exceptions
            foreach (var point in outOfBoundsPoints)
            {
                try
                {
                    octree.Add(point);
                    // If we get here, the point was added (which is acceptable if clamped)
                    // Let's verify it's retrievable
                    var retrievedPoints = octree.GetAllPoints().ToList();
                    Assert.IsTrue(retrievedPoints.Any(p => 
                        Math.Abs(p.X - point.X) < Epsilon && 
                        Math.Abs(p.Y - point.Y) < Epsilon && 
                        Math.Abs(p.Z - point.Z) < Epsilon), 
                        $"Out-of-bounds point {point} should be retrievable if it was added");
                }
                catch (Exception ex)
                {
                    // If an exception is thrown, it should be a specific validation exception, not a crash
                    Assert.IsInstanceOfType(ex, typeof(ArgumentOutOfRangeException), 
                        "If out-of-bounds points are rejected, it should be with a specific validation exception");
                }
            }
        }

        [TestMethod]
        public void Octree_ExtremelyClosePoints_HandlesCorrectly()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );

            // Create a set of extremely close points
            var basePoint = new Point3D(WorldSize / 2, WorldSize / 2, WorldSize / 2);
            var closePoints = new List<Point3D>();
            
            // Points with extremely small differences
            for (int i = 0; i < 20; i++)
            {
                var point = new Point3D(
                    basePoint.X + i * 0.0000001f,
                    basePoint.Y + i * 0.0000001f,
                    basePoint.Z + i * 0.0000001f
                );
                closePoints.Add(point);
            }

            // Act
            foreach (var point in closePoints)
            {
                octree.Add(point);
            }

            // Assert
            Assert.AreEqual(closePoints.Count, octree.GetAllPoints().Count(), "All close points should be added");
            
            // Verify all points are in the octree
            var allPoints = octree.GetAllPoints().ToList();
            foreach (var point in closePoints)
            {
                Assert.IsTrue(allPoints.Any(p => 
                    Math.Abs(p.X - point.X) < Epsilon && 
                    Math.Abs(p.Y - point.Y) < Epsilon && 
                    Math.Abs(p.Z - point.Z) < Epsilon), 
                    $"Close point {point} should be retrievable from the octree");
            }
        }

        [TestMethod]
        public void Octree_ConcurrentAccess_HandlesCorrectly()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );

            int pointCount = 1000;
            var random = new Random(42); // Fixed seed for reproducibility
            var points = new ConcurrentBag<Point3D>();

            // Act - Add points concurrently
            Parallel.For(0, pointCount, i =>
            {
                var point = new Point3D(
                    random.NextSingle() * WorldSize,
                    random.NextSingle() * WorldSize,
                    random.NextSingle() * WorldSize
                );
                points.Add(point);
                octree.Add(point);
            });

            // Assert
            Assert.AreEqual(pointCount, octree.GetAllPoints().Count(), "All points should be added");
            
            // Verify random subset of points are retrievable
            var pointsList = points.ToList();
            var allPoints = octree.GetAllPoints().ToList();
            for (int i = 0; i < 100; i++)
            {
                var testPoint = pointsList[random.Next(pointsList.Count)];
                Assert.IsTrue(allPoints.Any(p => 
                    Math.Abs(p.X - testPoint.X) < Epsilon && 
                    Math.Abs(p.Y - testPoint.Y) < Epsilon && 
                    Math.Abs(p.Z - testPoint.Z) < Epsilon), 
                    $"Point {testPoint} should be retrievable from the octree");
            }
        }

        #endregion

        #region Performance Threshold Tests

        [TestMethod]
        public void Octree_PerformanceThreshold_AddingPoints()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );

            int pointCount = 10000;
            var random = new Random(42); // Fixed seed for reproducibility

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            for (int i = 0; i < pointCount; i++)
            {
                var point = new Point3D(
                    random.NextSingle() * WorldSize,
                    random.NextSingle() * WorldSize,
                    random.NextSingle() * WorldSize
                );
                octree.Add(point);
            }
            
            stopwatch.Stop();
            
            // Assert
            Console.WriteLine($"Time to add {pointCount} points: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"Final octree state: Nodes={octree.NodeCount}, Depth={octree.Depth}");
            
            // Performance threshold - should be able to add 10,000 points in under 1 second
            // This is a conservative threshold that should pass on most systems
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000, 
                $"Adding {pointCount} points took {stopwatch.ElapsedMilliseconds}ms, which exceeds the 1000ms threshold");
            
            // Verify all points were added
            Assert.AreEqual(pointCount, octree.GetAllPoints().Count(), "All points should be added");
        }

        [TestMethod]
        public void Octree_PerformanceThreshold_RetrievingPoints()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );

            int pointCount = 10000;
            var random = new Random(42); // Fixed seed for reproducibility
            var points = new List<Point3D>();

            // Add points
            for (int i = 0; i < pointCount; i++)
            {
                var point = new Point3D(
                    random.NextSingle() * WorldSize,
                    random.NextSingle() * WorldSize,
                    random.NextSingle() * WorldSize
                );
                points.Add(point);
                octree.Add(point);
            }

            // Act - retrieve all points
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            var allPoints = octree.GetAllPoints().ToList();
            
            stopwatch.Stop();
            
            // Assert
            Console.WriteLine($"Time to retrieve {pointCount} points: {stopwatch.ElapsedMilliseconds}ms");
            
            // Performance threshold - should be able to retrieve all points in under 100ms
            // This is a conservative threshold that should pass on most systems
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100, 
                $"Retrieving {pointCount} points took {stopwatch.ElapsedMilliseconds}ms, which exceeds the 100ms threshold");
            
            // Verify all points were retrieved
            Assert.AreEqual(pointCount, allPoints.Count, "All points should be retrieved");
        }

        [TestMethod]
        public void Octree_PerformanceThreshold_RemovingPoints()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );

            int pointCount = 10000;
            var random = new Random(42); // Fixed seed for reproducibility
            var points = new List<Point3D>();

            // Add points
            for (int i = 0; i < pointCount; i++)
            {
                var point = new Point3D(
                    random.NextSingle() * WorldSize,
                    random.NextSingle() * WorldSize,
                    random.NextSingle() * WorldSize
                );
                points.Add(point);
                octree.Add(point);
            }

            // Act - remove points
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            foreach (var point in points)
            {
                octree.Remove(point);
            }
            
            stopwatch.Stop();
            
            // Assert
            Console.WriteLine($"Time to remove {pointCount} points: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"Final octree state: Nodes={octree.NodeCount}, Depth={octree.Depth}");
            
            // Performance threshold - should be able to remove 10,000 points in under 1 second
            // This is a conservative threshold that should pass on most systems
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000, 
                $"Removing {pointCount} points took {stopwatch.ElapsedMilliseconds}ms, which exceeds the 1000ms threshold");
            
            // Verify all points were removed
            Assert.AreEqual(0, octree.GetAllPoints().Count(), "All points should be removed");
            Assert.AreEqual(1, octree.NodeCount, "Only root node should remain after cleanup");
        }

        #endregion
    }
}
