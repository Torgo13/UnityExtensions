namespace PKGE
{
    public static class RandomExtensions
    {
        #region int
        public static int NextInt(ref System.Random random, int minInclusive = 0, int maxExclusive = int.MaxValue)
            => random.Next(minInclusive, maxExclusive);

        public static int NextInt(this System.Random random, int minInclusive = 0, int maxExclusive = int.MaxValue)
            => random.Next(minInclusive, maxExclusive);

        public static int NextInt(int minInclusive = 0, int maxExclusive = int.MaxValue)
            => UnityEngine.Random.Range(minInclusive, maxExclusive);

#if INCLUDE_MATHEMATICS
        public static int Int(ref this Unity.Mathematics.Random random, int minInclusive = 0, int maxExclusive = int.MaxValue)
            => random.NextInt(minInclusive, maxExclusive);
#endif // INCLUDE_MATHEMATICS
            
        public static int Int(ref System.Random random, int minInclusive = 0, int maxExclusive = int.MaxValue)
            => random.NextInt(minInclusive, maxExclusive);

        public static int Int(this System.Random random, int minInclusive = 0, int maxExclusive = int.MaxValue)
            => random.NextInt(minInclusive, maxExclusive);

        public static int Int(int minInclusive = 0, int maxExclusive = int.MaxValue)
            => NextInt(minInclusive, maxExclusive);

#if INCLUDE_MATHEMATICS
        public static int Next(ref this Unity.Mathematics.Random random, int minInclusive = 0, int maxExclusive = int.MaxValue)
            => random.NextInt(minInclusive, maxExclusive);
#endif // INCLUDE_MATHEMATICS

        public static int Next(ref System.Random random, int minInclusive = 0, int maxExclusive = int.MaxValue)
            => random.NextInt(minInclusive, maxExclusive);

        public static int Next(this System.Random random, int minInclusive = 0, int maxExclusive = int.MaxValue)
            => random.NextInt(minInclusive, maxExclusive);

        public static int Next(int minInclusive = 0, int maxExclusive = int.MaxValue)
            => NextInt(minInclusive, maxExclusive);

#if INCLUDE_MATHEMATICS
        public static int Range(ref this Unity.Mathematics.Random random, int minInclusive = 0, int maxExclusive = int.MaxValue)
            => random.NextInt(minInclusive, maxExclusive);
#endif // INCLUDE_MATHEMATICS

        public static int Range(ref System.Random random, int minInclusive = 0, int maxExclusive = int.MaxValue)
            => random.NextInt(minInclusive, maxExclusive);

        public static int Range(this System.Random random, int minInclusive = 0, int maxExclusive = int.MaxValue)
            => random.NextInt(minInclusive, maxExclusive);
        #endregion // int

        #region float
        public static float NextFloat(ref System.Random random, float minInclusive = 0, float maxExclusive = 1)
            => (float)(random.NextDouble() * (maxExclusive - minInclusive) + minInclusive);

        public static float NextFloat(this System.Random random, float minInclusive = 0, float maxExclusive = 1)
            => (float)(random.NextDouble() * (maxExclusive - minInclusive) + minInclusive);

        public static float NextFloat(float minInclusive = 0, float maxExclusive = 1)
            => UnityEngine.Random.Range(minInclusive, maxInclusive: maxExclusive - float.Epsilon);

#if INCLUDE_MATHEMATICS
        public static float Float(ref this Unity.Mathematics.Random random, float minInclusive = 0, float maxExclusive = 1)
            => random.NextFloat(minInclusive, maxExclusive);
#endif // INCLUDE_MATHEMATICS

        public static float Float(ref System.Random random, float minInclusive = 0, float maxExclusive = 1)
            => random.NextFloat(minInclusive, maxExclusive);

        public static float Float(this System.Random random, float minInclusive = 0, float maxExclusive = 1)
            => random.NextFloat(minInclusive, maxExclusive);

        public static float Float(float minInclusive = 0, float maxExclusive = 1)
            => NextFloat(minInclusive, maxExclusive);

#if INCLUDE_MATHEMATICS
        public static float Range(ref this Unity.Mathematics.Random random, float minInclusive = 0, float maxInclusive = 1)
            => random.NextUInt() / (float)(uint.MaxValue - 1U) * (maxInclusive - minInclusive) + minInclusive;
#endif // INCLUDE_MATHEMATICS

        public static float Range(ref System.Random random, float minInclusive = 0, float maxInclusive = 1)
            => (uint)random.NextInt(int.MinValue, int.MaxValue) / (float)(uint.MaxValue - 1U) * (maxInclusive - minInclusive) + minInclusive;

        public static float Range(this System.Random random, float minInclusive = 0, float maxInclusive = 1)
            => (uint)random.NextInt(int.MinValue, int.MaxValue) / (float)(uint.MaxValue - 1U) * (maxInclusive - minInclusive) + minInclusive;
        #endregion // float

        #region double
        public static double NextDouble(ref System.Random random, double minInclusive = 0, double maxExclusive = 1)
            => random.NextDouble() * (maxExclusive - minInclusive) + minInclusive;

        public static double NextDouble(this System.Random random, double minInclusive = 0, double maxExclusive = 1)
            => random.NextDouble() * (maxExclusive - minInclusive) + minInclusive;

        public static double NextDouble(double minInclusive = 0, double maxExclusive = 1)
            => UnityEngine.Random.Range((float)minInclusive, maxInclusive: (float)(maxExclusive - float.Epsilon));

#if INCLUDE_MATHEMATICS
        public static double Double(ref this Unity.Mathematics.Random random, double minInclusive = 0, double maxExclusive = 1)
            => random.NextDouble(minInclusive, maxExclusive);
#endif // INCLUDE_MATHEMATICS
            
        public static double Double(ref System.Random random, double minInclusive = 0, double maxExclusive = 1)
            => random.NextDouble(minInclusive, maxExclusive);

        public static double Double(this System.Random random, double minInclusive = 0, double maxExclusive = 1)
            => random.NextDouble(minInclusive, maxExclusive);

        public static double Double(double minInclusive = 0, double maxExclusive = 1)
            => NextDouble(minInclusive, maxExclusive);

#if INCLUDE_MATHEMATICS
        public static double Range(ref this Unity.Mathematics.Random random, double minInclusive = 0, double maxInclusive = 1)
            => random.NextUInt() / (double)(uint.MaxValue - 1U) * (maxInclusive - minInclusive) + minInclusive;
#endif // INCLUDE_MATHEMATICS

        public static double Range(ref System.Random random, double minInclusive = 0, double maxInclusive = 1)
            => (uint)random.NextInt(int.MinValue, int.MaxValue) / (double)(uint.MaxValue - 1U) * (maxInclusive - minInclusive) + minInclusive;

        public static double Range(this System.Random random, double minInclusive = 0, double maxInclusive = 1)
            => (uint)random.NextInt(int.MinValue, int.MaxValue) / (double)(uint.MaxValue - 1U) * (maxInclusive - minInclusive) + minInclusive;
        #endregion // double

        #region Secure
        public static int SecureRandomInt => System.Security.Cryptography.RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);

        public static uint SecureRandomUInt
        {
            get
            {
                System.Span<byte> bytes = stackalloc byte[sizeof(uint)];
                System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
                return System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(bytes)[0];
            }
        }

        public static ulong SecureRandomULong
        {
            get
            {
                System.Span<byte> bytes = stackalloc byte[sizeof(ulong)];
                System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
                return System.Runtime.InteropServices.MemoryMarshal.Cast<byte, ulong>(bytes)[0];
            }
        }

        public static T SecureRandom<T>() where T : unmanaged
        {
            System.Span<byte> bytes = stackalloc byte[SizeOfCache<T>.Size];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            return System.Runtime.InteropServices.MemoryMarshal.Cast<byte, T>(bytes)[0];
        }
        #endregion // Secure

        #region Create
        [System.Runtime.CompilerServices.MethodImpl(256)]
#if INCLUDE_MATHEMATICS
        public static Unity.Mathematics.Random Create()
#else
        public static System.Random Create()
#endif // INCLUDE_MATHEMATICS
        {
            return CreateSafe(SecureRandomUInt);
        }

        /// <summary>
        /// Constructs a <see cref="Unity.Mathematics.Random"/> instance with a given seed value.
        /// <paramref name="seed"/> can be any <see langword="uint"/> value
        /// because it is wrapped between 1 and <see cref="uint.MaxValue"/>.
        /// </summary>
        /// <inheritdoc cref="Unity.Mathematics.Random(uint)"/>
        [System.Runtime.CompilerServices.MethodImpl(256)]
#if INCLUDE_MATHEMATICS
        public static Unity.Mathematics.Random CreateSafe(uint seed)
        {
            return new Unity.Mathematics.Random(1u + (seed % uint.MaxValue));
        }
#else
        public static System.Random CreateSafe(uint seed)
        {
            return CreateSafe((int)seed);
        }
#endif // INCLUDE_MATHEMATICS

        /// <inheritdoc cref="CreateSafe(uint)"/>
        [System.Runtime.CompilerServices.MethodImpl(256)]
#if INCLUDE_MATHEMATICS
        public static Unity.Mathematics.Random CreateSafe(int seed)
        {
            return CreateSafe((uint)seed);
        }
#else
        public static System.Random CreateSafe(int seed)
        {
            return new System.Random(seed);
        }
#endif // INCLUDE_MATHEMATICS

        /// <summary>
        /// Constructs a <see cref="Unity.Mathematics.Random"/> instance with an index that gets hashed.
        /// <paramref name="index"/> can be any <see langword="uint"/> value
        /// because it is wrapped between 0 and <see cref="uint.MaxValue"/> - 1.
        /// </summary>
        /// <inheritdoc cref="Unity.Mathematics.Random.CreateFromIndex(uint)"/>
        [System.Runtime.CompilerServices.MethodImpl(256)]
#if INCLUDE_MATHEMATICS
        public static Unity.Mathematics.Random CreateFromIndexSafe(uint index)
        {
            return Unity.Mathematics.Random.CreateFromIndex(index % uint.MaxValue);
        }
#else
        public static System.Random CreateFromIndexSafe(uint index)
        {
            return CreateSafe(index.GetHashCode());
        }
#endif // INCLUDE_MATHEMATICS

        /// <inheritdoc cref="CreateFromIndexSafe(uint)"/>
        [System.Runtime.CompilerServices.MethodImpl(256)]
#if INCLUDE_MATHEMATICS
        public static Unity.Mathematics.Random CreateFromIndexSafe(int index)
        {
            return CreateFromIndexSafe((uint)index);
        }
#else
        public static System.Random CreateFromIndexSafe(int index)
        {
            return CreateFromIndexSafe((uint)index);
        }
#endif // INCLUDE_MATHEMATICS
        #endregion // Create

        #region UnitSphere
#if INCLUDE_MATHEMATICS
        [System.Runtime.CompilerServices.MethodImpl(256)]
        public static Unity.Mathematics.float3 NextInsideUnitSphere(ref this Unity.Mathematics.Random random)
            => random.NextOnUnitSphere() * random.NextFloat();
#endif // INCLUDE_MATHEMATICS

        [System.Runtime.CompilerServices.MethodImpl(256)]
        public static Unity.Mathematics.float3 NextInsideUnitSphere(ref System.Random random)
            => random.NextOnUnitSphere() * random.NextFloat();

        [System.Runtime.CompilerServices.MethodImpl(256)]
        public static UnityEngine.Vector3 NextInsideUnitSphere(this System.Random random)
            => random.NextOnUnitSphere() * random.NextFloat();

#if INCLUDE_MATHEMATICS
        [System.Runtime.CompilerServices.MethodImpl(256)]
        public static Unity.Mathematics.float3 NextOnUnitSphere(ref this Unity.Mathematics.Random random)
            => random.NextFloat3Direction();
#endif // INCLUDE_MATHEMATICS

        [System.Runtime.CompilerServices.MethodImpl(256)]
        public static Unity.Mathematics.float3 NextOnUnitSphere(ref System.Random random)
            => random.NextFloat3Direction();

        [System.Runtime.CompilerServices.MethodImpl(256)]
        public static UnityEngine.Vector3 NextOnUnitSphere(this System.Random random)
            => random.NextFloat3Direction();

        [System.Runtime.CompilerServices.MethodImpl(256)]
        private static UnityEngine.Vector3 NextFloat3Direction(this System.Random random)
        {
            var z = random.Double() * 2 - 1;
            var r = (float)System.Math.Sqrt(System.Math.Max(1 - z * z, 0));
            var sc = r * random.NextFloat2Direction();
            return new UnityEngine.Vector3(sc.x, sc.y, (float)z);
        }
        #endregion // UnitSphere

        #region UnitCircle
#if INCLUDE_MATHEMATICS
        [System.Runtime.CompilerServices.MethodImpl(256)]
        public static Unity.Mathematics.float2 NextInsideUnitCircle(ref this Unity.Mathematics.Random random)
            => random.NextOnUnitCircle() * random.NextFloat();
#endif // INCLUDE_MATHEMATICS

        [System.Runtime.CompilerServices.MethodImpl(256)]
        public static Unity.Mathematics.float2 NextInsideUnitCircle(ref System.Random random)
            => random.NextOnUnitCircle() * random.NextFloat();

        [System.Runtime.CompilerServices.MethodImpl(256)]
        public static UnityEngine.Vector2 NextInsideUnitCircle(this System.Random random)
            => random.NextOnUnitCircle() * random.NextFloat();

#if INCLUDE_MATHEMATICS
        [System.Runtime.CompilerServices.MethodImpl(256)]
        public static Unity.Mathematics.float2 NextOnUnitCircle(ref this Unity.Mathematics.Random random)
            => random.NextFloat2Direction();
#endif // INCLUDE_MATHEMATICS

        [System.Runtime.CompilerServices.MethodImpl(256)]
        public static Unity.Mathematics.float2 NextOnUnitCircle(ref System.Random random)
            => random.NextFloat2Direction();

        [System.Runtime.CompilerServices.MethodImpl(256)]
        public static UnityEngine.Vector2 NextOnUnitCircle(this System.Random random)
            => random.NextFloat2Direction();

        [System.Runtime.CompilerServices.MethodImpl(256)]
        private static UnityEngine.Vector2 NextFloat2Direction(this System.Random random)
        {
            var angle = random.Double() * (System.Math.PI * 2);
            var s = (float)System.Math.Sin(angle);
            var c = (float)System.Math.Cos(angle);
            return new UnityEngine.Vector2(c, s);
        }
        #endregion // UnitCircle

        //https://github.com/needle-mirror/com.unity.physics/blob/e68e39991dabb1007b1cbe7f710740876f683ddf/Unity.Physics/Numerics/Random/Random.cs
        #region Unity.Numerics.Random
#if INCLUDE_MATHEMATICS
        /// <see cref="NextGaussian(System.Random, ref double, double, double)"/>
        public static double NextGaussian(ref this Unity.Mathematics.Random random, ref double spareGaussian,
            double mean = 0, double stdDev = 1)
        {
            if (!double.IsNaN(spareGaussian))
            {
                double nextGaussian = spareGaussian * stdDev + mean;
                spareGaussian = double.NaN;
                return nextGaussian;
            }

            Unity.Mathematics.double2 uv;
            double s;
            do
            {
                uv = Unity.Mathematics.math.mad(random.NextDouble2(), 2, -1);
                s = Unity.Mathematics.math.dot(uv, uv);
            }
            while (s >= 1 || System.Math.Abs(s) <= double.Epsilon);

            s = System.Math.Sqrt(-2 * System.Math.Log(s) / s);
            spareGaussian = uv.y * s;

            return mean + stdDev * (uv.x * s);
        }
#endif // INCLUDE_MATHEMATICS

        /// <see cref="NextGaussian(System.Random, ref double, double, double)"/>
        public static double NextGaussian(ref System.Random random, ref double spareGaussian,
            double mean = 0, double stdDev = 1)
        {
            return random.NextGaussian(ref spareGaussian, mean, stdDev);
        }

        /// <summary>
        /// Returns a Gaussian deviate using the Marsaglia polar method.
        /// <see href="https://en.wikipedia.org/wiki/Marsaglia_polar_method"/>
        /// </summary>
        /// <param name="random"></param>
        /// <param name="spareGaussian">Use <see cref="double.NaN"/> for the initial value.</param>
        /// <param name="mean">The distribution mean.</param>
        /// <param name="stdDev">The standard deviation.</param>
        /// <returns>A Gaussian distributed random number.</returns>
        public static double NextGaussian(this System.Random random, ref double spareGaussian,
            double mean = 0, double stdDev = 1)
        {
            if (!double.IsNaN(spareGaussian))
            {
                double nextGaussian = spareGaussian * stdDev + mean;
                spareGaussian = double.NaN;
                return nextGaussian;
            }

            double u, v, s;
            do
            {
                u = random.NextDouble() * 2 - 1;
                v = random.NextDouble() * 2 - 1;
                s = u * u + v * v;
            }
            while (s >= 1 || System.Math.Abs(s) <= double.Epsilon);

            s = System.Math.Sqrt(-2 * System.Math.Log(s) / s);
            spareGaussian = v * s;

            return mean + stdDev * (u * s);
        }
        #endregion // Unity.Numerics.Random

        //https://github.com/needle-mirror/com.unity.entities/blob/7866660bdd3140414ffb634a962b4bad37887261/Unity.Mathematics.Extensions/GeneratePoints.cs
        #region Unity.Entities
#if INCLUDE_MATHEMATICS
        /// <inheritdoc cref="PointsInSphere(System.Random, ref Unity.Collections.NativeArray{UnityEngine.Vector3}, UnityEngine.Vector3, float)"/>
        public static void PointsInSphere(ref this Unity.Mathematics.Random random,
            ref Unity.Collections.NativeArray<Unity.Mathematics.float3> points, Unity.Mathematics.float3 center, float radius)
        {
#if ZERO
            var radiusSquared = radius * radius;
            var pointsFound = 0;

            while (pointsFound < points.Length)
            {
                var p = Unity.Mathematics.math.mad(random.NextFloat3(), radius + radius, -radius);
                if (Unity.Mathematics.math.lengthsq(p) < radiusSquared)
                {
                    points[pointsFound++] = center + p;
                }
            }
#else
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = Unity.Mathematics.math.mad(random.NextInsideUnitSphere(), radius, center);
            }
#endif // ZERO
        }
