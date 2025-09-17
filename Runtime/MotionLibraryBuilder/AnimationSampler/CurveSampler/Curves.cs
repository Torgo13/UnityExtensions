using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

#if INCLUDE_MATHEMATICS
using Unity.Mathematics;
#else
using PKGE.Mathematics;
#endif // INCLUDE_MATHEMATICS

namespace PKGE.Packages
{
    public readonly struct Curve : IDisposable
    {
        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Editor/MotionLibraryBuilder/AnimationSampler/CurveSampler/Editor/Curves.cs
        #region Unity.Curves
        private const int TRUE = 1;
        private const int FALSE = 1;
        public readonly NativeArray<Keyframe> Keys;
        private readonly int owner;

        public Curve(AnimationCurve c, Allocator alloc)
        {
            Keys = new NativeArray<Keyframe>(c.keys, alloc);
            owner = TRUE;
        }

        public Curve(int size, Allocator alloc)
        {
            Keys = new NativeArray<Keyframe>(size, alloc);
            owner = TRUE;
        }

        public Curve(Keyframe[] keyframes, Allocator alloc)
        {
            Keys = new NativeArray<Keyframe>(keyframes, alloc);
            owner = TRUE;
        }

        public Curve(NativeArray<Keyframe> keyframes, Allocator alloc)
        {
            Keys = new NativeArray<Keyframe>(keyframes, alloc);
            owner = TRUE;
        }

        public Curve(NativeArray<Keyframe> keyframes)
        {
            Keys = keyframes;
            owner = FALSE;
        }

        public Curve(Curve other)
        {
            Keys = other.Keys;
            owner = FALSE;
        }

        public Curve(Curve other, Allocator alloc)
        {
            Keys = new NativeArray<Keyframe>(other.Keys, alloc);
            owner = TRUE;
        }

        public void Dispose()
        {
            if (owner != 0)
            {
                Keys.Dispose();
            }
        }

        public float Evaluate(float time)
        {
            CurveSampling.ThreadSafe.Evaluate(Keys, time, out float result);
            return result;
        }

        public int Length => Keys.Length;
        public float Duration => Keys[Length - 1].time - Keys[0].time;
        #endregion // Unity.Curves
    }

    public struct CurveData
    {
        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Editor/MotionLibraryBuilder/AnimationSampler/CurveSampler/Editor/Curves.cs
        #region Unity.Curves
        public CurveData(Curve curve, Allocator alloc)
        {
            array = curve.Keys;
            size = curve.Length;
            allocatorLabel = alloc;
        }

        public static CurveData CreateInvalid()
        {
            return new CurveData()
            {
                array = default,
                size = 0,
                allocatorLabel = Allocator.Invalid
            };
        }

        public bool IsValid => array.IsCreated;

        public Keyframe this[int index] => array[index];

        public readonly Curve ToCurve()
        {
            NativeArray<Keyframe> keys = array;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.SetAtomicSafetyHandle(
                ref keys, Unity.Collections.LowLevel.Unsafe.AtomicSafetyHandle.Create());
#endif // ENABLE_UNITY_COLLECTIONS_CHECKS

            return new Curve(keys);
        }

        public NativeArray<Keyframe> array;
        public int size;
        public Allocator allocatorLabel;
        #endregion // Unity.Curves
    }

    [Unity.Burst.BurstCompile]
    public static class CurveSampling
    {
        //https://github.com/needle-mirror/com.unity.kinematica/blob/d5ae562615dab42e9e395479d5e3b4031f7dccaf/Editor/MotionLibraryBuilder/AnimationSampler/CurveSampler/Editor/Curves.cs
        #region Unity.Curves
        const float DefaultWeight = 0;

        [Unity.Burst.BurstCompile]
        public static class ThreadSafe
        {
            [Unity.Burst.BurstCompile]
            public static void Evaluate(in NativeArray<Keyframe> keys, float curveT,
                out float result)
            {
                EvaluateWithinRange(keys, curveT, 0, keys.Length - 1, out result);
            }

            [Unity.Burst.BurstCompile]
            public static void EvaluateWithHint(in NativeArray<Keyframe> keys, float curveT, ref int hintIndex,
                out float result)
            {
                int startIndex = 0;
                int endIndex = keys.Length - 1;
                if (endIndex <= hintIndex)
                {
                    result = keys[hintIndex].value;
                    return;
                }

                // wrap time
                curveT = math.clamp(curveT, keys[hintIndex].time, keys[endIndex].time);
                FindIndexForSampling(keys, curveT, startIndex, endIndex, hintIndex, out int lhsIndex, out int rhsIndex);

                Keyframe lhs = keys[hintIndex];
                Keyframe rhs = keys[rhsIndex];
                InterpolateKeyframe(lhs, rhs, curveT, out result);
            }

