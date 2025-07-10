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
        static readonly ComponentMap<TKey, TValue> Default = new ComponentMap<TKey, TValue>();
        public static ComponentMap<TKey, TValue> Instance => Default;

        readonly Dictionary<TKey, TValue> _keyToValueMap = new Dictionary<TKey, TValue>();
        readonly Dictionary<TValue, TKey> _valueToKeyMap = new Dictionary<TValue, TKey>();

        void Add(TKey key, TValue value)
        {
            _keyToValueMap.Add(key, value);
            _valueToKeyMap.Add(value, key);
        }

        void Remove(TKey key)
        {
            if (_keyToValueMap.Remove(key, out var value))
            {
                _ = _valueToKeyMap.Remove(value);
            }
        }

        void Remove(TValue value)
        {
            if (_valueToKeyMap.TryGetValue(value, out var key))
            {
                _ = _keyToValueMap.Remove(key);
                _ = _valueToKeyMap.Remove(value);
            }
        }

        void UpdateMap(TKey key, TValue instance)
        {
            Assert.IsNotNull(key);
            Assert.IsNotNull(instance);

            // In case the key exists already, check if the instance should be updated.
            if (_keyToValueMap.TryGetValue(key, out var existingValue))
            {
                // No modification needed.
                if (instance.Equals(existingValue))
                    return;

                Remove(existingValue);
            }

            // In case the instance was already registered, check if the key should be updated.
            if (_valueToKeyMap.TryGetValue(instance, out var existingKey))
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
        /// <returns>Indicates whether a corresponding value component was found.</returns>
        public bool TryGetInstance(TKey key, out TValue instance)
        {
            return _keyToValueMap.TryGetValue(key, out instance);
        }
        #endregion // Unity.LiveCapture.VirtualCamera
    }
}
