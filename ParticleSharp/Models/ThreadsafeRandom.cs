using System.Numerics;
using System;
using Random = System.Random;

namespace ParticleSharp.Models
{
    public class ThreadSafeRandom
    {
        private static readonly Random _global = new Random();
        [ThreadStatic]
        private static Random _local;

        public ThreadSafeRandom()
        {
            if (_local == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                }
                _local = new Random(seed);
            }
        }

        public static bool Next_b()
        {
            return Next_s() - .5 > 0 ? true : false;
        }

        internal static float Next_a()
        {
            return (float)(Next_s() * Math.PI * 2 - Math.PI);
        }

        internal static Vector3 Next_v3()
        {
            return new Vector3((float)(Next_s() * Math.PI * 2 - Math.PI), (float)(Next_s() * Math.PI * 2 - Math.PI), (float)(Next_s() * Math.PI * 2 - Math.PI));
        }

        internal static Vector3 Next_v3f()
        {
            return new Vector3((float)Next_s(), (float)Next_s(), (float)Next_s());
        }

        public static float Next(float low, float high, bool split = false)
        {
            var range = high - low;
            if (split)
                return Next(Next_b() ? range : low, Next_s() * range);
            return low + Next_s() * range;
        }

        public static float Next_s()
        {
            if (_local == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                }
                _local = new Random(seed);
            }

            return (float)_local.NextDouble() / 1;
        }

        public static float NextWeightedParticleSize()
        {
            float r = Next_s();


            if (r < 0.3f)
            {
                return Next(100000f, 500000f); // MASSIVE range
            }
            else if (r < 0.7f)
            {
                return Next(8f, 10f); // Large range
            }
            else if (r < 0.8f)
            {
                // Medium size (30% probability)
                return Next(3f, 8f); // Medium range
            }
            else
            {
                // Small size (60% probability)
                return Next(0.01f, 3f); // Small range
            }
        }

    }
}
