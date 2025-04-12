using ParticleLib.Modern.Examples;
using ParticleLib.Modern.Models;
using ParticleLib.Modern.Rendering;
using System.Numerics;

namespace ParticleSimDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Particle Physics Simulation Demo");
            Console.WriteLine("===============================");
            Console.WriteLine("Initializing simulation...");
            
            // Create the renderer
            var renderer = new ConsoleParticleRenderer();
            
            // Create the simulation with 120 physics updates per second
            var simulation = new SimulationExample(renderer, 120);
            
            // Add some particles
            Console.WriteLine("Adding particles...");
            simulation.AddRandomParticles(10000);
            
            // Start the simulation
            Console.WriteLine("Starting simulation...");
            simulation.Start();
            
            // Rendering loop
            Console.WriteLine("Press Escape to exit");
            bool running = true;
            while (running)
            {
                // Render the current state
                simulation.Render();
                
                // Check for exit
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        running = false;
                    }
                }
                
                // Limit rendering to ~30 FPS
                Thread.Sleep(1);
            }
            
            // Stop the simulation
            simulation.Stop();
            
            Console.WriteLine("Simulation stopped. Press any key to exit.");
            Console.ReadKey(true);
        }
    }
}
