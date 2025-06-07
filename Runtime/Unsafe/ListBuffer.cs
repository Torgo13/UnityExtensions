using System;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe
{
    /// <summary>
    /// A list that stores value on a provided memory buffer.
    /// <remarks>
    /// Usually use this to have a list on stack allocated memory.
    /// </remarks>
    /// </summary>
    /// <typeparam name="T">The type of the data stored in the list.</typeparam>
    public readonly unsafe struct ListBuffer<T>
        where T : unmanaged
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Common/ListBuffer.cs
        #region UnityEngine.Rendering
        private readonly T* _bufferPtr;
        private readonly int _capacity;
        private readonly int* _countPtr;

        /// <summary>
        /// The pointer to the memory storage.
        /// </summary>
        internal readonly T* BufferPtr => _bufferPtr;

        /// <summary>
        /// The number of item in the list.
        /// </summary>
        public readonly int Count => *_countPtr;

        /// <summary>
        /// The maximum number of item stored in this list.
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        /// Instantiate a new list.
        /// </summary>
        /// <param name="bufferPtr">The address in memory to store the data.</param>
        /// <param name="countPtr">The address in memory to store the number of item of this list.</param>
        /// <param name="capacity">The number of <typeparamref name="T"/> that can be stored in the buffer.</param>
        public ListBuffer(T* bufferPtr, int* countPtr, int capacity)
        {
            _bufferPtr = bufferPtr;
            _capacity = capacity;
            _countPtr = countPtr;
        }

        /// <summary>
        /// Get an item from the list.
        /// </summary>
        /// <param name="index">The index of the item to get.</param>
        /// <value>A reference to the item.</value>
        /// <exception cref="IndexOutOfRangeException">If the index is invalid.</exception>
        public readonly ref T this[in int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException(
                        $"Expected a value between 0 and {Count}, but received {index}.");
                return ref _bufferPtr[index];
            }
        }

        /// <summary>
        /// Get an item from the list.
        ///
        /// Safety: index must be inside the bounds of the list.
        /// </summary>
        /// <param name="index">The index of the item to get.</param>
        /// <returns>A reference to the item.</returns>
        public readonly ref T GetUnchecked(in int index) => ref _bufferPtr[index];

        /// <summary>
        /// Try to add a value in the list.
        /// </summary>
        /// <param name="value">A reference to the value to add.</param>
        /// <returns>
        ///   <c>true</c> when the value was added,
        ///   <c>false</c> when the value was not added because the capacity was reached.
        /// </returns>
        public readonly bool TryAdd(in T value)
        {
            if (Count >= _capacity)
                return false;

            _bufferPtr[Count] = value;
            ++*_countPtr;
            return true;
        }

        /// <summary>
        /// Copy the content of this list into another buffer in memory.
        ///
        /// Safety:
        ///  * The destination must have enough memory to receive the copied data.
        /// </summary>
        /// <param name="dstBuffer">The destination buffer of the copy operation.</param>
        /// <param name="startDstIndex">The index of the first element that will be copied in the destination buffer.</param>
        /// <param name="copyCount">The number of item to copy.</param>
        public readonly void CopyTo(T* dstBuffer, int startDstIndex, int copyCount)
        {
            UnsafeUtility.MemCpy(dstBuffer + startDstIndex, _bufferPtr,
                UnsafeUtility.SizeOf<T>() * copyCount);
        }

        /// <summary>
        /// Try to copy the list into another list.
        /// </summary>
        /// <param name="other">The destination of the copy.</param>
        /// <returns>
        ///   * <c>true</c> when the copy was performed.
        ///   * <c>false</c> when the copy was aborted because the destination have a capacity too small.
        /// </returns>
        public readonly bool TryCopyTo(ListBuffer<T> other)
        {
            if (other.Count + Count >= other._capacity)
                return false;

            UnsafeUtility.MemCpy(other._bufferPtr + other.Count, _bufferPtr, UnsafeUtility.SizeOf<T>() * Count);
            *other._countPtr += Count;
            return true;
        }

        /// <summary>
        /// Try to copy the data from a buffer in this list.
        /// </summary>
        /// <param name="srcPtr">The pointer of the source memory to copy.</param>
        /// <param name="count">The number of item to copy from the source buffer.</param>
        /// <returns>
        ///   * <c>true</c> when the copy was performed.
        ///   * <c>false</c> when the copy was aborted because the capacity of this list is too small.
        /// </returns>
        public readonly bool TryCopyFrom(T* srcPtr, int count)
        {
            if (count + Count > _capacity)
                return false;

            UnsafeUtility.MemCpy(_bufferPtr + Count, srcPtr, UnsafeUtility.SizeOf<T>() * count);
            *_countPtr += count;
            return true;
        }
        #endregion // UnityEngine.Rendering
    }
}
