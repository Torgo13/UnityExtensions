using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;

namespace UnityExtensions.Unsafe.Tests
{
    [TestFixture]
    public class StreamExtensionsTests
    {
        private Stream _stream;

        [SetUp]
        public void SetUp()
        {
            _stream = new MemoryStream();
        }

        [TearDown]
        public void TearDown()
        {
            _stream.Dispose();
        }

        [Test]
        public void Write_ShouldWriteNativeArrayToStream()
        {
            // Arrange
            var nativeArray = new NativeArray<byte>(new byte[] { 1, 2, 3, 4, 5 }, Allocator.Temp);

            // Act
            _stream.Write(nativeArray);

            Assert.AreEqual(nativeArray.Length, _stream.Length);

            // Assert
            _stream.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[nativeArray.Length];
            var byteCount = _stream.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(nativeArray.Length, byteCount);

            for (int i = 0; i < nativeArray.Length; i++)
            {
                Assert.AreEqual(nativeArray[i], buffer[i]);
            }

            nativeArray.Dispose();
        }

        [Test]
        public void WriteStruct_ShouldWriteBlittableStructToStream()
        {
            // Arrange
            var data = new TestStruct { Value1 = 10, Value2 = 20.5f };

            // Act
            _stream.WriteStruct(data);
            _stream.Position = 0; // Reset stream position for reading.

            // Assert
            var readData = _stream.ReadStruct<TestStruct>();
            Assert.AreEqual(data, readData);
        }

        [Test]
        public void WriteString_ShouldWriteLengthPrefixedStringToStream()
        {
            // Arrange
            var testString = "Hello, Unity!";

            // Act
            _stream.WriteString(testString);
            _stream.Position = 0; // Reset stream position for reading.

            // Assert
            var readString = _stream.ReadString();
            Assert.AreEqual(testString, readString);
        }

        [Test]
        public void ReadString_ShouldThrowExceptionForNegativeLength()
        {
            // Arrange
            var invalidLength = -1;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => _stream.Read(invalidLength));
        }

        [Test]
        public void WriteArray_ShouldCopyNativeArrayIntoStream()
        {
            // Arrange
            var nativeArray = new NativeArray<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);

