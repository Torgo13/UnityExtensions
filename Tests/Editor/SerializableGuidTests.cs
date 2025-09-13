using NUnit.Framework;
using System;

namespace PKGE.Tests
{
    public class SerializableGuidTests
    {
        /// <summary>
        /// Verifies that SerializableGuid.Empty maps correctly to Guid.Empty.
        /// </summary>
        [Test]
        public void SerializableGuid_Empty_ReturnsGuidEmpty()
        {
            // Act
            var emptyGuid = SerializableGuid.Empty;

            // Assert
            Assert.AreEqual(Guid.Empty, emptyGuid.Guid, "SerializableGuid.Empty should represent Guid.Empty.");
        }

        /// <summary>
        /// Ensures the SerializableGuid is properly constructed from two ulong values.
        /// </summary>
        [Test]
        public void SerializableGuid_ConstructsFromUlongs()
        {
            // Arrange
            ulong guidLow = 0x1234567890ABCDEF;
            ulong guidHigh = 0xFEDCBA0987654321;

            // Act
            var serializableGuid = new SerializableGuid(guidLow, guidHigh);

            // Assert
            Assert.AreEqual(guidLow, serializableGuid.Guid.ToByteArray().AsUlongLow(),
                "The low 8 bytes of the GUID should match the provided value.");
            Assert.AreEqual(guidHigh, serializableGuid.Guid.ToByteArray().AsUlongHigh(),
                "The high 8 bytes of the GUID should match the provided value.");
        }

        /// <summary>
        /// Confirms that equivalent SerializableGuid instances are considered equal.
        /// </summary>
        [Test]
        public void SerializableGuid_Equals_ReturnsTrueForEquivalentGuids()
        {
            // Arrange
            ulong guidLow = 0x1234567890ABCDEF;
            ulong guidHigh = 0xFEDCBA0987654321;
            var guid1 = new SerializableGuid(guidLow, guidHigh);
            var guid2 = new SerializableGuid(guidLow, guidHigh);

            // Act & Assert
            Assert.IsTrue(guid1.Equals(guid2), "Equals should return true for equivalent SerializableGuid instances.");
            Assert.IsTrue(guid1 == guid2,
                "Equality operator should return true for equivalent SerializableGuid instances.");
            Assert.IsFalse(guid1 != guid2,
                "Inequality operator should return false for equivalent SerializableGuid instances.");
        }

        /// <summary>
        /// Ensures that different SerializableGuid instances are not considered equal.
        /// </summary>
        [Test]
        public void SerializableGuid_Equals_ReturnsFalseForDifferentGuids()
        {
            // Arrange
            var guid1 = new SerializableGuid(0x1234567890ABCDEF, 0xFEDCBA0987654321);
            var guid2 = new SerializableGuid(0x1111111111111111, 0x2222222222222222);

            // Act & Assert
            Assert.IsFalse(guid1.Equals(guid2), "Equals should return false for different SerializableGuid instances.");
            Assert.IsFalse(guid1 == guid2,
                "Equality operator should return false for different SerializableGuid instances.");
            Assert.IsTrue(guid1 != guid2,
                "Inequality operator should return true for different SerializableGuid instances.");
        }

        /// <summary>
        /// Confirms the string representation matches the Guid's representation.
        /// </summary>
        [Test]
        public void SerializableGuid_ToString_MatchesGuidToString()
        {
            // Arrange
            ulong guidLow = 0x1234567890ABCDEF;
            ulong guidHigh = 0xFEDCBA0987654321;
            var serializableGuid = new SerializableGuid(guidLow, guidHigh);

            // Act
            string guidString = serializableGuid.ToString();

            // Assert
            Assert.AreEqual(serializableGuid.Guid.ToString(), guidString,
                "ToString should match the string representation of the underlying Guid.");
        }

        /// <summary>
        /// Verifies consistent hash codes for equivalent instances.
        /// </summary>
        [Test]
        public void SerializableGuid_GetHashCode_MatchesForEquivalentGuids()
        {
            // Arrange
            var guid1 = new SerializableGuid(0x1234567890ABCDEF, 0xFEDCBA0987654321);
            var guid2 = new SerializableGuid(0x1234567890ABCDEF, 0xFEDCBA0987654321);

            // Act & Assert
            Assert.AreEqual(guid1.GetHashCode(), guid2.GetHashCode(),
                "GetHashCode should produce the same value for equivalent SerializableGuid instances.");
        }
    }

    /// <summary>
    /// Helper extension methods for validation
    /// </summary>
    public static class GuidExtensions
    {
        public static ulong AsUlongLow(this byte[] bytes)
        {
            return BitConverter.ToUInt64(bytes, 0);
        }

        public static ulong AsUlongHigh(this byte[] bytes)
        {
            return BitConverter.ToUInt64(bytes, 8);
        }
    }
}
