using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using static Unity.Mathematics.math;
using Random = Unity.Mathematics.Random;

namespace UnityExtensions.Packages
{
    #region Union
    /// <inheritdoc cref="Union1"/>
    /// <remarks>Use __0 to access the Union without extensions.</remarks>
    [StructLayout(LayoutKind.Explicit)]
    public struct Union2
    {
        [FieldOffset(0)] public UnityExtensions.Union2 __0;

        [FieldOffset(0)] public half Half;
    }

    /// <inheritdoc cref="Union2"/>
    [StructLayout(LayoutKind.Explicit)]
    public struct Union4
    {
        [FieldOffset(0)] public UnityExtensions.Union4 __0;

        [FieldOffset(0)] public half2 Half2;

        [FieldOffset(0)] public Union2 _0;
        [FieldOffset(2)] public Union2 _2;
    }

    /// <inheritdoc cref="Union2"/>
    [StructLayout(LayoutKind.Explicit)]
    public struct Union8
    {
        [FieldOffset(0)] public UnityExtensions.Union8 __0;

        [FieldOffset(0)] public half4 Half4;
        [FieldOffset(0)] public float2 Float2;
        [FieldOffset(0)] public int2 Int2;
        [FieldOffset(0)] public uint2 UInt2;

        [FieldOffset(0)] public Union4 _0;
        [FieldOffset(4)] public Union4 _4;
    }

    /// <inheritdoc cref="Union2"/>
    [StructLayout(LayoutKind.Explicit)]
    public struct Union16
    {
        [FieldOffset(0)] public UnityExtensions.Union16 __0;

        [FieldOffset(0)] public float4 Float4;
        [FieldOffset(0)] public int4 Int4;
        [FieldOffset(0)] public uint4 UInt4;
        [FieldOffset(0)] public double2 Double2;
        [FieldOffset(0)] public quaternion Quaternion;

        [FieldOffset(0)] public Union8 _0;
        [FieldOffset(8)] public Union8 _8;
    }
    #endregion // Union

    public static class MathematicsExtensions
    {
        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Runtime/Supplementary/Math/AffineTransform.cs
        #region Unity.Mathematics
        // smallest such that 1.0+epsilon != 1.0
        internal const float epsilon = 1.192092896e-07f;

        internal static int roundToInt(float f)
        {
            return (int)floor(f + 0.5f);
        }

        public static float3 project(float3 a, float3 b)
        {
            return dot(a, b) * b;
        }

        internal static float sel(float x, float a, float b)
        {
            return x >= 0.0f ? a : b;
        }

        internal static float3 xaxis(quaternion q)
        {
            float s = 2.0f * q.value.w;
            float x2 = 2.0f * q.value.x;
            return new float3(
                x2 * q.value.x + s * q.value.w - 1.0f,
                x2 * q.value.y + s * q.value.z,
                x2 * q.value.z + s * -q.value.y);
        }

        internal static float3 yaxis(quaternion q)
        {
            float s = 2.0f * q.value.w;
            float y2 = 2.0f * q.value.y;
            return new float3(
                y2 * q.value.x + s * -q.value.z,
                y2 * q.value.y + s * q.value.w - 1.0f,
                y2 * q.value.z + s * q.value.x);
        }

        public static float3 zaxis(quaternion q)
        {
            // This should be fast than math.quaternion.forward().
            // Need to make sure this doesn't get translated to
            // float-by-float operations.
            float s = 2.0f * q.value.w;
            float z2 = 2.0f * q.value.z;
            return new float3(
                q.value.x * z2 + s * q.value.y,
                q.value.y * z2 + s * -q.value.x,
                q.value.z * z2 + s * q.value.w - 1.0f);
        }

        public static bool equalEps(float a, float b, float epsilon)
        {
            Assert.IsTrue(epsilon >= 0.0f);

            // If the following assert is triggered, it means that epsilon is so small compared to a or b that it is lower than the
            // float precision. To fix this assert either set epsilon to 0.0f or increase epsilon
            Assert.IsTrue(epsilon == 0.0f || epsilon >= min(abs(a), abs(b)) * 1e-8f);

            return abs(a - b) <= epsilon;
        }

        public static bool equalEps(float3 lhs, float3 rhs, float epsilon)
        {
            Assert.IsTrue(epsilon >= 0.0f);
            return
                equalEps(lhs.x, rhs.x, epsilon) &&
                equalEps(lhs.y, rhs.y, epsilon) &&
                equalEps(lhs.z, rhs.z, epsilon);
        }

        internal static bool equalEps(quaternion lhs, quaternion rhs, float epsilon)
        {
            return abs(dot(lhs, rhs)) > 1.0f - epsilon;
        }

        internal static bool equalEps(AffineTransform lhs, AffineTransform rhs, float epsilon)
        {
            return equalEps(lhs.q, rhs.q, epsilon) && equalEps(lhs.t, rhs.t, epsilon);
        }

        internal static float3 log(quaternion q)
        {
            float3 v = new float3(q.value.x, q.value.y, q.value.z);
            float sinHalfAngle = length(v);
            if (sinHalfAngle < epsilon)
            {
                return zero;
            }
            else
            {
                float f = atan2(sinHalfAngle, q.value.w) / sinHalfAngle;
                return new float3(f * q.value.x, f * q.value.y, f * q.value.z);
            }
        }

        internal static quaternion negate(quaternion q)
        {
            return new quaternion(-q.value.x, -q.value.y, -q.value.z, -q.value.w);
        }

        public static quaternion conjugate(quaternion q)
        {
            return new quaternion(-q.value.x, -q.value.y, -q.value.z, q.value.w);
        }

