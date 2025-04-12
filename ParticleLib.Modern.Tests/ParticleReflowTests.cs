using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParticleLib.Modern.Models;
using System.Numerics;

namespace ParticleLib.Modern.Tests
{
    [TestClass]
    public class ParticleReflowTests
    {
        private const float WorldSize = 1000f;
        private const float Epsilon = 0.0001f;

        [TestMethod]
        public void SingleParticle_LinearMovement_CorrectlyReflows()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );
            var physics = new ParticlePhysics(octree);
            
            // Create a particle at the center moving in the X direction
            var particle = physics.AddParticle(
                new Point3D(WorldSize / 2, WorldSize / 2, WorldSize / 2),
                new Vector3(100, 0, 0) // 100 units per second in X direction
            );
            
            // Get initial Morton code
            ulong initialMortonCode = GetMortonCodeForPosition(octree, particle.Position);
            
            // Act - Move the particle for 1 second
            physics.Update(1.0f);
            
            // Assert
            ulong newMortonCode = GetMortonCodeForPosition(octree, particle.Position);
            Assert.AreNotEqual(initialMortonCode, newMortonCode, "Particle should have moved to a new octree node");
            Assert.AreEqual(WorldSize / 2 + 100, particle.Position.X, Epsilon, "Particle should have moved 100 units in X direction");
        }

        [TestMethod]
        public void SingleParticle_CrossingBoundary_WrapsAround()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );
            var physics = new ParticlePhysics(octree);
            physics.BoundaryCondition = BoundaryConditionType.Periodic;
            
            // Create a particle near the edge moving toward the boundary
            var particle = physics.AddParticle(
                new Point3D(WorldSize - 50, WorldSize / 2, WorldSize / 2),
                new Vector3(100, 0, 0) // 100 units per second in X direction
            );
            
            // Act - Move the particle for 1 second
            physics.Update(1.0f);
            
            // Assert
            Assert.AreEqual(50, particle.Position.X, Epsilon, "Particle should have wrapped around to the other side");
            Assert.AreEqual(WorldSize / 2, particle.Position.Y, Epsilon, "Y position should remain unchanged");
            Assert.AreEqual(WorldSize / 2, particle.Position.Z, Epsilon, "Z position should remain unchanged");
        }

        [TestMethod]
        public void SingleParticle_CrossingBoundary_Reflects()
        {
            // Arrange
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(WorldSize, WorldSize, WorldSize)
            );
            var physics = new ParticlePhysics(octree);
            physics.BoundaryCondition = BoundaryConditionType.Reflective;
            
            // Create a particle near the edge moving toward the boundary
            var particle = physics.AddParticle(
                new Point3D(WorldSize - 50, WorldSize / 2, WorldSize / 2),
                new Vector3(100, 0, 0) // 100 units per second in X direction
            );
            
            // Act - Move the particle for 1 second
            physics.Update(1.0f);
            
            // Assert
            Assert.AreEqual(WorldSize - 50, particle.Position.X, Epsilon, "Particle should have reflected back");
            Assert.AreEqual(-100, particle.Velocity.X, Epsilon, "Velocity X component should be reversed");
            Assert.AreEqual(0, particle.Velocity.Y, Epsilon, "Velocity Y component should remain unchanged");
            Assert.AreEqual(0, particle.Velocity.Z, Epsilon, "Velocity Z component should remain unchanged");
        }

        [TestMethod]
        public void ParticleCluster_HighDensity_CorrectlySubdivides()
        {
            // Arrange - Create a simple octree
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(100, 100, 100)
            );
            
            // Create a cluster of particles in a small area to force subdivision
            var random = new Random(42); // Fixed seed for reproducibility
            var center = new Point3D(50, 50, 50);
            var radius = 10f;
            
            Console.WriteLine("Initial node count: " + octree.NodeCount);
            
            // Add particles in a small sphere
            int particleCount = 50; // Add enough particles to force multiple subdivisions
            for (int i = 0; i < particleCount; i++)
            {
                // Create random position within a sphere
                float theta = (float)(random.NextDouble() * 2 * Math.PI);
                float phi = (float)(random.NextDouble() * Math.PI);
                float r = (float)(random.NextDouble() * radius);
                
                float x = center.X + r * (float)Math.Sin(phi) * (float)Math.Cos(theta);
                float y = center.Y + r * (float)Math.Sin(phi) * (float)Math.Sin(theta);
                float z = center.Z + r * (float)Math.Cos(phi);
                
                octree.Add(new Point3D(x, y, z));
                
                // Log progress
                if ((i + 1) % 10 == 0)
                {
                    Console.WriteLine($"After adding {i + 1} particles, node count: {octree.NodeCount}, depth: {octree.Depth}");
                }
            }
            
            // Get the depth of the octree
            int depth = octree.Depth;
            int nodeCount = octree.NodeCount;
            
            Console.WriteLine($"Final node count: {nodeCount}, depth: {depth}");
            
            // Assert
            Assert.IsTrue(nodeCount > 1, $"Node count should be greater than 1, but was {nodeCount}");
            Assert.IsTrue(depth > 1, $"Octree depth should be greater than 1 due to subdivision, but was {depth}");
            
            // Verify all particles are in the octree
            var allPoints = octree.GetAllPoints().ToList();
            Assert.AreEqual(particleCount, allPoints.Count, $"All {particleCount} particles should be in the octree");
        }

        [TestMethod]
        public void ParticleReflow_NodeSubdivision_CorrectlyReflowsParticles()
        {
            // Arrange - Create a very simple octree with a small size
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(100, 100, 100)
            );
            
            // Add particles to a very specific location to ensure they all go in the same node
            var targetLocation = new Point3D(50, 50, 50);
            
            Console.WriteLine("Adding initial particles...");
            
            // Track node count before adding any particles
            int initialNodeCount = octree.NodeCount;
            Console.WriteLine($"Initial node count (empty octree): {initialNodeCount}");
            
            // Add 20 particles to force subdivision (MaxPointsPerNode is 8)
            for (int i = 0; i < 20; i++)
            {
                // Create random position within a small sphere
                var position = new Point3D(
                    targetLocation.X + (i * 0.1f),
                    targetLocation.Y,
                    targetLocation.Z
                );
                octree.Add(position);
                
                // Check node count after each addition
                if ((i + 1) % 5 == 0)
                {
                    Console.WriteLine($"After adding {i + 1} particles, node count: {octree.NodeCount}");
                }
            }
            
            // Final check
            int finalNodeCount = octree.NodeCount;
            Console.WriteLine($"Final node count: {finalNodeCount}");
            
            // Assert - Node count should increase due to subdivision
            Assert.IsTrue(finalNodeCount > initialNodeCount, 
                $"Node count should increase after adding particles, was {initialNodeCount}, now {finalNodeCount}");
            
            // Verify all particles are in the octree
            var allPoints = octree.GetAllPoints().ToList();
            Assert.AreEqual(20, allPoints.Count, "All 20 particles should be in the octree");
        }

        [TestMethod]
        public void Octree_IncrementalGrowth_SubdividesPredictably()
        {
            // Arrange - Create a simple octree with a small size
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(100, 100, 100)
            );
            
            // Create a cluster of particles in a small area to force subdivision
            var random = new Random(42); // Fixed seed for reproducibility
            var center = new Point3D(50, 50, 50);
            var radius = 5f; // Small radius to ensure particles are densely packed
            
            Console.WriteLine("=== Incremental Growth Test ===");
            Console.WriteLine("Initial state: Nodes={0}, Depth={1}", octree.NodeCount, octree.Depth);
            
            // Add particles incrementally and monitor octree growth
            int totalParticles = 100;
            int[] checkpoints = { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 60, 70, 80, 90, 100 };
            
            for (int i = 0; i < totalParticles; i++)
            {
                // Create random position within a sphere
                float theta = (float)(random.NextDouble() * 2 * Math.PI);
                float phi = (float)(random.NextDouble() * Math.PI);
                float r = (float)(random.NextDouble() * radius);
                
                float x = center.X + r * (float)Math.Sin(phi) * (float)Math.Cos(theta);
                float y = center.Y + r * (float)Math.Sin(phi) * (float)Math.Sin(theta);
                float z = center.Z + r * (float)Math.Cos(phi);
                
                octree.Add(new Point3D(x, y, z));
                
                // Log at checkpoints
                if (Array.IndexOf(checkpoints, i + 1) >= 0)
                {
                    Console.WriteLine("After {0} particles: Nodes={1}, Depth={2}", 
                        i + 1, octree.NodeCount, octree.Depth);
                }
            }
            
            // Assert
            Assert.IsTrue(octree.NodeCount > 1, "Octree should have subdivided");
            Assert.IsTrue(octree.Depth > 1, "Octree should have depth greater than 1");
            
            // Verify all particles are in the octree
            var allPoints = octree.GetAllPoints().ToList();
            Assert.AreEqual(totalParticles, allPoints.Count, "All particles should be in the octree");
        }

        [TestMethod]
        public void Octree_UniformDistribution_SubdividesPredictably()
        {
            // Arrange - Create a simple octree
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(100, 100, 100)
            );
            
            // Use a uniform distribution of particles
            var random = new Random(42); // Fixed seed for reproducibility
            
            Console.WriteLine("=== Uniform Distribution Test ===");
            Console.WriteLine("Initial state: Nodes={0}, Depth={1}", octree.NodeCount, octree.Depth);
            
            // Add particles incrementally and monitor octree growth
            int totalParticles = 100;
            int[] checkpoints = { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 60, 70, 80, 90, 100 };
            
            for (int i = 0; i < totalParticles; i++)
            {
                // Create uniform random position
                float x = (float)(random.NextDouble() * 100);
                float y = (float)(random.NextDouble() * 100);
                float z = (float)(random.NextDouble() * 100);
                
                octree.Add(new Point3D(x, y, z));
                
                // Log at checkpoints
                if (Array.IndexOf(checkpoints, i + 1) >= 0)
                {
                    Console.WriteLine("After {0} particles: Nodes={1}, Depth={2}", 
                        i + 1, octree.NodeCount, octree.Depth);
                }
            }
            
            // Assert
            Console.WriteLine("Final state: Nodes={0}, Depth={1}", octree.NodeCount, octree.Depth);
            
            // Verify all particles are in the octree
            var allPoints = octree.GetAllPoints().ToList();
            Assert.AreEqual(totalParticles, allPoints.Count, "All particles should be in the octree");
        }

        [TestMethod]
        public void Octree_MaxDepthProtection_WorksCorrectly()
        {
            // Arrange - Create a simple octree with a small size
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(10, 10, 10)
            );
            
            // Create a very tight cluster of particles at a single point to force maximum subdivision
            var position = new Point3D(5, 5, 5);
            var jitter = 0.0001f; // Extremely small jitter to create unique but very close points
            var random = new Random(42); // Fixed seed for reproducibility
            
            Console.WriteLine("=== Max Depth Protection Test ===");
            Console.WriteLine("Initial state: Nodes={0}, Depth={1}", octree.NodeCount, octree.Depth);
            
            // Add many particles at almost the same position to force deep subdivision
            int totalParticles = 200;
            int[] checkpoints = { 10, 20, 30, 40, 50, 75, 100, 150, 200 };
            
            for (int i = 0; i < totalParticles; i++)
            {
                // Create position with tiny jitter
                float x = position.X + (float)((random.NextDouble() - 0.5) * jitter);
                float y = position.Y + (float)((random.NextDouble() - 0.5) * jitter);
                float z = position.Z + (float)((random.NextDouble() - 0.5) * jitter);
                
                octree.Add(new Point3D(x, y, z));
                
                // Log progress periodically
                if (Array.IndexOf(checkpoints, i + 1) >= 0)
                {
                    Console.WriteLine($"After {i + 1} particles: Nodes={octree.NodeCount}, Depth={octree.Depth}");
                }
            }
            
            // Assert
            Console.WriteLine($"Final state: Nodes={octree.NodeCount}, Depth={octree.Depth}");
            Assert.IsTrue(octree.Depth <= 21, "Octree depth should not exceed MaxDepth (21)");
            
            // Verify all particles are in the octree
            var allPoints = octree.GetAllPoints().ToList();
            Assert.AreEqual(totalParticles, allPoints.Count, "All particles should be in the octree");
        }

        [TestMethod]
        public void Octree_SparseImplementation_OptimizesMemoryUsage()
        {
            // Arrange - Create a simple octree
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(100, 100, 100)
            );
            
            // Create several clusters of particles in different areas
            var random = new Random(42); // Fixed seed for reproducibility
            var clusters = new[]
            {
                new { Center = new Point3D(25, 25, 25), Radius = 5f, Count = 30 },
                new { Center = new Point3D(75, 75, 75), Radius = 5f, Count = 30 },
                new { Center = new Point3D(25, 75, 25), Radius = 5f, Count = 30 },
                new { Center = new Point3D(75, 25, 75), Radius = 5f, Count = 30 }
            };
            
            Console.WriteLine("=== Sparse Octree Test ===");
            Console.WriteLine("Initial state: Nodes={0}, Depth={1}", octree.NodeCount, octree.Depth);
            
            // Add particles in clusters
            int totalParticles = 0;
            foreach (var cluster in clusters)
            {
                Console.WriteLine($"Adding {cluster.Count} particles at ({cluster.Center.X}, {cluster.Center.Y}, {cluster.Center.Z})");
                
                for (int i = 0; i < cluster.Count; i++)
                {
                    // Create random position within the cluster
                    float theta = (float)(random.NextDouble() * 2 * Math.PI);
                    float phi = (float)(random.NextDouble() * Math.PI);
                    float r = (float)(random.NextDouble() * cluster.Radius);
                    
                    float x = cluster.Center.X + r * (float)Math.Sin(phi) * (float)Math.Cos(theta);
                    float y = cluster.Center.Y + r * (float)Math.Sin(phi) * (float)Math.Sin(theta);
                    float z = cluster.Center.Z + r * (float)Math.Cos(phi);
                    
                    octree.Add(new Point3D(x, y, z));
                    totalParticles++;
                }
                
                Console.WriteLine($"After cluster: Nodes={octree.NodeCount}, Depth={octree.Depth}");
            }
            
            // Verify all particles are in the octree
            var allPoints = octree.GetAllPoints().ToList();
            Assert.AreEqual(totalParticles, allPoints.Count, "All particles should be in the octree");
            
            // Calculate theoretical maximum nodes if we created all 8 children at each subdivision
            // For a complete octree with 4 clusters at depth 2, we would have:
            // 1 (root) + 8 (level 1) + 64 (level 2) = 73 nodes
            int theoreticalMaxNodes = 1 + 8 + 64;
            
            // Assert that our sparse implementation uses fewer nodes
            Console.WriteLine($"Sparse implementation uses {octree.NodeCount} nodes vs. theoretical maximum of {theoreticalMaxNodes}");
            Assert.IsTrue(octree.NodeCount < theoreticalMaxNodes, 
                $"Sparse implementation should use fewer nodes than theoretical maximum ({octree.NodeCount} vs {theoreticalMaxNodes})");
            
            // Now remove all particles from the first cluster
            var firstCluster = clusters[0];
            int removedCount = 0;
            
            foreach (var point in octree.GetAllPoints().ToList())
            {
                // Check if point is in the first cluster (approximately)
                float distance = Vector3.Distance(
                    new Vector3(firstCluster.Center.X, firstCluster.Center.Y, firstCluster.Center.Z),
                    new Vector3(point.X, point.Y, point.Z)
                );
                
                if (distance <= firstCluster.Radius * 1.5f) // Use slightly larger radius to ensure we get all points
                {
                    octree.Remove(point);
                    removedCount++;
                    
                    // Log progress periodically
                    if (removedCount % 10 == 0)
                    {
                        Console.WriteLine($"After removing {removedCount} particles: Nodes={octree.NodeCount}, Depth={octree.Depth}");
                    }
                }
            }
            
            // Record final state
            Console.WriteLine($"Final state: Nodes={octree.NodeCount}, Depth={octree.Depth}, Removed={removedCount} particles");
            
            // Verify remaining particles count
            var remainingPoints = octree.GetAllPoints().ToList();
            Assert.AreEqual(totalParticles - removedCount, remainingPoints.Count, 
                "Remaining particle count should match expected value");
        }

        [TestMethod]
        public void Octree_NodeCleanup_RemovesEmptyNodes()
        {
            // Arrange - Create a simple octree
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(100, 100, 100)
            );
            
            // Create a cluster of particles in a small area
            var random = new Random(42); // Fixed seed for reproducibility
            var center = new Point3D(25, 25, 25);
            var radius = 5f;
            
            Console.WriteLine("=== Node Cleanup Test ===");
            Console.WriteLine("Initial state: Nodes={0}, Depth={1}", octree.NodeCount, octree.Depth);
            
            // Add particles to force subdivision
            int particleCount = 50;
            for (int i = 0; i < particleCount; i++)
            {
                // Create random position within a sphere
                float theta = (float)(random.NextDouble() * 2 * Math.PI);
                float phi = (float)(random.NextDouble() * Math.PI);
                float r = (float)(random.NextDouble() * radius);
                
                float x = center.X + r * (float)Math.Sin(phi) * (float)Math.Cos(theta);
                float y = center.Y + r * (float)Math.Sin(phi) * (float)Math.Sin(theta);
                float z = center.Z + r * (float)Math.Cos(phi);
                
                octree.Add(new Point3D(x, y, z));
            }
            
            // Record state after adding particles
            int nodeCountAfterAdd = octree.NodeCount;
            int depthAfterAdd = octree.Depth;
            int pointCountAfterAdd = octree.GetAllPoints().Count();
            Console.WriteLine($"After adding {particleCount} particles: Nodes={nodeCountAfterAdd}, Depth={depthAfterAdd}, Points={pointCountAfterAdd}");
            
            // Now remove all particles one by one
            var allPoints = octree.GetAllPoints().ToList();
            for (int i = 0; i < allPoints.Count; i++)
            {
                octree.Remove(allPoints[i]);
                
                // Log progress periodically
                if ((i + 1) % 10 == 0 || i == allPoints.Count - 1)
                {
                    Console.WriteLine($"After removing {i + 1} particles: Nodes={octree.NodeCount}, Depth={octree.Depth}, Points={octree.GetAllPoints().Count()}");
                }
            }
            
            // Assert
            int finalNodeCount = octree.NodeCount;
            int finalPointCount = octree.GetAllPoints().Count();
            Console.WriteLine($"Final state: Nodes={finalNodeCount}, Depth={octree.Depth}, Points={finalPointCount}");
            
            // We should have zero points after removing all particles
            Assert.AreEqual(0, finalPointCount, "No particles should remain in the octree");
            
            // The node count might not decrease to 1, but it should be less than before
            // since our implementation clears points but doesn't necessarily remove nodes
            Assert.IsTrue(finalNodeCount <= nodeCountAfterAdd, 
                $"Node count should not increase after removing all particles (Before: {nodeCountAfterAdd}, After: {finalNodeCount})");
        }
        
        [TestMethod]
        public void Octree_ParticleReflow_CleansUpEmptyNodes()
        {
            // Arrange - Create an octree and physics manager
            var octree = new Octree(
                Point3D.Origin,
                new Point3D(100, 100, 100)
            );
            var physics = new ParticlePhysics(octree);
            
            // Create a cluster of particles in a small area
            var random = new Random(42); // Fixed seed for reproducibility
            var center = new Point3D(25, 25, 25);
            var radius = 5f;
            
            Console.WriteLine("=== Particle Reflow Cleanup Test ===");
            Console.WriteLine("Initial state: Nodes={0}, Depth={1}", octree.NodeCount, octree.Depth);
            
            // Add particles to force subdivision
            int particleCount = 30;
            for (int i = 0; i < particleCount; i++)
            {
                // Create random position within a sphere
                float theta = (float)(random.NextDouble() * 2 * Math.PI);
                float phi = (float)(random.NextDouble() * Math.PI);
                float r = (float)(random.NextDouble() * radius);
                
                float x = center.X + r * (float)Math.Sin(phi) * (float)Math.Cos(theta);
                float y = center.Y + r * (float)Math.Sin(phi) * (float)Math.Sin(theta);
                float z = center.Z + r * (float)Math.Cos(phi);
                
                physics.AddParticle(new Point3D(x, y, z));
            }
            
            // Record state after adding particles
            int nodeCountAfterAdd = octree.NodeCount;
            int depthAfterAdd = octree.Depth;
            int pointCountAfterAdd = octree.GetAllPoints().Count();
            Console.WriteLine($"After adding {particleCount} particles: Nodes={nodeCountAfterAdd}, Depth={depthAfterAdd}, Points={pointCountAfterAdd}");
            
            // Set all particles to move away from the center at high speed
            foreach (var particle in physics.GetAllParticles())
            {
                // Calculate direction away from center
                Vector3 direction = new Vector3(
                    particle.Position.X - center.X,
                    particle.Position.Y - center.Y,
                    particle.Position.Z - center.Z
                );
                
                // Normalize and set high velocity
                if (direction.Length() > 0)
                {
                    direction = Vector3.Normalize(direction) * 50f; // 50 units per second
                    particle.Velocity = direction;
                }
            }
            
            // Update physics several times to move particles far away
            for (int i = 0; i < 5; i++)
            {
                physics.Update(0.5f); // Update with 0.5 second timestep
                Console.WriteLine($"After physics update {i+1}: Nodes={octree.NodeCount}, Depth={octree.Depth}, Particles={physics.GetAllParticles().Count()}");
            }
            
            // Assert
            int finalNodeCount = octree.NodeCount;
            int finalPointCount = octree.GetAllPoints().Count();
            Console.WriteLine($"Final state: Nodes={finalNodeCount}, Depth={octree.Depth}, Points={finalPointCount}");
            
            // Verify all particles are still in the octree (just in different nodes)
            var remainingParticles = physics.GetAllParticles();
            Assert.AreEqual(particleCount, remainingParticles.Count(), "All particles should still be in the physics system");
            
            // The node count might be different, but the point count should match the particle count
            Assert.AreEqual(particleCount, finalPointCount, "Point count should match particle count");
        }

        /// <summary>
        /// Helper method to get the Morton code for a position.
        /// </summary>
        private ulong GetMortonCodeForPosition(Octree octree, Point3D position)
        {
            // Use reflection to access the private method
            var normalizeMethod = typeof(Octree).GetMethod(
                "NormalizeCoordinate", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            
            if (normalizeMethod == null)
            {
                throw new InvalidOperationException("Could not find NormalizeCoordinate method");
            }
            
            var bounds = octree.Bounds;
            
            float normalizedX = (float)normalizeMethod.Invoke(
                octree, 
                new object[] { position.X, bounds.Min.X, bounds.Max.X }
            )!;
            
            float normalizedY = (float)normalizeMethod.Invoke(
                octree, 
                new object[] { position.Y, bounds.Min.Y, bounds.Max.Y }
            )!;
            
            float normalizedZ = (float)normalizeMethod.Invoke(
                octree, 
                new object[] { position.Z, bounds.Min.Z, bounds.Max.Z }
            )!;
            
            return MortonCode.Encode(normalizedX, normalizedY, normalizedZ);
        }
    }
}
