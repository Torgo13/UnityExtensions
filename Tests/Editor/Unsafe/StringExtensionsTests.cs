using NUnit.Framework;

namespace UnityExtensions.Unsafe.Tests
{
    class StringExtensionsTests
    {
        [Test]
        public void StringExtensions_TryFromBase64()
        {
            // "Hello World!"
            string input = "SGVsbG8gV29ybGQh";

            byte[] byteArray = System.Convert.FromBase64String(input);
            int length = input.FromBase64_ComputeResultLength();

            Assert.AreEqual(length, byteArray.Length);

            System.Span<byte> bytes = stackalloc byte[length];
            Assert.IsTrue(System.Convert.TryFromBase64String(input, bytes, out int bytesWritten));

            Assert.AreEqual(bytesWritten, byteArray.Length);

            for (int i = 0; i < byteArray.Length; i++)
            {
                Assert.AreEqual(byteArray[i], bytes[i]);
            }
        }

        [Test]
        public void StringExtensions_TryFromBase64_LargeSpan()
        {
            // "Hello World!" * 50
            string input = "SGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQ==";

            byte[] byteArray = System.Convert.FromBase64String(input);
            int length = input.FromBase64_ComputeResultLength();

            Assert.AreEqual(length, byteArray.Length);

            System.Span<byte> bytes = stackalloc byte[length];
            Assert.IsTrue(System.Convert.TryFromBase64String(input, bytes, out int bytesWritten));

            Assert.AreEqual(bytesWritten, byteArray.Length);

            for (int i = 0; i < byteArray.Length; i++)
            {
                Assert.AreEqual(byteArray[i], bytes[i]);
            }
        }

        [Test]
        public void StringExtensions_TryFromBase64_ArrayPool()
        {
            // "Hello World!" * 50
            string input = "SGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQpIZWxsbyBXb3JsZCEKSGVsbG8gV29ybGQhCkhlbGxvIFdvcmxkIQ==";

            byte[] byteArray = System.Convert.FromBase64String(input);
            int length = input.FromBase64_ComputeResultLength();

            Assert.AreEqual(length, byteArray.Length);

            byte[] bytes = System.Buffers.ArrayPool<byte>.Shared.Rent(length);
            Assert.IsTrue(System.Convert.TryFromBase64String(input, bytes, out int bytesWritten));

            Assert.AreEqual(bytesWritten, byteArray.Length);

            for (int i = 0; i < byteArray.Length; i++)
            {
                Assert.AreEqual(byteArray[i], bytes[i]);
            }

            System.Buffers.ArrayPool<byte>.Shared.Return(bytes);
        }

        [Test]
        public unsafe void FromBase64_ComputeResultLength_ShouldReturnCorrectLength_Unsafe()
        {
            string base64String = "SGVsbG8gV29ybGQ="; // "Hello World" in Base64
            fixed (char* p = base64String)
            {
                int resultLength = StringExtensions.FromBase64_ComputeResultLength(p, base64String.Length);
                Assert.AreEqual(11, resultLength); // "Hello World" has 11 bytes
            }
        }

        [Test]
        public unsafe void FromBase64_ComputeResultLength_ShouldHandleWhiteSpace_Unsafe()
        {
            string base64String = " SG VsbG8g V2 9ybGQ= "; // "Hello World" in Base64 with spaces
            fixed (char* p = base64String)
            {
                int resultLength = StringExtensions.FromBase64_ComputeResultLength(p, base64String.Length);
                Assert.AreEqual(11, resultLength); // "Hello World" has 11 bytes
            }
        }
        
        [Test]
        public unsafe void FromBase64_ComputeResultLength_ShouldHandleEmptyString_Unsafe()
        {
            string base64String = "";
            fixed (char* p = base64String)
            {
                int resultLength = StringExtensions.FromBase64_ComputeResultLength(p, base64String.Length);
                Assert.AreEqual(0, resultLength);
            }
        }

        [Test]
        public void FromBase64_ComputeResultLength_ShouldReturnCorrectLength()
        {
            string base64String = "SGVsbG8gV29ybGQ="; // "Hello World" in Base64
            int resultLength = base64String.FromBase64_ComputeResultLength();
            Assert.AreEqual(11, resultLength); // "Hello World" has 11 bytes
        }

        [Test]
        public void FromBase64_ComputeResultLength_ShouldHandleWhiteSpace()
        {
            string base64String = " SG VsbG8g V2 9ybGQ= "; // "Hello World" in Base64 with spaces
            int resultLength = base64String.FromBase64_ComputeResultLength();
            Assert.AreEqual(11, resultLength); // "Hello World" has 11 bytes
        }
        
        [Test]
        public void FromBase64_ComputeResultLength_ShouldHandleEmptyString()
        {
            string base64String = "";
            int resultLength = base64String.FromBase64_ComputeResultLength();
            Assert.AreEqual(0, resultLength);
        }
    }
}
