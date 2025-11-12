// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PKGE.Unsafe
{
    /// <summary>
    /// Helpers for working with the <see cref="Span{T}"/> type.
    /// </summary>
    public static class SpanExtensions
    {
        //https://github.com/CommunityToolkit/dotnet/blob/657c6971a8d42655c648336b781639ed96c2c49f/src/CommunityToolkit.HighPerformance/Extensions/SpanExtensions.cs
        #region CommunityToolkit.HighPerformance
        /// <summary>
        /// Returns a reference to the first element within a given <see cref="Span{T}"/>, with no bounds checks.
        /// </summary>
        /// <typeparam name="T">The type of elements in the input <see cref="Span{T}"/> instance.</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance.</param>
        /// <returns>A reference to the first element within <paramref name="span"/>.</returns>
        /// <remarks>This method doesn't do any bounds checks, therefore it is responsibility of the caller to perform checks in case the returned value is dereferenced.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T DangerousGetReference<T>(this Span<T> span)
        {
            return ref MemoryMarshal.GetReference(span);
        }

        /// <summary>
        /// Returns a reference to an element at a specified index within a given <see cref="Span{T}"/>, with no bounds checks.
        /// </summary>
        /// <typeparam name="T">The type of elements in the input <see cref="Span{T}"/> instance.</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance.</param>
        /// <param name="i">The index of the element to retrieve within <paramref name="span"/>.</param>
        /// <returns>A reference to the element within <paramref name="span"/> at the index specified by <paramref name="i"/>.</returns>
        /// <remarks>This method doesn't do any bounds checks, therefore it is responsibility of the caller to ensure the <paramref name="i"/> parameter is valid.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T DangerousGetReferenceAt<T>(this Span<T> span, int i)
        {
            ref T r0 = ref MemoryMarshal.GetReference(span);
            ref T ri = ref System.Runtime.CompilerServices.Unsafe.Add(ref r0, (nint)(uint)i);

            return ref ri;
        }

        /// <summary>
        /// Returns a reference to an element at a specified index within a given <see cref="Span{T}"/>, with no bounds checks.
        /// </summary>
        /// <typeparam name="T">The type of elements in the input <see cref="Span{T}"/> instance.</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance.</param>
        /// <param name="i">The index of the element to retrieve within <paramref name="span"/>.</param>
        /// <returns>A reference to the element within <paramref name="span"/> at the index specified by <paramref name="i"/>.</returns>
        /// <remarks>This method doesn't do any bounds checks, therefore it is responsibility of the caller to ensure the <paramref name="i"/> parameter is valid.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T DangerousGetReferenceAt<T>(this Span<T> span, nint i)
        {
            ref T r0 = ref MemoryMarshal.GetReference(span);
            ref T ri = ref System.Runtime.CompilerServices.Unsafe.Add(ref r0, i);

            return ref ri;
        }

        /// <summary>
        /// Returns a <see cref="Span2D{T}"/> instance wrapping the underlying data for the given <see cref="Span{T}"/> instance.
        /// </summary>
        /// <typeparam name="T">The type of items in the input <see cref="Span{T}"/> instance.</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance.</param>
        /// <param name="height">The height of the resulting 2D area.</param>
        /// <param name="width">The width of each row in the resulting 2D area.</param>
        /// <returns>The resulting <see cref="Span2D{T}"/> instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when one of the input parameters is out of range.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the requested area is outside of bounds for <paramref name="span"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span2D<T> AsSpan2D<T>(this Span<T> span, int height, int width)
            where T : struct
        {
            return new Span2D<T>(span, height, width);
        }

        /// <summary>
        /// Returns a <see cref="Span2D{T}"/> instance wrapping the underlying data for the given <see cref="Span{T}"/> instance.
        /// </summary>
        /// <typeparam name="T">The type of items in the input <see cref="Span{T}"/> instance.</typeparam>
        /// <param name="span">The input <see cref="Span{T}"/> instance.</param>
        /// <param name="offset">The initial offset within <paramref name="span"/>.</param>
        /// <param name="height">The height of the resulting 2D area.</param>
        /// <param name="width">The width of each row in the resulting 2D area.</param>
        /// <param name="pitch">The pitch in the resulting 2D area.</param>
        /// <returns>The resulting <see cref="Span2D{T}"/> instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when one of the input parameters is out of range.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the requested area is outside of bounds for <paramref name="span"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span2D<T> AsSpan2D<T>(this Span<T> span, int offset, int height, int width, int pitch)
            where T : struct
        {
            return new Span2D<T>(span, offset, height, width, pitch);
        }

        /// <summary>
        /// Casts a <see cref="Span{T}"/> of one primitive type <typeparamref name="T"/> to <see cref="Span{T}"/> of bytes.
        /// </summary>
        /// <typeparam name="T">The type if items in the source <see cref="Span{T}"/>.</typeparam>
        /// <param name="span">The source slice, of type <typeparamref name="T"/>.</param>
        /// <returns>A <see cref="Span{T}"/> of bytes.</returns>
        /// <exception cref="OverflowException">
        /// Thrown if the <see cref="Span{T}.Length"/> property of the new <see cref="Span{T}"/> would exceed <see cref="int.MaxValue"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> AsBytes<T>(this Span<T> span)
            where T : unmanaged
        {
            return MemoryMarshal.AsBytes(span);
        }

        /// <summary>
        /// Casts a <see cref="Span{T}"/> of one primitive type <typeparamref name="TFrom"/> to another primitive type <typeparamref name="TTo"/>.
        /// </summary>
        /// <typeparam name="TFrom">The type of items in the source <see cref="Span{T}"/>.</typeparam>
        /// <typeparam name="TTo">The type of items in the destination <see cref="Span{T}"/>.</typeparam>
        /// <param name="span">The source slice, of type <typeparamref name="TFrom"/>.</param>
        /// <returns>A <see cref="Span{T}"/> of type <typeparamref name="TTo"/></returns>
        /// <remarks>
        /// Supported only for platforms that support misaligned memory access or when the memory block is aligned by other means.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<TTo> Cast<TFrom, TTo>(this Span<TFrom> span)
            where TFrom : unmanaged
            where TTo : unmanaged
        {
            return MemoryMarshal.Cast<TFrom, TTo>(span);
        }
        #endregion // CommunityToolkit.HighPerformance
    }

    /// <summary>
    /// Helpers for working with the <see cref="ReadOnlySpan{T}"/> type.
    /// </summary>
    public static class ReadOnlySpanExtensions
    {
        //https://github.com/CommunityToolkit/dotnet/blob/657c6971a8d42655c648336b781639ed96c2c49f/src/CommunityToolkit.HighPerformance/Extensions/ReadOnlySpanExtensions.cs
        #region CommunityToolkit.HighPerformance
        /// <summary>
        /// Returns a reference to the first element within a given <see cref="ReadOnlySpan{T}"/>, with no bounds checks.
        /// </summary>
        /// <typeparam name="T">The type of elements in the input <see cref="ReadOnlySpan{T}"/> instance.</typeparam>
        /// <param name="span">The input <see cref="ReadOnlySpan{T}"/> instance.</param>
        /// <returns>A reference to the first element within <paramref name="span"/>.</returns>
        /// <remarks>This method doesn't do any bounds checks, therefore it is responsibility of the caller to perform checks in case the returned value is dereferenced.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T DangerousGetReference<T>(this ReadOnlySpan<T> span)
        {
            return ref MemoryMarshal.GetReference(span);
        }
        #endregion // CommunityToolkit.HighPerformance
    }
}
