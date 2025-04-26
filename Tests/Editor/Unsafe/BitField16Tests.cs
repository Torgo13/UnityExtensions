using System;
using NUnit.Framework;
using Unity.Collections;

namespace UnityExtensions.Unsafe.Tests
{
    class BitFieldTests
    {
        [Test]
        public void BitField16_Get_Set()
        {
            var test = new BitField16();

            uint bits;

            bits = test.GetBits(0, 16);
            Assert.AreEqual(0x0, bits);

            test.SetBits(0, true);
            bits = test.GetBits(0, 16);
            Assert.AreEqual(0x1, bits);

            test.SetBits(0, true, 16);
            bits = test.GetBits(0, 16);
            Assert.AreEqual(0xffff, bits);
            Assert.IsTrue(test.TestAll(0, 16));

            test.SetBits(0, false, 16);
            bits = test.GetBits(0, 16);
            Assert.AreEqual(0x0, bits);

            test.SetBits(6, true, 7);
            Assert.IsTrue(test.TestAll(6, 7));
            test.SetBits(3, true, 3);
            Assert.IsTrue(test.TestAll(3, 3));
            bits = test.GetBits(0, 16);
            Assert.AreEqual(0x1ff8, bits);
            bits = test.GetBits(0, 15);
            Assert.AreEqual(0x1ff8, bits);
            Assert.IsFalse(test.TestNone(0, 16));
            Assert.IsFalse(test.TestAll(0, 16));
            Assert.IsTrue(test.TestAny(0, 16));
        }

        [Test]
        public void BitField16_Count_Leading_Trailing()
        {
            var test = new BitField16();

            Assert.AreEqual(0, test.CountBits());
            Assert.AreEqual(16, test.CountLeadingZeros());
            Assert.AreEqual(16, test.CountTrailingZeros());

            test.SetBits(15, true);
            Assert.AreEqual(1, test.CountBits());
            Assert.AreEqual(0, test.CountLeadingZeros());
            Assert.AreEqual(15, test.CountTrailingZeros());

            test.SetBits(0, true);
            Assert.AreEqual(2, test.CountBits());
            Assert.AreEqual(0, test.CountLeadingZeros());
            Assert.AreEqual(0, test.CountTrailingZeros());

            test.SetBits(15, false);
            Assert.AreEqual(1, test.CountBits());
            Assert.AreEqual(15, test.CountLeadingZeros());
            Assert.AreEqual(0, test.CountTrailingZeros());
        }

        [Test]
        public void BitField16_Throws()
        {
            var test = new BitField16();

            for (byte i = 0; i < 16; ++i)
            {
                Assert.DoesNotThrow(() => { test.GetBits(i, (byte)(16 - i)); });
            }

            Assert.Throws<ArgumentException>(() => { test.GetBits(0, 17); });
            Assert.Throws<ArgumentException>(() => { test.GetBits(1, 16); });
        }
    }
}
