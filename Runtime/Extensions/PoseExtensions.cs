using UnityEngine;

namespace UnityExtensions
{
    /// <summary>
    /// Extension methods for <see cref="Pose"/> structs.
    /// </summary>
    public static class PoseExtensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Extensions/PoseExtensions.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Offsets the given pose by this parent pose.
        /// </summary>
        /// <param name="pose">The pose defining the offset.</param>
        /// <param name="otherPose">The pose that will be offset.</param>
        /// <returns>A pose offset by <paramref name="pose"/>.</returns>
        public static Pose ApplyOffsetTo(this Pose pose, Pose otherPose)
        {
            var rotation = pose.rotation;
            return new Pose(
                rotation * otherPose.position + pose.position,
                rotation * otherPose.rotation);
        }

        /// <summary>
        /// Offsets the given position by this pose.
        /// </summary>
        /// <param name="pose">The pose defining the offset.</param>
        /// <param name="position">The position to be offset.</param>
        /// <returns>A position offset by <paramref name="pose"/>.</returns>
        public static Vector3 ApplyOffsetTo(this Pose pose, Vector3 position)
        {
            return pose.rotation * position + pose.position;
        }

        /// <summary>
        /// Offsets the given position by the inverse of this pose.
        /// </summary>
        /// <param name="pose">The pose defining the offset.</param>
        /// <param name="position">The position to be offset.</param>
        /// <returns>A position offset by the inverse of <paramref name="pose"/>.</returns>
        public static Vector3 ApplyInverseOffsetTo(this Pose pose, Vector3 position)
        {
            return Quaternion.Inverse(pose.rotation) * (position - pose.position);
        }
        #endregion // Unity.XR.CoreUtils
    }
}
