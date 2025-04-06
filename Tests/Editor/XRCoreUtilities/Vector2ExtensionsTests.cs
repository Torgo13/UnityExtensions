using NUnit.Framework;
using UnityEngine;

namespace UnityExtensions.Editor.Tests
{
    class Vector2ExtensionsTests
    {
        //https://github.com/needle-mirror/com.unity.cinemachine/blob/85e81c94d0839e65c46a6fe0cd638bd1c6cd48af/Runtime/Core/UnityVectorExtensions.cs
        #region Unity.Cinemachine
        [Test]
        public void FindIntersection_NoIntersection_ReturnsZero()
        {
            Vector2 p1 = new Vector2(0, 0);
            Vector2 p2 = new Vector2(1, 0);
            Vector2 q1 = new Vector2(0, 1);
            Vector2 q2 = new Vector2(1, 1);

            int result = Vector2Extensions.FindIntersection(p1, p2, q1, q2, out Vector2 intersection);

            Assert.AreEqual(0, result, "Expected no intersection between parallel lines");
            Assert.AreEqual(Vector2.positiveInfinity, intersection, "Intersection point should be Vector2.positiveInfinity when there is no intersection");
        }

        [Test]
        public void FindIntersection_LinesIntersect_ReturnsOne()
        {
            Vector2 p1 = new Vector2(0, 0);
            Vector2 p2 = new Vector2(-1, -1);
            Vector2 q1 = new Vector2(0, 1);
            Vector2 q2 = new Vector2(1, 0);

            int result = Vector2Extensions.FindIntersection(p1, p2, q1, q2, out Vector2 intersection);

            Assert.AreEqual(1, result, "Expected the lines to intersect but not the segments");
            var expected = new Vector2(0.5f, 0.5f);
            Assert.AreEqual(expected, intersection, "Intersection point calculation is incorrect");
        }

        [Test]
        public void FindIntersection_SegmentsIntersect_ReturnsTwo()
        {
            Vector2 p1 = new Vector2(0, 0);
            Vector2 p2 = new Vector2(2, 2);
            Vector2 q1 = new Vector2(0, 2);
            Vector2 q2 = new Vector2(2, 0);

            int result = Vector2Extensions.FindIntersection(p1, p2, q1, q2, out Vector2 intersection);

            Assert.AreEqual(2, result, "Expected the segments to intersect");
            var expected = new Vector2(1, 1);
            Assert.AreEqual(expected, intersection, "Intersection point calculation is incorrect");
        }

        [Test]
        public void FindIntersection_CollinearSegmentsDontTouch_ReturnsThree()
        {
            Vector2 p1 = new Vector2(0, 0);
            Vector2 p2 = new Vector2(1, 1);
            Vector2 q1 = new Vector2(2, 2);
            Vector2 q2 = new Vector2(3, 3);

            int result = Vector2Extensions.FindIntersection(p1, p2, q1, q2, out Vector2 intersection);

            Assert.AreEqual(3, result, "Expected collinear segments that don't touch to return 3");
            Assert.AreEqual(Vector2.positiveInfinity, intersection, "Intersection point should be Vector2.positiveInfinity for non-touching collinear segments");
        }

        [Test]
        public void FindIntersection_CollinearSegmentsOverlap_ReturnsFour()
        {
            Vector2 p1 = new Vector2(0, 0);
            Vector2 p2 = new Vector2(3, 3);
            Vector2 q1 = new Vector2(1, 1);
            Vector2 q2 = new Vector2(2, 2);

            int result = Vector2Extensions.FindIntersection(p1, p2, q1, q2, out Vector2 intersection);

            Assert.AreEqual(4, result, "Expected overlapping collinear segments to return 4");
            Assert.AreEqual(Vector2.positiveInfinity, intersection, "Intersection point should be Vector2.positiveInfinity for overlapping collinear segments");
        }
        #endregion // Unity.Cinemachine

        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Tests/Editor/XRCoreUtilities/Vector2ExtensionsTests.cs
        #region Unity.XR.CoreUtils.Editor.Tests
        [Test]
        public void Abs_NegativeValues_AreInverted()
        {
            Assert.AreEqual(new Vector2(2f, 1f), new Vector2(-2f, -1f).Abs());
        }

        [Test]
        public void Abs_PositiveValues_AreUnchanged()
        {
            Assert.AreEqual(new Vector2(2f, 1f), new Vector2(2f, 1f).Abs());
        }
        #endregion // Unity.XR.CoreUtils.Editor.Tests

        const float k_Delta = 0.00000001f;

