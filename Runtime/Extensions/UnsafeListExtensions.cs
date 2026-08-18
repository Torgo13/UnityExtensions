#if INCLUDE_COLLECTIONS
using System;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using Unity.Collections.LowLevel.Unsafe;

namespace TCGE
{
    public static class UnsafeListExtensions
    {
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
        public static ref readonly T UnsafeElementAt<T>(ref this UnsafeList<T> unsafeList, int index) where T : unmanaged
        {
            return ref unsafeList.UnsafeElementAtMutable(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T UnsafeElementAtMutable<T>(ref this UnsafeList<T> unsafeList, int index) where T : unmanaged
        {
            Assert.IsTrue(unsafeList.IsCreated);
            Assert.IsTrue(index >= 0);
            Assert.IsTrue(index < unsafeList.Capacity);

            if (index >= unsafeList.Length)
                unsafeList.Length = 1 + index;

            return ref unsafeList.ElementAt(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly T AsRefReadonly<T>(ref this UnsafeList<T> unsafeList) where T : unmanaged
        {
            return ref unsafeList.AsRef();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(ref this UnsafeList<T> unsafeList) where T : unmanaged
        {
            Assert.IsFalse(unsafeList.IsEmpty);

            return ref unsafeList.ElementAt(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(ref this UnsafeList<T> unsafeList, System.Collections.Generic.List<T> list) where T : unmanaged
        {
            unsafeList.AddRange(PKGE.ListExtensions.AsReadOnlySpan(list));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(ref this UnsafeList<T> unsafeList, ReadOnlySpan<T> span) where T : unmanaged
        {
            Assert.IsTrue(unsafeList.IsCreated);

            if (span == default || span.Length == 0)
                return;

            int length = unsafeList.Length;
            unsafeList.Length = length + span.Length;

            span.CopyTo(unsafeList.AsSpan()[length..]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRangeNoResize<T>(ref this UnsafeList<T> unsafeList, ReadOnlySpan<T> span) where T : unmanaged
        {
            Assert.IsTrue(unsafeList.IsCreated);

            if (span == default || span.Length == 0)
                return;

            if (unsafeList.Capacity < unsafeList.Length + span.Length)
                return;

            unsafeList.AddRange(span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(ref this UnsafeList<T> unsafeList) where T : unmanaged
        {
            return System.Runtime.InteropServices.MemoryMarshal.CreateSpan(
                ref unsafeList.ElementAt(0), unsafeList.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(ref this UnsafeList<T> unsafeList) where T : unmanaged
        {
            return System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(
                ref unsafeList.ElementAt(0), unsafeList.Length);
        }
    }
}
#endif // INCLUDE_COLLECTIONS
