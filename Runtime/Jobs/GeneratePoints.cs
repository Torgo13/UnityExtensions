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
            public int Count;
            public uint Seed;

            public void Execute()
            {
                var radiusSquared = Radius * Radius;
                var pointsFound = 0;
                var random = new Random(Seed);

                while (pointsFound < Count)
                {
                    var p = random.NextFloat3() * new float3(Radius + Radius) - new float3(Radius);
                    if (math.lengthsq(p) < radiusSquared)
                    {
                        Points[pointsFound] = Center + p;
                        pointsFound++;
                    }
                }
            }
        }

        /// <summary>
        /// Schedule a Burst job to generate random points inside of a sphere
        /// </summary>
        /// <param name="inputDeps">A JobHandle to wait for, before the job scheduled by this function</param>
        /// <returns>A JobHandle of the job that was created to generate random points inside a sphere</returns>
        /// <inheritdoc cref="RandomPointsInSphere(NativeArray{float3}, float3, float)"/>
        public static JobHandle RandomPointsInSphere(NativeArray<float3> points, float3 center, float radius,
            JobHandle inputDeps)
        {
            var pointsInSphereJob = new PointsInSphere
            {
                Radius = radius,
                Center = center,
                Points = points,
                Count = points.Length,
                Seed = 0x6E624EB7u,
            };
            var pointsInSphereJobHandle = pointsInSphereJob.Schedule(inputDeps);
            return pointsInSphereJobHandle;
        }

        /// <summary>
        /// A function that generates random points inside of a sphere. Schedules and completes jobs,
        /// before returning to its caller.
        /// </summary>
        /// <param name="points">A NativeArray in which to store the randomly generated points</param>
        /// <param name="center">The center of the sphere</param>
        /// <param name="radius">The radius of the sphere</param>
        public static void RandomPointsInSphere(NativeArray<float3> points, float3 center = default, float radius = 1.0f)
        {
            var pointsInSphereJob = new PointsInSphere
            {
                Radius = radius,
                Center = center,
                Points = points,
                Count = points.Length,
                Seed = 0x6E624EB7u,
            };
            pointsInSphereJob.Run();
        }

        /// <summary>
        /// A function that returns a single random position, fairly distributed inside the unit sphere.
        /// </summary>
        /// <param name="seed">A seed to the random number generator</param>
        /// <returns>A point inside of the unit sphere, fairly distributed</returns>
        public static float3 RandomPositionInsideUnitSphere(uint seed)
        {
            var random = new Random(seed);
            while (true)
            {
                float3 randomPosition = random.NextFloat3();
                var doubled = randomPosition * new float3(2);
                var offset = doubled - new float3(1, 1, 1);
                if (math.lengthsq(offset) > 1)
                    continue;

                return offset;
            }
        }
    }
    #endregion // Unity.Entities
}
#endif // INCLUDE_MATHEMATICS
