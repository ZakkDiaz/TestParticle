using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace StaticTree
{
    public static class Emitter
    {
        private static List<Thread> emitterThreads = new List<Thread>();
        public static void AddEmitter()
        {
            var addThread = new Thread(StartEmitter);
            addThread.Start();
            emitterThreads.Add(addThread);
        }

        private static int id = 0;
        private static void StartEmitter()
        {
            var center = Program.octree.To - Program.octree.From;

            int count = 0;
            int emitterId = id;
            id++;
            ///while (true) 
            {  
                for (var i = 0; i < 100000; i++)
                {
                    count++;
                    Program.octree.AddAsync(center.X * ParticleLib.Models.ThreadSafeRandom.Next_s() + Program.octree.From.X, center.Y * ParticleLib.Models.ThreadSafeRandom.Next_s() + Program.octree.From.Y, center.Z * ParticleLib.Models.ThreadSafeRandom.Next_s() + Program.octree.From.Z);
                }
                Console.WriteLine($"{emitterId}:{count}");
            }

            //int count = 0;
            //int emitterId = id;
            //id++;
            //while (true)
            //{
            //    try
            //    {
            //        count++;
            //        Program.octree.AddAsync(center.X*ParticleLib.Models.ThreadSafeRandom.Next_s() + Program.octree.From.X, center.Y * ParticleLib.Models.ThreadSafeRandom.Next_s() + Program.octree.From.Y, center.Z * ParticleLib.Models.ThreadSafeRandom.Next_s() + Program.octree.From.Z);
            //        Console.WriteLine($"{emitterId}:{count}");
            //    } catch(Exception ex)
            //    {

            //    }
            //}
        }
    }
}
