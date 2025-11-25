#if INCLUDE_COLLECTIONS
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PKGE.Unsafe
{
    public static partial class NativeListExtensions
    {
#if PKGE_USING_UNSAFE
        //https://github.com/Unity-Technologies/com.unity.formats.alembic/blob/main/com.unity.formats.alembic/Runtime/Scripts/Misc/RuntimeUtils.cs
        #region UnityEngine.Formats.Alembic.Importer
        public static unsafe T* GetPtr<T>(this NativeList<T> nativeList) where T : unmanaged
        {
            return nativeList.GetUnsafePtr();
        }

        public static unsafe T* GetReadOnlyPtr<T>(this NativeList<T> nativeList) where T : unmanaged
        {
            return nativeList.GetUnsafeReadOnlyPtr();
        }
        #endregion // UnityEngine.Formats.Alembic.Importer
#endif // PKGE_USING_UNSAFE

        //https://github.com/Unity-Technologies/Graphics/blob/2ecb711df890ca21a0817cf610ec21c500cb4bfe/Packages/com.unity.render-pipelines.universal/Runtime/UniversalRenderPipelineCore.cs
        #region UnityEngine.Rendering.Universal
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T UnsafeElementAt<T>(this NativeList<T> nativeList, int index) where T : unmanaged
        {
            return ref nativeList.UnsafeElementAtMutable(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T UnsafeElementAtMutable<T>(this NativeList<T> nativeList, int index) where T : unmanaged
        {
            Assert.IsTrue(nativeList.IsCreated);
            Assert.IsTrue(index < nativeList.Capacity);

            if (index >= nativeList.Length)
                nativeList.ResizeUninitialized(index);

            return ref nativeList.ElementAt(index);
        }
        #endregion // UnityEngine.Rendering.Universal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRange<T>(this NativeList<T> nativeList, NativeArray<T>.ReadOnly nativeArrayRO) where T : unmanaged
        {
            nativeList.AddRange(nativeArrayRO.GetUnsafeReadOnlyPtr(), nativeArrayRO.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRangeNoResize<T>(this NativeList<T> nativeList, NativeArray<T> nativeArray) where T : unmanaged
        {
            nativeList.AddRangeNoResize(nativeArray.GetUnsafeReadOnlyPtr(), nativeArray.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRangeNoResize<T>(this NativeList<T> nativeList, NativeArray<T>.ReadOnly nativeArrayRO) where T : unmanaged
        {
            nativeList.AddRangeNoResize(nativeArrayRO.GetUnsafeReadOnlyPtr(), nativeArrayRO.Length);
        }
    }
}
#endif // INCLUDE_COLLECTIONS
