using Unity.Collections;
using Unity.Jobs;

namespace PKGE.Packages
{
    //https://github.com/Unity-Technologies/Megacity-2019/blob/1d90090d6d23417c661e7937e283b77b8e1db29d/Assets/Scripts/Gameplay/Traffic/UtilityJobs.cs
    #region Unity.Megacity.Traffic
    /// <summary>
    /// Utility jobs to clear native containers
    /// </summary>
    [Unity.Burst.BurstCompile]
    public struct ClearArrayJob<T> : IJobParallelFor where T : struct
    {
        [NativeMatchesParallelForLength]
        [WriteOnly] public NativeArray<T> Data;

        public void Execute(int index)
        {
            Data[index] = default;
        }
    }

#if INCLUDE_COLLECTIONS
    [Unity.Burst.BurstCompile]
    public struct ClearHashJob<T> : IJob where T : unmanaged
    {
        [WriteOnly] public NativeParallelMultiHashMap<int, T> Hash;

        public void Execute()
        {
            Hash.Clear();
        }
    }
#endif // INCLUDE_COLLECTIONS

    [Unity.Burst.BurstCompile]
    public struct DisposeArrayJob<T> : IJob where T : struct
    {
        [WriteOnly] [DeallocateOnJobCompletion]
        public NativeArray<T> Data;

        public readonly void Execute()
        {
        }
    }
    #endregion // Unity.Megacity.Traffic

    [Unity.Burst.BurstCompile]
    public struct CopyArrayJob<T> : IJobFor where T : struct
    {
        [NativeMatchesParallelForLength]
        [ReadOnly] public NativeArray<T> src;
        [NativeMatchesParallelForLength]
        [WriteOnly] public NativeArray<T> dst;

        public void Execute(int index)
        {
            dst[index] = src[index];
        }
    }

    [Unity.Burst.BurstCompile]
    public struct SetArrayJob<T> : IJobFor where T : struct
    {
        //https://github.com/needle-mirror/com.unity.entities/blob/7866660bdd3140414ffb634a962b4bad37887261/Unity.Entities/CopyUtility.cs
        #region Unity.Entities
        [ReadOnly] public T src;
        [NativeMatchesParallelForLength]
        [WriteOnly] public NativeArray<T> dst;

        public void Execute(int index)
        {
            dst[index] = src;
        }
        #endregion // Unity.Entities
    }

    [Unity.Burst.BurstCompile]
    public struct SwapArrayJob<T> : IJobFor where T : struct
    {
        [NativeMatchesParallelForLength]
        public NativeArray<T> src0;
        [NativeMatchesParallelForLength]
        public NativeArray<T> src1;

        public void Execute(int index)
        {
            (src0[index], src1[index]) = (src1[index], src0[index]);
        }
    }

    [Unity.Burst.BurstCompile]
    public struct CompareArrayJob<T> : IJobFor where T : struct, System.IEquatable<T>
    {
        [NativeMatchesParallelForLength]
        [ReadOnly] public NativeArray<T> src0;
        [NativeMatchesParallelForLength]
        [ReadOnly] public NativeArray<T> src1;
        [NativeMatchesParallelForLength]
        [WriteOnly] public NativeArray<bool> dst;

        public void Execute(int index)
        {
            dst[index] = src0[index].Equals(src1[index]);
        }
    }

#if INCLUDE_COLLECTIONS
    /// <remarks>
    /// Does not clear <see cref="input"/> after the job is complete.
    /// </remarks>
    [Unity.Burst.BurstCompile]
    public struct CopyQueueJob<T> : IJobFor where T : unmanaged
    {
        [NativeMatchesParallelForLength]
        [ReadOnly] public NativeQueue<T>.ReadOnly input;
        [WriteOnly] public NativeQueue<T>.ParallelWriter output;

        public void Execute(int index)
        {
            output.Enqueue(input[index]);
        }
    }

    [Unity.Burst.BurstCompile]
    public struct ClearQueueJob<T> : IJob where T : unmanaged
    {
        [WriteOnly] public NativeQueue<T> input;

        public void Execute()
        {
            input.Clear();
        }
    }
#endif // INCLUDE_COLLECTIONS
}
