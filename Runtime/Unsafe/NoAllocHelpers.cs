// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe
{
    /// <summary>
    /// Some helpers to handle List&lt;T&gt; in C# api (used for no-alloc apis where user provides the list to be filled):
    /// on il2cpp/mono we can "resize" List&lt;T&gt; (up to Capacity, sure, but this is/should-be handled higher level)
    /// also we can easily "convert" List&lt;T&gt; to System.Array
    /// </summary>
    /// <remarks>
    /// NB .NET backend is treated as second-class citizen going through ToArray call
    /// </remarks>
    public static class NoAllocHelpers
    {
        //https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Scripting/NoAllocHelpers.bindings.cs
        #region UnityEngine
        public static void EnsureListElemCount<T>(List<T> list, int count)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (count < 0)
                throw new ArgumentException($"{nameof(count)} must not be negative.", nameof(count));

            list.Clear();

            // make sure capacity is enough (that's where alloc WILL happen if needed)
            if (list.Capacity < count)
                list.Capacity = count;

            if (count != list.Count)
            {
                ListPrivateFieldAccess<T> tListAccess = UnsafeUtility.As<List<T>, ListPrivateFieldAccess<T>>(ref list);
                tListAccess._size = count;
                tListAccess._version++;
            }
        }

        // tiny helpers
        public static int SafeLength(System.Array values) { return values != null ? values.Length : 0; }
        public static int SafeLength<T>(List<T> values) { return values != null ? values.Count : 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] ExtractArrayFromList<T>(List<T> list)
        {
            if (list == null)
                return null;

            var tListAccess = CoreUnsafeUtils.As<ListPrivateFieldAccess<T>>(list);
            return tListAccess._items;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetListContents<T>(List<T> list, ReadOnlySpan<T> span)
        {
            var tListAccess = CoreUnsafeUtils.As<ListPrivateFieldAccess<T>>(list);
            
            // Do not reallocate the _items array if it is already
            // large enough to contain all the elements of span
            if (tListAccess._items.Length >= span.Length)
                span.CopyTo(tListAccess._items);
            else
                tListAccess._items = span.ToArray();
            
            tListAccess._size = span.Length;
            tListAccess._version++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetListSize<T>(List<T> list, int size) where T : unmanaged
        {
            Assert.IsTrue(list.Capacity >= size);

            var tListAccess = CoreUnsafeUtils.As<ListPrivateFieldAccess<T>>(list);
            tListAccess._size = size;
            tListAccess._version++;
        }

        /// <summary>
        /// This is a helper class to allow the binding code to manipulate the internal fields of
        /// System.Collections.Generic.List. The field order below must not be changed.
        /// </summary>
        private class ListPrivateFieldAccess<T>
        {
#pragma warning disable CS0649
#pragma warning disable CS8618
            internal T[] _items; // Do not rename (binary serialization)
#pragma warning restore CS8618
            internal int _size; // Do not rename (binary serialization)
            internal int _version; // Do not rename (binary serialization)
#pragma warning restore CS0649
        }
        #endregion // UnityEngine
        
        /// <remarks>
        /// Set the Capacity before calling this function.
        /// </remarks>
        public static unsafe void ResetListContents<T>(List<T> list, T[] array) where T : struct
        {
            var tListAccess = UnsafeUtility.As<List<T>, ListPrivateFieldAccess<T>>(ref list);
            tListAccess._size = array.Length;

            UnsafeUtility.MemCpy(
                UnsafeUtility.PinGCArrayAndGetDataAddress(tListAccess._items, out var listGCHandle),
                UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var arrayGCHandle),
                tListAccess._size * UnsafeUtility.SizeOf<T>());

            tListAccess._version++;

            UnsafeUtility.ReleaseGCObject(arrayGCHandle);
            UnsafeUtility.ReleaseGCObject(listGCHandle);
        }

        /// <remarks>
        /// Set the Capacity before calling this function.
        /// </remarks>
        public static unsafe void ResetListContents<T>(List<T> list, NativeArray<T> array) where T : struct
        {
            var tListAccess = UnsafeUtility.As<List<T>, ListPrivateFieldAccess<T>>(ref list);
            tListAccess._size = array.Length;

            UnsafeUtility.MemCpy(
                UnsafeUtility.PinGCArrayAndGetDataAddress(tListAccess._items, out var listGCHandle),
                array.GetUnsafeReadOnlyPtr(),
                tListAccess._size * UnsafeUtility.SizeOf<T>());

            tListAccess._version++;

            UnsafeUtility.ReleaseGCObject(listGCHandle);
        }

        /// <remarks>
        /// Set the Capacity before calling this function.
        /// </remarks>
        public static unsafe void ResetListContents<T>(List<T> list, NativeList<T> nativeList) where T : unmanaged
        {
            var tListAccess = UnsafeUtility.As<List<T>, ListPrivateFieldAccess<T>>(ref list);
            tListAccess._size = nativeList.Length;

            fixed (T* listPtr = &tListAccess._items[0])
            {
                UnsafeUtility.MemCpy(listPtr, nativeList.GetUnsafeReadOnlyPtr(), tListAccess._size * sizeof(T));
            }

            tListAccess._version++;
        }
    }
}
