using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Assertions;
using UnityEngine.Jobs;

namespace UnityExtensions
{
    public static class ArrayExtensions
    {
        //https://github.com/Unity-Technologies/Graphics/blob/95e018183e0f74dc34855606bf3287b41ee6e6ab/Packages/com.unity.render-pipelines.core/Runtime/Utilities/ArrayExtensions.cs
        #region UnityEngine.Rendering
        /// <summary>
        /// Resizes a native array. If an empty native array is passed, it will create a new one.
        /// </summary>
        /// <typeparam name="T">The type of the array</typeparam>
        /// <param name="array">Target array to resize</param>
        /// <param name="capacity">New size of native array to resize</param>
        /// <param name="allocator">NativeArray allocator type</param>
        public static void ResizeArray<T>(this ref NativeArray<T> array, int capacity, Allocator allocator = Allocator.Temp) where T : struct
        {
            var newArray = new NativeArray<T>(capacity, allocator, NativeArrayOptions.UninitializedMemory);
            if (array.IsCreated)
            {
                NativeArray<T>.Copy(array, newArray, array.Length);
                array.Dispose();
            }
            array = newArray;
        }

        /// <summary>
        /// Resizes a transform access array.
        /// </summary>
        /// <param name="array">Target array to resize</param>
        /// <param name="capacity">New size of transform access array to resize</param>
        public static void ResizeArray(this ref TransformAccessArray array, int capacity)
        {
            var newArray = new TransformAccessArray(capacity);
            if (array.isCreated)
            {
                for (int i = 0; i < array.length; ++i)
                    newArray.Add(array[i]);

                array.Dispose();
            }
            array = newArray;
        }

        /// <summary>
        /// Resizes an array.
        /// </summary>
        /// <typeparam name="T">The type of the array</typeparam>
        /// <param name="array">Target array to resize</param>
        /// <param name="capacity">New size of array to resize</param>
        public static void ResizeArray<T>(this T[] array, int capacity)
        {
            Array.Resize(ref array, capacity);
        }
        #endregion // UnityEngine.Rendering
        
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/ecad5ff79b1fa55162c23108029609b16e9ffe6d/InternalPackages/com.unity.video-streaming.client/Runtime/Utils/ArrayUtils.cs
        #region Unity.LiveCapture.VideoStreaming.Client.Utils
        public static bool IsBytesEquals(this byte[] bytes1, int offset1, int count1, byte[] bytes2, int offset2, int count2)
        {
            if (count1 != count2)
                return false;

            for (int i = 0; i < count1; i++)
            {
                if (bytes1[offset1 + i] != bytes2[offset2 + i])
                    return false;
            }

            return true;
        }

        public static bool StartsWith(this byte[] array, int offset, int count, byte[] pattern)
        {
            int patternLength = pattern.Length;

            if (count < patternLength)
                return false;

            for (int i = 0; i < patternLength; i++, offset++)
            {
                if (array[offset] != pattern[i])
                    return false;
            }

            return true;
        }

        public static bool EndsWith(this byte[] array, int offset, int count, byte[] pattern)
        {
            int patternLength = pattern.Length;

            if (count < patternLength)
                return false;

            offset = offset + count - patternLength;

            for (int i = 0; i < patternLength; i++, offset++)
            {
                if (array[offset] != pattern[i])
                    return false;
            }

            return true;
        }

        public static int IndexOfBytes(this byte[] array, byte[] pattern, int startIndex, int count)
        {
            int patternLength = pattern.Length;

            if (count < patternLength)
                return -1;

            int endIndex = startIndex + count;

            int foundIndex = 0;
            for (; startIndex < endIndex; startIndex++)
            {
                if (array[startIndex] != pattern[foundIndex])
                    foundIndex = 0;
                else if (++foundIndex == patternLength)
                    return startIndex - foundIndex + 1;
            }

            return -1;
        }
        #endregion // Unity.LiveCapture.VideoStreaming.Client.Utils
        
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/ecad5ff79b1fa55162c23108029609b16e9ffe6d/InternalPackages/com.unity.video-streaming.client/Runtime/Utils/ArraySegmentExtensions.cs
        #region Unity.LiveCapture.VideoStreaming.Client.Utils
        public static ArraySegment<T> SubSegment<T>(this ArraySegment<T> arraySegment, int offset)
        {
            UnityEngine.Debug.Assert(arraySegment.Array != null, "arraySegment.Array != null");
            return new ArraySegment<T>(arraySegment.Array, arraySegment.Offset + offset, arraySegment.Count - offset);
        }
        #endregion // Unity.LiveCapture.VideoStreaming.Client.Utils
        
