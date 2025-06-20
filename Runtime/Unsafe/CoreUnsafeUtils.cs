using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace UnityExtensions.Unsafe
{
    /// <summary>
    /// Static class with unsafe utility functions.
    /// </summary>
    public static unsafe class CoreUnsafeUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T As<T>(this object from) where T : class
        {
            return System.Runtime.CompilerServices.Unsafe.As<T>(from);
        }
        
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Common/CoreUnsafeUtils.cs#L258
        #region UnityEngine.Rendering
        /// <summary>
        /// Fixed Buffer String Queue class.
        /// </summary>
        public struct FixedBufferStringQueue
        {
            byte* _readCursor;
            byte* _writeCursor;

            readonly byte* _bufferEnd;
            readonly byte* _bufferStart;
            readonly int _bufferLength;

            /// <summary>
            /// Number of element in the queue.
            /// </summary>
            public int Count { get; private set; }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="ptr">Buffer pointer.</param>
            /// <param name="length">Length of the provided allocated buffer in byte.</param>
            public FixedBufferStringQueue(byte* ptr, int length)
            {
                _bufferStart = ptr;
                _bufferLength = length;

                _bufferEnd = _bufferStart + _bufferLength;
                _readCursor = _bufferStart;
                _writeCursor = _bufferStart;
                Count = 0;
                Clear();
            }

            /// <summary>
            /// Try to push a new element in the queue.
            /// </summary>
            /// <param name="v">Element to push in the queue.</param>
            /// <returns>True if the new element could be pushed in the queue. False if reserved memory was not enough.</returns>
            public bool TryPush(string v)
            {
                var size = v.Length * sizeof(char) + sizeof(int);
                if (_writeCursor + size >= _bufferEnd)
                    return false;

                *(int*)_writeCursor = v.Length;
                _writeCursor += sizeof(int);

                var charPtr = (char*)_writeCursor;
                for (int i = 0; i < v.Length; ++i, ++charPtr)
                    *charPtr = v[i];

                _writeCursor += sizeof(char) * v.Length;
                ++Count;

                return true;
            }

            /// <summary>
            /// Pop an element of the queue.
            /// </summary>
            /// <param name="v">Output result string.</param>
            /// <returns>True if an element was successfully popped.</returns>
            public bool TryPop(out string v)
            {
                var size = *(int*)_readCursor;
                if (size != 0)
                {
                    _readCursor += sizeof(int);
                    v = new string((char*)_readCursor, 0, size);
                    _readCursor += size * sizeof(char);
                    return true;
                }

                v = default;
                return false;
            }

            /// <summary>
            /// Clear the queue.
            /// </summary>
            public void Clear()
            {
                _writeCursor = _bufferStart;
                _readCursor = _bufferStart;
                Count = 0;
                UnsafeUtility.MemClear(_bufferStart, _bufferLength);
            }
        }

        /// <summary>
        /// Key Getter interface.
        /// </summary>
        /// <typeparam name="TValue">Value</typeparam>
        /// <typeparam name="TKey">Key</typeparam>
        public interface IKeyGetter<TValue, TKey>
        {
            /// <summary>Getter</summary>
            /// <param name="v">The value</param>
            /// <returns>The key</returns>
            TKey Get(ref TValue v);
        }

        internal struct DefaultKeyGetter<T> : IKeyGetter<T, T>
        { public readonly T Get(ref T v) { return v; } }

        // Note: this is a workaround needed to circumvent some AOT issues when building for xbox
        internal struct UintKeyGetter : IKeyGetter<uint, uint>
        { public readonly uint Get(ref uint v) { return v; } }
        internal struct UlongKeyGetter : IKeyGetter<ulong, ulong>
        { public readonly ulong Get(ref ulong v) { return v; } }


        /// <summary>
        /// Extension method to copy elements of a list into a buffer.
        /// </summary>
        /// <typeparam name="T">Type of the provided List.</typeparam>
        /// <param name="list">Input List.</param>
        /// <param name="dest">Destination buffer.</param>
        /// <param name="count">Number of elements to copy.</param>
        public static void CopyTo<T>(this List<T> list, void* dest, int count)
            where T : struct
        {
            var c = min(count, list.Count);
            for (int i = 0; i < c; ++i)
                UnsafeUtility.WriteArrayElement(dest, i, list[i]);
        }

        /// <summary>
        /// Extension method to copy elements of an array into a buffer.
        /// </summary>
        /// <typeparam name="T">Type of the provided array.</typeparam>
        /// <param name="list">Input List.</param>
        /// <param name="dest">Destination buffer.</param>
        /// <param name="count">Number of elements to copy.</param>
        public static void CopyTo<T>(this T[] list, void* dest, int count)
            where T : struct
        {
            var c = min(count, list.Length);
            for (int i = 0; i < c; ++i)
                UnsafeUtility.WriteArrayElement(dest, i, list[i]);
        }

        private static void CalculateRadixParams(int radixBits, out int bitStates)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (radixBits != 2 && radixBits != 4 && radixBits != 8)
                throw new Exception("Radix bits must be 2, 4 or 8 for uint radix sort.");
#endif
            bitStates = 1 << radixBits;
        }

        private static int CalculateRadixSupportSize(int bitStates, int arrayLength)
        {
            return bitStates * 3 + arrayLength;
        }

        private static void CalculateRadixSortSupportArrays(
#pragma warning disable IDE0060 // Remove unused parameter
            int bitStates, int arrayLength, uint* supportArray,
#pragma warning restore IDE0060 // Remove unused parameter
            out uint* bucketIndices, out uint* bucketSizes, out uint* bucketPrefix, out uint* arrayOutput)
        {
            bucketIndices = supportArray;
            bucketSizes = bucketIndices + bitStates;
            bucketPrefix = bucketSizes + bitStates;
            arrayOutput = bucketPrefix + bitStates;
        }

        private static void MergeSort(uint* array, uint* support, int length)
        {
            for (int k = 1; k < length; k *= 2)
            {
                for (int left = 0; left + k < length; left += k * 2)
                {
                    int right = left + k;
                    int rightEnd = right + k;
                    if (rightEnd > length)
                        rightEnd = length;
                    int m = left;
                    int i = left;
                    int j = right;
                    while (i < right && j < rightEnd)
                    {
                        if (array[i] <= array[j])
                        {
                            support[m] = array[i++];
                        }
                        else
                        {
                            support[m] = array[j++];
                        }

                        m++;
                    }

                    while (i < right)
                    {
                        support[m] = array[i++];
                        m++;
                    }

                    while (j < rightEnd)
                    {
                        support[m] = array[j++];
                        m++;
                    }

                    for (m = left; m < rightEnd; m++)
                    {
                        array[m] = support[m];
                    }
                }
            }
        }

        /// <summary>
        /// Merge sort - non recursive
        /// </summary>
        /// <param name="arr">Array to sort.</param>
        /// <param name="sortSize">Size of the array to sort. If greater than array capacity, it will get clamped.</param>
        /// <param name="supportArray">Secondary array reference, used to store intermediate merge results.</param>
        public static void MergeSort(uint[] arr, int sortSize, ref uint[] supportArray)
        {
            if (arr == null)
                return;

            sortSize = min(sortSize, arr.Length);
            if (sortSize == 0)
                return;

            if (supportArray == null || supportArray.Length < sortSize)
                supportArray = new uint[sortSize];

            fixed (uint* arrPtr = arr)
            fixed (uint* supportPtr = supportArray)
                MergeSort(arrPtr, supportPtr, sortSize);
        }

        /// <summary>
        /// Merge sort - non recursive
        /// </summary>
        /// <param name="arr">Array to sort.</param>
        /// <param name="sortSize">Size of the array to sort. If greater than array capacity, it will get clamped.</param>
        /// <param name="supportArray">Secondary array reference, used to store intermediate merge results.</param>
        public static void MergeSort(NativeArray<uint> arr, int sortSize, ref NativeArray<uint> supportArray)
        {
            sortSize = min(sortSize, arr.Length);
            if (!arr.IsCreated || sortSize == 0)
                return;

            if (!supportArray.IsCreated || supportArray.Length < sortSize)
                supportArray.ResizeArray(arr.Length);

            MergeSort((uint*)arr.GetUnsafePtr(), (uint*)supportArray.GetUnsafePtr(), sortSize);
        }

        private static void InsertionSort(uint* arr, int length)
        {
            for (int i = 0; i < length; ++i)
            {
                for (int j = i; j >= 1; --j)
                {
                    if (arr[j] >= arr[j - 1])
                        break;

                    (arr[j], arr[j - 1]) = (arr[j - 1], arr[j]);
                }
            }
        }

        /// <summary>
        /// Insertion sort
        /// </summary>
        /// <param name="arr">Array to sort.</param>
        /// <param name="sortSize">Size of the array to sort. If greater than array capacity, it will get clamped.</param>
        public static void InsertionSort(uint[] arr, int sortSize)
        {
            if (arr == null)
                return;

            sortSize = min(arr.Length, sortSize);
            if (sortSize == 0)
                return;

            fixed (uint* ptr = arr)
                InsertionSort(ptr, sortSize);
        }

        /// <summary>
        /// Insertion sort
        /// </summary>
        /// <param name="arr">Array to sort.</param>
        /// <param name="sortSize">Size of the array to sort. If greater than array capacity, it will get clamped.</param>
        public static void InsertionSort(NativeArray<uint> arr, int sortSize)
        {
            sortSize = min(arr.Length, sortSize);
            if (!arr.IsCreated || sortSize == 0)
                return;

            InsertionSort((uint*)arr.GetUnsafePtr(), sortSize);
        }

        private static void RadixSort(uint* array, uint* support, int radixBits, int bitStates, int length)
        {
            uint mask = (uint)(bitStates - 1);
            CalculateRadixSortSupportArrays(bitStates, length, support, out uint* bucketIndices, out uint* bucketSizes, out uint* bucketPrefix, out uint* arrayOutput);

            int buckets = (sizeof(uint) * 8) / radixBits;
            uint* targetBuffer = arrayOutput;
            uint* inputBuffer = array;
            for (int b = 0; b < buckets; ++b)
            {
                int shift = b * radixBits;
                for (int s = 0; s < 3 * bitStates; ++s)
                    bucketIndices[s] = 0;//bucketSizes and bucketPrefix get zeroed, since we walk 3x the bit states

                for (int i = 0; i < length; ++i)
                    bucketSizes[((inputBuffer[i] >> shift) & mask)]++;

                for (int s = 1; s < bitStates; ++s)
                    bucketPrefix[s] = bucketPrefix[s - 1] + bucketSizes[s - 1];

                for (int i = 0; i < length; ++i)
                {
                    uint val = inputBuffer[i];
                    uint bucket = (val >> shift) & mask;
                    targetBuffer[bucketPrefix[bucket] + bucketIndices[bucket]++] = val;
                }

                uint* tmp = inputBuffer;
                inputBuffer = targetBuffer;
                targetBuffer = tmp;
            }
        }

        /// <summary>
        /// Radix sort or bucket sort, stable and non in-place.
        /// </summary>
        /// <param name="arr">Array to sort.</param>
        /// <param name="sortSize">Size of the array to sort. If greater than array capacity, it will get clamped.</param>
        /// <param name="supportArray">Array of uints that is used for support data. The algorithm will automatically allocate it if necessary.</param>
        /// <param name="radixBits">Number of bits to use for each bucket. Can only be 8, 4 or 2.</param>
        public static void RadixSort(uint[] arr, int sortSize, ref uint[] supportArray, int radixBits = 8)
        {
            if (arr == null)
                return;

            sortSize = min(sortSize, arr.Length);
            CalculateRadixParams(radixBits, out int bitStates);
            if (sortSize == 0)
                return;

            int supportSize = CalculateRadixSupportSize(bitStates, sortSize);
            if (supportArray == null || supportArray.Length < supportSize)
                supportArray = new uint[supportSize];

            fixed (uint* ptr = arr)
            fixed (uint* supportArrayPtr = supportArray)
                RadixSort(ptr, supportArrayPtr, radixBits, bitStates, sortSize);
        }

        /// <summary>
        /// Radix sort or bucket sort, stable and non in-place.
        /// </summary>
        /// <param name="array">Array to sort.</param>
        /// <param name="sortSize">Size of the array to sort. If greater than array capacity, it will get clamped.</param>
        /// <param name="supportArray">Array of uints that is used for support data. The algorithm will automatically allocate it if necessary.</param>
        /// <param name="radixBits">Number of bits to use for each bucket. Can only be 8, 4 or 2.</param>
        public static void RadixSort(NativeArray<uint> array, int sortSize, ref NativeArray<uint> supportArray, int radixBits = 8)
        {
            sortSize = min(sortSize, array.Length);
            CalculateRadixParams(radixBits, out int bitStates);
            if (!array.IsCreated || sortSize == 0)
                return;

            int supportSize = CalculateRadixSupportSize(bitStates, sortSize);
            if (!supportArray.IsCreated || supportArray.Length < supportSize)
                supportArray.ResizeArray(supportSize);

            RadixSort((uint*)array.GetUnsafePtr(), (uint*)supportArray.GetUnsafePtr(), radixBits, bitStates, sortSize);
        }

        /// <summary>
        /// Index of an element in a buffer.
        /// </summary>
        /// <typeparam name="T">Data type.</typeparam>
        /// <param name="data">Data buffer.</param>
        /// <param name="count">Number of elements.</param>
        /// <param name="v">Element to test against.</param>
        /// <returns>The first index of the provided element.</returns>
        public static int IndexOf<T>(void* data, int count, T v)
            where T : struct, IEquatable<T>
        {
            for (int i = 0; i < count; ++i)
            {
                if (UnsafeUtility.ReadArrayElement<T>(data, i).Equals(v))
                    return i;
            }

            return -1;
        }

        #region IntPtr
        /// <inheritdoc cref="CopyTo"/>
        public static void CopyTo<T>(this List<T> list, IntPtr dest, int count)
            where T : struct
        {
            CopyTo(list, (void*)dest, count);
        }

        /// <inheritdoc cref="CopyTo"/>
        public static void CopyTo<T>(this T[] list, IntPtr dest, int count)
            where T : struct
        {
            CopyTo(list, (void*)dest, count);
        }

        /// <inheritdoc cref="IndexOf"/>
        public static int IndexOf<T>(IntPtr data, int count, T v)
            where T : struct, IEquatable<T>
        {
            return IndexOf((void*)data, count, v);
        }
        #endregion // IntPtr
        #endregion // UnityEngine.Rendering
    }
}
