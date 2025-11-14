#if PKGE_USING_INTPTR
#if INCLUDE_COLLECTIONS
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PKGE.Unsafe
{
    public static partial class UnsafeListExtensions
    {
        //https://github.com/Unity-Technologies/com.unity.formats.alembic/blob/main/com.unity.formats.alembic/Runtime/Scripts/Misc/RuntimeUtils.cs
        #region UnityEngine.Formats.Alembic.Importer
        #region IntPtr
        public static unsafe IntPtr GetIntPtr<T>(this UnsafeList<T> unsafeList) where T : unmanaged
        {
            return unsafeList.IsCreated ? (IntPtr)unsafeList.Ptr : IntPtr.Zero;
        }

        public static unsafe IntPtr GetReadOnlyIntPtr<T>(this UnsafeList<T> unsafeList) where T : unmanaged
        {
            return unsafeList.IsCreated ? (IntPtr)unsafeList.AsReadOnly().Ptr : IntPtr.Zero;
        }
        #endregion // IntPtr
        #endregion // UnityEngine.Formats.Alembic.Importer
    }
}
#endif // INCLUDE_COLLECTIONS
#endif // PKGE_USING_INTPTR
