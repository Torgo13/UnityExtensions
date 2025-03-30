// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Unity.Collections;
using UnityEngine.Pool;

namespace UnityExtensions
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
        
        //https://github.com/Unity-Technologies/com.unity.formats.alembic/blob/3d486c22f22d65278f910f0835128afdb8f2a36e/com.unity.formats.alembic/Runtime/Scripts/Misc/RuntimeUtils.cs

        #region UnityEngine.Formats.Alembic.Importer
        public static void DisposeIfPossible<T>(this ref NativeArray<T> array) where T : struct
        {
            if (array.IsCreated)
            {
                array.Dispose();
            }
        }

        public static NativeArray<T> ResizeIfNeeded<T>(this ref NativeArray<T> array, int newLength, Allocator a = Allocator.Persistent) where T : struct
        {
            if (array.Length != newLength)
            {
                array.DisposeIfPossible();
                array = new NativeArray<T>(newLength, a);
            }

            // The array is either created and of the right size, or not created, and we are asking to resize to 0
            if (!array.IsCreated)
            {
                array = new NativeArray<T>(0, a);
            }

            return array;
        }
        #endregion // UnityEngine.Formats.Alembic.Importer
        
#if !NETSTANDARD2_1
        //https://github.com/dotnet/runtime/blob/7eb07dde40933cace91d57aae0a3e569fd042def/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/CollectionExtensions.cs
        #region System.Collections.Generic
        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) =>
            dictionary.GetValueOrDefault(key, default!);

#nullable enable
        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary is null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            return dictionary.TryGetValue(key, out TValue? value) ? value : defaultValue;
        }
#nullable disable

        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary is null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }

            return false;
        }

        public static bool Remove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            if (dictionary is null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (dictionary.TryGetValue(key, out value))
            {
                dictionary.Remove(key);
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Returns a read-only <see cref="ReadOnlyCollection{T}"/> wrapper
        /// for the specified list.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="list">The list to wrap.</param>
        /// <returns>An object that acts as a read-only wrapper around the current <see cref="IList{T}"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
        public static ReadOnlyCollection<T> AsReadOnly<T>(this IList<T> list) =>
            new ReadOnlyCollection<T>(list);

        /// <summary>
        /// Returns a read-only <see cref="ReadOnlyDictionary{TKey, TValue}"/> wrapper
        /// for the current dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to wrap.</param>
        /// <returns>An object that acts as a read-only wrapper around the current <see cref="IDictionary{TKey, TValue}"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is null.</exception>
        public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) where TKey : notnull =>
            new ReadOnlyDictionary<TKey, TValue>(dictionary);

        /// <summary>Adds the elements of the specified span to the end of the <see cref="List{T}"/>.</summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to which the elements should be added.</param>
        /// <param name="source">The span whose elements should be added to the end of the <see cref="List{T}"/>.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="list"/> is null.</exception>
        public static void AddRange<T>(this List<T> list, ReadOnlySpan<T> source)
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (!source.IsEmpty)
            {
                list.EnsureCapacity(source.Length);

                foreach (var item in source)
                    list.Add(item);
            }
        }

        /// <summary>Copies the entire <see cref="List{T}"/> to a span.</summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list from which the elements are copied.</param>
        /// <param name="destination">The span that is the destination of the elements copied from <paramref name="list"/>.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="list"/> is null.</exception>
        /// <exception cref="ArgumentException">The number of elements in the source <see cref="List{T}"/> is greater than the number of elements that the destination span can contain.</exception>
        public static void CopyTo<T>(this List<T> list, Span<T> destination)
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            for (int i = 0; i < list.Count; i++)
                destination[i] = list[i];
        }
        #endregion // System.Collections.Generic
#endif
    }
}