        [Test]
        public void MaxComponent_ReturnsMaxAxisValue()
        {
            var maxX = new Vector2(2f, 1f);
            Assert.AreEqual(maxX.MaxComponent(), maxX.x);
            var maxY = new Vector2(0f, 2f);
            Assert.AreEqual(maxY.MaxComponent(), maxY.y);
            var maxZ = new Vector2(0f, 1f);
            Assert.AreEqual(maxZ.MaxComponent(), maxZ.y);
        }

        [Test]
        public void Inverse_PositiveValues()
        {
            var vec3 = new Vector2(2f, 4f);
            var expected = new Vector2(.5f, .25f);
            Assert.That(vec3.Inverse(), Is.EqualTo(expected).Within(k_Delta));
        }

        [Test]
        public void Inverse_NegativeValues()
        {
            var vec3 = new Vector2(-10f, -4f);
            var expected = new Vector2(-.1f, -.25f);
            Assert.That(vec3.Inverse(), Is.EqualTo(expected).Within(k_Delta));
        }

        [Test]
        public void MultiplyTest()
        {
            var initial = new Vector2(2f, 2f);
            var scale = new Vector2(3f, 3f);
            var result = initial.Multiply(scale);
            var expected = new Vector2(6f, 6f);
            Assert.That(result, Is.EqualTo(expected).Within(k_Delta));
        }

        [Test]
        public void DivisionTest()
        {
            var initial = new Vector2(6f, 6f);
            var scale = new Vector2(3f, 3f);
            var result = initial.Divide(scale);
            var expected = new Vector2(2f, 2f);
            Assert.That(result, Is.EqualTo(expected).Within(k_Delta));
        }

        [Test]
        public void SafeDivisionTest()
        {
            var initial = new Vector2(6f, 6f);
            var scale = new Vector2(3f, 0);
            var result = initial.SafeDivide(scale);
            var expected = new Vector2(2f, 0f);
            Assert.That(result, Is.EqualTo(expected).Within(k_Delta));
        }

        [Test]
        public void MinComponent_ReturnsMinAxisValue()
        {
            var minX = new Vector2(2f, 1f);
            Assert.AreEqual(minX.MinComponent(), minX.y);
            var minY = new Vector2(0f, 2f);
            Assert.AreEqual(minY.MinComponent(), minY.x);
            var minZ = new Vector2(0f, 1f);
            Assert.AreEqual(minZ.MinComponent(), minZ.x);
        }

        //https://github.com/Unity-Technologies/Unity.Mathematics/blob/1.2.5/src/Tests/Tests/Shared/TestMath.gen.cs
        [Test]
        public void AbsTest()
        {
            Assert.AreEqual(new Vector2(0f, 1.1f), (new Vector2(0f, -1.1f)).Abs());
            Assert.AreEqual(new Vector2(float.PositiveInfinity, float.PositiveInfinity), (new Vector2(float.NegativeInfinity, float.PositiveInfinity)).Abs());
        }

        [Test]
        public void SafeDivideTest()
        {
            Vector2 a0 = new Vector2(-353.131439f, -102.799866f);
            Vector2 b0 = new Vector2(-178.739563f, -302.096283f);
            Vector2 r0 = new Vector2(1.97567582f, 0.34028843f);
            Assert.AreEqual(r0, a0.SafeDivide(b0));

            Vector2 a1 = new Vector2(-191.871674f, 8.041809f);
            Vector2 b1 = new Vector2(278.850769f, 502.3376f);
            Vector2 r1 = new Vector2(-0.688080132f, 0.0160087738f);
            Assert.AreEqual(r1, a1.SafeDivide(b1));

            Vector2 a2 = new Vector2(-136.0596f, -370.471f);
            Vector2 b2 = new Vector2(353.121033f, -38.894928f);
            Vector2 r2 = new Vector2(-0.385305852f, 9.524919f);
            Assert.AreEqual(r2, a2.SafeDivide(b2));

            Vector2 a3 = new Vector2(-432.546875f, 200.2655f);
            Vector2 b3 = new Vector2(-195.217834f, -405.034f);
            Vector2 r3 = new Vector2(2.215714f, -0.4944412f);
            Assert.AreEqual(r3, a3.SafeDivide(b3));

            Vector2 a4 = new Vector2(float.Epsilon, float.MaxValue);
            Vector2 b4 = new Vector2(float.NaN, float.PositiveInfinity);
            Vector2 r4 = new Vector2(0.0f, 0.0f);
            Assert.AreEqual(r4, a4.SafeDivide(b4));
        }
    }
}
