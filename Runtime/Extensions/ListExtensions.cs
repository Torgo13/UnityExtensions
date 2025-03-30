using System.Collections.Generic;

namespace UnityExtensions
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
            (list[first], list[second]) = (list[second], list[first]);
        }
        #endregion // Unity.XR.CoreUtils
        
        public static void EnsureRoom<T>(this List<T> list, int room)
        {
            var capacity = list.Count + room;
            if (list.Capacity < capacity)
                list.Capacity = capacity;
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
        public static bool RemoveSwapBack<T>(this List<T> list, System.Predicate<T> matcher)
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
    }
}
