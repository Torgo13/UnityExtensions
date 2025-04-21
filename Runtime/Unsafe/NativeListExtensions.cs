using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe
{
    public static class NativeListExtensions
    {
        //https://github.com/Unity-Technologies/com.unity.formats.alembic/blob/main/com.unity.formats.alembic/Runtime/Scripts/Misc/RuntimeUtils.cs
        #region UnityEngine.Formats.Alembic.Importer
        public static unsafe void* GetPtr<T>(this NativeList<T> nativeList) where T : unmanaged
        {
            return nativeList.Length == 0 ? null : nativeList.GetUnsafePtr();
        }

        public static unsafe void* GetReadOnlyPtr<T>(this NativeList<T> nativeList) where T : unmanaged
        {
            return nativeList.Length == 0 ? null : nativeList.GetUnsafeReadOnlyPtr();
        }

        #region IntPtr
        public static unsafe IntPtr GetIntPtr<T>(this NativeList<T> nativeList) where T : unmanaged
        {
            return (IntPtr)nativeList.GetUnsafePtr();
        }

        public static unsafe IntPtr GetReadOnlyIntPtr<T>(this NativeList<T> nativeList) where T : unmanaged
        {
            return (IntPtr)nativeList.GetReadOnlyPtr();
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
            return ref UnsafeUtility.ArrayElementAsRef<T>(nativeList.GetUnsafeReadOnlyPtr(), index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T UnsafeElementAtMutable<T>(this NativeList<T> nativeList, int index) where T : unmanaged
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(nativeList.GetUnsafePtr(), index);
        }
        #endregion // UnityEngine.Rendering.Universal

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRange<T>(ref this NativeList<T> nativeList, T[] array) where T : unmanaged
        {
            fixed (T* p = array)
            {
                nativeList.AddRange(p, array.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRange<T>(ref this NativeList<T> nativeList, T[] array, int count) where T : unmanaged
        {
            fixed (T* p = array)
            {
                nativeList.AddRange(p, count);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(ref this NativeList<T> nativeList, List<T> list) where T : unmanaged
        {
            nativeList.AddRange(NoAllocHelpers.ExtractArrayFromList(list), list.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<T> AsSpan<T>(this NativeList<T> nativeList) where T : unmanaged
        {
            return new Span<T>(nativeList.GetUnsafePtr(), nativeList.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this NativeList<T> nativeList) where T : unmanaged
        {
            return new ReadOnlySpan<T>(nativeList.GetUnsafeReadOnlyPtr(), nativeList.Length);
        }
    }
}
