using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PKGE.Unsafe
{
    //https://github.com/Unity-Technologies/UnityLiveCapture/blob/main/Packages/com.unity.live-capture/Networking/Utilities/Extensions/BufferExtensions.cs
    #region Unity.LiveCapture.Networking
    /// <summary>
    /// A class containing extension methods used to marshal structs to/from byte arrays.
    /// </summary>
    public static class BufferExtensions
    {
        /// <summary>
        /// Writes a blittable struct to the buffer.
        /// </summary>
        /// <remarks>
        /// Does not validate the arguments, it is the caller's responsibility to check for buffer
        /// overrun and underrun as needed.
        /// </remarks>
        /// <param name="buffer">The buffer to write the struct into.</param>
        /// <param name="data">The struct to write.</param>
        /// <param name="offset">The offset into the buffer to start writing at.</param>
        /// <typeparam name="T">A blittable struct type.</typeparam>
        /// <returns>The index in the buffer immediately following the last byte written.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteStruct<T>(this byte[] buffer, ref T data, int offset = 0) where T : struct
        {
            return WriteStruct(buffer.AsSpan(), ref data, offset);
        }

        /// <inheritdoc cref="WriteStruct{T}(byte[], ref T, int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteStruct<T>(this Span<byte> buffer, ref T data, int offset = 0) where T : struct
        {
            var size = SizeOfCache<T>.Size;
            MemoryMarshal.Cast<byte, T>(buffer[offset..])[0] = data;
            return offset + size;
        }

        /// <summary>
        /// Reads a blittable struct from the buffer.
        /// </summary>
        /// <remarks>
        /// Does not validate the arguments, it is the caller's responsibility to check for buffer
        /// overrun and underrun as needed.
        /// </remarks>
        /// <param name="buffer">The buffer to read the struct from.</param>
        /// <param name="offset">The offset into the buffer to start reading from.</param>
        /// <typeparam name="T">A blittable struct type.</typeparam>
        /// <returns>The read struct.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadStruct<T>(this byte[] buffer, int offset = 0) where T : struct
        {
            return ReadStruct<T>(buffer.AsSpan(), offset);
        }

        /// <inheritdoc cref="ReadStruct{T}(byte[], int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadStruct<T>(this Span<byte> buffer, int offset = 0) where T : struct
        {
            return MemoryMarshal.Cast<byte, T>(buffer[offset..])[0];
        }

        /// <summary>
        /// Reads a blittable struct from the buffer.
        /// </summary>
        /// <remarks>
        /// Does not validate the arguments, it is the caller's responsibility to check for buffer
        /// overrun and underrun as needed.
        /// </remarks>
        /// <param name="buffer">The buffer to read the struct from.</param>
        /// <param name="offset">The offset into the buffer to start reading from.</param>
        /// <param name="nextOffset">The index in the buffer immediately following the last byte read.</param>
        /// <typeparam name="T">A blittable struct type.</typeparam>
        /// <returns>The read struct.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadStruct<T>(this byte[] buffer, int offset, out int nextOffset) where T : struct
        {
            return ReadStruct<T>(buffer.AsSpan(), offset, out nextOffset);
        }

        /// <inheritdoc cref="ReadStruct{T}(byte[], int, out int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadStruct<T>(this Span<byte> buffer, int offset, out int nextOffset) where T : struct
        {
            nextOffset = offset + SizeOfCache<T>.Size;
            return MemoryMarshal.Cast<byte, T>(buffer[offset..])[0];
        }
    }
    #endregion // Unity.LiveCapture.Networking
}