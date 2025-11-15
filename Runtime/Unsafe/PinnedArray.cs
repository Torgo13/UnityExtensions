using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;

namespace PKGE.Unsafe
{
    public readonly struct PinnedArray<T> : IDisposable where T : struct
    {
        //https://github.com/Unity-Technologies/Graphics/blob/e1261529950630672ed38500dd626d4f4298b7bc/Packages/com.unity.render-pipelines.universal/Runtime/Memory/PinnedArray.cs
        #region UnityEngine.Rendering.Universal
        public readonly T[] managedArray;
        public readonly NativeArray<T> nativeArray;
        private readonly ulong handle;

        public int Length => nativeArray.Length;

        public PinnedArray(int length)
        {
            Assert.IsTrue(length > 0);
            managedArray = new T[length];
            unsafe
            {
                nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(
                    UnsafeUtility.PinGCArrayAndGetDataAddress(managedArray, out handle), length, Allocator.None);
            }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.Create());
#endif
        }

        public PinnedArray(T[] array, int length)
        {
            Assert.IsNotNull(array);
            Assert.IsTrue(length > 0);
            managedArray = array;
            unsafe
            {
                nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(
                    UnsafeUtility.PinGCArrayAndGetDataAddress(managedArray, out handle), length, Allocator.None);
            }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.Create());
#endif
        }

        public PinnedArray(T[] array)
        {
            Assert.IsNotNull(array);
            managedArray = array;
            unsafe
            {
                nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(
                    UnsafeUtility.PinGCArrayAndGetDataAddress(managedArray, out handle), array.Length, Allocator.None);
            }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.Create());
#endif
        }
        
        public PinnedArray(List<T> list)
        {
            Assert.IsNotNull(list);
            managedArray = list.ExtractArrayFromList();
            unsafe
            {
                nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(
                    UnsafeUtility.PinGCArrayAndGetDataAddress(managedArray, out handle), list.Count, Allocator.None);
            }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.Create());
#endif
        }

        public void Dispose()
        {
            if (managedArray == null)
                return;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.Release(NativeArrayUnsafeUtility.GetAtomicSafetyHandle(nativeArray));
#endif
            UnsafeUtility.ReleaseGCObject(handle);
        }
        #endregion // UnityEngine.Rendering.Universal
    }

    public static partial class ArrayExtensions
    {
        public static PinnedArray<T> AsNativeArray<T>(this T[] array, int length, out NativeArray<T> nativeArray)
            where T : struct
        {
            var pinnedArray = new PinnedArray<T>(array, length);
            nativeArray = pinnedArray.nativeArray;
            return pinnedArray;
        }

        public static PinnedArray<T> AsNativeArray<T>(this T[] array, out NativeArray<T> nativeArray)
            where T : struct
        {
            var pinnedArray = new PinnedArray<T>(array);
            nativeArray = pinnedArray.nativeArray;
            return pinnedArray;
        }
    }

    public static partial class ListExtensions
    {
        public static PinnedArray<T> AsNativeArray<T>(this List<T> list, out NativeArray<T> nativeArray)
            where T : struct
        {
            var pinnedArray = new PinnedArray<T>(list);
            nativeArray = pinnedArray.nativeArray;
            return pinnedArray;
        }
    }
}