        public static float3 axisAngle(quaternion q, out float angle)
        {
            float3 v = new float3(q.value.x, q.value.y, q.value.z);
            float sinHalfAngle = length(v);
            if (sinHalfAngle < epsilon)
            {
                angle = 0.0f;
                return new float3(1.0f, 0.0f, 0.0f);
            }
            else
            {
                angle = 2.0f * atan2(sinHalfAngle, q.value.w);
                return v * (1.0f / sinHalfAngle);
            }
        }

        internal static float angle(quaternion q)
        {
            return 2.0f * acos(clamp(q.value.w, -1.0f, 1.0f));
        }

        public static AffineTransform lerp(AffineTransform lhs, AffineTransform rhs, float theta)
        {
            return new AffineTransform(
                math.lerp(lhs.t, rhs.t, theta),
                slerp(lhs.q, rhs.q, theta));
        }

        internal static float squared(float value)
        {
            return value * value;
        }

        public static float3 zero
        {
            get
            {
                return new float3(0.0f);
            }
        }

        internal static float3 half
        {
            get
            {
                return new float3(0.5f);
            }
        }

        public static float3 right
        {
            get
            {
                return new float3(1.0f, 0.0f, 0.0f);
            }
        }

        public static float3 up
        {
            get
            {
                return new float3(0.0f, 1.0f, 0.0f);
            }
        }

        public static float3 forward
        {
            get
            {
                return new float3(0.0f, 0.0f, 1.0f);
            }
        }

        public static float3 NaN
        {
            get
            {
                return new float3(float.NaN, float.NaN, float.NaN);
            }
        }

        public static bool IsNaN(float3 value)
        {
            return
                float.IsNaN(value.x) ||
                float.IsNaN(value.y) ||
                float.IsNaN(value.y);
        }

        internal static float3 min
        {
            get
            {
                return new float3(float.MinValue, float.MinValue, float.MinValue);
            }
        }

        internal static float3 max
        {
            get
            {
                return new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            }
        }

        internal static float3 minimize(float3 lhs, float3 rhs)
        {
            return new float3(
                lhs.x < rhs.x ? lhs.x : rhs.x,
                lhs.y < rhs.y ? lhs.y : rhs.y,
                lhs.z < rhs.z ? lhs.z : rhs.z);
        }

        internal static float3 maximize(float3 lhs, float3 rhs)
        {
            return new float3(
                lhs.x > rhs.x ? lhs.x : rhs.x,
                lhs.y > rhs.y ? lhs.y : rhs.y,
                lhs.z > rhs.z ? lhs.z : rhs.z);
        }

        internal static float threshold(float lhs, float epsilon, float replacement)
        {
            return abs(lhs) <= epsilon ? replacement : lhs;
        }

        internal static float3 threshold(float3 lhs, float epsilon, float replacement)
        {
            return new float3(
                threshold(lhs.x, epsilon, replacement),
                threshold(lhs.y, epsilon, replacement),
                threshold(lhs.z, epsilon, replacement));
        }

        internal static float3 orthogonal(float3 v)
        {
            float3 vn = normalize(v);

            if (vn.z < 0.5f && vn.z > -0.5f)
            {
                return normalize(new float3(-vn.y, vn.x, 0));
            }
            else
            {
                return normalize(new float3(-vn.z, 0, vn.x));
            }
        }

        // Calculates quaternion required to rotate v1 into v2; v1 and v2 need not be normalised
        public static quaternion forRotation(float3 v1, float3 v2)
        {
            float k = sqrt(lengthsq(v1) * lengthsq(v2));

            float d = clamp(dot(v1, v2), -k, k);

            if (k < epsilon)
            {
                // Test for zero-length input, to avoid infinite loop.  Return identity (not much else to do)
                return Unity.Mathematics.quaternion.identity;
            }
            else if (abs(d + k) < epsilon * k)
            {
                // Test if v1 and v2 were antiparallel, to avoid singularity
                float3 m = orthogonal(v1);
                quaternion q1 = forRotation(m, v2);
                quaternion q2 = forRotation(v1, m);
                return math.mul(q1, q2);
            }
            else
            {
                // This means that xyz is k.sin(theta),
                // where a is the unit axis of rotation,
                // which equals 2k.sin(theta/2)cos(theta/2).
                float3 v = cross(v1, v2);

                // We then put 2kcos^2(theta/2) =
                // dot+k into the w part of the
                // quaternion and normalize.
                return normalize(new quaternion(v.x, v.y, v.z, d + k));
            }
        }

        public static int truncToInt(float value)
        {
            return (int)value;
        }

