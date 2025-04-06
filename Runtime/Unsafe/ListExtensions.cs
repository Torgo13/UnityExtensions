using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe
{
    public static class ListExtensions
    {
        //https://github.com/needle-mirror/com.unity.collections/blob/feee1d82af454e1023e3e04789fce4d30fc1d938/Unity.Collections/ListExtensions.cs
        #region Unity.Collections
        /// <summary>
        /// Returns a copy of this list.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to copy.</param>
        /// <param name="allocator">The allocator to use.</param>
        /// <returns>A copy of this list.</returns>
        public static unsafe NativeList<T> ToNativeList<T>(this List<T> list, AllocatorManager.AllocatorHandle allocator) where T : unmanaged
        {
            var container = new NativeList<T>(list.Count, allocator);
            fixed (T* p = NoAllocHelpers.ExtractArrayFromList(list))
            {
                container.AddRangeNoResize(p, list.Count);
            }

            return container;
        }

        /// <summary>
        /// Returns an array that is a copy of this list.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to copy.</param>
        /// <param name="allocator">The allocator to use.</param>
        /// <returns>An array that is a copy of this list.</returns>
        public static unsafe NativeArray<T> ToNativeArray<T>(this List<T> list, AllocatorManager.AllocatorHandle allocator) where T : unmanaged
        {
            var container = CollectionHelper.CreateNativeArray<T>(list.Count, allocator, NativeArrayOptions.UninitializedMemory);
            fixed (T* p = NoAllocHelpers.ExtractArrayFromList(list))
            {
                UnsafeUtility.MemCpy(container.GetUnsafePtr(), p, list.Count * sizeof(T));
            }

            return container;
        }
        #endregion // Unity.Collections

        //https://github.com/Unity-Technologies/UnityCsReference/blob/b1cf2a8251cce56190f455419eaa5513d5c8f609/Runtime/Export/Unsafe/UnsafeUtility.cs
        #region Unity.Collections.LowLevel.Unsafe
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> GetByteSpanFromList<T>(List<T> list) where T : struct
        {
            return MemoryMarshal.AsBytes(list.AsSpan());
        }
        #endregion // Unity.Collections.LowLevel.Unsafe
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this List<T> list)
        {
            return NoAllocHelpers.ExtractArrayFromList(list).AsSpan(0, list.Count);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this List<T> list)
        {
            return list.AsSpan();
        }
    }
}