using NUnit.Framework;
using System;
using System.Collections.Generic;
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
        [TestCaseSource(nameof(s_CopyToList))]
        public void CopyToList(List<TestData> data)
        {
            var dest = stackalloc TestData[data.Count];
            data.CopyTo(dest, data.Count);

            for (int i = 0; i < data.Count; ++i)
                Assert.AreEqual(data[i], dest[i]);
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
        [TestCaseSource(nameof(s_CopyToArray))]
        public void CopyToArray(TestData[] data)
        {
            var dest = stackalloc TestData[data.Length];
            data.CopyTo(dest, data.Length);

            for (int i = 0; i < data.Length; ++i)
                Assert.AreEqual(data[i], dest[i]);
        }

        static object[][] s_UintSortData = new object[][]
        {
            new object[] { new uint[] { 0 } },
            new object[] { new uint[] { 0, 1, 20123, 29, 0xffffff } },
            new object[] { new uint[] { 0xff1235, 92, 22125, 67358, 92123, 7012, 1234, 10000 } }, // Test with unique set
        };

        [Test]
        [TestCaseSource(nameof(s_UintSortData))]
        public void InsertionSort(uint[] values)
        {
            var array = new NativeArray<uint>(values, Allocator.Temp);
            CoreUnsafeUtils.InsertionSort(array, array.Length);
            for (int i = 0; i < array.Length - 1; ++i)
                Assert.LessOrEqual(array[i], array[i + 1]);

            array.Dispose();


            CoreUnsafeUtils.InsertionSort(values, values.Length);
            for (int i = 0; i < values.Length - 1; ++i)
                Assert.LessOrEqual(values[i], values[i + 1]);
        }

        [Test]
        [TestCaseSource(nameof(s_UintSortData))]
        public void MergeSort(uint[] values)
        {
            NativeArray<uint> supportArray = new NativeArray<uint>();
            var array = new NativeArray<uint>(values, Allocator.Temp);
            CoreUnsafeUtils.MergeSort(array, array.Length, ref supportArray);
            for (int i = 0; i < array.Length - 1; ++i)
                Assert.LessOrEqual(array[i], array[i + 1]);

            array.Dispose();
            supportArray.Dispose();


            var managedSupportArray = new uint[values.Length];
            CoreUnsafeUtils.MergeSort(values, values.Length, ref managedSupportArray);
            for (int i = 0; i < values.Length - 1; ++i)
                Assert.LessOrEqual(values[i], values[i + 1]);
        }

        [Test]
        [TestCaseSource(nameof(s_UintSortData))]
        public void RadixSort(uint[] values)
        {
            NativeArray<uint> supportArray = new NativeArray<uint>();
            var array = new NativeArray<uint>(values, Allocator.Temp);
            CoreUnsafeUtils.RadixSort(array, array.Length, ref supportArray);
            for (int i = 0; i < array.Length - 1; ++i)
                Assert.LessOrEqual(array[i], array[i + 1]);

            array.Dispose();
            supportArray.Dispose();


            var managedSupportArray = new uint[values.Length];
            CoreUnsafeUtils.RadixSort(values, values.Length, ref managedSupportArray);
            for (int i = 0; i < values.Length - 1; ++i)
                Assert.LessOrEqual(values[i], values[i + 1]);
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
        [TestCaseSource(nameof(s_PartialSortData))]
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
