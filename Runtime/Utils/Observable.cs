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
        public event Action<T> onValueChanged;

        private T m_Value;

        /// <summary>
        /// The current value.
        /// </summary>
        public T value
        {
            get => m_Value;
            set
            {
                // Only invoke the event if the new value is different from the current value
                if (!EqualityComparer<T>.Default.Equals(value, m_Value))
                {
                    m_Value = value;

                    // Notify subscribers when the value changes
                    onValueChanged?.Invoke(value);
                }
            }
        }

        /// <summary>
        /// Constructor with value
        /// </summary>
        /// <param name="newValue">The new value to be assigned.</param>
        public Observable(T newValue)
        {
            m_Value = newValue;
            onValueChanged = null;
        }
        #endregion // UnityEngine.Rendering
    }
}
