using System;
using UnityEngine.Assertions;

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
}
