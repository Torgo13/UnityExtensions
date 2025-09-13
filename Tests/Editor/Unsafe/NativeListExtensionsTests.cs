using NUnit.Framework;
using Unity.Collections;
using System.Collections.Generic;

namespace PKGE.Unsafe.Tests
{
    public class NativeListExtensionsTests
    {
        private NativeList<int> _nativeList;

        [SetUp]
        public void SetUp()
        {
            _nativeList = new NativeList<int>(Allocator.Temp);
        }

        [TearDown]
        public void TearDown()
        {
            if (_nativeList.IsCreated)
            {
                _nativeList.Dispose();
            }
        }

        [Test]
        public void AddRange_FromArray_ShouldAddElementsToNativeList()
        {
            int[] array = { 1, 2, 3, 4, 5 };

            _nativeList.AddRange(array);

            Assert.AreEqual(array.Length, _nativeList.Length);
            for (int i = 0; i < array.Length; i++)
            {
                Assert.AreEqual(array[i], _nativeList[i]);
            }
        }

        [Test]
        public void AddRange_FromArrayWithCount_ShouldAddSpecifiedCountOfElementsToNativeList()
        {
            int[] array = { 1, 2, 3, 4, 5 };
            int count = 3;

            _nativeList.AddRange(array, count);

            Assert.AreEqual(count, _nativeList.Length);
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(array[i], _nativeList[i]);
            }
        }

        [Test]
        public void AddRange_FromManagedList_ShouldAddElementsToNativeList()
        {
            List<int> managedList = new List<int> { 1, 2, 3, 4, 5 };

            _nativeList.AddRange(managedList);

            Assert.AreEqual(managedList.Count, _nativeList.Length);
            for (int i = 0; i < managedList.Count; i++)
            {
                Assert.AreEqual(managedList[i], _nativeList[i]);
            }
        }
    }
}
