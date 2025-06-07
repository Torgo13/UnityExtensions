using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExtensions.Collections
{
    /// <summary>
    /// A queue-like data structure that allows read-only access to all values in the queue.
    /// </summary>
    /// <typeparam name="T">The type of data stored in the buffer.</typeparam>
    public class CircularBuffer<T> : IReadOnlyList<T>
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/main/Packages/com.unity.live-capture/Runtime/Core/Utilities/CircularBuffer.cs
        #region Unity.LiveCapture
        T[] _data;
        int _startIndex ;
        int _endIndex;

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count => (_data.Length + _endIndex - _startIndex) % _data.Length;

        /// <summary>
        /// The maximum number of elements which can be stored in the collection.
        /// </summary>
        /// <remarks>If the new size is smaller than the current <see cref="Count"/>, elements are truncated from the front.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the capacity is not greater than zero.</exception>
        public int Capacity
        {
            get => _data.Length - 1;
            set => SetCapacity(value);
        }

        /// <summary>
        /// A callback invoked for each element that is discarded from the buffer.
        /// </summary>
        public event Action<T> ElementDiscarded;

        /// <summary>
        /// Constructs a new <see cref="CircularBuffer{T}"/> instance with an initial capacity.
        /// </summary>
        /// <param name="capacity">The maximum number of elements which can be stored in the collection.</param>
        public CircularBuffer(int capacity)
        {
            Capacity = capacity;
        }

        /// <inheritdoc cref="PushBack"/>
        public void Add(in T value)
        {
            // The add method is an alias for PushBack to support initializer syntax
            PushBack(value);
        }

        /// <summary>
        /// Adds an element to the back of the buffer.
        /// </summary>
        /// <remarks>
        /// If the buffer is full, the element at the front of the buffer will be discarded.
        /// </remarks>
        /// <param name="value">The element to add.</param>
        public void PushBack(in T value)
        {
            if (Count == Capacity)
            {
                OnValueDiscarded(PeekFront());
                IncrementIndex(ref _startIndex);
            }

            _data[_endIndex] = value;
            IncrementIndex(ref _endIndex);
        }

        /// <summary>
        /// Adds an element to the front of the buffer.
        /// </summary>
        /// <remarks>
        /// If the buffer is full, the element at the back of the buffer will be discarded.
        /// </remarks>
        /// <param name="value">The element to add.</param>
        public void PushFront(in T value)
        {
            if (Count == Capacity)
            {
                OnValueDiscarded(PeekBack());
                DecrementIndex(ref _endIndex);
            }

            DecrementIndex(ref _startIndex);
            _data[_startIndex] = value;
        }

        /// <summary>
        /// Insets an element into the buffer at the specified index.
        /// </summary>
        /// <remarks>
        /// If the buffer is full, the element at the front of the buffer will be discarded.
        /// </remarks>
        /// <param name="index">The index counting from the front of the buffer.</param>
        /// <param name="value">The element to add.</param>
        public void PushIndex(int index, in T value)
        {
            if (index == Count)
            {
                PushBack(value);
                return;
            }

            PreconditionInBounds(index);

            if (Count == Capacity)
            {
                OnValueDiscarded(PeekFront());

                for (var i = 0; i < index; i++)
                {
                    this[i] = this[i + 1];
                }
            }
            else
            {
                // duplicate the last element to grow the list
                PushBack(PeekBack());

                // the last element is already copied, so skip it
                for (var i = Count - 2; i > index; i--)
                {
                    this[i] = this[i - 1];
                }
            }

            this[index] = value;
        }

        /// <summary>
        /// Removes an element from the front of the buffer.
        /// </summary>
        /// <returns>The removed element.</returns>
        public T PopFront()
        {
            PreconditionNotEmpty();
            var item = PeekFront();
            IncrementIndex(ref _startIndex);
            return item;
        }

        /// <summary>
        /// Removes an element from the back of the buffer.
        /// </summary>
        /// <returns>The removed element.</returns>
        public T PopBack()
        {
            PreconditionNotEmpty();
            var item = PeekBack();
            DecrementIndex(ref _endIndex);
            return item;
        }

        /// <summary>
        /// Get the element at the front of the buffer.
        /// </summary>
        /// <returns>The element at the front of the buffer.</returns>
        public ref readonly T PeekFront()
        {
            PreconditionNotEmpty();
            return ref _data[_startIndex];
        }

        /// <summary>
        /// Get the element at the back of the buffer.
        /// </summary>
        /// <returns>The element at the back of the buffer.</returns>
        public ref readonly T PeekBack()
        {
            PreconditionNotEmpty();
            var backIndex = _endIndex;
            DecrementIndex(ref backIndex);
            return ref _data[backIndex];
        }

        /// <summary>
        /// Gets the element at the specified index in the buffer.
        /// </summary>
        /// <param name="index">The index counting from the front of the buffer.</param>
        /// <returns>The element at the specified index in the buffer.</returns>
        public ref readonly T PeekIndex(int index)
        {
            PreconditionInBounds(index);
            return ref _data[(_startIndex + index) % _data.Length];
        }

        /// <summary>
        /// Removes all items in the collection.
        /// </summary>
        public void Clear()
        {
            foreach (var value in this)
            {
                OnValueDiscarded(value);
            }

            _startIndex = 0;
            _endIndex = 0;
        }

        void SetCapacity(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Must be greater than zero.");
            }

            if (_data != null && capacity == Capacity)
            {
                return;
            }

            // We determine that the buffer is full by checking that the end index
            // is one less than the start index. This means that the capacity of
            // the buffer is the array length minus one, but it allows us to differentiate
            // between the empty case and the full case using the start and
            // end indices.
            var newArray = new T[capacity + 1];
            var endIndex = 0;

            if (_data != null)
            {
                while (Count > capacity)
                {
                    OnValueDiscarded(PopFront());
                }

                var index = _startIndex;
                while (index != _endIndex)
                {
                    newArray[endIndex] = _data[index];
                    IncrementIndex(ref index);
                    ++endIndex;
                }
            }

            _data = newArray;
            _startIndex = 0;
            _endIndex = endIndex;
        }

        /// <inheritdoc />
        public T this[int index]
        {
            get => PeekIndex(index);
            set
            {
                PreconditionInBounds(index);

                OnValueDiscarded(PeekIndex(index));

                _data[(index + _startIndex) % _data.Length] = value;
            }
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            var index = _startIndex;
            while (index != _endIndex)
            {
                yield return _data[index];
                IncrementIndex(ref index);
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void PreconditionNotEmpty()
        {
            if (_endIndex == _startIndex)
            {
                throw new InvalidOperationException("Buffer is empty");
            }
        }

        void PreconditionInBounds(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }
        }

        void IncrementIndex(ref int index)
        {
            index = (index + 1) % _data.Length;
        }

        void DecrementIndex(ref int index)
        {
            index = (_data.Length + index - 1) % _data.Length;
        }

        void OnValueDiscarded(in T value)
        {
            try
            {
                ElementDiscarded?.Invoke(value);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        #endregion // Unity.LiveCapture
    }
}
