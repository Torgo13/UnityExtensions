using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace UnityExtensions.Packages
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Union16
    {
        [FieldOffset(0)] public UnityExtensions.Union16 Union16_0;

        [FieldOffset(0)] public half Half_0;
    }
    
    [StructLayout(LayoutKind.Explicit)]
    public struct Union32
    {
        [FieldOffset(0)] public UnityExtensions.Union32 Union32_0;

        [FieldOffset(0)] public half2 Half2_0;


        [FieldOffset(0)] public half Half_0;
        [FieldOffset(2)] public half Half_1;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Union128
    {
        [FieldOffset(0)] public UnityExtensions.Union128 Union128_0;

        [FieldOffset(0)] public float4 Float4_0;

        [FieldOffset(0)] public int4 Int4_0;


        [FieldOffset(0)] public float3 Float3_0;

        [FieldOffset(0)] public int3 Int3_0;


        [FieldOffset(0)] public float2 Float2_0;
        [FieldOffset(8)] public float2 Float2_1;

        [FieldOffset(0)] public int2 Int2_0;
        [FieldOffset(8)] public int2 Int2_1;
    }
    
    public static class MathematicsExtensions
    {
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
            int initialPointsCount = points.Length;
            int totalPointsCount = initialPointsCount + newPointsCount;

            // First pass: generate points around the sphere in a semiregular distribution
            float goldenRatio = 1 + (sqrt(5f) / 4f);
            float angleIncrement = PI2 * goldenRatio;

            points.Capacity = totalPointsCount;

            var addPoints = new AddPointsJob
            {
                initialPointsCount = initialPointsCount,
                totalPointsCount = totalPointsCount,
                angleIncrement = angleIncrement,
                radius = radius,
                points = points.AsParallelWriter(),
            };

            var AddPointsHandle = addPoints.Schedule(totalPointsCount - initialPointsCount, dependency: default);

            // Second pass: make points repel each other
            if (totalPointsCount > 1)
            {
                var job = new GenerateEquidistantPointsOnSphereJob
                {
                    points = points,
                    radius = radius,
                };

                var jobHandle = job.Schedule(repelIterations, AddPointsHandle);
                jobHandle.Complete();
                return;
            }

            AddPointsHandle.Complete();
        }
        #endregion // MathUtilities

        [BurstCompile]
        public struct AddPointsJob : IJobFor
        {
            [ReadOnly] public int initialPointsCount;
            [ReadOnly] public float totalPointsCount;
            [ReadOnly] public float angleIncrement;
            [ReadOnly] public float radius;
            [WriteOnly] public NativeList<float3>.ParallelWriter points;

            public void Execute(int i)
            {
                i += initialPointsCount;

                float distance = i / totalPointsCount;
                float incline = acos(1f - (2f * distance));
                float azimuth = angleIncrement * i;

                sincos(incline, out var sinIncline, out var cosIncline);
                sincos(azimuth, out var sinAzimuth, out var cosAzimuth);

                float3 point = new float3
                {
                    x = sinIncline * cosAzimuth * radius,
                    y = sinIncline * sinAzimuth * radius,
                    z = cosIncline * radius,
                };

                points.AddNoResize(point);
            }
        }

        [BurstCompile]
        public struct GenerateEquidistantPointsOnSphereJob : IJobFor
        {
            public NativeList<float3> points;
            [ReadOnly] public float radius;

            public void Execute(int r)
            {
                const float repelAngleIncrements = PI * 0.01f;

                for (int a = 0; a < points.Length; a++)
                {
                    float3 dir = normalizesafe(points[a]);
                    float closestPointRemappedDot = 0f;
                    float3 closestPointRotationAxis = default;

                    for (int b = 0; b < points.Length; b++)
                    {
                        if (b != a)
                        {
                            float3 otherDir = normalizesafe(points[b]);

                            float dot = math.dot(dir, otherDir);
                            float remappedDot = remap(-1f, 1f, 0f, 1f, dot);

                            if (remappedDot > closestPointRemappedDot)
                            {
                                closestPointRemappedDot = remappedDot;
                                closestPointRotationAxis = -normalizesafe(cross(dir, otherDir));
                            }
                        }
                    }

                    quaternion repelRotation = Unity.Mathematics.quaternion.AxisAngle(closestPointRotationAxis, repelAngleIncrements);
                    dir = rotate(repelRotation, dir);
                    points[a] = dir * radius;
                }
            }
        }
    }
}
