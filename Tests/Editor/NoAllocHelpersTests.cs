using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace PKGE.Editor.Tests
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

        [Test]
        public void EnsureListElemCount_ShouldThrowArgumentNullException_WhenListIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => NoAllocHelpers.EnsureListElemCount<int>(null, 5));
        }

        [Test]
        public void EnsureListElemCount_ShouldThrowArgumentException_WhenCountIsNegative()
        {
            var list = new List<int>();
            Assert.Throws<ArgumentException>(() => NoAllocHelpers.EnsureListElemCount(list, -1));
        }

        [Test]
        public void EnsureListElemCount_ShouldResizeList()
        {
            var list = new List<int> { 1, 2, 3 };
            NoAllocHelpers.EnsureListElemCount(list, 5);

            Assert.AreEqual(5, list.Count);

            list[3] = 4;
            Assert.AreEqual(4, list[3]);
        }

        [Test]
        public void SafeLength_Array_ShouldReturnLengthOfArray()
        {
            int[] array = { 1, 2, 3, 4, 5 };
            Assert.AreEqual(5, NoAllocHelpers.SafeLength(array));
        }

        [Test]
        public void SafeLength_Array_ShouldReturnZero_WhenArrayIsNull()
        {
            int[] array = null;
            Assert.AreEqual(0, NoAllocHelpers.SafeLength(array));
        }

        [Test]
        public void SafeLength_List_ShouldReturnCountOfList()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            Assert.AreEqual(5, NoAllocHelpers.SafeLength(list));
        }

        [Test]
        public void SafeLength_List_ShouldReturnZero_WhenListIsNull()
        {
            List<int> list = null;
            Assert.AreEqual(0, NoAllocHelpers.SafeLength(list));
        }

        [Test]
        public void ExtractArrayFromList_ShouldReturnInternalArray()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var array = NoAllocHelpers.ExtractArrayFromList(list);

            Assert.AreEqual(list.Capacity, array.Length);
            for (int i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(list[i], array[i]);
            }
        }

        [Test]
        public void ExtractArrayFromList_ShouldReturnNull_WhenListIsNull()
        {
            List<int> list = null;
            var array = NoAllocHelpers.ExtractArrayFromList(list);

            Assert.IsNull(array);
        }

        [Test]
        public void ResetListContents_ShouldSetListContents()
        {
            var list = new List<int> { 1, 2, 3 };
            var span = new ReadOnlySpan<int>(new int[] { 4, 5, 6 });

            NoAllocHelpers.ResetListContents(list, span);

            Assert.AreEqual(span.Length, list.Count);
            for (int i = 0; i < span.Length; i++)
            {
                Assert.AreEqual(span[i], list[i]);
            }
        }

        [Test]
        public void ResetListSize_ShouldSetListSize()
        {
            var list = new List<int> { 1, 2, 3 };

            NoAllocHelpers.ResetListSize(list, 5);

            Assert.AreEqual(5, list.Count);
        }

        [Test]
        public void ResetListSizeNoResize_ShouldSetListSize()
        {
            var list = new List<int> { 1, 2, 3 };

            // Must set List capacity first
            list.Capacity = 5;
            NoAllocHelpers.ResetListSizeNoResize(list, 5);

            Assert.AreEqual(5, list.Count);
        }
    }
}
