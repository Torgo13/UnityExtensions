using System;
using System.Runtime.InteropServices;

namespace PKGE
{
    /// <summary>
    /// Stores the marshalled size of a struct.
    /// </summary>
    /// <typeparam name="T">The type of struct to get the size of.</typeparam>
    public class SizeOfCache<T>
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Networking/Utilities/SizeOfCache.cs
        #region Unity.LiveCapture.Networking
        /// <summary>
        /// The size of the struct in bytes.
        /// </summary>
        public static int Size { get; }

        static SizeOfCache()
        {
            Type t = typeof(T);
            bool isEnum = t.IsEnum;
            if (!t.IsValueType && !isEnum)
            {
                Size = -1;
                return;
            }

            t = isEnum ? Enum.GetUnderlyingType(t) : t;
            Size = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf(t);
        }
        #endregion // Unity.LiveCapture.Networking
    }
}