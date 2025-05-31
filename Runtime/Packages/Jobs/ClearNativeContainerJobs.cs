using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UnityExtensions.Packages
{
    //https://github.com/Unity-Technologies/Megacity-2019/blob/1d90090d6d23417c661e7937e283b77b8e1db29d/Assets/Scripts/Gameplay/Traffic/UtilityJobs.cs
    #region Unity.Megacity.Traffic
    /// <summary>
    /// Utility jobs to clear native containers
    /// </summary>
    [BurstCompile]
    public struct ClearArrayJob<T> : IJobParallelFor where T : struct
    {
        [WriteOnly] public NativeArray<T> Data;

        public void Execute(int index)
        {
            Data[index] = default;
        }
    }

    [BurstCompile]
    public struct ClearHashJob<T> : IJob where T : unmanaged
    {
        [WriteOnly] public NativeParallelMultiHashMap<int, T> Hash;

        public void Execute()
        {
            Hash.Clear();
        }
    }

    [BurstCompile]
    public struct DisposeArrayJob<T> : IJob where T : struct
    {
        [WriteOnly] [DeallocateOnJobCompletion]
        public NativeArray<T> Data;

        public readonly void Execute()
        {
        }
    }
    #endregion // Unity.Megacity.Traffic

    [BurstCompile]
    public struct CopyArrayJob<T> : IJobFor where T : struct
    {
        [ReadOnly] public NativeArray<T> src;
        [WriteOnly] public NativeArray<T> dst;

        public void Execute(int index)
        {
            dst[index] = src[index];
        }
    }

    [BurstCompile]
    public struct SetArrayJob<T> : IJobFor where T : struct
    {
        [ReadOnly] public T src;
        [WriteOnly] public NativeArray<T> dst;

        public void Execute(int index)
        {
            dst[index] = src;
        }
    }

    [BurstCompile]
    public struct SwapArrayJob<T> : IJobFor where T : struct
    {
        public NativeArray<T> src0;
        public NativeArray<T> src1;

        public void Execute(int index)
        {
            (src0[index], src1[index]) = (src1[index], src0[index]);
        }
    }

    [BurstCompile]
    public struct CompareArrayJob<T> : IJobFor where T : struct, System.IEquatable<T>
    {
        [ReadOnly] public NativeArray<T> src0;
        [ReadOnly] public NativeArray<T> src1;
        [WriteOnly] public NativeArray<bool> dst;

        public void Execute(int index)
        {
            dst[index] = src0[index].Equals(src1[index]);
        }
    }
}
