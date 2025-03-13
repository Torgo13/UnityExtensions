using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe.Tests
{
    public class NativeArrayExtensionsTests
    {
        [Test]
        public void PtrToNativeArrayWithDefault_ShouldCreateNativeArrayAndCopySourceData()
        {
            int length = 5;
            int defaultValue = 0;
            int[] sourceArray = { 1, 2, 3, 4, 5 };
            int elementSize = UnsafeUtility.SizeOf<int>();

            unsafe
            {
                fixed (void* sourcePtr = sourceArray)
                {
                    var nativeArray = NativeCopyUtility.PtrToNativeArrayWithDefault(defaultValue, sourcePtr, elementSize, length, Allocator.Temp);

                    Assert.AreEqual(length, nativeArray.Length);
                    for (int i = 0; i < length; i++)
                    {
                        Assert.AreEqual(sourceArray[i], nativeArray[i]);
                    }

                    nativeArray.Dispose();
                }
            }
        }

        [Test]
        public void FillArrayWithValue_ShouldFillNativeArrayWithSpecifiedValue()
        {
            int length = 5;
            int value = 10;
            var nativeArray = new NativeArray<int>(length, Allocator.Temp);

            NativeCopyUtility.FillArrayWithValue(nativeArray, value);

            for (int i = 0; i < length; i++)
            {
                Assert.AreEqual(value, nativeArray[i]);
            }

            nativeArray.Dispose();
        }

        [Test]
        public void CreateArrayFilledWithValue_ShouldCreateNativeArrayAndFillWithSpecifiedValue()
        {
            int length = 5;
            int value = 10;
            var nativeArray = NativeCopyUtility.CreateArrayFilledWithValue(value, length, Allocator.Temp);

            Assert.AreEqual(length, nativeArray.Length);
            for (int i = 0; i < length; i++)
            {
                Assert.AreEqual(value, nativeArray[i]);
            }

            nativeArray.Dispose();
        }
        
        [Test]
        [Category("Utilities")]
        public void Utilities_CanEraseInNativeArrayWithCapacity()
        {
            var array1 = new NativeArray<int>(new[] { 1, 2, 3, 4, 5, 0, 0, 0 }, Allocator.Temp);
            var array2 = new NativeArray<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, Allocator.Temp);
            var array3 = new NativeArray<int>(new[] { 1, 2, 3, 4, 0, 0, 0, 0 }, Allocator.Temp);

            var array1Length = 5;
            var array2Length = 8;
            var array3Length = 4;

            try
            {
                array1.EraseAtWithCapacity(ref array1Length, 2);
                array2.EraseAtWithCapacity(ref array2Length, 7);
                array3.EraseAtWithCapacity(ref array3Length, 0);

                // For NativeArray, we don't clear memory.
                Assert.That(array1, Is.EqualTo(new[] { 1, 2, 4, 5, 5, 0, 0, 0 }));
                Assert.That(array2, Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
                Assert.That(array3, Is.EqualTo(new[] { 2, 3, 4, 4, 0, 0, 0, 0 }));

                Assert.That(array1Length, Is.EqualTo(4));
                Assert.That(array2Length, Is.EqualTo(7));
                Assert.That(array3Length, Is.EqualTo(3));
            }
            finally
            {
                array1.Dispose();
                array2.Dispose();
                array3.Dispose();
            }
        }

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
            NativeArrayExtensions.Resize(ref _array, 20, Allocator.Temp);
            Assert.AreEqual(20, _array.Length);

            NativeArrayExtensions.Resize(ref _array, 5, Allocator.Temp);
            Assert.AreEqual(5, _array.Length);
        }

        [Test]
        public void GrowBy_ShouldGrowArrayBySpecifiedCount()
        {
            NativeArrayExtensions.GrowBy(ref _array, 10, Allocator.Temp);
            Assert.AreEqual(20, _array.Length);
        }

        [Test]
        public void EraseAtWithCapacity_ShouldEraseElementAtSpecifiedIndex()
        {
            _array[0] = 1;
            _array[1] = 2;
            _array[2] = 3;
            _count = 3;

            _array.EraseAtWithCapacity(ref _count, 1);
            Assert.AreEqual(2, _count);
            Assert.AreEqual(1, _array[0]);
            Assert.AreEqual(3, _array[1]);
        }

        [Test]
        public void AppendWithCapacity_ShouldAppendValueToArray()
        {
            NativeArrayExtensions.AppendWithCapacity(ref _array, ref _count, 10, 10, Allocator.Temp);
            Assert.AreEqual(1, _count);
            Assert.AreEqual(10, _array[0]);
        }

        [Test]
        public void GrowWithCapacity_ShouldGrowArrayAndIncreaseCount()
        {
            var offset = NativeArrayExtensions.GrowWithCapacity(ref _array, ref _count, 5, 10, Allocator.Temp);
            Assert.AreEqual(5, _count);
            Assert.AreEqual(0, offset);
        }
    }
}