            [Unity.Burst.BurstCompile]
            public static void EvaluateWithinRange(in NativeArray<Keyframe> keys, float curveT, int startIndex,
                int endIndex, out float result)
            {
                if (endIndex <= startIndex)
                {
                    result = keys[startIndex].value;
                    return;
                }

                // wrap time
                curveT = math.clamp(curveT, keys[startIndex].time, keys[endIndex].time);
                FindIndexForSampling(keys, curveT, startIndex, endIndex, -1, out int lhsIndex, out int rhsIndex);

                Keyframe lhs = keys[lhsIndex];
                Keyframe rhs = keys[rhsIndex];
                InterpolateKeyframe(lhs, rhs, curveT, out result);
            }

            [Unity.Burst.BurstCompile]
            static private void FindIndexForSampling(in NativeArray<Keyframe> keys, float curveT, int start, int end, int hint,
                out int lhs, out int rhs)
            {
                if (hint != -1)
                {
                    hint = math.clamp(hint, start, end);
                    // We can not use the cache time or time end since that is in unwrapped time space!
                    float time = keys[hint].time;

                    if (curveT > time)
                    {
                        const int kMaxLookahead = 3;
                        for (int i = 0; i < kMaxLookahead; i++)
                        {
                            int index = hint + i;
                            if (index + 1 < end && keys[index + 1].time > curveT)
                            {
                                lhs = index;
                                rhs = math.min(lhs + 1, end);
                                return;
                            }
                        }
                    }
                }

                // Fall back to using binary search
                // upper bound (first value larger than curveT)
                int __len = end - start;
                int __half;
                int __middle;
                int __first = start;
                while (__len > 0)
                {
                    __half = __len >> 1;
                    __middle = __first + __half;

                    var mid = keys[__middle];
                    if (curveT < mid.time)
                        __len = __half;
                    else
                    {
                        __first = __middle;
                        ++__first;
                        __len = __len - __half - 1;
                    }
                }

                // If not within range, we pick the last element twice
                lhs = __first - 1;
                rhs = math.min(end, __first);
            }

            [Unity.Burst.BurstCompile]
            public static void InterpolateKeyframe(in Keyframe lhs, in Keyframe rhs, float curveT,
                out float output)
            {
                if ((lhs.weightedMode & WeightedMode.Out) != 0 || (rhs.weightedMode & WeightedMode.In) != 0)
                    BezierInterpolate(curveT, lhs, rhs, out output);
                else
                    HermiteInterpolate(curveT, lhs, rhs, out output);

                HandleSteppedCurve(lhs, rhs, ref output);
            }

