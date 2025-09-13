using NUnit.Framework;
using UnityEngine;

namespace PKGE.Editor.Tests
{
    class PoseExtensionsTests
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Tests/Editor/XRCoreUtilities/PoseExtensionsTests.cs
        #region Unity.XR.CoreUtils.Editor.Tests
        readonly Pose m_IdentityPose = new Pose(new Vector3(), Quaternion.identity);
        readonly Pose m_NonIdentityRotationPose = new Pose(new Vector3(), Quaternion.Inverse(Quaternion.Euler(10f, 20f, 30f)));
        readonly Pose m_PositionOnlyOffsetPose = new Pose(new Vector3(2f, 2f, 2f), Quaternion.identity);
        readonly Pose m_DefaultPose = new Pose(new Vector3(1f, 1f, 1f), Quaternion.identity);

        [Test]
        public void IdentityPoseOffsetDoesNothing()
        {
            Assert.AreEqual(m_DefaultPose, m_IdentityPose.ApplyOffsetTo(m_DefaultPose));
            Assert.AreEqual(m_DefaultPose, m_DefaultPose.ApplyOffsetTo(m_IdentityPose));
        }

        [Test]
        public void ApplyOffsetToPosition()
        {
            var offset = m_PositionOnlyOffsetPose.ApplyOffsetTo(m_DefaultPose);
            Assert.AreEqual(m_DefaultPose.position + m_PositionOnlyOffsetPose.position, offset.position);
            Assert.AreEqual(Quaternion.identity, offset.rotation);
        }

        [Test]
        public void ApplyOffsetToRotation()
        {
            var offset = m_NonIdentityRotationPose.ApplyOffsetTo(m_DefaultPose);
            Assert.AreEqual(m_NonIdentityRotationPose.rotation, offset.rotation);
        }
        #endregion // Unity.XR.CoreUtils.Editor.Tests

        [Test]
        public void ApplyInverseOffsetToPosition()
        {
            var offset = m_PositionOnlyOffsetPose.ApplyOffsetTo(m_DefaultPose.position);
            Assert.AreEqual(m_DefaultPose.position + m_PositionOnlyOffsetPose.position, offset);

            var inverseOffset = m_PositionOnlyOffsetPose.ApplyInverseOffsetTo(offset);
            Assert.AreEqual(offset - m_PositionOnlyOffsetPose.position, inverseOffset);
        }
    }
}
