#nullable enable
#if INCLUDE_COLLECTIONS
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine.Assertions;

namespace PKGE
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
        public static void AddRange<T>(this NativeList<T> nativeList, ReadOnlySpan<T> span) where T : unmanaged
        {
            Assert.IsTrue(nativeList.IsCreated);

            int start = nativeList.Length;
            nativeList.ResizeUninitialized(start + span.Length);
            span.CopyTo(nativeList.AsSpan().Slice(start, span.Length));
        }

        /// <exception cref="ArgumentOutOfRangeException">Thrown if count is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this NativeList<T> nativeList, T[] array, int count) where T : unmanaged
        {
            Assert.IsTrue(count <= array.Length);

            nativeList.AddRange(array.AsSpan(start: 0, length: count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this NativeList<T> nativeList, System.Collections.Generic.List<T> list) where T : unmanaged
        {
            nativeList.AddRange(list.AsSpan());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRangeNoResize<T>(this NativeList<T> nativeList, ReadOnlySpan<T> span) where T : unmanaged
        {
            Assert.IsTrue(nativeList.IsCreated);

            if (nativeList.Capacity < nativeList.Length + span.Length)
                return;

            nativeList.AddRange(span);
        }

        //https://github.com/Unity-Technologies/Graphics/blob/2ecb711df890ca21a0817cf610ec21c500cb4bfe/Packages/com.unity.render-pipelines.universal/Runtime/UniversalRenderPipelineCore.cs
        #region UnityEngine.Rendering.Universal
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly T UnsafeElementAt<T>(this NativeList<T> nativeList, int index) where T : unmanaged
        {
            return ref nativeList.UnsafeElementAtMutable(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T UnsafeElementAtMutable<T>(this NativeList<T> nativeList, int index) where T : unmanaged
        {
            Assert.IsTrue(nativeList.IsCreated);
            Assert.IsTrue(index < nativeList.Capacity);

            if (index >= nativeList.Length)
                nativeList.ResizeUninitialized(1 + index);

            return ref nativeList.ElementAt(index);
        }
        #endregion // UnityEngine.Rendering.Universal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly T AsRefReadonly<T>(this NativeList<T> nativeList) where T : unmanaged
        {
            return ref nativeList.AsRef();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(this NativeList<T> nativeList) where T : unmanaged
        {
            Assert.IsFalse(nativeList.IsEmpty);

            return ref nativeList.ElementAt(0);
        }
    }
}
#endif // INCLUDE_COLLECTIONS