        //https://github.com/needle-mirror/com.unity.addressables/blob/b9b97fefbdf24fe7f86d2f50efae7f0fd5a1bba7/Runtime/Utility/SerializationUtilities.cs
        #region UnityEngine.AddressableAssets.Utility
        public static int ReadInt32FromByteArray(this byte[] data, int offset)
        {
            return data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);
        }

        public static int WriteInt32ToByteArray(this byte[] data, int val, int offset)
        {
            data[offset] = (byte)(val & 0xFF);
            data[offset + 1] = (byte)((val >> 8) & 0xFF);
            data[offset + 2] = (byte)((val >> 16) & 0xFF);
            data[offset + 3] = (byte)((val >> 24) & 0xFF);
            return offset + 4;
        }
        #endregion // UnityEngine.AddressableAssets.Utility

        //https://github.com/Unity-Technologies/InputSystem/blob/fb786d2a7d01b8bcb8c4218522e5f4b9afea13d7/Packages/com.unity.inputsystem/InputSystem/Utilities/ArrayHelpers.cs
        #region UnityEngine.InputSystem.Utilities
        public static int LengthSafe<TValue>(this TValue[] array)
        {
            if (array == null)
                return 0;
            return array.Length;
        }

        public static void Clear<TValue>(this TValue[] array)
        {
            if (array == null)
                return;

            Array.Clear(array, 0, array.Length);
        }

        public static void Clear<TValue>(this TValue[] array, int count)
        {
            if (array == null)
                return;
            Array.Clear(array, 0, count);
        }

        public static void Clear<TValue>(this TValue[] array, ref int count)
        {
            if (array == null)
                return;

            Array.Clear(array, 0, count);
            count = 0;
        }

        public static void EnsureCapacity<TValue>(ref TValue[] array, int count, int capacity, int capacityIncrement = 10)
        {
            if (capacity == 0)
                return;

            if (array == null)
            {
                array = new TValue[Math.Max(capacity, capacityIncrement)];
                return;
            }

            var currentCapacity = array.Length - count;
            if (currentCapacity >= capacity)
                return;

            DuplicateWithCapacity(ref array, count, capacity, capacityIncrement);
        }

        public static void DuplicateWithCapacity<TValue>(ref TValue[] array, int count, int capacity, int capacityIncrement = 10)
        {
            if (array == null)
            {
                array = new TValue[Math.Max(capacity, capacityIncrement)];
                return;
            }

            var newSize = count + Math.Max(capacity, capacityIncrement);
            var newArray = new TValue[newSize];
            Array.Copy(array, newArray, count);
            array = newArray;
        }

        public static bool Contains<TValue>(this TValue[] array, TValue value)
        {
            if (array == null)
                return false;

            var comparer = EqualityComparer<TValue>.Default;
            for (var i = 0; i < array.Length; ++i)
                if (comparer.Equals(array[i], value))
                    return true;

            return false;
        }

        public static bool ContainsReference<TValue>(this TValue[] array, TValue value)
            where TValue : class
        {
            if (array == null)
                return false;

            return ContainsReference(array, array.Length, value);
        }

        public static bool ContainsReference<TFirst, TSecond>(this TFirst[] array, int count, TSecond value)
            where TSecond : class
            where TFirst : TSecond
        {
            return IndexOfReference(array, value, count) != -1;
        }

