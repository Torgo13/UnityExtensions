using UnityEngine;

namespace PKGE
{
    /// <summary>
    /// Extension methods for <see cref="Quaternion"/> structs.
    /// </summary>
    public static class QuaternionExtensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Extensions/QuaternionExtensions.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Returns a rotation that only contains the yaw component of the specified rotation.
        /// The resulting rotation is not normalized.
        /// </summary>
        /// <param name="rotation">The source rotation.</param>
        /// <returns>A yaw-only rotation that matches the input rotation's yaw.</returns>
        public static Quaternion ConstrainYaw(this Quaternion rotation)
        {
            rotation.x = 0;
            rotation.z = 0;
            return rotation;
        }

        /// <summary>
        /// Returns a normalized rotation that only contains the yaw component of the specified rotation.
        /// </summary>
        /// <param name="rotation">The source rotation.</param>
        /// <returns>A yaw-only rotation that matches the input rotation's yaw.</returns>
        public static Quaternion ConstrainYawNormalized(this Quaternion rotation)
        {
            rotation.x = 0;
            rotation.z = 0;
            rotation.Normalize();
            return rotation;
        }

        /// <summary>
        /// Returns a normalized rotation that only contains the yaw and pitch components of the specified rotation
        /// </summary>
        /// <param name="rotation">The source rotation.</param>
        /// <returns>A yaw- and pitch-only rotation that matches the input rotation's yaw and pitch.</returns>
        public static Quaternion ConstrainYawPitchNormalized(this Quaternion rotation)
        {
            var euler = rotation.eulerAngles;
            euler.z = 0;
            return Quaternion.Euler(euler);
        }
        #endregion // Unity.XR.CoreUtils
        
        //https://github.com/needle-mirror/com.unity.cinemachine/blob/85e81c94d0839e65c46a6fe0cd638bd1c6cd48af/Runtime/Core/UnityVectorExtensions.cs
        #region Unity.Cinemachine
/// <summary>Normalize a quaternion</summary>
        /// <param name="q">The quaternion to normalize</param>
        /// <returns>The normalized quaternion.  Unit length is 1.</returns>
        public static Quaternion Normalized(this Quaternion q)
        {
            Vector4 v = new Vector4(q.x, q.y, q.z, q.w).normalized;
            return new Quaternion(v.x, v.y, v.z, v.w);
        }

        /// <summary>
        /// Get the rotations, first about world up, then about (travelling) local right,
        /// necessary to align the quaternion's forward with the target direction.
        /// This represents the tripod head movement needed to look at the target.
        /// This formulation makes it easy to interpolate without introducing spurious roll.
        /// </summary>
        /// <param name="orient">The Quaternion to examine.</param>
        /// <param name="lookAtDir">The world-space target direction in which we want to look</param>
        /// <param name="worldUp">Which way is up.  Must have a length of 1.</param>
        /// <returns>Vector2.y is rotation about worldUp, and Vector2.x is second rotation,
        /// about local right.</returns>
        public static Vector2 GetCameraRotationToTarget(
            this Quaternion orient, Vector3 lookAtDir, Vector3 worldUp)
        {
            if (lookAtDir.AlmostZero())
                return Vector2.zero;  // degenerate

            // Work in local space
            var toLocal = Quaternion.Inverse(orient);
            var up = toLocal * worldUp;
            lookAtDir = toLocal * lookAtDir;

            // Align yaw based on world up
            float angleH = 0;
            {
                Vector3 targetDirH = lookAtDir.ProjectOntoPlane(up);
                if (!targetDirH.AlmostZero())
                {
                    var currentDirH = Vector3.forward.ProjectOntoPlane(up);
                    if (currentDirH.AlmostZero())
                        currentDirH = Vector3.up.ProjectOntoPlane(up);

                    angleH = currentDirH.SignedAngle(targetDirH, up);
                }
            }

            var q = Quaternion.AngleAxis(angleH, up);

            // Get local vertical angle
            float angleV = (q * Vector3.forward).SignedAngle(lookAtDir, q * Vector3.right);

            return new Vector2(angleV, angleH);
        }

        /// <summary>
        /// Apply rotations, first about world up, then about (travelling) local right.
        /// rot.y is rotation about worldUp, and rot.x is second rotation, about local right.
        /// </summary>
        /// <param name="orient">The quaternion to which to apply the rotation.</param>
        /// <param name="rot">Vector2.y is rotation about worldUp, and Vector2.x is second rotation,
        /// about local right.</param>
        /// <param name="worldUp">Which way is up</param>
        /// <returns>Result rotation after the input is applied to the input quaternion</returns>
        public static Quaternion ApplyCameraRotation(
            this Quaternion orient, Vector2 rot, Vector3 worldUp)
        {
            if (rot.sqrMagnitude < 0.0001f)
                return orient;

            var q = Quaternion.AngleAxis(rot.x, Vector3.right);
            return (Quaternion.AngleAxis(rot.y, worldUp) * orient) * q;
        }
        
        /// <summary>This is a slerp that mimics a camera operator's movement in that
        /// it chooses a path that avoids the lower hemisphere, as defined by
        /// the up param</summary>
        /// <param name="qA">First direction</param>
        /// <param name="qB">Second direction</param>
        /// <param name="t">Interpolation amount</param>
        /// <param name="up">Defines the up direction.  Must have a length of 1.</param>
        /// <returns>Interpolated quaternion</returns>
        public static Quaternion SlerpWithReferenceUp(
            this Quaternion qA, Quaternion qB, float t, Vector3 up)
        {
            var dirA = (qA * Vector3.forward).ProjectOntoPlane(up);
            var dirB = (qB * Vector3.forward).ProjectOntoPlane(up);
            if (dirA.AlmostZero() || dirB.AlmostZero())
                return Quaternion.Slerp(qA, qB, t);

            // Work on the plane, in eulers
            var qBase = Quaternion.LookRotation(dirA, up);
            var qBaseInv = Quaternion.Inverse(qBase);
            Quaternion qA1 = qBaseInv * qA;
            Quaternion qB1 = qBaseInv * qB;
            var eA = qA1.eulerAngles;
            var eB = qB1.eulerAngles;
            return qBase * Quaternion.Euler(
                Mathf.LerpAngle(eA.x, eB.x, t),
                Mathf.LerpAngle(eA.y, eB.y, t),
                Mathf.LerpAngle(eA.z, eB.z, t));
        }
        #endregion // Unity.Cinemachine
    }
}
