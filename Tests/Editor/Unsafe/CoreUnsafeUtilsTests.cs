using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using UnityExtensions.Unsafe;

namespace UnityExtensions.Editor.Unsafe.Tests
{
    unsafe class CoreUnsafeUtilsTests
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Tests/Editor/CoreUnsafeUtilsTests.cs
        #region UnityEditor.Rendering.Tests
        public struct TestData : IEquatable<TestData>
        {
            public int intValue;
            public float floatValue;

            public bool Equals(TestData other)
            {
                return intValue == other.intValue && floatValue == other.floatValue;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is TestData))
                    return false;
                return Equals((TestData)obj);
            }

            public override int GetHashCode()
            {
                fixed (float* fptr = &floatValue)
                    return intValue ^ *(int*)fptr;
            }
        }

        static object[][] s_CopyToList = new object[][]
        {
            new object[] { new List<TestData>
            {
                new TestData { floatValue = 2, intValue = 1 },
                new TestData { floatValue = 3, intValue = 2 },
                new TestData { floatValue = 4, intValue = 3 },
                new TestData { floatValue = 5, intValue = 4 },
                new TestData { floatValue = 6, intValue = 5 },
            } }
        };

        [Test]
        [TestCaseSource("s_CopyToList")]
        public void CopyToList(List<TestData> datas)
        {
            var dest = stackalloc TestData[datas.Count];
            datas.CopyTo(dest, datas.Count);

            for (int i = 0; i < datas.Count; ++i)
                Assert.AreEqual(datas[i], dest[i]);
        }

        static object[][] s_CopyToArray = new object[][]
        {
            new object[] { new TestData[]
            {
                new TestData { floatValue = 2, intValue = 1 },
                new TestData { floatValue = 3, intValue = 2 },
                new TestData { floatValue = 4, intValue = 3 },
                new TestData { floatValue = 5, intValue = 4 },
                new TestData { floatValue = 6, intValue = 5 },
            } }
        };

        [Test]
        [TestCaseSource("s_CopyToArray")]
        public void CopyToArray(TestData[] datas)
        {
            var dest = stackalloc TestData[datas.Length];
            datas.CopyTo(dest, datas.Length);

            for (int i = 0; i < datas.Length; ++i)
                Assert.AreEqual(datas[i], dest[i]);
        }

        static object[][] s_UintSortData = new object[][]
        {
            new object[] { new uint[] { 0 } },
            new object[] { new uint[] { 0, 1, 20123, 29, 0xffffff } },
            new object[] { new uint[] { 0xff1235, 92, 22125, 67358, 92123, 7012, 1234, 10000 } }, // Test with unique set
        };

        [Test]
        [TestCaseSource("s_UintSortData")]
        public void InsertionSort(uint[] values)
        {
            var array = new NativeArray<uint>(values, Allocator.Temp);
            CoreUnsafeUtils.InsertionSort(array, array.Length);
            for (int i = 0; i < array.Length - 1; ++i)
                Assert.LessOrEqual(array[i], array[i + 1]);

            array.Dispose();
        }

        [Test]
        [TestCaseSource("s_UintSortData")]
        public void MergeSort(uint[] values)
        {
            NativeArray<uint> supportArray = new NativeArray<uint>();
            var array = new NativeArray<uint>(values, Allocator.Temp);
            CoreUnsafeUtils.MergeSort(array, array.Length, ref supportArray);
            for (int i = 0; i < array.Length - 1; ++i)
                Assert.LessOrEqual(array[i], array[i + 1]);

            array.Dispose();
            supportArray.Dispose();
        }

        [Test]
        [TestCaseSource("s_UintSortData")]
        public void RadixSort(uint[] values)
        {
            NativeArray<uint> supportArray = new NativeArray<uint>();
            var array = new NativeArray<uint>(values, Allocator.Temp);
            CoreUnsafeUtils.RadixSort(array, array.Length, ref supportArray);
            for (int i = 0; i < array.Length - 1; ++i)
                Assert.LessOrEqual(array[i], array[i + 1]);

            array.Dispose();
            supportArray.Dispose();
        }

        static object[][] s_PartialSortData = new object[][]
        {
            new object[] { new uint[] { 2, 8, 9, 2, 4, 0, 1, 0, 1, 0 } }
        };
        private enum SortAlgorithm
        {
            Insertion,
            Merge,
            Radix
        };

        [Test]
        [TestCaseSource("s_PartialSortData")]
        public void PartialSortInsertionMergeRadix(uint[] values)
        {
            NativeArray<uint> supportArray = new NativeArray<uint>();
            int sortCount = 5;

            foreach (var algorithmId in Enum.GetValues(typeof(SortAlgorithm)))
            {
                var algorithmValue = (SortAlgorithm)algorithmId;
                var array = new NativeArray<uint>(values, Allocator.Temp);
                if (algorithmValue == SortAlgorithm.Insertion)
                    CoreUnsafeUtils.InsertionSort(array, sortCount);
                else if (algorithmValue == SortAlgorithm.Merge)
                    CoreUnsafeUtils.MergeSort(array, sortCount, ref supportArray);
                else if (algorithmValue == SortAlgorithm.Radix)
                    CoreUnsafeUtils.RadixSort(array, sortCount, ref supportArray);

                for (int i = 0; i < sortCount - 1; ++i)
                    Assert.LessOrEqual(array[i], array[i + 1]);
                for (int i = sortCount; i < array.Length; ++i)
                    Assert.That(array[i] == 0 || array[i] == 1);
                array.Dispose();
            }

            supportArray.Dispose();
        }
        #endregion // UnityEditor.Rendering.Tests
    }
}
