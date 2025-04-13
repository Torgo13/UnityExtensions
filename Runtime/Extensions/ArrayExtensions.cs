using System;
using Unity.Collections;
using UnityEngine.Jobs;

namespace UnityExtensions
{
    public static class ArrayExtensions
    {
        public static bool Contains<T>(this T[] array, T item)
        {
            if (array == null)
                return false;

            foreach (var element in array)
            {
                if (element.Equals(item))
                    return true;
            }

            return false;
        }

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
    }
}
