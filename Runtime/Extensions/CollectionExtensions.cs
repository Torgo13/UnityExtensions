// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Collections;
using UnityEngine.Pool;

namespace PKGE
{
    /// <summary>
    /// Extension methods for <see cref="ICollection{T}"/> objects.
    /// </summary>
    public static class CollectionExtensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Extensions/CollectionExtensions.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Creates a comma separated string of all elements in the collection. Each collection element is implicitly converted
        /// to a string and added to the list.
        /// </summary>
        /// <param name="collection">The collection to create a string from.</param>
        /// <typeparam name="T">The type of objects in the collection.</typeparam>
        /// <returns>A string with all elements in the collection converted to strings and separated by commas.</returns>
        public static string Stringify<T>(this ICollection<T> collection)
        {
            using var _0 = StringBuilderPool.Get(out var sb);
            var endIndex = collection.Count - 1;
            var counter = 0;
            foreach (var t in collection)
            {
                if (counter++ == endIndex)
                {
                    sb.Append(t);
                }
                else
                {
                    sb.Append(t).Append(',').Append(' ');
                }
            }

            return sb.ToString();
        }
        #endregion // Unity.XR.CoreUtils

        //https://github.com/Unity-Technologies/UnityCsReference/blob/6000.1/Runtime/Export/Collections/CollectionExtensions.cs
        #region Unity.Collections
        /// <summary>
        /// Add an element to the correct position in presorted List.
        /// These methods remove the need to call Sort() on the List.
        /// </summary>
        /// <param name="list">Presorted List</param>
        /// <param name="item">Element to add</param>
        /// <param name="comparer">Comparator if Comparer&lt;T&gt;. Default is not suite</param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ArgumentNullException">Can throw exception if list is null</exception>
        public static void AddSorted<T>([DisallowNull] this List<T> list, T item, IComparer<T> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list), $"{nameof(list)} must not be null.");

            comparer ??= Comparer<T>.Default;

            //No elements in the list yet
            if (list.Count == 0)
            {
                list.Add(item);
                return;
            }

            if (comparer.Compare(list[^1], item) <= 0)
            {
                list.Add(item);
                return;
            }

            if (comparer.Compare(list[0], item) >= 0)
            {
                list.Insert(0, item);
                return;
            }

            var index = list.BinarySearch(item, comparer);
            if (index < 0)
                index = ~index;
            list.Insert(index, item);
        }

        /// <summary>
        /// Adds <paramref name="count"/> elements <paramref name="value"/> to the list.
        /// </summary>
        /// <param name="dest">The list to modify.</param>
        /// <param name="value">The value to add.</param>
        /// <param name="count">The number of values to add.</param>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dest"/> is null.</exception>
        public static void Fill<T>([DisallowNull] this List<T> dest, T value, int count)
        {
            if (dest == null)
            {
                throw new ArgumentNullException(nameof(dest));
            }

            dest.Capacity = Math.Max(dest.Capacity, dest.Count + count);
            while (count-- > 0)
            {
                dest.Add(value);
            }
        }

        /// <summary>
        /// Returns the first element of collection sorted by comparer.
        /// This method removes the need to call Sort() and FirstOrDefault() on the List.
        /// </summary>
        /// <param name="collection">Collection to inspect</param>
        /// <param name="comparer">Comparator if Comparer&lt;T&gt;. Default is not suite</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>The first element if the list was sorted or default</returns>
        /// <exception cref="ArgumentNullException">Can throw exception if list is null</exception>
        public static T FirstOrDefaultSorted<T>(this IEnumerable<T> collection, IComparer<T> comparer = null)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection), $"{nameof(collection)} must not be null.");

            comparer ??= Comparer<T>.Default;

            var firstAssignment = false;
            T element = default;
            foreach (var e in collection)
            {
                if (!firstAssignment)
                {
                    element = e;
                    firstAssignment = true;
                }

                if (comparer.Compare(e, element) < 0)
                    element = e;
            }

            return element;
        }

        /// <summary>
        /// Returns collections as a string. This can be useful for debug collections in Debug.Log.
        /// String is also Json compatible. It uses the [ , , ] convention.
        /// </summary>
        /// <param name="collection">Serializable collection</param>
        /// <param name="serializeElement">Function to serialize element of collection</param>
        /// <typeparam name="T">Collection type</typeparam>
        /// <returns>Serialized collection</returns>
        /// <exception cref="ArgumentNullException">Can produce exception if collection or serialize method is null</exception>
        public static string SerializedView<T>([DisallowNull] this IEnumerable<T> collection, [DisallowNull] Func<T, string> serializeElement)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection), $"{nameof(collection)} must not be null.");

            if (serializeElement == null)
                throw new ArgumentNullException(nameof(serializeElement), $"Argument {nameof(serializeElement)} must not be null.");

            using var _0 = StringBuilderPool.Get(out var sb);
            sb.Append('[');
            foreach (var t in collection)
            {
                sb.Append(t == null ? "null" : serializeElement(t));
                sb.Append(',');
            }

            // Remove last comma
            sb.Remove(sb.Length - 1, 1);
            
            sb.Append(']');
            return sb.ToString();
        }

        /// <summary>
        /// Check if element is in collection.
        /// This method replaces the Linq implementation to reduce GC allocations.
        /// </summary>
        /// <param name="collection">Collection to inspect</param>
        /// <param name="element">Element to find</param>
        /// <typeparam name="T">Collection type</typeparam>
        /// <returns>True if element found and False if not</returns>
        /// <exception cref="ArgumentNullException">Can produce exception if collection is null</exception>
        public static bool ContainsByEquals<T>([DisallowNull] this IEnumerable<T> collection, T element)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection), $"{nameof(collection)} must not be null.");

            foreach (var e in collection)
            {
                if (e.Equals(element))
                    return true;
            }

            return false;
        }
        #endregion // Unity.Collections
    }
}
