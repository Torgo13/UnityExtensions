// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools.Utils;
using Unity.Mathematics;

namespace UnityExtensions.Tests
{
    class MathematicsExtensionsTests
    {
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
    }
}