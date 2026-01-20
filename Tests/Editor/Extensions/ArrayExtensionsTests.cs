using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace PKGE.Tests
{
    class ArrayExtensionsTests
    {
        //https://github.com/Unity-Technologies/InputSystem/blob/fb786d2a7d01b8bcb8c4218522e5f4b9afea13d7/Assets/Tests/InputSystem/Utilities/ArrayHelperTests.cs
        #region UnityEngine.InputSystem.Utilities
        [Test]
        [Category("Utilities")]
        public void Utilities_CanMoveArraySlice()
        {
            var array1 = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            var array2 = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            var array3 = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            var array4 = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            var array5 = new[] { 1, 2, 3, 4 };
            var array6 = new[] { 1, 2, 3, 4 };
            var array7 = new[] { 1, 2, 3, 4 };
            var array8 = new[] { 1, 2, 3, 4 };
            var array9 = new[] { 1, 2, 3, 4 };

            ArrayExtensions.MoveSlice(array1, 1, 6, 2);
            ArrayExtensions.MoveSlice(array2, 6, 1, 2);
            ArrayExtensions.MoveSlice(array3, 0, 5, 3);
            ArrayExtensions.MoveSlice(array4, 4, 2, 2);
            ArrayExtensions.MoveSlice(array5, 0, 2, 2);
            ArrayExtensions.MoveSlice(array6, 2, 1, 2);
            ArrayExtensions.MoveSlice(array7, 3, 0, 1);
            ArrayExtensions.MoveSlice(array8, 1, 0, 3);
            ArrayExtensions.MoveSlice(array9, 0, 1, 3);

            Assert.That(array1, Is.EqualTo(new[] { 1, 4, 5, 6, 7, 8, 2, 3 }));
            Assert.That(array2, Is.EqualTo(new[] { 1, 7, 8, 2, 3, 4, 5, 6 }));
            Assert.That(array3, Is.EqualTo(new[] { 4, 5, 6, 7, 8, 1, 2, 3 }));
            Assert.That(array4, Is.EqualTo(new[] { 1, 2, 5, 6, 3, 4, 7, 8 }));
            Assert.That(array5, Is.EqualTo(new[] { 3, 4, 1, 2 }));
            Assert.That(array6, Is.EqualTo(new[] { 1, 3, 4, 2 }));
            Assert.That(array7, Is.EqualTo(new[] { 4, 1, 2, 3 }));
            Assert.That(array8, Is.EqualTo(new[] { 2, 3, 4, 1 }));
            Assert.That(array9, Is.EqualTo(new[] { 4, 1, 2, 3 }));
        }

        [Test]
        [Category("Utilities")]
        public void Utilities_CanEraseInArrayWithCapacity()
        {
            var array1 = new[] { 1, 2, 3, 4, 5, 0, 0, 0 };
            var array2 = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            var array3 = new[] { 1, 2, 3, 4, 0, 0, 0, 0 };

            var array1Length = 5;
            var array2Length = 8;
            var array3Length = 4;

            ArrayExtensions.EraseAtWithCapacity(array1, ref array1Length, 2);
            ArrayExtensions.EraseAtWithCapacity(array2, ref array2Length, 7);
            ArrayExtensions.EraseAtWithCapacity(array3, ref array3Length, 0);

            Assert.That(array1, Is.EqualTo(new[] { 1, 2, 4, 5, 0, 0, 0, 0 }));
            Assert.That(array2, Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7, 0 }));
            Assert.That(array3, Is.EqualTo(new[] { 2, 3, 4, 0, 0, 0, 0, 0 }));

            Assert.That(array1Length, Is.EqualTo(4));
            Assert.That(array2Length, Is.EqualTo(7));
            Assert.That(array3Length, Is.EqualTo(3));
        }

        [Test]
        [Category("Utilities")]
        public void Utilities_IndexOfReference__IsUsingReferenceEqualsAndConstrainedByStartIndexAndCount()
        {
            var arr = new object[] { new object(), new object(), new object(), new object(), new object() };

            Assert.AreEqual(-1, arr.IndexOfReference(new object(), 0, arr.Length));
            Assert.AreEqual(-1, arr.IndexOfReference(arr[4], 4, 0));

            Assert.AreEqual(0, arr.IndexOfReference(arr[0], 0, arr.Length));
            Assert.AreEqual(4, arr.IndexOfReference(arr[4], 0, arr.Length));

            Assert.AreEqual(-1, arr.IndexOfReference(arr[0], 1, 3));
            Assert.AreEqual(-1, arr.IndexOfReference(arr[4], 1, 3));
            Assert.AreEqual(1, arr.IndexOfReference(arr[1], 1, 3));
            Assert.AreEqual(2, arr.IndexOfReference(arr[2], 1, 3));
        }

        [Test]
        [Category("Utilities")]
        public void Utilities_IndexOfPredicate__IsUsingPredicateForEqualityAndConstraintedByStartIndexAndCount()
        {
            var arr = new int[] { 0, 1, 2, 3, 4, 5 };

            Assert.That(arr.IndexOf(x => x >= 3, 0, arr.Length), Is.EqualTo(3));
            Assert.That(arr.IndexOf(x => x >= 3, 0, 3), Is.EqualTo(-1));
            Assert.That(arr.IndexOf(x => x >= 3, 1, 3), Is.EqualTo(3));
            Assert.That(arr.IndexOf(x => x >= 3, 4, 0), Is.EqualTo(-1));
            Assert.That(arr.IndexOf(x => x < 0, 3, 3), Is.EqualTo(-1));
        }
        #endregion // UnityEngine.InputSystem.Utilities

        #region ResizeArray_TransformAccessArray

        [Test]
        public void ResizeArray_TransformAccessArray_CopiesExistingElements()
        {
            var go1 = new GameObject("A");
            var go2 = new GameObject("B");
            var taa = new TransformAccessArray(2);
            taa.Add(go1.transform);
            taa.Add(go2.transform);

            taa.ResizeArray(4);

            Assert.AreEqual(2, taa.length);
            Assert.AreEqual(4, taa.capacity);
            Assert.AreEqual(go1.transform, taa[0]);
            Assert.AreEqual(go2.transform, taa[1]);

            taa.Dispose();
            UnityEngine.Object.DestroyImmediate(go1);
            UnityEngine.Object.DestroyImmediate(go2);
        }

        [Test]
        public void ResizeArray_TransformAccessArray_FromUncreatedArray_CreatesNew()
        {
            var taa = new TransformAccessArray(0);
            taa.Dispose(); // Now isCreated = false

            taa.ResizeArray(3);
            Assert.AreEqual(3, taa.capacity);

            taa.Dispose();
        }

        #endregion

        #region IsBytesEquals

        [Test]
        public void IsBytesEquals_ReturnsTrue_WhenSegmentsMatch()
        {
            byte[] a = { 1, 2, 3, 4, 5 };
            byte[] b = { 9, 2, 3, 4, 8 };

            Assert.IsTrue(a.IsBytesEquals(1, 3, b, 1, 3));
        }

        [Test]
        public void IsBytesEquals_ReturnsFalse_WhenLengthsDiffer()
        {
            byte[] a = { 1, 2, 3 };
            byte[] b = { 1, 2, 3, 4 };
            Assert.IsFalse(a.IsBytesEquals(0, 3, b, 0, 4));
        }

        #endregion

        #region StartsWith / EndsWith

        [Test]
        public void StartsWith_ReturnsTrue_WhenPatternMatchesStart()
        {
            var bytes = new byte[] { 10, 20, 30, 40 };
            var pattern = new byte[] { 20, 30 };
            Assert.IsTrue(bytes.StartsWith(1, 3, pattern));
        }

        [Test]
        public void EndsWith_ReturnsTrue_WhenPatternMatchesEnd()
        {
            var bytes = new byte[] { 10, 20, 30, 40 };
            var pattern = new byte[] { 30, 40 };
            Assert.IsTrue(bytes.EndsWith(0, 4, pattern));
        }

        #endregion

        #region IndexOfBytes

        [Test]
        public void IndexOfBytes_FindsPatternInArray()
        {
            var data = new byte[] { 1, 2, 3, 4, 2, 3 };
            var pattern = new byte[] { 2, 3 };
            int idx = data.IndexOfBytes(pattern, 0, data.Length);
            Assert.AreEqual(1, idx);
        }

        [Test]
        public void IndexOfBytes_ReturnsMinusOne_WhenPatternMissing()
        {
            var data = new byte[] { 1, 2, 3 };
            var pattern = new byte[] { 4, 5 };
            Assert.AreEqual(-1, data.IndexOfBytes(pattern, 0, 3));
        }

        #endregion

        #region SubSegment

        [Test]
        public void SubSegment_ReturnsCorrectSlice()
        {
            var arr = new int[] { 10, 20, 30, 40 };
            var seg = new ArraySegment<int>(arr, 1, 3);
            var sub = seg.SubSegment(1);

            Assert.AreEqual(2, sub.Count);
            Assert.AreEqual(30, sub.Array[sub.Offset]);
        }

        #endregion

        #region LengthSafe

        [Test]
        public void LengthSafe_ReturnsZero_ForNull()
        {
            int[] arr = null;
            Assert.AreEqual(0, arr.LengthSafe());
        }

        #endregion

        #region Clear

        [Test]
        public void Clear_ZeroesArray()
        {
            var arr = new int[] { 1, 2, 3 };
            arr.Clear();
            CollectionAssert.AreEqual(new int[3], arr);
        }

        [Test]
        public void Clear_WithCount_ZeroesPrefix()
        {
            var arr = new int[] { 1, 2, 3 };
            arr.Clear(2);
            Assert.AreEqual(0, arr[0]);
            Assert.AreEqual(0, arr[1]);
            Assert.AreEqual(3, arr[2]);
        }

        [Test]
        public void Clear_WithRefCount_SetsCountZero()
        {
            var arr = new int[] { 1, 2 };
            int c = 2;
            arr.Clear(ref c);
            Assert.AreEqual(0, c);
            CollectionAssert.AreEqual(new int[2], arr);
        }

        #endregion

        #region EnsureCapacity / DuplicateWithCapacity

        [Test]
        public void EnsureCapacity_CreatesArray_IfNull()
        {
            int[] arr = null;
            ArrayExtensions.EnsureCapacity(ref arr, 0, 5);
            Assert.IsTrue(arr.Length >= 5);
        }

        [Test]
        public void DuplicateWithCapacity_CopiesElements()
        {
            int[] arr = { 1, 2, 3 };
            ArrayExtensions.DuplicateWithCapacity(ref arr, 3, 5);
            Assert.AreEqual(1, arr[0]);
            Assert.GreaterOrEqual(arr.Length, 8);
        }

        #endregion

        #region Contains / ContainsReference

        [Test]
        public void Contains_ReturnsTrue_WhenElementPresent()
        {
            string[] arr = { "a", "b" };
            Assert.IsTrue(arr.Contains("b"));
        }

        [Test]
        public void ContainsReference_ReturnsTrue_WhenSameReferenceExists()
        {
            var obj = new object();
            object[] arr = { obj, new object() };
            Assert.IsTrue(arr.ContainsReference(obj));
        }

        #endregion

        #region HaveDuplicateReferences

        [Test]
        public void HaveDuplicateReferences_ReturnsTrue_WhenDuplicatesExist()
        {
            var o = new object();
            var arr = new[] { o, o, new object() };
            Assert.IsTrue(arr.HaveDuplicateReferences(0, 3));
        }

        #endregion

        #region HaveEqualElements

        [Test]
        public void HaveEqualElements_True_ForEqualArrays()
        {
            int[] a = { 1, 2, 3 };
            int[] b = { 1, 2, 3 };
            Assert.IsTrue(a.HaveEqualElements(b));
        }

        #endregion

        #region IndexOf / IndexOf (predicate)

        [Test]
        public void IndexOf_FindsIndex()
        {
            int[] a = { 1, 2, 3 };
            Assert.AreEqual(1, a.IndexOf(2));
        }

        [Test]
        public void IndexOf_Predicate_FindsMatch()
        {
            int[] a = { 5, 10 };
            Assert.AreEqual(0, a.IndexOf(v => v == 5));
        }

        #endregion

        #region IndexOfValue

        [Test]
        public void IndexOfValue_FindsStructValue()
        {
            var arr = new[] { 1, 2, 3 };
            Assert.AreEqual(2, arr.IndexOfValue(3));
        }

        #endregion
    }

    class ArrayPoolExtensionsTests
    {
        #region DisposeArrayPool<T>
        [Test]
        public void DisposeArrayPool_RentsArray_AndDisposesToPool()
        {
            using (var pooled = new DisposeArrayPool<int>(5))
            {
                Assert.IsNotNull(pooled.PooledArray);
                Assert.GreaterOrEqual(pooled.PooledArray.Length, 5);
            }
            // No explicit assert after Dispose — pooled array is returned internally
        }

        [Test]
        public void DisposeArrayPool_Rent_FactoryMethodBehavesSameAsCtor()
        {
            using (var pooled = DisposeArrayPool<int>.Rent(3))
            {
                Assert.IsNotNull(pooled.PooledArray);
                Assert.GreaterOrEqual(pooled.PooledArray.Length, 3);
            }
        }

        [Test]
        public void DisposeArrayPool_Throws_IfMinimumLengthNotPositive()
        {
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                var _ = new DisposeArrayPool<int>(0);
            });
        }
        #endregion // DisposeArrayPool<T>

        #region ResizableArrayPool<T>
        [Test]
        public void ResizableArrayPool_Rents_AndCanResize()
        {
            using (var pooled = new ResizableArrayPool<int>(2))
            {
                Assert.IsNotNull(pooled.PooledArray);
                Assert.GreaterOrEqual(pooled.PooledArray.Length, 2);

                var resized = pooled.Resize(10);
                Assert.GreaterOrEqual(resized.Length, 10);
                // Resizing preserves old content (default ints are 0) — check length only here
            }
        }

        [Test]
        public void ResizableArrayPool_Rent_FactoryMethodWorks()
        {
            using (var pooled = ResizableArrayPool<string>.Rent(4))
            {
                Assert.IsNotNull(pooled.PooledArray);
                Assert.GreaterOrEqual(pooled.PooledArray.Length, 4);
            }
        }

        [Test]
        public void ResizableArrayPool_ThrowsOnZeroLengthConstructionOrResize()
        {
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                var _ = new ResizableArrayPool<int>(0);
            });

            var ok = new ResizableArrayPool<int>(2);
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                ok.Resize(0);
            });
            ok.Dispose();
        }
        #endregion // ResizableArrayPool<T>

        #region ArrayPool<T>.Resize Extension
        [Test]
        public void ArrayPoolResize_CreatesArray_WhenNull()
        {
            int[] arr = null;
            ArrayPool<int>.Shared.Resize(ref arr, 5);
            Assert.IsNotNull(arr);
            Assert.GreaterOrEqual(arr.Length, 5);
        }

        [Test]
        public void ArrayPoolResize_DoesNothing_WhenSameSize()
        {
            int[] arr = ArrayPool<int>.Shared.Rent(5);
            int[] original = arr;
            ArrayPool<int>.Shared.Resize(ref arr, arr.Length);
            Assert.AreSame(original, arr);
            ArrayPool<int>.Shared.Return(arr);
        }

        [Test]
        public void ArrayPoolResize_CopiesData_WhenCopyArrayTrue()
        {
            int[] arr = ArrayPool<int>.Shared.Rent(4);
            arr[0] = 42;
            ArrayPool<int>.Shared.Resize(ref arr, 6, copyArray: true);
            Assert.AreEqual(42, arr[0]);
            ArrayPool<int>.Shared.Return(arr);
        }

        [Test]
        public void ArrayPoolResize_DoesNotCopy_WhenCopyArrayFalse()
        {
            int[] arr = ArrayPool<int>.Shared.Rent(4);
            arr[0] = 99;
            ArrayPool<int>.Shared.Resize(ref arr, 6, copyArray: false);
            Assert.AreNotEqual(99, arr[0]);
            ArrayPool<int>.Shared.Return(arr);
        }
        #endregion // ArrayPool<T>.Resize Extension

        #region EnsureCapacity
        [Test]
        public void EnsureCapacity_RentsNewArray_WhenNull()
        {
            string[] arr = null;
            ArrayPool<string>.Shared.EnsureCapacity(ref arr, 5);
            Assert.IsNotNull(arr);
            Assert.GreaterOrEqual(arr.Length, 5);
        }

        [Test]
        public void EnsureCapacity_ThrowsOnNegativeCapacity()
        {
            string[] arr = null;
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                ArrayPool<string>.Shared.EnsureCapacity(ref arr, -1);
            });
        }

        [Test]
        public void EnsureCapacity_DoesNothing_WhenCapacityAlreadyEnough()
        {
            var arr = ArrayPool<int>.Shared.Rent(10);
            int[] original = arr;
            ArrayPool<int>.Shared.EnsureCapacity(ref arr, 5);
            Assert.AreSame(original, arr);
            ArrayPool<int>.Shared.Return(arr);
        }

        [Test]
        public void EnsureCapacity_RentsBiggerArray_WhenTooSmall()
        {
            var arr = ArrayPool<int>.Shared.Rent(3);
            int[] original = arr;
            ArrayPool<int>.Shared.EnsureCapacity(ref arr, arr.Length * 2);
            Assert.AreNotSame(original, arr);
            ArrayPool<int>.Shared.Return(arr);
            ArrayPool<int>.Shared.Return(original);
        }
        #endregion // EnsureCapacity

        #region Resize
        [Test]
        public void StaticResize_DelegatesToSharedPool()
        {
            int[] arr = null;
            ArrayPoolExtensions.Resize(ref arr, 4);
            Assert.IsNotNull(arr);
            Assert.GreaterOrEqual(arr.Length, 4);
        }
        #endregion // Resize
    }

    class NativeArrayExtensionsTests
    {
        private NativeArray<int> _array;
        private int _count;

        [SetUp]
        public void SetUp()
        {
            _array = new NativeArray<int>(10, Allocator.Temp);
            _count = 0;
        }

        [TearDown]
        public void TearDown()
        {
            if (_array.IsCreated)
            {
                _array.Dispose();
            }
        }

        [Test]
        public void Resize_ShouldResizeArray()
        {
            ArrayExtensions.Resize(ref _array, 20, Allocator.Temp);
            Assert.AreEqual(20, _array.Length);

            ArrayExtensions.Resize(ref _array, 5, Allocator.Temp);
            Assert.AreEqual(5, _array.Length);
        }

        [Test]
        public void GrowBy_ShouldGrowArrayBySpecifiedCount()
        {
            const int grow = 10;
            int arrayLength = _array.Length;

            ArrayExtensions.GrowBy(ref _array, grow, Allocator.Temp);
            Assert.AreEqual(grow + arrayLength, _array.Length);
        }

        [Test]
        public void AppendWithCapacity_ShouldAppendValueToArray()
        {
            ArrayExtensions.AppendWithCapacity(ref _array, ref _count, 10, 10, Allocator.Temp);
            Assert.AreEqual(1, _count);
            Assert.AreEqual(10, _array[0]);
        }

        [Test]
        public void GrowWithCapacity_ShouldGrowArrayAndIncreaseCount()
        {
            var offset = ArrayExtensions.GrowWithCapacity(ref _array, ref _count, 5, 10, Allocator.Temp);
            Assert.AreEqual(5, _count);
            Assert.AreEqual(0, offset);
        }
    }
}
