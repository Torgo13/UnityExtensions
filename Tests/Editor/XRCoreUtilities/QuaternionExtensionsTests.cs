using NUnit.Framework;
using UnityEngine;

namespace PKGE.Editor.Tests
{
    class QuaternionExtensionsTests
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Tests/Editor/XRCoreUtilities/QuaternionExtensionsTests.cs
        #region Unity.XR.CoreUtils.Editor.Tests
        [Test]
        public void ConstrainYawRotation()
        {
            var rotation = new Quaternion(4, 3, 2, 1);
            var newRotation = rotation.ConstrainYaw();
            Assert.AreEqual(new Quaternion(0, rotation.y, 0, rotation.w), newRotation);
        }

        [Test]
        public void ConstrainYawRotationNormalized()
        {
            var rotation = Quaternion.Euler(4, 3, 2);
            var newRotation = rotation.ConstrainYawNormalized();
            Assert.IsTrue(Quaternion.Euler(0, rotation.eulerAngles.y, 0) == newRotation);
        }

        [Test]
        public void ConstrainYawPitchRotationNormalized()
        {
            var rotation = Quaternion.Euler(15, 30, 60);
            var newRotation = rotation.ConstrainYawPitchNormalized();
            Assert.IsTrue(Quaternion.Euler(15, 30, 0) == newRotation);
        }
        #endregion // Unity.XR.CoreUtils.Editor.Tests
    }
}
