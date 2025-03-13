using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe.Tests
{
    public class UnsafeListExtensionsTests
    {
        [Test]
        public void AsNativeArray_ShouldConvertUnsafeListToNativeArray()
        {
            var allocator = Allocator.Temp;
            var unsafeList = new UnsafeList<int>(5, allocator);
            unsafeList.AddRange(new int[] { 1, 2, 3, 4, 5 });

            var nativeArray = unsafeList.AsNativeArray();

            Assert.AreEqual(unsafeList.Length, nativeArray.Length);
            for (int i = 0; i < unsafeList.Length; i++)
            {
                Assert.AreEqual(unsafeList[i], nativeArray[i]);
            }

            nativeArray.Dispose();
            unsafeList.Dispose();
        }

        [Test]
        public void GetSubNativeArray_ShouldReturnSubArrayOfNativeArray()
        {
            var allocator = Allocator.Temp;
            var unsafeList = new UnsafeList<int>(5, allocator);
            unsafeList.AddRange(new int[] { 1, 2, 3, 4, 5 });

            var subArray = unsafeList.GetSubNativeArray(1, 3);

            Assert.AreEqual(3, subArray.Length);
            for (int i = 0; i < subArray.Length; i++)
            {
                Assert.AreEqual(unsafeList[i + 1], subArray[i]);
            }

            subArray.Dispose();
            unsafeList.Dispose();
        }

        [Test]
        public void AddRange_FromArray_ShouldAddElementsToUnsafeList()
        {
            var allocator = Allocator.Temp;
            var unsafeList = new UnsafeList<int>(5, allocator);
            for (int i = 0; i < 5; i++)
                unsafeList.Add(i);
            int[] array = { 6, 7, 8 };

            unsafeList.AddRange(array);

            Assert.AreEqual(8, unsafeList.Length);
            for (int i = 0; i < array.Length; i++)
            {
                Assert.AreEqual(array[i], unsafeList[i + 5]);
            }

            unsafeList.Dispose();
        }

        [Test]
        public void AddRange_FromArrayWithCount_ShouldAddSpecifiedCountOfElementsToUnsafeList()
        {
            var allocator = Allocator.Temp;
            var unsafeList = new UnsafeList<int>(5, allocator);
            for (int i = 0; i < 5; i++)
                unsafeList.Add(i);
            int[] array = { 6, 7, 8, 9, 10 };
            int count = 3;

            unsafeList.AddRange(array, count);

            Assert.AreEqual(8, unsafeList.Length);
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(array[i], unsafeList[i + 5]);
            }

            unsafeList.Dispose();
        }
    }
}
