namespace PKGE
{
    public static class RandomExtensions
    {
        #region int
#if INCLUDE_MATHEMATICS
        public static int Int(ref this Unity.Mathematics.Random random, int minInclusive = 0, int maxExclusive = 1)
            => random.NextInt(minInclusive, maxExclusive);
#endif // INCLUDE_MATHEMATICS

        public static int Int(this System.Random random, int minInclusive = 0, int maxExclusive = 1)
            => random.Next(minInclusive, maxExclusive);

        public static int Int(int minInclusive = 0, int maxExclusive = 1)
            => UnityEngine.Random.Range(minInclusive, maxExclusive);

        public static int NextInt(this System.Random random, int minInclusive = 0, int maxExclusive = 1)
            => random.Int(minInclusive, maxExclusive);

        public static int NextInt(int minInclusive = 0, int maxExclusive = 1)
            => Int(minInclusive, maxExclusive);

#if INCLUDE_MATHEMATICS
        public static int Next(ref this Unity.Mathematics.Random random, int minInclusive = 0, int maxExclusive = 1)
            => random.Int(minInclusive, maxExclusive);
#endif // INCLUDE_MATHEMATICS

        public static int Next(int minInclusive = 0, int maxExclusive = 1)
            => Int(minInclusive, maxExclusive);

#if INCLUDE_MATHEMATICS
        public static int Range(ref this Unity.Mathematics.Random random, int minInclusive = 0, int maxExclusive = 1)
            => random.Int(minInclusive, maxExclusive);
#endif // INCLUDE_MATHEMATICS

        public static int Range(this System.Random random, int minInclusive = 0, int maxExclusive = 1)
            => random.Int(minInclusive, maxExclusive);
        #endregion // int

        #region float
#if INCLUDE_MATHEMATICS
        public static float Float(ref this Unity.Mathematics.Random random, float minInclusive = 0, float maxExclusive = 1)
            => random.NextFloat(minInclusive, maxExclusive);
#endif // INCLUDE_MATHEMATICS

        public static float Float(this System.Random random, float minInclusive = 0, float maxExclusive = 1)
            => (float)random.NextDouble() * (maxExclusive - minInclusive) + minInclusive;

        public static float Float(float minInclusive = 0, float maxExclusive = 1)
            => UnityEngine.Random.Range(minInclusive, maxInclusive: maxExclusive - float.Epsilon);

        public static float NextFloat(this System.Random random, float minInclusive = 0, float maxExclusive = 1)
            => random.Float(minInclusive, maxExclusive);

        public static float NextFloat(float minInclusive = 0, float maxExclusive = 1)
            => Float(minInclusive, maxExclusive);

#if INCLUDE_MATHEMATICS
        public static float Range(ref this Unity.Mathematics.Random random, float minInclusive = 0, float maxExclusive = 1)
            => random.Float(minInclusive, maxExclusive);
#endif // INCLUDE_MATHEMATICS

        public static float Range(this System.Random random, float minInclusive = 0, float maxExclusive = 1)
            => random.Float(minInclusive, maxExclusive);
        #endregion // float

        #region double
#if INCLUDE_MATHEMATICS
        public static double Double(ref this Unity.Mathematics.Random random, double minInclusive = 0, double maxExclusive = 1)
            => random.NextDouble(minInclusive, maxExclusive);
#endif // INCLUDE_MATHEMATICS

        public static double Double(this System.Random random, double minInclusive = 0, double maxExclusive = 1)
            => random.NextDouble() * (maxExclusive - minInclusive) + minInclusive;

        public static double Double(double minInclusive = 0, double maxExclusive = 1)
            => UnityEngine.Random.Range((float)minInclusive, maxInclusive: (float)(maxExclusive - double.Epsilon));

#if INCLUDE_MATHEMATICS
        public static double Range(ref this Unity.Mathematics.Random random, double minInclusive = 0, double maxExclusive = 1)
            => random.Double(minInclusive, maxExclusive);
#endif // INCLUDE_MATHEMATICS

        public static double Range(this System.Random random, double minInclusive = 0, double maxExclusive = 1)
            => random.Double(minInclusive, maxExclusive);

        public static double NextDouble(this System.Random random, double minInclusive = 0, double maxExclusive = 1)
            => random.Double(minInclusive, maxExclusive);

        public static double NextDouble(double minInclusive = 0, double maxExclusive = 1)
            => Double(minInclusive, maxExclusive);
        #endregion // double
    }
}
