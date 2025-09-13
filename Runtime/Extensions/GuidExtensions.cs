using System;

namespace PKGE
{
    /// <summary>
    /// Extensions to the <see cref="System.Guid"/> type.
    /// </summary>
    public static class GuidExtensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Extensions/GuidExtensions.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Decomposes a 16-byte <c>Guid</c> into two 8-byte <c>ulong</c>s.
        /// Recompose with <see cref="GuidUtil.Compose(ulong, ulong)"/>.
        /// </summary>
        /// <param name="guid">The <c>Guid</c> being extended</param>
        /// <param name="low">The lower 8 bytes of the guid.</param>
        /// <param name="high">The upper 8 bytes of the guid.</param>
        public static void Decompose(this Guid guid, out ulong low, out ulong high)
        {
            Span<byte> bytes = stackalloc byte[16];
            guid.TryWriteBytes(bytes);
            low = BitConverter.ToUInt64(bytes.Slice(0, 8));
            high = BitConverter.ToUInt64(bytes.Slice(8, 8));
        }
        #endregion // Unity.XR.CoreUtils
    }
}