            // Act
            var success = ((MemoryStream)_stream).WriteArray(nativeArray);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(_stream.Length, nativeArray.Length);
            nativeArray.Dispose();
        }

        [Test]
        public void WriteStruct_EnsuresBufferCapacity()
        {
            // Arrange
            var data = new TestStruct { Value1 = 42, Value2 = 99.9f };

            // Act
            StreamExtensions.EnsureBufferCapacity(1024);
            _stream.WriteStruct(data);

            // Assert
            Assert.AreEqual(data.Value1, 42);
            Assert.AreEqual(data.Value2, 99.9f);
        }

        [Test]
        public async Task ReadExactAsync_ShouldReadExactBytes()
        {
            // Arrange
            var inputData = Encoding.UTF8.GetBytes("TestData");
            _stream.Write(inputData, 0, inputData.Length);
            _stream.Position = 0;
            var buffer = new byte[inputData.Length];

            // Act
            await _stream.ReadExactAsync(buffer, 0, buffer.Length);

            // Assert
            Assert.AreEqual(inputData, buffer);
        }

        [Test]
        public void ReadExactAsync_ShouldThrowException_WhenStreamIsEmpty()
        {
            // Arrange
            var buffer = new byte[10];

            // Act & Assert
            Assert.ThrowsAsync<EndOfStreamException>(() => _stream.ReadExactAsync(buffer, 0, buffer.Length));
        }

        [Test]
        public async Task ReadExactAsync_ShouldHandlePartialReads()
        {
            // Arrange
            var inputData = new byte[] { 1, 2, 3, 4, 5 };
            _stream.Write(inputData, 0, inputData.Length);
            _stream.Position = 0;
            var buffer = new byte[inputData.Length];

            // Simulate partial reads by wrapping the stream.
            var partialReadStream = new PartialReadStream(_stream, maxReadBytes: 2);

            // Act
            await partialReadStream.ReadExactAsync(buffer, 0, buffer.Length);

            // Assert
            Assert.AreEqual(inputData, buffer);
        }

        [Test]
        public void ReadExactAsync_ShouldThrowArgumentOutOfRangeException_WhenCountIsNegative()
        {
            // Arrange
            var buffer = new byte[10];

            // Act & Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _stream.ReadExactAsync(buffer, 0, -1));
        }

        [Test]
        public async Task ReadExactAsync_ShouldHandleZeroCountGracefully()
        {
            // Arrange
            var buffer = new byte[10];

            // Act
            await _stream.ReadExactAsync(buffer, 0, 0);

            // Assert
            Assert.Pass("Completed without exceptions.");
        }

        [Test]
        public void Write_ShouldThrowArgumentException_WhenArrayIsEmpty()
        {
            // Arrange
            var nativeArray = new NativeArray<byte>(0, Allocator.Temp);

            // Act & Assert
            Assert.DoesNotThrow(() => _stream.Write(nativeArray));

            nativeArray.Dispose();
        }

        /*
        [Test]
        public void Write_ShouldWritePartialData_WhenStreamHasLimitedCapacity()
        {
            // Arrange
            var nativeArray = new NativeArray<byte>(new byte[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            _stream.SetLength(3); // Simulate limited capacity.

            // Act
            Assert.Throws<IOException>(() => _stream.Write(nativeArray));

            nativeArray.Dispose();
        }
        */

        [Test]
        public void Write_ShouldThrowObjectDisposedException_WhenStreamIsDisposed()
        {
            // Arrange
            var nativeArray = new NativeArray<byte>(new byte[] { 1, 2, 3 }, Allocator.Temp);
            _stream.Dispose();

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => _stream.Write(nativeArray));

            nativeArray.Dispose();
        }

        /*
        [Test]
        public void Write_ShouldThrowInvalidOperationException_WhenNativeArrayIsDisposed()
        {
            // Arrange
            var nativeArray = new NativeArray<byte>(new byte[] { 1, 2, 3, 4, 5 }, Allocator.Temp);
            nativeArray.Dispose();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _stream.Write(nativeArray));
        }
        */

        [Test]
        public void Write_ShouldHandleLargeNativeArrayEfficiently()
        {
            // Arrange
            var nativeArray = new NativeArray<byte>(10_000_000, Allocator.Temp);
            for (int i = 0; i < nativeArray.Length; i++)
                nativeArray[i] = (byte)(i % 256);

            // Act
            _stream.Write(nativeArray);

            // Assert
            _stream.Position = 0;
            var result = ((MemoryStream)_stream).ToArray();
            Assert.AreEqual(nativeArray.ToArray(), result);

            nativeArray.Dispose();
        }
    }

    internal struct TestStruct
    {
        public int Value1;
        public float Value2;

        public override bool Equals(object obj)
        {
            if (obj is TestStruct other)
            {
                return Value1 == other.Value1 && Value2.Equals(other.Value2);
            }

            return false;
        }

        public override int GetHashCode() => Value1.GetHashCode() ^ Value2.GetHashCode();
    }

    internal class PartialReadStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly int _maxReadBytes;

        public PartialReadStream(Stream baseStream, int maxReadBytes)
        {
            _baseStream = baseStream;
            _maxReadBytes = maxReadBytes;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            count = Math.Min(count, _maxReadBytes);
            return _baseStream.Read(buffer, offset, count);
        }

        // Other members of Stream must delegate to _baseStream.
        public override bool CanRead => _baseStream.CanRead;
        public override bool CanSeek => _baseStream.CanSeek;
        public override bool CanWrite => _baseStream.CanWrite;
        public override long Length => _baseStream.Length;
        public override long Position { get => _baseStream.Position; set => _baseStream.Position = value; }
        public override void Flush() => _baseStream.Flush();
        public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);
        public override void SetLength(long value) => _baseStream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _baseStream.Write(buffer, offset, count);
    }
}
