#if INCLUDE_COLLECTIONS
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PKGE.Unsafe
{
    public static class NativeListExtensions
    {
        //https://github.com/Unity-Technologies/com.unity.formats.alembic/blob/main/com.unity.formats.alembic/Runtime/Scripts/Misc/RuntimeUtils.cs
        #region UnityEngine.Formats.Alembic.Importer
        public static unsafe void* GetPtr<T>(this NativeList<T> nativeList) where T : unmanaged
        {
            return nativeList.GetUnsafePtr();
        }

        public static unsafe void* GetReadOnlyPtr<T>(this NativeList<T> nativeList) where T : unmanaged
        {
            return nativeList.GetUnsafeReadOnlyPtr();
        }

        #region IntPtr
        public static unsafe IntPtr GetIntPtr<T>(this NativeList<T> nativeList) where T : unmanaged
        {
            return nativeList.IsCreated ? (IntPtr)nativeList.GetUnsafePtr() : IntPtr.Zero;
        }

        public static unsafe IntPtr GetReadOnlyIntPtr<T>(this NativeList<T> nativeList) where T : unmanaged
        {
            return nativeList.IsCreated ? (IntPtr)nativeList.GetUnsafeReadOnlyPtr() : IntPtr.Zero;
        }
        #endregion // IntPtr
        #endregion // UnityEngine.Formats.Alembic.Importer

        //https://github.com/Unity-Technologies/Graphics/blob/2ecb711df890ca21a0817cf610ec21c500cb4bfe/Packages/com.unity.render-pipelines.universal/Runtime/UniversalRenderPipelineCore.cs
        #region UnityEngine.Rendering.Universal
        /// <summary>
        /// IMPORTANT: Make sure you do not write to the value! There are no checks for this!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T UnsafeElementAt<T>(this NativeList<T> nativeList, int index) where T : unmanaged
        {
            Assert.IsTrue(nativeList.IsCreated);

            return ref UnsafeUtility.ArrayElementAsRef<T>(nativeList.GetUnsafeReadOnlyPtr(), index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T UnsafeElementAtMutable<T>(this NativeList<T> nativeList, int index) where T : unmanaged
        {
            Assert.IsTrue(nativeList.IsCreated);

            return ref UnsafeUtility.ArrayElementAsRef<T>(nativeList.GetUnsafePtr(), index);
        }
        #endregion // UnityEngine.Rendering.Universal

        /// <exception cref="ArgumentOutOfRangeException">Thrown if count is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRange<T>(this NativeList<T> nativeList, T[] array, int count) where T : unmanaged
        {
            Assert.IsTrue(nativeList.IsCreated);
            Assert.IsNotNull(array);
            Assert.IsTrue(count >= 0);
            Assert.IsTrue(count <= array.Length);

            fixed (T* p = array)
            {
                nativeList.AddRange(p, count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this NativeList<T> nativeList, T[] array) where T : unmanaged
        {
            Assert.IsNotNull(array);

            nativeList.AddRange(array, array.Length);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this NativeList<T> nativeList, List<T> list) where T : unmanaged
        {
            Assert.IsNotNull(list);

            nativeList.AddRange(NoAllocHelpers.ExtractArrayFromList(list), list.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRangeNoResize<T>(this NativeList<T> nativeList, NativeArray<T> nativeArray) where T : unmanaged
        {
            nativeList.AddRangeNoResize(nativeArray.GetUnsafeReadOnlyPtr(), nativeArray.Length);
        }
    }
}
#endif // INCLUDE_COLLECTIONS
