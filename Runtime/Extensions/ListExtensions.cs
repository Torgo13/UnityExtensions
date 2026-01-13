using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Assertions;

#if INCLUDE_COLLECTIONS
using Unity.Collections;
#endif // INCLUDE_COLLECTIONS

namespace PKGE
{
    /// <summary>
    /// Extension methods for <see cref="List{T}"/> objects.
    /// </summary>
    public static class ListExtensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Extensions/ListExtensions.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Fills the list with default objects of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="list">The list to populate.</param>
        /// <param name="count">The number of items to add to the list.</param>
        /// <typeparam name="T">The type of objects in this list.</typeparam>
        /// <returns>The list that was filled.</returns>
        public static List<T> Fill<T>(this List<T> list, int count)
            where T : new()
        {
            list.EnsureRoom(count);

            for (var i = 0; i < count; i++)
            {
                list.Add(new T());
            }

            return list;
        }

        /// <summary>
        /// Ensures that the capacity of this list is at least as large the given value.
        /// </summary>
        /// <remarks>Increases the capacity of the list, if necessary, but does not decrease the
        /// capacity if it already exceeds the specified value.</remarks>
        /// <typeparam name="T">The list element type.</typeparam>
        /// <param name="list">The list whose capacity will be ensured.</param>
        /// <param name="capacity">The minimum number of elements the list storage must contain.</param>
        public static void EnsureCapacity<T>(this List<T> list, int capacity)
        {
            if (list.Capacity < capacity)
                list.Capacity = capacity;
        }

        /// <summary>
        /// Swaps the elements at <paramref name="first"/> and <paramref name="second"/> with minimal copying.
        /// Works for any type of <see cref="List{T}"/>.
        /// </summary>
        /// <param name="list">The list to perform the swap on.</param>
        /// <param name="first">The index of the first item to swap.</param>
        /// <param name="second">The index of the second item to swap.</param>
        /// <typeparam name="T">The type of list items to be swapped.</typeparam>
        public static void SwapAtIndices<T>(this List<T> list, int first, int second)
        {
            UnityEngine.Assertions.Assert.IsTrue(first >= 0);
            UnityEngine.Assertions.Assert.IsTrue(second >= 0);
            UnityEngine.Assertions.Assert.IsTrue(first < list.Count);
            UnityEngine.Assertions.Assert.IsTrue(second < list.Count);

            (list[first], list[second]) = (list[second], list[first]);
        }
        #endregion // Unity.XR.CoreUtils
        
        public static void EnsureRoom<T>(this List<T> list, int room)
        {
            var capacity = list.Count + room;
            if (list.Capacity < capacity)
                list.Capacity = capacity;
        }

        public static void RemoveNull<T>(this List<T> list)
            where T : class
        {
            var temp = UnityEngine.Pool.ListPool<T>.Get();
            temp.EnsureCapacity(list.Count);

            foreach (var item in list)
            {
                if (item != null)
                    temp.Add(item);
            }

            list.Clear();
            list.AddRange(temp);
            UnityEngine.Pool.ListPool<T>.Release(temp);
        }
        
        //https://github.com/needle-mirror/com.unity.collections/blob/feee1d82af454e1023e3e04789fce4d30fc1d938/Unity.Collections/ListExtensions.cs
        #region Unity.Collections
        /// <summary>
        /// Finds and removes the first occurrence of a particular value in the list.
        /// </summary>
        /// <remarks>
        /// If found, the first occurrence of the value is overwritten by the last element of the list, and the list's length is decremented by one.
        /// </remarks>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to search.</param>
        /// <param name="value">The value to locate and remove.</param>
        /// <returns>Returns true if an element was removed.</returns>
        public static bool RemoveSwapBack<T>(this List<T> list, T value)
        {
            int index = list.IndexOf(value);
            if (index < 0)
                return false;

            RemoveAtSwapBack(list, index);
            return true;
        }

        /// <summary>
        /// Finds and removes the first value which satisfies a predicate.
        /// </summary>
        /// <remarks>
        /// The first value satisfying the predicate is overwritten by the last element of the list, and the list's length is decremented by one.
        /// </remarks>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to search.</param>
        /// <param name="matcher">The predicate for testing the elements of the list.</param>
        /// <returns>Returns true if an element was removed.</returns>
        public static bool RemoveSwapBack<T>(this List<T> list, Predicate<T> matcher)
        {
            int index = list.FindIndex(matcher);
            if (index < 0)
                return false;

            RemoveAtSwapBack(list, index);
            return true;
        }

        /// <summary>
        /// Removes the value at an index.
        /// </summary>
        /// <remarks>
        /// The value at the index is overwritten by the last element of the list, and the list's length is decremented by one.
        /// </remarks>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to search.</param>
        /// <param name="index">The index at which to remove an element from the list.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        public static void RemoveAtSwapBack<T>(this List<T> list, int index)
        {
            int lastIndex = list.Count - 1;
            list[index] = list[lastIndex];
            list.RemoveAt(lastIndex);
        }
        #endregion // Unity.Collections

