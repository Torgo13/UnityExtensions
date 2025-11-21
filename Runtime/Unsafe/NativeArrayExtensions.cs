using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PKGE.Unsafe
{
    public static partial class NativeCopyUtility
    {
        //https://github.com/needle-mirror/com.unity.xr.arfoundation/blob/master/Runtime/ARSubsystems/NativeCopyUtility.cs
        #region UnityEngine.XR.ARSubsystems
#if PKGE_USING_UNSAFE
        /// <summary>
        /// Creates a <c>NativeArray</c> from a pointer by first copying <paramref name="length"/>
        /// <paramref name="defaultT"/>s into the <c>NativeArray</c>, and then overwriting the
        /// data in the array with <paramref name="source"/>, assuming each element in <paramref name="source"/>
        /// is <paramref name="sourceElementSize"/> bytes.
        /// </summary>
        /// <remarks>
        /// This is useful for native inter-operations with structs that might change over time. This allows
        /// new fields to be added to the C# struct without breaking data obtained from data calls.
        /// </remarks>
        /// <typeparam name="T">The type of struct to copy.</typeparam>
        /// <param name="defaultT">A default version of <typeparamref name="T"/>, which will be used to first fill the array
        /// before copying from <paramref name="source"/>.</param>
        /// <param name="source">A pointer to a contiguous block of data of size <paramref name="sourceElementSize"/> * <paramref name="length"/>.</param>
        /// <param name="sourceElementSize">The size of one element in <paramref name="source"/>.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="allocator">The allocator to use when creating the <c>NativeArray</c>.</param>
        /// <returns>
        /// A new <c>NativeArray</c> populating with <paramref name="defaultT"/> and <paramref name="source"/>.
        /// The caller owns the memory.
        /// </returns>
        public static unsafe NativeArray<T> PtrToNativeArrayWithDefault<T>(
            T defaultT,
            void* source,
            int sourceElementSize,
            int length,
            Allocator allocator) where T : struct
        {
            var array = CreateArrayFilledWithValue(defaultT, length, allocator);

            // Then overwrite with the source data, which may have a different size
            UnsafeUtility.MemCpyStride(
                destination: array.GetUnsafePtr(),
                destinationStride: SizeOfCache<T>.Size,
                source: source,
                sourceStride: sourceElementSize,
                elementSize: sourceElementSize,
                count: length);

            return array;
        }
#endif // PKGE_USING_UNSAFE

        /// <summary>
        /// Fills <paramref name="array"/> with repeated copies of <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">The type of the <c>NativeArray</c>. Must be a <c>struct</c>.</typeparam>
        /// <param name="array">The array to fill.</param>
        /// <param name="value">The value with which to fill the array.</param>
        public static void FillArrayWithValue<T>(this NativeArray<T> array, T value) where T : struct
        {
#if PKGE_USING_UNSAFE
            // Early out if array is zero, or iOS will crash in MemCpyReplicate.
            if (array.Length == 0)
                return;

            unsafe
            {
                UnsafeUtility.MemCpyReplicate(
                    array.GetUnsafePtr(),
                    UnsafeUtility.AddressOf(ref value),
                    SizeOfCache<T>.Size,
                    array.Length);
            }
#else
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
#endif // PKGE_USING_UNSAFE
        }

        /// <summary>
        /// Creates a new array allocated with <paramref name="allocator"/> and initialized with <paramref name="length"/>
        /// copies of <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">The type of the <c>NativeArray</c> to create. Must be a <c>struct</c>.</typeparam>
        /// <param name="value">The value with which to fill the array.</param>
        /// <param name="length">The length of the array to create.</param>
        /// <param name="allocator">The allocator with which to create the <c>NativeArray</c>.</param>
        /// <returns>A new <c>NativeArray</c> initialized with copies of <paramref name="value"/>.</returns>
        public static NativeArray<T> CreateArrayFilledWithValue<T>(T value, int length, Allocator allocator) where T : struct
        {
            var array = new NativeArray<T>(length, allocator, NativeArrayOptions.UninitializedMemory);
            FillArrayWithValue(array, value);
            return array;
        }
        #endregion // UnityEngine.XR.ARSubsystems
    }

    public static partial class NativeArrayExtensions
    {
#if PKGE_USING_UNSAFE
        //https://github.com/Unity-Technologies/UnityCsReference/blob/4b463aa72c78ec7490b7f03176bd012399881768/Runtime/Export/NativeArray/NativeArray.cs#L1024
        #region Unity.Collections.LowLevel.Unsafe
#if UNITY_6000_3_OR_NEWER
#else
        /// <summary>Internal method used typically by other systems to provide a view on them.</summary>
        public static unsafe NativeArray<T> ConvertExistingDataToNativeArray<T>(Span<T> data,
            Allocator allocator = Allocator.None) where T : unmanaged
        {
            fixed (T* addr = data)
            {
                var nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(addr, data.Length, allocator);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
                return nativeArray;
            }
        }
#endif // UNITY_6000_3_OR_NEWER
        #endregion // Unity.Collections.LowLevel.Unsafe
#endif // PKGE_USING_UNSAFE

        /// <summary>Internal method used typically by other systems to provide a view on them.</summary>
        /// <remarks>The caller is still the owner of the data.</remarks>
        public static NativeArray<T> AsNativeArray<T>(this Span<T> span, Allocator allocator = Allocator.None) where T : struct
        {
            NativeArray<T> nativeArray;
            unsafe
            {
                nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(
                    UnsafeUtility.AddressOf(ref span[0]), span.Length, allocator);
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
            return nativeArray;
        }

        /// <inheritdoc cref="AsNativeArray{T}(Span{T}, Allocator)"/>
        public static NativeArray<T> AsNativeArray<T>(this Span<T> span) where T : unmanaged
        {
#if UNITY_6000_3_OR_NEWER
            var nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray(
                span, Allocator.None);
#else
            NativeArray<T> nativeArray;
            unsafe
            {
                nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(
                    UnsafeUtility.AddressOf(ref span[0]), span.Length, Allocator.None);
            }
#endif // UNITY_6000_3_OR_NEWER

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
            return nativeArray;
        }

        /// <param name="gcHandle">
        /// Use <see cref="UnsafeUtility.ReleaseGCObject(ulong)"/> with <paramref name="gcHandle"/>
        /// when finished with the returned <see cref="NativeArray{T}"/>.
        /// </param>
        public static NativeArray<T> AsNativeArray<T>(this T[] array, int length, out ulong gcHandle) where T : struct
        {
            Assert.IsNotNull(array);
            Assert.IsTrue(length > 0);
            Assert.IsTrue(length <= array.Length);

            NativeArray<T> nativeArray;
            unsafe
            {
                nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(
                    UnsafeUtility.PinGCArrayAndGetDataAddress(array, out gcHandle), length, Allocator.None);
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
            return nativeArray;
        }

        /// <inheritdoc cref="AsNativeArray{T}(T[], int, out ulong)"/>
        public static NativeArray<T> AsNativeArray<T>(this T[] array, out ulong gcHandle) where T : struct
        {
            Assert.IsNotNull(array);

            return array.AsNativeArray(array.Length, out gcHandle);
        }

        /// <inheritdoc cref="AsNativeArray{T}(T[], int, out ulong)"/>
        public static NativeArray<T> AsNativeArray<T>(this List<T> list, out ulong gcHandle) where T : struct
        {
            Assert.IsNotNull(list);

            return list.ExtractArrayFromList().AsNativeArray(list.Count, out gcHandle);
        }

#if ZERO
#if INCLUDE_COLLECTIONS
        public static unsafe NativeList<T> AsNativeList<T>(this Span<T> span) where T : unmanaged
        {
            var nativeList = new NativeList<T>(span.Length, AllocatorManager.None);
            *nativeList.GetUnsafeList() = span.AsUnsafeList();
            return nativeList;
        }

        public static unsafe NativeList<T> AsNativeList<T>(this T[] array, int count) where T : unmanaged
        {
            var nativeList = new NativeList<T>(count, AllocatorManager.None);
            *nativeList.GetUnsafeList() = array.AsUnsafeList();
            return nativeList;
        }

        public static unsafe NativeList<T> AsNativeList<T>(this T[] array) where T : unmanaged
        {
            return array.AsNativeList(array.Length);
        }

        public static unsafe UnsafeList<T> AsUnsafeList<T>(this Span<T> span) where T : unmanaged
        {
            return new UnsafeList<T>((T*)UnsafeUtility.AddressOf(ref span[0]), span.Length);
        }

        public static unsafe UnsafeList<T> AsUnsafeList<T>(this T[] array, int count) where T : unmanaged
        {
            return new UnsafeList<T>((T*)UnsafeUtility.AddressOf(ref array[0]), count);
        }

        public static unsafe UnsafeList<T> AsUnsafeList<T>(this T[] array) where T : unmanaged
        {
            return array.AsUnsafeList(array.Length);
        }

        public static unsafe NativeList<T> AsNativeList<T>(this NativeArray<T> array) where T : unmanaged
        {
            var nativeList = new NativeList<T>(array.Length, AllocatorManager.Temp);
            nativeList.GetUnsafeList()->Ptr = (T*)array.GetUnsafePtr();
            nativeList.ResizeUninitialized(array.Length);
            return nativeList;
        }

        public static unsafe UnsafeList<T> AsUnsafeList<T>(this NativeArray<T> array) where T : unmanaged
        {
            return new UnsafeList<T>((T*)array.GetUnsafePtr(), array.Length);
        }

        public static unsafe UnsafeList<T>.ReadOnly AsReadOnlyUnsafeList<T>(this NativeArray<T> array) where T : unmanaged
        {
            return new UnsafeList<T>((T*)array.GetUnsafeReadOnlyPtr(), array.Length).AsReadOnly();
        }
#endif // INCLUDE_COLLECTIONS
#endif // ZERO

#if PKGE_USING_UNSAFE
        //https://github.com/Unity-Technologies/com.unity.formats.alembic/blob/main/com.unity.formats.alembic/Runtime/Scripts/Misc/RuntimeUtils.cs
        #region UnityEngine.Formats.Alembic.Importer
        public static unsafe void* GetPtr<T>(this NativeArray<T> array) where T : struct
        {
            return array.IsCreated ? array.GetUnsafePtr() : null;
        }

        public static unsafe void* GetReadOnlyPtr<T>(this NativeArray<T> array) where T : struct
        {
            return array.IsCreated ? array.GetUnsafeReadOnlyPtr() : null;
        }
        #endregion // UnityEngine.Formats.Alembic.Importer
#endif // PKGE_USING_UNSAFE

        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Utilities/ArrayExtensions.cs
        #region UnityEngine.Rendering
        /// <summary>
        /// Fills an array with the same value.
        /// </summary>
        /// <typeparam name="T">The type of the array</typeparam>
        /// <param name="array">Target array to fill</param>
        /// <param name="value">Value to fill</param>
        /// <param name="startIndex">Start index to fill</param>
        /// <param name="length">The number of entries to write, or -1 to fill until the end of the array</param>
        public static void FillArray<T>(this NativeArray<T> array, in T value, int startIndex = 0, int length = -1)
            where T : unmanaged
        {
            if (!array.IsCreated)
                throw new InvalidOperationException(nameof(array));
            if (startIndex < 0)
                throw new IndexOutOfRangeException(nameof(startIndex));
            if (startIndex + length >= array.Length)
                throw new IndexOutOfRangeException(nameof(length));

#if PKGE_USING_UNSAFE
            unsafe
            {
                T* ptr = (T*)array.GetUnsafePtr();

                int endIndex = length == -1 ? array.Length : startIndex + length;

                for (int i = startIndex; i < endIndex; ++i)
                    ptr[i] = value;
            }
#else
            int endIndex = length == -1 ? array.Length : startIndex + length;

            for (int i = startIndex; i < endIndex; ++i)
                array[i] = value;
#endif // PKGE_USING_UNSAFE
        }
        #endregion // UnityEngine.Rendering

        //https://github.com/Unity-Technologies/Graphics/blob/2ecb711df890ca21a0817cf610ec21c500cb4bfe/Packages/com.unity.render-pipelines.universal/Runtime/UniversalRenderPipelineCore.cs
        #region UnityEngine.Rendering.Universal
        /// <summary>
        /// IMPORTANT: Make sure you do not write to the value! There are no checks for this!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T UnsafeElementAt<T>(this NativeArray<T> array, int index) where T : struct
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafeReadOnlyPtr(), index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T UnsafeElementAtMutable<T>(this NativeArray<T> array, int index) where T : struct
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), index);
        }
        #endregion // UnityEngine.Rendering.Universal

        //https://github.com/Unity-Technologies/InputSystem/blob/fb786d2a7d01b8bcb8c4218522e5f4b9afea13d7/Packages/com.unity.inputsystem/InputSystem/Utilities/ArrayHelpers.cs
        #region UnityEngine.InputSystem.Utilities
        public static void EraseAtWithCapacity<TValue>(this NativeArray<TValue> array, ref int count, int index)
            where TValue : struct
        {
            Assert.IsTrue(array.IsCreated);
            Assert.IsTrue(count <= array.Length);
            Assert.IsTrue(index >= 0 && index < count);

            // If we're erasing from the beginning or somewhere in the middle, move
            // the array contents down from after the index.
            if (index < count - 1)
            {
                var elementSize = SizeOfCache<TValue>.Size;
                unsafe
                {
                    var arrayPtr = (byte*)array.GetUnsafePtr();

                    UnsafeUtility.MemCpy(arrayPtr + elementSize * index, arrayPtr + elementSize * (index + 1),
                        (count - index - 1) * elementSize);
                }
            }

            --count;
        }
        #endregion // UnityEngine.InputSystem.Utilities
    }

    public static class NativeReferenceExtensions
    {
        //https://github.com/Unity-Technologies/Graphics/blob/2ecb711df890ca21a0817cf610ec21c500cb4bfe/Packages/com.unity.render-pipelines.universal/Runtime/UniversalRenderPipelineCore.cs
        #region UnityEngine.Rendering.Universal
        /// <summary>
        /// IMPORTANT: Make sure you do not write to the value! There are no checks for this!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T AsRef<T>(this NativeReference<T> reference) where T : unmanaged
        {
            return ref UnsafeUtility.AsRef<T>(reference.GetUnsafeReadOnlyPtr());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T AsRefMutable<T>(this NativeReference<T> reference) where T : unmanaged
        {
            return ref UnsafeUtility.AsRef<T>(reference.GetUnsafePtr());
        }
        #endregion // UnityEngine.Rendering.Universal
    }
}
