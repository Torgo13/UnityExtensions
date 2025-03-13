using NUnit.Framework;
using Unity.Collections;

namespace UnityExtensions.Unsafe.Tests
{
    public class CollectionsExtensionsTests
    {
        [Test]
        public unsafe void GetPointer_ShouldReturnNullForEmptyArray()
        {
            var emptyArray = new NativeArray<int>(0, Allocator.Temp);
            Assert.IsTrue(null == emptyArray.GetPointer());
        }

        [Test]
        public unsafe void GetPointer_ShouldReturnValidPointerForNonEmptyArray()
        {
            var array = new NativeArray<int>(10, Allocator.Temp);
            Assert.IsTrue(null != array.GetPointer());
        }
    }
}
