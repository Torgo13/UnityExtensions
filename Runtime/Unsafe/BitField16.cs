using System;
using System.Diagnostics;
using Unity.Mathematics;

namespace Unity.Collections
{
    [GenerateTestsForBurstCompatibility]
    internal unsafe struct Bitwise
    {
        //https://github.com/needle-mirror/com.unity.collections/blob/feee1d82af454e1023e3e04789fce4d30fc1d938/Unity.Collections/BitField.cs
        #region Unity.Collections
        internal static int AlignDown(int value, int alignPow2)
        {
            return value & ~(alignPow2 - 1);
        }

        internal static int AlignUp(int value, int alignPow2)
        {
            return AlignDown(value + alignPow2 - 1, alignPow2);
        }

        internal static int FromBool(bool value)
        {
            return value ? 1 : 0;
        }

        // 16-bit ushort

        internal static ushort ExtractBits(ushort input, int pos, ushort mask)
        {
            var tmp0 = input >> pos;
            return (ushort)(tmp0 & mask);
        }

        internal static ushort ReplaceBits(ushort input, int pos, ushort mask, ushort value)
        {
            var tmp0 = (value & mask) << pos;
            var tmp1 = input & ~(mask << pos);
            return (ushort)(tmp0 | tmp1);
        }

        internal static ushort SetBits(ushort input, int pos, ushort mask, bool value)
        {
            return ReplaceBits(input, pos, mask, (ushort)-FromBool(value));
        }

        internal static int lzcnt(byte value)
        {
            return math.lzcnt((uint)value) - 24;
        }

        internal static int tzcnt(byte value)
        {
            return math.min(8, math.tzcnt((uint)value));
        }

        internal static int lzcnt(ushort value)
        {
            return math.lzcnt((uint)value) - 16;
        }

        internal static int tzcnt(ushort value)
        {
            return math.min(16, math.tzcnt((uint)value));
        }

        static int FindUshort(ulong* ptr, int beginBit, int endBit, int numBits)
        {
            var bits = (ushort*)ptr;
            var numSteps = (numBits + 15) >> 4;
            var numBitsPerStep = 16;
            var maxBits = numSteps * numBitsPerStep;

            for (int i = beginBit / numBitsPerStep, end = AlignUp(endBit, numBitsPerStep) / numBitsPerStep; i < end; ++i)
            {
                if (bits[i] != 0)
                {
                    continue;
                }

                var idx = i * numBitsPerStep;
                var num = math.min(idx + numBitsPerStep, endBit) - idx;

                if (idx != beginBit)
                {
                    var test = bits[idx / numBitsPerStep - 1];
                    var newIdx = math.max(idx - lzcnt(test), beginBit);

                    num += idx - newIdx;
                    idx = newIdx;
                }

                for (++i; i < end; ++i)
                {
                    if (num >= numBits)
                    {
                        return idx;
                    }

                    var test = bits[i];
                    var pos = i * numBitsPerStep;
                    num += math.min(pos + tzcnt(test), endBit) - pos;

                    if (test != 0)
                    {
                        break;
                    }
                }

                if (num >= numBits)
                {
                    return idx;
                }
            }

            return endBit;
        }

        static int FindByte(ulong* ptr, int beginBit, int endBit, int numBits)
        {
            var bits = (byte*)ptr;
            var numSteps = (numBits + 7) >> 3;
            var numBitsPerStep = 8;
            var maxBits = numSteps * numBitsPerStep;

            for (int i = beginBit / numBitsPerStep, end = AlignUp(endBit, numBitsPerStep) / numBitsPerStep; i < end; ++i)
            {
                if (bits[i] != 0)
                {
                    continue;
                }

                var idx = i * numBitsPerStep;
                var num = math.min(idx + numBitsPerStep, endBit) - idx;

                if (idx != beginBit)
                {
                    var test = bits[idx / numBitsPerStep - 1];
                    var newIdx = math.max(idx - lzcnt(test), beginBit);

                    num += idx - newIdx;
                    idx = newIdx;
                }

                for (++i; i < end; ++i)
                {
                    if (num >= numBits)
                    {
                        return idx;
                    }

                    var test = bits[i];
                    var pos = i * numBitsPerStep;
                    num += math.min(pos + tzcnt(test), endBit) - pos;

                    if (test != 0)
                    {
                        break;
                    }
                }

                if (num >= numBits)
                {
                    return idx;
                }
            }

            return endBit;
        }

