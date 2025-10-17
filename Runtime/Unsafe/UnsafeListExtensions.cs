#if INCLUDE_COLLECTIONS
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PKGE.Unsafe
{
    public static class UnsafeListExtensions
    {
        //https://github.com/Unity-Technologies/com.unity.formats.alembic/blob/main/com.unity.formats.alembic/Runtime/Scripts/Misc/RuntimeUtils.cs
        #region UnityEngine.Formats.Alembic.Importer
        public static unsafe void* GetPtr<T>(this UnsafeList<T> unsafeList) where T : unmanaged
        {
            return unsafeList.IsEmpty ? null : unsafeList.Ptr;
        }

        public static unsafe void* GetReadOnlyPtr<T>(this UnsafeList<T> unsafeList) where T : unmanaged
        {
            return unsafeList.IsEmpty ? null : unsafeList.AsReadOnly().Ptr;
        }

        #region IntPtr
        public static unsafe IntPtr GetIntPtr<T>(this UnsafeList<T> unsafeList) where T : unmanaged
        {
            return unsafeList.IsEmpty ? IntPtr.Zero : (IntPtr)unsafeList.Ptr;
        }

        public static unsafe IntPtr GetReadOnlyIntPtr<T>(this UnsafeList<T> unsafeList) where T : unmanaged
        {
            return unsafeList.IsEmpty ? IntPtr.Zero : (IntPtr)unsafeList.AsReadOnly().Ptr;
        }
        #endregion // IntPtr
        #endregion // UnityEngine.Formats.Alembic.Importer

        //https://github.com/Unity-Technologies/Graphics/blob/2ecb711df890ca21a0817cf610ec21c500cb4bfe/Packages/com.unity.render-pipelines.universal/Runtime/UniversalRenderPipelineCore.cs
        #region UnityEngine.Rendering.Universal
        /// <summary>
        /// IMPORTANT: Make sure you do not write to the value! There are no checks for this!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T UnsafeElementAt<T>(this UnsafeList<T> unsafeList, int index) where T : unmanaged
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(unsafeList.Ptr, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T UnsafeElementAtMutable<T>(this UnsafeList<T> unsafeList, int index) where T : unmanaged
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(unsafeList.Ptr, index);
        }
        #endregion // UnityEngine.Rendering.Universal

        //https://github.com/needle-mirror/com.unity.entities.graphics/blob/master/Unity.Entities.Graphics/EntitiesGraphicsCulling.cs
        #region CullingExtensions
        public static unsafe NativeArray<T> AsNativeArray<T>(this UnsafeList<T> unsafeList) where T : unmanaged
        {
            Assert.IsTrue(unsafeList.IsCreated);

            var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(unsafeList.Ptr, unsafeList.Length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
            return array;
        }

        /// <exception cref="System.ArgumentOutOfRangeException">start must be >= 0</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Length must be >= 0</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">sub array range {start}-{start + length - 1} is outside the range of the native array 0-{Length - 1}</exception>
        /// <exception cref="System.ArgumentException">sub array range {start}-{start + length - 1} caused an integer overflow and is outside the range of the native array 0-{Length - 1}</exception>
        public static NativeArray<T> GetSubNativeArray<T>(this UnsafeList<T> unsafeList, int start, int length)
            where T : unmanaged =>
            unsafeList.AsNativeArray().GetSubArray(start, length);
        #endregion // CullingExtensions

        /// <exception cref="ArgumentOutOfRangeException">Thrown if count is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRange<T>(this UnsafeList<T> unsafeList, T[] array, int count) where T : unmanaged
        {
            Assert.IsTrue(unsafeList.IsCreated);
            Assert.IsNotNull(array);
            Assert.IsTrue(count >= 0);
            Assert.IsTrue(count <= array.Length);

            fixed (T* p = array)
            {
                unsafeList.AddRange(p, count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this UnsafeList<T> unsafeList, T[] array) where T : unmanaged
        {
            Assert.IsNotNull(array);

            unsafeList.AddRange(array, array.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this UnsafeList<T> unsafeList, List<T> list) where T : unmanaged
        {
            Assert.IsNotNull(list);

            unsafeList.AddRange(NoAllocHelpers.ExtractArrayFromList(list), list.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<T> AsSpan<T>(this UnsafeList<T> unsafeList) where T : unmanaged
        {
            if (unsafeList.IsEmpty)
                return new Span<T>();

            return new Span<T>(unsafeList.Ptr, unsafeList.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this UnsafeList<T> unsafeList) where T : unmanaged
        {
            return unsafeList.AsSpan();
        }
    }
}
#endif // INCLUDE_COLLECTIONS