        public static bool ContainsReference<TFirst, TSecond>(this TFirst[] array, int startIndex, int count, TSecond value)
            where TSecond : class
            where TFirst : TSecond
        {
            return IndexOfReference(array, value, startIndex, count) != -1;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "index", Justification = "Keep this for future implementation")]
        public static bool HaveDuplicateReferences<TFirst>(this TFirst[] first, int index, int count)
        {
            for (var i = 0; i < count; ++i)
            {
                var element = first[i];
                for (var n = i + 1; n < count - i; ++n)
                {
                    if (ReferenceEquals(element, first[n]))
                        return true;
                }
            }
            return false;
        }

        public static bool HaveEqualElements<TValue>(this TValue[] first, TValue[] second, int count = int.MaxValue)
        {
            if (first == null || second == null)
                return second == first;

            var lengthFirst = Math.Min(count, first.Length);
            var lengthSecond = Math.Min(count, second.Length);

            if (lengthFirst != lengthSecond)
                return false;

            var comparer = EqualityComparer<TValue>.Default;
            for (var i = 0; i < lengthFirst; ++i)
                if (!comparer.Equals(first[i], second[i]))
                    return false;

            return true;
        }

        ////REVIEW: remove this to get rid of default equality comparer?
        public static int IndexOf<TValue>(this TValue[] array, TValue value, int startIndex = 0, int count = -1)
        {
            if (array == null)
                return -1;

            if (count < 0)
                count = array.Length - startIndex;
            var comparer = EqualityComparer<TValue>.Default;
            for (var i = startIndex; i < startIndex + count; ++i)
                if (comparer.Equals(array[i], value))
                    return i;

            return -1;
        }

        public static int IndexOf<TValue>(this TValue[] array, Predicate<TValue> predicate)
        {
            if (array == null)
                return -1;

            var length = array.Length;
            for (var i = 0; i < length; ++i)
                if (predicate(array[i]))
                    return i;

            return -1;
        }

        public static int IndexOf<TValue>(this TValue[] array, Predicate<TValue> predicate, int startIndex, int count = -1)
        {
            if (array == null)
                return -1;

            var end = startIndex + (count < 0 ? array.Length - startIndex : count);
            for (var i = startIndex; i < end; ++i)
            {
                if (predicate(array[i]))
                    return i;
            }

            return -1;
        }

        public static int IndexOfReference<TFirst, TSecond>(this TFirst[] array, TSecond value, int count = -1)
            where TSecond : class
            where TFirst : TSecond
        {
            return IndexOfReference(array, value, 0, count);
        }

        public static int IndexOfReference<TFirst, TSecond>(this TFirst[] array, TSecond value, int startIndex, int count)
            where TSecond : class
            where TFirst : TSecond
        {
            if (array == null)
                return -1;

            if (count < 0)
                count = array.Length - startIndex;
            for (var i = startIndex; i < startIndex + count; ++i)
                if (ReferenceEquals(array[i], value))
                    return i;

            return -1;
        }

        public static int IndexOfValue<TValue>(this TValue[] array, TValue value, int startIndex = 0, int count = -1)
            where TValue : struct, IEquatable<TValue>
        {
            if (array == null)
                return -1;

            if (count < 0)
                count = array.Length - startIndex;
            for (var i = startIndex; i < startIndex + count; ++i)
                if (value.Equals(array[i]))
                    return i;

            return -1;
        }

        public static int Append<TValue>(ref TValue[] array, TValue value)
        {
            if (array == null)
            {
                array = new TValue[1];
                array[0] = value;
                return 0;
            }

            var length = array.Length;
            Array.Resize(ref array, length + 1);
            array[length] = value;
            return length;
        }

        public static int Append<TValue>(ref TValue[] array, IEnumerable<TValue> values)
        {
            using var _0 = UnityEngine.Pool.ListPool<TValue>.Get(out var list);
            list.AddRange(values);

            if (array == null)
            {
                array = list.ToArray();
                return 0;
            }

            var oldLength = array.Length;
            var valueCount = list.Count;

            Array.Resize(ref array, oldLength + valueCount);

            var index = oldLength;
            foreach (var value in list)
                array[index++] = value;

            return oldLength;
        }

        // Append to an array that is considered immutable. This allows using 'values' as is
        // if 'array' is null.
        // Returns the index of the first newly added element in the resulting array.
        public static int AppendToImmutable<TValue>(ref TValue[] array, TValue[] values)
        {
            if (array == null)
            {
                array = values;
                return 0;
            }

            if (values != null && values.Length > 0)
            {
                var oldCount = array.Length;
                var valueCount = values.Length;
                Array.Resize(ref array, oldCount + valueCount);
                Array.Copy(values, 0, array, oldCount, valueCount);
                return oldCount;
            }

            return array.Length;
        }

        public static int AppendWithCapacity<TValue>(ref TValue[] array, ref int count, TValue value, int capacityIncrement = 10)
        {
            if (array == null)
            {
                array = new TValue[capacityIncrement];
                array[0] = value;
                ++count;
                return 0;
            }

            var capacity = array.Length;
            if (capacity == count)
            {
                capacity += capacityIncrement;
                Array.Resize(ref array, capacity);
            }

            var index = count;
            array[index] = value;
            ++count;

            return index;
        }

        public static int AppendListWithCapacity<TValue, TValues>(ref TValue[] array, ref int length, TValues values, int capacityIncrement = 10)
            where TValues : IReadOnlyList<TValue>
        {
            var numToAdd = values.Count;
            if (array == null)
            {
                var size = Math.Max(numToAdd, capacityIncrement);
                array = new TValue[size];
                for (var i = 0; i < numToAdd; ++i)
                    array[i] = values[i];
                length += numToAdd;
                return 0;
            }

            var capacity = array.Length;
            if (capacity < length + numToAdd)
            {
                capacity += Math.Max(length + numToAdd, capacityIncrement);
                Array.Resize(ref array, capacity);
            }

            var index = length;
            for (var i = 0; i < numToAdd; ++i)
                array[index + i] = values[i];
            length += numToAdd;

            return index;
        }

        public static void InsertAt<TValue>(ref TValue[] array, int index, TValue value)
        {
            if (array == null)
            {
                ////REVIEW: allow growing array to specific size by inserting at arbitrary index?
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));

                array = new TValue[1];
                array[0] = value;
                return;
            }

            // Reallocate.
            var oldLength = array.Length;
            Array.Resize(ref array, oldLength + 1);

            // Make room for element.
            if (index != oldLength)
                Array.Copy(array, index, array, index + 1, oldLength - index);

            array[index] = value;
        }

        public static void InsertAtWithCapacity<TValue>(ref TValue[] array, ref int count, int index, TValue value, int capacityIncrement = 10)
        {
            EnsureCapacity(ref array, count, count + 1, capacityIncrement);

            if (index != count)
                Array.Copy(array, index, array, index + 1, count - index);

            array[index] = value;
            ++count;
        }

        public static void PutAtIfNotSet<TValue>(ref TValue[] array, int index, Func<TValue> valueFn)
        {
            if (array.LengthSafe() < index + 1)
                Array.Resize(ref array, index + 1);

            if (EqualityComparer<TValue>.Default.Equals(array[index], default(TValue)))
                array[index] = valueFn();
        }

        // Adds 'count' entries to the array. Returns first index of newly added entries.
        public static int GrowBy<TValue>(ref TValue[] array, int count)
        {
            if (array == null)
            {
                array = new TValue[count];
                return 0;
            }

            var oldLength = array.Length;
            Array.Resize(ref array, oldLength + count);
            return oldLength;
        }

        public static int GrowWithCapacity<TValue>(ref TValue[] array, ref int count, int growBy, int capacityIncrement = 10)
        {
            var length = array != null ? array.Length : 0;
            if (length < count + growBy)
            {
                if (capacityIncrement < growBy)
                    capacityIncrement = growBy;
                GrowBy(ref array, capacityIncrement);
            }

            var offset = count;
            count += growBy;
            return offset;
        }

        public static TValue[] Join<TValue>(this TValue value, params TValue[] values)
        {
            // Determine length.
            var length = 0;
            if (value != null)
                ++length;
            if (values != null)
                length += values.Length;

            if (length == 0)
                return null;

            var array = new TValue[length];

            // Populate.
            var index = 0;
            if (value != null)
                array[index++] = value;

            if (values != null)
                Array.Copy(values, 0, array, index, values.Length);

            return array;
        }

        public static TValue[] Merge<TValue>(this TValue[] first, TValue[] second)
            where TValue : IEquatable<TValue>
        {
            if (first == null)
                return second;
            if (second == null)
                return first;

            var merged = new List<TValue>();
            merged.AddRange(first);

            for (var i = 0; i < second.Length; ++i)
            {
                var secondValue = second[i];
                if (!merged.Exists(x => x.Equals(secondValue)))
                {
                    merged.Add(secondValue);
                }
            }

            return merged.ToArray();
        }

        public static TValue[] Merge<TValue>(this TValue[] first, TValue[] second, IEqualityComparer<TValue> comparer)
        {
            if (first == null)
                return second;
            if (second == null)
                return null;

            var merged = new List<TValue>();
            merged.AddRange(first);

            for (var i = 0; i < second.Length; ++i)
            {
                var secondValue = second[i];
                if (!merged.Exists(_ => comparer.Equals(secondValue)))
                {
                    merged.Add(secondValue);
                }
            }

            return merged.ToArray();
        }

        public static void EraseAt<TValue>(ref TValue[] array, int index)
        {
            Assert.IsNotNull(array);
            Assert.IsTrue(index >= 0 && index < array.Length);

            var length = array.Length;
            if (index == 0 && length == 1)
            {
                array = null;
                return;
            }

            if (index < length - 1)
                Array.Copy(array, index + 1, array, index, length - index - 1);

            Array.Resize(ref array, length - 1);
        }

        public static void EraseAtWithCapacity<TValue>(this TValue[] array, ref int count, int index)
        {
            Assert.IsNotNull(array);
            Assert.IsTrue(count <= array.Length);
            Assert.IsTrue(index >= 0 && index < count);

            // If we're erasing from the beginning or somewhere in the middle, move
            // the array contents down from after the index.
            if (index < count - 1)
            {
                Array.Copy(array, index + 1, array, index, count - index - 1);
            }

            array[count - 1] = default; // Tail has been moved down by one.
            --count;
        }

        public static bool Erase<TValue>(ref TValue[] array, TValue value)
        {
            var index = IndexOf(array, value);
            if (index != -1)
            {
                EraseAt(ref array, index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Erase an element from the array by moving the tail element into its place.
        /// </summary>
        /// <param name="array">Array to modify. May be not <c>null</c>.</param>
        /// <param name="count">Current number of elements inside of array. May be less than <c>array.Length</c>.</param>
        /// <param name="index">Index of element to remove. Tail element will get moved into its place.</param>
        /// <typeparam name="TValue"></typeparam>
        /// <remarks>
        /// This method does not re-allocate the array. Instead <paramref name="count"/> is used
        /// to keep track of how many elements there actually are in the array.
        /// </remarks>
        public static void EraseAtByMovingTail<TValue>(this TValue[] array, ref int count, int index)
        {
            Assert.IsNotNull(array);
            Assert.IsTrue(index >= 0 && index < array.Length);
            Assert.IsTrue(count >= 0 && count <= array.Length);
            Assert.IsTrue(index < count);

            // Move tail, if necessary.
            if (index != count - 1)
                array[index] = array[count - 1];

            // Destroy current tail.
            if (count >= 1)
                array[count - 1] = default;
            --count;
        }

        public static TValue[] Copy<TValue>(this TValue[] array)
        {
            if (array == null)
                return null;

            var length = array.Length;
            var result = new TValue[length];
            Array.Copy(array, result, length);
            return result;
        }

        public static TValue[] Clone<TValue>(this TValue[] array)
            where TValue : ICloneable
        {
            if (array == null)
                return null;

            var count = array.Length;
            var result = new TValue[count];

            for (var i = 0; i < count; ++i)
                result[i] = (TValue)array[i].Clone();

            return result;
        }

        public static TNew[] Select<TOld, TNew>(this TOld[] array, Func<TOld, TNew> converter)
        {
            if (array == null)
                return null;

            var length = array.Length;
            var result = new TNew[length];

            for (var i = 0; i < length; ++i)
                result[i] = converter(array[i]);

            return result;
        }

        public static void Swap<TValue>(ref TValue first, ref TValue second)
        {
            var temp = first;
            first = second;
            second = temp;
        }

        /// <summary>
        /// Move a slice in the array to a different place without allocating a temporary array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="sourceIndex"></param>
        /// <param name="destinationIndex"></param>
        /// <param name="count"></param>
        /// <typeparam name="TValue"></typeparam>
        /// <remarks>
        /// The slice is moved by repeatedly swapping slices until all the slices are where they
        /// are supposed to go. This is not super efficient but avoids having to allocate a temporary
        /// array on the heap.
        /// </remarks>
        public static void MoveSlice<TValue>(this TValue[] array, int sourceIndex, int destinationIndex, int count)
        {
            if (count <= 0 || sourceIndex == destinationIndex)
                return;

            // Determine the number of elements in the window.
            int elementCount;
            if (destinationIndex > sourceIndex)
                elementCount = destinationIndex + count - sourceIndex;
            else
                elementCount = sourceIndex + count - destinationIndex;

            // If the source and target slice are right next to each other, just go
            // and swap out the elements in both slices.
            if (elementCount == count * 2)
            {
                for (var i = 0; i < count; ++i)
                    Swap(ref array[sourceIndex + i], ref array[destinationIndex + i]);
            }
            else
            {
                // There's elements in-between the two slices.
                //
                // The easiest way to picture this operation is as a rotation of the elements within
                // the window given by sourceIndex, destination, and count. Within that window, we are
                // simply treating it as a wrap-around buffer and then sliding the elements clockwise
                // or counter-clockwise (depending on whether we move up or down, respectively) through
                // the window.
                //
                // Unfortunately, we can't just memcopy the slices within that window as we have to
                // have a temporary copy in place in order to preserve element values. So instead, we
                // go and swap elements one by one, something that doesn't require anything other than
                // a single value temporary copy.

                // Determine the number of swaps we need to achieve the desired order. Swaps
                // operate in pairs so it's one less than the number of elements in the range.
                var swapCount = elementCount - 1;

                // We simply take sourceIndex as fixed and do all swaps from there until all
                // the elements in the window are in the right order. Each swap will put one
                // element in its final place.
                var dst = destinationIndex;
                for (var i = 0; i < swapCount; ++i)
                {
                    // Swap source into its destination place. This puts the current sourceIndex
                    // element in its final place.
                    Swap(ref array[dst], ref array[sourceIndex]);

                    // Find out where the element that we now swapped into sourceIndex should
                    // actually go.
                    if (destinationIndex > sourceIndex)
                    {
                        // Rotating clockwise.
                        dst -= count;
                        if (dst < sourceIndex)
                            dst = destinationIndex + count - Math.Abs(sourceIndex - dst); // Wrap around.
                    }
                    else
                    {
                        // Rotating counter-clockwise.
                        dst += count;
                        if (dst >= sourceIndex + count)
                            dst = destinationIndex + (dst - (sourceIndex + count)); // Wrap around.
                    }
                }
            }
        }

        public static void EraseSliceWithCapacity<TValue>(ref TValue[] array, ref int length, int index, int count)
        {
            // Move elements down.
            if (count < length)
                Array.Copy(array, index + count, array, index, length - index - count);

            // Erase now vacant slots.
            for (var i = 0; i < count; ++i)
                array[length - i - 1] = default;

            length -= count;
        }

        public static void SwapElements<TValue>(this TValue[] array, int index1, int index2)
        {
            Swap(ref array[index1], ref array[index2]);
        }

        public static void SwapElements<TValue>(ref this NativeArray<TValue> array, int index1, int index2)
            where TValue : struct
        {
            var temp = array[index1];
            array[index1] = array[index2];
            array[index2] = temp;
        }
        #endregion // UnityEngine.InputSystem.Utilities
    }
}
