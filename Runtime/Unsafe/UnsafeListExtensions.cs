using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe
{
    public static class UnsafeListExtensions
    {
        //https://github.com/needle-mirror/com.unity.entities.graphics/blob/master/Unity.Entities.Graphics/EntitiesGraphicsCulling.cs
        #region CullingExtensions
        public static unsafe NativeArray<T> AsNativeArray<T>(this UnsafeList<T> list) where T : unmanaged
        {
            var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(list.Ptr, list.Length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
            return array;
        }

        public static NativeArray<T> GetSubNativeArray<T>(ref this UnsafeList<T> list, int start, int length)
            where T : unmanaged =>
            list.AsNativeArray().GetSubArray(start, length);
        #endregion // CullingExtensions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRange<T>(ref this UnsafeList<T> list, T[] array) where T : unmanaged
        {
            Assert.IsNotNull(array);
            
            fixed (T* p = array)
            {
                list.AddRange(p, array.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRange<T>(ref this UnsafeList<T> list, T[] array, int count) where T : unmanaged
        {
            Assert.IsNotNull(array);
            Assert.IsTrue(count >= 0 && count <= array.Length);
            
            fixed (T* p = array)
            {
                list.AddRange(p, count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCapacity<T>(ref this UnsafeList<T> list, int capacity) where T : unmanaged
        {
            if (list.Capacity < capacity)
                list.Capacity = capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureRoom<T>(ref this UnsafeList<T> list, int room) where T : unmanaged
        {
            var capacity = list.Length + room;
            if (list.Capacity < capacity)
                list.Capacity = capacity;
        }
    }
}
