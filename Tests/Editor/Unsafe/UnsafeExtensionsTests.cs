using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Collections;

namespace UnityExtensions.Unsafe.Tests
{
    public class UnsafeExtensionsTests
    {
        [Test]
        public void UnsafeElementAt_ShouldReturnCorrectReference()
        {
            int[] array = { 1, 2, 3, 4, 5 };
            int index = 2;

            ref int element = ref array.UnsafeElementAt(index);

            Assert.AreEqual(array[index], element);

            element = 10;
            Assert.AreEqual(10, array[index]);
        }

        /*
        [Test]
        public void UnsafeElementAt_ShouldThrowException_WhenArrayIsEmpty()
        {
            int[] array = new int[0];
            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                ref int element = ref array.UnsafeElementAt(0);
            });
        }
        */

        [Test]
        public void UnsafeElementAt_List_ShouldReturnCorrectReference()
        {
            List <int> list = new() { 1, 2, 3, 4, 5 };
            int index = 2;

            ref int element = ref list.UnsafeElementAt(index);

            Assert.AreEqual(list[index], element);

            element = 10;
            Assert.AreEqual(10, list[index]);
        }

        [Test]
        public void CopyToList_FromArray_ShouldCopyElementsToList()
        {
            int[] array = { 1, 2, 3, 4, 5 };
            var list = new List<int>(array.Length);

            array.CopyTo(list);

            Assert.AreEqual(array.Length, list.Count);
            for (int i = 0; i < array.Length; i++)
            {
                Assert.AreEqual(array[i], list[i]);
            }
        }

        [Test]
        public void CopyToList_FromNativeArray_ShouldCopyElementsToList()
        {
            var nativeArray = new NativeArray<int>(new int[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            var list = new List<int>(nativeArray.Length);

            nativeArray.CopyTo(list);

            Assert.AreEqual(nativeArray.Length, list.Count);
            for (int i = 0; i < nativeArray.Length; i++)
            {
                Assert.AreEqual(nativeArray[i], list[i]);
            }

            nativeArray.Dispose();
        }

        [Test]
        public void CopyToList_FromNativeList_ShouldCopyElementsToList()
        {
            var nativeList = new NativeList<int>(Allocator.Temp);
            nativeList.AddRange(new int[] { 1, 2, 3, 4, 5 });
            var list = new List<int>(nativeList.Length);

            nativeList.CopyTo(list);

            Assert.AreEqual(nativeList.Length, list.Count);
            for (int i = 0; i < nativeList.Length; i++)
            {
                Assert.AreEqual(nativeList[i], list[i]);
            }

            nativeList.Dispose();
        }

        [Test]
        public void CopyToNativeArray_FromArray_ShouldCopyElementsToNativeArray()
        {
            int[] array = { 1, 2, 3, 4, 5 };
            var nativeArray = new NativeArray<int>(array.Length, Allocator.Temp);

            array.CopyTo(ref nativeArray);

            for (int i = 0; i < array.Length; i++)
            {
                Assert.AreEqual(array[i], nativeArray[i]);
            }

            nativeArray.Dispose();
        }

        [Test]
        public void CopyToNativeArray_FromList_ShouldCopyElementsToNativeArray()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var nativeArray = new NativeArray<int>(list.Count, Allocator.Temp);

            list.CopyTo(ref nativeArray);

            for (int i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(list[i], nativeArray[i]);
            }

            nativeArray.Dispose();
        }

        [Test]
        public void CopyToNativeArray_FromNativeList_ShouldCopyElementsToNativeArray()
        {
            var nativeList = new NativeList<int>(Allocator.Temp);
            nativeList.AddRange(new int[] { 1, 2, 3, 4, 5 });
            var nativeArray = new NativeArray<int>(nativeList.Length, Allocator.Temp);

            nativeList.CopyTo(ref nativeArray);

            for (int i = 0; i < nativeList.Length; i++)
            {
                Assert.AreEqual(nativeList[i], nativeArray[i]);
            }

            nativeList.Dispose();
            nativeArray.Dispose();
        }

        [Test]
        public void CopyToNativeList_FromArray_ShouldCopyElementsToNativeList()
        {
            int[] array = { 1, 2, 3, 4, 5 };
            var nativeList = new NativeList<int>(Allocator.Temp);

            array.CopyTo(ref nativeList);

            Assert.AreEqual(array.Length, nativeList.Length);
            for (int i = 0; i < array.Length; i++)
            {
                Assert.AreEqual(array[i], nativeList[i]);
            }

            nativeList.Dispose();
        }

        [Test]
        public void CopyToNativeList_FromList_ShouldCopyElementsToNativeList()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var nativeList = new NativeList<int>(Allocator.Temp);

            list.CopyTo(ref nativeList);

            Assert.AreEqual(list.Count, nativeList.Length);
            for (int i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(list[i], nativeList[i]);
            }

            nativeList.Dispose();
        }

        [Test]
        public void CopyToNativeList_FromNativeArray_ShouldCopyElementsToNativeList()
        {
            var nativeArray = new NativeArray<int>(new int[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            var nativeList = new NativeList<int>(Allocator.Temp);

            nativeArray.CopyTo(ref nativeList);

            Assert.AreEqual(nativeArray.Length, nativeList.Length);
            for (int i = 0; i < nativeArray.Length; i++)
            {
                Assert.AreEqual(nativeArray[i], nativeList[i]);
            }

            nativeArray.Dispose();
            nativeList.Dispose();
        }
    }
}
