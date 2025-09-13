using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PKGE
{
    /// <summary>
    /// Math utilities.
    /// </summary>
    public static class MathUtility
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/MathUtility.cs
        #region Unity.XR.CoreUtils
        // constants used in approximate equality checks
        internal static readonly float EpsilonScaled = Mathf.Epsilon * 8;

        /// <summary>
        /// A faster replacement for <see cref="Mathf.Approximately(float, float)"/>.
        /// </summary>
        /// <remarks>
        /// Compares two floating point values and returns true if they are similar.
        /// As an optimization, this method does not take into account the magnitude of the values it is comparing.
        /// This method may not provide the same results as `Mathf.Approximately` for extremely large values.
        /// </remarks>
        /// <param name="a">The first float to compare.</param>
        /// <param name="b">The second float to compare.</param>
        /// <returns><see langword="true"/> if the values are similar. Otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(this float a, float b)
        {
            var d = b - a;
            var absDiff = d >= 0f ? d : -d;
            return absDiff < EpsilonScaled;
        }

        /// <summary>
        /// A slightly faster way to do `Approximately(a, 0f)`.
        /// </summary>
        /// <param name="a">The floating point value to compare with 0.</param>
        /// <returns><see langword="true"/> if the value is comparable to zero. Otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproximatelyZero(this float a)
        {
            return (a >= 0f ? a : -a) < EpsilonScaled;
        }

        /// <summary>
        /// Constrains a value between a minimum and a maximum.
        /// </summary>
        /// <param name="input">The input number.</param>
        /// <param name="min">The minimum output.</param>
        /// <param name="max">The maximum output.</param>
        /// <returns>The <paramref name="input"/> number, clamped between <paramref name="min"/> and <paramref name="max"/> (inclusive).</returns>
        public static double Clamp(this double input, double min, double max)
        {
            return Math.Max(Math.Min(input, max), min);
        }

        /// <summary>
        /// Finds the smallest angle between two angles.
        /// </summary>
        /// <param name="start">The start value.</param>
        /// <param name="end">The end value.</param>
        /// <param name="halfMax">Half of the max angle.</param>
        /// <param name="max">The max angle value.</param>
        /// <returns>The angle distance between start and end.</returns>
        public static double ShortestAngleDistance(this double start, double end, double halfMax, double max)
        {
            var angleDelta = end - start;
            var angleSign = Math.Sign(angleDelta);

            angleDelta = Math.Abs(angleDelta) % max;
            if (angleDelta > halfMax)
                angleDelta = -(max - angleDelta);

            return angleDelta * angleSign;
        }

        /// <summary>
        /// Finds the smallest angle between two angles.
        /// </summary>
        /// <param name="start">The start value.</param>
        /// <param name="end">The end value.</param>
        /// <param name="halfMax">Half of the max angle.</param>
        /// <param name="max">The max angle value.</param>
        /// <returns>The angle distance between start and end.</returns>
        public static float ShortestAngleDistance(this float start, float end, float halfMax, float max)
        {
            var angleDelta = end - start;
            var angleSign = Mathf.Sign(angleDelta);

            angleDelta = Math.Abs(angleDelta) % max;
            if (angleDelta > halfMax)
                angleDelta = -(max - angleDelta);

            return angleDelta * angleSign;
        }

        /// <summary>
        /// Checks whether a value is undefined (<see cref="float.PositiveInfinity"/>,
        /// <see cref="float.NegativeInfinity"/>, or <see cref="float.NaN"/>).
        /// </summary>
        /// <seealso cref="float.IsInfinity(float)"/>
        /// <seealso cref="float.IsNaN(float)"/>
        /// <param name="value">The float value.</param>
        /// <returns>True if the value is infinity or NaN (not a number), otherwise false.</returns>
        public static bool IsUndefined(this float value)
        {
            return float.IsInfinity(value) || float.IsNaN(value);
        }

        /// <summary>
        /// Checks if a vector is aligned with one of the axis vectors.
        /// </summary>
        /// <param name="v"> The vector.</param>
        /// <returns>True if the vector is aligned with any axis, otherwise false.</returns>
        public static bool IsAxisAligned(this Vector3 v)
        {
            return ApproximatelyZero(v.x * v.y) && ApproximatelyZero(v.y * v.z) && ApproximatelyZero(v.z * v.x);
        }

        /// <summary>
        /// Checks if a value is a positive power of two.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the value is a positive power of two, false otherwise.</returns>
        public static bool IsPositivePowerOfTwo(this int value)
        {
            return value > 0 && (value & (value - 1)) == 0;
        }

        /// <summary>
        /// Returns the index of the first true bit of a flag.
        /// </summary>
        /// <param name="value">The flags value to check.</param>
        /// <returns>The index of the first active flag.</returns>
        public static int FirstActiveFlagIndex(this int value)
        {
            if (value == 0)
                return 0;

            const int bits = sizeof(int) * 8;
            for (var i = 0; i < bits; i++)
                if ((value & 1 << i) != 0)
                    return i;

            return 0;
        }
        #endregion // Unity.XR.CoreUtils
        
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/main/Packages/com.unity.live-capture/Runtime/Core/Utilities/MathUtility.cs
        #region Unity.LiveCapture
        enum RotationOrder { OrderXYZ, OrderXZY, OrderYZX, OrderYXZ, OrderZXY, OrderZYX }

        const float EpsilonLegacyEuler = 1.0E-3f;
        static readonly Vector4[] RotationOrderLut =
        {
            new Vector4(1f, 1f, 1f, 1f), new Vector4(-1f, 1f, -1f, 1f), //XYZ
            new Vector4(1f, 1f, 1f, 1f), new Vector4(1f, 1f, -1f, -1f), //XZY
            new Vector4(1f, -1f, 1f, 1f), new Vector4(-1f, 1f, 1f, 1f), //YZX
            new Vector4(1f, 1f, 1f, 1f), new Vector4(-1f, 1f, 1f, -1f), //YXZ
            new Vector4(1f, -1f, 1f, 1f), new Vector4(1f, 1f, -1f, 1f), //ZXY
            new Vector4(1f, -1f, 1f, 1f), new Vector4(1f, 1f, 1f, -1f) //ZYX
        };

        static Vector3 XZY(this Vector3 v) => new Vector3(v.x, v.z, v.y);
        static Vector3 YZX(this Vector3 v) => new Vector3(v.y, v.z, v.x);
        static Vector3 YXZ(this Vector3 v) => new Vector3(v.y, v.x, v.z);
        static Vector3 ZXY(this Vector3 v) => new Vector3(v.z, v.x, v.y);
        static Vector3 ZYX(this Vector3 v) => new Vector3(v.z, v.y, v.x);
        static Vector3 XYZ(this Vector4 v) => new Vector3(v.x, v.y, v.z);
        static Vector4 XYXY(this Vector4 v) => new Vector4(v.x, v.y, v.x, v.y);
        static Vector4 XWYZ(this Vector4 v) => new Vector4(v.x, v.w, v.y, v.z);
        static Vector4 YYWW(this Vector4 v) => new Vector4(v.y, v.y, v.w, v.w);
        static Vector4 YWZX(this Vector4 v) => new Vector4(v.y, v.w, v.z, v.x);
        static Vector4 YZXW(this Vector4 v) => new Vector4(v.y, v.z, v.x, v.w);
        static Vector4 ZXZX(this Vector4 v) => new Vector4(v.z, v.x, v.z, v.x);
        static Vector4 ZZWW(this Vector4 v) => new Vector4(v.z, v.z, v.w, v.w);
        static Vector4 ZWXY(this Vector4 v) => new Vector4(v.z, v.w, v.x, v.y);
        static Vector4 WWWW(this Vector4 v) => new Vector4(v.w, v.w, v.w, v.w);
        static Vector4 WXYZ(this Vector4 v) => new Vector4(v.w, v.x, v.y, v.z);
        static Vector4 WZXY(this Vector4 v) => new Vector4(v.w, v.z, v.x, v.y);
        static float ChangeSign(this float x, float y) => y < 0 ? -x : x;
        static Vector3 ChangeSign(Vector3 x, Vector3 y) => new Vector3(ChangeSign(x.x, y.x), ChangeSign(x.y, y.y), ChangeSign(x.z, y.z));
        static Vector4 ChangeSign(Vector4 x, Vector4 y) => new Vector4(ChangeSign(x.x, y.x), ChangeSign(x.y, y.y), ChangeSign(x.z, y.z), ChangeSign(x.w, y.w));
        static Vector3 Mul(this Vector3 a, Vector3 b) => Vector3.Scale(a, b);
        static Vector4 Mul(this Vector4 a, Vector4 b) => Vector4.Scale(a, b);
        static Vector3 Div(this Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        static Vector3 Round(Vector3 v) => new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
        static float Degrees(this float v) => v * Mathf.Rad2Deg;
        static Vector3 Degrees(Vector3 v) => new Vector3(Degrees(v.x), Degrees(v.y), Degrees(v.z));
        static float Radians(this float v) => v * Mathf.Deg2Rad;
        static Vector3 Radians(Vector3 v) => new Vector3(Radians(v.x), Radians(v.y), Radians(v.z));
        static Vector3 Select(Vector3 a, Vector3 b, bool c) => c ? b : a;
        static float CSum(Vector4 v) => v.x + v.y + v.z + v.w;
        static void SinCos(Vector3 v, out Vector3 s, out Vector3 c)
        {
            s = new Vector3(Mathf.Sin(v.x), Mathf.Sin(v.y), Mathf.Sin(v.z));
            c = new Vector3(Mathf.Cos(v.x), Mathf.Cos(v.y), Mathf.Cos(v.z));
        }

        /// <exception cref="ArgumentException">Thrown if <paramref name="order"/> is not a valid rotation.</exception>
        static Vector3 EulerReorder(Vector3 euler, RotationOrder order)
        {
            switch (order)
            {
                case RotationOrder.OrderXYZ:
                    return euler;
                case RotationOrder.OrderXZY:
                    return euler.XZY();
                case RotationOrder.OrderYZX:
                    return euler.YZX();
                case RotationOrder.OrderYXZ:
                    return euler.YXZ();
                case RotationOrder.OrderZXY:
                    return euler.ZXY();
                case RotationOrder.OrderZYX:
                    return euler.ZYX();
            }

            throw new ArgumentException("invalid rotationOrder");
        }

        /// <exception cref="ArgumentException">Thrown if <paramref name="order"/> is not a valid rotation.</exception>
        static Vector3 EulerReorderBack(Vector3 euler, RotationOrder order)
        {
            switch (order)
            {
                case RotationOrder.OrderXYZ:
                    return euler;
                case RotationOrder.OrderXZY:
                    return euler.XZY();
                case RotationOrder.OrderYZX:
                    return euler.ZXY();
                case RotationOrder.OrderYXZ:
                    return euler.YXZ();
                case RotationOrder.OrderZXY:
                    return euler.YZX();
                case RotationOrder.OrderZYX:
                    return euler.ZYX();
            }

            throw new ArgumentException("invalid rotationOrder");
        }

        static Vector3 QuatToEuler(Vector4 q, RotationOrder order)
        {
            //prepare the data
            Vector4 d1 = q.Mul(q.WWWW()) * 2f; //xw, yw, zw, ww
            Vector4 d2 = q.Mul(q.YZXW()) * 2f; //xy, yz, zx, ww
            Vector4 d3 = q.Mul(q);
            Vector3 euler = Vector3.zero;

            const float cutoff = (1f - 2f * float.Epsilon) * (1f - 2f * float.Epsilon);

            switch (order)
            {
                case RotationOrder.OrderZYX: //ZYX
                {
                    float y1 = d2.z + d1.y;
                    if (y1 * y1 < cutoff)
                    {
                        float x1 = -d2.x + d1.z;
                        float x2 = d3.x + d3.w - d3.y - d3.z;
                        float z1 = -d2.y + d1.x;
                        float z2 = d3.z + d3.w - d3.y - d3.x;
                        euler = new Vector3(Mathf.Atan2(x1, x2), Mathf.Asin(y1), Mathf.Atan2(z1, z2));
                    }
                    else     //zxz
                    {
                        y1 = Mathf.Clamp(y1, -1.0f, 1.0f);
                        Vector4 abcd = new Vector4(d2.z, d1.y, d2.y, d1.x);
                        float x1 = 2.0f * (abcd.x * abcd.w + abcd.y * abcd.z);     //2(ad+bc)
                        float x2 = CSum(abcd.Mul(abcd).Mul(new Vector4(-1f, 1f, -1f, 1f)));
                        euler = new Vector3(Mathf.Atan2(x1, x2), Mathf.Asin(y1), 0f);
                    }
                    break;
                }
                case RotationOrder.OrderZXY: //ZXY
                {
                    float y1 = d2.y - d1.x;
                    if (y1 * y1 < cutoff)
                    {
                        float x1 = d2.x + d1.z;
                        float x2 = d3.y + d3.w - d3.x - d3.z;
                        float z1 = d2.z + d1.y;
                        float z2 = d3.z + d3.w - d3.x - d3.y;
                        euler = new Vector3(Mathf.Atan2(x1, x2), -Mathf.Asin(y1), Mathf.Atan2(z1, z2));
                    }
                    else     //zxz
                    {
                        y1 = Mathf.Clamp(y1, -1f, 1f);
                        Vector4 abcd = new Vector4(d2.z, d1.y, d2.y, d1.x);
                        float x1 = 2f * (abcd.x * abcd.w + abcd.y * abcd.z);     //2(ad+bc)
                        float x2 = CSum(abcd.Mul(abcd).Mul(new Vector4(-1f, 1f, -1f, 1f)));
                        euler = new Vector3(Mathf.Atan2(x1, x2), -Mathf.Asin(y1), 0f);
                    }
                    break;
                }
                case RotationOrder.OrderYXZ: //YXZ
                {
                    float y1 = d2.y + d1.x;
                    if (y1 * y1 < cutoff)
                    {
                        float x1 = -d2.z + d1.y;
                        float x2 = d3.z + d3.w - d3.x - d3.y;
                        float z1 = -d2.x + d1.z;
                        float z2 = d3.y + d3.w - d3.z - d3.x;
                        euler = new Vector3(Mathf.Atan2(x1, x2), Mathf.Asin(y1), Mathf.Atan2(z1, z2));
                    }
                    else     //yzy
                    {
                        y1 = Mathf.Clamp(y1, -1f, 1f);
                        Vector4 abcd = new Vector4(d2.x, d1.z, d2.y, d1.x);
                        float x1 = 2.0f * (abcd.x * abcd.w + abcd.y * abcd.z);     //2(ad+bc)
                        float x2 = CSum(abcd.Mul(abcd).Mul(new Vector4(-1f, 1f, -1f, 1f)));
                        euler = new Vector3(Mathf.Atan2(x1, x2), Mathf.Asin(y1), 0f);
                    }
                    break;
                }
                case RotationOrder.OrderYZX: //YZX
                {
                    float y1 = d2.x - d1.z;
                    if (y1 * y1 < cutoff)
                    {
                        float x1 = d2.z + d1.y;
                        float x2 = d3.x + d3.w - d3.z - d3.y;
                        float z1 = d2.y + d1.x;
                        float z2 = d3.y + d3.w - d3.x - d3.z;
                        euler = new Vector3(Mathf.Atan2(x1, x2), -Mathf.Asin(y1), Mathf.Atan2(z1, z2));
                    }
                    else     //yxy
                    {
                        y1 = Mathf.Clamp(y1, -1f, 1f);
                        Vector4 abcd = new Vector4(d2.x, d1.z, d2.y, d1.x);
                        float x1 = 2f * (abcd.x * abcd.w + abcd.y * abcd.z);     //2(ad+bc)
                        float x2 = CSum(abcd.Mul(abcd).Mul(new Vector4(-1f, 1f, -1f, 1f)));
                        euler = new Vector3(Mathf.Atan2(x1, x2), -Mathf.Asin(y1), 0f);
                    }
                    break;
                }

                case RotationOrder.OrderXZY: //XZY
                {
                    float y1 = d2.x + d1.z;
                    if (y1 * y1 < cutoff)
                    {
                        float x1 = -d2.y + d1.x;
                        float x2 = d3.y + d3.w - d3.z - d3.x;
                        float z1 = -d2.z + d1.y;
                        float z2 = d3.x + d3.w - d3.y - d3.z;
                        euler = new Vector3(Mathf.Atan2(x1, x2), Mathf.Asin(y1), Mathf.Atan2(z1, z2));
                    }
                    else     //xyx
                    {
                        y1 = Mathf.Clamp(y1, -1f, 1f);
                        Vector4 abcd = new Vector4(d2.x, d1.z, d2.z, d1.y);
                        float x1 = 2f * (abcd.x * abcd.w + abcd.y * abcd.z);     //2(ad+bc)
                        float x2 = CSum(abcd.Mul(abcd).Mul(new Vector4(-1f, 1f, -1f, 1f)));
                        euler = new Vector3(Mathf.Atan2(x1, x2), Mathf.Asin(y1), 0f);
                    }
                    break;
                }
                case RotationOrder.OrderXYZ: //XYZ
                {
                    float y1 = d2.z - d1.y;
                    if (y1 * y1 < cutoff)
                    {
                        float x1 = d2.y + d1.x;
                        float x2 = d3.z + d3.w - d3.y - d3.x;
                        float z1 = d2.x + d1.z;
                        float z2 = d3.x + d3.w - d3.y - d3.z;
                        euler = new Vector3(Mathf.Atan2(x1, x2), -Mathf.Asin(y1), Mathf.Atan2(z1, z2));
                    }
                    else     //xzx
                    {
                        y1 = Mathf.Clamp(y1, -1f, 1f);
                        Vector4 abcd = new Vector4(d2.z, d1.y, d2.x, d1.z);
                        float x1 = 2f * (abcd.x * abcd.w + abcd.y * abcd.z);     //2(ad+bc)
                        float x2 = CSum(abcd.Mul(abcd).Mul(new Vector4(-1f, 1f, -1f, 1f)));
                        euler = new Vector3(Mathf.Atan2(x1, x2), -Mathf.Asin(y1), 0f);
                    }
                    break;
                }
            }

            return EulerReorderBack(euler, order);
        }

        static Vector4 EulerToQuat(Vector3 euler, RotationOrder order = RotationOrder.OrderXYZ)
        {
            Vector3 c, s;
            SinCos(euler * 0.5f, out s, out c);

            Vector4 t = new Vector4(s.x * c.z, s.x * s.z, c.x * s.z, c.x * c.z);

            return c.y * t.Mul(RotationOrderLut[2 * (int)order]) + s.y * RotationOrderLut[2 * (int)order + 1].Mul(t.ZWXY());
        }

        static Vector4 QuatMul(Vector4 q1, Vector4 q2)
        {
            return ChangeSign(
                (q1.YWZX().Mul(q2.XWYZ()) -
                    q1.WXYZ().Mul(q2.ZXZX()) -
                    q1.ZZWW().Mul(q2.WZXY()) -
                    q1.XYXY().Mul(q2.YYWW())).ZWXY(), new Vector4(-1f, -1f, -1f, 1f));
        }

        static float QuatDiff(Vector4 a, Vector4 b)
        {
            float diff = Mathf.Asin(QuatMul(QuatConj(a), b).normalized.XYZ().magnitude);
            return diff + diff;
        }

        static Vector4 QuatConj(Vector4 q)
        {
            return ChangeSign(q, new Vector4(-1f, -1f, -1f, 1f));
        }

        static Vector3 AlternateEuler(Vector3 euler, RotationOrder rotationOrder)
        {
            Vector3 eulerAlt = EulerReorder(euler, rotationOrder);
            eulerAlt += new Vector3(180f, 180f, 180f);
            eulerAlt = ChangeSign(eulerAlt, new Vector3(1f, -1f, 1f));
            return EulerReorderBack(eulerAlt, rotationOrder);
        }

        static Vector3 SyncEuler(Vector3 euler, Vector3 eulerHint)
        {
            return euler + Round((eulerHint - euler).Div(new Vector3(360f, 360f, 360f))).Mul(new Vector3(360f, 360f, 360f));
        }

        static Vector3 ClosestEuler(Vector3 euler, Vector3 eulerHint, RotationOrder rotationOrder)
        {
            Vector3 eulerSynced = SyncEuler(euler, eulerHint);
            Vector3 altEulerSynced = SyncEuler(AlternateEuler(euler, rotationOrder), eulerHint);

            Vector3 diff = eulerSynced - eulerHint;
            Vector3 altDiff = altEulerSynced - eulerHint;

            return Select(altEulerSynced, eulerSynced, Vector3.Dot(diff, diff) < Vector3.Dot(altDiff, altDiff));
        }

        static Vector3 ClosestEuler(Vector4 q, Vector3 eulerHint, RotationOrder rotationOrder)
        {
            var eps = new Vector3(EpsilonLegacyEuler, EpsilonLegacyEuler, EpsilonLegacyEuler);
            Vector3 euler = Degrees(QuatToEuler(q, rotationOrder));
            euler = Round(euler.Div(eps)).Mul(eps);
            Vector4 qHint = EulerToQuat(Radians(eulerHint), rotationOrder);
            float angleDiff = Degrees(QuatDiff(q, qHint));

            return Select(ClosestEuler(euler, eulerHint, rotationOrder), eulerHint, angleDiff < EpsilonLegacyEuler);
        }

        static Vector3 ClosestEuler(Quaternion quaternion, Vector3 eulerHint, RotationOrder rotationOrder)
        {
            return ClosestEuler(new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w), eulerHint, rotationOrder);
        }

        public static Vector3 ClosestEuler(Quaternion quaternion, Vector3 eulerHint)
        {
            return ClosestEuler(quaternion, eulerHint, RotationOrder.OrderZXY);
        }

        public static Quaternion Add(this in Quaternion a, in Quaternion b)
        {
            return new Quaternion(
                a.x + b.x,
                a.y + b.y,
                a.z + b.z,
                a.w + b.w
            );
        }

        public static Quaternion Sub(this in Quaternion a, in Quaternion b)
        {
            return new Quaternion(
                a.x - b.x,
                a.y - b.y,
                a.z - b.z,
                a.w - b.w
            );
        }

        public static Quaternion Mul(this in Quaternion a, float s)
        {
            return new Quaternion(
                a.x * s,
                a.y * s,
                a.z * s,
                a.w * s
            );
        }

        public static Quaternion Div(this in Quaternion a, float s)
        {
            return new Quaternion(
                a.x / s,
                a.y / s,
                a.z / s,
                a.w / s
            );
        }
        
        public static float SafeDivide(this float y, float x, float threshold = 0f)
        {
            if (Mathf.Abs(x) > threshold)
                return y / x;
            else
                return default;
        }
        
        public static double SafeDivide(this double y, double x, double threshold = 0)
        {
            if (Math.Abs(x) > threshold)
                return y / x;
            else
                return default;
        }

        public static Vector2 SafeDivide(this Vector2 y, float x, float threshold = 0f)
        {
            if (Mathf.Abs(x) > threshold)
                return y / x;
            else
                return default;
        }

        public static Vector3 SafeDivide(this Vector3 y, float x, float threshold = 0f)
        {
            if (Mathf.Abs(x) > threshold)
                return y / x;
            else
                return default;
        }

        public static Vector4 SafeDivide(this Vector4 y, float x, float threshold = 0f)
        {
            if (Mathf.Abs(x) > threshold)
                return y / x;
            else
                return default;
        }

        public static Quaternion SafeDivide(this Quaternion y, float x, float threshold = 0f)
        {
            if (Mathf.Abs(x) > threshold)
                return Div(y, x);
            else
                return default;
        }

        public static float Magnitude(this Quaternion q)
        {
            return Mathf.Sqrt(Quaternion.Dot(q, q));
        }

        public static bool CompareApproximately(this float f0, float f1, float epsilon = 0.000001F)
        {
            var dist = (f0 - f1);
            dist = Mathf.Abs(dist);
            return dist <= epsilon;
        }

        public static float Hermite(this float t, float p0, float m0, float m1, float p1)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            var a = 2.0F * t3 - 3.0F * t2 + 1.0F;
            var b = t3 - 2.0F * t2 + t;
            var c = t3 - t2;
            var d = -2.0F * t3 + 3.0F * t2;

            return a * p0 + b * m0 + c * m1 + d * p1;
        }

        public static Vector2 Hermite(this float t, in Vector2 p0, in Vector2 m0, in Vector2 m1, in Vector2 p1)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            var a = 2.0F * t3 - 3.0F * t2 + 1.0F;
            var b = t3 - 2.0F * t2 + t;
            var c = t3 - t2;
            var d = -2.0F * t3 + 3.0F * t2;

            return new Vector2(
                a * p0.x + b * m0.x + c * m1.x + d * p1.x,
                a * p0.y + b * m0.y + c * m1.y + d * p1.y
            );
        }

        public static Vector3 Hermite(this float t, in Vector3 p0, in Vector3 m0, in Vector3 m1, in Vector3 p1)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            var a = 2.0F * t3 - 3.0F * t2 + 1.0F;
            var b = t3 - 2.0F * t2 + t;
            var c = t3 - t2;
            var d = -2.0F * t3 + 3.0F * t2;

            return new Vector3(
                a * p0.x + b * m0.x + c * m1.x + d * p1.x,
                a * p0.y + b * m0.y + c * m1.y + d * p1.y,
                a * p0.z + b * m0.z + c * m1.z + d * p1.z
            );
        }

        public static Vector4 Hermite(this float t, in Vector4 p0, in Vector4 m0, in Vector4 m1, in Vector4 p1)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            var a = 2.0F * t3 - 3.0F * t2 + 1.0F;
            var b = t3 - 2.0F * t2 + t;
            var c = t3 - t2;
            var d = -2.0F * t3 + 3.0F * t2;

            return new Vector4(
                a * p0.x + b * m0.x + c * m1.x + d * p1.x,
                a * p0.y + b * m0.y + c * m1.y + d * p1.y,
                a * p0.z + b * m0.z + c * m1.z + d * p1.z,
                a * p0.w + b * m0.w + c * m1.w + d * p1.w
            );
        }

        public static Quaternion Hermite(this float t, in Quaternion p0, in Quaternion m0, in Quaternion m1, in Quaternion p1)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            var a = 2.0F * t3 - 3.0F * t2 + 1.0F;
            var b = t3 - 2.0F * t2 + t;
            var c = t3 - t2;
            var d = -2.0F * t3 + 3.0F * t2;

            return new Quaternion(
                a * p0.x + b * m0.x + c * m1.x + d * p1.x,
                a * p0.y + b * m0.y + c * m1.y + d * p1.y,
                a * p0.z + b * m0.z + c * m1.z + d * p1.z,
                a * p0.w + b * m0.w + c * m1.w + d * p1.w
            );
        }
        #endregion // Unity.LiveCapture
        
        //https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/3417c4765f52f72d2384f2f7e65bd9d2d1dfd7ac/com.unity.netcode.gameobjects/Runtime/Serialization/Arithmetic.cs
        #region Unity.Netcode
        // Sign bits for different data types
        internal const long SIGN_BIT_64 = long.MinValue;
        internal const int SIGN_BIT_32 = int.MinValue;
        internal const short SIGN_BIT_16 = short.MinValue;
        internal const sbyte SIGN_BIT_8 = sbyte.MinValue;

        // Ceiling function that doesn't deal with floating point values
        // these only work correctly with positive numbers
        public static ulong CeilingExact(this ulong u1, ulong u2) => (u1 + u2 - 1) / u2;
        public static long CeilingExact(this long u1, long u2) => (u1 + u2 - 1) / u2;
        public static uint CeilingExact(this uint u1, uint u2) => (u1 + u2 - 1) / u2;
        public static int CeilingExact(this int u1, int u2) => (u1 + u2 - 1) / u2;
        public static ushort CeilingExact(this ushort u1, ushort u2) => (ushort)((u1 + u2 - 1) / u2);
        public static short CeilingExact(this short u1, short u2) => (short)((u1 + u2 - 1) / u2);
        public static byte CeilingExact(this byte u1, byte u2) => (byte)((u1 + u2 - 1) / u2);
        public static sbyte CeilingExact(this sbyte u1, sbyte u2) => (sbyte)((u1 + u2 - 1) / u2);

        /// <summary>
        /// ZigZag encodes a signed integer and maps it to an unsigned integer
        /// </summary>
        /// <param name="value">The signed integer to encode</param>
        /// <returns>A ZigZag encoded version of the integer</returns>
        public static ulong ZigZagEncode(this long value) => (ulong)((value >> 63) ^ (value << 1));

        /// <inheritdoc cref="ZigZagEncode(long)"/>
        public static uint ZigZagEncode(this int value) => (uint)((value >> 31) ^ (value << 1));

        /// <summary>
        /// Decides a ZigZag encoded integer back to a signed integer
        /// </summary>
        /// <param name="value">The unsigned integer</param>
        /// <returns>The signed version of the integer</returns>
        public static long ZigZagDecode(this ulong value) => (((long)(value >> 1) & 0x7FFFFFFFFFFFFFFFL) ^ ((long)(value << 63) >> 63));

        /// <inheritdoc cref="ZigZagDecode(ulong)"/>
        public static long ZigZagDecode(this uint value) => (((int)(value >> 1) & 0x7FFFFFFF) ^ ((int)(value << 31) >> 31));

        /// <summary>
        /// Gets the output size in bytes after VarInting an unsigned integer
        /// </summary>
        /// <param name="value">The unsigned integer whose length to get</param>
        /// <returns>The amount of bytes</returns>
        public static int VarIntSize(this ulong value) =>
            value <= 240 ? 1 :
            value <= 2287 ? 2 :
            value <= 67823 ? 3 :
            value <= 16777215 ? 4 :
            value <= 4294967295 ? 5 :
            value <= 1099511627775 ? 6 :
            value <= 281474976710655 ? 7 :
            value <= 72057594037927935 ? 8 :
            9;

        public static long Div8Ceil(this ulong value) => (long)((value >> 3) + ((value & 1UL) | ((value >> 1) & 1UL) | ((value >> 2) & 1UL)));
        #endregion // Unity.Netcode
        
        //https://github.com/needle-mirror/com.unity.cinemachine/blob/85e81c94d0839e65c46a6fe0cd638bd1c6cd48af/Runtime/Core/UnityVectorExtensions.cs
        #region Unity.Cinemachine
        /// <summary>
        /// Put euler angle in the range of -180...180
        /// </summary>
        /// <param name="angle">The angle to normalize</param>
        /// <returns>The angle expressed as a value -180...180</returns>
        public static float NormalizeAngle(this float angle)
        {
            angle %= 360;
            return angle > 180 ? angle - 360 : angle;
        }
        #endregion // Unity.Cinemachine
    }
}