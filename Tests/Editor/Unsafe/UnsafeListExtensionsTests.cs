using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using NUnitAssert = NUnit.Framework.Assert;

namespace PKGE.Unsafe.Tests
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
        public void AddRange_FromList_ShouldAddElementsToUnsafeList()
        {
            var allocator = Allocator.Temp;
            var unsafeList = new UnsafeList<int>(5, allocator);
            for (int i = 0; i < 5; i++)
                unsafeList.Add(i);
            List<int> list = new() { 6, 7, 8 };

            unsafeList.AddRange(list);

            Assert.AreEqual(8, unsafeList.Length);
            for (int i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(list[i], unsafeList[i + 5]);
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

        private UnsafeList<int> _intDefault;               // default (not created)
        private UnsafeList<int> _intEmptyCreated;          // created, capacity > 0, length == 0
        private UnsafeList<int> _intSmall;                 // created, small capacity, length set by helper
        private UnsafeList<MyStruct> _structList;          // created, for generic coverage

        [SetUp]
        public unsafe void SetUp()
        {
            // default (not created)
            _intDefault = default;

            // created (length 0)
            _intEmptyCreated = new UnsafeList<int>(4, Allocator.Persistent);
            _intEmptyCreated.Length = 0;

            // small list with values 1, 2, 3
            _intSmall = new UnsafeList<int>(4, Allocator.Persistent);
            Populate(ref _intSmall, new[] { 1, 2, 3 });

            // struct list with single element
            _structList = new UnsafeList<MyStruct>(2, Allocator.Persistent);
            Populate(ref _structList, new[] { new MyStruct { A = 7, B = 11 } });
        }

        [TearDown]
        public void TearDown()
        {
            DisposeIfCreated(ref _intEmptyCreated);
            DisposeIfCreated(ref _intSmall);
            DisposeIfCreated(ref _structList);
            // _intDefault is not created
        }

        #region GetPtr
        [Test]
        public unsafe void GetPtr_ReturnsNull_OnDefaultNotCreated()
        {
            var p = _intDefault.GetPtr<int>();
            NUnitAssert.AreEqual(IntPtr.Zero, (IntPtr)p);
        }

        [Test]
        public unsafe void GetPtr_ReturnsNull_OnCreatedButEmpty()
        {
            var p = _intEmptyCreated.GetPtr<int>();
            NUnitAssert.AreEqual(IntPtr.Zero, (IntPtr)p);
        }

        [Test]
        public unsafe void GetPtr_ReturnsListPtr_OnNonEmpty()
        {
            var p = _intSmall.GetPtr<int>();
            NUnitAssert.AreNotEqual(IntPtr.Zero, (IntPtr)p);
            NUnitAssert.AreEqual(new IntPtr(_intSmall.Ptr), (IntPtr)p);
        }
        #endregion // GetPtr

        #region GetReadOnlyPtr
        [Test]
        public unsafe void GetReadOnlyPtr_ReturnsNull_OnDefaultNotCreated()
        {
            var p = _intDefault.GetReadOnlyPtr<int>();
            NUnitAssert.AreEqual(IntPtr.Zero, (IntPtr)p);
        }

        [Test]
        public unsafe void GetReadOnlyPtr_ReturnsNull_OnCreatedButEmpty()
        {
            var p = _intEmptyCreated.GetReadOnlyPtr<int>();
            NUnitAssert.AreEqual(IntPtr.Zero, (IntPtr)p);
        }

        [Test]
        public unsafe void GetReadOnlyPtr_ReturnsReadOnlyPtr_OnNonEmpty()
        {
            var p = _intSmall.GetReadOnlyPtr<int>();
            NUnitAssert.AreNotEqual(IntPtr.Zero, (IntPtr)p);
            NUnitAssert.AreEqual(new IntPtr(_intSmall.AsReadOnly().Ptr), (IntPtr)p);
        }
        #endregion // GetReadOnlyPtr

        #region GetIntPtr
        [Test]
        public void GetIntPtr_ReturnsZero_OnDefaultNotCreated()
        {
            var ip = _intDefault.GetIntPtr();
            NUnitAssert.AreEqual(IntPtr.Zero, ip);
        }

        [Test]
        public void GetIntPtr_ReturnsZero_OnCreatedButEmpty()
        {
            var ip = _intEmptyCreated.GetIntPtr();
            NUnitAssert.AreEqual(IntPtr.Zero, ip);
        }

        [Test]
        public unsafe void GetIntPtr_ReturnsListPtr_OnNonEmpty()
        {
            var ip = _intSmall.GetIntPtr();
            NUnitAssert.AreNotEqual(IntPtr.Zero, ip);
            NUnitAssert.AreEqual(new IntPtr(_intSmall.Ptr), ip);
        }
        #endregion // GetIntPtr

        #region GetReadOnlyIntPtr
        [Test]
        public void GetReadOnlyIntPtr_ReturnsZero_OnDefaultNotCreated()
        {
            var ip = _intDefault.GetReadOnlyIntPtr();
            NUnitAssert.AreEqual(IntPtr.Zero, ip);
        }

        [Test]
        public void GetReadOnlyIntPtr_ReturnsZero_OnCreatedButEmpty()
        {
            var ip = _intEmptyCreated.GetReadOnlyIntPtr();
            NUnitAssert.AreEqual(IntPtr.Zero, ip);
        }

        [Test]
        public unsafe void GetReadOnlyIntPtr_EqualsReadOnlyPtr_OnNonEmpty()
        {
            var ip = _intSmall.GetReadOnlyIntPtr();
            NUnitAssert.AreNotEqual(IntPtr.Zero, ip);
            NUnitAssert.AreEqual(new IntPtr(_intSmall.AsReadOnly().Ptr), ip);
        }
        #endregion // GetReadOnlyIntPtr

        #region EnsureCapacity
        [Test]
        public void EnsureCapacity_Throws_WhenNotCreated()
        {
            // UnityEngine.Assertions.Assert throws UnityEngine.Assertions.AssertionException
            NUnitAssert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                UnsafeListExtensions.EnsureCapacity(ref _intDefault, 1);
            });
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void EnsureCapacity_Throws_WhenCapacityNotPositive(int requested)
        {
            var list = new UnsafeList<int>(4, Allocator.Persistent);
            try
            {
                NUnitAssert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                {
                    UnsafeListExtensions.EnsureCapacity(ref list, requested);
                });
            }
            finally
            {
                if (list.IsCreated)
                    list.Dispose();
            }
        }

        [Test]
        public void EnsureCapacity_Increases_WhenBelowRequested()
        {
            var list = new UnsafeList<int>(4, Allocator.Persistent)
            {
                0,
                0,
                0,
                0
            };
            try
            {
                NUnitAssert.AreEqual(4, list.Length);
                NUnitAssert.AreEqual(16, list.Capacity);
                UnsafeListExtensions.EnsureCapacity(ref list, 32);
                NUnitAssert.AreEqual(32, list.Capacity);
            }
            finally
            {
                if (list.IsCreated)
                    list.Dispose();
            }
        }

        [Test]
        public void EnsureCapacity_NoChange_WhenAlreadyEnough()
        {
            var list = new UnsafeList<int>(16, Allocator.Persistent);
            try
            {
                UnsafeListExtensions.EnsureCapacity(ref list, 10);
                NUnitAssert.AreEqual(16, list.Capacity);
            }
            finally
            {
                if (list.IsCreated)
                    list.Dispose();
            }
        }
        #endregion // EnsureCapacity

        #region EnsureRoom
        [Test]
        public void EnsureRoom_Throws_WhenNotCreated()
        {
            NUnitAssert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                UnsafeListExtensions.EnsureRoom(ref _intDefault, 1);
            });
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void EnsureRoom_Throws_WhenRoomNotPositive(int room)
        {
            var list = new UnsafeList<int>(4, Allocator.Persistent);
            try
            {
                NUnitAssert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                {
                    UnsafeListExtensions.EnsureRoom(ref list, room);
                });
            }
            finally
            {
                if (list.IsCreated)
                    list.Dispose();
            }
        }

        [Test]
        public unsafe void EnsureRoom_GrowsTo_LengthPlusRoom_WhenInsufficientCapacity()
        {
            var list = new UnsafeList<int>(4, Allocator.Persistent);
            try
            {
                // length = 3, capacity = 4 -> room 5 requires capacity >= 8
                Populate(ref list, new[] { 10, 20, 30 });
                NUnitAssert.AreEqual(3, list.Length);
                NUnitAssert.AreEqual(16, list.Capacity); // Minimum capacity is 16

                UnsafeListExtensions.EnsureRoom(ref list, 5);
                NUnitAssert.AreEqual(16, list.Capacity); // Minimum capacity is 16
            }
            finally
            {
                if (list.IsCreated)
                    list.Dispose();
            }
        }

        [Test]
        public unsafe void EnsureRoom_NoChange_WhenCapacityAlreadySufficient()
        {
            var list = new UnsafeList<int>(10, Allocator.Persistent);
            try
            {
                Populate(ref list, new[] { 10, 20, 30 }); // length = 3
                UnsafeListExtensions.EnsureRoom(ref list, 2); // needs 5
                NUnitAssert.AreEqual(16, list.Capacity); // Minimum capacity is 16
            }
            finally
            {
                if (list.IsCreated)
                    list.Dispose();
            }
        }
        #endregion // EnsureRoom

        #region AsSpan
        [Test]
        public void AsSpan_ReturnsEmpty_OnDefaultNotCreated()
        {
            var span = _intDefault.AsSpan();
            NUnitAssert.AreEqual(0, span.Length);
            NUnitAssert.IsTrue(span.IsEmpty);
        }

        [Test]
        public void AsSpan_ReturnsEmpty_OnCreatedButEmpty()
        {
            var span = _intEmptyCreated.AsSpan();
            NUnitAssert.AreEqual(0, span.Length);
            NUnitAssert.IsTrue(span.IsEmpty);
        }

        [Test]
        public unsafe void AsSpan_ExposesWritableView_BackedByListMemory()
        {
            var span = _intSmall.AsSpan();
            NUnitAssert.AreEqual(_intSmall.Length, span.Length);
            NUnitAssert.AreEqual(3, span.Length);
            // mutate via span
            span[1] = 99;

            // verify underlying memory changed
            var value = UnsafeUtility.ReadArrayElement<int>(_intSmall.Ptr, 1);
            NUnitAssert.AreEqual(99, value);
        }

        [Test]
        public unsafe void AsSpan_WorksWithStructs_WriteThroughUpdatesUnderlying()
        {
            var span = _structList.AsSpan();
            NUnitAssert.AreEqual(1, span.Length);
            var s = span[0];
            NUnitAssert.AreEqual(7, s.A);
            NUnitAssert.AreEqual(11, s.B);

            // write through span
            s.B = 42;
            span[0] = s;

            // verify via raw read
            var raw = UnsafeUtility.ReadArrayElement<MyStruct>(_structList.Ptr, 0);
            NUnitAssert.AreEqual(42, raw.B);
        }
        #endregion // AsSpan

        #region AsReadOnlySpan
        [Test]
        public void AsReadOnlySpan_ReturnsEmpty_OnDefaultNotCreated()
        {
            var ro = _intDefault.AsReadOnlySpan();
            NUnitAssert.AreEqual(0, ro.Length);
            NUnitAssert.IsTrue(ro.IsEmpty);
        }

        [Test]
        public void AsReadOnlySpan_ReturnsEmpty_OnCreatedButEmpty()
        {
            var ro = _intEmptyCreated.AsReadOnlySpan();
            NUnitAssert.AreEqual(0, ro.Length);
            NUnitAssert.IsTrue(ro.IsEmpty);
        }

        [Test]
        public void AsReadOnlySpan_ReflectsCurrentContent()
        {
            var ro = _intSmall.AsReadOnlySpan();
            NUnitAssert.AreEqual(_intSmall.Length, ro.Length);
            NUnitAssert.AreEqual(1, ro[0]);
            NUnitAssert.AreEqual(2, ro[1]);
            NUnitAssert.AreEqual(3, ro[2]);
        }
        #endregion // AsReadOnlySpan

        #region Helpers
        private static unsafe void Populate<T>(ref UnsafeList<T> list, T[] values) where T : unmanaged
        {
            if (!list.IsCreated)
                list = new UnsafeList<T>(values.Length, Allocator.Persistent);

            list.EnsureCapacity(values.Length);
            list.Length = values.Length;

            for (int i = 0; i < values.Length; i++)
                UnsafeUtility.WriteArrayElement(list.Ptr, i, values[i]);
        }

        private static void DisposeIfCreated<T>(ref UnsafeList<T> list) where T : unmanaged
        {
            if (list.IsCreated)
                list.Dispose();

            list = default;
        }

        private struct MyStruct
        {
            public int A;
            public int B;
        }
        #endregion // Helpers
    }
}
