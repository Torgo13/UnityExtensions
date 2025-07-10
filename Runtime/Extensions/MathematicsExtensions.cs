using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityExtensions
{
    //https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/3417c4765f52f72d2384f2f7e65bd9d2d1dfd7ac/com.unity.netcode.gameobjects/Runtime/Serialization/MemoryStructures/ByteBool.cs
    #region Unity.Netcode
    /// <summary>
    /// A struct with an explicit memory layout.
    /// </summary>
    /// <remarks>
    /// Proper usage of this struct is:
    /// <code>
    /// private byte ReadByteBits(int bitCount)
    /// {
    ///     if (bitCount &gt; 8)
    ///         throw new ArgumentOutOfRangeException(nameof(bitCount), "Cannot read more than 8 bits into an 8-bit value!");
    ///
    ///     if (bitCount &lt; 0)
    ///         throw new ArgumentOutOfRangeException(nameof(bitCount), "Cannot read fewer than 0 bits!");
    ///
    ///     int result = 0;
    ///     var convert = new ByteBool();
    ///     for (int i = 0; i &lt; bitCount; ++i)
    ///     {
    ///         ReadBit(out bool bit);
    ///         result |= convert.Collapse(bit) &lt;&lt; i;
    ///     }
    ///
    ///     return (byte)result;
    /// }
    /// </code>
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    public struct ByteBool
    {
        [FieldOffset(0)] public bool BoolValue;
        [FieldOffset(0)] public byte ByteValue;

        public byte Collapse() =>
            ByteValue = (byte)((
                // Collapse all bits to position 1 and reassign as bit
                (ByteValue >> 7) |
                (ByteValue >> 6) |
                (ByteValue >> 5) |
                (ByteValue >> 4) |
                (ByteValue >> 3) |
                (ByteValue >> 2) |
                (ByteValue >> 1) |
                ByteValue
            ) & 1);

        public byte Collapse(bool b)
        {
            BoolValue = b;
            return Collapse();
        }
    }
    #endregion // Unity.Netcode

    #region Union
    /// <summary>
    /// <see href="https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/3417c4765f52f72d2384f2f7e65bd9d2d1dfd7ac/com.unity.netcode.gameobjects/Runtime/Serialization/MemoryStructures/UIntFloat.cs"/>
    /// A struct with an explicit memory layout.
    /// Every field has the same starting point in memory. If you insert a float value, it can be extracted as a uint.
    /// This is to allow for lockless and garbage free conversion from float to uint and double to ulong.
    /// This allows for VarInt encoding and other integer encodings.
    /// </summary>
    /// <remarks>
    /// <see cref="System.Runtime.InteropServices.Marshal.SizeOf{T}()"/> will return at least 4 bytes.
    /// Use <see cref="Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf{T}()"/> to return the actual size.
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    public struct Union1 : System.IEquatable<Union1>
    {
        [FieldOffset(0)] public byte Byte;
        [FieldOffset(0)] public sbyte SByte;
        [FieldOffset(0)] public bool Bool;

        public readonly bool Equals(Union1 other) => Byte == other.Byte;
        public readonly override bool Equals(object obj) => obj is Union1 other && Equals(other);
        public readonly override int GetHashCode() => Byte;
    }

    /// <inheritdoc cref="Union1"/>
    [StructLayout(LayoutKind.Explicit)]
    public struct Union2 : System.IEquatable<Union2>
    {
        [FieldOffset(0)] public short Short;
        [FieldOffset(0)] public ushort UShort;
        [FieldOffset(0)] public char Char;

        [FieldOffset(0)] public Union1 _0;
        [FieldOffset(1)] public Union1 _1;

        public readonly bool Equals(Union2 other) => Short == other.Short;
        public readonly override bool Equals(object obj) => obj is Union2 other && Equals(other);
        public readonly override int GetHashCode() => Short;
    }

    /// <inheritdoc cref="Union1"/>
    [StructLayout(LayoutKind.Explicit)]
    public struct Union4 : System.IEquatable<Union4>
    {
        [FieldOffset(0)] public float Float;        
        [FieldOffset(0)] public int Int;
        [FieldOffset(0)] public uint UInt;
        [FieldOffset(0)] public Color32 Color32;
        
        [FieldOffset(0)] public Union2 _0;
        [FieldOffset(2)] public Union2 _2;

        public readonly bool Equals(Union4 other) => Int == other.Int;
        public readonly override bool Equals(object obj) => obj is Union4 other && Equals(other);
        public readonly override int GetHashCode() => Int;
    }

    /// <inheritdoc cref="Union1"/>
    [StructLayout(LayoutKind.Explicit)]
    public struct Union8 : System.IEquatable<Union8>
    {
        [FieldOffset(0)] public Vector2 Vector2;
        [FieldOffset(0)] public Vector2Int Vector2Int;
        [FieldOffset(0)] public double Double;
        [FieldOffset(0)] public long Long;
        [FieldOffset(0)] public ulong ULong;
        [FieldOffset(0)] public GradientAlphaKey GradientAlphaKey;
        [FieldOffset(0)] public RangeInt RangeInt;

        [FieldOffset(0)] public Union4 _0;
        [FieldOffset(4)] public Union4 _4;

        public readonly bool Equals(Union8 other) => Long == other.Long;
        public readonly override bool Equals(object obj) => obj is Union8 other && Equals(other);
        public readonly override int GetHashCode() => Long.GetHashCode();
    }

    /// <inheritdoc cref="Union1"/>
    [StructLayout(LayoutKind.Explicit)]
    public struct Union16 : System.IEquatable<Union16>
    {
        [FieldOffset(0)] public Vector4 Vector4;
        [FieldOffset(0)] public Quaternion Quaternion;
        [FieldOffset(0)] public Color Color;
        [FieldOffset(0)] public Rect Rect;
        [FieldOffset(0)] public Plane Plane;
        [FieldOffset(0)] public Random.State State;

        [FieldOffset(0)] public Vector3 Vector3;
        [FieldOffset(0)] public Vector3Int Vector3Int;

        [FieldOffset(0)] public Union8 _0;
        [FieldOffset(8)] public Union8 _8;

        public readonly bool Equals(Union16 other) => _0.Long == other._0.Long && _8.Long == other._8.Long;
        public readonly override bool Equals(object obj) => obj is Union16 other && Equals(other);
        public readonly override int GetHashCode() => _0.Long.GetHashCode() ^ (_8.Long.GetHashCode() << 2);
    }
    #endregion // Union

    public static class MathematicsExtensions
    {
        public static (int, int) CalculateLength(this int inputLength, int start = 0, int length = 0)
        {
            start = System.Math.Clamp(start, 0, inputLength - 1);
            int maxLength = inputLength - start;
            if (length <= 0 || length > maxLength)
                length = maxLength;

            return (start, length);
        }

        #region SafeRange
        /// <summary>
        /// Check if the value is within the range that it can
        /// be safely cast from signed to unsigned or vice versa.
        /// </summary>
        public static bool SafeRange(this byte Byte)
        {
            return Byte <= sbyte.MaxValue;
        }

        /// <inheritdoc cref="SafeRange"/>
        public static bool SafeRange(this sbyte SByte)
        {
            return SByte >= 0;
        }

        /// <inheritdoc cref="SafeRange"/>
        public static bool SafeRange(this ushort UShort)
        {
            return UShort <= short.MaxValue;
        }

        /// <inheritdoc cref="SafeRange"/>
        public static bool SafeRange(this short Short)
        {
            return Short >= 0;
        }

        /// <inheritdoc cref="SafeRange"/>
        public static bool SafeRange(this uint UInt)
        {
            return UInt <= int.MaxValue;
        }

        /// <inheritdoc cref="SafeRange"/>
        public static bool SafeRange(this int Int)
        {
            return Int >= 0;
        }

        /// <inheritdoc cref="SafeRange"/>
        public static bool SafeRange(this ulong ULong)
        {
            return ULong <= long.MaxValue;
        }

        /// <inheritdoc cref="SafeRange"/>
        public static bool SafeRange(this long Long)
        {
            return Long >= 0;
        }

        /// <inheritdoc cref="SafeRange"/>
        public static bool SafeRange(this Union1 Union1)
        {
            return Union1.SByte >= 0;
        }

        /// <inheritdoc cref="SafeRange"/>
        public static bool SafeRange(this Union2 Union2)
        {
            return Union2.Short >= 0;
        }

        /// <inheritdoc cref="SafeRange"/>
        public static bool SafeRange(this Union4 Union4)
        {
            return Union4.Int >= 0;
        }

        /// <inheritdoc cref="SafeRange"/>
        public static bool SafeRange(this Union8 Union8)
        {
            return Union8.Long >= 0;
        }
        #endregion // SafeRange

        /// <summary>
        /// Creates a translation, rotation and scaling matrix from just the translation.
        /// </summary>
        /// <param name="pos">Translation vector.</param>
        /// <returns>Matrix4x4 with only translation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 TRS(this Vector3 pos)
        {
            return Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);
        }

        /// <summary>
        /// Creates a translation, rotation and scaling matrix from just the rotation.
        /// </summary>
        /// <param name="q">Rotation quaternion.</param>
        /// <returns>Matrix4x4 with only rotation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 TRS(this Quaternion q)
        {
            return Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
        }
        
        public static void ConvertToHex(this uint number, System.Span<char> buffer, bool padZeroes = false)
        {
            const string hexChars = "0123456789ABCDEF";
            int index = buffer.Length - 1;

            InitialiseBuffer(buffer, padZeroes);

            do
            {
                buffer[index--] = hexChars[(int)(number & 0xF)];
                number >>= 4;
            } while (number != 0 && index >= 0);
        }
        
        public static void ConvertToHex(this byte number, System.Span<char> buffer, bool padZeroes = false)
        {
            const string hexChars = "0123456789ABCDEF";
            int index = buffer.Length - 1;
            
            InitialiseBuffer(buffer, padZeroes);

            do
            {
                buffer[index--] = hexChars[number & 0xF];
                number >>= 4;
            } while (number != 0 && index >= 0);
        }
        
        static void InitialiseBuffer(System.Span<char> buffer, bool padZeroes = false)
        {
            char c = padZeroes ? '0' : '\0';
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = c;
            }
        }
    }
}
