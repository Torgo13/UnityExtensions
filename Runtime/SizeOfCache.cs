using System;

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "StaticMemberInGenericType")]
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

    /// <summary>
    /// Stores the alignment of a struct.
    /// </summary>
    /// <typeparam name="T">The type of struct to get the alignment of.</typeparam>
    public class AlignOfCache<T> where T : struct
    {
        /// <summary>
        /// The alignment of the struct in bytes.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        public static int Alignment { get; }

        static AlignOfCache()
        {
            Type t = typeof(T);
            bool isEnum = t.IsEnum;
            if (!t.IsValueType && !isEnum)
            {
                Alignment = -1;
                return;
            }

            Alignment = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AlignOf<T>();
        }
    }
}