        //https://github.com/needle-mirror/com.unity.entities/blob/1.3.9/Unity.Entities.CodeGen/ListExtensions.cs
        #region Unity.Entities.CodeGen
        public static void Add<T>(this List<T> list, IEnumerable<T> elementsToAdd) => list.AddRange(elementsToAdd);
        #endregion // Unity.Entities.CodeGen

        //https://github.com/Unity-Technologies/UnityCsReference/blob/b1cf2a8251cce56190f455419eaa5513d5c8f609/Runtime/Export/Unsafe/UnsafeUtility.cs
        #region Unity.Collections.LowLevel.Unsafe
        public static Span<byte> AsBytes<T>(this List<T> list) where T : struct
        {
            return MemoryMarshal.AsBytes(list.AsSpan());
        }
        #endregion // Unity.Collections.LowLevel.Unsafe

        public static Span<TTo> Cast<TFrom, TTo>(this List<TFrom> list)
            where TFrom : struct
            where TTo : struct
        {
            UnityEngine.Assertions.Assert.IsTrue(
                list.Count * SizeOfCache<TFrom>.Size
                % SizeOfCache<TTo>.Size == 0);

            return MemoryMarshal.Cast<TFrom, TTo>(list.AsSpan());
        }

        public static T[] AsArray<T>(this List<T> list)
        {
            return NoAllocHelpers.ExtractArrayFromList(list);
        }

        public static Span<T> AsSpan<T>(this List<T> list)
        {
            if (list == null)
                return new Span<T>();

            return list.AsArray().AsSpan(start: 0, length: list.Count);
        }

        public static Span<T> AsSpan<T>(this List<T> list, int start, int length)
        {
            if (list == null)
                return new Span<T>();

            return list.AsArray().AsSpan(start, length);
        }

        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this List<T> list)
        {
            return list.AsSpan();
        }

        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this List<T> list, int start, int length)
        {
            return list.AsSpan(start, length);
        }

        /// <inheritdoc cref="Dictionary{TKey, TValue}.TryAdd(TKey, TValue)"/>
        public static bool TryAdd<T>(this List<T> list, T element)
        {
            if (!list.Contains(element))
            {
                list.Add(element);
                return true;
            }

            return false;
        }

        public static List<T> Remove<T>(this List<T> list, List<T> remove)
        {
            using var _0 = UnityEngine.Pool.ListPool<T>.Get(out var temp);
            temp.EnsureCapacity(list.Count);

            foreach (var item in list)
            {
                if (!remove.Contains(item))
                    temp.Add(item);
            }

            list.Clear();
            list.AddRange(temp);
            return list;
        }

        public static List<T> As<U, T>(this List<U> list)
        {
            Assert.AreEqual(SizeOfCache<T>.Size, SizeOfCache<U>.Size);

            return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<List<U>, List<T>>(ref list);
        }

        public static ref T AsRef<T>(this List<T> list, int index) where T : struct
        {
            Assert.IsTrue(index >= 0);
            Assert.IsTrue(index < list.Count);

            return ref NoAllocHelpers.ExtractArrayFromList(list)[index];
        }

#if INCLUDE_COLLECTIONS
        //https://github.com/needle-mirror/com.unity.collections/blob/feee1d82af454e1023e3e04789fce4d30fc1d938/Unity.Collections/ListExtensions.cs
        #region Unity.Collections
        /// <summary>
        /// Returns a <see cref="Unity.Collections.NativeList{T}"/> that is a copy of this <see cref="System.Collections.Generic.List{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to copy.</param>
        /// <param name="allocator">The allocator to use.</param>
        /// <returns>A copy of this list.</returns>
        public static NativeList<T> ToNativeList<T>(this List<T> list,
            AllocatorManager.AllocatorHandle allocator) where T : unmanaged
        {
            var count = list.Count;
            var container = new NativeList<T>(count, allocator);
            container.ResizeUninitialized(count);
            list.AsSpan(start: 0, length: count).CopyTo(container.AsArray().AsSpan());
            container.Length = count;

            return container;
        }

        /// <summary>
        /// Returns a <see cref="Unity.Collections.NativeArray{T}"/> that is a copy of this <see cref="System.Collections.Generic.List{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to copy.</param>
        /// <param name="allocator">The allocator to use.</param>
        /// <returns>An array that is a copy of this list.</returns>
        public static NativeArray<T> ToNativeArray<T>(this List<T> list,
            AllocatorManager.AllocatorHandle allocator) where T : unmanaged
        {
            var container = CollectionHelper.CreateNativeArray<T>(list.Count, allocator, NativeArrayOptions.UninitializedMemory);
            list.AsSpan().CopyTo(container.AsSpan());

            return container;
        }
        #endregion // Unity.Collections

        public static void ResetListContents<T>(this List<T> list, NativeList<T> nativeList)
            where T : unmanaged
        {
            NoAllocHelpers.ResetListContents(list, nativeList.AsReadOnly().AsReadOnlySpan());
        }
#endif // INCLUDE_COLLECTIONS
    }
}
