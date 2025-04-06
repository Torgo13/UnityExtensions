using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe.Tests
{
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

            array = new int[]
            {
                0, 1, 2, 3
            };
            offset = ArrayExtensions.CalculateOffset(ref array[3], ref array[1]);
            Assert.AreEqual(UnsafeUtility.SizeOf<int>() * 2, offset);
        }
    }
}
