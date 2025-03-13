// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using NUnit.Framework;

namespace UnityExtensions.Unsafe.Tests
{
    class MathematicsExtensionsTests
    {
        //https://github.com/Unity-Technologies/InputSystem/blob/develop/Assets/Tests/InputSystem/Utilities/NumberHelpersTests.cs
        #region InputSystem

        [Test]
        [Category("Utilities")]
        // out of boundary tests
        [TestCase(0U, 1U, 2U, 0.0f)]
        [TestCase(3U, 1U, 2U, 1.0f)]
        // [10, 30]
        [TestCase(10U, 10U, 30U, 0.0f)]
        [TestCase(25U, 10U, 30U, 0.75f)]
        [TestCase(30U, 10U, 30U, 1.0f)]
        // [0, 255]
        [TestCase(0U, byte.MinValue, byte.MaxValue, 0.0f)]
        [TestCase(128U, byte.MinValue, byte.MaxValue, 0.501960813999176025391f)]
        [TestCase(255U, byte.MinValue, byte.MaxValue, 1.0f)]
        // [0, 65535]
        [TestCase(0U, ushort.MinValue, ushort.MaxValue, 0.0f)]
        [TestCase(32767U, ushort.MinValue, ushort.MaxValue, 0.49999237060546875f)]
        [TestCase(65535U, ushort.MinValue, ushort.MaxValue, 1.0f)]
        // [0, 4294967295]
        [TestCase(0U, uint.MinValue, uint.MaxValue, 0.0f)]
        [TestCase(2147483647U, uint.MinValue, uint.MaxValue, 0.5f)]
        [TestCase(4294967295U, uint.MinValue, uint.MaxValue, 1.0f)]
        public void Utilities_NumberHelpers_CanConvertUIntToNormalizedFloatAndBack(uint value, uint minValue, uint maxValue, float expected)
        {
            var result = value.UIntToNormalizedFloat(minValue, maxValue);
            Assert.That(result, Is.EqualTo(expected).Within(float.Epsilon));

            var integer = result.NormalizedFloatToUInt(minValue, maxValue);
            Assert.That(integer, Is.EqualTo(Clamp(value, minValue, maxValue)));
        }

        // Mathf.Clamp is not overloaded for uint's, Math.Clamp is only available in .NET core 2.0+ / .NET 5
        private static uint Clamp(uint value, uint min, uint max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }

        #endregion // InputSystem

        [Test]
        public void AlignToMultipleOfInt_ShouldAlignNumberToMultipleOfAlignment()
        {
            Assert.AreEqual(10, 10.AlignToMultipleOf(5));
            Assert.AreEqual(12, 10.AlignToMultipleOf(4));
            Assert.AreEqual(15, 13.AlignToMultipleOf(5));
        }

        [Test]
        public void AlignToMultipleOfLong_ShouldAlignNumberToMultipleOfAlignment()
        {
            Assert.AreEqual(10L, 10L.AlignToMultipleOf(5L));
            Assert.AreEqual(12L, 10L.AlignToMultipleOf(4L));
            Assert.AreEqual(15L, 13L.AlignToMultipleOf(5L));
        }

        [Test]
        public void AlignToMultipleOfUInt_ShouldAlignNumberToMultipleOfAlignment()
        {
            Assert.AreEqual(10u, 10u.AlignToMultipleOf(5u));
            Assert.AreEqual(12u, 10u.AlignToMultipleOf(4u));
            Assert.AreEqual(15u, 13u.AlignToMultipleOf(5u));
        }

        [Test]
        public void UIntToNormalizedFloat_ShouldConvertUIntToNormalizedFloat()
        {
            Assert.AreEqual(0.0f, 0u.UIntToNormalizedFloat(0, 10));
            Assert.AreEqual(1.0f, 10u.UIntToNormalizedFloat(0, 10));
            Assert.AreEqual(0.5f, 5u.UIntToNormalizedFloat(0, 10));
        }

        [Test]
        public void NormalizedFloatToUInt_ShouldConvertNormalizedFloatToUInt()
        {
            Assert.AreEqual(0u, 0.0f.NormalizedFloatToUInt(0, 10));
            Assert.AreEqual(10u, 1.0f.NormalizedFloatToUInt(0, 10));
            Assert.AreEqual(5u, 0.5f.NormalizedFloatToUInt(0, 10));
        }

        [Test]
        public void RemapUIntBitsToNormalizeFloatToUIntBits_ShouldRemapUIntBits()
        {
            Assert.AreEqual(0u, 0u.RemapUIntBitsToNormalizeFloatToUIntBits(8, 16));
            Assert.AreEqual(65535u, 255u.RemapUIntBitsToNormalizeFloatToUIntBits(8, 16));
            Assert.AreEqual(32639u, 127u.RemapUIntBitsToNormalizeFloatToUIntBits(8, 16));
        }
    }
}