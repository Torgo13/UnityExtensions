#if INCLUDE_MATHEMATICS
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace PKGE.Packages
{
    /// <summary>
    /// Tools for generating random points inside of a shape of interest
    /// </summary>
    public struct GeneratePoints
    {
        //https://github.com/needle-mirror/com.unity.entities/blob/7866660bdd3140414ffb634a962b4bad37887261/Unity.Mathematics.Extensions/GeneratePoints.cs
        #region Unity.Entities
        [Unity.Burst.BurstCompile]
        struct PointsInSphere : IJob
        {
            public float Radius;
            public float3 Center;
            [WriteOnly]
            public NativeArray<float3> Points;
            public uint Seed;

            public void Execute()
            {
                var random = new Random(Seed);
                random.PointsInSphere(ref Points, Center, Radius);
            }
        }

        /// <summary>
        /// Schedule a Burst job to generate random points inside of a sphere
        /// </summary>
        /// <param name="inputDeps">A JobHandle to wait for, before the job scheduled by this function</param>
        /// <returns>A JobHandle of the job that was created to generate random points inside a sphere</returns>
        /// <inheritdoc cref="RandomPointsInSphere(NativeArray{float3}, float3, float, uint)"/>
        public static JobHandle RandomPointsInSphere(NativeArray<float3> points, float3 center, float radius,
            uint seed, JobHandle inputDeps)
        {
            return new PointsInSphere
            {
                Radius = radius,
                Center = center,
                Points = points,
                Seed = seed,
            }.Schedule(inputDeps);
        }

        /// <summary>
        /// A function that generates random points inside of a sphere. Schedules and completes jobs,
        /// before returning to its caller.
        /// </summary>
        /// <param name="points">A NativeArray in which to store the randomly generated points</param>
        /// <param name="center">The center of the sphere</param>
        /// <param name="radius">The radius of the sphere</param>
        public static void RandomPointsInSphere(NativeArray<float3> points, float3 center = default, float radius = 1.0f,
            uint seed = 0x6E624EB7u)
        {
            new PointsInSphere
            {
                Radius = radius,
                Center = center,
                Points = points,
                Seed = seed,
            }.Run();
        }
        #endregion // Unity.Entities
    }
}
#endif // INCLUDE_MATHEMATICS
