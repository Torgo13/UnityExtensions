using System;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe.Tests
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

        [Test]
        public void CalculateOffset_ShouldReturnCorrectOffset()
        {
            int[] array = {
                5, 10
            };
            int offset = ArrayExtensions.CalculateOffset(ref array[1], ref array[0]);
            Assert.AreEqual(UnsafeUtility.SizeOf<int>(), offset);

            unsafe
            {
                fixed (int* ptr = &array[0])
                {
                    offset = ArrayExtensions.CalculateOffset((IntPtr)(ptr + 1), (IntPtr)ptr);
                    Assert.AreEqual(UnsafeUtility.SizeOf<int>(), offset);
                }
            }

            array = new int[]
            {
                0, 1, 2, 3
            };
            offset = ArrayExtensions.CalculateOffset(ref array[3], ref array[1]);
            Assert.AreEqual(UnsafeUtility.SizeOf<int>() * 2, offset);

            unsafe
            {
                fixed (int* ptr = &array[1])
                {
                    offset = ArrayExtensions.CalculateOffset((IntPtr)(ptr + 2), (IntPtr)ptr);
                    Assert.AreEqual(UnsafeUtility.SizeOf<int>() * 2, offset);
                }
            }
        }

        //https://github.com/Unity-Technologies/InputSystem/blob/fb786d2a7d01b8bcb8c4218522e5f4b9afea13d7/Assets/Tests/InputSystem/Utilities/ArrayHelperTests.cs
        #region UnityEngine.InputSystem.Utilities
        [Test]
        [Category("Utilities")]
        public void Utilities_CanEraseInNativeArrayWithCapacity()
        {
            var array1 = new NativeArray<int>(new[] { 1, 2, 3, 4, 5, 0, 0, 0 }, Allocator.Temp);
            var array2 = new NativeArray<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, Allocator.Temp);
            var array3 = new NativeArray<int>(new[] { 1, 2, 3, 4, 0, 0, 0, 0 }, Allocator.Temp);

            var array1Length = 5;
            var array2Length = 8;
            var array3Length = 4;

            try
            {
                ArrayExtensions.EraseAtWithCapacity(array1, ref array1Length, 2);
                ArrayExtensions.EraseAtWithCapacity(array2, ref array2Length, 7);
                ArrayExtensions.EraseAtWithCapacity(array3, ref array3Length, 0);

                // For NativeArray, we don't clear memory.
                Assert.That(array1, Is.EqualTo(new[] { 1, 2, 4, 5, 5, 0, 0, 0 }));
                Assert.That(array2, Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
                Assert.That(array3, Is.EqualTo(new[] { 2, 3, 4, 4, 0, 0, 0, 0 }));

                Assert.That(array1Length, Is.EqualTo(4));
                Assert.That(array2Length, Is.EqualTo(7));
                Assert.That(array3Length, Is.EqualTo(3));
            }
            finally
            {
                array1.Dispose();
                array2.Dispose();
                array3.Dispose();
            }
        }
        #endregion // UnityEngine.InputSystem.Utilities

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
