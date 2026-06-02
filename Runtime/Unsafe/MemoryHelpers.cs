#if PKGE_USING_UNSAFE
#if INCLUDE_COLLECTIONS
using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;
using static Unity.Mathematics.math;

namespace PKGE.Unsafe
{
    #region Unity.Collections
    internal struct Memory
    {
        internal const long k_MaximumRamSizeInBytes = 1L << 40; // a terabyte

        [GenerateTestsForBurstCompatibility]
        unsafe internal struct Unmanaged
        {
            internal static void* Allocate(long size, int align, AllocatorManager.AllocatorHandle allocator)
            {
                return Array.Resize(null, 0, 1, allocator, size, align);
            }

            internal static void Free(void* pointer, AllocatorManager.AllocatorHandle allocator)
            {
                if (pointer == null)
                    return;
                Array.Resize(pointer, 1, 0, allocator, 1, 1);
            }

            [GenerateTestsForBurstCompatibility(GenericTypeArguments = new[] { typeof(int) })]
            internal static T* Allocate<T>(AllocatorManager.AllocatorHandle allocator) where T : unmanaged
            {
                return Array.Resize<T>(null, 0, 1, allocator);
            }

            [GenerateTestsForBurstCompatibility(GenericTypeArguments = new[] { typeof(int) })]
            internal static void Free<T>(T* pointer, AllocatorManager.AllocatorHandle allocator) where T : unmanaged
            {
                if (pointer == null)
                    return;
                Array.Resize(pointer, 1, 0, allocator);
            }

            [GenerateTestsForBurstCompatibility]
            internal struct Array
            {
                static bool IsCustom(AllocatorManager.AllocatorHandle allocator)
                {
                    return (int)allocator.Index >= AllocatorManager.FirstUserIndex;
                }

                static void* CustomResize(void* oldPointer, long oldCount, long newCount, AllocatorManager.AllocatorHandle allocator, long size, int align)
                {
                    AllocatorManager.Block block = default;
                    block.Range.Allocator = allocator;
                    block.Range.Items = (int)newCount;
                    block.Range.Pointer = (IntPtr)oldPointer;
                    block.BytesPerItem = (int)size;
                    block.Alignment = align;
                    block.AllocatedItems = (int)oldCount;
                    var error = AllocatorManager.Try(ref block);
                    CheckFailedToAllocate(error);
                    return (void*)block.Range.Pointer;
                }

                internal static void* Resize(void* oldPointer, long oldCount, long newCount, AllocatorManager.AllocatorHandle allocator,
                    long size, int align)
                {
                    // Make the alignment multiple of cacheline size
                    var alignment = Unity.Mathematics.math.max(Unity.Jobs.LowLevel.Unsafe.JobsUtility.CacheLineSize, align);

                    if (IsCustom(allocator))
                        return CustomResize(oldPointer, oldCount, newCount, allocator, size, alignment);
                    void* newPointer = default;
                    if (newCount > 0)
                    {
                        long bytesToAllocate = newCount * size;
                        CheckByteCountIsReasonable(bytesToAllocate);
                        newPointer = UnsafeUtility.MallocTracked(bytesToAllocate, alignment, allocator.ToAllocator, 0);
                        if (oldCount > 0)
                        {
                            long count = Unity.Mathematics.math.min(oldCount, newCount);
                            long bytesToCopy = count * size;
                            CheckByteCountIsReasonable(bytesToCopy);
                            UnsafeUtility.MemCpy(newPointer, oldPointer, bytesToCopy);
                        }
                    }
                    if (oldCount > 0)
                        UnsafeUtility.FreeTracked(oldPointer, allocator.ToAllocator);
                    return newPointer;
                }

                [GenerateTestsForBurstCompatibility(GenericTypeArguments = new[] { typeof(int) })]
                internal static T* Resize<T>(T* oldPointer, long oldCount, long newCount, AllocatorManager.AllocatorHandle allocator) where T : unmanaged
                {
                    return (T*)Resize((byte*)oldPointer, oldCount, newCount, allocator, SizeOfCache<T>.Size, AlignOfCache<T>.Alignment);
                }

                [GenerateTestsForBurstCompatibility(GenericTypeArguments = new[] { typeof(int) })]
                internal static T* Allocate<T>(long count, AllocatorManager.AllocatorHandle allocator)
                    where T : unmanaged
                {
                    return Resize<T>(null, 0, count, allocator);
                }