#endif // INCLUDE_MATHEMATICS

        /// <inheritdoc cref="PointsInSphere(System.Random, ref Unity.Collections.NativeArray{UnityEngine.Vector3}, UnityEngine.Vector3, float)"/>
        public static void PointsInSphere(ref System.Random random,
            ref Unity.Collections.NativeArray<UnityEngine.Vector3> points, UnityEngine.Vector3 center, float radius)
        {
            random.PointsInSphere(ref points, center, radius);
        }

        /// <summary>
        /// A function that generates random points inside of a sphere.
        /// </summary>
        /// <param name="random"></param>
        /// <param name="points">A <see cref="Unity.Collections.NativeArray{T}"/> in which to store the randomly generated points</param>
        /// <param name="center">The center of the sphere</param>
        /// <param name="radius">The radius of the sphere</param>
        public static void PointsInSphere(this System.Random random,
            ref Unity.Collections.NativeArray<UnityEngine.Vector3> points, UnityEngine.Vector3 center, float radius)
        {
#if ZERO
            var radiusSquared = radius * radius;
            var pointsFound = 0;

            while (pointsFound < points.Length)
            {
                var p = new UnityEngine.Vector3(random.NextFloat(-radius, radius), random.NextFloat(-radius, radius), random.NextFloat(-radius, radius));
                if (p.sqrMagnitude < radiusSquared)
                {
                    points[pointsFound++] = center + p;
                }
            }
#else
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = random.NextInsideUnitSphere() * radius + center;
            }
#endif // ZERO
        }
        #endregion // Unity.Entities
    }
}
