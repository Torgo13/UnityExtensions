// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools.Utils;
using Unity.Collections;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace UnityExtensions.Packages.Tests
{
    class MathematicsExtensionsTests
    {
        [Test]
        public void Union_SizeOf()
        {
            Assert.AreEqual(2, Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<Union2>());
            Assert.AreEqual(4, Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<Union4>());
            Assert.AreEqual(8, Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<Union8>());
            Assert.AreEqual(16, Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<Union16>());
        }

        static Vector3EqualityComparer s_Vector3Comparer;
        static QuaternionEqualityComparer s_QuaternionComparer;

        static Matrix4x4 s_UnityMatrix;
        static float4x4 s_Matrix;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            s_Vector3Comparer = new Vector3EqualityComparer(10e-6f);
            s_QuaternionComparer = new QuaternionEqualityComparer(10e-6f);

            // Corner case matrix (90°/0°/45° rotation with -1/-1/-1 scale)
            s_UnityMatrix = new Matrix4x4(
                new Vector4(
                    -0.7071067811865474f,
                    0f,
                    -0.7071067811865477f,
                    0f
                ),
                new Vector4(
                    0.7071067811865477f,
                    0f,
                    -0.7071067811865474f,
                    0f
                ),
                new Vector4(
                    0f,
                    1f,
                    0f,
                    0f
                ),
                new Vector4(
                    0f,
                    0f,
                    0f,
                    1f
                )
            );

            s_Matrix = new float4x4(
                s_UnityMatrix.m00, s_UnityMatrix.m01, s_UnityMatrix.m02, s_UnityMatrix.m03,
                s_UnityMatrix.m10, s_UnityMatrix.m11, s_UnityMatrix.m12, s_UnityMatrix.m13,
                s_UnityMatrix.m20, s_UnityMatrix.m21, s_UnityMatrix.m22, s_UnityMatrix.m23,
                s_UnityMatrix.m30, s_UnityMatrix.m31, s_UnityMatrix.m32, s_UnityMatrix.m33
            );
        }

        [Test]
        public void MatrixDecomposeTest()
        {
            Profiler.BeginSample("Matrix4x4.DecomposeUnity");
            if (s_UnityMatrix.ValidTRS())
            {
                // ReSharper disable UnusedVariable
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                var t1 = new Vector3(s_UnityMatrix.m03, s_UnityMatrix.m13, s_UnityMatrix.m23);
                var r1 = s_UnityMatrix.rotation;
                var s1 = s_UnityMatrix.lossyScale;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                // ReSharper restore UnusedVariable
            }

            Profiler.EndSample();

            Profiler.BeginSample("Matrix4x4.DecomposeCustom");
            s_UnityMatrix.Decompose(out var t, out var r, out var s);
            Profiler.EndSample();

            Assert.That(t, Is.EqualTo(new Vector3(0, 0, 0)).Using(s_Vector3Comparer));
            Assert.That(r, Is.EqualTo(
                new Quaternion(0.65328151f, -0.270598054f, 0.270598054f, 0.65328151f))
                .Using(s_QuaternionComparer)
            );
            Assert.That(s, Is.EqualTo(new Vector3(-.99999994f, -.99999994f, -1)).Using(s_Vector3Comparer));

            Profiler.BeginSample("float4x4.Decompose");
            s_Matrix.Decompose(out var t3, out quaternion r3, out var s3);
            Profiler.EndSample();

            Assert.That((Vector3)t3, Is.EqualTo(new Vector3(0, 0, 0)).Using(s_Vector3Comparer));
            Assert.That(
                (Quaternion)r3,
                Is.EqualTo(new Quaternion(0.65328151f, -0.270598054f, 0.270598054f, 0.65328151f))
                    .Using(s_QuaternionComparer)
                );
            Assert.That(
                (Vector3)s3,
                Is.EqualTo(new Vector3(-.99999994f, -.99999994f, -1))
                    .Using(s_Vector3Comparer)
                );
        }

        //https://github.com/Unity-Technologies/InputSystem/blob/develop/Assets/Tests/InputSystem/Utilities/NumberHelpersTests.cs
        #region InputSystem

        [Test]
        [Category("Utilities")]
        // out of boundary tests
        [TestCase(-1, 0, 1, 0.0f)]
        [TestCase(2, 0, 1, 1.0f)]
        // [0, 1]
        [TestCase(0, 0, 1, 0.0f)]
        [TestCase(1, 0, 1, 1.0f)]
        // [-128, 127]
        [TestCase(-128, sbyte.MinValue, sbyte.MaxValue, 0.0f)]
        [TestCase(0, sbyte.MinValue, sbyte.MaxValue, 0.501960813999176025391f)]
        [TestCase(127, sbyte.MinValue, sbyte.MaxValue, 1.0f)]
        // [0, 255]
        [TestCase(0, byte.MinValue, byte.MaxValue, 0.0f)]
        [TestCase(128, byte.MinValue, byte.MaxValue, 0.501960813999176025391f)]
        [TestCase(255, byte.MinValue, byte.MaxValue, 1.0f)]
        // [-32768, 32767]
        [TestCase(-32768, short.MinValue, short.MaxValue, 0.0f)]
        [TestCase(0, short.MinValue, short.MaxValue, 0.50000762939453125f)]
        [TestCase(32767, short.MinValue, short.MaxValue, 1.0f)]
        // [0, 65535]
        [TestCase(0, ushort.MinValue, ushort.MaxValue, 0.0f)]
        [TestCase(32767, ushort.MinValue, ushort.MaxValue, 0.49999237060546875f)]
        [TestCase(65535, ushort.MinValue, ushort.MaxValue, 1.0f)]
        // [-2147483648, 2147483647]
        [TestCase(-2147483648, int.MinValue, int.MaxValue, 0.0f)]
        [TestCase(0, int.MinValue, int.MaxValue, 0.5f)]
        [TestCase(2147483647, int.MinValue, int.MaxValue, 1.0f)]
        public void Utilities_NumberHelpers_CanConvertIntToNormalizedFloatAndBack(int value, int minValue, int maxValue, float expected)
        {
            var result = MathematicsExtensions.IntToNormalizedFloat(value, minValue, maxValue);
            Assert.That(result, Is.EqualTo(expected).Within(float.Epsilon));

            var integer = MathematicsExtensions.NormalizedFloatToInt(result, minValue, maxValue);
            Assert.That(integer, Is.EqualTo(Mathf.Clamp(value, minValue, maxValue)));
        }

        #endregion // InputSystem

        //https://github.com/Unity-Technologies/com.unity.formats.alembic/blob/3d486c22f22d65278f910f0835128afdb8f2a36e/com.unity.formats.alembic/Runtime/Scripts/Misc/RuntimeUtils.cs
        #region UnityEngine.Formats.Alembic.Importer
        /// <summary>
        /// Deterministic Results:
        /// Ensures that the hash function produces the same output for identical inputs, validating its reliability.
        /// </summary>
        [Test]
        public void CombineHash_CreatesDeterministicHash()
        {
            ulong h1 = 123456789;
            ulong h2 = 987654321;

            ulong result1 = MathematicsExtensions.CombineHash(h1, h2);
            ulong result2 = MathematicsExtensions.CombineHash(h1, h2);

            Assert.AreEqual(result1, result2, "Hash should be deterministic for the same inputs");
        }

        /// <summary>
        /// Order Sensitivity:
        /// Verifies that the order of h1 and h2 affects the hash result.
        /// </summary>
        [Test]
        public void CombineHash_IsSensitiveToOrder()
        {
            ulong h1 = 123456789;
            ulong h2 = 987654321;

            ulong result1 = MathematicsExtensions.CombineHash(h1, h2);
            ulong result2 = MathematicsExtensions.CombineHash(h2, h1);

            Assert.AreNotEqual(result1, result2, "Hash should be sensitive to the order of inputs");
        }

        /// <summary>
        /// Uniqueness:
        /// Tests that the function generates different hashes for varying inputs.
        /// </summary>
        [Test]
        public void CombineHash_ProducesUniqueHashes()
        {
            ulong h1 = 123456789;
            ulong h2 = 987654321;
            ulong h3 = 555555555;

            ulong result1 = MathematicsExtensions.CombineHash(h1, h2);
            ulong result2 = MathematicsExtensions.CombineHash(h1, h3);

            Assert.AreNotEqual(result1, result2, "Hash should produce unique values for different inputs");
        }

        /// <summary>
        /// Comparison to Boost:
        /// Validates the similarity to boost::hash_combine by using an equivalent calculation and checking for consistency.
        /// </summary>
        [Test]
        public void CombineHash_ConsistencyWithBoostHashCombine()
        {
            ulong h1 = 123456789;
            ulong h2 = 987654321;

            // Expected result calculated using boost::hash_combine equivalent logic
            ulong expected = h1 ^ h2 + 0x9e3779b9 + (h1 << 6) + (h1 >> 2);

            ulong result = MathematicsExtensions.CombineHash(h1, h2);

            Assert.AreEqual(expected, result, "CombineHash should match the behavior of boost::hash_combine");
        }
        #endregion // UnityEngine.Formats.Alembic.Importer

        //https://github.com/Unity-Technologies/com.unity.demoteam.hair/blob/75a7f446209896bc1bce0da2682cfdbdf30ce447/Runtime/Utility/AffineUtility.cs
        #region Unity.DemoTeam.Hair
        /// <summary>
        /// Tests the interpolation of float3x3 matrices using quaternions and linear interpolation.
        /// </summary>
        [Test]
        public void AffineInterpolateUpper3x3_LerpsCorrectly()
        {
            float3x3 A = math.float3x3(
                1.0f, 0.0f, 0.0f,
                0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 1.0f
            );
            float4 q = new float4(0, 0, 0, 1);
            float t = 0.5f;

            MathematicsExtensions.AffineInterpolateUpper3x3(ref A, q, t, out float3x3 result);

            Assert.AreEqual(A, result, "Interpolation with identity quaternion failed");
        }

        /// <summary>
        /// Tests the interpolation of float3x4 matrices using quaternions and linear interpolation.
        /// </summary>
        [Test]
        public void AffineInterpolate3x4_LerpsTranslationCorrectly()
        {
            float3x4 M = math.float3x4(
                1.0f, 0.0f, 0.0f, 2.0f,
                0.0f, 1.0f, 0.0f, 3.0f,
                0.0f, 0.0f, 1.0f, 4.0f
            );
            float4 q = new float4(0, 0, 0, 1);
            float t = 0.5f;

            MathematicsExtensions.AffineInterpolate3x4(ref M, q, t, out float3x4 result);

            Assert.AreEqual(1.0f, result.c3.x, "Translation interpolation failed on X");
            Assert.AreEqual(1.5f, result.c3.y, "Translation interpolation failed on Y");
            Assert.AreEqual(2.0f, result.c3.z, "Translation interpolation failed on Z");
        }

        /// <summary>
        /// Tests the interpolation of float4x4 matrices using quaternions and linear interpolation.
        /// </summary>
        [Test]
        public void AffineInterpolate4x4_CreatesInterpolatedMatrix()
        {
            float4x4 M = math.float4x4(
                1.0f, 0.0f, 0.0f, 2.0f,
                0.0f, 1.0f, 0.0f, 3.0f,
                0.0f, 0.0f, 1.0f, 4.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            );
            float4 q = new float4(0, 0, 0, 1);
            float t = 0.5f;

            MathematicsExtensions.AffineInterpolate4x4(ref M, q, t, out float4x4 result);

            Assert.AreEqual(1.0f, result.c3.x, "Translation interpolation failed on X in 4x4");
            Assert.AreEqual(1.5f, result.c3.y, "Translation interpolation failed on Y in 4x4");
            Assert.AreEqual(2.0f, result.c3.z, "Translation interpolation failed on Z in 4x4");
        }

        /// <summary>
        /// Ensures that the inverse for a 3x3 affine transformation is calculated accurately.
        /// </summary>
        [Test]
        public void AffineInverseUpper3x3_CalculatesInverseCorrectly()
        {
            float3x3 A = math.float3x3(
                2.0f, 0.0f, 0.0f,
                0.0f, 2.0f, 0.0f,
                0.0f, 0.0f, 2.0f
            );

            MathematicsExtensions.AffineInverseUpper3x3(ref A, out float3x3 result);

            float3x3 expected = math.float3x3(
                0.5f, 0.0f, 0.0f,
                0.0f, 0.5f, 0.0f,
                0.0f, 0.0f, 0.5f
            );

            Assert.AreEqual(expected, result, "Affine inverse calculation failed for 3x3 matrix");
        }

        /// <summary>
        /// Ensures that the inverse for a 4x4 affine transformation is calculated accurately.
        /// </summary>
        [Test]
        public void AffineInverse4x4_CalculatesFullInverse()
        {
            float4x4 M = math.float4x4(
                2.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 2.0f, 0.0f, 2.0f,
                0.0f, 0.0f, 2.0f, 3.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            );

            MathematicsExtensions.AffineInverse4x4(ref M, out float4x4 result);

            float4x4 expected = math.float4x4(
                0.5f, 0.0f, 0.0f, -0.5f,
                0.0f, 0.5f, 0.0f, -1.0f,
                0.0f, 0.0f, 0.5f, -1.5f,
                0.0f, 0.0f, 0.0f, 1.0f
            );

            Assert.AreEqual(expected, result, "Affine inverse calculation failed for 4x4 matrix");
        }

        /// <summary>
        /// Validates the correctness of affine matrix multiplication.
        /// </summary>
        [Test]
        public void AffineMul4x4_MultipliesCorrectly()
        {
            float4x4 Ma = math.float4x4(
                1.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 2.0f,
                0.0f, 0.0f, 1.0f, 3.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            );

            float4x4 Mb = math.float4x4(
                2.0f, 0.0f, 0.0f, 0.0f,
                0.0f, 2.0f, 0.0f, 0.0f,
                0.0f, 0.0f, 2.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            );

            MathematicsExtensions.AffineMul4x4(ref Ma, ref Mb, out float4x4 result);

            float4x4 expected = math.float4x4(
                2.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 2.0f, 0.0f, 2.0f,
                0.0f, 0.0f, 2.0f, 3.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            );

            Assert.AreEqual(expected, result, "Affine multiplication failed for 4x4 matrices");
        }
        #endregion // Unity.DemoTeam.Hair

        //https://github.com/Unity-Technologies/InputSystem/blob/36a93fe84a95a380be438412258a5305fcdfc740/Packages/com.unity.inputsystem/InputSystem/Utilities/NumberHelpers.cs
        #region UnityEngine.InputSystem.Utilities
        /// <summary>
        /// Tests whether two double values are approximately equal using tolerances derived from relative and absolute differences.
        /// </summary>
        [Test]
        public void Approximately_WhenValuesAreClose_ReturnsTrue()
        {
            double a = 0.001;
            double b = 0.001000001;
            bool result = MathematicsExtensions.Approximately(a, b);

            Assert.IsTrue(result, "Approximately should return true for values close enough");
        }

        [Test]
        public void Approximately_WhenValuesAreNotClose_ReturnsFalse()
        {
            double a = 0.001;
            double b = 0.01;
            bool result = MathematicsExtensions.Approximately(a, b);

            Assert.IsFalse(result, "Approximately should return false for values too far apart");
        }

        /// <summary>
        /// Ensures proper mapping of integer values into the normalized range [0.0, 1.0].
        /// </summary>
        [Test]
        public void IntToNormalizedFloat_WhenValueIsMinimum_ReturnsZero()
        {
            int value = 10;
            int minValue = 10;
            int maxValue = 20;
            float result = MathematicsExtensions.IntToNormalizedFloat(value, minValue, maxValue);

            Assert.AreEqual(0.0f, result, "Conversion from minimum int value to normalized float failed");
        }

        [Test]
        public void IntToNormalizedFloat_WhenValueIsMaximum_ReturnsOne()
        {
            int value = 20;
            int minValue = 10;
            int maxValue = 20;
            float result = MathematicsExtensions.IntToNormalizedFloat(value, minValue, maxValue);

            Assert.AreEqual(1.0f, result, "Conversion from maximum int value to normalized float failed");
        }

        [Test]
        public void IntToNormalizedFloat_WhenValueIsMidRange_ReturnsCorrectNormalizedValue()
        {
            int value = 15;
            int minValue = 10;
            int maxValue = 20;
            float result = MathematicsExtensions.IntToNormalizedFloat(value, minValue, maxValue);

            Assert.AreEqual(0.5f, result, "Conversion from mid-range int value to normalized float failed");
        }

        /// <summary>
        /// Confirms that normalized float values are correctly converted back into integers within the specified range.
        /// </summary>
        [Test]
        public void NormalizedFloatToInt_WhenValueIsZero_ReturnsMinimum()
        {
            float value = 0.0f;
            int intMinValue = 10;
            int intMaxValue = 20;
            int result = MathematicsExtensions.NormalizedFloatToInt(value, intMinValue, intMaxValue);

            Assert.AreEqual(intMinValue, result, "Conversion from zero normalized float to int failed");
        }

        [Test]
        public void NormalizedFloatToInt_WhenValueIsOne_ReturnsMaximum()
        {
            float value = 1.0f;
            int intMinValue = 10;
            int intMaxValue = 20;
            int result = MathematicsExtensions.NormalizedFloatToInt(value, intMinValue, intMaxValue);

            Assert.AreEqual(intMaxValue, result, "Conversion from one normalized float to int failed");
        }

        [Test]
        public void NormalizedFloatToInt_WhenValueIsMidRange_ReturnsCorrectIntValue()
        {
            float value = 0.5f;
            int intMinValue = 10;
            int intMaxValue = 20;
            int result = MathematicsExtensions.NormalizedFloatToInt(value, intMinValue, intMaxValue);

            Assert.AreEqual(15, result, "Conversion from mid-range normalized float to int failed");
        }
        #endregion // UnityEngine.InputSystem.Utilities

        //https://github.com/Unity-Technologies/sentis-samples/blob/526fbb4e2e6767afe347cd3393becd0e3e64ae2b/BlazeDetectionSample/Face/Assets/Scripts/BlazeUtils.cs
        #region BlazeUtils
        /// <summary>
        /// Tests that the matrix correctly represents a rotation for the given angle (theta).
        /// </summary>
        [Test]
        public void RotationMatrix_CreatesCorrectRotation()
        {
            float theta = math.PI / 4; // 45 degrees
            MathematicsExtensions.RotationMatrix(theta, out float2x3 rotationMatrix);

            float2x3 expected = new float2x3(
                math.sqrt(0.5f), -math.sqrt(0.5f), 0,
                math.sqrt(0.5f), math.sqrt(0.5f), 0
            );

            Assert.AreEqual(expected, rotationMatrix, "Rotation matrix is incorrect");
        }

        /// <summary>
        /// Validates that the matrix applies the given translation correctly.
        /// </summary>
        [Test]
        public void TranslationMatrix_CreatesCorrectTranslation()
        {
            float2 delta = new float2(3, 4);
            MathematicsExtensions.TranslationMatrix(delta, out float2x3 translationMatrix);

            float2x3 expected = new float2x3(
                1, 0, 3,
                0, 1, 4
            );

            Assert.AreEqual(expected, translationMatrix, "Translation matrix is incorrect");
        }

        /// <summary>
        /// Ensures the scale matrix accurately applies the scaling transformations to coordinates.
        /// </summary>
        [Test]
        public void ScaleMatrix_CreatesCorrectScaling()
        {
            float2 scale = new float2(2, 3);
            MathematicsExtensions.ScaleMatrix(scale, out float2x3 scaleMatrix);

            float2x3 expected = new float2x3(
                2, 0, 0,
                0, 3, 0
            );

            Assert.AreEqual(expected, scaleMatrix, "Scaling matrix is incorrect");
        }
        #endregion // BlazeUtils

        //https://github.com/Unity-Technologies/ECSGalaxySample/blob/84f9bec931de73f76731f230d126e0d348b6065c/Assets/Scripts/Utilities/MathUtilities.cs
        #region MathUtilities
        [Test]
        public void ProjectOnPlane_Test()
        {
            float3 vector = new float3(1, 1, 1);
            float3 planeNormal = new float3(0, 1, 0); // Y-axis
            float3 result = MathematicsExtensions.ProjectOnPlane(vector, planeNormal);

            Assert.AreEqual(new float3(1, 0, 1), result, "Projection on plane failed");
        }

        [Test]
        public void ClampToMaxLength_WhenShorterThanMax_ReturnsSameVector()
        {
            float3 vector = new float3(1, 2, 2);
            float maxLength = 5f;
            float3 result = MathematicsExtensions.ClampToMaxLength(vector, maxLength);

            Assert.AreEqual(vector, result, "Clamping changed a vector that was already within bounds");
        }

        [Test]
        public void ClampToMaxLength_WhenLongerThanMax_ClampsCorrectly()
        {
            float3 vector = new float3(3, 4, 0); // Length = 5
            float maxLength = 4f;
            float3 result = MathematicsExtensions.ClampToMaxLength(vector, maxLength);            

            Assert.IsTrue(MathematicsExtensions.Approximately(new float3(2.4f, 3.2f, 0f), result), "Clamping did not correctly shorten the vector");
        }

        [Test]
        public void GetSharpnessInterpolant_Test()
        {
            float sharpness = 5f;
            float dt = 0.1f;
            float result = MathematicsExtensions.GetSharpnessInterpolant(sharpness, dt);

            Assert.IsTrue(result > 0 && result <= 1, "Sharpness interpolant is outside the expected range");
        }

        [Test]
        public void RandomInSphere_GeneratesPointsWithinSphere()
        {
            Random random = new Random(1234);
            float radius = 1f;
            float3 point = MathematicsExtensions.RandomInSphere(ref random, radius);

            Assert.IsTrue(math.length(point) <= radius, "Point is outside the sphere");
        }

        [Test]
        public void SegmentIntersectsSphere_IntersectionDetected()
        {
            float3 p1 = new float3(1, 0, 0);
            float3 p2 = new float3(-1, 0, 0);
            float3 sphereCenter = new float3(0, 0, 0);
            float sphereRadius = 0.5f;

            bool intersects = MathematicsExtensions.SegmentIntersectsSphere(p1, p2, sphereCenter, sphereRadius);

            Assert.IsTrue(intersects, "Segment intersection was not detected");
        }

        [Test]
        public void SegmentIntersectsSphere_NoIntersection()
        {
            float3 p1 = new float3(1, 0, 0);
            float3 p2 = new float3(2, 0, 0);
            float3 sphereCenter = new float3(0, 0, 0);
            float sphereRadius = 0.5f;

            bool intersects = MathematicsExtensions.SegmentIntersectsSphere(p1, p2, sphereCenter, sphereRadius);

            Assert.IsFalse(intersects, "False positive for segment-sphere intersection");
        }

        [Test]
        public void Clamp_WithinBounds()
        {
            float val = 5f;
            float2 bounds = new float2(0, 10);
            float result = MathematicsExtensions.Clamp(val, bounds);

            Assert.AreEqual(val, result, "Clamp altered a value within bounds");
        }

        [Test]
        public void Clamp_OutOfBounds()
        {
            float val = -5f;
            float2 bounds = new float2(0, 10);
            float result = MathematicsExtensions.Clamp(val, bounds);

            Assert.AreEqual(bounds.x, result, "Clamp failed to restrict a value below the lower bound");

            val = 15f;
            result = MathematicsExtensions.Clamp(val, bounds);

            Assert.AreEqual(bounds.y, result, "Clamp failed to restrict a value above the upper bound");
        }

        [Test]
        public void GenerateEquidistantPointsOnSphere_CorrectPointCount()
        {
            NativeList<float3> points = new NativeList<float3>(Allocator.TempJob);
            int initialCount = points.Length;
            int newPointsCount = 10;
            float radius = 1f;

            MathematicsExtensions.GenerateEquidistantPointsOnSphere(ref points, newPointsCount, radius);

            Assert.AreEqual(initialCount + newPointsCount, points.Length, "Point generation count is incorrect");

            // Does not search every pair of points
            var distance = Vector3.Distance(points[0], points[1]);
            Assert.AreNotEqual(Vector3.zero, distance, "Points should not be in the same position");

            for (int i = 2; i < initialCount; i++)
            {
                Assert.AreEqual(distance, Vector3.Distance(points[i], points[i - 1]), "Points are not equidistant");
            }

            points.Dispose();
        }
        #endregion // MathUtilities
    }
}