        public static Vector3 Convert(float3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static float3 Convert(Vector3 v)
        {
            return new float3(v.x, v.y, v.z);
        }

        internal static Quaternion Convert(quaternion q)
        {
            return new Quaternion(
                q.value.x, q.value.y,
                q.value.z, q.value.w);
        }

        public static quaternion Convert(Quaternion q)
        {
            return new quaternion(q.x, q.y, q.z, q.w);
        }

        public static AffineTransform Convert(Transform transform)
        {
            return new AffineTransform(transform.position, transform.rotation);
        }

        internal static Vector3 cartesianToSpherical(Vector3 cartesian)
        {
            float x = cartesian.x;
            float y = cartesian.y;
            float z = cartesian.z;

            float radius = Mathf.Sqrt(x * x + y * y + z * z);
            float theta = Mathf.Atan2(y, x);
            float phi = Mathf.Acos(z / radius);

            Assert.IsTrue(theta >= -Mathf.PI && theta <= Mathf.PI);
            Assert.IsTrue(phi >= 0.0f && phi <= Mathf.PI);

            return new Vector3(radius, theta, phi);
        }

        internal static Vector3 sphericalToCartesian(Vector3 spherical)
        {
            float radius = spherical.x;
            float theta = spherical.y;
            float phi = spherical.z;

            float x = Mathf.Cos(theta) * Mathf.Cos(phi) * radius;
            float y = Mathf.Sin(theta) * Mathf.Cos(phi) * radius;
            float z = Mathf.Sin(phi) * radius;

            return new Vector3(x, y, z);
        }

        internal static float mean(float min, float max)
        {
            return (min + max) * 0.5f;
        }

        internal static Vector3 mean(Vector3 min, Vector3 max)
        {
            return new Vector3(
                mean(min.x, max.x),
                mean(min.y, max.y),
                mean(min.z, max.z));
        }

        internal static float standardDeviation(float min, float max)
        {
            Assert.IsTrue(max >= min);
            float r = max - min;
            if (r <= Mathf.Epsilon)
                return 1.0f;
            return r * 0.5f;
        }

        internal static Vector3 standardDeviation(Vector3 min, Vector3 max)
        {
            return new Vector3(
                standardDeviation(min.x, max.x),
                standardDeviation(min.y, max.y),
                standardDeviation(min.z, max.z));
        }

        internal static float safeAngle(float3 lhs, float3 rhs)
        {
            float lengthLhs = length(lhs);
            float lengthRhs = length(rhs);

            if (lengthLhs < epsilon || lengthRhs < epsilon)
            {
                return 0.0f;
            }
            else
            {
                float3 normalizedLhs = lhs * rcp(lengthLhs);
                float3 normalizedRhs = rhs * rcp(lengthRhs);

                return acos(dot(normalizedLhs, normalizedRhs));
            }
        }

        internal static float inverseAbs(float x)
        {
            return 1.0f - min(1.0f, abs(x));
        }

        // theta is 'reference point', i.e. [-1;theta;+1]
        internal static float inverseAbsHat(float x, float theta)
        {
            Assert.IsTrue(x >= -1.0f && x <= 1.0f);
            Assert.IsTrue(theta >= -1.0f && theta <= 1.0f);

            if (abs(x - theta) <= 0.001f)
            {
                return 1.0f;
            }

            if (x < theta)
            {
                float z = (theta - x) / (theta + 1.0f);

                Assert.IsTrue(z >= 0.0f && z <= 1.0f);

                return inverseAbs(z);
            }
            else
            {
                float z = (x - theta) / (1.0f - theta);

                Assert.IsTrue(z >= 0.0f && z <= 1.0f);

                return inverseAbs(z);
            }
        }
        #endregion // Unity.Mathematics

        //https://github.com/Unity-Technologies/DOTSSample/blob/5a8230597a8c4b999b278a63844c5238dacf51b6/Assets/Unity.Sample.Core/Scripts/Utils/MathHelper.cs
        #region Unity.Sample.Core
        // Collection of converted classic Unity (Mathf, Vector3 etc.) + some homegrown math functions using Unity.Mathematics
        const float kEpsilonNormalSqrt = 1e-15F;
        // TODO: Should likely be platform dependent, maybe a value in unity.math?
        const float kEpisilon = 1.17549435E-38f;

#if INCLUDEMATHCHECKS
        [ConfigVar(Name = "math.show.comparison", DefaultValue = "1", Description = "Show old vs new math comparison")]
        public static ConfigVar CompareMath;
#endif

        public static uint hash(uint i)
        {
            return i * 0x83B58237u + 0xA9D919BFu;
        }

        public static uint hash(int i)
        {
            return (uint)i * 0x83B58237u + 0xA9D919BFu;
        }

        // Sign function that has the old Mathf behaviour of returning 1 if f == 0
        static float MathfStyleZeroIsOneSign(float f)
        {
            return f >= 0F ? 1F : -1F;
        }

        public static float SignedAngle(float a, float b)
        {
            var difference = b - a;
            var sign = math.sign(difference);
            var offset = sign * 180.0f;

            return ((difference + offset) % 360.0f) - offset;
        }

        public static float SignedAngle(float3 from, float3 to, float3 axis)
        {
            float unsignedAngle = Angle(from, to);
            float sign = MathfStyleZeroIsOneSign(math.dot(axis, math.cross(from, to)));
            var result = unsignedAngle * sign;

#if INCLUDEMATHCHECKS
            var oldMath = Vector3.SignedAngle(from, to, axis);
            if (math.abs(oldMath - result) > 0.1f)
                GameDebug.Log(WorldId.Undefined, CompareMath, "MathHelper.SignedAngle: Result not within tolerance! {0} : {1}", oldMath, result);
#endif
            return result;
        }

        public static float Angle(float3 from, float3 to)
        {
            float result;

            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            float denominator = math.sqrt(math.lengthsq(from) * math.lengthsq(to));

            if (Hint.Unlikely(denominator < kEpsilonNormalSqrt))
            {
                result = 0F;
            }
            else
            {
                float dot = math.clamp(math.dot(from, to) / denominator, -1F, 1F);
                result = math.degrees(math.acos(dot));
            }

#if INCLUDEMATHCHECKS
            var oldMath = Vector3.Angle(from, to);
            if (math.abs(oldMath - result) > 0.1f)
                GameDebug.Log(WorldId.Undefined, CompareMath, "MathHelper.Angle: Result not within tolerance! {0} : {1}", oldMath, result);
#endif
            return result;
        }

        public static float MoveTowards(float current, float target, float maxDelta)
        {
            float result;

            if (math.abs(target - current) <= maxDelta)
                result = target;
            else
                result = current + MathfStyleZeroIsOneSign(target - current) * maxDelta;

#if INCLUDEMATHCHECKS
            var oldMath = Mathf.MoveTowards(current, target, maxDelta);
            if (math.abs(oldMath - result) > 0.00001f)
                GameDebug.Log(WorldId.Undefined, CompareMath, "MathHelper.MoveTowards: Result not within tolerance! {0} : {1}", oldMath, result);
#endif
            return result;
        }

        public static float LerpAngle(float a, float b, float t)
        {
            float delta = Repeat((b - a), 360);
            if (delta > 180)
                delta -= 360;
            var result = a + delta * math.clamp(t, 0f, 1f);

#if INCLUDEMATHCHECKS
            var oldMath = Mathf.LerpAngle(a, b, t);
            if (math.abs(oldMath - result) > 0.00001f)
                GameDebug.Log(WorldId.Undefined, CompareMath, "MathHelper.LerpAngle: Result not within tolerance! {0} : {1}", oldMath, result);
#endif
            return result;
        }

        public static float Repeat(float t, float length)
        {
            return math.clamp(t - math.floor(t / length) * length, 0.0f, length);
        }

        public static float DeltaAngle(float current, float target)
        {
            float delta = Repeat((target - current), 360.0F);
            if (delta > 180.0F)
                delta -= 360.0F;
            var result = delta;

#if INCLUDEMATHCHECKS
            var oldMath = Mathf.DeltaAngle(current, target);
            if (math.abs(oldMath - result) > 0.00001f)
                GameDebug.Log(WorldId.Undefined, CompareMath, "MathHelper.DeltaAngle: Result not within tolerance! {0} : {1}", oldMath, result);
#endif
            return result;
        }

        public static float SmoothDampAngle(
            float current,
            float target,
            ref float currentVelocity,
            float smoothTime,
            float maxSpeed,
            float deltaTime)
        {
#if INCLUDEMATHCHECKS
            var currentVelocityDebug = currentVelocity;
#endif

            target = current + DeltaAngle(current, target);
            var result = SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);

#if INCLUDEMATHCHECKS
            // TODO: (sunek) Double check that this is atually a copy!?
            var oldMath = Mathf.SmoothDampAngle(current, target, ref currentVelocityDebug, smoothTime, maxSpeed, deltaTime);

            if (math.abs(oldMath - result) > 0.00001f)
                GameDebug.Log(WorldId.Undefined, CompareMath, "MathHelper.SmoothDampAngle: Result not within tolerance! {0} : {1}", oldMath, result);

            if (math.abs(currentVelocity - currentVelocityDebug) > 0.00001f)
                GameDebug.Log(WorldId.Undefined, CompareMath, "MathHelper.SmoothDampAngle: Current Velocity not within tolerance! {0} : {1}", currentVelocityDebug, currentVelocity);
#endif

            return result;
        }

