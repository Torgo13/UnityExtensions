using System;

namespace UnityExtensions
{
    /// <summary>
    /// Helper class for caching enum values.
    /// </summary>
    /// <typeparam name="T">The enum type whose values should be cached.</typeparam>
    public static class EnumValues<T>
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/EnumValues.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Cached result of Enum.GetValues.
        /// </summary>
        public static readonly T[] Values = (T[])Enum.GetValues(typeof(T));
        #endregion // Unity.XR.CoreUtils
    }
}
