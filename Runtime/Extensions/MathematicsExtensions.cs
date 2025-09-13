using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

#if PACKAGE_MATHEMATICS
#else
using float2 = UnityEngine.Vector2;
using float3 = UnityEngine.Vector3;
using float4 = UnityEngine.Vector4;
using quaternion = UnityEngine.Quaternion;
#endif // PACKAGE_MATHEMATICS

namespace PKGE
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

        [FieldOffset(0)] public ByteBool ByteBool;

        [FieldOffset(0)] public BitArray8 BitArray8;

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

        [FieldOffset(0)] public BitArray16 BitArray16;

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

        [FieldOffset(0)] public Color24 Color24;

        [FieldOffset(0)] public BitArray32 BitArray32;

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
        [FieldOffset(0)] public double Double;
        [FieldOffset(0)] public long Long;
        [FieldOffset(0)] public ulong ULong;
        [FieldOffset(0)] public GradientAlphaKey GradientAlphaKey;
        [FieldOffset(0)] public RangeInt RangeInt;
        [FieldOffset(0)] public Vector2 Vector2;
        [FieldOffset(0)] public Vector2Int Vector2Int;

        [FieldOffset(0)] public BitArray64 BitArray64;

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
        [FieldOffset(0)] public Quaternion Quaternion;
        [FieldOffset(0)] public Color Color;
        [FieldOffset(0)] public Rect Rect;
        [FieldOffset(0)] public RectInt RectInt;
        [FieldOffset(0)] public Plane Plane;
        [FieldOffset(0)] public Random.State State;
        [FieldOffset(0)] public Vector4 Vector4;

        [FieldOffset(0)] public Vector3 Vector3;
        [FieldOffset(0)] public Vector3Int Vector3Int;

        [FieldOffset(0)] public BitArray128 BitArray128;

        [FieldOffset(0)] public Union8 _0;
        [FieldOffset(8)] public Union8 _8;

        public readonly bool Equals(Union16 other) => _0.Long == other._0.Long && _8.Long == other._8.Long;
        public readonly override bool Equals(object obj) => obj is Union16 other && Equals(other);
        public readonly override int GetHashCode() => _0.Long.GetHashCode() ^ (_8.Long.GetHashCode() << 2);
    }

    /// <inheritdoc cref="Union1"/>
    [StructLayout(LayoutKind.Explicit)]
    public struct Union12 : System.IEquatable<Union12>
    {
        [FieldOffset(0)] public Vector3 Vector3;
        [FieldOffset(0)] public Vector3Int Vector3Int;

        [FieldOffset(0)] public Union4 Union4_0;
        [FieldOffset(4)] public Union4 Union4_4;
        [FieldOffset(8)] public Union4 Union4_8;

        public readonly bool Equals(Union12 other) => Vector3Int == other.Vector3Int;
        public readonly override bool Equals(object obj) => obj is Union12 other && Equals(other);
        public readonly override int GetHashCode() => Vector3Int.GetHashCode();
    }

    /// <inheritdoc cref="Union1"/>
    [StructLayout(LayoutKind.Explicit)]
    public struct Union48 : System.IEquatable<Union48>
    {
        [FieldOffset(00)] public Union16 U16_00;
        [FieldOffset(16)] public Union16 U16_16;
        [FieldOffset(32)] public Union16 U16_32;

        [FieldOffset(00)] public Union12 U12_00;
        [FieldOffset(12)] public Union12 U12_12;
        [FieldOffset(24)] public Union12 U12_24;
        [FieldOffset(36)] public Union12 U12_36;

        public readonly bool Equals(Union48 other) => U16_00.Equals(other.U16_00) && U16_16.Equals(other.U16_16) && U16_32.Equals(other.U16_32);
        public readonly override bool Equals(object obj) => obj is Union48 other && Equals(other);
        public readonly override int GetHashCode() => U16_00.GetHashCode() ^ (U16_16.GetHashCode() << 2) ^ (U16_32.GetHashCode() >> 2);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Remap(this float value, float from0, float to0, float from1 = 0f, float to1 = 1f)
        {
            return from1 + (value - from0) * (to1 - from1) / (to0 - from0);
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

#if PACKAGE_MATHEMATICS
#else
#pragma warning disable IDE1006 // Naming Styles
    public static class math
    {
        public static readonly float SQRT2 = (float)System.Math.Sqrt(2);
        public const float FLT_MIN_NORMAL = 1.175494351e-38F;
        public const float PI = (float)System.Math.PI;
        public const float PI2 = (float)(System.Math.PI * 2);

        public static uint ceilpow2(uint x)
        {
            x -= 1;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return x + 1;
        }

        public static int max(int a, int b) => System.Math.Max(a, b);
        public static uint max(uint a, uint b) => System.Math.Max(a, b);
        public static uint max(uint a, int b) => System.Math.Max(a, (uint)b);
        public static float max(float a, float b) => System.Math.Max(a, b);

        public static int min(int a, int b) => System.Math.Min(a, b);
        public static uint min(uint a, uint b) => System.Math.Min(a, b);
        public static uint min(uint a, int b) => System.Math.Min(a, (uint)b);
        public static float min(float a, float b) => System.Math.Min(a, b);

        public static int mad(int a, int b, int c) => a * b + c;
        public static float mad(float a, float b, float c) => a * b + c;
        public static float3 mad(float3 a, float b, float c) => (a * b) + new float3(c, c, c);
        public static float4 mad(float4 a, float4 b, float4 c) => float4.Scale(a, b) + c;

        public static float ceil(float a) => (float)System.Math.Ceiling(a);
        public static float floor(float a) => (float)System.Math.Floor(a);

        public static int clamp(int a, int b, int c) => System.Math.Clamp(a, b, c);
        public static float clamp(float a, float b, float c) => System.Math.Clamp(a, b, c);

        public static float log(float a) => (float)System.Math.Log(a);
        public static float exp(float a) => (float)System.Math.Exp(a);

        public static float sqrt(float a) => (float)System.Math.Sqrt(a);
        public static float rsqrt(float a) => 1f / sqrt(a);

        public static float lengthsq(float2 a) => a.x * a.x + a.y * a.y;
        public static float lengthsq(float3 a) => a.x * a.x + a.y * a.y + a.z * a.z;

        public static float sign(float x) => (x > 0.0f ? 1.0f : 0.0f) - (x < 0.0f ? 1.0f : 0.0f);

        public static float abs(float a) => System.Math.Abs(a);
        public static float3 abs(float3 a) => new float3(System.Math.Abs(a.x), System.Math.Abs(a.y), System.Math.Abs(a.z));
        public static float4 abs(float4 a) => new float4(
            System.Math.Abs(a.x),
            System.Math.Abs(a.y),
            System.Math.Abs(a.z),
            System.Math.Abs(a.w));

        public static float3 normalize(float3 a) => a.normalized;
        public static float dot(float2 a, float2 b) => float2.Dot(a, b);
        public static float dot(float3 a, float3 b) => float3.Dot(a, b);
        public static float rcp(int a) => 1f / a;
        public static float distance(float3 a, float3 b) => float3.Distance(a, b);
        public static float distancesq(float2 a, float2 b)
        {
            float num = a.x - b.x;
            float num2 = a.y - b.y;
            return num * num + num2 * num2;
        }

        public static float acos(float a) => (float)System.Math.Acos(a);
        public static float atan2(float y, float x) => (float)System.Math.Atan2(y, x);
        public static float sin(float a) => (float)System.Math.Sin(a);
        public static float cos(float a) => (float)System.Math.Cos(a);
        public static float tan(float a) => (float)System.Math.Tan(a);
        public static void sincos(float a, out float s, out float c) { s = sin(a); c = cos(a); }
        public static float radians(float a) => UnityEngine.Mathf.Deg2Rad * a;
        public static float degrees(float a) => UnityEngine.Mathf.Rad2Deg * a;

        public static float saturate(float a) => System.Math.Clamp(a, 0, 1);

        public static float2 normalizesafe(float2 x, float2 defaultvalue = new float2())
        {
            float len = math.dot(x, x);
            return math.select(defaultvalue, x * math.rsqrt(len), len > FLT_MIN_NORMAL);
        }

        public static float3 normalizesafe(float3 x, float3 defaultvalue = new float3())
        {
            float len = math.dot(x, x);
            return math.select(defaultvalue, x * math.rsqrt(len), len > FLT_MIN_NORMAL);
        }

        public static float3 select(float3 falseValue, float3 trueValue, bool test) { return test ? trueValue : falseValue; }

        public static float3 cross(float3 a, float3 b) => float3.Cross(a, b);
        public static float3 cross(quaternion a, float3 b) => float3.Cross(new float3(a.x, a.y, a.z), b);

        public static float3 rotate(quaternion q, float3 v)
        {
            float3 t = 2 * cross(q, v);
            return v + q.w * t + cross(q, t);
        }

        public static float4 mul(float4 a, quaternion b) => new float4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
        public static float4 mul(float4 a, float4 b) => new float4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);

        public static quaternion mul(quaternion a, quaternion b)
        {
            float4 t = mul(new float4(a.w, a.w, a.w, a.w), b)
                + mul(mul(new float4(a.x, a.y, a.z, a.x), new float4(b.w, b.w, b.w, b.x))
                + mul(new float4(a.y, a.z, a.x, a.y), new float4(b.z, b.x, b.y, b.y)), new float4(1.0f, 1.0f, 1.0f, -1.0f))
                - mul(new float4(a.z, a.x, a.y, a.z), new float4(b.y, b.z, b.x, b.z));

            return new quaternion(t.x, t.y, t.z, t.w);
        }

        public static float lerp(float start, float end, float t) => start + t * (end - start);
        public static float3 lerp(float3 start, float3 end, float t) => float3.Lerp(start, end, t);
        public static float unlerp(float start, float end, float x) => (x - start) / (end - start);
        public static float remap(float srcStart, float srcEnd, float dstStart, float dstEnd, float x)
            => lerp(dstStart, dstEnd, unlerp(srcStart, srcEnd, x));

        public static int countbits(int x) => countbits((uint)x);
        public static int countbits(uint x)
        {
            x = x - ((x >> 1) & 0x55555555);
            x = (x & 0x33333333) + ((x >> 2) & 0x33333333);
            return (int)((((x + (x >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24);
        }

        public static int lzcnt(int x) => lzcnt((uint)x);
        public static int lzcnt(uint x)
        {
            if (x == 0)
                return 32;

            Union8 u;
            u.Double = 0.0;
            u.Long = 0x4330000000000000L + x;
            u.Double -= 4503599627370496.0;
            return 0x41E - (int)(u.Long >> 52);
        }

        public static int tzcnt(int x) => tzcnt((uint)x);
        public static int tzcnt(uint x)
        {
            if (x == 0)
                return 32;

            x &= (uint)-x;
            Union8 u;
            u.Double = 0.0;
            u.Long = 0x4330000000000000L + x;
            u.Double -= 4503599627370496.0;
            return (int)(u.Long >> 52) - 0x3FF;
        }
    }
#pragma warning restore IDE1006 // Naming Styles
#endif // PACKAGE_MATHEMATICS
}
