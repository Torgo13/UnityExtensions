using System;
using System.Collections.Generic;
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

        /// <exception cref="ArgumentOutOfRangeException">Thrown if count is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRange<T>(ref this UnsafeList<T> unsafeList, T[] array, int count) where T : unmanaged
        {
            Assert.IsTrue(unsafeList.IsCreated);
            Assert.IsNotNull(array);
            Assert.IsTrue(count >= 0);
            Assert.IsTrue(count <= array.Length);

            fixed (T* p = array)
            {
                unsafeList.AddRange(p, count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRange<T>(ref this UnsafeList<T> unsafeList, T[] array) where T : unmanaged
        {
            Assert.IsNotNull(array);

            unsafeList.AddRange(array, array.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(ref this UnsafeList<T> unsafeList, List<T> list) where T : unmanaged
        {
            Assert.IsNotNull(list);

            unsafeList.AddRange(NoAllocHelpers.ExtractArrayFromList(list), list.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCapacity<T>(ref this UnsafeList<T> unsafeList, int capacity) where T : unmanaged
        {
            Assert.IsTrue(unsafeList.IsCreated);
            Assert.IsTrue(capacity > 0);

            if (unsafeList.Capacity < capacity)
                unsafeList.Capacity = capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureRoom<T>(ref this UnsafeList<T> unsafeList, int room) where T : unmanaged
        {
            Assert.IsTrue(unsafeList.IsCreated);
            Assert.IsTrue(room > 0);

            var capacity = unsafeList.Length + room;
            if (unsafeList.Capacity < capacity)
                unsafeList.Capacity = capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<T> AsSpan<T>(this UnsafeList<T> unsafeList) where T : unmanaged
        {
            Assert.IsTrue(unsafeList.IsCreated);

            return new Span<T>(unsafeList.Ptr, unsafeList.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this UnsafeList<T> unsafeList) where T : unmanaged
        {
            Assert.IsTrue(unsafeList.IsCreated);

            return new ReadOnlySpan<T>(unsafeList.Ptr, unsafeList.Length);
        }
    }
}
