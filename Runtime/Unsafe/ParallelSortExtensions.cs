#if INCLUDE_COLLECTIONS
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace PKGE.Unsafe
{
    public static class ParallelSortExtensions
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/GPUDriven/Utilities/ParallelSortExtensions.cs
        #region UnityEngine.Rendering
        const int MinRadixSortArraySize = 2048;
        const int MinRadixSortBatchSize = 256;

        public static JobHandle ParallelSort(this NativeArray<int> array)
        {
            if (array.Length <= 1)
                return new JobHandle();

            var jobHandle = new JobHandle();

            if (array.Length >= MinRadixSortArraySize)
            {
                int workersCount = System.Math.Max(Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount + 1, 1);
                int batchSize = System.Math.Max(MinRadixSortBatchSize, (int)System.Math.Ceiling((double)array.Length / workersCount));
                int jobsCount = (int)System.Math.Ceiling((double)array.Length / batchSize);

                Assert.IsTrue(jobsCount * batchSize >= array.Length);

                var supportArray = new NativeArray<int>(array.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                var counter = new NativeArray<int>(1, Allocator.TempJob);
                var buckets = new NativeArray<int>(jobsCount * 256, Allocator.TempJob);
                var indices = new NativeArray<int>(jobsCount * 256, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                var indicesSum = new NativeArray<int>(16, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                var arraySource = array;
                var arrayDest = supportArray;

                for (int radix = 0; radix < 4; ++radix)
                {
                    var bucketCountJobData = new RadixSortBucketCountJob
                    {
                        Radix = radix,
                        JobsCount = jobsCount,
                        BatchSize = batchSize,
                        Buckets = buckets,
                        Array = arraySource
                    };

                    var batchPrefixSumJobData = new RadixSortBatchPrefixSumJob
                    {
                        Radix = radix,
                        JobsCount = jobsCount,
                        Array = arraySource,
                        Counter = counter,
                        Buckets = buckets,
                        Indices = indices,
                        IndicesSum = indicesSum
                    };

                    var prefixSumJobData = new RadixSortPrefixSumJob
                    {
                        JobsCount = jobsCount,
                        Indices = indices,
                        IndicesSum = indicesSum
                    };

                    var bucketSortJobData = new RadixSortBucketSortJob
                    {
                        Radix = radix,
                        BatchSize = batchSize,
                        Indices = indices,
                        Array = arraySource,
                        ArraySorted = arrayDest
                    };

                    jobHandle = bucketCountJobData.ScheduleParallel(jobsCount, 1, jobHandle);
                    jobHandle = batchPrefixSumJobData.ScheduleParallel(16, 1, jobHandle);
                    jobHandle = prefixSumJobData.ScheduleParallel(16, 1, jobHandle);
                    jobHandle = bucketSortJobData.ScheduleParallel(jobsCount, 1, jobHandle);

                    JobHandle.ScheduleBatchedJobs();

                    (arraySource, arrayDest) = (arrayDest, arraySource);
                }

                _ = supportArray.Dispose(jobHandle);
                _ = counter.Dispose(jobHandle);
                _ = buckets.Dispose(jobHandle);
                _ = indices.Dispose(jobHandle);
                _ = indicesSum.Dispose(jobHandle);
            }
            else
            {
                jobHandle = array.SortJob().Schedule();
            }

            return jobHandle;
        }

        [BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
        private struct RadixSortBucketCountJob : IJobFor
        {
            [ReadOnly] public int Radix;
            [ReadOnly] public int JobsCount;
            [ReadOnly] public int BatchSize;
            [ReadOnly] [NativeDisableContainerSafetyRestriction, NoAlias] public NativeArray<int> Array;

            [NativeDisableContainerSafetyRestriction, NoAlias] public NativeArray<int> Buckets;

            public void Execute(int index)
            {
                int start = index * BatchSize;
                int end = math.min(start + BatchSize, Array.Length);

                int jobBuckets = index * 256;

                for (int i = start; i < end; ++i)
                {
                    int value = Array[i];
                    int bucket = (value >> Radix * 8) & 0xFF;
                    Buckets[jobBuckets + bucket] += 1;
                }
            }
        }

        [BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
        private struct RadixSortBatchPrefixSumJob : IJobFor
        {
            [ReadOnly] public int Radix;
            [ReadOnly] public int JobsCount;
            [ReadOnly] [NativeDisableContainerSafetyRestriction, NoAlias] public NativeArray<int> Array;

            [NativeDisableContainerSafetyRestriction, NoAlias] public NativeArray<int> Counter;
            [NativeDisableContainerSafetyRestriction, NoAlias] public NativeArray<int> IndicesSum;
            [NativeDisableContainerSafetyRestriction, NoAlias] public NativeArray<int> Buckets;
            [WriteOnly] [NativeDisableContainerSafetyRestriction, NoAlias] public NativeArray<int> Indices;

            private static int AtomicIncrement(NativeArray<int> counter)
            {
                return Interlocked.Increment(ref counter.UnsafeElementAtMutable(0));
            }

            private int JobIndexPrefixSum(int sum, int i)
            {
                for (int j = 0; j < JobsCount; ++j)
                {
                    int k = i + j * 256;

                    Indices[k] = sum;
                    sum += Buckets[k];
                    Buckets[k] = 0;
                }

                return sum;
            }

            public void Execute(int index)
            {
                int start = index * 16;
                int end = start + 16;

                int jobSum = 0;

                for (int i = start; i < end; ++i)
                    jobSum = JobIndexPrefixSum(jobSum, i);

                IndicesSum[index] = jobSum;

                if (AtomicIncrement(Counter) == 16)
                {
                    int sum = 0;

                    if(Radix < 3)
                    {
                        for (int i = 0; i < 16; ++i)
                        {
                            int indexSum = IndicesSum[i];
                            IndicesSum[i] = sum;
                            sum += indexSum;
                        }
                    }
                    else // Negative
                    {
                        for (int i = 8; i < 16; ++i)
                        {
                            int indexSum = IndicesSum[i];
                            IndicesSum[i] = sum;
                            sum += indexSum;
                        }

                        for (int i = 0; i < 8; ++i)
                        {
                            int indexSum = IndicesSum[i];
                            IndicesSum[i] = sum;
                            sum += indexSum;
                        }
                    }

                    Assert.AreEqual(sum, Array.Length);

                    Counter[0] = 0;
                }
            }
        }

        [BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
        private struct RadixSortPrefixSumJob : IJobFor
        {
            [ReadOnly] public int JobsCount;

            [ReadOnly] [NativeDisableContainerSafetyRestriction, NoAlias] public NativeArray<int> IndicesSum;
            [NativeDisableContainerSafetyRestriction, NoAlias] public NativeArray<int> Indices;

            public void Execute(int index)
            {
                int start = index * 16;
                int end = start + 16;

                int jobSum = IndicesSum[index];

                for (int j = 0; j < JobsCount; ++j)
                {
                    for (int i = start; i < end; ++i)
                    {
                        int k = j * 256 + i;
                        Indices[k] += jobSum;
                    }
                }
            }
        }

        [BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
        private struct RadixSortBucketSortJob : IJobFor
        {
            [ReadOnly] public int Radix;
            [ReadOnly] public int BatchSize;
            [ReadOnly] [NativeDisableContainerSafetyRestriction, NoAlias] public NativeArray<int> Array;

            [NativeDisableContainerSafetyRestriction, NoAlias] public NativeArray<int> Indices;
            [WriteOnly] [NativeDisableContainerSafetyRestriction, NoAlias] public NativeArray<int> ArraySorted;

            public void Execute(int index)
            {
                int start = index * BatchSize;
                int end = math.min(start + BatchSize, Array.Length);

                int jobIndices = index * 256;

                for (int i = start; i < end; ++i)
                {
                    int value = Array[i];
                    int bucket = (value >> Radix * 8) & 0xFF;
                    int sortedIndex = Indices[jobIndices + bucket]++;
                    ArraySorted[sortedIndex] = value;
                }
            }
        }
        #endregion // UnityEngine.Rendering
    }
}
#endif // INCLUDE_COLLECTIONS
