using NUnit.Framework;
using System;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe.Tests
{
    public class ByteUtilityTests
    {
        [Test]
        public void GetSetBit_Byte_Works()
        {
            byte field = 0;
            Assert.IsFalse(ByteUtility.GetBit(field, (byte)0));

            ByteUtility.SetBit(ref field, 0, true);
            Assert.IsTrue(ByteUtility.GetBit(field, (byte)0));

            ByteUtility.SetBit(ref field, 0, false);
            Assert.IsFalse(ByteUtility.GetBit(field, (byte)0));
        }

        [Test]
        public void GetSetBit_UShort_Works()
        {
            ushort field = 0;
            Assert.IsFalse(ByteUtility.GetBit(field, (ushort)5));

            ByteUtility.SetBit(ref field, 5, true);
            Assert.IsTrue(ByteUtility.GetBit(field, (ushort)5));

            ByteUtility.SetBit(ref field, 5, false);
            Assert.IsFalse(ByteUtility.GetBit(field, (ushort)5));
        }

        [Test]
        public void GetSetBit_UInt_Works()
        {
            uint field = 0;
            Assert.IsFalse(ByteUtility.GetBit(field, 10));

            ByteUtility.SetBit(ref field, 10, true);
            Assert.IsTrue(ByteUtility.GetBit(field, 10));

            ByteUtility.SetBit(ref field, 10, false);
            Assert.IsFalse(ByteUtility.GetBit(field, 10));
        }

        [Test]
        public void GetSetBit_ULong_Works()
        {
            ulong field = 0;
            Assert.IsFalse(ByteUtility.GetBit(field, 20));

            ByteUtility.SetBit(ref field, 20, true);
            Assert.IsTrue(ByteUtility.GetBit(field, 20));

            ByteUtility.SetBit(ref field, 20, false);
            Assert.IsFalse(ByteUtility.GetBit(field, 20));
        }

        [Test]
        public void GetSetBit_Int_Works()
        {
            int field = 0;
            Assert.IsFalse(ByteUtility.GetBit(field, 3));

            ByteUtility.SetBit(ref field, 3, true);
            Assert.IsTrue(ByteUtility.GetBit(field, 3));

            ByteUtility.SetBit(ref field, 3, false);
            Assert.IsFalse(ByteUtility.GetBit(field, 3));
        }

        [Test]
        public void SetBit_PreservesOtherBits()
        {
            int field = 0b1010; // bit1 and bit3 set
            ByteUtility.SetBit(ref field, 2, true);  // set bit2
            Assert.AreEqual(0b1110, field);

            ByteUtility.SetBit(ref field, 1, false); // clear bit1
            Assert.AreEqual(0b1100, field);
        }

        [Test]
        public void GetBit_MultiplePositions()
        {
            uint field = 0;
            for (ushort i = 0; i < 8; i++)
            {
                ByteUtility.SetBit(ref field, i, i % 2 == 0);
                Assert.AreEqual(i % 2 == 0, ByteUtility.GetBit(field, i));
            }
        }
    }

    public class ByteExtensionsTests
    {
        [Test]
        public void ToByte_ShouldConvertBoolToByte()
        {
            Assert.AreEqual(1, true.ToByte());
            Assert.AreEqual(0, false.ToByte());
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