        public static float SmoothDamp
        (
            float current,
            float target,
            ref float currentVelocity,
            float smoothTime,
            float maxSpeed,
            float deltaTime)
        {
#if INCLUDEMATHCHECKS
            var currentVelocityDebug = currentVelocity;
#endif

            // Based on Game Programming Gems 4 Chapter 1.10
            smoothTime = math.max(0.0001F, smoothTime);
            float omega = 2F / smoothTime;

            float x = omega * deltaTime;
            float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);
            float change = current - target;
            float originalTo = target;

            // Clamp maximum speed
            float maxChange = maxSpeed * smoothTime;
            change = math.clamp(change, -maxChange, maxChange);
            target = current - change;

            float temp = (currentVelocity + omega * change) * deltaTime;
            currentVelocity = (currentVelocity - omega * temp) * exp;
            float result = target + (change + temp) * exp;

            // Prevent overshooting
            if (originalTo - current > 0.0F == result > originalTo)
            {
                result = originalTo;
                currentVelocity = (result - originalTo) / deltaTime;
            }

#if INCLUDEMATHCHECKS
            var oldMath = Mathf.SmoothDamp(current, target, ref currentVelocityDebug, smoothTime, maxSpeed, deltaTime);

            if (math.abs(oldMath - result) > 0.00001f)
                GameDebug.Log(WorldId.Undefined, CompareMath, "MathHelper.SmoothDamp: Result not within tolerance! {0} : {1}", oldMath, result);

            if (math.abs(currentVelocity - currentVelocityDebug) > 0.00001f)
                GameDebug.Log(WorldId.Undefined, CompareMath, "MathHelper.SmoothDamp: Current Velocity not within tolerance! {0} : {1}", currentVelocityDebug, currentVelocity);
#endif
            return result;
        }

        // Projects a vector onto another vector.
        public static float3 Project(float3 vector, float3 onNormal)
        {
            float3 result;

            float sqrMag = math.dot(onNormal, onNormal);
            if (Hint.Unlikely(sqrMag < kEpisilon))
                result = Unity.Mathematics.float3.zero;
            else
                result = onNormal * math.dot(vector, onNormal) / sqrMag;

#if INCLUDEMATHCHECKS
            var oldMath = Vector3.Project(vector, onNormal);
            if (oldMath != (Vector3)result)
            {
                GameDebug.Log(WorldId.Undefined, CompareMath, "MathHelper.Project: Result not within tolerance! {0} : {1}", oldMath, result);
            }
#endif
            return result;
        }

        /*
        public static float3 ProjectOnPlane(float3 vector, float3 planeNormal)
        {
            var result = vector - Project(vector, planeNormal);

#if INCLUDEMATHCHECKS
            var oldMath = Vector3.ProjectOnPlane(vector, planeNormal);
            if (oldMath != (Vector3)result)
            {
                GameDebug.Log(WorldId.Undefined, CompareMath, "MathHelper.ProjectOnPlane: Result not within tolerance! {0} : {1}", oldMath, result);
            }
#endif
            return result;
        }
        */

