#if PKGE_USING_INTPTR
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

#if INCLUDE_MATHEMATICS
using Unity.Mathematics;
using static Unity.Mathematics.math;
#else
using PKGE.Mathematics;
using static PKGE.Mathematics.math;
#endif // INCLUDE_MATHEMATICS

namespace PKGE.Unsafe
{
    /// <summary>
    /// Static class with unsafe utility functions.
    /// </summary>
    public static unsafe partial class CoreUnsafeUtils
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Common/CoreUnsafeUtils.cs#L258
        #region UnityEngine.Rendering
        #region IntPtr
        /// <inheritdoc cref="CopyTo"/>
        public static void CopyTo<T>(this List<T> list, IntPtr dest, int count)
            where T : struct
        {
            CopyTo(list, (void*)dest, count);
        }

        /// <inheritdoc cref="CopyTo"/>
        public static void CopyTo<T>(this T[] list, IntPtr dest, int count)
            where T : struct
        {
            CopyTo(list, (void*)dest, count);
        }

        /// <inheritdoc cref="IndexOf"/>
        public static int IndexOf<T>(IntPtr data, int count, T v)
            where T : struct, IEquatable<T>
        {
            return IndexOf((void*)data, count, v);
        }
        #endregion // IntPtr
        #endregion // UnityEngine.Rendering
    }
}
#endif // PKGE_USING_INTPTR
