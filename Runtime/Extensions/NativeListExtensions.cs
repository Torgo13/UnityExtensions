#if INCLUDE_COLLECTIONS
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine.Assertions;

namespace PKGE.Packages
{
    public static class NativeListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCapacity<T>(this NativeList<T> nativeList, int capacity) where T : unmanaged
        {
            Assert.IsTrue(nativeList.IsCreated);
            Assert.IsTrue(capacity > 0);

            if (nativeList.Capacity < capacity)
                nativeList.Capacity = capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureRoom<T>(this NativeList<T> nativeList, int room) where T : unmanaged
        {
            Assert.IsTrue(nativeList.IsCreated);
            Assert.IsTrue(room > 0);

            var capacity = nativeList.Length + room;
            if (nativeList.Capacity < capacity)
                nativeList.Capacity = capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this NativeList<T> nativeList) where T : unmanaged
        {
            return nativeList.AsArray().AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this NativeList<T> nativeList) where T : unmanaged
        {
            return nativeList.AsReadOnly().AsReadOnlySpan();
        }

        /// <exception cref="ArgumentOutOfRangeException">Thrown if count is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this NativeList<T> nativeList, T[] array, int count) where T : unmanaged
        {
            Assert.IsTrue(nativeList.IsCreated);
            Assert.IsTrue(count >= 0);
            Assert.IsTrue(count <= array.Length);

            int start = nativeList.Length;
            nativeList.ResizeUninitialized(start + count);
            array.AsSpan(0, count).CopyTo(nativeList.AsSpan().Slice(start, count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this NativeList<T> nativeList, T[] array) where T : unmanaged
        {
            nativeList.AddRange(array, array.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this NativeList<T> nativeList, System.Collections.Generic.List<T> list) where T : unmanaged
        {
            nativeList.AddRange(NoAllocHelpers.ExtractArrayFromList(list), list.Count);
        }
    }
}
#endif // INCLUDE_COLLECTIONS
