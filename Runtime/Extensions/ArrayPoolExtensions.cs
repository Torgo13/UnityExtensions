using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace UnityExtensions
{
    public readonly struct DisposeArrayPool<T> : IDisposable
    {
        private readonly T[] _pooledArray;
        public T[] PooledArray => _pooledArray;

        public DisposeArrayPool(int minimumLength)
        {
            UnityEngine.Assertions.Assert.IsTrue(minimumLength > 0);

            _pooledArray = ArrayPool<T>.Shared.Rent(minimumLength);
        }

        public static DisposeArrayPool<T> Rent(int minimumLength)
        {
            return new DisposeArrayPool<T>(minimumLength);
        }

        #region IDisposable
        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(PooledArray, clearArray: true);
        }
        #endregion // IDisposable
    }

    public struct ResizableArrayPool<T> : IDisposable
    {
        private T[] _pooledArray;
        public readonly T[] PooledArray => _pooledArray;

        public ResizableArrayPool(int minimumLength)
        {
            UnityEngine.Assertions.Assert.IsTrue(minimumLength > 0);

            _pooledArray = ArrayPool<T>.Shared.Rent(minimumLength);
        }

        public static ResizableArrayPool<T> Rent(int minimumLength)
        {
            return new ResizableArrayPool<T>(minimumLength);
        }

        public T[] Resize(int minimumLength)
        {
            UnityEngine.Assertions.Assert.IsTrue(minimumLength > 0);

            ArrayPoolExtensions.Resize(ref _pooledArray, minimumLength);

            return _pooledArray;
        }

        #region IDisposable
        public readonly void Dispose()
        {
            ArrayPool<T>.Shared.Return(PooledArray, clearArray: true);
        }
        #endregion // IDisposable
    }

    public static class ArrayPoolExtensions
    {
        //https://github.com/CommunityToolkit/dotnet/blob/657c6971a8d42655c648336b781639ed96c2c49f/src/CommunityToolkit.HighPerformance/Extensions/ArrayPoolExtensions.cs
        #region CommunityToolkit.HighPerformance
#nullable enable
        /// <summary>
        /// Changes the number of elements of a rented one-dimensional array to the specified new size.
        /// </summary>
        /// <typeparam name="T">The type of items into the target array to resize.</typeparam>
        /// <param name="pool">The target <see cref="ArrayPool{T}"/> instance to use to resize the array.</param>
        /// <param name="array">The rented <typeparamref name="T"/> array to resize, or <see langword="null"/> to create a new array.</param>
        /// <param name="newSize">The size of the new array.</param>
        /// <param name="clearArray">Indicates whether the contents of the array should be cleared before reuse.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="newSize"/> is less than 0.</exception>
        /// <remarks>When this method returns, the caller must not use any references to the old array anymore.</remarks>
        public static void Resize<T>(this ArrayPool<T> pool, [NotNull] ref T[]? array, int newSize, bool clearArray = false,
            bool copyArray = true)
        {
            // If the old array is null, just create a new one with the requested size
            if (array is null)
            {
                array = pool.Rent(newSize);

                return;
            }

            // If the new size is the same as the current size, do nothing
            if (array.Length == newSize)
            {
                return;
            }

            // Rent a new array with the specified size, and copy as many items from the current array
            // as possible to the new array. This mirrors the behavior of the Array.Resize API from
            // the BCL: if the new size is greater than the length of the current array, copy all the
            // items from the original array into the new one. Otherwise, copy as many items as possible,
            // until the new array is completely filled, and ignore the remaining items in the first array.
            T[] newArray = pool.Rent(newSize);

            if (copyArray)
            {
                int itemsToCopy = Math.Min(array.Length, newSize);
                Array.Copy(array, 0, newArray, 0, itemsToCopy);
            }

            pool.Return(array, clearArray);

            array = newArray;
        }

        /// <summary>
        /// Ensures that when the method returns <paramref name="array"/> is not null and is at least <paramref name="capacity"/> in length.
        /// Contents of <paramref name="array"/> are not copied if a new array is rented.
        /// </summary>
        /// <typeparam name="T">The type of items into the target array given as input.</typeparam>
        /// <param name="pool">The target <see cref="ArrayPool{T}"/> instance used to rent and/or return the array.</param>
        /// <param name="array">The rented <typeparamref name="T"/> array to ensure capacity for, or <see langword="null"/> to rent a new array.</param>
        /// <param name="capacity">The minimum length of <paramref name="array"/> when the method returns.</param>
        /// <param name="clearArray">Indicates whether the contents of the array should be cleared if returned to the pool.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> is less than 0.</exception>
        /// <remarks>When this method returns, the caller must not use any references to the old array anymore.</remarks>
        public static void EnsureCapacity<T>(this ArrayPool<T> pool, [NotNull] ref T[]? array, int capacity, bool clearArray = false)
        {
            if (capacity < 0)
            {
                ThrowArgumentOutOfRangeExceptionForNegativeArrayCapacity();
            }

            if (array is null)
            {
                array = pool.Rent(capacity);
            }
            else if (array.Length < capacity)
            {
                // Ensure rent succeeds before returning the original array to the pool
                T[] newArray = pool.Rent(capacity);

                pool.Return(array, clearArray);

                array = newArray;
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> when the "capacity" parameter is negative.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The array capacity must be a positive number.</exception>
        private static void ThrowArgumentOutOfRangeExceptionForNegativeArrayCapacity()
        {
            throw new ArgumentOutOfRangeException("capacity", "The array capacity must be a positive number.");
        }
        #endregion // CommunityToolkit.HighPerformance

        /// <summary>
        /// Changes the number of elements of a rented one-dimensional array to the specified new size.
        /// </summary>
        /// <typeparam name="T">The type of items into the target array to resize.</typeparam>
        /// <param name="array">The rented <typeparamref name="T"/> array to resize, or <see langword="null"/> to create a new array.</param>
        /// <param name="newSize">The size of the new array.</param>
        /// <param name="clearArray">Indicates whether the contents of the array should be cleared before reuse.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="newSize"/> is less than 0.</exception>
        /// <remarks>When this method returns, the caller must not use any references to the old array anymore.</remarks>
        public static void Resize<T>([NotNull] ref T[]? array, int newSize, bool clearArray = false)
        {
            ArrayPool<T>.Shared.Resize(ref array, newSize, clearArray);
        }
#nullable restore
    }
}
