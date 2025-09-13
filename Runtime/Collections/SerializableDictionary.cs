using System;
using System.Collections.Generic;
using UnityEngine;

namespace PKGE.Collections
{
    /// <summary>
    /// A dictionary class that can be serialized by Unity.
    /// Inspired by the implementation in <see href="http://answers.unity3d.com/answers/809221/view.html"/>
    /// </summary>
    /// <typeparam name="TKey">The dictionary key.</typeparam>
    /// <typeparam name="TValue">The dictionary value.</typeparam>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Collections/SerializableDictionary.cs
        #region Unity.XR.CoreUtils.Collections
        /// <summary>
        /// Class that stores the serialized items in this dictionary.
        /// </summary>
        [Serializable]
        public struct Item
        {
            /// <summary>
            /// The dictionary item key.
            /// </summary>
            public TKey key;

            /// <summary>
            /// The dictionary item value.
            /// </summary>
            public TValue value;
        }

        [SerializeField]
        List<Item> items = new List<Item>();

        /// <summary>
        /// The serialized items in this dictionary.
        /// </summary>
        public List<Item> SerializedItems => items;

        /// <summary>
        /// Initializes a new instance of the dictionary.
        /// </summary>
        public SerializableDictionary() { }

        /// <summary>
        /// Initializes a new instance of the dictionary that contains elements copied from the given
        /// <paramref name="input"/> dictionary.
        /// </summary>
        /// <param name="input">The dictionary from which to copy the elements.</param>
        public SerializableDictionary(IDictionary<TKey, TValue> input) : base(input) { }

        /// <summary>
        /// See <see cref="ISerializationCallbackReceiver"/>
        /// Save this dictionary to the <see cref="SerializedItems"/> list.
        /// </summary>
        public virtual void OnBeforeSerialize()
        {
            items.Clear();
            items.EnsureCapacity(Count);
            foreach (var pair in this)
                items.Add(new Item { key = pair.Key, value = pair.Value });
        }

        /// <summary>
        /// See <see cref="ISerializationCallbackReceiver"/>
        /// Load this dictionary from the <see cref="SerializedItems"/> list.
        /// </summary>
        public virtual void OnAfterDeserialize()
        {
            Clear();
            foreach (var item in items)
            {
                if (!TryAdd(item.key, item.value))
                {
                    Debug.LogWarning($"The key \"{item.key}\" is duplicated in " +
                                     $"{GetType().Name}.{nameof(SerializedItems)} and will be ignored.");
                }
            }
        }
        #endregion // Unity.XR.CoreUtils.Collections
    }
}
