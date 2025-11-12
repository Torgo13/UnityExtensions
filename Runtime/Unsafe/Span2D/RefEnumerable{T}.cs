// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PKGE.Unsafe
{
    /// <summary>
    /// A <see langword="ref"/> <see langword="struct"/> that iterates items from arbitrary memory locations.
    /// </summary>
    /// <typeparam name="T">The type of items to enumerate.</typeparam>
    public readonly ref struct RefEnumerable<T>
    {
        //https://github.com/CommunityToolkit/dotnet/blob/657c6971a8d42655c648336b781639ed96c2c49f/src/CommunityToolkit.HighPerformance/Enumerables/RefEnumerable%7BT%7D.cs
        #region CommunityToolkit.HighPerformance.Enumerables
        /// <summary>
        /// The <see cref="Span{T}"/> instance pointing to the first item in the target memory area.
        /// </summary>
        /// <remarks>The <see cref="Span{T}.Length"/> field maps to the total available length.</remarks>
        internal readonly Span<T> Span;

        /// <summary>
        /// The distance between items in the sequence to enumerate.
        /// </summary>
        /// <remarks>The distance refers to <typeparamref name="T"/> items, not byte offset.</remarks>
        internal readonly int Step;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefEnumerable{T}"/> struct.
        /// </summary>
        /// <param name="reference">A reference to the first item of the sequence.</param>
        /// <param name="length">The number of items in the sequence.</param>
        /// <param name="step">The distance between items in the sequence to enumerate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RefEnumerable(ref T reference, int length, int step)
        {
            this.Span = MemoryMarshal.CreateSpan(ref reference, length);
            this.Step = step;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RefEnumerable{T}"/> struct with the specified parameters.
        /// </summary>
        /// <param name="value">The reference to the first <typeparamref name="T"/> item to map.</param>
        /// <param name="length">The number of items in the sequence.</param>
        /// <param name="step">The distance between items in the sequence to enumerate.</param>
        /// <returns>A <see cref="RefEnumerable{T}"/> instance with the specified parameters.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when one of the parameters are negative.</exception>
        public static RefEnumerable<T> DangerousCreate(ref T value, int length, int step)
        {
            if (length < 0)
            {
                ThrowArgumentOutOfRangeExceptionForLength();
            }

            if (step < 0)
            {
                ThrowArgumentOutOfRangeExceptionForStep();
            }

            OverflowHelper.EnsureIsInNativeIntRange(length, 1, step);

            return new(ref value, length, step);
        }

        /// <summary>
        /// Gets the total available length for the sequence.
        /// </summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.Span.Length;
        }

        /// <summary>
        /// Gets the element at the specified zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index of the element.</param>
        /// <returns>A reference to the element at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when <paramref name="index"/> is invalid.
        /// </exception>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)Length)
                {
                    ThrowHelper.ThrowIndexOutOfRangeException();
                }

                ref T r0 = ref MemoryMarshal.GetReference(this.Span);
                nint offset = (nint)(uint)index * (nint)(uint)this.Step;
                ref T ri = ref System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset);

                return ref ri;
            }
        }

        /// <summary>
        /// Gets the element at the specified zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index of the element.</param>
        /// <returns>A reference to the element at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when <paramref name="index"/> is invalid.
        /// </exception>
        public ref T this[Index index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref this[index.GetOffset(Length)];
        }

        /// <inheritdoc cref="System.Collections.IEnumerable.GetEnumerator"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            return new(this.Span, this.Step);
        }

        /// <summary>
        /// Clears the contents of the current <see cref="RefEnumerable{T}"/> instance.
        /// </summary>
        public void Clear()
        {
            if (this.Step == 1)
            {
                this.Span.Clear();

                return;
            }

            ref T r0 = ref this.Span.DangerousGetReference();
            int length = this.Span.Length;

            RefEnumerableHelper.Clear(ref r0, (nint)(uint)length, (nint)(uint)this.Step);
        }

        /// <summary>
        /// Copies the contents of this <see cref="RefEnumerable{T}"/> into a destination <see cref="RefEnumerable{T}"/> instance.
        /// </summary>
        /// <param name="destination">The destination <see cref="RefEnumerable{T}"/> instance.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="destination"/> is shorter than the source <see cref="RefEnumerable{T}"/> instance.
        /// </exception>
        public void CopyTo(RefEnumerable<T> destination)
        {
            if (this.Step == 1)
            {
                destination.CopyFrom(this.Span);

                return;
            }

            if (destination.Step == 1)
            {
                CopyTo(destination.Span);

                return;
            }

            ref T sourceRef = ref this.Span.DangerousGetReference();
            ref T destinationRef = ref destination.Span.DangerousGetReference();
            int sourceLength = this.Span.Length;
            int destinationLength = destination.Span.Length;

            if ((uint)destinationLength < (uint)sourceLength)
            {
                ThrowArgumentExceptionForDestinationTooShort();
            }

            RefEnumerableHelper.CopyTo(ref sourceRef, ref destinationRef, (nint)(uint)sourceLength, (nint)(uint)this.Step, (nint)(uint)destination.Step);
        }

        /// <summary>
        /// Attempts to copy the current <see cref="RefEnumerable{T}"/> instance to a destination <see cref="RefEnumerable{T}"/>.
        /// </summary>
        /// <param name="destination">The target <see cref="RefEnumerable{T}"/> of the copy operation.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool TryCopyTo(RefEnumerable<T> destination)
        {
            int sourceLength = this.Span.Length;
            int destinationLength = destination.Span.Length;

            if (destinationLength >= sourceLength)
            {
                CopyTo(destination);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Copies the contents of this <see cref="RefEnumerable{T}"/> into a destination <see cref="Span{T}"/> instance.
        /// </summary>
        /// <param name="destination">The destination <see cref="Span{T}"/> instance.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="destination"/> is shorter than the source <see cref="RefEnumerable{T}"/> instance.
        /// </exception>
        public void CopyTo(Span<T> destination)
        {
            if (this.Step == 1)
            {
                this.Span.CopyTo(destination);

                return;
            }

            ref T sourceRef = ref this.Span.DangerousGetReference();
            int length = this.Span.Length;
            if ((uint)destination.Length < (uint)length)
            {
                ThrowArgumentExceptionForDestinationTooShort();
            }

            ref T destinationRef = ref destination.DangerousGetReference();

            RefEnumerableHelper.CopyTo(ref sourceRef, ref destinationRef, (nint)(uint)length, (nint)(uint)this.Step);
        }

        /// <summary>
        /// Attempts to copy the current <see cref="RefEnumerable{T}"/> instance to a destination <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="destination">The target <see cref="Span{T}"/> of the copy operation.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool TryCopyTo(Span<T> destination)
        {
            int length = this.Span.Length;

            if (destination.Length >= length)
            {
                CopyTo(destination);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Copies the contents of a source <see cref="ReadOnlySpan{T}"/> into the current <see cref="RefEnumerable{T}"/> instance.
        /// </summary>
        /// <param name="source">The source <see cref="ReadOnlySpan{T}"/> instance.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the current <see cref="RefEnumerable{T}"/> is shorter than the source <see cref="ReadOnlySpan{T}"/> instance.
        /// </exception>
        internal void CopyFrom(ReadOnlySpan<T> source)
        {
            if (this.Step == 1)
            {
                source.CopyTo(this.Span);

                return;
            }

            ref T destinationRef = ref this.Span.DangerousGetReference();
            int destinationLength = this.Span.Length;
            ref T sourceRef = ref source.DangerousGetReference();
            int sourceLength = source.Length;

            if ((uint)destinationLength < (uint)sourceLength)
            {
                ThrowArgumentExceptionForDestinationTooShort();
            }

            RefEnumerableHelper.CopyFrom(ref sourceRef, ref destinationRef, (nint)(uint)sourceLength, (nint)(uint)this.Step);
        }

        /// <summary>
        /// Attempts to copy the source <see cref="ReadOnlySpan{T}"/> into the current <see cref="RefEnumerable{T}"/> instance.
        /// </summary>
        /// <param name="source">The source <see cref="ReadOnlySpan{T}"/> instance.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool TryCopyFrom(ReadOnlySpan<T> source)
        {
            int length = this.Span.Length;

            if (length >= source.Length)
            {
                CopyFrom(source);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Fills the elements of this <see cref="RefEnumerable{T}"/> with a specified value.
        /// </summary>
        /// <param name="value">The value to assign to each element of the <see cref="RefEnumerable{T}"/> instance.</param>
        public void Fill(T value)
        {
            if (this.Step == 1)
            {
                this.Span.Fill(value);

                return;
            }

            ref T r0 = ref this.Span.DangerousGetReference();
            int length = this.Span.Length;

            RefEnumerableHelper.Fill(ref r0, (nint)(uint)length, (nint)(uint)this.Step, value);
        }

        /// <summary>
        /// Returns a <typeparamref name="T"/> array with the values in the target row.
        /// </summary>
        /// <returns>A <typeparamref name="T"/> array with the values in the target row.</returns>
        /// <remarks>
        /// This method will allocate a new <typeparamref name="T"/> array, so only
        /// use it if you really need to copy the target items in a new memory location.
        /// </remarks>
        public T[] ToArray()
        {
            int length = this.Span.Length;

            // Empty array if no data is mapped
            if (length == 0)
            {
                return Array.Empty<T>();
            }

            T[] array = new T[length];

            CopyTo(array);

            return array;
        }

        /// <summary>
        /// A custom enumerator type to traverse items within a <see cref="RefEnumerable{T}"/> instance.
        /// </summary>
        public ref struct Enumerator
        {
            /// <inheritdoc cref="RefEnumerable{T}.Span"/>
            private readonly Span<T> span;

            /// <inheritdoc cref="RefEnumerable{T}.Step"/>
            private readonly int step;

            /// <summary>
            /// The current position in the sequence.
            /// </summary>
            private int position;

            /// <summary>
            /// Initializes a new instance of the <see cref="Enumerator"/> struct.
            /// </summary>
            /// <param name="span">The <see cref="Span{T}"/> instance with the info on the items to traverse.</param>
            /// <param name="step">The distance between items in the sequence to enumerate.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(Span<T> span, int step)
            {
                this.span = span;
                this.step = step;
                this.position = -1;
            }

            /// <inheritdoc cref="System.Collections.IEnumerator.MoveNext"/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                return ++this.position < this.span.Length;
            }

            /// <inheritdoc cref="System.Collections.Generic.IEnumerator{T}.Current"/>
            public readonly ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    ref T r0 = ref this.span.DangerousGetReference();

                    // Here we just offset by shifting down as if we were traversing a 2D array with a
                    // a single column, with the width of each row represented by the step, the height
                    // represented by the current position, and with only the first element of each row
                    // being inspected. We can perform all the indexing operations in this type as nint,
                    // as the maximum offset is guaranteed never to exceed the maximum value, since on
                    // 32 bit architectures it's not possible to allocate that much memory anyway.
                    nint offset = (nint)(uint)this.position * (nint)(uint)this.step;
                    ref T ri = ref System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset);

                    return ref ri;
                }
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> when the "length" parameter is invalid.
        /// </summary>
        private static void ThrowArgumentOutOfRangeExceptionForLength()
        {
            throw new ArgumentOutOfRangeException("length");
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> when the "step" parameter is invalid.
        /// </summary>
        private static void ThrowArgumentOutOfRangeExceptionForStep()
        {
            throw new ArgumentOutOfRangeException("step");
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> when the target span is too short.
        /// </summary>
        private static void ThrowArgumentExceptionForDestinationTooShort()
        {
            throw new ArgumentException("The target span is too short to copy all the current items to.");
        }
        #endregion // CommunityToolkit.HighPerformance.Enumerables
    }

    /// <summary>
    /// Helpers to process sequences of values by reference with a given step.
    /// </summary>
    internal static class RefEnumerableHelper
    {
        //https://github.com/CommunityToolkit/dotnet/blob/657c6971a8d42655c648336b781639ed96c2c49f/src/CommunityToolkit.HighPerformance/Enumerables/RefEnumerable%7BT%7D.cs
        #region CommunityToolkit.HighPerformance.Enumerables
        /// <summary>
        /// Clears a target memory area.
        /// </summary>
        /// <typeparam name="T">The type of values to clear.</typeparam>
        /// <param name="r0">A <typeparamref name="T"/> reference to the start of the memory area.</param>
        /// <param name="length">The number of items in the memory area.</param>
        /// <param name="step">The number of items between each consecutive target value.</param>
        public static void Clear<T>(ref T r0, nint length, nint step)
        {
            nint offset = 0;

            // Main loop with 8 unrolled iterations
            while (length >= 8)
            {
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset) = default!;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = default!;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = default!;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = default!;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = default!;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = default!;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = default!;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = default!;

                length -= 8;
                offset += step;
            }

            if (length >= 4)
            {
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset) = default!;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = default!;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = default!;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = default!;

                length -= 4;
                offset += step;
            }

            // Clear the remaining values
            while (length > 0)
            {
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset) = default!;

                length -= 1;
                offset += step;
            }
        }

        /// <summary>
        /// Copies a sequence of discontiguous items from one memory area to another.
        /// </summary>
        /// <typeparam name="T">The type of items to copy.</typeparam>
        /// <param name="sourceRef">The source reference to copy from.</param>
        /// <param name="destinationRef">The target reference to copy to.</param>
        /// <param name="length">The total number of items to copy.</param>
        /// <param name="sourceStep">The step between consecutive items in the memory area pointed to by <paramref name="sourceRef"/>.</param>
        public static void CopyTo<T>(ref T sourceRef, ref T destinationRef, nint length, nint sourceStep)
        {
            nint sourceOffset = 0;
            nint destinationOffset = 0;

            while (length >= 8)
            {
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset + 0) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset + 1) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset + 2) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset + 3) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset + 4) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset + 5) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset + 6) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset + 7) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);

                length -= 8;
                sourceOffset += sourceStep;
                destinationOffset += 8;
            }

            if (length >= 4)
            {
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset + 0) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset + 1) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset + 2) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset + 3) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);

                length -= 4;
                sourceOffset += sourceStep;
                destinationOffset += 4;
            }

            while (length > 0)
            {
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset);

                length -= 1;
                sourceOffset += sourceStep;
                destinationOffset += 1;
            }
        }

        /// <summary>
        /// Copies a sequence of discontiguous items from one memory area to another.
        /// </summary>
        /// <typeparam name="T">The type of items to copy.</typeparam>
        /// <param name="sourceRef">The source reference to copy from.</param>
        /// <param name="destinationRef">The target reference to copy to.</param>
        /// <param name="length">The total number of items to copy.</param>
        /// <param name="sourceStep">The step between consecutive items in the memory area pointed to by <paramref name="sourceRef"/>.</param>
        /// <param name="destinationStep">The step between consecutive items in the memory area pointed to by <paramref name="destinationRef"/>.</param>
        public static void CopyTo<T>(ref T sourceRef, ref T destinationRef, nint length, nint sourceStep, nint destinationStep)
        {
            nint sourceOffset = 0;
            nint destinationOffset = 0;

            while (length >= 8)
            {
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += destinationStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += destinationStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += destinationStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += destinationStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += destinationStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += destinationStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += destinationStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);

                length -= 8;
                sourceOffset += sourceStep;
                destinationOffset += destinationStep;
            }

            if (length >= 4)
            {
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += destinationStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += destinationStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += destinationStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset += sourceStep);

                length -= 4;
                sourceOffset += sourceStep;
                destinationOffset += destinationStep;
            }

            while (length > 0)
            {
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset);

                length -= 1;
                sourceOffset += sourceStep;
                destinationOffset += destinationStep;
            }
        }

        /// <summary>
        /// Copies a sequence of discontiguous items from one memory area to another. This mirrors
        /// <see cref="CopyTo{T}(ref T,ref T,nint,nint)"/>, but <paramref name="sourceStep"/> refers to <paramref name="destinationRef"/> instead.
        /// </summary>
        /// <typeparam name="T">The type of items to copy.</typeparam>
        /// <param name="sourceRef">The source reference to copy from.</param>
        /// <param name="destinationRef">The target reference to copy to.</param>
        /// <param name="length">The total number of items to copy.</param>
        /// <param name="sourceStep">The step between consecutive items in the memory area pointed to by <paramref name="sourceRef"/>.</param>
        public static void CopyFrom<T>(ref T sourceRef, ref T destinationRef, nint length, nint sourceStep)
        {
            nint sourceOffset = 0;
            nint destinationOffset = 0;

            while (length >= 8)
            {
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += sourceStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset + 1);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += sourceStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset + 2);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += sourceStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset + 3);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += sourceStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset + 4);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += sourceStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset + 5);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += sourceStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset + 6);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += sourceStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset + 7);

                length -= 8;
                sourceOffset += 8;
                destinationOffset += sourceStep;
            }

            if (length >= 4)
            {
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += sourceStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset + 1);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += sourceStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset + 2);
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset += sourceStep) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset + 3);

                length -= 4;
                sourceOffset += 4;
                destinationOffset += sourceStep;
            }

            while (length > 0)
            {
                System.Runtime.CompilerServices.Unsafe.Add(ref destinationRef, destinationOffset) = System.Runtime.CompilerServices.Unsafe.Add(ref sourceRef, sourceOffset);

                length -= 1;
                sourceOffset += 1;
                destinationOffset += sourceStep;
            }
        }

        /// <summary>
        /// Fills a target memory area.
        /// </summary>
        /// <typeparam name="T">The type of values to fill.</typeparam>
        /// <param name="r0">A <typeparamref name="T"/> reference to the start of the memory area.</param>
        /// <param name="length">The number of items in the memory area.</param>
        /// <param name="step">The number of items between each consecutive target value.</param>
        /// <param name="value">The value to assign to every item in the target memory area.</param>
        public static void Fill<T>(ref T r0, nint length, nint step, T value)
        {
            nint offset = 0;

            while (length >= 8)
            {
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset) = value;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = value;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = value;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = value;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = value;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = value;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = value;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = value;

                length -= 8;
                offset += step;
            }

            if (length >= 4)
            {
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset) = value;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = value;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = value;
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset += step) = value;

                length -= 4;
                offset += step;
            }

            while (length > 0)
            {
                System.Runtime.CompilerServices.Unsafe.Add(ref r0, offset) = value;

                length -= 1;
                offset += step;
            }
        }
        #endregion // CommunityToolkit.HighPerformance.Enumerables
    }
}