        public static float2 ClampMagnitude(float2 vector, float maxLength)
        {
            if (math.lengthsq(vector) > maxLength * maxLength)
                return math.normalizesafe(vector) * maxLength;
            return vector;
        }

        public static float2 SmoothDamp(
            float2 current,
            float2 target,
            ref float2 currentVelocity,
            float smoothTime,
            float maxSpeed,
            float deltaTime)
        {
#if INCLUDEMATHCHECKS
            Vector2 currentVelocityDebug = currentVelocity;
#endif
            // Based on Game Programming Gems 4 Chapter 1.10
            smoothTime = math.max(0.0001F, smoothTime);
            float omega = 2F / smoothTime;

            float x = omega * deltaTime;
            float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);
            float2 change = current - target;
            float2 originalTo = target;

            // Clamp maximum speed
            float maxChange = maxSpeed * smoothTime;
            change = ClampMagnitude(change, maxChange);
            target = current - change;

            float2 temp = (currentVelocity + omega * change) * deltaTime;
            currentVelocity = (currentVelocity - omega * temp) * exp;
            float2 output = target + (change + temp) * exp;

            // Prevent overshooting
            if (math.dot(originalTo - current, output - originalTo) > 0)
            {
                output = originalTo;
                currentVelocity = (output - originalTo) / deltaTime;
            }

#if INCLUDEMATHCHECKS
            // TODO: (sunek) Double check that this is atually a copy!?
            var oldMath = Vector2.SmoothDamp(current, target, ref currentVelocityDebug, smoothTime, maxSpeed, deltaTime);

            if (oldMath != (Vector2)output)
                GameDebug.Log(WorldId.Undefined, CompareMath, "MathHelper.SmoothDampVector2: Result not within tolerance! {0} : {1}", oldMath, output);

            if ((Vector2)currentVelocity != currentVelocityDebug)
                GameDebug.Log(WorldId.Undefined, CompareMath, "MathHelper.SmoothDampVector2: Current Velocity not within tolerance! {0} : {1}", currentVelocityDebug, currentVelocity);
#endif
            return output;
        }
