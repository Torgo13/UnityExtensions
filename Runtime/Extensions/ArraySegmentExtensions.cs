using System;
using UnityEngine.Assertions;

namespace PKGE
{
    public static class ArraySegmentExtensions
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/InternalPackages/com.unity.video-streaming.client/Runtime/Utils/ArraySegmentExtensions.cs
        #region Unity.LiveCapture.VideoStreaming.Client.Utils
        public static ArraySegment<T> SubSegment<T>(this ArraySegment<T> arraySegment, int offset)
        {
            Assert.IsNotNull(arraySegment.Array, $"{nameof(arraySegment.Array)} is null.");
            return new ArraySegment<T>(arraySegment.Array, arraySegment.Offset + offset, arraySegment.Count - offset);
        }
        #endregion // Unity.LiveCapture.VideoStreaming.Client.Utils
    }
}