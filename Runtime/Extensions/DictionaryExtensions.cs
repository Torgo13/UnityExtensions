using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PKGE
{
    /// <summary>
    /// Extension methods for <see cref="Dictionary{TKey, TValue}"/> objects.
    /// </summary>
    public static class DictionaryExtensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Extensions/DictionaryExtensions.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Gets the first element in the dictionary.
        /// </summary>
        /// <remarks>
        /// Equivalent to the <see cref="System.Linq"/> `.First()` method, but does not allocate.
        /// </remarks>
        /// <param name="dictionary">Dictionary to retrieve the element from.</param>
        /// <typeparam name="TKey">Dictionary's Key type.</typeparam>
        /// <typeparam name="TValue">Dictionary's Value type.</typeparam>
        /// <returns>The first element in the dictionary.</returns>
        public static KeyValuePair<TKey, TValue> First<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            var kvp = default(KeyValuePair<TKey, TValue>);
            var enumerator = dictionary.GetEnumerator();
            if (enumerator.MoveNext())
            {
                kvp = enumerator.Current;
            }

            enumerator.Dispose();
            return kvp;
        }
        #endregion // Unity.XR.CoreUtils
        
        //https://github.com/Unity-Technologies/UnityCsReference/blob/b1cf2a8251cce56190f455419eaa5513d5c8f609/Modules/UIElements/Core/Collections/DictionaryExtensions.cs
        #region UnityEngine.UIElements.Collections
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key,
            TValue fallbackValue = default(TValue))
        {
            return dict.TryGetValue(key, out var result) ? result : fallbackValue;
        }
        #endregion // UnityEngine.UIElements.Collections
        
        //https://github.com/Unity-Technologies/UnityCsReference/blob/b1cf2a8251cce56190f455419eaa5513d5c8f609/Modules/PackageManagerUI/Editor/Services/Common/DictionaryExtensions.cs
        #region UnityEditor.PackageManager.UI.Internal
        public static T Get<T>(this IDictionary<string, object> dict, string key, T fallbackValue = default)
        {
            if (key == null)
                return fallbackValue;

            var result = dict.TryGetValue(key, out var value);
            try
            {
                return result ? (T)value : fallbackValue;
            }
            catch (InvalidCastException)
            {
                throw new FieldAccessException(key); //(key, typeof(T), value.GetType());
            }
        }

        public static T Get<T>(this IDictionary<long, T> dict, long key)
        {
            return dict.TryGetValue(key, out var result) ? result : default;
        }

        public static T Get<T>(this IDictionary<string, T> dict, string key, T fallbackValue = default)
        {
            return key != null && dict.TryGetValue(key, out var result) ? result : fallbackValue;
        }

        public static IDictionary<string, object> GetDictionary(this IDictionary<string, object> dict, string key)
        {
            return Get<IDictionary<string, object>>(dict, key);
        }

        public static IEnumerable<T> GetList<T>(this IDictionary<string, object> dict, string key)
        {
#if USING_LINQ
            return Get<IList>(dict, key)?.OfType<T>();
#else
            return Get<IList>(dict, key) as IEnumerable<T>;
#endif // USING_LINQ
        }

        public static string GetString(this IDictionary<string, object> dict, string key)
        {
            return Get<string>(dict, key);
        }

        public static long GetStringAsLong(this IDictionary<string, object> dict, string key, long fallbackValue = default(long))
        {
            var stringValue = Get<string>(dict, key);
            return long.TryParse(stringValue, out var result) ? result : fallbackValue;
        }
        #endregion // UnityEditor.PackageManager.UI.Internal
        
        //https://github.com/needle-mirror/com.unity.film-internal-utilities/blob/2cfc425a6f0bf909732b9ca80f2385ea3ff92850/Runtime/Scripts/Extensions/DictionaryExtensions.cs
        #region Unity.FilmInternalUtilities
        public static void Loop<K, V>(this Dictionary<K, V> collection, System.Action<K, V> eachAction)
        {
            using var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var kv = enumerator.Current;
                eachAction(kv.Key, kv.Value);
            }
        }
        #endregion // Unity.FilmInternalUtilities

        public static Dictionary<K, V> RemoveKeys<K, V>(this Dictionary<K, V> dictionary, List<K> remove)
        {
            using var _0 = UnityEngine.Pool.DictionaryPool<K, V>.Get(out var temp);

            foreach (var item in dictionary)
            {
                if (!remove.Contains(item.Key))
                    temp.Add(item.Key, item.Value);
            }

            dictionary.Clear();

            foreach (var item in temp)
            {
                dictionary.Add(item.Key, item.Value);
            }

            return dictionary;
        }

        public static Dictionary<K, V> RemoveValues<K, V>(this Dictionary<K, V> dictionary, List<V> remove)
        {
            using var _0 = UnityEngine.Pool.DictionaryPool<K, V>.Get(out var temp);

            foreach (var item in dictionary)
            {
                if (!remove.Contains(item.Value))
                    temp.Add(item.Key, item.Value);
            }

            dictionary.Clear();

            foreach (var item in temp)
            {
                dictionary.Add(item.Key, item.Value);
            }

            return dictionary;
        }
    }
}
