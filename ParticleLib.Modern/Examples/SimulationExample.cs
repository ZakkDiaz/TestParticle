using ParticleLib.Modern.Models;
using ParticleLib.Modern.Rendering;
using System.Numerics;

namespace ParticleLib.Modern.Examples
{
    /// <summary>
    /// Example showing how to run a high-performance particle simulation with rendering support.
    /// </summary>
    public class SimulationExample
    {
        private readonly Octree _octree;
        private readonly ParticlePhysics _physics;
        private readonly IParticleRenderer _renderer;
        private readonly Thread _simulationThread;
        private readonly int _simulationStepsPerSecond;
        private volatile bool _isRunning = false;
        
        /// <summary>
        /// Initializes a new instance of the SimulationExample class.
        /// </summary>
        /// <param name="renderer">The renderer to use for visualization.</param>
        /// <param name="simulationStepsPerSecond">The number of simulation steps to run per second.</param>
        public SimulationExample(IParticleRenderer renderer, int simulationStepsPerSecond = 60)
        {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _simulationStepsPerSecond = simulationStepsPerSecond;
            
            // Create the simulation space
            _octree = new Octree(
                Point3D.Origin,
                new Point3D(1000, 1000, 1000)
            );
            
            // Create the physics system
            _physics = new ParticlePhysics(_octree);
            
            // Configure snapshot interval based on desired rendering frame rate
            // For example, if we want 30 FPS rendering and 60 simulation steps per second,
            // we should take a snapshot every 2 simulation steps
            _physics.SnapshotInterval = Math.Max(1, simulationStepsPerSecond / 30);
            
            // Create the simulation thread
            _simulationThread = new Thread(RunSimulation);
            _simulationThread.IsBackground = true;
        }
        
        /// <summary>
        /// Adds random particles to the simulation.
        /// </summary>
        /// <param name="count">The number of particles to add.</param>
        public void AddRandomParticles(int count)
        {
            var random = new Random();
            var bounds = _octree.Bounds;
            var size = bounds.Max - bounds.Min;
            
            for (int i = 0; i < count; i++)
            {
                // Random position within bounds
                var position = new Point3D(
                    bounds.Min.X + (float)random.NextDouble() * size.X,
                    bounds.Min.Y + (float)random.NextDouble() * size.Y,
                    bounds.Min.Z + (float)random.NextDouble() * size.Z
                );
                
                // Random velocity
                var velocity = new Vector3(
                    (float)(random.NextDouble() * 20 - 10),
                    (float)(random.NextDouble() * 20 - 10),
                    (float)(random.NextDouble() * 20 - 10)
                );
                
                _physics.AddParticle(position, velocity);
            }
        }
        
        /// <summary>
        /// Gets or sets the boundary condition type for the simulation.
        /// </summary>
        public BoundaryConditionType BoundaryCondition
        {
            get => _physics.BoundaryCondition;
            set => _physics.BoundaryCondition = value;
        }
        
        /// <summary>
        /// Gets the latest simulation snapshot for rendering.
        /// </summary>
        public SimulationSnapshot LatestSnapshot => _physics.LatestSnapshot;
        
        /// <summary>
        /// Starts the simulation.
        /// </summary>
        public void Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                _simulationThread.Start();
            }
        }
        
        /// <summary>
        /// Stops the simulation.
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            
            // Wait for the simulation thread to stop
            if (_simulationThread.IsAlive)
            {
                _simulationThread.Join(1000);
            }
        }
        
        /// <summary>
        /// Renders the current state of the simulation.
        /// </summary>
        public void Render()
        {
            // The physics system will use the latest snapshot for rendering
            _physics.Render(_renderer);
        }
        
        /// <summary>
        /// Runs the simulation loop.
        /// </summary>
        private void RunSimulation()
        {
            float deltaTime = 1.0f / _simulationStepsPerSecond;
            long targetTicksPerFrame = TimeSpan.TicksPerSecond / _simulationStepsPerSecond;
            long previousTicks = DateTime.UtcNow.Ticks;
            
            while (_isRunning)
            {
                // Update the simulation
                _physics.Update(deltaTime);
                
                // Calculate how long to sleep to maintain the desired simulation rate
                long currentTicks = DateTime.UtcNow.Ticks;
                long elapsedTicks = currentTicks - previousTicks;
                long sleepTicks = targetTicksPerFrame - elapsedTicks;
                
                if (sleepTicks > 0)
                {
                    int sleepMilliseconds = (int)(sleepTicks / TimeSpan.TicksPerMillisecond);
                    if (sleepMilliseconds > 0)
                    {
                        Thread.Sleep(sleepMilliseconds);
                    }
                }
                
                previousTicks = DateTime.UtcNow.Ticks;
            }
        }
    }
}
