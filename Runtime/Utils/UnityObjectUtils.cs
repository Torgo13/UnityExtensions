using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityObject = UnityEngine.Object;

namespace UnityExtensions
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
        public static void RemoveDestroyedObjects<T>(List<T> list) where T : UnityObject
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
        }

        /// <summary>
        /// Removes any destroyed keys from a dictionary that uses UnityObjects as its key type.
        /// </summary>
        /// <typeparam name="TKey">The specific type of UnityObject serving as keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The value type of the dictionary.</typeparam>
        /// <param name="dictionary">A dictionary of UnityObjects that may contain destroyed objects.</param>
        public static void RemoveDestroyedKeys<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
            where TKey : UnityObject
        {
            var removeList = ListPool<TKey>.Get();
            removeList.EnsureCapacity(dictionary.Count);
            
            foreach (var kvp in dictionary)
            {
                var key = kvp.Key;
                if (key == null)
                    removeList.Add(key);
            }

            foreach (var key in removeList)
            {
                dictionary.Remove(key);
            }

            ListPool<TKey>.Release(removeList);
        }
        #endregion // Unity.XR.CoreUtils
    }
}