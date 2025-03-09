using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe
{
    public static class MemoryUtilities
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/GPUDriven/Utilities/MemoryUtilities.cs
        #region UnityEngine.Rendering
        public static unsafe T* Malloc<T>(int count, Allocator allocator) where T : unmanaged
        {
            return (T*)Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Malloc(
                Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<T>() * count,
                Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AlignOf<T>(),
                allocator);
        }

        public static unsafe void Free<T>(T* p, Allocator allocator) where T : unmanaged
        {
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Free(p, allocator);
        }
        #endregion // UnityEngine.Rendering
    }
}
