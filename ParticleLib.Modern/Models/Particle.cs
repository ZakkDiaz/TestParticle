using System.Numerics;

namespace ParticleLib.Modern.Models
{
    /// <summary>
    /// Represents a particle in 3D space with position, velocity, and other properties.
    /// </summary>
    public class Particle
    {
        /// <summary>
        /// Gets or sets the unique identifier for this particle.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the position of the particle.
        /// </summary>
        public Point3D Position { get; set; }

        /// <summary>
        /// Gets or sets the velocity of the particle.
        /// </summary>
        public Vector3 Velocity { get; set; }

        /// <summary>
        /// Gets or sets the acceleration of the particle.
        /// </summary>
        public Vector3 Acceleration { get; set; }

        /// <summary>
        /// Gets or sets the mass of the particle.
        /// </summary>
        public float Mass { get; set; }

        /// <summary>
        /// Gets or sets whether the particle is active in the simulation.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Creates a new particle with the specified position.
        /// </summary>
        public Particle(Point3D position)
        {
            Position = position;
            Velocity = Vector3.Zero;
            Acceleration = Vector3.Zero;
            Mass = 1.0f;
        }

        /// <summary>
        /// Creates a new particle with the specified position and velocity.
        /// </summary>
        public Particle(Point3D position, Vector3 velocity)
        {
            Position = position;
            Velocity = velocity;
            Acceleration = Vector3.Zero;
            Mass = 1.0f;
        }

        /// <summary>
        /// Creates a new particle with the specified position, velocity, and mass.
        /// </summary>
        public Particle(Point3D position, Vector3 velocity, float mass)
        {
            Position = position;
            Velocity = velocity;
            Acceleration = Vector3.Zero;
            Mass = mass;
        }

        /// <summary>
        /// Applies a force to the particle, updating its acceleration based on F=ma.
        /// </summary>
        public void ApplyForce(Vector3 force)
        {
            Acceleration += force / Mass;
        }

        /// <summary>
        /// Updates the particle's position and velocity based on its current acceleration.
        /// </summary>
        public void Update(float deltaTime)
        {
            Velocity += Acceleration * deltaTime;
            Position += new Point3D(
                Velocity.X * deltaTime,
                Velocity.Y * deltaTime,
                Velocity.Z * deltaTime
            );
            Acceleration = Vector3.Zero; // Reset acceleration for the next frame
        }

        /// <summary>
        /// Returns a string representation of the particle.
        /// </summary>
        public override string ToString()
        {
            return $"Particle {Id}: Pos={Position}, Vel={Velocity}, Mass={Mass}";
        }
    }
}
