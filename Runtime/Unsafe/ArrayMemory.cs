using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe
{
    public struct ArrayMemory : IDisposable
    {
        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Runtime/Supplementary/Utility/ArrayMemory.cs
        #region Unity.Kinematica
        NativeArray<byte> bytes;

        int requiredSize;
        int usedSize;

        public static ArrayMemory Create()
        {
            return new ArrayMemory()
            {
                requiredSize = 0,
                usedSize = -1
            };
        }

        public void Dispose()
        {
            bytes.Dispose();
        }

        public void Reserve<T>(int numElements) where T : struct
        {
            int elemSize = UnsafeUtility.SizeOf<T>();
            requiredSize += numElements * elemSize;
        }

        public void Allocate(Allocator allocator)
        {
            bytes = new NativeArray<byte>(requiredSize, allocator);
            usedSize = 0;
        }

        public NativeSlice<T> CreateSlice<T>(int length) where T : struct
        {
            if (usedSize < 0)
            {
                throw new Exception("ArraMemory must be allocated before it can create native slices");
            }

            int elemSize = UnsafeUtility.SizeOf<T>();
            int sliceSize = length * elemSize;
            int availableSize = bytes.Length - usedSize;

            if (sliceSize > availableSize)
            {
                throw new Exception($"Memory overflow : trying to create a slice of {sliceSize} bytes whereas there are only {availableSize} bytes available");
            }

            var slice = (new NativeSlice<byte>(bytes, usedSize, sliceSize)).SliceConvert<T>();
            usedSize += sliceSize;

            return slice;
        }
        #endregion // Unity.Kinematica
    }
}
