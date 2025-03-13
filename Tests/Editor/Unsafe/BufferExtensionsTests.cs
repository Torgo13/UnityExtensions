using NUnit.Framework;
using System;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe.Tests
{
    public class ByteExtensionsTests
    {
        [Test]
        public void ToByte_ShouldConvertBoolToByte()
        {
            bool valueTrue = true;
            bool valueFalse = false;

            byte byteTrue = valueTrue.ToByte();
            byte byteFalse = valueFalse.ToByte();

            Assert.AreEqual(1, byteTrue);
            Assert.AreEqual(0, byteFalse);
        }
    }

    public class BufferExtensionsTests
    {
        [Test]
        public void WriteStruct_ShouldWriteStructToBuffer()
        {
            byte[] buffer = new byte[20];
            int value = 12345;
            int offset = 0;

            offset = buffer.WriteStruct(ref value, offset);

            Assert.AreEqual(12345, BitConverter.ToInt32(buffer, 0));
            Assert.AreEqual(UnsafeUtility.SizeOf<int>(), offset);
        }

        [Test]
        public void ReadStruct_ShouldReadStructFromBuffer()
        {
            byte[] buffer = new byte[20];
            int value = 12345;
            int offset = buffer.WriteStruct(ref value);

            int readValue = buffer.ReadStruct<int>(0);

            Assert.AreEqual(12345, readValue);
        }

        [Test]
        public void ReadStructWithNextOffset_ShouldReadStructAndReturnNextOffset()
        {
            byte[] buffer = new byte[20];
            int value = 12345;
            int offset = buffer.WriteStruct(ref value);

            int readValue = buffer.ReadStruct<int>(0, out int nextOffset);

            Assert.AreEqual(12345, readValue);
            Assert.AreEqual(UnsafeUtility.SizeOf<int>(), nextOffset);
        }
    }
}
