#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityObject = UnityEngine.Object;

namespace PKGE
{
    /// <summary>
    /// Utility methods for working with UnityEngine <see cref="Object"/> types.
    /// </summary>
    public static class UnityObjectUtils
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/UnityObjectUtils.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Removes any destroyed UnityObjects from a list.
        /// </summary>
        /// <typeparam name="T">The specific type of UnityObject in the dictionary.</typeparam>
        /// <param name="list">A list of UnityObjects that may contain destroyed objects.</param>
        public static List<T> RemoveDestroyedObjects<T>(this List<T?> list) where T : UnityObject
        {
            var nonNull = ListPool<T>.Get();
            nonNull.EnsureCapacity(list.Count);
            
            foreach (var component in list)
            {
                if (component != null)
                    nonNull.Add(component);
            }
            
            list.Clear();
            list.AddRange(nonNull);
            ListPool<T>.Release(nonNull);

            return (List<T>)list!;
        }

        /// <summary>
        /// Removes any destroyed keys from a dictionary that uses UnityObjects as its key type.
        /// </summary>
        /// <typeparam name="TKey">The specific type of UnityObject serving as keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The value type of the dictionary.</typeparam>
        /// <param name="dictionary">A dictionary of UnityObjects that may contain destroyed objects.</param>
        public static Dictionary<TKey, TValue> RemoveDestroyedKeys<TKey, TValue>(this Dictionary<TKey?, TValue> dictionary)
            where TKey : UnityObject
        {
            var keepList = ListPool<(TKey, TValue)>.Get();
            keepList.EnsureCapacity(dictionary.Count);
            
            foreach (var kvp in dictionary)
            {
                TKey? key = kvp.Key;
                if (key != null)
                    keepList.Add((key, kvp.Value));
            }

            dictionary.Clear();
            foreach (var key in keepList)
            {
                dictionary.Add(key.Item1, key.Item2);
            }

            ListPool<(TKey, TValue)>.Release(keepList);

            return (Dictionary<TKey, TValue>)dictionary!;
        }
        #endregion // Unity.XR.CoreUtils
    }
}