#endregion // Unity.Sample.Core

        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Tests/Editor/GPUDriven/GPUDrivenRenderingUtils.cs
        #region UnityEngine.Rendering.Tests
        public static uint4 UnpackUintTo4x8Bit(this uint val)
        {
            return new uint4(val & 0xFF, (val >> 8) & 0xFF, (val >> 16) & 0xFF, (val >> 24) & 0xFF);
        }
        #endregion // UnityEngine.Rendering.Tests
        
        //https://github.com/Unity-Technologies/megacity-metro/blob/13069724080c2aacc89b735206a7af1c9df81b51/Assets/Scripts/Utils/Misc/MathUtilities.cs
        #region Utils.Misc
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetSharpnessInterpolant(this float sharpness, float dt)
        {
            return saturate(1f - exp(-sharpness * dt));
        }

        public static float GetDampingInterpolant(this float damping, float dt)
        {
            if (damping != 0f)
            {
                return GetSharpnessInterpolant(1f / damping, dt);
            }

            return 1f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngleRadiansToDotRatio(this float angleRadians)
        {
            return cos(angleRadians);
        }

        public static float GetConeRadiusAtLength(this float length, float coneAngleRadians)
        {
            return tan(coneAngleRadians) * length;
        }

        public static float3 SmoothFollow(this float3 currentSelf, float3 prevTarget, float3 newTarget, float dt, float sharpness)
        {
            float scaledDeltaTime = sharpness * dt;
            if (scaledDeltaTime != 0f)
            {
                float3 smoothingOffsetFromTargetDisplacement = -(newTarget - prevTarget) / scaledDeltaTime;
                float3 smoothingOffsetFromDistanceToTarget = (currentSelf - prevTarget - smoothingOffsetFromTargetDisplacement) * exp(-scaledDeltaTime);
                float3 smoothingOffset = smoothingOffsetFromTargetDisplacement + smoothingOffsetFromDistanceToTarget;
                return newTarget + smoothingOffset;
            }

            return currentSelf;
        }
        #endregion // Utils.Misc
        
        //https://github.com/Unity-Technologies/com.unity.cloud.gltfast/blob/4516607ef01664e48949f37c995e36bc5d413a1f/Packages/com.unity.cloud.gltfast/Runtime/Scripts/Mathematics.cs
        #region GLTFast
        /// <summary>
        /// Decomposes a 4x4 TRS matrix into separate transforms (translation * rotation * scale)
        /// Matrix may not contain skew
        /// </summary>
        /// <param name="m">Input matrix</param>
        /// <param name="translation">Translation</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="scale">Scale</param>
        public static void Decompose(
            this UnityEngine.Matrix4x4 m,
            out UnityEngine.Vector3 translation,
            out UnityEngine.Quaternion rotation,
            out UnityEngine.Vector3 scale
            )
        {
            translation = new UnityEngine.Vector3(m.m03, m.m13, m.m23);
            var mRotScale = new float3x3(
                m.m00, m.m01, m.m02,
                m.m10, m.m11, m.m12,
                m.m20, m.m21, m.m22
                );

            mRotScale.Decompose(out var mRotation, out var mScale);
            rotation = mRotation;
            scale = new UnityEngine.Vector3(mScale.x, mScale.y, mScale.z);
        }

        /// <summary>
        /// Decomposes a 4x4 TRS matrix into separate transforms (translation * rotation * scale)
        /// Matrix may not contain skew
        /// </summary>
        /// <param name="m">Input matrix</param>
        /// <param name="translation">Translation</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="scale">Scale</param>
        public static void Decompose(
            this float4x4 m,
            out float3 translation,
            out quaternion rotation,
            out float3 scale
            )
        {
            var mRotScale = new float3x3(
                m.c0.xyz,
                m.c1.xyz,
                m.c2.xyz
                );

            mRotScale.Decompose(out rotation, out scale);
            translation = m.c3.xyz;
        }

        /// <summary>
        /// Decomposes a 3x3 matrix into rotation and scale
        /// </summary>
        /// <param name="m">Input matrix</param>
        /// <param name="rotation">Rotation quaternion values</param>
        /// <param name="scale">Scale</param>
        static void Decompose(this float3x3 m, out quaternion rotation, out float3 scale)
        {
            var lenC0 = length(m.c0);
            var lenC1 = length(m.c1);
            var lenC2 = length(m.c2);

            float3x3 rotationMatrix;
            rotationMatrix.c0 = m.c0 / lenC0;
            rotationMatrix.c1 = m.c1 / lenC1;
            rotationMatrix.c2 = m.c2 / lenC2;

            scale.x = lenC0;
            scale.y = lenC1;
            scale.z = lenC2;

            if (rotationMatrix.IsNegative())
            {
                rotationMatrix *= -1f;
                scale *= -1f;
            }

            // Inlined normalize(rotationMatrix)
            rotationMatrix.c0 = normalize(rotationMatrix.c0);
            rotationMatrix.c1 = normalize(rotationMatrix.c1);
            rotationMatrix.c2 = normalize(rotationMatrix.c2);

            rotation = new quaternion(rotationMatrix);
        }

        static bool IsNegative(this float3x3 m)
        {
            var cross = math.cross(m.c0, m.c1);
            return dot(cross, m.c2) < 0f;
        }

        /// <summary>
        /// Normalizes a vector
        /// </summary>
        /// <param name="input">Input vector</param>
        /// <param name="output">Normalized output vector</param>
        /// <returns>Length/magnitude of input vector</returns>
        public static float Normalize(this float2 input, out float2 output)
        {
            var len = length(input);
            output = input / len;
            return len;
        }
        #endregion // GLTFast
        
        //https://github.com/Unity-Technologies/com.unity.formats.alembic/blob/3d486c22f22d65278f910f0835128afdb8f2a36e/com.unity.formats.alembic/Runtime/Scripts/Misc/RuntimeUtils.cs
        #region UnityEngine.Formats.Alembic.Importer
        public static ulong CombineHash(this ulong h1, ulong h2)
        {
            unchecked
            {
                return h1 ^ h2 + 0x9e3779b9 + (h1 << 6) + (h1 >> 2); // Similar to c++ boost::hash_combine
            }
        }
        #endregion // UnityEngine.Formats.Alembic.Importer
        
        //https://github.com/Unity-Technologies/com.unity.demoteam.hair/blob/75a7f446209896bc1bce0da2682cfdbdf30ce447/Runtime/Utility/AffineUtility.cs
        #region Unity.DemoTeam.Hair
        public static void AffineInterpolateUpper3x3(ref this float3x3 A, float4 q, float t,
            out float3x3 affineInterpolateUpper3x3)
        {
            static float3x3 lerp(float3x3 a, float3x3 b, float t) => float3x3(
                math.lerp(a.c0, b.c0, t),
                math.lerp(a.c1, b.c1, t),
                math.lerp(a.c2, b.c2, t));

            // A = QR
            // Q^-1 A = R

            float3x3 Q_inv = float3x3(conjugate(q));
            float3x3 R = math.mul(Q_inv, A);
            float3x3 I = float3x3(
                1.0f, 0.0f, 0.0f,
                0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 1.0f);

            float3x3 Q_t = float3x3(slerp(Unity.Mathematics.quaternion.identity, q, t));
            float3x3 R_t = lerp(I, R, t);
            affineInterpolateUpper3x3 = math.mul(Q_t, R_t); // A_t
        }

        public static void AffineInterpolate3x4(ref this float3x4 M, float4 q, float t,
            out float3x4 affineInterpolate3x4)
        {
            // M = | A T |

            var A = float3x3(M.c0, M.c1, M.c2);
            AffineInterpolateUpper3x3(ref A, q, t, out float3x3 A_t);
            float3 T_t = M.c3 * t;

            affineInterpolate3x4 = float3x4(
                A_t.c0.x, A_t.c1.x, A_t.c2.x, T_t.x,
                A_t.c0.y, A_t.c1.y, A_t.c2.y, T_t.y,
                A_t.c0.z, A_t.c1.z, A_t.c2.z, T_t.z);
        }

        public static void AffineInterpolate4x4(ref this float4x4 M, float4 q, float t,
            out float4x4 affineInterpolate4x4)
        {
            // M = | A T |
            //     | 0 1 |

            var M_3x3 = (float3x3)M;
            AffineInterpolateUpper3x3(ref M_3x3, q, t, out float3x3 A_t);
            float3 T_t = M.c3.xyz * t;

            affineInterpolate4x4 = float4x4(
                A_t.c0.x, A_t.c1.x, A_t.c2.x, T_t.x,
                A_t.c0.y, A_t.c1.y, A_t.c2.y, T_t.y,
                A_t.c0.z, A_t.c1.z, A_t.c2.z, T_t.z,
                0.0f, 0.0f, 0.0f, 1.0f);
        }

        public static void AffineInverseUpper3x3(ref this float3x3 A, out float3x3 affineInverseUpper3x3)
        {
            float3 c0 = A.c0;
            float3 c1 = A.c1;
            float3 c2 = A.c2;

            float3 cp0x1 = cross(c0, c1);
            float3 cp1x2 = cross(c1, c2);
            float3 cp2x0 = cross(c2, c0);

            affineInverseUpper3x3 = float3x3(cp1x2, cp2x0, cp0x1) / dot(c0, cp1x2);
        }

        public static void AffineInverse4x4(ref this float4x4 M, out float4x4 affineInverse4x4)
        {
            // | A T |
            // | 0 1 |

            var M_3x3 = (float3x3)M;
            AffineInverseUpper3x3(ref M_3x3, out float3x3 A_inv);
            float3 T_inv = -math.mul(A_inv, M.c3.xyz);

            affineInverse4x4 = float4x4(
                A_inv.c0.x, A_inv.c1.x, A_inv.c2.x, T_inv.x,
                A_inv.c0.y, A_inv.c1.y, A_inv.c2.y, T_inv.y,
                A_inv.c0.z, A_inv.c1.z, A_inv.c2.z, T_inv.z,
                0.0f, 0.0f, 0.0f, 1.0f);
        }

        public static void AffineMul3x4(ref this float3x4 Ma, ref float3x4 Mb, out float3x4 affineMul3x4)
        {
            // Ma x Mb  =  | A Ta |  x  | B Tb |
            //             | 0 1  |     | 0 1  |
            //
            //          =  | mul(A,B)  mul(A,Tb)+Ta |
            //             | 0         1            |

            float3x3 A = float3x3(Ma.c0, Ma.c1, Ma.c2);
            float3x3 B = float3x3(Mb.c0, Mb.c1, Mb.c3);

            float3x3 AB = math.mul(A, B);
            float3 ATb = math.mul(A, Mb.c3);
            float3 Ta = Ma.c3;

            affineMul3x4 = float3x4(
                AB.c0.x, AB.c1.x, AB.c2.x, ATb.x + Ta.x,
                AB.c0.y, AB.c1.y, AB.c2.y, ATb.y + Ta.y,
                AB.c0.z, AB.c1.z, AB.c2.z, ATb.z + Ta.z);
        }

        public static void AffineMul4x4(ref this float4x4 a, ref float4x4 b, out float4x4 affineMul4x4)
        {
            affineMul4x4 = float4x4(
                a.c0 * b.c0.x + a.c1 * b.c0.y + a.c2 * b.c0.z + a.c3 * b.c0.w,
                a.c0 * b.c1.x + a.c1 * b.c1.y + a.c2 * b.c1.z + a.c3 * b.c1.w,
                a.c0 * b.c2.x + a.c1 * b.c2.y + a.c2 * b.c2.z + a.c3 * b.c2.w,
                a.c0 * b.c3.x + a.c1 * b.c3.y + a.c2 * b.c3.z + a.c3 * b.c3.w);
        }
        #endregion // Unity.DemoTeam.Hair

        //https://github.com/Unity-Technologies/InputSystem/blob/36a93fe84a95a380be438412258a5305fcdfc740/Packages/com.unity.inputsystem/InputSystem/Utilities/NumberHelpers.cs
        #region UnityEngine.InputSystem.Utilities
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(this double a, double b)
        {
            return abs(b - a) < max(1E-06 * max(abs(a), abs(b)), double.Epsilon * 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(this float a, float b)
        {
            return abs(b - a) < max(1E-06 * max(abs(a), abs(b)), double.Epsilon * 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(this float3 a, float3 b)
        {
            for (int i = 0; i < 3; i++)
            {
                if (!a[i].Approximately(b[i]))
                    return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float IntToNormalizedFloat(this int value, int minValue, int maxValue)
        {
            if (value <= minValue)
                return 0.0f;

            if (value >= maxValue)
                return 1.0f;

            // using double here because int.MaxValue is not representable in floats
            // as int.MaxValue = 2147483647 will become 2147483648.0 when cast to a float
            return (float)(((double)value - minValue) / ((double)maxValue - minValue));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NormalizedFloatToInt(this float value, int intMinValue, int intMaxValue)
        {
            if (value <= 0.0f)
                return intMinValue;

            if (value >= 1.0f)
                return intMaxValue;

            return (int)(value * ((double)intMaxValue - intMinValue) + intMinValue);
        }
        #endregion // UnityEngine.InputSystem.Utilities
        
        //https://github.com/Unity-Technologies/sentis-samples/blob/526fbb4e2e6767afe347cd3393becd0e3e64ae2b/BlazeDetectionSample/Face/Assets/Scripts/BlazeUtils.cs
        #region BlazeUtils
        public static void RotationMatrix(this float theta, out float2x3 rotationMatrix)
        {
            sincos(theta, out var sinTheta, out var cosTheta);
            rotationMatrix = new float2x3(
                cosTheta, -sinTheta, 0,
                sinTheta, cosTheta, 0
            );
        }

        public static void TranslationMatrix(this float2 delta, out float2x3 translationMatrix)
        {
            translationMatrix = new float2x3(
                1, 0, delta.x,
                0, 1, delta.y
            );
        }

        public static void ScaleMatrix(this float2 scale, out float2x3 scaleMatrix)
        {
            scaleMatrix = new float2x3(
                scale.x, 0, 0,
                0, scale.y, 0
            );
        }
        #endregion // BlazeUtils
        
        //https://github.com/Unity-Technologies/ECSGalaxySample/blob/84f9bec931de73f76731f230d126e0d348b6065c/Assets/Scripts/Utilities/MathUtilities.cs
        #region MathUtilities
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ProjectOnPlane(this float3 vector, float3 onPlaneNormal)
        {
            return vector - projectsafe(vector, onPlaneNormal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ClampToMaxLength(this float3 vector, float maxLength)
        {
            float sqrMag = lengthsq(vector);
            if (sqrMag > maxLength * maxLength)
            {
                float mag = sqrt(sqrMag);
                float normalizedX = vector.x / mag;
                float normalizedY = vector.y / mag;
                float normalizedZ = vector.z / mag;
                return new float3(
                    normalizedX * maxLength,
                    normalizedY * maxLength,
                    normalizedZ * maxLength);
            }

            return vector;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 RandomInSphere(ref Random random, float radius)
        {
            float3 v = random.NextFloat3Direction();
            v *= pow(random.NextFloat(), 1.0f / 3.0f);
            return v * radius;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(this float val, float2 bounds)
        {
            return clamp(val, bounds.x, bounds.y);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SegmentIntersectsSphere(float3 p1, float3 p2, float3 sphereCenter, float sphereRadius)
        {
            float distanceSqToSphereCenter; // = float.MaxValue;
            float segmentLengthSq = distancesq(p1, p2);
            if (segmentLengthSq == 0.0)
            {
                distanceSqToSphereCenter = distancesq(sphereCenter, p1);
            }
            else
            {
                float t = max(0f, min(1f, dot(sphereCenter - p1, p2 - p1) / segmentLengthSq));
                float3 projection = p1 + t * (p2 - p1);
                distanceSqToSphereCenter = distancesq(sphereCenter, projection);
            }

            return distanceSqToSphereCenter <= sphereRadius * sphereRadius;
        }

        public static void GenerateEquidistantPointsOnSphere(ref NativeList<float3> points, int newPointsCount, float radius,
            int repelIterations = 50)
        {
            GenerateEquidistantPointsOnSphereJobHandle(ref points, newPointsCount, radius,
                repelIterations).Complete();
        }

        public static JobHandle GenerateEquidistantPointsOnSphereJobHandle(ref NativeList<float3> points, int newPointsCount, float radius,
            int repelIterations = 50)
        {
            int initialPointsCount = points.Length;
            int totalPointsCount = initialPointsCount + newPointsCount;

            // Set the Length instead of the Capacity because it will be passed as a NativeArray
            points.Length = totalPointsCount;

            // First pass: generate points around the sphere in a semiregular distribution
            float goldenRatio = 1 + (sqrt(5f) / 4f);
            float angleIncrement = PI2 * goldenRatio;

            var addPoints = new AddPointsJob
            {
                initialPointsCount = initialPointsCount,
                totalPointsCount = totalPointsCount,
                angleIncrement = angleIncrement,
                radius = radius,
                points = points.AsArray(),
            };

            var jobHandle = addPoints.ScheduleParallel(totalPointsCount - initialPointsCount,
                innerloopBatchCount: 32, dependency: default);

            // Second pass: make points repel each other
            if (totalPointsCount > 1)
            {
                var job = new GenerateEquidistantPointsOnSphereJob
                {
                    points = points,
                    radius = radius,
                };

                jobHandle = job.Schedule(repelIterations, jobHandle);
            }

            return jobHandle;
        }
        #endregion // MathUtilities

        [BurstCompile(FloatMode = FloatMode.Fast)]
        public struct AddPointsJob : IJobFor
        {
            [ReadOnly] public int initialPointsCount;
            [ReadOnly] public float totalPointsCount;
            [ReadOnly] public float angleIncrement;
            [ReadOnly] public float radius;
            [WriteOnly] public NativeArray<float3> points;

            public void Execute(int index)
            {
                int i = index + initialPointsCount;

                float distance = i / totalPointsCount;
                float incline = acos(mad(-2f, distance, 1f));
                float azimuth = angleIncrement * i;

                sincos(incline, out float sinIncline, out float cosIncline);
                sincos(azimuth, out float sinAzimuth, out float cosAzimuth);

                float3 point;
                point.x = sinIncline * cosAzimuth * radius;
                point.y = sinIncline * sinAzimuth * radius;
                point.z = cosIncline * radius;

                points[i] = point;
            }
        }

        [BurstCompile(FloatMode = FloatMode.Fast)]
        public struct GenerateEquidistantPointsOnSphereJob : IJobFor
        {
            public NativeList<float3> points;
            [ReadOnly] public float radius;

            public void Execute(int index)
            {
                const float repelAngleIncrements = PI * 0.01f;

                for (int a = 0; a < points.Length; a++)
                {
                    float3 dir = normalizesafe(points[a]);
                    float closestPointRemappedDot = 0f;
                    float3 closestPointRotationAxis = default;

                    for (int b = 0; b < a; b++)
                    {
                        float3 otherDir = normalizesafe(points[b]);
                        ClosestPoint(dir, otherDir, ref closestPointRemappedDot, ref closestPointRotationAxis);
                    }

                    for (int b = a + 1; b < points.Length; b++)
                    {
                        float3 otherDir = normalizesafe(points[b]);
                        ClosestPoint(dir, otherDir, ref closestPointRemappedDot, ref closestPointRotationAxis);
                    }

                    quaternion repelRotation = Unity.Mathematics.quaternion.AxisAngle(closestPointRotationAxis, repelAngleIncrements);
                    dir = rotate(repelRotation, dir);
                    points[a] = dir * radius;
                }
            }

            static void ClosestPoint(in float3 dir, in float3 otherDir,
                ref float closestPointRemappedDot, ref float3 closestPointRotationAxis)
            {
                float dot = math.dot(dir, otherDir);
                float remappedDot = remap(-1f, 1f, 0f, 1f, dot);

                if (remappedDot > closestPointRemappedDot)
                {
                    closestPointRemappedDot = remappedDot;
                    closestPointRotationAxis = -normalizesafe(cross(dir, otherDir));
                }
            }
        }
    }
}