        static int FindUpto14bits(ulong* ptr, int beginBit, int endBit, int numBits)
        {
            var bits = (byte*)ptr;

            var bit = (byte)(beginBit & 7);
            byte beginMask = (byte)~(0xff << bit);

            var lz = 0;
            for (int begin = beginBit / 8, end = AlignUp(endBit, 8) / 8, i = begin; i < end; ++i)
            {
                var test = bits[i];
                test |= i == begin ? beginMask : (byte)0;

                if (test == 0xff)
                {
                    continue;
                }

                var pos = i * 8;
                var tz = math.min(pos + tzcnt(test), endBit) - pos;

                if (lz + tz >= numBits)
                {
                    return pos - lz;
                }

                lz = lzcnt(test);

                var idx = pos + 8;
                var newIdx = math.max(idx - lz, beginBit);
                lz = math.min(idx, endBit) - newIdx;

                if (lz >= numBits)
                {
                    return newIdx;
                }
            }

            return endBit;
        }

        static int FindUpto6bits(ulong* ptr, int beginBit, int endBit, int numBits)
        {
            var bits = (byte*)ptr;

            byte beginMask = (byte)~(0xff << (beginBit & 7));
            byte endMask = (byte)~(0xff >> ((8 - (endBit & 7) & 7)));

            var mask = 1 << numBits - 1;

            for (int begin = beginBit / 8, end = AlignUp(endBit, 8) / 8, i = begin; i < end; ++i)
            {
                var test = bits[i];
                test |= i == begin ? beginMask : (byte)0;
                test |= i == end - 1 ? endMask : (byte)0;

                if (test == 0xff)
                {
                    continue;
                }

                for (int pos = i * 8, posEnd = pos + 7; pos < posEnd; ++pos)
                {
                    var tz = tzcnt((byte)(test ^ 0xff));
                    test >>= tz;

                    pos += tz;

                    if ((test & mask) == 0)
                    {
                        return pos;
                    }

                    test >>= 1;
                }
            }

            return endBit;
        }
        #endregion // Unity.Collections
    }

    /// <summary>
    /// A 16-bit array of bits.
    /// </summary>
    /// <remarks>
    /// Stack allocated, so it does not require thread safety checks or disposal.
    /// </remarks>
    [DebuggerTypeProxy(typeof(BitField16DebugView))]
    [GenerateTestsForBurstCompatibility]
    public struct BitField16
    {
        //https://github.com/needle-mirror/com.unity.collections/blob/feee1d82af454e1023e3e04789fce4d30fc1d938/Unity.Collections/BitField.cs
        #region Unity.Collections
        /// <summary>
        /// The 16 bits, stored as a ushort.
        /// </summary>
        /// <value>The 16 bits, stored as a ushort.</value>
        public ushort Value;

        /// <summary>
        /// Initializes and returns an instance of BitField16.
        /// </summary>
        /// <param name="initialValue">Initial value of the bit field. Default is 0.</param>
        public BitField16(ushort initialValue = 0)
        {
            Value = initialValue;
        }

        /// <summary>
        /// Clears all the bits to 0.
        /// </summary>
        public void Clear()
        {
            Value = 0;
        }

        /// <summary>
        /// Sets a single bit to 1 or 0.
        /// </summary>
        /// <param name="pos">Position in this bit field to set (must be 0-15).</param>
        /// <param name="value">If true, sets the bit to 1. If false, sets the bit to 0.</param>
        /// <exception cref="ArgumentException">Thrown if `pos` is out of range.</exception>
        public void SetBits(int pos, bool value)
        {
            CheckArgs(pos, 1);
            Value = Bitwise.SetBits(Value, pos, 1, value);
        }

        /// <summary>
        /// Sets one or more contiguous bits to 1 or 0.
        /// </summary>
        /// <param name="pos">Position in the bit field of the first bit to set (must be 0-15).</param>
        /// <param name="value">If true, sets the bits to 1. If false, sets the bits to 0.</param>
        /// <param name="numBits">Number of bits to set (must be 1-16).</param>
        /// <exception cref="ArgumentException">Thrown if `pos` or `numBits` are out of bounds or if `pos + numBits` exceeds 16.</exception>
        public void SetBits(int pos, bool value, int numBits)
        {
            CheckArgs(pos, numBits);
            var mask = (ushort)(0xffffu >> (16 - numBits));
            Value = Bitwise.SetBits(Value, pos, mask, value);
        }

