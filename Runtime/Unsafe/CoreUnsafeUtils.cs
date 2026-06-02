#if PKGE_USING_UNSAFE
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Collections;

#if INCLUDE_MATHEMATICS
using Unity.Mathematics;
using static Unity.Mathematics.math;
#else
using PKGE.Mathematics;
using static PKGE.Mathematics.math;
#endif // INCLUDE_MATHEMATICS

namespace PKGE.Unsafe
{
    /// <summary>
    /// Static class with unsafe utility functions.
    /// </summary>
    public static partial class CoreUnsafeUtils
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Common/CoreUnsafeUtils.cs#L258
        #region UnityEngine.Rendering
        /// <summary>
        /// Fixed Buffer String Queue class.
        /// </summary>
        public unsafe struct FixedBufferStringQueue
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
        #endregion // UnityEngine.Rendering
    }
}
#endif // PKGE_USING_UNSAFE
