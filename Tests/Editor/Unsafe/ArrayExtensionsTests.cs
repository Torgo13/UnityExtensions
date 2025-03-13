using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe.Tests
{
    public class ArrayExtensionsTests
    {
        [Test]
        public void CalculateOffset_ShouldReturnCorrectOffset()
        {
            int[] array = new int[2];
            array[0] = 5;
            array[1] = 10;
            int offset = ArrayExtensions.CalculateOffset(ref array[1], ref array[0]);
            Assert.AreEqual(UnsafeUtility.SizeOf<int>(), offset);
        }
    }
}
