using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityExtensions
{
    //https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/3417c4765f52f72d2384f2f7e65bd9d2d1dfd7ac/com.unity.netcode.gameobjects/Runtime/Serialization/MemoryStructures/UIntFloat.cs
    #region Unity.Netcode
    /// <summary>
    /// A struct with an explicit memory layout. The struct has 4 fields; float, uint, double and ulong.
    /// Every field has the same starting point in memory. If you insert a float value, it can be extracted as a uint.
    /// This is to allow for lockless and garbage free conversion from float to uint and double to ulong.
    /// This allows for VarInt encoding and other integer encodings.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct UIntFloat
    {
        [FieldOffset(0)] public float FloatValue;
        [FieldOffset(0)] public uint UIntValue;
        [FieldOffset(0)] public double DoubleValue;
        [FieldOffset(0)] public ulong ULongValue;
    }
    #endregion // Unity.Netcode
    
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

    [StructLayout(LayoutKind.Explicit)]
    public struct Union16
    {
        [FieldOffset(0)] public short Short_0;

        [FieldOffset(0)] public ushort UShort_0;

        [FieldOffset(0)] public char Char_0;


        [FieldOffset(0)] public byte Byte_0;
        [FieldOffset(1)] public byte Byte_1;
    }
    
    [StructLayout(LayoutKind.Explicit)]
    public struct Union32
    {
        [FieldOffset(0)] public float Float_0;
        
        [FieldOffset(0)] public int Int_0;

        [FieldOffset(0)] public uint UInt_0;
        
        [FieldOffset(0)] public Color32 Color32_0;
        

        [FieldOffset(0)] public Union16 Union16_0;
        [FieldOffset(2)] public Union16 Union16_1;

        [FieldOffset(0)] public short Short_0;
        [FieldOffset(2)] public short Short_1;

        [FieldOffset(0)] public ushort UShort_0;
        [FieldOffset(2)] public ushort UShort_1;

        [FieldOffset(0)] public char Char_0;
        [FieldOffset(2)] public char Char_1;


        [FieldOffset(0)] public byte Byte_0;
        [FieldOffset(1)] public byte Byte_1;
        [FieldOffset(2)] public byte Byte_2;
        [FieldOffset(3)] public byte Byte_3;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Union128
    {
        [FieldOffset(0)] public Vector4 Vector4_0;
        
        [FieldOffset(0)] public Color Color_0;

        [FieldOffset(0)] public Vector3 Vector3_0;

        [FieldOffset(0)] public Vector3Int Vector3Int_0;
        

        [FieldOffset(0)] public Vector2 Vector2_0;
        [FieldOffset(8)] public Vector2 Vector2_1;

        [FieldOffset(0)] public double Double_0;
        [FieldOffset(8)] public double Double_1;

        [FieldOffset(0)] public long Long_0;
        [FieldOffset(8)] public long Long_1;

        [FieldOffset(0)] public ulong ULong_0;
        [FieldOffset(8)] public ulong ULong_1;
        

        [FieldOffset(0)] public Union32 Union32_0;
        [FieldOffset(4)] public Union32 Union32_1;
        [FieldOffset(8)] public Union32 Union32_2;
        [FieldOffset(12)] public Union32 Union32_3;

        [FieldOffset(0)] public float Float_0;
        [FieldOffset(4)] public float Float_1;
        [FieldOffset(8)] public float Float_2;
        [FieldOffset(12)] public float Float_3;

        [FieldOffset(0)] public int Int_0;
        [FieldOffset(4)] public int Int_1;
        [FieldOffset(8)] public int Int_2;
        [FieldOffset(12)] public int Int_3;

        [FieldOffset(0)] public uint UInt_0;
        [FieldOffset(4)] public uint UInt_1;
        [FieldOffset(8)] public uint UInt_2;
        [FieldOffset(12)] public uint UInt_3;
    }
    
    public static class MathematicsExtensions
    {
        public static (int, int) CalculateLength(this int inputLength, int start = 0, int length = 0)
        {
            start = Mathf.Clamp(start, 0, inputLength - 1);
            int maxLength = inputLength - start;
            if (length <= 0 || length > maxLength)
                length = maxLength;

            return (start, length);
        }
        
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
    }
}
