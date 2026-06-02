#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using UnityEngine.Scripting;

namespace PKGE
{
    // Non-allocating sorts.
    public struct Sorting
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.universal/Runtime/NoAllocUtils.cs
        #region UnityEngine.Rendering.Universal
        /// <summary>Add profiling samplers to sorts as they are often a bottleneck when scaling things up.</summary>
        /// <remarks>By default, avoid sampling recursion, but these can be used externally as well.</remarks>
        static readonly ProfilingSampler QuickSortSampler = new ProfilingSampler(nameof(QuickSort));
        /// <inheritdoc cref="QuickSortSampler"/>
        static readonly ProfilingSampler InsertionSortSampler = new ProfilingSampler(nameof(InsertionSort));

        public static void QuickSort<T>(T[] data, Func<T, T, int> compare)
        {
            using var scope = new ProfilingScope(QuickSortSampler);
            QuickSort(data, 0, data.Length - 1, compare);
        }

        /// <summary>
        /// A non-allocating predicated sub-array quick sort for managed arrays.
        /// </summary>
        /// <remarks>
        /// Similar to <see cref="UnityEngine.Rendering.CoreUnsafeUtils.QuickSort"/> in CoreUnsafeUtils.cs
        /// <example><code>
        /// Sorting.QuickSort(test, 0, test.Length - 1, (int a, int b) => a - b);
        /// </code></example>
        /// </remarks>
        /// <param name="data"></param>
        /// <param name="start"></param>
        /// <param name="end">The end param is inclusive.</param>
        /// <param name="compare">Returns -1, 0 or 1.</param>
        public static void QuickSort<T>(T[] data, int start, int end, Func<T, T, int> compare)
        {
            while (true)
            {
                int diff = end - start;
                if (diff < 1)
                    return;

                if (diff < 8)
                {
                    InsertionSort(data, start, end, compare);
                    return;
                }

                Assert.IsTrue((uint)start < data.Length);
                Assert.IsTrue((uint)end < data.Length); // end == inclusive

                if (start < end)
                {
                    int pivot = Partition(data, start, end, compare);

                    if (pivot >= 1)
                        QuickSort(data, start, pivot, compare);

                    if (pivot + 1 < end)
                    {
                        start = pivot + 1;
                        continue;
                    }
                }

                break;
            }
        }

        static T Median3Pivot<T>(T[] data, int start, int pivot, int end, Func<T, T, int> compare)
        {
            if (compare(data[end], data[start]) < 0)
                Swap(start, end);

            if (compare(data[pivot], data[start]) < 0)
                Swap(start, pivot);

            if (compare(data[end], data[pivot]) < 0)
                Swap(pivot, end);

            return data[pivot];

            void Swap(int a, int b)
            {
                (data[a], data[b]) = (data[b], data[a]);
            }
        }

        static int Partition<T>(T[] data, int start, int end, Func<T, T, int> compare)
        {
            int diff = end - start;
            int pivot = start + diff / 2;

            var pivotValue = Median3Pivot(data, start, pivot, end, compare);

            while (true)
            {
                while (compare(data[start], pivotValue) < 0)
                    ++start;

                while (compare(data[end], pivotValue) > 0)
                    --end;

                if (start >= end)
                {
                    return end;
                }

                var tmp = data[start];
                data[start++] = data[end];
                data[end--] = tmp;
            }
        }

        public static void InsertionSort<T>(T[] data, Func<T, T, int> compare)
        {
            using var scope = new ProfilingScope(InsertionSortSampler);
            InsertionSort(data, 0, data.Length - 1, compare);
        }

