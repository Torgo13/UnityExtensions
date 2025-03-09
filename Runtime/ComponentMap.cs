using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityExtensions
{
    /// <summary>
    /// A map between components that provides a static default instance.
    /// </summary>
    /// <remarks>
    /// For example, this is useful for associating rendering components to a given camera.
    /// </remarks>
    /// <typeparam name="TKey">The type of key component.</typeparam>
    /// <typeparam name="TValue">The type of value component, typically the one extending this base class.</typeparam>
    public class ComponentMap<TKey, TValue>
        where TKey : Component
        where TValue : Component
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/VirtualCamera/Utilities/ComponentMap.cs
        #region Unity.LiveCapture.VirtualCamera
        static ComponentMap<TKey, TValue> s_Default = new ComponentMap<TKey, TValue>();
        public static ComponentMap<TKey, TValue> Instance => s_Default;

        readonly Dictionary<TKey, TValue> s_KeyToValueMap = new Dictionary<TKey, TValue>();
        readonly Dictionary<TValue, TKey> s_ValueToKeyMap = new Dictionary<TValue, TKey>();

        void Add(TKey key, TValue value)
        {
            s_KeyToValueMap.Add(key, value);
            s_ValueToKeyMap.Add(value, key);
        }

        void Remove(TKey key)
        {
            if (s_KeyToValueMap.Remove(key, out var value))
            {
                s_ValueToKeyMap.Remove(value);
            }
        }

        void Remove(TValue value)
        {
            if (s_ValueToKeyMap.TryGetValue(value, out var key))
            {
                s_KeyToValueMap.Remove(key);
                s_ValueToKeyMap.Remove(value);
            }
        }

        void UpdateMap(TKey key, TValue instance)
        {
            Assert.IsNotNull(key);
            Assert.IsNotNull(instance);

            // In case the key exists already, check if the instance should be updated.
            if (s_KeyToValueMap.TryGetValue(key, out var existingValue))
            {
                // No modification needed.
                if (instance.Equals(existingValue))
                    return;

                Remove(existingValue);
            }

            // In case the instance was already registered, check if the key should be updated.
            if (s_ValueToKeyMap.TryGetValue(instance, out var existingKey))
            {
                // No modification needed.
                if (key.Equals(existingKey))
                    return;

                Remove(existingKey);
            }

            Add(key, instance);
        }

        public void RemoveInstance(TValue instance)
        {
            Assert.IsNotNull(instance);
            Remove(instance);
        }

        /// <summary>
        /// Register a component instance (value) associated with another component (key).
        /// </summary>
        /// <param name="key">The key component.</param>
        /// <param name="instance">The value component.</param>
        public void AddUniqueInstance(TKey key, TValue instance)
        {
            Assert.IsNotNull(key);
            Assert.IsNotNull(instance);
            UpdateMap(key, instance);
        }

        /// <summary>
        /// Tries to retrieve the value component associated to the provided key component.
        /// </summary>
        /// <param name="key">The key component.</param>
        /// <param name="instance">The value component.</param>
        /// <returns>Indicates whether or not a corresponding value component was found.</returns>
        public bool TryGetInstance(TKey key, out TValue instance)
        {
            return s_KeyToValueMap.TryGetValue(key, out instance);
        }
        #endregion // Unity.LiveCapture.VirtualCamera
    }
}
