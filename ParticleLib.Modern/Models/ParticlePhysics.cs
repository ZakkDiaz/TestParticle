using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using ParticleLib.Modern.Rendering;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ParticleLib.Modern.Models
{
    /// <summary>
    /// Boundary condition types for the particle simulation.
    /// </summary>
    public enum BoundaryConditionType
    {
        /// <summary>
        /// Particles wrap around to the opposite side when they cross a boundary.
        /// </summary>
        Periodic,
        
        /// <summary>
        /// Particles bounce off boundaries, reversing their velocity component.
        /// </summary>
        Reflective,
        
        /// <summary>
        /// Particles are removed when they cross a boundary.
        /// </summary>
        Open
    }
    
    /// <summary>
    /// Manages physics calculations and updates for particles within an octree.
    /// </summary>
    public class ParticlePhysics
    {
        private readonly Octree _octree;
        private readonly List<Particle> _particles = new();
        private readonly object _particleLock = new();
        private int _nextParticleId = 0;
        
        // For rendering support
        private SimulationSnapshot _latestSnapshot;
        private readonly object _snapshotLock = new();
        private bool _snapshotRequested = false;
        private int _snapshotInterval = 1; // Take snapshot every N updates
        private int _updateCounter = 0;
        
        // Performance optimization flags
        private bool _isUpdating = false;
        private bool _isTakingSnapshot = false;
        
        /// <summary>
        /// Gets or sets the boundary condition type for the simulation.
        /// </summary>
        public BoundaryConditionType BoundaryCondition { get; set; } = BoundaryConditionType.Periodic;
        
        /// <summary>
        /// Gets or sets the interval (in update cycles) at which snapshots are taken.
        /// Setting to 0 disables automatic snapshots.
        /// </summary>
        public int SnapshotInterval
        {
            get => _snapshotInterval;
            set => _snapshotInterval = Math.Max(0, value);
        }
        
        /// <summary>
        /// Gets the latest simulation snapshot for rendering.
        /// </summary>
        public SimulationSnapshot LatestSnapshot
        {
            get
            {
                lock (_snapshotLock)
                {
                    return _latestSnapshot;
                }
            }
        }
        
        /// <summary>
        /// Creates a new particle physics manager for the specified octree.
        /// </summary>
        public ParticlePhysics(Octree octree)
        {
            _octree = octree;
            _latestSnapshot = new SimulationSnapshot(Array.Empty<Particle>(), Array.Empty<OctreeNodeInfo>());
        }
        
        /// <summary>
        /// Adds a particle to the simulation.
        /// </summary>
        public Particle AddParticle(Point3D position, Vector3 velocity = default, float mass = 1.0f)
        {
            // Create the particle
            var particle = new Particle(position, velocity, mass)
            {
                Id = Interlocked.Increment(ref _nextParticleId) - 1
            };
            
            try
            {
                // Add the position to the octree first to ensure it's valid
                _octree.Add(particle.Position);
                
                // If successful, add the particle to the list
                lock (_particleLock)
                {
                    _particles.Add(particle);
                }
                
                return particle;
            }
            catch (ArgumentOutOfRangeException)
            {
                // If the position is outside the octree bounds, don't add the particle
                System.Diagnostics.Debug.WriteLine($"Failed to add particle at position {position} - outside octree bounds");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding particle: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Adds multiple particles at once to improve performance.
        /// </summary>
        public void AddParticles(IEnumerable<(Point3D position, Vector3 velocity, float mass)> particleData)
        {
            List<Particle> newParticles = new List<Particle>();
            
            foreach (var (position, velocity, mass) in particleData)
            {
                try
                {
                    // Create the particle
                    var particle = new Particle(position, velocity, mass)
                    {
                        Id = Interlocked.Increment(ref _nextParticleId) - 1
                    };
                    
                    // Add the position to the octree
                    _octree.Add(particle.Position);
                    
                    // Add to our temporary list
                    newParticles.Add(particle);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error adding particle: {ex.Message}");
                }
            }
            
            // Add all successful particles to the main list
            if (newParticles.Count > 0)
            {
                lock (_particleLock)
                {
                    _particles.AddRange(newParticles);
                }
            }
        }
        
        /// <summary>
        /// Updates the physics simulation by the specified time step.
        /// </summary>
        public void Update(float deltaTime)
        {
            // Prevent concurrent updates or adding particles during update
            if (_isUpdating)
            {
                return;
            }
            
            _isUpdating = true;
            
            try
            {
                List<Particle> particles;
                
                // Create a thread-safe copy of the particles
                lock (_particleLock)
                {
                    particles = _particles.ToList();
                }
                
                // Track nodes that had particles removed for cleanup
                var nodesToCheck = new HashSet<Point3D>();
                var particlesToRemove = new List<Particle>();
                
                // Update each particle's position based on its velocity and acceleration
                foreach (var particle in particles)
                {
                    // Store the old position for octree update
                    var oldPosition = particle.Position;
                    
                    // Update velocity based on acceleration
                    particle.Velocity += particle.Acceleration * deltaTime;
                    
                    // Update position based on velocity
                    var newPosition = new Point3D(
                        oldPosition.X + particle.Velocity.X * deltaTime,
                        oldPosition.Y + particle.Velocity.Y * deltaTime,
                        oldPosition.Z + particle.Velocity.Z * deltaTime
                    );
                    
                    // Apply boundary conditions
                    newPosition = ApplyBoundaryConditions(particle, newPosition);
                    
                    // If the particle is no longer active, mark it for removal
                    if (!particle.IsActive)
                    {
                        particlesToRemove.Add(particle);
                        continue;
                    }
                    
                    // If the position has changed, update the octree
                    if (newPosition != oldPosition)
                    {
                        // Track the old position for potential node cleanup
                        nodesToCheck.Add(oldPosition);
                        
                        // Remove from old position
                        _octree.Remove(oldPosition);
                        
                        // Update the particle's position
                        particle.Position = newPosition;
                        
                        // Add to the octree at the new position
                        try
                        {
                            _octree.Add(newPosition);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            // If the particle is outside the octree bounds and we're using open boundaries,
                            // mark it for removal
                            if (BoundaryCondition == BoundaryConditionType.Open)
                            {
                                particlesToRemove.Add(particle);
                            }
                            else
                            {
                                // This shouldn't happen with other boundary types, but just in case
                                // Add it back at the old position
                                _octree.Add(oldPosition);
                                particle.Position = oldPosition;
                            }
                        }
                    }
                }
                
                // Remove inactive particles in a thread-safe manner
                if (particlesToRemove.Count > 0)
                {
                    lock (_particleLock)
                    {
                        foreach (var particle in particlesToRemove)
                        {
                            _particles.Remove(particle);
                        }
                    }
                }
                
                // Check if we need to take a snapshot for rendering
                _updateCounter++;
                if (_snapshotInterval > 0 && (_updateCounter % _snapshotInterval == 0 || _snapshotRequested))
                {
                    // Take snapshot asynchronously to avoid blocking the update
                    Task.Run(() => TakeSnapshot());
                    _snapshotRequested = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during physics update: {ex.Message}");
            }
            finally
            {
                _isUpdating = false;
            }
        }
        
        /// <summary>
        /// Applies boundary conditions to a particle's position.
        /// </summary>
        private Point3D ApplyBoundaryConditions(Particle particle, Point3D position)
        {
            var bounds = _octree.Bounds;
            
            // Handle X boundaries
            if (position.X < bounds.Min.X)
            {
                switch (BoundaryCondition)
                {
                    case BoundaryConditionType.Periodic:
                        position = new Point3D(
                            bounds.Max.X - (bounds.Min.X - position.X),
                            position.Y,
                            position.Z
                        );
                        break;
                    case BoundaryConditionType.Reflective:
                        position = new Point3D(
                            bounds.Min.X + (bounds.Min.X - position.X),
                            position.Y,
                            position.Z
                        );
                        particle.Velocity = new Vector3(
                            -particle.Velocity.X,
                            particle.Velocity.Y,
                            particle.Velocity.Z
                        );
                        break;
                    case BoundaryConditionType.Open:
                        particle.IsActive = false;
                        return position;
                }
            }
            else if (position.X > bounds.Max.X)
            {
                switch (BoundaryCondition)
                {
                    case BoundaryConditionType.Periodic:
                        position = new Point3D(
                            bounds.Min.X + (position.X - bounds.Max.X),
                            position.Y,
                            position.Z
                        );
                        break;
                    case BoundaryConditionType.Reflective:
                        position = new Point3D(
                            bounds.Max.X - (position.X - bounds.Max.X),
                            position.Y,
                            position.Z
                        );
                        particle.Velocity = new Vector3(
                            -particle.Velocity.X,
                            particle.Velocity.Y,
                            particle.Velocity.Z
                        );
                        break;
                    case BoundaryConditionType.Open:
                        particle.IsActive = false;
                        return position;
                }
            }
            
            // Handle Y boundaries
            if (position.Y < bounds.Min.Y)
            {
                switch (BoundaryCondition)
                {
                    case BoundaryConditionType.Periodic:
                        position = new Point3D(
                            position.X,
                            bounds.Max.Y - (bounds.Min.Y - position.Y),
                            position.Z
                        );
                        break;
                    case BoundaryConditionType.Reflective:
                        position = new Point3D(
                            position.X,
                            bounds.Min.Y + (bounds.Min.Y - position.Y),
                            position.Z
                        );
                        particle.Velocity = new Vector3(
                            particle.Velocity.X,
                            -particle.Velocity.Y,
                            particle.Velocity.Z
                        );
                        break;
                    case BoundaryConditionType.Open:
                        particle.IsActive = false;
                        return position;
                }
            }
            else if (position.Y > bounds.Max.Y)
            {
                switch (BoundaryCondition)
                {
                    case BoundaryConditionType.Periodic:
                        position = new Point3D(
                            position.X,
                            bounds.Min.Y + (position.Y - bounds.Max.Y),
                            position.Z
                        );
                        break;
                    case BoundaryConditionType.Reflective:
                        position = new Point3D(
                            position.X,
                            bounds.Max.Y - (position.Y - bounds.Max.Y),
                            position.Z
                        );
                        particle.Velocity = new Vector3(
                            particle.Velocity.X,
                            -particle.Velocity.Y,
                            particle.Velocity.Z
                        );
                        break;
                    case BoundaryConditionType.Open:
                        particle.IsActive = false;
                        return position;
                }
            }
            
            // Handle Z boundaries
            if (position.Z < bounds.Min.Z)
            {
                switch (BoundaryCondition)
                {
                    case BoundaryConditionType.Periodic:
                        position = new Point3D(
                            position.X,
                            position.Y,
                            bounds.Max.Z - (bounds.Min.Z - position.Z)
                        );
                        break;
                    case BoundaryConditionType.Reflective:
                        position = new Point3D(
                            position.X,
                            position.Y,
                            bounds.Min.Z + (bounds.Min.Z - position.Z)
                        );
                        particle.Velocity = new Vector3(
                            particle.Velocity.X,
                            particle.Velocity.Y,
                            -particle.Velocity.Z
                        );
                        break;
                    case BoundaryConditionType.Open:
                        particle.IsActive = false;
                        return position;
                }
            }
            else if (position.Z > bounds.Max.Z)
            {
                switch (BoundaryCondition)
                {
                    case BoundaryConditionType.Periodic:
                        position = new Point3D(
                            position.X,
                            position.Y,
                            bounds.Min.Z + (position.Z - bounds.Max.Z)
                        );
                        break;
                    case BoundaryConditionType.Reflective:
                        position = new Point3D(
                            position.X,
                            position.Y,
                            bounds.Max.Z - (position.Z - bounds.Max.Z)
                        );
                        particle.Velocity = new Vector3(
                            particle.Velocity.X,
                            particle.Velocity.Y,
                            -particle.Velocity.Z
                        );
                        break;
                    case BoundaryConditionType.Open:
                        particle.IsActive = false;
                        return position;
                }
            }
            
            return position;
        }
        
        /// <summary>
        /// Gets all particles in the simulation.
        /// </summary>
        public IReadOnlyList<Particle> GetAllParticles()
        {
            lock (_particleLock)
            {
                return _particles.AsReadOnly();
            }
        }
        
        /// <summary>
        /// Requests a snapshot to be taken on the next update cycle, regardless of the snapshot interval.
        /// </summary>
        public void RequestSnapshot()
        {
            _snapshotRequested = true;
        }
        
        /// <summary>
        /// Takes a snapshot of the current simulation state for rendering.
        /// </summary>
        private void TakeSnapshot()
        {
            // Prevent concurrent snapshots
            if (_isTakingSnapshot)
            {
                return;
            }
            
            _isTakingSnapshot = true;
            
            try
            {
                List<Particle> particlesCopy;
                
                // Create a thread-safe copy of the particles
                lock (_particleLock)
                {
                    particlesCopy = _particles.ToList();
                }
                
                // Get octree node information
                var nodeInfos = new List<OctreeNodeInfo>();
                
                // Extract node information from the octree
                foreach (var nodeEntry in _octree.GetNodeInfo())
                {
                    bool hasParticles = _octree.HasPointsAtNode(nodeEntry.MortonCode);
                    nodeInfos.Add(new OctreeNodeInfo(
                        nodeEntry.BoundingBox,
                        nodeEntry.Depth,
                        hasParticles,
                        nodeEntry.MortonCode
                    ));
                }
                
                // Create a new snapshot
                var snapshot = new SimulationSnapshot(particlesCopy, nodeInfos);
                
                // Update the latest snapshot
                lock (_snapshotLock)
                {
                    _latestSnapshot = snapshot;
                }
            }
            catch (Exception ex)
            {
                // If we encounter an error during snapshot creation, log it
                System.Diagnostics.Debug.WriteLine($"Error taking snapshot: {ex.Message}");
            }
            finally
            {
                _isTakingSnapshot = false;
            }
        }
        
        /// <summary>
        /// Renders the current simulation state using the provided renderer.
        /// </summary>
        public void Render(IParticleRenderer renderer)
        {
            // Get the latest snapshot
            SimulationSnapshot snapshot;
            lock (_snapshotLock)
            {
                snapshot = _latestSnapshot;
            }
            
            // Begin rendering
            renderer.BeginFrame();
            
            // Render octree nodes
            foreach (var node in snapshot.Nodes)
            {
                renderer.RenderOctreeNode(node.BoundingBox, node.Depth, node.HasParticles);
            }
            
            // Render particles
            foreach (var particle in snapshot.Particles)
            {
                renderer.RenderParticle(particle.Position, particle.Velocity);
            }
            
            // End rendering
            renderer.EndFrame();
        }
    }
}
