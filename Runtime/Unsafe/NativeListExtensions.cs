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
        /// <summary>
        /// IMPORTANT: Make sure you do not write to the value! There are no checks for this!
        /// </summary>
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

        /// <exception cref="ArgumentOutOfRangeException">Thrown if count is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this NativeList<T> nativeList, T[] array, int count) where T : unmanaged
        {
            Assert.IsTrue(nativeList.IsCreated);
            Assert.IsNotNull(array);
            Assert.IsTrue(count >= 0);
            Assert.IsTrue(count <= array.Length);

#if UNITY_6000_3_OR_NEWER
            nativeList.AddRange(NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray(
                array.AsSpan(start: 0, length: count), Allocator.None));
#else
            nativeList.AddRange(array.AsSpan(start: 0, length: count).AsNativeArray());
#endif // UNITY_6000_3_OR_NEWER
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