        /// <summary>
        /// Returns one or more contiguous bits from the bit field as the lower bits of a ushort.
        /// </summary>
        /// <param name="pos">Position in the bit field of the first bit to get (must be 0-15).</param>
        /// <param name="numBits">Number of bits to get (must be 1-16).</param>
        /// <exception cref="ArgumentException">Thrown if `pos` or `numBits` are out of bounds or if `pos + numBits` exceeds 16.</exception>
        /// <returns>The requested range of bits from the bit field stored in the least-significant bits of a ushort. All other bits of the ushort will be 0.</returns>
        public ushort GetBits(int pos, int numBits = 1)
        {
            CheckArgs(pos, numBits);
            var mask = (ushort)(0xffffu >> (16 - numBits));
            return Bitwise.ExtractBits(Value, pos, mask);
        }

        /// <summary>
        /// Returns true if the bit at a position is 1.
        /// </summary>
        /// <param name="pos">Position in the bit field (must be 0-15).</param>
        /// <returns>True if the bit at the position is 1.</returns>
        public bool IsSet(int pos)
        {
            return 0 != GetBits(pos);
        }

        /// <summary>
        /// Returns true if none of the bits in a contiguous range are 1.
        /// </summary>
        /// <param name="pos">Position in the bit field (must be 0-15).</param>
        /// <param name="numBits">Number of bits to test (must be 1-16).</param>
        /// <exception cref="ArgumentException">Thrown if `pos` or `numBits` are out of bounds or if `pos + numBits` exceeds 16.</exception>
        /// <returns>True if none of the bits in the contiguous range are 1.</returns>
        public bool TestNone(int pos, int numBits = 1)
        {
            return 0 == GetBits(pos, numBits);
        }

        /// <summary>
        /// Returns true if any of the bits in a contiguous range are 1.
        /// </summary>
        /// <param name="pos">Position in the bit field (must be 0-15).</param>
        /// <param name="numBits">Number of bits to test (must be 1-16).</param>
        /// <exception cref="ArgumentException">Thrown if `pos` or `numBits` are out of bounds or if `pos + numBits` exceeds 16.</exception>
        /// <returns>True if at least one bit in the contiguous range is 1.</returns>
        public bool TestAny(int pos, int numBits = 1)
        {
            return 0 != GetBits(pos, numBits);
        }

        /// <summary>
        /// Returns true if all of the bits in a contiguous range are 1.
        /// </summary>
        /// <param name="pos">Position in the bit field (must be 0-15).</param>
        /// <param name="numBits">Number of bits to test (must be 1-16).</param>
        /// <exception cref="ArgumentException">Thrown if `pos` or `numBits` are out of bounds or if `pos + numBits` exceeds 16.</exception>
        /// <returns>True if all bits in the contiguous range are 1.</returns>
        public bool TestAll(int pos, int numBits = 1)
        {
            CheckArgs(pos, numBits);
            var mask = (ushort)(0xffffu >> (16 - numBits));
            return mask == Bitwise.ExtractBits(Value, pos, mask);
        }

        /// <summary>
        /// Returns the number of bits that are 1.
        /// </summary>
        /// <returns>The number of bits that are 1.</returns>
        public int CountBits()
        {
            return math.countbits((int)Value);
        }

        /// <summary>
        /// Returns the number of leading zeroes.
        /// </summary>
        /// <returns>The number of leading zeros.</returns>
        public int CountLeadingZeros()
        {
            return math.clamp(math.lzcnt(Value << 16), 0, 16);
        }

        /// <summary>
        /// Returns the number of trailing zeros.
        /// </summary>
        /// <returns>The number of trailing zeros.</returns>
        public int CountTrailingZeros()
        {
            return math.clamp(math.tzcnt((int)Value), 0, 16);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS"), Conditional("UNITY_DOTS_DEBUG")]
        static void CheckArgs(int pos, int numBits)
        {
            if (pos > 15
                || numBits == 0
                || numBits > 16
                || pos + numBits > 16)
            {
                throw new ArgumentException($"BitField16 invalid arguments: pos {pos} (must be 0-15), numBits {numBits} (must be 1-16).");
            }
        }
        #endregion // Unity.Collections
    }

    sealed class BitField16DebugView
    {
        //https://github.com/needle-mirror/com.unity.collections/blob/feee1d82af454e1023e3e04789fce4d30fc1d938/Unity.Collections/BitField.cs
        #region Unity.Collections
        BitField16 BitField;

        public BitField16DebugView(BitField16 bitfield)
        {
            BitField = bitfield;
        }

        public bool[] Bits
        {
            get
            {
                var array = new bool[16];
                for (int i = 0; i < 16; ++i)
                {
                    array[i] = BitField.IsSet(i);
                }
                return array;
            }
        }
        #endregion // Unity.Collections
    }
}
