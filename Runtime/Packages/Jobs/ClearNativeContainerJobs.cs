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
}