                [GenerateTestsForBurstCompatibility(GenericTypeArguments = new[] { typeof(int) })]
                internal static void Free<T>(T* pointer, long count, AllocatorManager.AllocatorHandle allocator)
                    where T : unmanaged
                {
                    if (pointer == null)
                        return;
                    Resize(pointer, count, 0, allocator);
                }
            }
        }

        [GenerateTestsForBurstCompatibility]
        unsafe internal struct Array
        {
            [GenerateTestsForBurstCompatibility(GenericTypeArguments = new[] { typeof(int) })]
            internal static void Set<T>(T* pointer, long count, T t = default) where T : unmanaged
            {
                long bytesToSet = count * SizeOfCache<T>.Size;
                CheckByteCountIsReasonable(bytesToSet);
                for (var i = 0; i < count; ++i)
                    pointer[i] = t;
            }

            [GenerateTestsForBurstCompatibility(GenericTypeArguments = new[] { typeof(int) })]
            internal static void Clear<T>(T* pointer, long count) where T : unmanaged
            {
                long bytesToClear = count * SizeOfCache<T>.Size;
                CheckByteCountIsReasonable(bytesToClear);
                UnsafeUtility.MemClear(pointer, bytesToClear);
            }

            [GenerateTestsForBurstCompatibility(GenericTypeArguments = new[] { typeof(int) })]
            internal static void Copy<T>(T* dest, T* src, long count) where T : unmanaged
            {
                long bytesToCopy = count * SizeOfCache<T>.Size;
                CheckByteCountIsReasonable(bytesToCopy);
                UnsafeUtility.MemCpy(dest, src, bytesToCopy);
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS"), Conditional("UNITY_DOTS_DEBUG")]
        internal static void CheckByteCountIsReasonable(long size)
        {
            if (size < 0)
                throw new InvalidOperationException($"Attempted to operate on {size} bytes of memory: negative size");
            if (size > k_MaximumRamSizeInBytes)
                throw new InvalidOperationException($"Attempted to operate on {size} bytes of memory: size too big");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS"), Conditional("UNITY_DOTS_DEBUG")]
        internal static void CheckFailedToAllocate(int error)
        {
            if (error != 0)
                throw new ArgumentException("failed to allocate");
        }
    }
    #endregion // Unity.Collections

    public static partial class MemoryHelpers
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/GPUDriven/Utilities/MemoryUtilities.cs
        #region UnityEngine.Rendering
        public static unsafe T* MallocTracked<T>(int count, Allocator allocator, int callstacksToSkip = 0) where T : unmanaged
        {
            return (T*)UnsafeUtility.MallocTracked(
                SizeOfCache<T>.Size * count,
                AlignOfCache<T>.Alignment,
                allocator,
                callstacksToSkip);
        }

        public static unsafe void FreeTracked<T>(T* p, Allocator allocator) where T : unmanaged
        {
            UnsafeUtility.FreeTracked(p, allocator);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Compatibility")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "UnusedParameter.Global")]
        public static unsafe T* Malloc<T>(int count, Allocator allocator, int callstacksToSkip = 0) where T : unmanaged
        {
            return (T*)UnsafeUtility.Malloc(
                SizeOfCache<T>.Size * count,
                AlignOfCache<T>.Alignment,
                allocator);
        }

        public static unsafe void Free<T>(T* p, Allocator allocator) where T : unmanaged
        {
            UnsafeUtility.Free(p, allocator);
        }
        #endregion // UnityEngine.Rendering

        //https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/InputSystem/Utilities/MemoryHelpers.cs
        #region UnityEngine.InputSystem.Utilities
        public static unsafe void MemCpyBitRegion(Span<byte> destination, Span<byte> source, uint bitOffset, uint bitCount)
        {
            var destPtr = (byte*)UnsafeUtility.AddressOf(ref destination[0]);
            var sourcePtr = (byte*)UnsafeUtility.AddressOf(ref source[0]);

            // If we're offset by more than a byte, adjust our pointers.
            if (bitOffset >= 8)
            {
                var skipBytes = bitOffset / 8;
                destPtr += skipBytes;
                sourcePtr += skipBytes;
                bitOffset %= 8;
            }

            // Copy unaligned prefix, if any.
            if (bitOffset > 0)
            {
                var byteMask = 0xFF << (int)bitOffset;
                if (bitCount + bitOffset < 8)
                    byteMask &= 0xFF >> (int)(8 - (bitCount + bitOffset));

                *destPtr = (byte)(((*destPtr & ~byteMask) | (*sourcePtr & byteMask)) & 0xFF);

                // If the total length of the memory region is equal or less than a byte,
                // we're done.
                if (bitCount + bitOffset <= 8)
                    return;

                ++destPtr;
                ++sourcePtr;

                bitCount -= 8 - bitOffset;
            }

            // Copy contiguous bytes in-between, if any.
            var byteCount = bitCount / 8;
            if (byteCount >= 1)
                UnsafeUtility.MemCpy(destPtr, sourcePtr, byteCount);

            // Copy unaligned suffix, if any.
            var remainingBitCount = bitCount % 8;
            if (remainingBitCount > 0)
            {
                destPtr += byteCount;
                sourcePtr += byteCount;

                // We want the lowest remaining bits.
                var byteMask = 0xFF >> (int)(8 - remainingBitCount);

                *destPtr = (byte)(((*destPtr & ~byteMask) | (*sourcePtr & byteMask)) & 0xFF);
            }
        }

        public static unsafe void MemSet(Span<byte> destination, int numBytes, byte value)
        {
            var to = (byte*)UnsafeUtility.AddressOf(ref destination[0]);
            var pos = 0;

            unchecked
            {
                // 64bit blocks.
#if UNITY_64
                while (numBytes >= 8)
                {
                    *(ulong*)&to[pos] = ((ulong)value << 56) | ((ulong)value << 48) | ((ulong)value << 40) | ((ulong)value << 32)
                        | ((ulong)value << 24) | ((ulong)value << 16) | ((ulong)value << 8) | value;
                    numBytes -= 8;
                    pos += 8;
                }
#endif

                // 32bit blocks.
                while (numBytes >= 4)
                {
                    *(uint*)&to[pos] = ((uint)value << 24) | ((uint)value << 16) | ((uint)value << 8) | value;
                    numBytes -= 4;
                    pos += 4;
                }

                // Remaining bytes.
                while (numBytes > 0)
                {
                    to[pos] = value;
                    numBytes -= 1;
                    pos += 1;
                }
            }
        }

        /// <summary>
        /// Copy from <paramref name="source"/> to <paramref name="destination"/> all the bits that
        /// ARE set in <paramref name="mask"/>.
        /// </summary>
        /// <param name="destination">Memory to copy to.</param>
        /// <param name="source">Memory to copy from.</param>
        /// <param name="numBytes">Number of bytes to copy.</param>
        /// <param name="mask">Bitmask that determines which bits to copy. Bits that are set WILL be copied.</param>
        public static unsafe void MemCpyMasked(Span<byte> destination, Span<byte> source, int numBytes, Span<byte> mask)
        {
            var from = (byte*)UnsafeUtility.AddressOf(ref source[0]);
            var to = (byte*)UnsafeUtility.AddressOf(ref destination[0]);
            var bits = (byte*)UnsafeUtility.AddressOf(ref mask[0]);
            var pos = 0;

            unchecked
            {
                // Copy 64bit blocks.
#if UNITY_64
                while (numBytes >= 8)
                {
                    *(ulong*)(to + pos) &= ~*(ulong*)(bits + pos); // Preserve unmasked bits.
                    *(ulong*)(to + pos) |= *(ulong*)(from + pos) & *(ulong*)(bits + pos); // Copy masked bits.
                    numBytes -= 8;
                    pos += 8;
                }
#endif

                // Copy 32bit blocks.
                while (numBytes >= 4)
                {
                    *(uint*)(to + pos) &= ~*(uint*)(bits + pos); // Preserve unmasked bits.
                    *(uint*)(to + pos) |= *(uint*)(from + pos) & *(uint*)(bits + pos); // Copy masked bits.
                    numBytes -= 4;
                    pos += 4;
                }

                // Copy remaining bytes.
                while (numBytes > 0)
                {
                    unchecked
                    {
                        to[pos] &= (byte)~bits[pos]; // Preserve unmasked bits.
                        to[pos] |= (byte)(from[pos] & bits[pos]); // Copy masked bits.
                    }

                    numBytes -= 1;
                    pos += 1;
                }
            }
        }
        #endregion // UnityEngine.InputSystem.Utilities
    }
}
#endif // INCLUDE_COLLECTIONS
#endif // PKGE_USING_UNSAFE
