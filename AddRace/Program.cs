using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace AddRace
{
    class Program
    {
        private static int threadCount = 100;
        private static List<Thread> threads = new List<Thread>();

        private static List<int> queue = new List<int>();

        static void Main(string[] args)
        {
            for(var i = 0; i < threadCount; i++)
            {
                var t = new Thread(ParallelAdd);
                t.Start();
                threads.Add(t);
            }
            var count = queue.Count;
            var ts = DateTime.Now.Ticks;
            while(true)
            {
                System.Threading.Thread.Sleep(100);
                lock (queue)
                    count = queue.Count;
                var ts2 = DateTime.Now.Ticks;
                Console.WriteLine($"{count / (double)((ts2 - ts) / TimeSpan.TicksPerSecond)}");
            }
        }

        private static void ParallelAdd()
        {
            var i = 0;
            while(true)
            {
                lock(queue)
                    queue.Add(i);
                i++;
            }
        }
    }
}
