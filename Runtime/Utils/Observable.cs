using System;
using System.Collections.Generic;

namespace UnityExtensions
{
    /// <summary>
    /// Represents an observable value of type T. Subscribers can be notified when the value changes.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public struct Observable<T>
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Common/Observable.cs
        #region UnityEngine.Rendering
        /// <summary>
        /// Event that is triggered when the value changes.
        /// </summary>
        public event Action<T> OnValueChanged;

        private T _value;

        /// <summary>
        /// The current value.
        /// </summary>
        public T value
        {
            get => _value;
            set
            {
                // Only invoke the event if the new value is different from the current value
                if (!EqualityComparer<T>.Default.Equals(value, _value))
                {
                    _value = value;

                    // Notify subscribers when the value changes
                    OnValueChanged?.Invoke(value);
                }
            }
        }

        /// <summary>
        /// Constructor with value
        /// </summary>
        /// <param name="newValue">The new value to be assigned.</param>
        public Observable(T newValue)
        {
            _value = newValue;
            OnValueChanged = null;
        }
        #endregion // UnityEngine.Rendering
    }
}
