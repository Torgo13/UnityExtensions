// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace UnityExtensions.Unsafe
{
    public static class UnsafeExtensions
    {
        //https://github.com/Unity-Technologies/UnityCsReference/blob/b42ec0031fc505c35aff00b6a36c25e67d81e59e/Runtime/Export/Unsafe/UnsafeUtility.cs
        #region Unity.Collections.LowLevel.Unsafe
        #region Blittable
        public static bool IsBlittableValueType(Type t) { return t.IsValueType && UnsafeUtility.IsBlittable(t); }

        public static string GetReasonForTypeNonBlittableImpl(Type t, string name)
        {
            if (!t.IsValueType)
                return $"{name} is not blittable because it is not of value type ({t})\n";

            if (t.IsPrimitive)
                return $"{name} is not blittable ({t})\n";

            using var sb = StringBuilderPool.Get(out var ret);
            foreach (FieldInfo f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!IsBlittableValueType(f.FieldType))
                    _ = ret.Append(GetReasonForTypeNonBlittableImpl(f.FieldType, $"{name}.{f.Name}"));
            }

            return ret.ToString();
        }

        // while it would make sense to have functions like ThrowIfArgumentIsNonBlittable
        // currently we insist on including part of call stack into exception message in these cases
        // e.g. "T used in NativeArray<T> must be blittable"
        // due to that we will need to pass message string to this function
        //   but most of the time we will be creating it using string.Format and it will happen on every check
        //   instead of "only if we fail check for is-blittable"
        // that's why we provide the means to implement this pattern on your code (but not function itself)

        public static bool IsArrayBlittable(Array arr)
        {
            return IsBlittableValueType(arr.GetType().GetElementType());
        }

        public static bool IsGenericListBlittable<T>() where T : struct
        {
            return UnsafeUtility.IsBlittable(typeof(T));
        }

        public static string GetReasonForArrayNonBlittable(Array arr)
        {
            Type t = arr.GetType().GetElementType();
            return GetReasonForTypeNonBlittableImpl(t, t?.Name);
        }

        public static string GetReasonForGenericListNonBlittable<T>() where T : struct
        {
            Type t = typeof(T);
            return GetReasonForTypeNonBlittableImpl(t, t.Name);
        }

        public static string GetReasonForTypeNonBlittable(Type t)
        {
            return GetReasonForTypeNonBlittableImpl(t, t.Name);
        }

        public static string GetReasonForValueTypeNonBlittable<T>() where T : struct
        {
            Type t = typeof(T);
            return GetReasonForTypeNonBlittableImpl(t, t.Name);
        }
        #endregion // Blittable

        #region IntPtr
        /// <exception cref="ArgumentNullException">target</exception>
        public unsafe static IntPtr PinGCObjectAndGetAddress(object target, out ulong gcHandle)
        {
            return (IntPtr)UnsafeUtility.PinGCObjectAndGetAddress(target, out gcHandle);
        }

        /// <exception cref="ArgumentNullException">target</exception>
        public unsafe static IntPtr PinGCArrayAndGetDataAddress(Array target, out ulong gcHandle)
        {
            return (IntPtr)UnsafeUtility.PinGCArrayAndGetDataAddress(target, out gcHandle);
        }

        /// <summary>
        /// Assigns an Object reference to a struct or pinned class.
        /// </summary>
        /// <remarks>
        /// Additional resources: <see cref="UnsafeUtility.PinGCObjectAndGetAddress"/>
        /// </remarks>
        /// <exception cref="UnityEngine.UnityException">Can only be called from the main thread</exception>
        public unsafe static void CopyObjectAddressToPtr(object target, IntPtr dstPtr)
        {
            UnsafeUtility.CopyObjectAddressToPtr(target, (void*)dstPtr);
        }

        /// <exception cref="UnityEngine.UnityException">Can only be called from the main thread</exception>
        public unsafe static IntPtr MallocTracked(long size, int alignment, Allocator allocator, int callstacksToSkip)
        {
            return (IntPtr)UnsafeUtility.MallocTracked(size, alignment, allocator, callstacksToSkip);
        }

        /// <summary>
        /// Free memory with leak tracking.
        /// </summary>
        /// <param name="memory">Memory pointer.</param>
        /// <param name="allocator">Allocator.</param>
        /// <exception cref="UnityEngine.UnityException">Can only be called from the main thread</exception>
        public unsafe static void FreeTracked(IntPtr memory, Allocator allocator)
        {
            UnsafeUtility.FreeTracked((void*)memory, allocator);
        }

        /// <exception cref="UnityEngine.UnityException">Can only be called from the main thread</exception>
        public unsafe static IntPtr Malloc(long size, int alignment, Allocator allocator)
        {
            return (IntPtr)UnsafeUtility.Malloc(size, alignment, allocator);
        }

        /// <exception cref="UnityEngine.UnityException">Can only be called from the main thread</exception>
        public unsafe static void Free(IntPtr memory, Allocator allocator)
        {
            UnsafeUtility.Free((void*)memory, allocator);
        }

        /// <exception cref="UnityEngine.UnityException">Can only be called from the main thread</exception>
        public unsafe static void MemCpy(IntPtr destination, IntPtr source, long size)
        {
            UnsafeUtility.MemCpy((void*)destination, (void*)source, size);
        }

        /// <exception cref="UnityEngine.UnityException">Can only be called from the main thread</exception>
        public unsafe static void MemCpyReplicate(IntPtr destination, IntPtr source, int size, int count)
        {
            UnsafeUtility.MemCpyReplicate((void*)destination, (void*)source, size, count);
        }

        /// <summary>
        /// Similar to UnsafeUtility.MemCpy but can skip bytes via desinationStride and sourceStride.
        /// </summary>
        /// <exception cref="UnityEngine.UnityException">Can only be called from the main thread</exception>
        public unsafe static void MemCpyStride(IntPtr destination, int destinationStride, IntPtr source, int sourceStride, int elementSize, int count)
        {
            UnsafeUtility.MemCpyStride((void*)destination, destinationStride, (void*)source, sourceStride, elementSize, count);
        }

        /// <exception cref="UnityEngine.UnityException">Can only be called from the main thread</exception>
        public unsafe static void MemMove(IntPtr destination, IntPtr source, long size)
        {
            UnsafeUtility.MemMove((void*)destination, (void*)source, size);
        }

        /// <exception cref="UnityEngine.UnityException">Can only be called from the main thread</exception>
        public unsafe static void MemSet(IntPtr destination, byte value, long size)
        {
            UnsafeUtility.MemSet((void*)destination, value, size);
        }

        /// <exception cref="UnityEngine.UnityException">Can only be called from the main thread</exception>
        public static void MemClear(IntPtr destination, long size)
        {
            MemSet(destination, 0, size);
        }

        /// <exception cref="UnityEngine.UnityException">Can only be called from the main thread</exception>
        public unsafe static int MemCmp(IntPtr ptr1, IntPtr ptr2, long size)
        {
            return UnsafeUtility.MemCmp((void*)ptr1, (void*)ptr2, size);
        }

        /// <exception cref="ArgumentNullException">ptr</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CopyPtrToStructure<T>(IntPtr ptr, out T output) where T : struct
        {
            UnsafeUtility.CopyPtrToStructure((void*)ptr, out output);
        }

        /// <exception cref="ArgumentNullException">ptr</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CopyStructureToPtr<T>(ref T input, IntPtr ptr) where T : struct
        {
            UnsafeUtility.CopyStructureToPtr(ref input, (void*)ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static T ReadArrayElement<T>(IntPtr source, int index)
        {
            return UnsafeUtility.ReadArrayElement<T>((void*)source, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static T ReadArrayElementWithStride<T>(IntPtr source, int index, int stride)
        {
            return UnsafeUtility.ReadArrayElementWithStride<T>((void*)source, index, stride);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void WriteArrayElement<T>(IntPtr destination, int index, T value)
        {
            UnsafeUtility.WriteArrayElement<T>((void*)destination, index, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void WriteArrayElementWithStride<T>(IntPtr destination, int index, int stride, T value)
        {
            UnsafeUtility.WriteArrayElementWithStride<T>((void*)destination, index, stride, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static IntPtr AddressOf<T>(ref T output) where T : struct
        {
            return (IntPtr)UnsafeUtility.AddressOf<T>(ref output);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static ref T AsRef<T>(IntPtr ptr) where T : struct
        {
            return ref UnsafeUtility.AsRef<T>((void*)ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static ref T ArrayElementAsRef<T>(IntPtr ptr, int index) where T : struct
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>((void*)ptr, index);
        }
        #endregion // IntPtr
        #endregion // Unity.Collections.LowLevel.Unsafe

        #region Unmanaged
        #region ArrayToIntPtr
        /// <inheritdoc cref="MemCpy"/>
        public unsafe static void MemCpy<T>(IntPtr destination, T[] source, long size) where T : unmanaged
        {
            fixed (void* p = source)
            {
                UnsafeUtility.MemCpy((void*)destination, p, size);
            }
        }

        /// <inheritdoc cref="MemCpyReplicate"/>
        public unsafe static void MemCpyReplicate<T>(IntPtr destination, T[] source, int size, int count) where T : unmanaged
        {
            fixed (void* p = source)
            {
                UnsafeUtility.MemCpyReplicate((void*)destination, p, size, count);
            }
        }

        /// <inheritdoc cref="MemCpyStride"/>
        public unsafe static void MemCpyStride<T>(IntPtr destination, int destinationStride, T[] source, int sourceStride, int elementSize, int count) where T : unmanaged
        {
            fixed (void* p = source)
            {
                UnsafeUtility.MemCpyStride((void*)destination, destinationStride, p, sourceStride, elementSize, count);
            }
        }
        #endregion // ArrayToIntPtr

        #region IntPtrToArray
        /// <inheritdoc cref="MemCpy"/>
        public unsafe static void MemCpy<T>(T[] destination, IntPtr source, long size) where T : unmanaged
        {
            fixed (void* p = destination)
            {
                UnsafeUtility.MemCpy(p, (void*)source, size);
            }
        }

        /// <inheritdoc cref="MemCpyReplicate"/>
        public unsafe static void MemCpyReplicate<T>(T[] destination, IntPtr source, int size, int count) where T : unmanaged
        {
            fixed (void* p = destination)
            {
                UnsafeUtility.MemCpyReplicate(p, (void*)source, size, count);
            }
        }

        /// <inheritdoc cref="MemCpyStride"/>
        public unsafe static void MemCpyStride<T>(T[] destination, int destinationStride, IntPtr source, int sourceStride, int elementSize, int count) where T : unmanaged
        {
            fixed (void* p = destination)
            {
                UnsafeUtility.MemCpyStride(p, destinationStride, (void*)source, sourceStride, elementSize, count);
            }
        }
        #endregion // IntPtrToArray

        #region ArrayToArray
        /// <inheritdoc cref="MemCpy"/>
        public unsafe static void MemCpy<T>(T[] destination, T[] source, long size) where T : unmanaged
        {
            fixed (void* p0 = source)
            fixed (void* p1 = source)
            {
                UnsafeUtility.MemCpy(p1, p0, size);
            }
        }

        /// <inheritdoc cref="MemCpyReplicate"/>
        public unsafe static void MemCpyReplicate<T>(T[] destination, T[] source, int size, int count) where T : unmanaged
        {
            fixed (void* p0 = source)
            fixed (void* p1 = source)
            {
                UnsafeUtility.MemCpyReplicate(p1, p0, size, count);
            }
        }

        /// <inheritdoc cref="MemCpyStride"/>
        public unsafe static void MemCpyStride<T>(T[] destination, int destinationStride, T[] source, int sourceStride, int elementSize, int count) where T : unmanaged
        {
            fixed (void* p0 = source)
            fixed (void* p1 = source)
            {
                UnsafeUtility.MemCpyStride(p1, destinationStride, p0, sourceStride, elementSize, count);
            }
        }
        #endregion // ArrayToArray
        #endregion // Unmanaged

        #region Struct
        #region ArrayToIntPtr
        /// <inheritdoc cref="MemCpy"/>
        public static void MemCpyStruct<T>(IntPtr destination, T[] source, long size) where T : struct
        {
            MemCpy(destination, PinGCArrayAndGetDataAddress(source, out var sourceGCHandle), size);
            UnsafeUtility.ReleaseGCObject(sourceGCHandle);
        }

        /// <inheritdoc cref="MemCpyReplicate"/>
        public static void MemCpyReplicateStruct<T>(IntPtr destination, T[] source, int size, int count) where T : struct
        {
            MemCpyReplicate(destination, PinGCArrayAndGetDataAddress(source, out var sourceGCHandle), size, count);
            UnsafeUtility.ReleaseGCObject(sourceGCHandle);
        }

        /// <inheritdoc cref="MemCpyStride"/>
        public static void MemCpyStrideStruct<T>(IntPtr destination, int destinationStride, T[] source, int sourceStride, int elementSize, int count) where T : struct
        {
            MemCpyStride(destination, destinationStride, PinGCArrayAndGetDataAddress(source, out var sourceGCHandle), sourceStride, elementSize, count);
            UnsafeUtility.ReleaseGCObject(sourceGCHandle);
        }
        #endregion // ArrayToIntPtr

        #region IntPtrToArray
        /// <inheritdoc cref="MemCpy"/>
        public static void MemCpyStruct<T>(T[] destination, IntPtr source, long size) where T : struct
        {
            MemCpy(PinGCArrayAndGetDataAddress(destination, out var sourceGCHandle), source, size);
            UnsafeUtility.ReleaseGCObject(sourceGCHandle);
        }

        /// <inheritdoc cref="MemCpyReplicate"/>
        public static void MemCpyReplicateStruct<T>(T[] destination, IntPtr source, int size, int count) where T : struct
        {
            MemCpyReplicate(PinGCArrayAndGetDataAddress(destination, out var sourceGCHandle), source, size, count);
            UnsafeUtility.ReleaseGCObject(sourceGCHandle);
        }

        /// <inheritdoc cref="MemCpyStride"/>
        public static void MemCpyStrideStruct<T>(T[] destination, int destinationStride, IntPtr source, int sourceStride, int elementSize, int count) where T : struct
        {
            MemCpyStride(PinGCArrayAndGetDataAddress(destination, out var sourceGCHandle), destinationStride, source, sourceStride, elementSize, count);
            UnsafeUtility.ReleaseGCObject(sourceGCHandle);
        }
        #endregion // IntPtrToArray

        #region ArrayToArray
        /// <inheritdoc cref="MemCpy"/>
        public static void MemCpyStruct<T>(T[] destination, T[] source, long size) where T : struct
        {
            MemCpy(
                PinGCArrayAndGetDataAddress(destination, out var destinationGCHandle),
                PinGCArrayAndGetDataAddress(source, out var sourceGCHandle),
                size);

            UnsafeUtility.ReleaseGCObject(sourceGCHandle);
            UnsafeUtility.ReleaseGCObject(destinationGCHandle);
        }

        /// <inheritdoc cref="MemCpyReplicate"/>
        public static void MemCpyReplicateStruct<T>(T[] destination, T[] source, int size, int count) where T : struct
        {
            MemCpyReplicate(
                PinGCArrayAndGetDataAddress(destination, out var destinationGCHandle),
                PinGCArrayAndGetDataAddress(source, out var sourceGCHandle),
                size, count);

            UnsafeUtility.ReleaseGCObject(sourceGCHandle);
            UnsafeUtility.ReleaseGCObject(destinationGCHandle);
        }

        /// <inheritdoc cref="MemCpyStride"/>
        public static void MemCpyStrideStruct<T>(T[] destination, int destinationStride, T[] source, int sourceStride, int elementSize, int count) where T : struct
        {
            MemCpyStride(
                PinGCArrayAndGetDataAddress(destination, out var destinationGCHandle),
                destinationStride,
                PinGCArrayAndGetDataAddress(source, out var sourceGCHandle),
                sourceStride,
                elementSize,
                count);

            UnsafeUtility.ReleaseGCObject(sourceGCHandle);
            UnsafeUtility.ReleaseGCObject(destinationGCHandle);
        }
        #endregion // ArrayToArray
        #endregion // Struct

        //https://github.com/needle-mirror/com.unity.physics/blob/master/Unity.Physics/Base/Containers/UnsafeEx.cs
        #region Unity.Physics
        public static unsafe long CalculateOffset<T, U>(ref T value, ref U baseValue)
            where T : struct
            where U : struct
        {
            return (byte*)UnsafeUtility.AddressOf(ref value)
                         - (byte*)UnsafeUtility.AddressOf(ref baseValue);
        }

        public static unsafe long CalculateOffset<T>(void* value, ref T baseValue)
            where T : struct
        {
            return (byte*)value - (byte*)UnsafeUtility.AddressOf(ref baseValue);
        }

        #region IntPtr
        public static long CalculateOffset<T>(IntPtr value, ref T baseValue)
            where T : struct
        {
            return value.ToInt64() - AddressOf(ref baseValue).ToInt64();
        }

        public static long CalculateOffset(IntPtr value, IntPtr baseValue)
        {
            return value.ToInt64() - baseValue.ToInt64();
        }
        #endregion // IntPtr
        #endregion // Unity.Physics

        #region CopyTo
        #region CopyToArray
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this List<T> list, T[] array) where T : struct
        {
            Assert.IsNotNull(list);
            Assert.IsNotNull(array);
            Assert.IsTrue(
                array.Length >= list.Count,
                $"Cannot copy {nameof(list)} of size {list.Count} into {nameof(array)} of size {array.Length}.");

            MemCpyStruct(
                array,
                NoAllocHelpers.ExtractArrayFromList(list),
                list.Count * UnsafeUtility.SizeOf<T>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this NativeArray<T> nativeArray, T[] array) where T : struct
        {
            Assert.IsTrue(nativeArray.IsCreated);
            Assert.IsNotNull(array);
            Assert.IsTrue(
                array.Length >= nativeArray.Length,
                $"Cannot copy {nameof(nativeArray)} of size {nativeArray.Length} into {nameof(array)} of size {array.Length}.");

            MemCpyStruct(
                array,
                nativeArray.GetIntPtr(),
                nativeArray.Length * UnsafeUtility.SizeOf<T>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this NativeList<T> nativeList, T[] array) where T : unmanaged
        {
            Assert.IsTrue(nativeList.IsCreated);
            Assert.IsNotNull(array);
            Assert.IsTrue(
                array.Length >= nativeList.Length,
                $"Cannot copy {nameof(nativeList)} of size {nativeList.Length} into {nameof(array)} of size {array.Length}.");

            MemCpy(
                array,
                nativeList.GetIntPtr(),
                nativeList.Length * UnsafeUtility.SizeOf<T>());
        }
        #endregion // CopyToArray

        #region CopyToList
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[] array, List<T> list) where T : struct
        {
            Assert.IsNotNull(array);
            Assert.IsNotNull(list);

            list.EnsureCapacity(array.Length);
            NoAllocHelpers.ResetListContents(list, array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this NativeArray<T> nativeArray, List<T> list) where T : struct
        {
            Assert.IsTrue(nativeArray.IsCreated);
            Assert.IsNotNull(list);

            list.EnsureCapacity(nativeArray.Length);
            NoAllocHelpers.ResetListContents(list, nativeArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this NativeList<T> nativeList, List<T> list) where T : unmanaged
        {
            Assert.IsTrue(nativeList.IsCreated);
            Assert.IsNotNull(list);

            list.EnsureCapacity(nativeList.Length);
            NoAllocHelpers.ResetListContents(list, nativeList.AsSpan());
        }
        #endregion // CopyToList

        #region CopyToNativeArray
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[] array, ref NativeArray<T> nativeArray) where T : struct
        {
            Assert.IsNotNull(array);
            Assert.IsTrue(nativeArray.IsCreated);

            Assert.IsTrue(
                nativeArray.Length >= array.Length,
                $"Cannot copy {nameof(array)} of size {array.Length} into {nameof(nativeArray)} of size {nativeArray.Length}.");

            NativeArray<T>.Copy(array, nativeArray, array.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this List<T> list, ref NativeArray<T> nativeArray) where T : struct
        {
            Assert.IsNotNull(list);
            Assert.IsTrue(nativeArray.IsCreated);

            Assert.IsTrue(
                nativeArray.Length >= list.Count,
                $"Cannot copy {nameof(list)} of size {list.Count} into {nameof(nativeArray)} of size {nativeArray.Length}.");

            NativeArray<T>.Copy(NoAllocHelpers.ExtractArrayFromList(list), nativeArray, list.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this NativeList<T> nativeList, ref NativeArray<T> nativeArray) where T : unmanaged
        {
            Assert.IsTrue(nativeList.IsCreated);
            Assert.IsTrue(nativeArray.IsCreated);

            Assert.IsTrue(
                nativeArray.Length >= nativeList.Length,
                $"Cannot copy {nameof(nativeList)} of size {nativeList.Length} into {nameof(nativeArray)} of size {nativeArray.Length}.");

            nativeArray.CopyFrom(nativeList.AsArray());
        }
        #endregion // CopyToNativeArray

        #region CopyToNativeList
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[] array, ref NativeList<T> nativeList) where T : unmanaged
        {
            Assert.IsNotNull(array);
            Assert.IsTrue(nativeList.IsCreated);

            Unity.Collections.NotBurstCompatible.Extensions.CopyFromNBC(nativeList, array);

            //nativeList.Clear();
            //nativeList.AddRange(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this List<T> list, ref NativeList<T> nativeList) where T : unmanaged
        {
            Assert.IsNotNull(list);
            Assert.IsTrue(nativeList.IsCreated);

            nativeList.Clear();
            nativeList.AddRange(list);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this NativeArray<T> nativeArray, ref NativeList<T> nativeList) where T : unmanaged
        {
            Assert.IsTrue(nativeArray.IsCreated);
            Assert.IsTrue(nativeList.IsCreated);

            nativeList.Clear();
            nativeList.CopyFrom(nativeArray);
        }
        #endregion // CopyToNativeList
        #endregion // CopyTo
    }
}
