using System;

namespace PKGE
{
    /// <summary>
    /// Utility for creating a <see cref="SerializableGuid"/>.
    /// Unity can serialize a <see cref="SerializableGuid"/>, but not a <see cref="System.Guid"/>.
    /// </summary>
    public static class SerializableGuidUtil
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/SerializableGuidUtil.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Creates a <see cref="SerializableGuid"/> from a <see cref="System.Guid"/>.
        /// </summary>
        /// <param name="guid">The <see cref="System.Guid"/> to represent as a <see cref="SerializableGuid"/>.</param>
        /// <returns>A serializable version of <paramref name="guid"/>.</returns>
        public static SerializableGuid Create(Guid guid)
        {
            guid.Decompose(out var low, out var high);
            return new SerializableGuid(low, high);
        }
        #endregion // Unity.XR.CoreUtils
    }
}