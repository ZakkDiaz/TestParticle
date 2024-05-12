//namespace BarnesHut
//{
//    using System;
//    using System.Runtime.CompilerServices;
//    using System.Runtime.Intrinsics;
//    using System.Runtime.Intrinsics.X86;

//    using V256d = System.Runtime.Intrinsics.Vector256<double>;


//    public class Program {
//        [SkipLocalsInit]
//        public unsafe static void Main(string[] args)
//        {
//            int iterations = args.Length > 0 ? Int32.Parse(args[0]) : 10000;
//            if (iterations <= 0) { return; }

//            V256d* mem = stackalloc V256d[18];
//            // Align the memory (C# doesn't have a built in way AFAIK) to prevent fault when calling Avx.LoadAlignedVector256 or Avx.StoreAligned
//            mem = (V256d*)((((UInt64)mem) + 31UL) & ~31UL);

//            Net60_NBody_AVX_9_3b.InitSystem(mem, out V256d* m, out V256d* p, out V256d* v);

//            Console.WriteLine(Net60_NBody_AVX_9_3b.Energy((double*)mem, p, v).ToString("F9"));

//            Net60_NBody_AVX_9_3b.Advance(iterations, 0.01, m, p, v);

//            Console.WriteLine(Net60_NBody_AVX_9_3b.Energy((double*)mem, p, v).ToString("F9"));
//        }
//    }
//}
