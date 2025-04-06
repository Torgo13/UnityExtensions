// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace UnityExtensions.Unsafe
{
    public static class UnsafeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T UnsafeElementAt<T>(this T[] array, int index) where T : struct
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(UnsafeUtility.AddressOf(ref array[0]), index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T UnsafeElementAt<T>(this List<T> list, int index) where T : struct
        {
            return ref NoAllocHelpers.ExtractArrayFromList(list).UnsafeElementAt(index);
        }

        //https://github.com/Unity-Technologies/UnityCsReference/blob/b42ec0031fc505c35aff00b6a36c25e67d81e59e/Runtime/Export/Unsafe/UnsafeUtility.cs
        #region Unity.Collections.LowLevel.Unsafe
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
                    ret.Append(GetReasonForTypeNonBlittableImpl(f.FieldType, $"{name}.{f.Name}"));
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> GetByteSpanFromList<T>(List<T> list) where T : struct
        {
            return MemoryMarshal.AsBytes(NoAllocHelpers.ExtractArrayFromList(list).AsSpan(0, list.Count));
        }
        #endregion // Unity.Collections.LowLevel.Unsafe

        #region ToArray
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyTo<T>(this List<T> list, T[] array) where T : struct
        {
            Assert.IsNotNull(list);
            Assert.IsNotNull(array);
            Assert.IsTrue(
                array.Length >= list.Count,
                $"Cannot copy {nameof(list)} of size {list.Count} into {nameof(array)} of size {array.Length}.");

            UnsafeUtility.MemCpy(
                UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var arrayGCHandle),
                UnsafeUtility.PinGCArrayAndGetDataAddress(NoAllocHelpers.ExtractArrayFromList(list), out var listGCHandle),
                list.Count * UnsafeUtility.SizeOf<T>());

            UnsafeUtility.ReleaseGCObject(arrayGCHandle);
            UnsafeUtility.ReleaseGCObject(listGCHandle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyTo<T>(this NativeArray<T> nativeArray, T[] array) where T : struct
        {
            Assert.IsTrue(nativeArray.IsCreated);
            Assert.IsNotNull(array);
            Assert.IsTrue(
                array.Length >= nativeArray.Length,
                $"Cannot copy {nameof(nativeArray)} of size {nativeArray.Length} into {nameof(array)} of size {array.Length}.");

            UnsafeUtility.MemCpy(
                UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var arrayGCHandle),
                nativeArray.GetUnsafePtr(),
                nativeArray.Length * UnsafeUtility.SizeOf<T>());

            UnsafeUtility.ReleaseGCObject(arrayGCHandle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyTo<T>(this NativeList<T> nativeList, T[] array) where T : unmanaged
        {
            Assert.IsTrue(nativeList.IsCreated);
            Assert.IsNotNull(array);
            Assert.IsTrue(
                array.Length >= nativeList.Length,
                $"Cannot copy {nameof(nativeList)} of size {nativeList.Length} into {nameof(array)} of size {array.Length}.");

            UnsafeUtility.MemCpy(
                UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var arrayGCHandle),
                nativeList.GetUnsafePtr(),
                nativeList.Length * UnsafeUtility.SizeOf<T>());

            UnsafeUtility.ReleaseGCObject(arrayGCHandle);
        }
        #endregion // ToArray

        #region ToList
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[] array, List<T> list) where T : struct
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Assert.IsNotNull(array);
            Assert.IsNotNull(list);
#endif // ENABLE_UNITY_COLLECTIONS_CHECKS

            list.EnsureCapacity(array.Length);
            NoAllocHelpers.ResetListContents(list, array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this NativeArray<T> nativeArray, List<T> list) where T : struct
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Assert.IsTrue(nativeArray.IsCreated);
            Assert.IsNotNull(list);
#endif // ENABLE_UNITY_COLLECTIONS_CHECKS

            list.EnsureCapacity(nativeArray.Length);
            NoAllocHelpers.ResetListContents(list, nativeArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this NativeList<T> nativeList, List<T> list) where T : unmanaged
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Assert.IsTrue(nativeList.IsCreated);
            Assert.IsNotNull(list);
#endif // ENABLE_UNITY_COLLECTIONS_CHECKS

            list.EnsureCapacity(nativeList.Length);
            NoAllocHelpers.ResetListContents(list, nativeList);
        }
        #endregion // ToList

        #region ToNativeArray
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[] array, ref NativeArray<T> nativeArray) where T : struct
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Assert.IsNotNull(array);
            Assert.IsTrue(nativeArray.IsCreated);
#endif // ENABLE_UNITY_COLLECTIONS_CHECKS

            Assert.IsTrue(
                nativeArray.Length >= array.Length,
                $"Cannot copy {nameof(array)} of size {array.Length} into {nameof(nativeArray)} of size {nativeArray.Length}.");

            NativeArray<T>.Copy(array, nativeArray, array.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this List<T> list, ref NativeArray<T> nativeArray) where T : struct
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Assert.IsNotNull(list);
            Assert.IsTrue(nativeArray.IsCreated);
#endif // ENABLE_UNITY_COLLECTIONS_CHECKS

            Assert.IsTrue(
                nativeArray.Length >= list.Count,
                $"Cannot copy {nameof(list)} of size {list.Count} into {nameof(nativeArray)} of size {nativeArray.Length}.");

            NativeArray<T>.Copy(NoAllocHelpers.ExtractArrayFromList(list), nativeArray, list.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this NativeList<T> nativeList, ref NativeArray<T> nativeArray) where T : unmanaged
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Assert.IsTrue(nativeList.IsCreated);
            Assert.IsTrue(nativeArray.IsCreated);
#endif // ENABLE_UNITY_COLLECTIONS_CHECKS

            Assert.IsTrue(
                nativeArray.Length >= nativeList.Length,
                $"Cannot copy {nameof(nativeList)} of size {nativeList.Length} into {nameof(nativeArray)} of size {nativeArray.Length}.");

            nativeArray.CopyFrom(nativeList.AsArray());
        }
        #endregion // ToNativeArray

        #region ToNativeList
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[] array, ref NativeList<T> nativeList) where T : unmanaged
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Assert.IsNotNull(array);
            Assert.IsTrue(nativeList.IsCreated);
#endif // ENABLE_UNITY_COLLECTIONS_CHECKS

            nativeList.Clear();
            nativeList.AddRange(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this List<T> list, ref NativeList<T> nativeList) where T : unmanaged
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Assert.IsNotNull(list);
            Assert.IsTrue(nativeList.IsCreated);
#endif // ENABLE_UNITY_COLLECTIONS_CHECKS

            nativeList.Clear();
            nativeList.AddRange(list);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this NativeArray<T> nativeArray, ref NativeList<T> nativeList) where T : unmanaged
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Assert.IsTrue(nativeArray.IsCreated);
            Assert.IsTrue(nativeList.IsCreated);
#endif // ENABLE_UNITY_COLLECTIONS_CHECKS

            nativeList.Clear();
            nativeList.CopyFrom(nativeArray);
        }
        #endregion // ToNativeList
    }
}
