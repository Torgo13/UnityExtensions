using System;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe
{
    public static class StreamExtensions
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/main/Packages/com.unity.live-capture/Networking/Utilities/Extensions/StreamExtensions.cs
        #region Unity.LiveCapture.Networking
        /// <summary>
        /// Copies a native array into a stream.
        /// </summary>
        /// <param name="stream">The stream to write the array into.</param>
        /// <param name="array">The array to write.</param>
        /// <typeparam name="T">The type of data in the native array.</typeparam>
        /// <returns><see langword="true"/> if the array was successfully written into the stream; otherwise, <see langword="false"/>.</returns>
        public static bool WriteArray<T>(this MemoryStream stream, NativeArray<T> array) where T : struct
        {
            stream.SetLength(stream.Length + array.Length);

            if (!stream.TryGetBuffer(out var buffer) || buffer.Array == null)
            {
                return false;
            }

            unsafe
            {
                fixed (void* streamPtr = &buffer.Array[buffer.Offset + stream.Position])
                {
                    UnsafeUtility.MemCpy(streamPtr, array.GetUnsafePtr(), array.Length);
                }
            }

            stream.Position += array.Length;
            return true;
        }
        #endregion // Unity.LiveCapture.Networking

        //https://github.com/needle-mirror/com.unity.cloud.gltfast/blob/master/Runtime/Scripts/Export/StreamExtension.cs
        #region GLTFast.Export
        public static unsafe void Write(this Stream stream, NativeArray<byte> array)
        {
            var span = new ReadOnlySpan<byte>(array.GetUnsafeReadOnlyPtr(), array.Length);
            stream.Write(span);
        }
        #endregion // GLTFast.Export
    }
}