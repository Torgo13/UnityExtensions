using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe
{
    public static class StreamExtensions
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/ecad5ff79b1fa55162c23108029609b16e9ffe6d/InternalPackages/com.unity.video-streaming.client/Runtime/Utils/StreamExtensions.cs
        #region Unity.LiveCapture.VideoStreaming.Client.Utils
        /// <exception cref="EndOfStreamException"></exception>
        public static async Task ReadExactAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return;

            do
            {
                int read = await stream.ReadAsync(buffer, offset, count);

                if (read == 0)
                    throw new EndOfStreamException();

                count -= read;
                offset += read;
            }
            while (count != 0);
        }
        #endregion // Unity.LiveCapture.VideoStreaming.Client.Utils

        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/ecad5ff79b1fa55162c23108029609b16e9ffe6d/Packages/com.unity.live-capture/Networking/Utilities/Extensions/StreamExtensions.cs
        #region Unity.LiveCapture.Networking
        [ThreadStatic]
        static byte[] s_TempBuffer;
        static readonly UTF8Encoding s_Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        /// <summary>
        /// Writes a blittable struct to the stream.
        /// </summary>
        /// <param name="stream">The stream to write the struct into.</param>
        /// <param name="data">The struct to write.</param>
        /// <typeparam name="T">A blittable struct type.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteStruct<T>(this Stream stream, T data) where T : struct
        {
            stream.WriteStruct(ref data);
        }

        /// <summary>
        /// Writes a blittable struct to the stream.
        /// </summary>
        /// <param name="stream">The stream to write the struct into.</param>
        /// <param name="data">The struct to write.</param>
        /// <typeparam name="T">A blittable struct type.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteStruct<T>(this Stream stream, ref T data) where T : struct
        {
            var size = SizeOfCache<T>.Size;

            EnsureBufferCapacity(size);
            _ = s_TempBuffer.WriteStruct(ref data);

            stream.Write(s_TempBuffer, 0, size);
        }

        /// <summary>
        /// Reads a blittable struct from the stream.
        /// </summary>
        /// <param name="stream">The stream to read the struct from.</param>
        /// <typeparam name="T">A blittable struct type.</typeparam>
        /// <returns>The read struct.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadStruct<T>(this Stream stream) where T : struct
        {
            var size = SizeOfCache<T>.Size;

            Read(stream, size);

            return s_TempBuffer.ReadStruct<T>();
        }

        /// <summary>
        /// Writes a length prefixed UTF-8 string to the stream.
        /// </summary>
        /// <param name="stream">The stream to write the string into.</param>
        /// <param name="str">The string to write.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteString(this Stream stream, string str)
        {
            var strLen = s_Encoding.GetByteCount(str);
            var size = sizeof(int) + strLen;

            EnsureBufferCapacity(size);
            var offset = s_TempBuffer.WriteStruct(ref strLen);
            _ = s_Encoding.GetBytes(str, 0, str.Length, s_TempBuffer, offset);

            stream.Write(s_TempBuffer, 0, size);
        }

        /// <summary>
        /// Reads a length prefixed UTF-8 string from the stream.
        /// </summary>
        /// <param name="stream">The stream to read the string from.</param>
        /// <returns>The read struct.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadString(this Stream stream)
        {
            var strLen = stream.ReadStruct<int>();

            Read(stream, strLen);

            return s_Encoding.GetString(s_TempBuffer, 0, strLen);
        }

        internal static void Read(this Stream stream, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Must be non negative.");

            EnsureBufferCapacity(count);

            if (count == 0)
                return;

            var offset = 0;

            do
            {
                var readBytes = stream.Read(s_TempBuffer, offset, count);

                if (readBytes <= 0)
                    throw new EndOfStreamException();

                offset += readBytes;
            }
            while (offset < count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EnsureBufferCapacity(int capacity)
        {
            if (s_TempBuffer == null || s_TempBuffer.Length < capacity)
                s_TempBuffer = new byte[capacity];
        }
        
        /// <summary>
        /// Copies a native array into a stream.
        /// </summary>
        /// <param name="stream">The stream to write the array into.</param>
        /// <param name="array">The array to write.</param>
        /// <typeparam name="T">The type of data in the native array.</typeparam>
        /// <returns><see langword="true"/> if the array was successfully written into the stream; otherwise, <see langword="false"/>.</returns>
        public static bool WriteArray<T>(this MemoryStream stream, NativeArray<T> array) where T : struct
        {
            stream.SetLength(stream.Length + array.Length);

            if (!stream.TryGetBuffer(out var buffer) || buffer.Array == null)
            {
                return false;
            }

            unsafe
            {
                fixed (void* streamPtr = &buffer.Array[buffer.Offset + stream.Position])
                {
                    UnsafeUtility.MemCpy(streamPtr, array.GetUnsafePtr(), array.Length);
                }
            }

            stream.Position += array.Length;
            return true;
        }
        #endregion // Unity.LiveCapture.Networking

        public static Span<byte> AsSpan(this MemoryStream stream)
        {
            if (!stream.TryGetBuffer(out _))
                return new Span<byte>();

            return new Span<byte>(stream.GetBuffer(), 0, (int)stream.Length);
        }
    }
}