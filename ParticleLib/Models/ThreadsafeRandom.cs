using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace ParticleLib.Models
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
            return (Next_s() - .5) > 0 ? true : false;
        }

        internal static float Next_a()
        {
            return (float)((Next_s() * Math.PI * 2) - Math.PI);
        }

        internal static Vector3 Next_v3()
        {
            return new Vector3((float)((Next_s() * Math.PI * 2) - Math.PI), (float)((Next_s() * Math.PI * 2) - Math.PI), (float)((Next_s() * Math.PI * 2) - Math.PI));
        }

        public static float Next(float low, float high, bool split = false)
        {
            var range = high - low;
            if (split)
                return Next(Next_b() ? range : low, (Next_s() * range));
            return low + (Next_s()* range);
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

            return (float)_local.NextDouble()/1;
        }
    }
}
