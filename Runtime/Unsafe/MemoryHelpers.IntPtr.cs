#if PKGE_USING_INTPTR
#if INCLUDE_COLLECTIONS
using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.Assertions;
using static Unity.Mathematics.math;

namespace PKGE.Unsafe
{
    public static unsafe partial class MemoryHelpers
    {
        //https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/InputSystem/Utilities/MemoryHelpers.cs
        #region UnityEngine.InputSystem.Utilities
        public static bool Compare(IntPtr ptr1, IntPtr ptr2, BitRegion region)
        {
            return Compare((void*)ptr1, (void*)ptr2, region);
        }

        public static void WriteSingleBit(IntPtr ptr, uint bitOffset, bool value)
        {
            WriteSingleBit((void*)ptr, bitOffset, value);
        }

        public static bool ReadSingleBit(IntPtr ptr, uint bitOffset)
        {
            return ReadSingleBit((void*)ptr, bitOffset);
        }

        /// <inheritdoc cref="MemCpyBitRegion"/>
        public static void MemCpyBitRegion(IntPtr destination, IntPtr source, uint bitOffset, uint bitCount)
        {
            MemCpyBitRegion((void*)destination, (void*)source, bitOffset, bitCount);
        }

        /// <inheritdoc cref="MemCmpBitRegion"/>
        public static bool MemCmpBitRegion(IntPtr ptr1, IntPtr ptr2, uint bitOffset, uint bitCount, IntPtr mask = default)
        {
            return MemCmpBitRegion((void*)ptr1, (void*)ptr2, bitOffset, bitCount, (void*)mask);
        }

        /// <inheritdoc cref="MemSet"/>
        public static void MemSet(IntPtr destination, int numBytes, byte value)
        {
            MemSet((void*)destination, numBytes, value);
        }

        /// <inheritdoc cref="MemCpyMasked"/>
        public static void MemCpyMasked(IntPtr destination, IntPtr source, int numBytes, IntPtr mask)
        {
            MemCpyMasked((void*)destination, (void*)source, numBytes, (void*)mask);
        }

        /// <inheritdoc cref="ReadMultipleBitsAsUInt"/>
        public static uint ReadMultipleBitsAsUInt(IntPtr ptr, uint bitOffset, uint bitCount)
        {
            return ReadMultipleBitsAsUInt((void*)ptr, bitOffset, bitCount);
        }

        /// <inheritdoc cref="WriteUIntAsMultipleBits"/>
        public static void WriteUIntAsMultipleBits(IntPtr ptr, uint bitOffset, uint bitCount, uint value)
        {
            WriteUIntAsMultipleBits((void*)ptr, bitOffset, bitCount, value);
        }

        /// <inheritdoc cref="ReadTwosComplementMultipleBitsAsInt"/>
        public static int ReadTwosComplementMultipleBitsAsInt(IntPtr ptr, uint bitOffset, uint bitCount)
        {
            return ReadTwosComplementMultipleBitsAsInt((void*)ptr, bitOffset, bitCount);
        }

        /// <inheritdoc cref="WriteIntAsTwosComplementMultipleBits"/>
        public static void WriteIntAsTwosComplementMultipleBits(IntPtr ptr, uint bitOffset, uint bitCount, int value)
        {
            WriteIntAsTwosComplementMultipleBits((void*)ptr, bitOffset, bitCount, value);
        }

        /// <inheritdoc cref="ReadExcessKMultipleBitsAsInt"/>
        public static int ReadExcessKMultipleBitsAsInt(IntPtr ptr, uint bitOffset, uint bitCount)
        {
            return ReadExcessKMultipleBitsAsInt((void*)ptr, bitOffset, bitCount);
        }

        /// <inheritdoc cref="WriteIntAsExcessKMultipleBits"/>
        public static void WriteIntAsExcessKMultipleBits(IntPtr ptr, uint bitOffset, uint bitCount, int value)
        {
            WriteIntAsExcessKMultipleBits((void*)ptr, bitOffset, bitCount, value);
        }

        /// <inheritdoc cref="ReadMultipleBitsAsNormalizedUInt"/>
        public static float ReadMultipleBitsAsNormalizedUInt(IntPtr ptr, uint bitOffset, uint bitCount)
        {
            return ReadMultipleBitsAsNormalizedUInt((void*)ptr, bitOffset, bitCount);
        }

        /// <inheritdoc cref="WriteNormalizedUIntAsMultipleBits"/>
        public static void WriteNormalizedUIntAsMultipleBits(IntPtr ptr, uint bitOffset, uint bitCount, float value)
        {
            WriteNormalizedUIntAsMultipleBits((void*)ptr, bitOffset, bitCount, value);
        }

        public static void SetBitsInBuffer(IntPtr buffer, int byteOffset, int bitOffset, int sizeInBits, bool value)
        {
            SetBitsInBuffer((void*)buffer, byteOffset, bitOffset, sizeInBits, value);
        }
        #endregion // UnityEngine.InputSystem.Utilities
    }
}
#endif // INCLUDE_COLLECTIONS
#endif // PKGE_USING_INTPTR
