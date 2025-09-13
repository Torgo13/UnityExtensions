using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PKGE.Unsafe.Tests
{
    public class NativeViewTests
    {
        [Test]
        public void AsNativeView_FromNativeArray_ShouldReturnNativeViewWithCorrectPtrAndCount()
        {
            var nativeArray = new NativeArray<int>(new int[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            var nativeView = nativeArray.AsNativeView();

            Assert.AreEqual(nativeArray.Length, nativeView.Count);
            unsafe
            {
                Assert.IsTrue(nativeArray.GetUnsafePtr() == nativeView.Ptr);
            }

            nativeArray.Dispose();
        }

        [Test]
        public void AsNativeView_FromNativeSlice_ShouldReturnNativeViewWithCorrectPtrAndCount()
        {
            var nativeArray = new NativeArray<int>(new int[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            var nativeSlice = new NativeSlice<int>(nativeArray, 1, 3);
            var nativeView = nativeSlice.AsNativeView();

            Assert.AreEqual(nativeSlice.Length, nativeView.Count);
            unsafe
            {
                Assert.IsTrue(nativeSlice.GetUnsafePtr() == nativeView.Ptr);
            }

            nativeArray.Dispose();
        }

        [Test]
        public void NativeView_ShouldHandleEmptyArray()
        {
            var emptyArray = new NativeArray<int>(0, Allocator.Temp);
            var nativeView = emptyArray.AsNativeView();

            Assert.AreEqual(emptyArray.Length, nativeView.Count);
            unsafe
            {
                Assert.IsTrue(null != nativeView.Ptr);
            }

            emptyArray.Dispose();
        }

        [Test]
        public void NativeView_ShouldHandleEmptySlice()
        {
            var nativeArray = new NativeArray<int>(new int[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            var emptySlice = new NativeSlice<int>(nativeArray, 0, 0);
            var nativeView = emptySlice.AsNativeView();

            Assert.AreEqual(emptySlice.Length, nativeView.Count);
            unsafe
            {
                Assert.IsTrue(null != nativeView.Ptr);
            }

            nativeArray.Dispose();
        }
    }
}
