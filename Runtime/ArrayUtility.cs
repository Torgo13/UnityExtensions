using System;
using System.Runtime.CompilerServices;

namespace UnityExtensions
{
    public static class ArrayUtility
    {
        //https://github.com/needle-mirror/com.unity.entities/blob/1.3.9/Unity.Entities.UI.Editor/Utility/Internal/ArrayUtility.cs
        #region Unity.Entities.UI
        public static T[] RemoveAt<T>(T[] source, int index)
        {
            if (index < 0)
                throw new ArgumentException(nameof(ArrayUtility) + ": index must be in [0, Length -1] range.");

            var dest = new T[source.Length - 1];
            if (index > 0)
                Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }

        public static T[] InsertAt<T>(T[] source, int index, T value)
        {
            if (index < 0 || index > source.Length)
                throw new ArgumentException(nameof(ArrayUtility) + ": index must be in [0, Length] range.");

            var dest = new T[source.Length + 1];
            if (index == 0)
            {
                dest[0] = value;
                Copy(source, 0, dest, 1, source.Length);
                return dest;
            }

            if (index == source.Length)
            {
                dest[source.Length] = value;
                Copy(source, 0, dest, 0, source.Length);
                return dest;
            }

            dest[index] = value;
            Copy(source, 0, dest, 0, index);
            Copy(source, index, dest, index + 1, source.Length - index);
            return dest;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Copy<T>(
            T[] sourceArray,
            int sourceIndex,
            T[] destinationArray,
            int destinationIndex,
            int count)
        {
            for (var index = 0; index < count && destinationIndex < destinationArray.Length; index++, destinationIndex++, sourceIndex++)
            {
                var sourceValue = sourceArray[sourceIndex];
                destinationArray[destinationIndex] = sourceValue;
            }
        }
        #endregion // Unity.Entities.UI
        
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/InternalPackages/com.unity.video-streaming.client/Runtime/Utils/ArrayUtils.cs
        #region Unity.LiveCapture.VideoStreaming.Client.Utils
        public static bool IsBytesEquals(byte[] bytes1, int offset1, int count1, byte[] bytes2, int offset2, int count2)
        {
            if (count1 != count2)
                return false;

            for (int i = 0; i < count1; i++)
                if (bytes1[offset1 + i] != bytes2[offset2 + i])
                    return false;

            return true;
        }

        public static bool StartsWith(byte[] array, int offset, int count, byte[] pattern)
        {
            int patternLength = pattern.Length;

            if (count < patternLength)
                return false;

            for (int i = 0; i < patternLength; i++, offset++)
                if (array[offset] != pattern[i])
                    return false;

            return true;
        }

        public static bool EndsWith(byte[] array, int offset, int count, byte[] pattern)
        {
            int patternLength = pattern.Length;

            if (count < patternLength)
                return false;

            offset = offset + count - patternLength;

            for (int i = 0; i < patternLength; i++, offset++)
                if (array[offset] != pattern[i])
                    return false;

            return true;
        }

        public static int IndexOfBytes(byte[] array, byte[] pattern, int startIndex, int count)
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
    }
}