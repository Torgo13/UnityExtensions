using NUnit.Framework;
using Unity.Collections;
using System.Collections.Generic;

namespace UnityExtensions.Unsafe.Tests
{
    public class ListExtensionsTests
    {
        [Test]
        public void ToNativeList_ShouldReturnNativeListCopy()
        {
            var list = new List<int> { 1, 2, 3 };
            var nativeList = list.ToNativeList(Allocator.Temp);

            Assert.AreEqual(3, nativeList.Length);
            Assert.AreEqual(1, nativeList[0]);
            Assert.AreEqual(2, nativeList[1]);
            Assert.AreEqual(3, nativeList[2]);

            nativeList.Dispose();
        }

        [Test]
        public void ToNativeArray_ShouldReturnNativeArrayCopy()
        {
            var list = new List<int> { 1, 2, 3 };
            var nativeArray = list.ToNativeArray(Allocator.Temp);

            Assert.AreEqual(3, nativeArray.Length);
            Assert.AreEqual(1, nativeArray[0]);
            Assert.AreEqual(2, nativeArray[1]);
            Assert.AreEqual(3, nativeArray[2]);

            nativeArray.Dispose();
        }
    }
}