        /// <summary>
        /// A non-allocating predicated sub-array insertion sort for managed arrays.
        /// </summary>
        /// <remarks>Called also from <see cref="QuickSort{T}(T[], int, int, Func{T, T, int})"/> for small ranges.</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="compare"></param>
        public static void InsertionSort<T>(T[] data, int start, int end, Func<T, T, int> compare)
        {
            Assert.IsTrue((uint)start < data.Length);
            Assert.IsTrue((uint)end < data.Length);

            for (int i = start + 1; i < end + 1; i++)
            {
                var iData = data[i];
                int j = i - 1;
                while (j >= 0 && compare(iData, data[j]) < 0)
                {
                    data[j + 1] = data[j];
                    j--;
                }

                data[j + 1] = iData;
            }
        }
        #endregion // UnityEngine.Rendering.Universal
    }

    /// <summary>
    /// <para>Some helpers to handle <see cref="List{T}"/> in C# API (used for no-alloc APIs where user provides the list to be filled).</para>
    /// <para>On il2cpp/mono we can "resize" <see cref="List{T}"/> (up to <see cref="List{T}.Capacity"/>, sure, but this is/should-be handled higher level).</para>
    /// <para>Also, we can easily "convert" <see cref="List{T}"/> to <see cref="Array"/>.</para>
    /// </summary>
    /// <remarks>
    /// NB .NET backend is treated as second-class citizen going through <see cref="List{T}.ToArray()"/> call.
    /// </remarks>
    public static class NoAllocHelpers
    {
        //https://github.com/Unity-Technologies/UnityCsReference/blob/b42ec0031fc505c35aff00b6a36c25e67d81e59e/Runtime/Export/Scripting/NoAllocHelpers.bindings.cs
        #region UnityEngine
        /// <summary><see cref="ResetListSize{T}"/> with runtime checks.</summary>
        /// <remarks>Also clears the <see cref="List{T}"/> <paramref name="list"/>.</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">Input <see cref="List{T}"/>.</param>
        /// <param name="count">Desired element count.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void EnsureListElemCount<T>(this List<T> list, int count)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (count < 0)
                throw new ArgumentException($"{nameof(count)} must not be negative.", nameof(count));

            list.Clear();

            // make sure capacity is enough (that's where alloc WILL happen if needed)
            if (list.Capacity < count)
                list.Capacity = count;

            if (count != list.Count)
            {
                var tListAccess = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<List<T>, ListPrivateFieldAccess<T>>(ref list);
                tListAccess._size = count;
                tListAccess._version++;
            }
        }

        // tiny helpers
        public static int SafeLength(this Array? values) { return values != null ? values.Length : 0; }
        public static int SafeLength<T>(this List<T>? values) { return values != null ? values.Count : 0; }

        /// <remarks>
        /// Returned array will be invalid if the Capacity of the List is modified.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">Input <see cref="System.Collections.Generic.List{T}"/>.</param>
        /// <returns>Internal <see cref="{T}[]"/> of <paramref name="list"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[]? ExtractArrayFromList<T>(this List<T>? list)
        {
            if (list == null)
                return null;

            var tListAccess = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<List<T>, ListPrivateFieldAccess<T>>(ref list);
            return tListAccess._items;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetListContents<T>(this List<T> list, ReadOnlySpan<T> span)
        {
            var tListAccess = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<List<T>, ListPrivateFieldAccess<T>>(ref list);

            // Do not reallocate the _items array if it is already
            // large enough to contain all the elements of span
            if (tListAccess._items.Length >= span.Length)
                span.CopyTo(tListAccess._items);
            else
                tListAccess._items = span.ToArray();

            tListAccess._size = span.Length;
            tListAccess._version++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ResetListSize<T>(this List<T> list, int size)
        {
            var tListAccess = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<List<T>, ListPrivateFieldAccess<T>>(ref list);
            tListAccess._size = size;
            tListAccess._version++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetListSizeNoResize<T>(this List<T> list, int size)
        {
            Assert.IsTrue(size >= 0);
            Assert.IsTrue(size <= list.Capacity);

            list.ResetListSize(size);
        }

        /// <summary>
        /// This is a helper class to allow the binding code to manipulate the internal fields of
        /// System.Collections.Generic.List. The field order below must not be changed.
        /// </summary>
        [Preserve]
        internal class ListPrivateFieldAccess<T>
        {
#pragma warning disable CS0649
#pragma warning disable CS8618
            internal T[] _items; // Do not rename (binary serialization)
#pragma warning restore CS8618
            internal int _size; // Do not rename (binary serialization)
            internal int _version; // Do not rename (binary serialization)
#pragma warning restore CS0649
        }
        #endregion // UnityEngine

        public static void AddRange<T>(this List<T> list, ReadOnlySpan<T> span)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            // make sure capacity is enough (that's where alloc WILL happen if needed)
            list.EnsureRoom(span.Length);

            var tListAccess = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<List<T>, ListPrivateFieldAccess<T>>(ref list);
            span.CopyTo(tListAccess._items.AsSpan(tListAccess._size, span.Length));
            tListAccess._size += span.Length;
            tListAccess._version++;
        }
    }

    public static partial class MemoryHelpers
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/GPUDriven/Utilities/MemoryUtilities.cs
        #region UnityEngine.InputSystem.Utilities

        public readonly struct BitRegion
        {
            public readonly uint BITOffset;
            public readonly uint SizeInBits;

            public bool isEmpty => SizeInBits == 0;

            public BitRegion(uint bitOffset, uint sizeInBits)
            {
                this.BITOffset = bitOffset;
                this.SizeInBits = sizeInBits;
            }

            public BitRegion(uint byteOffset, uint bitOffset, uint sizeInBits)
            {
                this.BITOffset = byteOffset * 8 + bitOffset;
                this.SizeInBits = sizeInBits;
            }

            public BitRegion Overlap(BitRegion other)
            {
                ////REVIEW: too many branches; this can probably be done much smarter

                var thisEnd = BITOffset + SizeInBits;
                var otherEnd = other.BITOffset + other.SizeInBits;

                if (thisEnd <= other.BITOffset || otherEnd <= BITOffset)
                    return default;

                var end = System.Math.Min(thisEnd, otherEnd);
                var start = System.Math.Max(BITOffset, other.BITOffset);

                return new BitRegion(start, end - start);
            }
        }

        public static bool Compare(Span<byte> ptr1, Span<byte> ptr2, BitRegion region)
        {
            if (region.SizeInBits == 1)
                return ReadSingleBit(ptr1, region.BITOffset) == ReadSingleBit(ptr2, region.BITOffset);
            return MemCmpBitRegion(ptr1, ptr2, region.BITOffset, region.SizeInBits);
        }

        public static uint ComputeFollowingByteOffset(uint byteOffset, uint sizeInBits)
        {
            return (uint)(byteOffset + sizeInBits / 8 + (sizeInBits % 8 > 0 ? 1 : 0));
        }

        public static void WriteSingleBit(Span<byte> ptr, uint bitOffset, bool value)
        {
            int byteOffset = (int)(bitOffset >> 3);
            bitOffset &= 7;
            if (value)
                ptr[byteOffset] |= (byte)(1U << (int)bitOffset);
            else
                ptr[byteOffset] &= (byte)~(1U << (int)bitOffset);
        }

        public static bool ReadSingleBit(Span<byte> ptr, uint bitOffset)
        {
            int byteOffset = (int)(bitOffset >> 3);
            bitOffset &= 7;
            return (ptr[byteOffset] & (1U << (int)bitOffset)) != 0;
        }

        /// <summary>
        /// Compare two memory regions that may be offset by a bit-count and have a length expressed
        /// in bits.
        /// </summary>
        /// <param name="ptr1">Pointer to start of first memory region.</param>
        /// <param name="ptr2">Pointer to start of second memory region.</param>
        /// <param name="bitOffset">Offset in bits from each of the pointers to the start of the memory region to compare.</param>
        /// <param name="bitCount">Number of bits to compare in the memory region.</param>
        /// <param name="mask">If not null, only compare bits set in the mask. This allows comparing two memory regions while
        /// ignoring specific bits.</param>
        /// <returns>True if the two memory regions are identical, false otherwise.</returns>
        public static bool MemCmpBitRegion(Span<byte> ptr1, Span<byte> ptr2, uint bitOffset, uint bitCount, Span<byte> mask = default)
        {
            var bytePtr1 = ptr1;
            var bytePtr2 = ptr2;
            var maskPtr = mask;

            // If we're offset by more than a byte, adjust our pointers.
            if (bitOffset >= 8)
            {
                var skipBytes = (int)(bitOffset / 8);
                bytePtr1 = bytePtr1[skipBytes..];
                bytePtr2 = bytePtr2[skipBytes..];
                if (maskPtr != null)
                    maskPtr = bytePtr2[skipBytes..];
                bitOffset %= 8;
            }

            // Compare unaligned prefix, if any.
            if (bitOffset > 0)
            {
                // If the total length of the memory region is less than a byte, we need
                // to mask out parts of the bits we're reading.
                var byteMask = 0xFF << (int)bitOffset;
                if (bitCount + bitOffset < 8)
                    byteMask &= 0xFF >> (int)(8 - (bitCount + bitOffset));

                if (maskPtr != null)
                {
                    byteMask &= maskPtr[0];
                    maskPtr = maskPtr[1..];
                }

                var byte1 = bytePtr1[0] & byteMask;
                var byte2 = bytePtr2[0] & byteMask;

                if (byte1 != byte2)
                    return false;

                // If the total length of the memory region is equal or less than a byte,
                // we're done.
                if (bitCount + bitOffset <= 8)
                    return true;

                bytePtr1 = bytePtr1[1..];
                bytePtr2 = bytePtr2[1..];

                bitCount -= 8 - bitOffset;
            }

            // Compare contiguous bytes in-between, if any.
            var byteCount = (int)(bitCount / 8);
            if (byteCount >= 1)
            {
                if (maskPtr != null)
                {
                    ////REVIEW: could go int by int here for as long as we can
                    // Have to go byte-by-byte in order to apply the masking.
                    for (var i = 0; i < byteCount; ++i)
                    {
                        var byte1 = bytePtr1[i];
                        var byte2 = bytePtr2[i];
                        var byteMask = maskPtr[i];

                        if ((byte1 & byteMask) != (byte2 & byteMask))
                            return false;
                    }
                }
                else
                {
                    if (!bytePtr1.SequenceEqual(bytePtr2))
                        return false;
                }
            }

            // Compare unaligned suffix, if any.
            var remainingBitCount = bitCount % 8;
            if (remainingBitCount > 0)
            {
                bytePtr1 = bytePtr1[byteCount..];
                bytePtr2 = bytePtr2[byteCount..];

                // We want the lowest remaining bits.
                var byteMask = 0xFF >> (int)(8 - remainingBitCount);

                if (maskPtr != null)
                {
                    maskPtr = maskPtr[byteCount..];
                    byteMask &= maskPtr[0];
                }

                var byte1 = bytePtr1[0] & byteMask;
                var byte2 = bytePtr2[0] & byteMask;

                if (byte1 != byte2)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Reads bits memory region as unsigned int, up to and including 32 bits, least-significant bit first (LSB).
        /// </summary>
        /// <param name="ptr">Pointer to memory region.</param>
        /// <param name="bitOffset">Offset in bits from the pointer to the start of the unsigned integer.</param>
        /// <param name="bitCount">Number of bits to read.</param>
        /// <returns>Read unsigned integer.</returns>
        public static uint ReadMultipleBitsAsUInt(Span<byte> ptr, uint bitOffset, uint bitCount)
        {
            if (ptr == null)
                throw new ArgumentNullException(nameof(ptr));
            if (bitCount > sizeof(int) * 8)
                throw new ArgumentException("Trying to read more than 32 bits as int", nameof(bitCount));

            // Shift the pointer up on larger bitmasks and retry.
            if (bitOffset > 32)
            {
                var newBitOffset = (int)bitOffset % 32;
                var intOffset = ((int)bitOffset - newBitOffset) / 32;
                ptr = ptr[(intOffset * 4)..];
                bitOffset = (uint)newBitOffset;
            }

            // Bits out of byte.
            if (bitOffset + bitCount <= 8)
            {
                var value = ptr[0];
                value >>= (int)bitOffset;
                var mask = 0xFFu >> (8 - (int)bitCount);
                return value & mask;
            }

            // Bits out of short.
            if (bitOffset + bitCount <= 16)
            {
                var value = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, ushort>(ptr)[0];
                value >>= (int)bitOffset;
                var mask = 0xFFFFu >> (16 - (int)bitCount);
                return value & mask;
            }

            // Bits out of int.
            if (bitOffset + bitCount <= 32)
            {
                var value = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(ptr)[0];
                value >>= (int)bitOffset;
                var mask = 0xFFFFFFFFu >> (32 - (int)bitCount);
                return value & mask;
            }

            throw new NotImplementedException("Reading int straddling int boundary");
        }

        /// <summary>
        /// Writes unsigned int as bits to memory region, up to and including 32 bits, least-significant bit first (LSB).
        /// </summary>
        /// <param name="ptr">Pointer to memory region.</param>
        /// <param name="bitOffset">Offset in bits from the pointer to the start of the unsigned integer.</param>
        /// <param name="bitCount">Number of bits to read.</param>
        /// <param name="value">Value to write.</param>
        public static void WriteUIntAsMultipleBits(Span<byte> ptr, uint bitOffset, uint bitCount, uint value)
        {
            if (ptr == null)
                throw new ArgumentNullException(nameof(ptr));
            if (bitCount > sizeof(int) * 8)
                throw new ArgumentException("Trying to write more than 32 bits as int", nameof(bitCount));

            // Shift the pointer up on larger bitmasks and retry.
            if (bitOffset > 32)
            {
                var newBitOffset = (int)bitOffset % 32;
                var intOffset = ((int)bitOffset - newBitOffset) / 32;
                ptr = ptr[(intOffset * 4)..];
                bitOffset = (uint)newBitOffset;
            }

            // Bits out of byte.
            if (bitOffset + bitCount <= 8)
            {
                var byteValue = (byte)value;
                byteValue <<= (int)bitOffset;
                var mask = ~((0xFFU >> (8 - (int)bitCount)) << (int)bitOffset);
                ptr[0] = (byte)((ptr[0] & mask) | byteValue);
                return;
            }

            // Bits out of short.
            if (bitOffset + bitCount <= 16)
            {
                var ushortValue = (ushort)value;
                ushortValue <<= (int)bitOffset;
                var mask = ~((0xFFFFU >> (16 - (int)bitCount)) << (int)bitOffset);
                var ushortPtr = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, ushort>(ptr);
                ushortPtr[0] = (ushort)((ushortPtr[0] & mask) | ushortValue);
                return;
            }

            // Bits out of int.
            if (bitOffset + bitCount <= 32)
            {
                var uintValue = value;
                uintValue <<= (int)bitOffset;
                var mask = ~((0xFFFFFFFFU >> (32 - (int)bitCount)) << (int)bitOffset);
                var uintPtr = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(ptr);
                uintPtr[0] = (uintPtr[0] & mask) | uintValue;
                return;
            }

            throw new NotImplementedException("Writing int straddling int boundary");
        }

        /// <summary>
        /// Reads bits memory region as two's complement integer, up to and including 32 bits, least-significant bit first (LSB).
        /// For example reading 0xff as 8 bits will result in -1.
        /// </summary>
        /// <param name="ptr">Pointer to memory region.</param>
        /// <param name="bitOffset">Offset in bits from the pointer to the start of the integer.</param>
        /// <param name="bitCount">Number of bits to read.</param>
        /// <returns>Read integer.</returns>
        public static int ReadTwosComplementMultipleBitsAsInt(Span<byte> ptr, uint bitOffset, uint bitCount)
        {
            // int is already represented as two's complement
            return (int)ReadMultipleBitsAsUInt(ptr, bitOffset, bitCount);
        }

        /// <summary>
        /// Writes bits memory region as two's complement integer, up to and including 32 bits, least-significant bit first (LSB).
        /// </summary>
        /// <param name="ptr">Pointer to memory region.</param>
        /// <param name="bitOffset">Offset in bits from the pointer to the start of the integer.</param>
        /// <param name="bitCount">Number of bits to read.</param>
        /// <param name="value">Value to write.</param>
        public static void WriteIntAsTwosComplementMultipleBits(Span<byte> ptr, uint bitOffset, uint bitCount, int value)
        {
            // int is already represented as two's complement, so write as-is
            WriteUIntAsMultipleBits(ptr, bitOffset, bitCount, (uint)value);
        }

        /// <summary>
        /// Reads bits memory region as excess-K integer where K is set to (2^bitCount)/2, up to and including 32 bits, least-significant bit first (LSB).
        /// For example reading 0 as 8 bits will result in -128. Reading 0xff as 8 bits will result in 127.
        /// </summary>
        /// <param name="ptr">Pointer to memory region.</param>
        /// <param name="bitOffset">Offset in bits from the pointer to the start of the integer.</param>
        /// <param name="bitCount">Number of bits to read.</param>
        /// <returns>Read integer.</returns>
        public static int ReadExcessKMultipleBitsAsInt(Span<byte> ptr, uint bitOffset, uint bitCount)
        {
            // https://en.wikipedia.org/wiki/Signed_number_representations#Offset_binary
            var value = (long)ReadMultipleBitsAsUInt(ptr, bitOffset, bitCount);
            var halfMax = (long)((1UL << (int)bitCount) / 2);
            return (int)(value - halfMax);
        }

        /// <summary>
        /// Writes bits memory region as excess-K integer where K is set to (2^bitCount)/2, up to and including 32 bits, least-significant bit first (LSB).
        /// </summary>
        /// <param name="ptr">Pointer to memory region.</param>
        /// <param name="bitOffset">Offset in bits from the pointer to the start of the integer.</param>
        /// <param name="bitCount">Number of bits to read.</param>
        /// <param name="value">Value to write.</param>
        public static void WriteIntAsExcessKMultipleBits(Span<byte> ptr, uint bitOffset, uint bitCount, int value)
        {
            // https://en.wikipedia.org/wiki/Signed_number_representations#Offset_binary
            var halfMax = (long)((1UL << (int)bitCount) / 2);
            var unsignedValue = halfMax + value;
            WriteUIntAsMultipleBits(ptr, bitOffset, bitCount, (uint)unsignedValue);
        }

        /// <summary>
        /// Reads bits memory region as normalized unsigned integer, up to and including 32 bits, least-significant bit first (LSB).
        /// For example reading 0 as 8 bits will result in 0.0f. Reading 0xff as 8 bits will result in 1.0f.
        /// </summary>
        /// <param name="ptr">Pointer to memory region.</param>
        /// <param name="bitOffset">Offset in bits from the pointer to the start of the unsigned integer.</param>
        /// <param name="bitCount">Number of bits to read.</param>
        /// <returns>Normalized unsigned integer.</returns>
        public static float ReadMultipleBitsAsNormalizedUInt(Span<byte> ptr, uint bitOffset, uint bitCount)
        {
            var uintValue = ReadMultipleBitsAsUInt(ptr, bitOffset, bitCount);
            var maxValue = (uint)((1UL << (int)bitCount) - 1);
            return uintValue.UIntToNormalizedFloat(0, maxValue);
        }

        /// <summary>
        /// Writes bits memory region as normalized unsigned integer, up to and including 32 bits, least-significant bit first (LSB).
        /// </summary>
        /// <param name="ptr">Pointer to memory region.</param>
        /// <param name="bitOffset">Offset in bits from the pointer to the start of the unsigned integer.</param>
        /// <param name="bitCount">Number of bits to read.</param>
        /// <param name="value">Normalized value to write.</param>
        public static void WriteNormalizedUIntAsMultipleBits(Span<byte> ptr, uint bitOffset, uint bitCount, float value)
        {
            var maxValue = (uint)((1UL << (int)bitCount) - 1);
            var uintValue = value.NormalizedFloatToUInt(0, maxValue);
            WriteUIntAsMultipleBits(ptr, bitOffset, bitCount, uintValue);
        }

        public static void SetBitsInBuffer(Span<byte> buffer, int byteOffset, int bitOffset, int sizeInBits, bool value)
        {
            if (buffer == null)
                throw new ArgumentException("A buffer must be provided to apply the bitmask on", nameof(buffer));
            if (sizeInBits < 0)
                throw new ArgumentException("Negative sizeInBits", nameof(sizeInBits));
            if (bitOffset < 0)
                throw new ArgumentException("Negative bitOffset", nameof(bitOffset));
            if (byteOffset < 0)
                throw new ArgumentException("Negative byteOffset", nameof(byteOffset));

            // If we're offset by more than a byte, adjust our pointers.
            if (bitOffset >= 8)
            {
                var skipBytes = bitOffset / 8;
                byteOffset += skipBytes;
                bitOffset %= 8;
            }

            var bytePos = buffer[byteOffset..];
            var sizeRemainingInBits = sizeInBits;

            // Handle first byte separately if unaligned to byte boundary.
            if (bitOffset != 0)
            {
                var mask = 0xFF << bitOffset;
                if (sizeRemainingInBits + bitOffset < 8)
                {
                    mask &= 0xFF >> (8 - (sizeRemainingInBits + bitOffset));
                }

                if (value)
                    bytePos[0] |= (byte)mask;
                else
                    bytePos[0] &= (byte)~mask;
                bytePos = bytePos[1..];
                sizeRemainingInBits -= 8 - bitOffset;
            }

            // Handle full bytes in-between.
            while (sizeRemainingInBits >= 8)
            {
                bytePos[0] = value ? (byte)0xFF : (byte)0;
                bytePos = bytePos[1..];
                sizeRemainingInBits -= 8;
            }

            // Handle unaligned trailing byte, if present.
            if (sizeRemainingInBits > 0)
            {
                var mask = (byte)(0xFF >> 8 - sizeRemainingInBits);
                if (value)
                    bytePos[0] |= mask;
                else
                    bytePos[0] &= (byte)~mask;
            }

#if ZERO
            unsafe
            {
                Assert.IsTrue((byte*)UnsafeUtility.AddressOf(ref bytePos[0]) <= (byte*)UnsafeUtility.AddressOf(ref buffer[
                    (int)ComputeFollowingByteOffset((uint)byteOffset, (uint)bitOffset + (uint)sizeInBits)]));
            }
#endif // ZERO
        }

        public static uint AlignNatural(uint offset, uint sizeInBytes)
        {
            var alignment = System.Math.Min(8, sizeInBytes);
            return offset.AlignToMultipleOf(alignment);
        }
        #endregion // UnityEngine.InputSystem.Utilities
    }
}
