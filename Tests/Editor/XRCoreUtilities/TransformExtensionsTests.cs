using System;
using NUnit.Framework;
using UnityEngine;

namespace UnityExtensions.Editor.Tests
{
    class TransformExtensionsTests
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Tests/Editor/XRCoreUtilities/TransformExtensionsTests.cs
        #region Unity.XR.CoreUtils.Editor.Tests
        readonly Pose m_OffsetPose = new Pose(new Vector3(2f, 3f, 4f), Quaternion.Euler(10f, 20f, 30f));
        const float k_DeltaTolerance = 0.0001f;

        Transform m_TestTransform;
        Transform m_FixedTransform;

        [OneTimeSetUp]
        public void Setup()
        {
            m_TestTransform = new GameObject("transform extensions test").transform;
            m_FixedTransform = new GameObject("transform extensions test - fixed").transform;
            m_FixedTransform.position = new Vector3(1f, 2f, 3f);
        }

        [Test]
        public void GetLocalPose()
        {
            var localPose = m_FixedTransform.GetLocalPose();
            Assert.AreEqual(m_FixedTransform.localPosition, localPose.position);
            AssertRotationApproximatelyEqual(localPose.rotation, m_FixedTransform.localRotation);
        }

        [Test]
        public void GetWorldPose()
        {
            var worldPose = m_FixedTransform.GetWorldPose();
            Assert.AreEqual(worldPose.position, m_FixedTransform.position);
            AssertRotationApproximatelyEqual(worldPose.rotation, m_FixedTransform.rotation);
        }

        [Test]
        public void SetLocalPose()
        {
            m_TestTransform.SetLocalPose(m_OffsetPose);
            Assert.AreEqual(m_TestTransform.localPosition, m_OffsetPose.position);
            AssertRotationApproximatelyEqual(m_OffsetPose.rotation, m_TestTransform.localRotation);
        }

        [Test]
        public void SetWorldPose()
        {
            m_TestTransform.SetWorldPose(m_OffsetPose);
            Assert.AreEqual(m_OffsetPose.position, m_TestTransform.position);
            AssertRotationApproximatelyEqual(m_OffsetPose.rotation, m_TestTransform.rotation);
        }

        // equality comparison on Quaternions using Assert.AreEqual fails when the numbers are almost exactly the same
        static void AssertRotationApproximatelyEqual(Quaternion left, Quaternion right)
        {
            Assert.That(left.x, Is.EqualTo(right.x).Within(k_DeltaTolerance));
            Assert.That(left.y, Is.EqualTo(right.y).Within(k_DeltaTolerance));
            Assert.That(left.z, Is.EqualTo(right.z).Within(k_DeltaTolerance));
            Assert.That(left.w, Is.EqualTo(right.w).Within(k_DeltaTolerance));
        }
        #endregion // Unity.XR.CoreUtils.Editor.Tests

        [Test]
        public void TransformPoseTest()
        {
            var testPose = m_OffsetPose;

            var transformedPose = m_TestTransform.TransformPose(testPose);
            var inverseTransformedPose = m_TestTransform.InverseTransformPose(transformedPose);
            AssertPositionApproximatelyEqual(testPose.position, inverseTransformedPose.position);
            AssertRotationApproximatelyEqual(testPose.rotation, inverseTransformedPose.rotation);
        }

        [Test]
        public void InverseTransformPoseTest()
        {
            var testPose = m_OffsetPose;

            Transform nullTransform = null;
            Assert.Throws<ArgumentNullException>(() => nullTransform.InverseTransformPose(testPose));

            var inverseTransformedPose = m_TestTransform.InverseTransformPose(testPose);
            var transformedPose = m_TestTransform.TransformPose(inverseTransformedPose);
            AssertPositionApproximatelyEqual(testPose.position, transformedPose.position);
            AssertRotationApproximatelyEqual(testPose.rotation, transformedPose.rotation);
        }

        [Test]
        public void TransformRayTest()
        {
            var testRay = new Ray(m_OffsetPose.position, m_OffsetPose.rotation.eulerAngles);

            var transformedRay = m_TestTransform.TransformRay(testRay);
            var inverseTransformedRay = m_TestTransform.InverseTransformRay(transformedRay);
            AssertPositionApproximatelyEqual(testRay.origin, inverseTransformedRay.origin);
            AssertPositionApproximatelyEqual(testRay.direction, inverseTransformedRay.direction);
        }

        [Test]
        public void InverseTransformRayTest()
        {
            var testRay = new Ray(m_OffsetPose.position, m_OffsetPose.rotation.eulerAngles);

            Transform nullTransform = null;
            Assert.Throws<ArgumentNullException>(() => nullTransform.InverseTransformRay(testRay));

            var inverseTransformedRay = m_TestTransform.InverseTransformRay(testRay);
            var transformedRay = m_TestTransform.TransformRay(inverseTransformedRay);
            AssertPositionApproximatelyEqual(testRay.origin, transformedRay.origin);
            AssertPositionApproximatelyEqual(testRay.direction, transformedRay.direction);
        }

        // equality comparison on Vector3 using Assert.AreEqual fails when the numbers are almost exactly the same
        static void AssertPositionApproximatelyEqual(Vector3 left, Vector3 right)
        {
            Assert.That(left.x, Is.EqualTo(right.x).Within(k_DeltaTolerance));
            Assert.That(left.y, Is.EqualTo(right.y).Within(k_DeltaTolerance));
            Assert.That(left.z, Is.EqualTo(right.z).Within(k_DeltaTolerance));
        }
    }
}
