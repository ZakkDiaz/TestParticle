namespace BarnesHut
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Intrinsics;
    using System.Runtime.Intrinsics.X86;

    using static System.Runtime.CompilerServices.MethodImplOptions;

    using V256d = System.Runtime.Intrinsics.Vector256<double>;

    public static unsafe class Net60_NBody_AVX_9_3b
    {


        [MethodImpl(AggressiveOptimization | AggressiveInlining)]
        private static V256d Permute2x128AndBlend(V256d t0, V256d t1)
          => Avx.Add(Avx.Permute2x128(t0, t1, 0b10_0001), Avx.Blend(t0, t1, 0b1100));

        public static void Advance(int iterations, double dt, double[] masses, V256d[] positions, V256d[] velocities)
        {
            var step = Vector256.Create(dt);
            var c0375 = Vector256.Create(0.375);
            var c1250 = Vector256.Create(1.25);
            var c1875 = Vector256.Create(1.875);
            var r = new V256d[velocities.Length * velocities.Length];
            var w = new double[velocities.Length * velocities.Length]; // Adjust size as needed for storing intermediate calculations

            for (int iter = 0; iter < iterations; iter++)
            {
                InitDiffs(positions, r);
                CalcStepDistances(step, c0375, c1250, c1875, r, w);
                CalcNewVelocities(velocities, masses, r, w);
                CalcNewPositions(step, positions, velocities);
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            static void CalcStepDistances(V256d step, V256d c0375, V256d c1250, V256d c1875, V256d[] r, double[] w)
            {
                w[0] = TimeAdjust(step, FastReciprocalSqRoot(c0375, c1250, c1875, Avx.HorizontalAdd(Square(r[0]), Square(r[1])), Avx.HorizontalAdd(Square(r[2]), Square(r[3])))).GetElement(0);
                w[1] = TimeAdjust(step, FastReciprocalSqRoot(c0375, c1250, c1875, Avx.HorizontalAdd(Square(r[4]), Square(r[5])), Avx.HorizontalAdd(Square(r[6]), Square(r[7])))).GetElement(0);
                w[2] = TimeAdjust(step, FastReciprocalSqRoot(c0375, c1250, c1875, Avx.HorizontalAdd(Square(r[8]), Square(r[9])), V256d.Zero)).GetElement(0);

                [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
                static V256d TimeAdjust(V256d rt, V256d x) => Avx.Multiply(Avx.Multiply(x, x), Avx.Multiply(x, rt));
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            static void CalcNewVelocities(V256d[] v, double[] m, V256d[] r, double[] w)
            {
                int rIdx = 0;
                for (int i = 1; i < v.Length; ++i)
                {
                    V256d iV = v[i];
                    double iM = m[i];
                    for (int j = 0; j < i; ++j)
                    {
                        V256d kW = Vector256.Create(w[rIdx]);
                        V256d kR = r[rIdx++];
                        double jM = m[j];
                        V256d jV = v[j];
                        V256d t = Avx.Multiply(kR, kW);
                        V256d jM_t = Avx.Multiply(Vector256.Create(jM), t);
                        V256d iM_t = Avx.Multiply(Vector256.Create(iM), t);
                        iV = Avx.Subtract(iV, jM_t);
                        v[j] = Avx.Add(jV, iM_t);
                    }
                    v[i] = iV;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            static void CalcNewPositions(V256d step, V256d[] p, V256d[] v)
            {
                for (int i = 0; i < p.Length; i++)
                {
                    p[i] = Avx.Add(p[i], Avx.Multiply(v[i], step));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        private static V256d FastReciprocalSqRoot(V256d c0375, V256d c1250, V256d c1875, V256d t0, V256d t1)
        {
            V256d s = Avx.Add(Avx.Permute2x128(t0, t1, 0b10_0001), Avx.Blend(t0, t1, 0b1100));
            V256d x = Avx.ConvertToVector256Double(Sse.ReciprocalSqrt(Avx.ConvertToVector128Single(s)));
            V256d y = Avx.Multiply(s, Avx.Multiply(x, x));
            V256d y0 = Avx.Multiply(Avx.Multiply(y, c0375), y);
            V256d y1 = Avx.Subtract(Avx.Multiply(y, c1250), c1875);
            return Avx.Multiply(x, Avx.Subtract(y0, y1));
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        private static V256d Square(V256d x) => Avx.Multiply(x, x);

        private static void InitDiffs(V256d[] positions, V256d[] rsqrts)
        {
            int k = 0;
            for (int i = 1; i < positions.Length; ++i)
            {
                V256d pi = positions[i];
                for (int j = 0; j < i; ++j, ++k)
                {
                    rsqrts[k] = Avx.Subtract(pi, positions[j]);
                }
            }
        }


        //[SkipLocalsInit]
        //public static double Energy(double* m, V256d* p, V256d* v)
        //{
        //    unchecked
        //    {
        //        double e = SumComponents256(
        //          Avx.Multiply(
        //            Avx.Multiply(
        //              Permute2x128AndBlend(
        //                Avx.HorizontalAdd(Square(v[0]), Square(v[1])),
        //                Avx.HorizontalAdd(Square(v[2]), Square(v[3]))),
        //              Avx.LoadAlignedVector256(m)),
        //            Vector256.Create(0.5)))
        //          + Permute2x128AndBlend(Avx.HorizontalAdd(Square(v[4]), V256d.Zero), V256d.Zero).GetElement(0) * m[4] * 0.5;


        //        V256d* r = stackalloc V256d[14];
        //        // Align the memory (C# doesn't have a built in way AFAIK) to prevent fault when calling Avx.LoadAlignedVector256 or Avx.StoreAligned
        //        r = (V256d*)((((UInt64)r) + 31UL) & ~31UL);
        //        InitDiffs(p, r);

        //        V256d c0375 = Vector256.Create(0.375), c1250 = Vector256.Create(1.25), c1875 = Vector256.Create(1.875);
        //        r[10] = FastReciprocalSqRoot(c0375, c1250, c1875, Avx.HorizontalAdd(Square(r[0]), Square(r[1])), Avx.HorizontalAdd(Square(r[2]), Square(r[3])));
        //        r[11] = FastReciprocalSqRoot(c0375, c1250, c1875, Avx.HorizontalAdd(Square(r[4]), Square(r[5])), Avx.HorizontalAdd(Square(r[6]), Square(r[7])));
        //        r[12] = FastReciprocalSqRoot(c0375, c1250, c1875, Avx.HorizontalAdd(Square(r[8]), Square(r[9])), V256d.Zero);

        //        double* w = (double*)(r + 10);
        //        for (int i = 1; i < 5; ++i)
        //        {
        //            double iMass = m[i];
        //            for (int j = 0; j < i; ++j, ++w)
        //            {
        //                e = e - (iMass * m[j] * w[0]);
        //            }
        //        }
        //        return e;

        //        [MethodImpl(AggressiveOptimization | AggressiveInlining)]
        //        static double SumComponents128(Vector128<double> x) => x.GetElement(1) + x.GetElement(0);
        //        [MethodImpl(AggressiveOptimization | AggressiveInlining)]
        //        static double SumComponents256(V256d x) => SumComponents128(Avx.Add(x.GetLower(), x.GetUpper()));
        //    }
        //}
        public static void InitSystem(out double[] masses, out V256d[] positions, out V256d[] velocities)
        {
            const double SOLAR_MASS = (4 * Math.PI * Math.PI);
            const double DAYS_PER_YEAR = 365.24;
            const double POSITION_RANGE = 500.0; // Scale for positions
            const int PARTICLE_COUNT = 25; // Number of particles to simulate

            masses = new double[PARTICLE_COUNT];
            positions = new V256d[PARTICLE_COUNT];
            velocities = new V256d[PARTICLE_COUNT];

            var random = new Random();

            // Initialize masses and generate random masses
            for (int i = 0; i < PARTICLE_COUNT; i++)
            {
                if (i == 0)
                {
                    masses[i] = SOLAR_MASS; // Assign solar mass to the first particle
                }
                else
                {
                    masses[i] = random.NextDouble() * SOLAR_MASS * 0.001; // Random smaller mass
                }
            }

            // Generate random positions close to the center
            for (int i = 0; i < PARTICLE_COUNT; i++)
            {
                positions[i] = Vector256.Create(
                    RandomRange(random, -POSITION_RANGE, POSITION_RANGE),
                    RandomRange(random, -POSITION_RANGE, POSITION_RANGE),
                    RandomRange(random, -POSITION_RANGE, POSITION_RANGE),
                    0.0 // Unused fourth value
                );
            }

            // Generate random velocities
            for (int i = 0; i < PARTICLE_COUNT; i++)
            {
                velocities[i] = Vector256.Create(
                    RandomRange(random, -1e-3, 1e-3),
                    RandomRange(random, -1e-3, 1e-3),
                    RandomRange(random, -1e-3, 1e-3),
                    0.0 // Unused fourth value
                );
            }

            // Offset momentum to ensure total momentum is zero
            V256d momentum = V256d.Zero;
            for (int i = 1; i < PARTICLE_COUNT; i++)
            {
                momentum = Avx.Add(momentum, Avx.Multiply(velocities[i], Vector256.Create(masses[i])));
            }

            velocities[0] = Avx.Divide(momentum, Avx.Multiply(Vector256.Create(-1.0), Vector256.Create(masses[0])));
        }

        // Helper function to generate a random double within a range
        private static double RandomRange(Random random, double minValue, double maxValue)
        {
            return minValue + (random.NextDouble() * (maxValue - minValue));
        }

    }
}
