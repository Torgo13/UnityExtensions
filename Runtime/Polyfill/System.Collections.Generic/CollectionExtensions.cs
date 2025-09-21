#if NETSTANDARD2_1
#else
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace PKGE
{
    /// <summary>
    /// Extension methods for <see cref="ICollection{T}"/> objects.
    /// </summary>
    public static partial class CollectionExtensions
    {
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
    }
}
#endif // NETSTANDARD2_1
