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
#if PKGE_USING_UNSAFE
        //https://github.com/Unity-Technologies/com.unity.formats.alembic/blob/main/com.unity.formats.alembic/Runtime/Scripts/Misc/RuntimeUtils.cs
        #region UnityEngine.Formats.Alembic.Importer
        public static unsafe T* GetPtr<T>(this UnsafeList<T> unsafeList) where T : unmanaged
        {
            return unsafeList.Ptr;
        }

        public static unsafe T* GetReadOnlyPtr<T>(this UnsafeList<T> unsafeList) where T : unmanaged
        {
            return unsafeList.IsCreated ? unsafeList.AsReadOnly().Ptr : null;
        }
        #endregion // UnityEngine.Formats.Alembic.Importer
#endif // PKGE_USING_UNSAFE

        //https://github.com/Unity-Technologies/Graphics/blob/2ecb711df890ca21a0817cf610ec21c500cb4bfe/Packages/com.unity.render-pipelines.universal/Runtime/UniversalRenderPipelineCore.cs
        #region UnityEngine.Rendering.Universal
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T UnsafeElementAt<T>(ref this UnsafeList<T> unsafeList, int index) where T : unmanaged
        {
            return ref unsafeList.UnsafeElementAtMutable(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T UnsafeElementAtMutable<T>(ref this UnsafeList<T> unsafeList, int index) where T : unmanaged
        {
            Assert.IsTrue(unsafeList.IsCreated);
            Assert.IsTrue(index < unsafeList.Capacity);

            if (index >= unsafeList.Length)
                unsafeList.Length = index;

            return ref unsafeList.ElementAt(index);
        }
        #endregion // UnityEngine.Rendering.Universal

        //https://github.com/needle-mirror/com.unity.entities.graphics/blob/a50f9d68777370c4f402c07f175d74bdbbcb24f9/Unity.Entities.Graphics/EntitiesGraphicsCulling.cs#L258
        #region CullingExtensions
        /// <remarks>Cannot use <see cref="NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray{T}(Span{T}, Allocator)"/>
        /// as this would cause recursion when calling <see cref="AsSpan{T}(UnsafeList{T})"/>.</remarks>
        public static NativeArray<T> AsNativeArray<T>(this UnsafeList<T> unsafeList) where T : unmanaged
        {
            NativeArray<T> array;
            unsafe
            {
                array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(unsafeList.Ptr, unsafeList.Length, Allocator.None);
            }

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
        public static void AddRange<T>(ref this UnsafeList<T> unsafeList, T[] array, int count) where T : unmanaged
        {
            Assert.IsTrue(unsafeList.IsCreated);
            Assert.IsNotNull(array);
            Assert.IsTrue(count >= 0);
            Assert.IsTrue(count <= array.Length);

            unsafeList.AddRange(array.AsSpan(start: 0, length: count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(ref this UnsafeList<T> unsafeList, T[] array) where T : unmanaged
        {
            Assert.IsNotNull(array);

            unsafeList.AddRange(array.AsSpan());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(ref this UnsafeList<T> unsafeList, List<T> list) where T : unmanaged
        {
            Assert.IsNotNull(list);

            unsafeList.AddRange(list.AsSpan());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRange<T>(ref this UnsafeList<T> unsafeList, Span<T> span) where T : unmanaged
        {
            Assert.IsTrue(unsafeList.IsCreated);
            Assert.IsFalse(span == null);

            unsafeList.AddRange(UnsafeUtility.AddressOf(ref span[0]), span.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this UnsafeList<T> unsafeList) where T : unmanaged
        {
            return unsafeList.AsNativeArray().AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this UnsafeList<T> unsafeList) where T : unmanaged
        {
            return unsafeList.AsNativeArray().AsReadOnlySpan();
        }

        /// <summary>Create an <see cref="UnsafeList{T}"/> that aliases a <paramref name="nativeArray"/>.</summary>
        /// <remarks>The returned <see cref="UnsafeList{T}"/> must not have its <see cref="UnsafeList{T}.Capacity"/> changed.</remarks>
        /// <typeparam name="T"><see cref="NativeArray{T}"/> supports <see langword="struct"/>, but it can only
        /// be converted to an <see cref="UnsafeList{T}"/> if it contains <see langword="unmanaged"/> types.</typeparam>
        /// <param name="nativeArray">Input <see cref="NativeArray{T}"/>.</param>
        /// <returns>An <see cref="UnsafeList{T}"/> that aliases the <paramref name="nativeArray"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe UnsafeList<T> AsUnsafeList<T>(this NativeArray<T> nativeArray) where T : unmanaged
        {
            return new UnsafeList<T>((T*)nativeArray.GetUnsafePtr(), nativeArray.Length);
        }
    }
}
#endif // INCLUDE_COLLECTIONS
