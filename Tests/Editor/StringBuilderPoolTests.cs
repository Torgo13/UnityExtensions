using NUnit.Framework;
using System.Text;

namespace UnityExtensions.Tests
{
    public class StringBuilderPoolTests
    {
        /// <summary>
        /// Ensures that Get retrieves a non-null and cleared StringBuilder.
        /// </summary>
        [Test]
        public void StringBuilderPool_Get_ReturnsStringBuilder()
        {
            // Act
            StringBuilder sb = StringBuilderPool.Get();

            // Assert
            Assert.NotNull(sb, "StringBuilderPool should return a non-null StringBuilder.");
            Assert.AreEqual(0, sb.Length, "StringBuilder should be cleared before being returned.");
        }

        /// <summary>
        /// Verifies Get(out StringBuilder) properly retrieves and provides a StringBuilder.
        /// </summary>
        [Test]
        public void StringBuilderPool_GetWithOut_ProvidesStringBuilder()
        {
            // Act
            using var pooledObject = StringBuilderPool.Get(out var sb);

            // Assert
            Assert.NotNull(sb, "StringBuilderPool should return a non-null StringBuilder with out parameter.");
            Assert.AreEqual(0, sb.Length, "StringBuilder should be cleared before being returned.");
        }

        /// <summary>
        /// Confirms that a released StringBuilder is cleared before being reused.
        /// </summary>
        [Test]
        public void StringBuilderPool_Release_ClearsStringBuilder()
        {
            // Arrange
            StringBuilder sb = StringBuilderPool.Get();
            sb.Append("Test Data");

            // Act
            StringBuilderPool.Release(sb);
            StringBuilder sbReused = StringBuilderPool.Get();

            // Assert
            Assert.AreEqual(0, sbReused.Length, "Released StringBuilder should be cleared before being reused.");
        }

        /// <summary>
        /// Ensures that the pool reuses StringBuilder instances effectively when available.
        /// </summary>
        [Test]
        public void StringBuilderPool_Reuse_SameInstance()
        {
            // Arrange
            StringBuilder sb1 = StringBuilderPool.Get();
            StringBuilderPool.Release(sb1);

            // Act
            StringBuilder sb2 = StringBuilderPool.Get();

            // Assert
            Assert.AreSame(sb1, sb2, "StringBuilderPool should reuse the same StringBuilder instance when available.");
        }

        /// <summary>
        /// Validates the pool handles multiple simultaneous StringBuilder requests.
        /// </summary>
        [Test]
        public void StringBuilderPool_HandlesMultipleInstances()
        {
            // Act
            StringBuilder sb1 = StringBuilderPool.Get();
            StringBuilder sb2 = StringBuilderPool.Get();

            // Assert
            Assert.AreNotSame(sb1, sb2, "StringBuilderPool should handle multiple instances correctly.");
        }
    }
}
