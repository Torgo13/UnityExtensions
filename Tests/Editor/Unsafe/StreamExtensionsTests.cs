using NUnit.Framework;
using System.IO;
using Unity.Collections;

namespace UnityExtensions.Unsafe.Tests
{
    public class StreamExtensionsTests
    {
        [Test]
        public void Write_ShouldWriteNativeArrayToStream()
        {
            var nativeArray = new NativeArray<byte>(new byte[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            var memoryStream = new MemoryStream();

            memoryStream.Write(nativeArray);

            Assert.AreEqual(nativeArray.Length, memoryStream.Length);

            memoryStream.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[nativeArray.Length];
            var byteCount = memoryStream.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(nativeArray.Length, byteCount);

            for (int i = 0; i < nativeArray.Length; i++)
            {
                Assert.AreEqual(nativeArray[i], buffer[i]);
            }

            nativeArray.Dispose();
        }
    }
}
