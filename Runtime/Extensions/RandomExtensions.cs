#if INCLUDE_MATHEMATICS
using Random = Unity.Mathematics.Random;
#else
using Random = System.Random;
#endif // INCLUDE_MATHEMATICS

namespace PKGE
{
    public static class RandomExtensions
    {
        #region int
#if INCLUDE_MATHEMATICS
        public static int Range(ref this Random random, int minInclusive = 0, int maxExclusive = 1)
            => random.NextInt(minInclusive, maxExclusive);
#else
        public static int Range(this Random random, int minInclusive = 0, int maxExclusive = 1)
            => random.Next(minInclusive, maxExclusive);
#endif // INCLUDE_MATHEMATICS

#if INCLUDE_MATHEMATICS
        public static int NextInt(ref this Random random, int minInclusive = 0, int maxExclusive = 1)
#else
        public static int NextInt(this Random random, int minInclusive = 0, int maxExclusive = 1)
#endif // INCLUDE_MATHEMATICS
            => random.Range(minInclusive, maxExclusive);

        public static int NextInt(int minInclusive = 0, int maxExclusive = 1)
            => UnityEngine.Random.Range(minInclusive, maxExclusive);

#if INCLUDE_MATHEMATICS
        public static int Next(ref this Random random, int minInclusive = 0, int maxExclusive = 1)
#else
        public static int Next(this Random random, int minInclusive = 0, int maxExclusive = 1)
#endif // INCLUDE_MATHEMATICS
            => random.Range(minInclusive, maxExclusive);

        public static int Next(int minInclusive = 0, int maxExclusive = 1)
            => UnityEngine.Random.Range(minInclusive, maxExclusive);
        #endregion // int

        #region float
#if INCLUDE_MATHEMATICS
        public static float Range(ref this Random random, float minInclusive = 0, float maxExclusive = 1)
            => random.NextFloat(minInclusive, maxExclusive);
#else
        public static float Range(this Random random, float minInclusive = 0, float maxExclusive = 1)
            => (float)random.NextDouble() * (maxExclusive - minInclusive) + minInclusive;
#endif // INCLUDE_MATHEMATICS

#if INCLUDE_MATHEMATICS
        public static float NextFloat(ref this Random random, float minInclusive = 0, float maxExclusive = 1)
#else
        public static float NextFloat(this Random random, float minInclusive = 0, float maxExclusive = 1)
#endif // INCLUDE_MATHEMATICS
            => random.Range(minInclusive, maxExclusive);
        #endregion // float

        #region double
#if INCLUDE_MATHEMATICS
        public static double Range(ref this Random random, double minInclusive = 0, double maxExclusive = 1)
            => random.NextDouble(minInclusive, maxExclusive);
#else
        public static double Range(this Random random, double minInclusive = 0, double maxExclusive = 1)
            => random.NextDouble() * (maxExclusive - minInclusive) + minInclusive;
#endif // INCLUDE_MATHEMATICS

#if INCLUDE_MATHEMATICS
        public static double NextDouble(ref this Random random, double minInclusive = 0, double maxExclusive = 1)
#else
        public static double NextDouble(this Random random, double minInclusive = 0, double maxExclusive = 1)
#endif // INCLUDE_MATHEMATICS
            => random.Range(minInclusive, maxExclusive);
        #endregion // double
    }
}
