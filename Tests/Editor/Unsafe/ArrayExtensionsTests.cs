using System;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PKGE.Unsafe.Tests
{
    [TestFixture]
    public class ArrayExtensionsTests
    {
        [Test]
        public void GetByteSpanFromArray_ShouldReturnCorrectSpan_WhenArrayIsNotNullOrEmpty()
        {
            int[] array = { 1, 2, 3, 4, 5 };
            int elementSize = UnsafeUtility.SizeOf<int>();

            var byteSpan = ArrayExtensions.GetByteSpanFromArray(array, elementSize);

            Assert.AreEqual(array.Length * elementSize, byteSpan.Length);
            unsafe
            {
                fixed (void* ptr = byteSpan)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        Assert.AreEqual(array[i], ((int*)ptr)[i]);
                    }
                }
            }
        }

        [Test]
        public void GetByteSpanFromArray_ShouldReturnEmptySpan_WhenArrayIsNull()
        {
            int[] array = null;
            int elementSize = UnsafeUtility.SizeOf<int>();

            var byteSpan = ArrayExtensions.GetByteSpanFromArray(array, elementSize);

            Assert.AreEqual(0, byteSpan.Length);
        }

        [Test]
        public void GetByteSpanFromArray_ShouldReturnEmptySpan_WhenArrayIsEmpty()
        {
            int[] array = new int[0];
            int elementSize = UnsafeUtility.SizeOf<int>();

            var byteSpan = ArrayExtensions.GetByteSpanFromArray(array, elementSize);

            Assert.AreEqual(0, byteSpan.Length);
        }

#if PKGE_USING_UNSAFE
        [Test]
        public void CalculateOffset_ShouldReturnCorrectOffset()
        {
            int idx = 0, idxOffset = 1;

            int[] array = {
                5, 10
            };
            var offset = UnsafeExtensions.CalculateOffset(ref array[idx + idxOffset], ref array[idx]);
            Assert.AreEqual(UnsafeUtility.SizeOf<int>() * idxOffset, offset);

#if PKGE_USING_INTPTR
            unsafe
            {
                fixed (int* ptr = &array[0])
                {
                    offset = UnsafeExtensions.CalculateOffset((IntPtr)(ptr + idxOffset), (IntPtr)ptr);
                    Assert.AreEqual(UnsafeUtility.SizeOf<int>() * idxOffset, offset);
                }
            }
#endif // PKGE_USING_INTPTR

            idx = 1; idxOffset = 2;

            array = new int[]
            {
                0, 1, 2, 3
            };
            offset = UnsafeExtensions.CalculateOffset(ref array[idx + idxOffset], ref array[idx]);
            Assert.AreEqual(UnsafeUtility.SizeOf<int>() * idxOffset, offset);

#if PKGE_USING_INTPTR
            unsafe
            {
                fixed (int* ptr = &array[1])
                {
                    offset = UnsafeExtensions.CalculateOffset((IntPtr)(ptr + idxOffset), (IntPtr)ptr);
                    Assert.AreEqual(UnsafeUtility.SizeOf<int>() * idxOffset, offset);
                }
            }
#endif // PKGE_USING_INTPTR
        }
#endif // PKGE_USING_UNSAFE

        /// <summary>
        /// Correctness: Ensures the entire array is filled with the specified value
        /// </summary>
        [Test]
        public void FillArray_ShouldFillArrayWithSpecifiedValue()
        {
            // Arrange
            var array = new NativeArray<int>(5, Allocator.Temp);
            const int fillValue = 42;

            // Act
            array.FillArray(fillValue);

            // Assert
            foreach (var item in array)
            {
                Assert.AreEqual(fillValue, item);
            }

            array.Dispose();
        }

        /// <summary>
        /// Correctness: Validates partial filling with a start index and length
        /// </summary>
        [Test]
        public void FillArray_ShouldFillArrayPartially_WithSpecifiedValue()
        {
            // Arrange
            var array = new NativeArray<int>(5, Allocator.Temp);
            const int fillValue = 99;

            // Act
            array.FillArray(fillValue, startIndex: 2, length: 2);

            // Assert
            for (int i = 0; i < array.Length; i++)
            {
                if (i >= 2 && i < 4)
                {
                    Assert.AreEqual(fillValue, array[i]);
                }
                else
                {
                    Assert.AreEqual(0, array[i]);
                }
            }

            array.Dispose();
        }

        /// <summary>
        /// Edge Cases: Handles negative lengths gracefully, filling until the array's end
        /// </summary>
        [Test]
        public void FillArray_ShouldFillFromStartIndexUntilEnd_WhenLengthIsNegative()
        {
            // Arrange
            var array = new NativeArray<int>(5, Allocator.Temp);
            const int fillValue = 7;

            // Act
            array.FillArray(fillValue, startIndex: 1, length: -1);

            // Assert
            for (int i = 0; i < array.Length; i++)
            {
                if (i >= 1)
                {
                    Assert.AreEqual(fillValue, array[i]);
                }
                else
                {
                    Assert.AreEqual(0, array[i]);
                }
            }

            array.Dispose();
        }

        /// <summary>
        /// Edge Cases: Throws appropriate exceptions for invalid arguments
        /// (e.g., negative start indexes)
        /// </summary>
        [Test]
        public void FillArray_ShouldThrowException_WhenStartIndexIsNegative()
        {
            // Arrange
            var array = new NativeArray<int>(5, Allocator.Temp);

            // Act & Assert
            Assert.Throws<IndexOutOfRangeException>(() =>
                array.FillArray(42, startIndex: -1));

            array.Dispose();
        }

        /// <summary>
        /// Edge Cases: Throws appropriate exceptions for invalid arguments
        /// (e.g., out-of-bound lengths)
        /// </summary>
        [Test]
        public void FillArray_ShouldThrowException_WhenLengthExceedsArrayBounds()
        {
            // Arrange
            var array = new NativeArray<int>(5, Allocator.Temp);

            // Act & Assert
            Assert.Throws<IndexOutOfRangeException>(() =>
                array.FillArray(42, startIndex: 2, length: 10));

            array.Dispose();
        }

        /// <summary>
        /// Invalid Operations: Tests behavior with disposed arrays
        /// </summary>
        [Test]
        public void FillArray_ShouldThrowInvalidOperationException_WhenNativeArrayIsDisposed()
        {
            // Arrange
            var array = new NativeArray<int>(5, Allocator.Temp);
            array.Dispose();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                array.FillArray(42));
        }

        /// <summary>
        /// Performance: Verifies functionality with large arrays
        /// </summary>
        [Test]
        public void FillArray_ShouldHandleLargeNativeArraysEfficiently()
        {
            // Arrange
            var largeArray = new NativeArray<int>(10_000_000, Allocator.Temp);
            const int fillValue = 123;

            // Act
            largeArray.FillArray(fillValue);

            // Assert
            foreach (var item in largeArray)
            {
                Assert.AreEqual(fillValue, item);
            }

            largeArray.Dispose();
        }
    }
}
