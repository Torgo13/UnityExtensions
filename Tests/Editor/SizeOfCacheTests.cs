using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace UnityExtensions.Tests
{
    public class SizeOfCacheTests
    {
        private struct TestStruct
        {
            public int IntField;
            public float FloatField;
        }

        private enum TestEnum : byte
        {
            Value1,
            Value2,
            Value3
        }

        private enum DefaultEnum
        {
            A,
            B,
            C
        }

        /// <summary>
        /// Confirms the size calculation for a user-defined struct.
        /// </summary>
        [Test]
        public void SizeOfCache_ReturnsCorrectSize_ForStruct()
        {
            // Act
            int size = SizeOfCache<TestStruct>.Size;

            // Assert
            Assert.AreEqual(Marshal.SizeOf(typeof(TestStruct)), size,
                "SizeOfCache should return the correct marshalled size for a struct.");
        }

        /// <summary>
        /// Verifies the size for an enum with a non-default (e.g., byte) underlying type.
        /// </summary>
        [Test]
        public void SizeOfCache_ReturnsCorrectSize_ForEnumWithCustomUnderlyingType()
        {
            // Act
            int size = SizeOfCache<TestEnum>.Size;

            // Assert
            Assert.AreEqual(Marshal.SizeOf(typeof(byte)), size,
                "SizeOfCache should return the correct size for an enum with a custom underlying type.");
        }

        /// <summary>
        /// Ensures proper handling of enums with default underlying types.
        /// </summary>
        [Test]
        public void SizeOfCache_ReturnsCorrectSize_ForDefaultEnum()
        {
            // Act
            int size = SizeOfCache<DefaultEnum>.Size;

            // Assert
            Assert.AreEqual(Marshal.SizeOf(Enum.GetUnderlyingType(typeof(DefaultEnum))), size,
                "SizeOfCache should return the correct size for an enum with the default underlying type.");
        }
    }
}
