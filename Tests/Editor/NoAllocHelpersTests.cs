using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace UnityExtensions.Editor.Tests
{
    public class NoAllocHelpersTests
    {
        [Test]
        public void ExtractArrayFromList_ValidList_ReturnsArray()
        {
            var list = new List<int> { 1, 2, 3 };
            var array = NoAllocHelpers.ExtractArrayFromList(list);
            Assert.AreEqual(list.Capacity, array.Length);
            Assert.AreEqual(list[0], array[0]);
            Assert.AreEqual(list[1], array[1]);
            Assert.AreEqual(list[2], array[2]);
        }

        [Test]
        public void SafeLength_NullArray_ReturnsZero()
        {
            int[] array = null;
            int length = NoAllocHelpers.SafeLength(array);
            Assert.AreEqual(0, length);
        }

        [Test]
        public void SafeLength_ValidArray_ReturnsArrayLength()
        {
            var array = new int[5];
            int length = NoAllocHelpers.SafeLength(array);
            Assert.AreEqual(array.Length, length);
        }

        [Test]
        public void ResizeList_ValidList_ResizesCorrectly()
        {
            var list = new List<int> { 1, 2, 3 };
            NoAllocHelpers.EnsureListElemCount(list, 5);
            Assert.AreEqual(5, list.Count);
            NoAllocHelpers.ResetListSize(list, 2);
            Assert.AreEqual(2, list.Count);
        }

        [Test]
        public void EnsureListElemCount_ValidList_EnsuresCorrectCount()
        {
            var list = new List<int> { 1, 2, 3 };
            NoAllocHelpers.EnsureListElemCount(list, 5);
            Assert.AreEqual(5, list.Count);
            NoAllocHelpers.EnsureListElemCount(list, 2);
            Assert.AreEqual(2, list.Count);
        }
    }
}
