using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace UnityExtensions.Packages
{
    /// <summary>
    /// Utility for generating lists of poisson disk sampled points
    /// </summary>
    public static class CustomPoissonDiskSampling
    {
        //https://github.com/Unity-Technologies/PeopleSansPeople/blob/0026dedf5fe7364db4f60a5c22a91d90e1c7b2f5/peoplesanspeople_unity_env/Assets/Scripts/Utilities/CustomPoissonDiskSampling.cs
        #region UnityEngine.Perception.Randomization.Randomizers.Utilities
        const int k_DefaultSamplingResolution = 30;

        /// <summary>
        /// Returns a list of poisson disc sampled points for a given area and density
        /// </summary>
        /// <example><code>
        /// var seed = SamplerState.NextRandomState();
        /// using var placementSamples = CustomPoissonDiskSampling.GenerateSamples(
        ///     _widthX, _widthY, separationDistance, seed);
        /// var offset = new Vector3(minX, minY, 0f); // Recentre the samples at the origin
        /// foreach (var sample in placementSamples)
        /// {
        ///     float placementDepth = depth.Sample();
        ///     var instance = m_GameObjectOneWayCache.GetOrInstantiate(prefabs.Sample());
        ///     instance.transform.position = new Vector3(sample.x, sample.y, placementDepth) + offset;
        /// }
        /// </code></example>
        /// <param name="width">Width of the sampling area</param>
        /// <param name="height">Height of the sampling area</param>
        /// <param name="minimumRadius">The minimum distance required between each sampled point</param>
        /// <param name="seed">The random seed used to initialize the algorithm state</param>
        /// <param name="samplingResolution">The number of potential points sampled around every valid point</param>
        /// <param name="allocator">The allocator to use for the samples container</param>
        /// <returns>The list of generated poisson points</returns>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static NativeList<float2> GenerateSamples(
            float width,
            float height,
            float minimumRadius,
            uint seed = 12345,
            int samplingResolution = k_DefaultSamplingResolution,
            Allocator allocator = Allocator.TempJob)
        {
            GenerateSamplesJobHandle(out var croppedSamples,
                width, height, minimumRadius, seed, samplingResolution, allocator).Complete();

            return croppedSamples;
        }

        /// <inheritdoc cref="GenerateSamples"/>
        public static JobHandle GenerateSamplesJobHandle(
            out NativeList<float2> croppedSamples,
            float width,
            float height,
            float minimumRadius,
            uint seed = 12345,
            int samplingResolution = k_DefaultSamplingResolution,
            Allocator allocator = Allocator.TempJob)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Width cannot be negative");

            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height), "Height cannot be negative");

            if (minimumRadius < 0)
                throw new ArgumentOutOfRangeException(nameof(minimumRadius), "MinimumRadius cannot be negative");

            if (seed == 0)
                throw new ArgumentOutOfRangeException(nameof(seed), "Random seed cannot be 0");

            if (samplingResolution <= 0)
                throw new ArgumentOutOfRangeException(nameof(samplingResolution), "SamplingAttempts cannot be <= 0");

            var superSampledPoints = new NativeList<float2>(allocator);
            var sampleJob = new SampleJob
            {
                width = width + minimumRadius * 2,
                height = height + minimumRadius * 2,
                minimumRadius = minimumRadius,
                seed = seed,
                samplingResolution = samplingResolution,
                samples = superSampledPoints
            }.Schedule();

            croppedSamples = new NativeList<float2>(allocator);
            sampleJob = new CropJob
            {
                width = width,
                height = height,
                minimumRadius = minimumRadius,
                superSampledPoints = superSampledPoints.AsDeferredJobArray(),
                croppedSamples = croppedSamples
            }.Schedule(sampleJob);

            return sampleJob;
        }

        [BurstCompile(FloatMode = FloatMode.Fast)]
        struct SampleJob : IJob
        {
            public float width;
            public float height;
            public float minimumRadius;
            public uint seed;
            public int samplingResolution;
            public NativeList<float2> samples;

            public void Execute()
            {
                var newSamples = Sample(width, height, minimumRadius, seed, samplingResolution, Allocator.Temp);
                samples.AddRange(newSamples.AsArray());
                newSamples.Dispose();
            }
        }

        /// <summary>
        /// This job is for filtering out all super sampled Poisson points that are found outside the originally
        /// specified 2D region. This job will also shift the cropped points back to their original region.
        /// </summary>
        [BurstCompile(FloatMode = FloatMode.Fast)]
        struct CropJob : IJob
        {
            public float width;
            public float height;
            public float minimumRadius;
            [DeallocateOnJobCompletion]
            [ReadOnly] public NativeArray<float2> superSampledPoints;
            public NativeList<float2> croppedSamples;

            public void Execute()
            {
                var results = new NativeArray<bool>(
                    superSampledPoints.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

                // The comparisons operations made in this loop are done separately from the list-building loop
                // so that burst can automatically generate vectorized assembly code for this portion of the job.
                for (var i = 0; i < superSampledPoints.Length; i++)
                {
                    var point = superSampledPoints[i];
                    results[i] = point.x >= minimumRadius && point.x <= width + minimumRadius
                        && point.y >= minimumRadius && point.y <= height + minimumRadius;
                }

                var requiredCapacity = croppedSamples.Length + superSampledPoints.Length;
                if (croppedSamples.Capacity < requiredCapacity)
                    croppedSamples.Capacity = requiredCapacity;

                // This list-building code is done separately from the filtering loop
                // because it cannot be vectorized by burst.
                for (var i = 0; i < superSampledPoints.Length; i++)
                {
                    if (results[i])
                        croppedSamples.AddNoResize(superSampledPoints[i]);
                }

                // Remove the positional offset from the filtered-but-still-super-sampled points
                var offset = new float2(minimumRadius, minimumRadius);
                for (var i = 0; i < croppedSamples.Length; i++)
                    croppedSamples[i] -= offset;

                results.Dispose();
            }
        }

        // Algorithm sourced from Robert Bridson's paper "Fast Poisson Disk Sampling in Arbitrary Dimensions"
        // https://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf
        /// <summary>
        /// Returns a list of poisson disc sampled points for a given area and density
        /// </summary>
        /// <param name="width">Width of the sampling area</param>
        /// <param name="height">Height of the sampling area</param>
        /// <param name="minimumRadius">The minimum distance required between each sampled point</param>
        /// <param name="seed">The random seed used to initialize the algorithm state</param>
        /// <param name="samplingResolution">The number of potential points sampled around every valid point</param>
        /// <param name="allocator">The allocator type of the generated native container</param>
        /// <returns>The list of generated poisson points</returns>
        static NativeList<float2> Sample(
            float width,
            float height,
            float minimumRadius,
            uint seed,
            int samplingResolution,
            Allocator allocator)
        {
            var samples = new NativeList<float2>(allocator);

            // Calculate occupancy grid dimensions
            var random = new Unity.Mathematics.Random(seed);
            var cellSize = minimumRadius / SQRT2;
            var rows = (int)ceil(height / cellSize);
            var cols = (int)ceil(width / cellSize);
            var gridSize = rows * cols;
            if (gridSize == 0)
                return samples;

            // Initialize a few constants
            var rSqr = minimumRadius * minimumRadius;
            var samplingArc = PI2 / samplingResolution;
            var halfSamplingArc = samplingArc / 2;

            // Initialize a hash array that maps a sample's grid position to it's index
            var gridToSampleIndex = new NativeArray<int>(gridSize, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < gridSize; i++)
                gridToSampleIndex[i] = -1;

            // This list will track all points that may still have space around them for generating new points
            var activePoints = new NativeList<float2>(Allocator.Temp);

            // Randomly place a seed point to kick off the algorithm
            var firstPoint = new float2(random.NextFloat(0f, width), random.NextFloat(0f, height));
            samples.Add(firstPoint);
            var firstPointCol = (int)floor(firstPoint.x / cellSize);
            var firstPointRow = (int)floor(firstPoint.y / cellSize);
            gridToSampleIndex[firstPointCol + firstPointRow * cols] = 0;
            activePoints.Add(firstPoint);

            while (activePoints.Length > 0)
            {
                var randomIndex = random.NextInt(0, activePoints.Length);
                var activePoint = activePoints[randomIndex];

                var nextPointFound = false;
                for (var i = 0; i < samplingResolution; i++)
                {
                    var length = random.NextFloat(minimumRadius, minimumRadius * 2);
                    var angle = samplingArc * i + random.NextFloat(-halfSamplingArc, halfSamplingArc);

                    // Generate a new point within the circular placement region around the active point
                    sincos(angle, out var sin, out var cos);
                    var newPoint = activePoint + new float2(
                        cos * length,
                        sin * length);

                    var col = (int)floor(newPoint.x / cellSize);
                    var row = (int)floor(newPoint.y / cellSize);

                    if (row < 0 || row >= rows || col < 0 || col >= cols)
                        continue;

                    // Iterate over the 8 surrounding grid locations to check if the newly generated point is too close
                    // to an existing point
                    var tooCloseToAnotherPoint = false;
                    for (var x = -2; x <= 2; x++)
                    {
                        if ((col + x) < 0 || (col + x) >= cols)
                            continue;

                        for (var y = -2; y <= 2; y++)
                        {
                            if ((row + y) < 0 || (row + y) >= rows)
                                continue;

                            var gridIndex = (col + x) + (row + y) * cols;
                            if (gridToSampleIndex[gridIndex] < 0)
                                continue;

                            var distanceSqr = distancesq(samples[gridToSampleIndex[gridIndex]], newPoint);

                            if (distanceSqr >= rSqr)
                                continue;

                            tooCloseToAnotherPoint = true;
                            break;
                        }
                    }

                    if (tooCloseToAnotherPoint)
                        continue;

                    // If the new point is accepted, add it to the occupancy grid and the list of generated samples
                    nextPointFound = true;
                    activePoints.Add(newPoint);
                    samples.Add(newPoint);
                    gridToSampleIndex[col + row * cols] = samples.Length - 1;
                }

                if (!nextPointFound)
                    activePoints.RemoveAtSwapBack(randomIndex);
            }

            return samples;
        }
        #endregion // UnityEngine.Perception.Randomization.Randomizers.Utilities
    }
}