            [Unity.Burst.BurstCompile]
            static void HermiteInterpolate(float curveT, in Keyframe lhs, in Keyframe rhs,
                out float result)
            {
                float dx = rhs.time - lhs.time;
                float m1;
                float m2;
                float t;
                if (dx != 0.0F)
                {
                    t = (curveT - lhs.time) / dx;
                    m1 = lhs.outTangent * dx;
                    m2 = rhs.inTangent * dx;
                }
                else
                {
                    t = 0.0F;
                    m1 = 0;
                    m2 = 0;
                }

                result = HermiteInterpolate(t, lhs.value, m1, m2, rhs.value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float HermiteInterpolate(float t, float p0, float m0, float m1, float p1)
            {
                float t2 = t * t;
                float t3 = t2 * t;

                float a = 2.0F * t3 - 3.0F * t2 + 1.0F;
                float b = t3 - 2.0F * t2 + t;
                float c = t3 - t2;
                float d = -2.0F * t3 + 3.0F * t2;

                return a * p0 + b * m0 + c * m1 + d * p1;
            }

            [Unity.Burst.BurstCompile]
            static void BezierInterpolate(float curveT, in Keyframe lhs, in Keyframe rhs,
                out float result)
            {
                float lhsOutWeight = (lhs.weightedMode & WeightedMode.Out) != 0 ? lhs.outWeight : DefaultWeight;
                float rhsInWeight = (rhs.weightedMode & WeightedMode.In) != 0 ? rhs.inWeight : DefaultWeight;

                float dx = rhs.time - lhs.time;
                if (dx == 0.0F)
                    result = lhs.value;
                else
                    result = BezierInterpolate((curveT - lhs.time) / dx, lhs.value, lhs.outTangent * dx, lhsOutWeight,
                        rhs.value, rhs.inTangent * dx, rhsInWeight);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float FAST_CBRT_POSITIVE(float x)
            {
                return math.exp(math.log(x) / 3.0f);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float FAST_CBRT(float x)
            {
                return (((x) < 0) ? -math.exp(math.log(-(x)) / 3.0f) : math.exp(math.log(x) / 3.0f));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float BezierExtractU(float t, float w1, float w2)
            {
                float a = 3.0F * w1 - 3.0F * w2 + 1.0F;
                float b = -6.0F * w1 + 3.0F * w2;
                float c = 3.0F * w1;
                float d = -t;

                if (math.abs(a) > 1e-3f)
                {
                    float p = -b / (3.0F * a);
                    float p2 = p * p;
                    float p3 = p2 * p;

                    float q = p3 + (b * c - 3.0F * a * d) / (6.0F * a * a);
                    float q2 = q * q;

                    float r = c / (3.0F * a);
                    float rmp2 = r - p2;

                    float s = q2 + rmp2 * rmp2 * rmp2;

                    if (s < 0.0F)
                    {
                        float ssi = math.sqrt(-s);
                        float r_1 = math.sqrt(-s + q2);
                        float phi = math.atan2(ssi, q);

                        float r_3 = FAST_CBRT_POSITIVE(r_1);
                        float phi_3 = phi / 3.0F;

                        // Extract cubic roots.
                        float u1 = 2.0F * r_3 * math.cos(phi_3) + p;
                        float u2 = 2.0F * r_3 * math.cos(phi_3 + 2.0F * (float)math.PI / 3.0f) + p;
                        float u3 = 2.0F * r_3 * math.cos(phi_3 - 2.0F * (float)math.PI / 3.0f) + p;

                        if (u1 >= 0.0F && u1 <= 1.0F)
                            return u1;
                        else if (u2 >= 0.0F && u2 <= 1.0F)
                            return u2;
                        else if (u3 >= 0.0F && u3 <= 1.0F)
                            return u3;

                        // Aiming at solving numerical imprecision when u is outside [0,1].
                        return (t < 0.5F) ? 0.0F : 1.0F;
                    }
                    else
                    {
                        float ss = math.sqrt(s);
                        float u = FAST_CBRT(q + ss) + FAST_CBRT(q - ss) + p;

                        if (u >= 0.0F && u <= 1.0F)
                            return u;

                        // Aiming at solving numerical imprecision when u is outside [0,1].
                        return (t < 0.5F) ? 0.0F : 1.0F;
                    }
                }

                if (math.abs(b) > 1e-3f)
                {
                    float s = c * c - 4.0F * b * d;
                    float ss = math.sqrt(s);

                    float u1 = (-c - ss) / (2.0F * b);
                    float u2 = (-c + ss) / (2.0F * b);

                    if (u1 >= 0.0F && u1 <= 1.0F)
                        return u1;
                    else if (u2 >= 0.0F && u2 <= 1.0F)
                        return u2;

                    // Aiming at solving numerical imprecision when u is outside [0,1].
                    return (t < 0.5F) ? 0.0F : 1.0F;
                }

                if (math.abs(c) > 1e-3f)
                {
                    return (-d / c);
                }

                return 0.0F;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float BezierInterpolate(float t, float v1, float m1, float w1, float v2, float m2, float w2)
            {
                float u = BezierExtractU(t, w1, 1.0F - w2);
                return BezierInterpolate(u, v1, w1 * m1 + v1, v2 - w2 * m2, v2);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float BezierInterpolate(float t, float p0, float p1, float p2, float p3)
            {
                float t2 = t * t;
                float t3 = t2 * t;
                float omt = 1.0F - t;
                float omt2 = omt * omt;
                float omt3 = omt2 * omt;

                return omt3 * p0 + 3.0F * t * omt2 * p1 + 3.0F * t2 * omt * p2 + t3 * p3;
            }

            [Unity.Burst.BurstCompile]
            static void HandleSteppedCurve(in Keyframe lhs, in Keyframe rhs, ref float value)
            {
                if (float.IsInfinity(lhs.outTangent) || float.IsInfinity(rhs.inTangent))
                    value = lhs.value;
            }
        }
        #endregion // Unity.Curves
    }
}
