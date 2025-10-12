#if INCLUDE_MATHEMATICS
#else
using System.Runtime.CompilerServices;
using PKGE;
using float2 = UnityEngine.Vector2;
using float3 = UnityEngine.Vector3;
using float3x3 = UnityEngine.Matrix4x4;
using float4 = UnityEngine.Vector4;
using quaternion = UnityEngine.Quaternion;

namespace PKGE.Mathematics
{
#pragma warning disable IDE1006 // Naming Styles
    public static class math
    {
        public static readonly float SQRT2 = (float)System.Math.Sqrt(2);
        public const float FLT_MIN_NORMAL = 1.175494351e-38F;
        public const float PI = (float)System.Math.PI;
        public const float PI2 = (float)(System.Math.PI * 2);

        public static int asint(float f) => new Union4 { Float = f }.Int;
        public static float asfloat(int i) => new Union4 { Int = i }.Float;

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
        public static double max(double a, double b) => System.Math.Max(a, b);

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

        public static float length(float2 a) => sqrt(lengthsq(a));
        public static float length(float3 a) => sqrt(lengthsq(a));
        public static float lengthsq(float2 a) => a.x * a.x + a.y * a.y;
        public static float lengthsq(float3 a) => a.x * a.x + a.y * a.y + a.z * a.z;

        public static float sign(float x) => (x > 0.0f ? 1.0f : 0.0f) - (x < 0.0f ? 1.0f : 0.0f);

        public static float abs(float a) => System.Math.Abs(a);
        public static double abs(double a) => System.Math.Abs(a);
        public static float3 abs(float3 a) => new float3(System.Math.Abs(a.x), System.Math.Abs(a.y), System.Math.Abs(a.z));
        public static float4 abs(float4 a) => new float4(
            System.Math.Abs(a.x),
            System.Math.Abs(a.y),
            System.Math.Abs(a.z),
            System.Math.Abs(a.w));

        public static float3 normalize(float3 a) => a.normalized;
        public static quaternion normalize(quaternion a) => a.normalized;
        public static float dot(float2 a, float2 b) => float2.Dot(a, b);
        public static float dot(float3 a, float3 b) => float3.Dot(a, b);
        public static float dot(quaternion a, quaternion b) => quaternion.Dot(a, b);
        public static float rcp(int a) => 1f / a;
        public static float rcp(float a) => 1f / a;
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
        public static float3x3 mul(float3x3 a, float3x3 b) => a * b;

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
        public static quaternion slerp(quaternion start, quaternion end, float t) => quaternion.Lerp(start, end, t);
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

        public static quaternion conjugate(quaternion q)
        {
            return new quaternion(-q.x, -q.y, -q.z, q.w);
        }

        /// <summary>Returns a float3x3 matrix that rotates around the y-axis by a given number of radians.</summary>
        /// <param name="angle">The clockwise rotation angle when looking along the y-axis towards the origin in radians.</param>
        /// <returns>The float3x3 rotation matrix representing a rotation around the y-axis.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3x3 RotateY(float angle)
        {
            // {{c_1, 0, s_1}, {0, 1, 0}, {-s_1, 0, c_1}}
            float s, c;
            sincos(angle, out s, out c);
            return new float3x3(new float4(c, 0, -s), new float4(0, 1, 0), new float4(s, 0, c), default);
        }
    }
#pragma warning restore IDE1006 // Naming Styles
}
#endif // INCLUDE_MATHEMATICS
