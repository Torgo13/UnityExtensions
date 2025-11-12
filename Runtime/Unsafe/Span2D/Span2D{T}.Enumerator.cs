// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PKGE.Unsafe
{
    partial struct Span2D<T>
    {
        //https://github.com/CommunityToolkit/dotnet/blob/657c6971a8d42655c648336b781639ed96c2c49f/src/CommunityToolkit.HighPerformance/Memory/Span2D%7BT%7D.Enumerator.cs
        #region CommunityToolkit.HighPerformance
        /// <summary>
        /// Gets an enumerable that traverses items in a specified row.
        /// </summary>
        /// <param name="row">The target row to enumerate within the current <see cref="Span2D{T}"/> instance.</param>
        /// <returns>A <see cref="RefEnumerable{T}"/> with target items to enumerate.</returns>
        /// <remarks>The returned <see cref="RefEnumerable{T}"/> value shouldn't be used directly: use this extension in a <see langword="foreach"/> loop.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RefEnumerable<T> GetRow(int row)
        {
            if ((uint)row >= Height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForRow();
            }

            nint startIndex = (nint)(uint)this.Stride * (nint)(uint)row;
            ref T r0 = ref DangerousGetReference();
            ref T r1 = ref System.Runtime.CompilerServices.Unsafe.Add(ref r0, startIndex);

            return new(ref r1, Width, 1);
        }

        /// <summary>
        /// Gets an enumerable that traverses items in a specified column.
        /// </summary>
        /// <param name="column">The target column to enumerate within the current <see cref="Span2D{T}"/> instance.</param>
        /// <returns>A <see cref="RefEnumerable{T}"/> with target items to enumerate.</returns>
        /// <remarks>The returned <see cref="RefEnumerable{T}"/> value shouldn't be used directly: use this extension in a <see langword="foreach"/> loop.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RefEnumerable<T> GetColumn(int column)
        {
            if ((uint)column >= Width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForColumn();
            }

            ref T r0 = ref DangerousGetReference();
            ref T r1 = ref System.Runtime.CompilerServices.Unsafe.Add(ref r0, (nint)(uint)column);

            return new(ref r1, Height, this.Stride);
        }

        /// <summary>
        /// Returns an enumerator for the current <see cref="Span2D{T}"/> instance.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to traverse the items in the current <see cref="Span2D{T}"/> instance
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        /// <summary>
        /// Provides an enumerator for the elements of a <see cref="Span2D{T}"/> instance.
        /// </summary>
        public ref struct Enumerator
        {
            /// <summary>
            /// The <see cref="Span{T}"/> instance pointing to the first item in the target memory area.
            /// </summary>
            /// <remarks>Just like in <see cref="Span2D{T}"/>, the length is the height of the 2D region.</remarks>
            private readonly Span<T> span;

            /// <summary>
            /// The width of the specified 2D region.
            /// </summary>
            private readonly int width;

            /// <summary>
            /// The stride of the specified 2D region.
            /// </summary>
            private readonly int stride;

            /// <summary>
            /// The current horizontal offset.
            /// </summary>
            private int x;

            /// <summary>
            /// The current vertical offset.
            /// </summary>
            private int y;

            /// <summary>
            /// Initializes a new instance of the <see cref="Enumerator"/> struct.
            /// </summary>
            /// <param name="span">The target <see cref="Span2D{T}"/> instance to enumerate.</param>
            internal Enumerator(Span2D<T> span)
            {
                this.span = span.span;
                this.width = span.width;
                this.stride = span.Stride;
                this.x = -1;
                this.y = 0;
            }

            /// <summary>
            /// Implements the duck-typed <see cref="System.Collections.IEnumerator.MoveNext"/> method.
            /// </summary>
            /// <returns><see langword="true"/> whether a new element is available, <see langword="false"/> otherwise</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                int x = this.x + 1;

                // Horizontal move, within range
                if (x < this.width)
                {
                    this.x = x;

                    return true;
                }

                // We reached the end of a row and there is at least
                // another row available: wrap to a new line and continue.
                this.x = 0;

                return ++this.y < this.span.Length;
            }

            /// <summary>
            /// Gets the duck-typed <see cref="System.Collections.Generic.IEnumerator{T}.Current"/> property.
            /// </summary>
            public readonly ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    ref T r0 = ref MemoryMarshal.GetReference(this.span);
                    nint index = ((nint)(uint)this.y * (nint)(uint)this.stride) + (nint)(uint)this.x;

                    return ref System.Runtime.CompilerServices.Unsafe.Add(ref r0, index);
                }
            }
        }
        #endregion // CommunityToolkit.HighPerformance
    }
}
