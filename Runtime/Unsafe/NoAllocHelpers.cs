// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityExtensions.Unsafe
{
    /// <summary>
    /// Some helpers to handle List&lt;T&gt; in C# api (used for no-alloc apis where user provides list and we fill it):
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
                throw new ArgumentNullException(nameof(list), "list");

            if (count < 0)
                throw new ArgumentException("invalid size to resize.", nameof(list));

            list.Clear();

            // make sure capacity is enough (that's where alloc WILL happen if needed)
            if (list.Capacity < count)
                list.Capacity = count;

            if (count != list.Count)
            {
                ListPrivateFieldAccess<T> tListAccess = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<List<T>, ListPrivateFieldAccess<T>>(ref list);
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

            var tListAccess = UnsafeUtility.As<ListPrivateFieldAccess<T>>(list);
            return tListAccess._items;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetListContents<T>(List<T> list, ReadOnlySpan<T> span)
        {
            var tListAccess = UnsafeUtility.As<ListPrivateFieldAccess<T>>(list);
            tListAccess._items = span.ToArray();
            tListAccess._size = span.Length;
            tListAccess._version++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetListSize<T>(List<T> list, int size) where T : unmanaged
        {
            Debug.Assert(list.Capacity >= size);

            var tListAccess = UnsafeUtility.As<ListPrivateFieldAccess<T>>(list);
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
#pragma warning disable S4487
            internal int _size; // Do not rename (binary serialization)
            internal int _version; // Do not rename (binary serialization)
#pragma warning restore S4487
#pragma warning restore CS0649
        }
        #endregion // UnityEngine
    }
}
