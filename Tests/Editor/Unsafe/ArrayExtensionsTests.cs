using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe.Tests
{
    public class ArrayExtensionsTests
    {
        [Test]
        public void CalculateOffset_ShouldReturnCorrectOffset()
        {
            int[] array = {
                5, 10
            };
            int offset = ArrayExtensions.CalculateOffset(ref array[1], ref array[0]);
            Assert.AreEqual(UnsafeUtility.SizeOf<int>(), offset);

            array = new int[]
            {
                0, 1, 2, 3
            };
            offset = ArrayExtensions.CalculateOffset(ref array[3], ref array[1]);
            Assert.AreEqual(UnsafeUtility.SizeOf<int>() * 2, offset);
        }
    }
}
