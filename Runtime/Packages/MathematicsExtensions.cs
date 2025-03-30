using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace UnityExtensions
{
    [StructLayout(LayoutKind.Explicit)]
    public struct PackedFloat
    {
        [FieldOffset(0)] public int Int0;

        [FieldOffset(0)] public uint UInt0;

        [FieldOffset(0)] public float Float0;

        [FieldOffset(0)] public short Short0;
        [FieldOffset(2)] public short Short1;

        [FieldOffset(0)] public ushort UShort0;
        [FieldOffset(2)] public ushort UShort1;

        [FieldOffset(0)] public half Half0;
        [FieldOffset(2)] public half Half1;

        [FieldOffset(0)] public byte Byte0;
        [FieldOffset(1)] public byte Byte1;
        [FieldOffset(2)] public byte Byte2;
        [FieldOffset(3)] public byte Byte3;
    }
    
    public static class MathematicsExtensions
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Tests/Editor/GPUDriven/GPUDrivenRenderingUtils.cs
        #region UnityEngine.Rendering.Tests
        public static uint4 UnpackUintTo4x8Bit(uint val)
        {
            return new uint4(val & 0xFF, (val >> 8) & 0xFF, (val >> 16) & 0xFF, (val >> 24) & 0xFF);
        }
        #endregion // UnityEngine.Rendering.Tests
        
        //https://github.com/Unity-Technologies/megacity-metro/blob/13069724080c2aacc89b735206a7af1c9df81b51/Assets/Scripts/Utils/Misc/MathUtilities.cs
        #region Utils.Misc
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetSharpnessInterpolant(float sharpness, float dt)
        {
            return saturate(1f - exp(-sharpness * dt));
        }

        public static float GetDampingInterpolant(float damping, float dt)
        {
            if (damping != 0f)
            {
                return GetSharpnessInterpolant(1f / damping, dt);
            }

            return 1f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngleRadiansToDotRatio(float angleRadians)
        {
            return cos(angleRadians);
        }

        public static float GetConeRadiusAtLength(float length, float coneAngleRadians)
        {
            return tan(coneAngleRadians) * length;
        }

        public static float3 SmoothFollow(float3 currentSelf, float3 prevTarget, float3 newTarget, float dt, float sharpness)
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
        public static float Normalize(float2 input, out float2 output)
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
        public static float3x3 AffineInterpolateUpper3x3(this float3x3 A, float4 q, float t)
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
            float3x3 A_t = math.mul(Q_t, R_t);

            return A_t;
        }

        public static float3x4 AffineInterpolate3x4(this float3x4 M, float4 q, float t)
        {
            // M = | A T |

            float3x3 A_t = AffineInterpolateUpper3x3(float3x3(M.c0, M.c1, M.c2), q, t);
            float3 T_t = M.c3 * t;

            return float3x4(
                A_t.c0.x, A_t.c1.x, A_t.c2.x, T_t.x,
                A_t.c0.y, A_t.c1.y, A_t.c2.y, T_t.y,
                A_t.c0.z, A_t.c1.z, A_t.c2.z, T_t.z);
        }

        public static float4x4 AffineInterpolate4x4(this float4x4 M, float4 q, float t)
        {
            // M = | A T |
            //     | 0 1 |

            float3x3 A_t = AffineInterpolateUpper3x3((float3x3)M, q, t);
            float3 T_t = M.c3.xyz * t;

            return float4x4(
                A_t.c0.x, A_t.c1.x, A_t.c2.x, T_t.x,
                A_t.c0.y, A_t.c1.y, A_t.c2.y, T_t.y,
                A_t.c0.z, A_t.c1.z, A_t.c2.z, T_t.z,
                0.0f, 0.0f, 0.0f, 1.0f);
        }

        public static float3x3 AffineInverseUpper3x3(this float3x3 A)
        {
            float3 c0 = A.c0;
            float3 c1 = A.c1;
            float3 c2 = A.c2;

            float3 cp0x1 = cross(c0, c1);
            float3 cp1x2 = cross(c1, c2);
            float3 cp2x0 = cross(c2, c0);

            return float3x3(cp1x2, cp2x0, cp0x1) / dot(c0, cp1x2);
        }

        public static float4x4 AffineInverse4x4(this float4x4 M)
        {
            // | A T |
            // | 0 1 |

            float3x3 A_inv = AffineInverseUpper3x3((float3x3)M);
            float3 T_inv = -math.mul(A_inv, M.c3.xyz);

            return float4x4(
                A_inv.c0.x, A_inv.c1.x, A_inv.c2.x, T_inv.x,
                A_inv.c0.y, A_inv.c1.y, A_inv.c2.y, T_inv.y,
                A_inv.c0.z, A_inv.c1.z, A_inv.c2.z, T_inv.z,
                0.0f, 0.0f, 0.0f, 1.0f);
        }

        public static float3x4 AffineMul3x4(this float3x4 Ma, float3x4 Mb)
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

            return float3x4(
                AB.c0.x, AB.c1.x, AB.c2.x, ATb.x + Ta.x,
                AB.c0.y, AB.c1.y, AB.c2.y, ATb.y + Ta.y,
                AB.c0.z, AB.c1.z, AB.c2.z, ATb.z + Ta.z);
        }
        #endregion // Unity.DemoTeam.Hair
        
        //https://github.com/Unity-Technologies/InputSystem/blob/36a93fe84a95a380be438412258a5305fcdfc740/Packages/com.unity.inputsystem/InputSystem/Utilities/NumberHelpers.cs
        #region UnityEngine.InputSystem.Utilities
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(double a, double b)
        {
            return abs(b - a) < max(1E-06 * max(abs(a), abs(b)), double.Epsilon * 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float IntToNormalizedFloat(int value, int minValue, int maxValue)
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
        public static int NormalizedFloatToInt(float value, int intMinValue, int intMaxValue)
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
        // matrix utility
        public static float2x3 mul(this float2x3 a, float2x3 b)
        {
            return new float2x3(
                a[0][0] * b[0][0] + a[1][0] * b[0][1],
                a[0][0] * b[1][0] + a[1][0] * b[1][1],
                a[0][0] * b[2][0] + a[1][0] * b[2][1] + a[2][0],
                a[0][1] * b[0][0] + a[1][1] * b[0][1],
                a[0][1] * b[1][0] + a[1][1] * b[1][1],
                a[0][1] * b[2][0] + a[1][1] * b[2][1] + a[2][1]
            );
        }

        public static float2 mul(this float2x3 a, float2 b)
        {
            return new float2(
                a[0][0] * b.x + a[1][0] * b.y + a[2][0],
                a[0][1] * b.x + a[1][1] * b.y + a[2][1]
            );
        }

        public static float2x3 RotationMatrix(this float theta)
        {
            sincos(theta, out var sinTheta, out var cosTheta);
            return new float2x3(
                cosTheta, -sinTheta, 0,
                sinTheta, cosTheta, 0
            );
        }

        public static float2x3 TranslationMatrix(this float2 delta)
        {
            return new float2x3(
                1, 0, delta.x,
                0, 1, delta.y
            );
        }

        public static float2x3 ScaleMatrix(this float2 scale)
        {
            return new float2x3(
                scale.x, 0, 0,
                0, scale.y, 0
            );
        }
        #endregion // BlazeUtils
        
        //https://github.com/Unity-Technologies/ECSGalaxySample/blob/84f9bec931de73f76731f230d126e0d348b6065c/Assets/Scripts/Utilities/MathUtilities.cs
        #region MathUtilities
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ProjectOnPlane(float3 vector, float3 onPlaneNormal)
        {
            return vector - projectsafe(vector, onPlaneNormal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ClampToMaxLength(float3 vector, float maxLength)
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
        public static float Clamp(float val, float2 bounds)
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

        public static void GenerateEquidistantPointsOnSphere(ref Unity.Collections.NativeList<float3> points, int newPointsCount, float radius,
            int repelIterations = 50)
        {
            int initialPointsCount = points.Length;
            int totalPointsCount = initialPointsCount + newPointsCount;

            // First pass: generate points around the sphere in a semiregular distribution
            float goldenRatio = 1 + (sqrt(5f) / 4f);
            float angleIncrement = PI * 2f * goldenRatio;
            for (int i = initialPointsCount; i < totalPointsCount; i++)
            {
                float distance = i / (float)totalPointsCount;
                float incline = acos(1f - (2f * distance));
                float azimuth = angleIncrement * i;

                float3 point = new float3
                {
                    x = sin(incline) * cos(azimuth) * radius,
                    y = sin(incline) * sin(azimuth) * radius,
                    z = cos(incline) * radius,
                };

                points.Add(point);
            }

            // Second pass: make points repel each other
            if (points.Length > 1)
            {
                const float repelAngleIncrements = PI * 0.01f;
                for (int r = 0; r < repelIterations; r++)
                {
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
        #endregion // MathUtilities
    }
}
