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
using PKGE.Packages;

namespace PKGE.Unsafe
{
    public static partial class UnsafeExtensions
    {
        //https://github.com/Unity-Technologies/UnityCsReference/blob/b42ec0031fc505c35aff00b6a36c25e67d81e59e/Runtime/Export/Unsafe/UnsafeUtility.cs
        #region Unity.Collections.LowLevel.Unsafe
        #region Blittable
        public static bool IsBlittableValueType(Type t) { return t.IsValueType && UnsafeUtility.IsBlittable(t); }

#if USING_REFLECTION
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
#endif // USING_REFLECTION

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

#if USING_REFLECTION
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
#endif // USING_REFLECTION
        #endregion // Blittable
        #endregion // Unity.Collections.LowLevel.Unsafe

        #region Unmanaged
        #region ArrayToArray
        /// <inheritdoc cref="MemCpy"/>
        public unsafe static void MemCpy<T>(Span<T> destination, Span<T> source, long size) where T : unmanaged
        {
            fixed (void* p0 = source)
            fixed (void* p1 = destination)
            {
                UnsafeUtility.MemCpy(p1, p0, size);
            }
        }

        /// <inheritdoc cref="MemCpyReplicate"/>
        public unsafe static void MemCpyReplicate<T>(Span<T> destination, Span<T> source, int size, int count) where T : unmanaged
        {
            fixed (void* p0 = source)
            fixed (void* p1 = destination)
            {
                UnsafeUtility.MemCpyReplicate(p1, p0, size, count);
            }
        }

        /// <inheritdoc cref="MemCpyStride"/>
        public unsafe static void MemCpyStride<T>(Span<T> destination, int destinationStride, Span<T> source, int sourceStride, int elementSize, int count) where T : unmanaged
        {
            fixed (void* p0 = source)
            fixed (void* p1 = destination)
            {
                UnsafeUtility.MemCpyStride(p1, destinationStride, p0, sourceStride, elementSize, count);
            }
        }
        #endregion // ArrayToArray
        #endregion // Unmanaged

        #region Struct
        #region ArrayToArray
        /// <inheritdoc cref="MemCpy"/>
        public static unsafe void MemCpyStruct<T>(Span<T> destination, Span<T> source, long size) where T : struct
        {
            UnsafeUtility.MemCpy(
                UnsafeUtility.AddressOf(ref destination[0]),
                UnsafeUtility.AddressOf(ref source[0]),
                size);
        }

        /// <inheritdoc cref="MemCpyReplicate"/>
        public static unsafe void MemCpyReplicateStruct<T>(Span<T> destination, Span<T> source, int size, int count) where T : struct
        {
            UnsafeUtility.MemCpyReplicate(
                UnsafeUtility.AddressOf(ref destination[0]),
                UnsafeUtility.AddressOf(ref source[0]),
                size,
                count);
        }

        /// <inheritdoc cref="MemCpyStride"/>
        public static unsafe void MemCpyStrideStruct<T>(Span<T> destination, int destinationStride, Span<T> source, int sourceStride, int elementSize, int count) where T : struct
        {
            UnsafeUtility.MemCpyStride(
                UnsafeUtility.AddressOf(ref destination[0]),
                destinationStride,
                UnsafeUtility.AddressOf(ref source[0]),
                sourceStride,
                elementSize,
                count);
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
                list.AsSpan(),
                list.Count * SizeOfCache<T>.Size);
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
                nativeArray.AsSpan(),
                nativeArray.Length * SizeOfCache<T>.Size);
        }

#if INCLUDE_COLLECTIONS
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
                nativeList.AsSpan(),
                nativeList.Length * SizeOfCache<T>.Size);
        }
#endif // INCLUDE_COLLECTIONS
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
        
#if INCLUDE_COLLECTIONS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this NativeList<T> nativeList, List<T> list) where T : unmanaged
        {
            Assert.IsTrue(nativeList.IsCreated);
            Assert.IsNotNull(list);

            list.EnsureCapacity(nativeList.Length);
            NoAllocHelpers.ResetListContents(list, nativeList.AsSpan());
        }
#endif // INCLUDE_COLLECTIONS
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

#if INCLUDE_COLLECTIONS
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
#endif // INCLUDE_COLLECTIONS
#endregion // CopyToNativeArray

#if INCLUDE_COLLECTIONS
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
#endif // INCLUDE_COLLECTIONS
#endregion // CopyTo
    }
}
