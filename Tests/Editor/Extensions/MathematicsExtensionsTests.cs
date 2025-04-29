using NUnit.Framework;
using System.Linq;

namespace UnityExtensions.Tests
{
    public class MathematicsExtensionsTests
    {
        [Test]
        public void Union_SizeOf()
        {
            Assert.AreEqual(1, Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<Union1>());
            Assert.AreEqual(2, Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<Union2>());
            Assert.AreEqual(4, Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<Union4>());
            Assert.AreEqual(8, Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<Union8>());
            Assert.AreEqual(16, Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<Union16>());
        }

        /// <summary>
        /// Accuracy: Validates correct conversion for standard hex representation
        /// </summary>
        [Test]
        public void ConvertToHex_ValidNumber_FillsBufferWithHexRepresentation()
        {
            uint number = 255; // 0xFF
            char[] buffer = new char[8]; // Sufficient buffer size
            number.ConvertToHex(buffer);

            string result = new string(buffer).Trim('\0'); // Convert buffer to string and trim unused space
            Assert.AreEqual("FF", result);
            
            
            ((byte)number).ConvertToHex(buffer);

            result = new string(buffer).Trim('\0'); // Convert buffer to string and trim unused space
            Assert.AreEqual("FF", result);
        }

        /// <summary>
        /// Accuracy: Validates correct conversion for standard hex representation
        /// </summary>
        [Test]
        public void ConvertToHex_NumberZero_FillsBufferWithSingleZero()
        {
            uint number = 0;
            char[] buffer = new char[8]; // Sufficient buffer size
            number.ConvertToHex(buffer);

            string result = new string(buffer).Trim('\0'); // Convert buffer to string and trim unused space
            Assert.AreEqual("0", result);
            
            
            ((byte)number).ConvertToHex(buffer);

            result = new string(buffer).Trim('\0'); // Convert buffer to string and trim unused space
            Assert.AreEqual("0", result);
        }

        /// <summary>
        /// Edge Case: Test largest input (uint.MaxValue)
        /// </summary>
        [Test]
        public void ConvertToHex_MaximumUIntNumber_FillsBufferWithFullHexRepresentation()
        {
            uint number = uint.MaxValue; // 0xFFFFFFFF
            char[] buffer = new char[8]; // Buffer size for 8 characters
            number.ConvertToHex(buffer);

            string result = new string(buffer).Trim('\0'); // Convert buffer to string and trim unused space
            Assert.AreEqual("FFFFFFFF", result);
        }

        /// <summary>
        /// Buffer Behavior: Ensures the function handles small buffer sizes gracefully.
        /// </summary>
        [Test]
        public void ConvertToHex_SmallBuffer_TruncatesResult()
        {
            uint number = 65535; // 0xFFFF
            char[] buffer = new char[2]; // Buffer size smaller than required
            number.ConvertToHex(buffer);

            string result = new string(buffer); // Convert buffer to string without trimming
            Assert.AreEqual("FF", result); // Expected truncation
        }

        /// <summary>
        /// Buffer Behavior: Ensures the function handles small buffer sizes gracefully.
        /// </summary>
        [Test]
        public void ConvertToHex_BufferTooSmall_HandlesGracefullyWithoutErrors()
        {
            uint number = 12345; // 0x3039
            char[] buffer = new char[2]; // Insufficient buffer size
            Assert.DoesNotThrow(() => number.ConvertToHex(buffer));
        }

        /// <summary>
        /// Buffer Behavior: Ensures the function handles large buffer sizes gracefully.
        /// </summary>
        [Test]
        public void ConvertToHex_LargeBuffer_PadsRemainingSpaceWithEmptyCharacters()
        {
            uint number = 255; // 0xFF
            char[] buffer = new char[8]; // Large buffer
            number.ConvertToHex(buffer);

            string result = new string(buffer);
            Assert.IsTrue(result.StartsWith("FF")); // Hex value
            Assert.IsTrue(result.Trim('F').All(c => c == '\0')); // Remaining buffer space is empty
            
            
            ((byte)number).ConvertToHex(buffer);
            
            result = new string(buffer);
            Assert.IsTrue(result.StartsWith("FF")); // Hex value
            Assert.IsTrue(result.Trim('F').All(c => c == '\0')); // Remaining buffer space is empty
        }
    }
}
