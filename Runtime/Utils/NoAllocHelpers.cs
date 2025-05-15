using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;
using UnityEngine.Scripting;

namespace UnityExtensions
{
    // Non-allocating sorts.
    public struct Sorting
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.universal/Runtime/NoAllocUtils.cs
        #region UnityEngine.Rendering.Universal
        // Add profiling samplers to sorts as they are often a bottleneck when scaling things up.
        // By default, avoid sampling recursion, but these can be used externally as well.
        static readonly ProfilingSampler QuickSortSampler = new ProfilingSampler("QuickSort");
        static readonly ProfilingSampler InsertionSortSampler = new ProfilingSampler("InsertionSort");

        public static void QuickSort<T>(T[] data, Func<T, T, int> compare)
        {
            using var scope = new ProfilingScope(QuickSortSampler);
            QuickSort(data, 0, data.Length - 1, compare);
        }

        // TODO: parallel alternative
        /// <summary>
        /// A non-allocating predicated sub-array quick sort for managed arrays.
        /// </summary>
        /// <remarks>
        /// Similar to UnityEngine.Rendering.CoreUnsafeUtils.QuickSort in CoreUnsafeUtils.cs
        /// </remarks>>
        /// <example><code>
        /// Sorting.QuickSort(test, 0, test.Length - 1, (int a, int b) => a - b);
        /// </code></example>
        /// <param name="data"></param>
        /// <param name="start"></param>
        /// <param name="end">The end param is inclusive.</param>
        /// <param name="compare">Returns -1, 0 or 1.</param>
        public static void QuickSort<T>(T[] data, int start, int end, Func<T, T, int> compare)
        {
            while (true)
            {
                int diff = end - start;
                if (diff < 1)
                    return;
                if (diff < 8)
                {
                    InsertionSort(data, start, end, compare);
                    return;
                }

                Assert.IsTrue((uint)start < data.Length);
                Assert.IsTrue((uint)end < data.Length); // end == inclusive

                if (start < end)
                {
                    int pivot = Partition(data, start, end, compare);

                    if (pivot >= 1)
                        QuickSort(data, start, pivot, compare);

                    if (pivot + 1 < end)
                    {
                        start = pivot + 1;
                        continue;
                    }
                }

                break;
            }
        }

        static T Median3Pivot<T>(T[] data, int start, int pivot, int end, Func<T, T, int> compare)
        {
            if (compare(data[end], data[start]) < 0) Swap(start, end);
            if (compare(data[pivot], data[start]) < 0) Swap(start, pivot);
            if (compare(data[end], data[pivot]) < 0) Swap(pivot, end);
            return data[pivot];

            void Swap(int a, int b)
            {
                (data[a], data[b]) = (data[b], data[a]);
            }
        }

        static int Partition<T>(T[] data, int start, int end, Func<T, T, int> compare)
        {
            int diff = end - start;
            int pivot = start + diff / 2;

            var pivotValue = Median3Pivot(data, start, pivot, end, compare);

            while (true)
            {
                while (compare(data[start], pivotValue) < 0) ++start;
                while (compare(data[end], pivotValue) > 0) --end;

                if (start >= end)
                {
                    return end;
                }

                var tmp = data[start];
                data[start++] = data[end];
                data[end--] = tmp;
            }
        }

        public static void InsertionSort<T>(T[] data, Func<T, T, int> compare)
        {
            using var scope = new ProfilingScope(InsertionSortSampler);
            InsertionSort(data, 0, data.Length - 1, compare);
        }

        /// <summary>
        /// A non-allocating predicated sub-array insertion sort for managed arrays.
        /// </summary>
        /// <remarks>Called also from QuickSort for small ranges.</remarks>
        public static void InsertionSort<T>(T[] data, int start, int end, Func<T, T, int> compare)
        {
            Assert.IsTrue((uint)start < data.Length);
            Assert.IsTrue((uint)end < data.Length);

            for (int i = start + 1; i < end + 1; i++)
            {
                var iData = data[i];
                int j = i - 1;
                while (j >= 0 && compare(iData, data[j]) < 0)
                {
                    data[j + 1] = data[j];
                    j--;
                }
                data[j + 1] = iData;
            }
        }
        #endregion // UnityEngine.Rendering.Universal
    }

    /// <summary>
    /// <para>Some helpers to handle <see cref="List{T}"/> in C# API (used for no-alloc APIs where user provides the list to be filled).</para>
    /// <para>On il2cpp/mono we can "resize" <see cref="List{T}"/> (up to <see cref="List{T}.Capacity"/>, sure, but this is/should-be handled higher level).</para>
    /// <para>Also, we can easily "convert" <see cref="List{T}"/> to <see cref="Array"/>.</para>
    /// </summary>
    /// <remarks>
    /// NB .NET backend is treated as second-class citizen going through <see cref="List{T}.ToArray()"/> call.
    /// </remarks>
    public static class NoAllocHelpers
    {
        //https://github.com/Unity-Technologies/UnityCsReference/blob/b42ec0031fc505c35aff00b6a36c25e67d81e59e/Runtime/Export/Scripting/NoAllocHelpers.bindings.cs
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
        public static int SafeLength(Array values) { return values != null ? values.Length : 0; }
        public static int SafeLength<T>(List<T> values) { return values != null ? values.Count : 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] ExtractArrayFromList<T>(List<T> list)
        {
            if (list == null)
                return null;

            var tListAccess = Unsafe.As<ListPrivateFieldAccess<T>>(list);
            return tListAccess._items;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetListContents<T>(List<T> list, ReadOnlySpan<T> span)
        {
            var tListAccess = Unsafe.As<ListPrivateFieldAccess<T>>(list);

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
            var tListAccess = Unsafe.As<ListPrivateFieldAccess<T>>(list);
            tListAccess._size = size;
            tListAccess._version++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetListSizeNoResize<T>(List<T> list, int size) where T : unmanaged
        {
            Assert.IsTrue(list.Capacity >= size);

            var tListAccess = UnsafeUtility.As<List<T>, ListPrivateFieldAccess<T>>(ref list);
            tListAccess._size = size;
            tListAccess._version++;
        }

        /// <summary>
        /// This is a helper class to allow the binding code to manipulate the internal fields of
        /// System.Collections.Generic.List. The field order below must not be changed.
        /// </summary>
        [Preserve]
        internal class ListPrivateFieldAccess<T>
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
    }
}
