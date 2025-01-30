using NUnit.Framework;
using UnityEngine;
using UnityExtensions.Tests;

namespace UnityExtensions.Editor.Tests
{
    class Vector3ExtensionsTests
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Tests/Editor/XRCoreUtilities/Vector3ExtensionsTests.cs
        #region Unity.XR.CoreUtils.Editor.Tests
        const float k_Delta = 0.00000001f;

        [Test]
        public void MaxComponent_ReturnsMaxAxisValue()
        {
            var maxX = new Vector3(2f, 1f, 0f);
            Assert.AreEqual(maxX.MaxComponent(), maxX.x);
            var maxY = new Vector3(0f, 2f, 1f);
            Assert.AreEqual(maxY.MaxComponent(), maxY.y);
            var maxZ = new Vector3(0f, 1f, 2f);
            Assert.AreEqual(maxZ.MaxComponent(), maxZ.z);
        }

        [Test]
        public void Inverse_PositiveValues()
        {
            var vec3 = new Vector3(2f, 4f, 10f);
            var expected = new Vector3(.5f, .25f, .1f);
            Assert.That(vec3.Inverse(), Is.EqualTo(expected).Within(k_Delta));
        }

        [Test]
        public void Inverse_NegativeValues()
        {
            var vec3 = new Vector3(-10f, -4f, -2f);
            var expected = new Vector3(-.1f, -.25f, -.5f);
            Assert.That(vec3.Inverse(), Is.EqualTo(expected).Within(k_Delta));
        }

        [Test]
        public void MultiplyTest()
        {
            var initial = new Vector3(2f, 2f, 2f);
            var scale = new Vector3(3f, 3f, 3f);
            var result = initial.Multiply(scale);
            var expected = new Vector3(6f, 6f, 6f);
            Assert.That(result, Is.EqualTo(expected).Within(k_Delta));
        }

        [Test]
        public void DivisionTest()
        {
            var initial = new Vector3(6f, 6f, 6f);
            var scale = new Vector3(3f, 3f, 3f);
            var result = initial.Divide(scale);
            var expected = new Vector3(2f, 2f, 2f);
            Assert.That(result, Is.EqualTo(expected).Within(k_Delta));
        }

        [Test]
        public void SafeDivisionTest()
        {
            var initial = new Vector3(6f, 6f, 6f);
            var scale = new Vector3(3f, 0, 3f);
            var result = initial.SafeDivide(scale);
            var expected = new Vector3(2f, 0f, 2f);
            Assert.That(result, Is.EqualTo(expected).Within(k_Delta));
        }
        #endregion // Unity.XR.CoreUtils.Editor.Tests

        [Test]
        public void MinComponent_ReturnsMinAxisValue()
        {
            var minX = new Vector3(2f, 1f, 0f);
            Assert.AreEqual(minX.MinComponent(), minX.z);
            var minY = new Vector3(0f, 2f, 1f);
            Assert.AreEqual(minY.MinComponent(), minY.x);
            var minZ = new Vector3(0f, 1f, 2f);
            Assert.AreEqual(minZ.MinComponent(), minZ.x);
        }

        //https://github.com/Unity-Technologies/Unity.Mathematics/blob/1.2.5/src/Tests/Tests/Shared/TestMath.gen.cs
        [Test]
        public void AbsTest()
        {
            Assert.AreEqual(new Vector3(0f, 1.1f, 2.2f), (new Vector3(0f, -1.1f, 2.2f)).Abs());
            Assert.AreEqual(new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity), (new Vector3(float.NegativeInfinity, float.PositiveInfinity, float.PositiveInfinity)).Abs());
        }

        [Test]
        public void SafeDivideTest()
        {
            Vector3 a0 = new Vector3(-353.131439f, -102.799866f, 51.3191528f);
            Vector3 b0 = new Vector3(-178.739563f, -302.096283f, -199.405823f);
            Vector3 r0 = new Vector3(1.97567582f, 0.34028843f, -0.257360339f);
            Assert.AreEqual(r0, a0.SafeDivide(b0));

            Vector3 a1 = new Vector3(-191.871674f, 8.041809f, -128.73764f);
            Vector3 b1 = new Vector3(278.850769f, 502.3376f, -361.484833f);
            Vector3 r1 = new Vector3(-0.688080132f, 0.0160087738f, 0.356135666f);
            Assert.AreEqual(r1, a1.SafeDivide(b1));

            Vector3 a2 = new Vector3(-136.0596f, -370.471f, -237.69455f);
            Vector3 b2 = new Vector3(353.121033f, -38.894928f, -75.76474f);
            Vector3 r2 = new Vector3(-0.385305852f, 9.524919f, 3.1372714f);
            Assert.AreEqual(r2, a2.SafeDivide(b2));

            Vector3 a3 = new Vector3(-432.546875f, 200.2655f, 361.4416f);
            Vector3 b3 = new Vector3(-195.217834f, -405.034f, -394.23f);
            Vector3 r3 = new Vector3(2.215714f, -0.4944412f, -0.9168292f);
            Assert.AreEqual(r3, a3.SafeDivide(b3));

            Vector3 a4 = new Vector3(float.Epsilon, float.MaxValue, float.MinValue);
            Vector3 b4 = new Vector3(float.NaN, float.PositiveInfinity, float.NegativeInfinity);
            Vector3 r4 = new Vector3(0.0f, 0.0f, 0.0f);
            Assert.AreEqual(r4, a4.SafeDivide(b4));

            /*
             * public const Single Epsilon = 1.401298E-45F;
                public const Single MaxValue = 3.40282347E+38F;
                public const Single MinValue = -3.40282347E+38F;
                public const Single NaN = 0F / 0F;
                public const Single NegativeInfinity = -1F / 0F;
                public const Single PositiveInfinity = 1F / 0F;
             */
        }
    }
}
