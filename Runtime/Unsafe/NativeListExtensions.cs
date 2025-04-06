using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe
{
    public static class NativeListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRange<T>(ref this NativeList<T> nativeList, T[] array) where T : unmanaged
        {
            fixed (T* p = array)
            {
                nativeList.AddRange(p, array.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRange<T>(ref this NativeList<T> nativeList, T[] array, int count) where T : unmanaged
        {
            fixed (T* p = array)
            {
                nativeList.AddRange(p, count);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(ref this NativeList<T> nativeList, List<T> list) where T : unmanaged
        {
            nativeList.AddRange(NoAllocHelpers.ExtractArrayFromList(list), list.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<T> AsSpan<T>(this NativeList<T> nativeList) where T : unmanaged
        {
            return new Span<T>(nativeList.GetUnsafePtr(), nativeList.Length * sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this NativeList<T> nativeList) where T : unmanaged
        {
            return new ReadOnlySpan<T>(nativeList.GetUnsafeReadOnlyPtr(), nativeList.Length * sizeof(T));
        }
    }
}
