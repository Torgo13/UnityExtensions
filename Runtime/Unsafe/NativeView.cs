using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe
{
    //https://github.com/needle-mirror/com.unity.xr.arcore/blob/master/Runtime/NativeView.cs
    #region UnityEngine.XR.ARCore
    /// <summary>
    /// Similar to NativeSlice but blittable. Provides a "view"
    /// into a contiguous array of memory. Used to interop with C.
    /// </summary>
    public unsafe struct NativeView
    {
        public void* Ptr;
        public int Count;
    }

    public static class NativeViewExtensions
    {
        public static unsafe NativeView AsNativeView<T>(this NativeArray<T> array) where T : struct => new NativeView
        {
            Ptr = array.GetUnsafePtr(),
            Count = array.Length
        };

        public static unsafe NativeView AsNativeView<T>(this NativeSlice<T> slice) where T : struct => new NativeView
        {
            Ptr = slice.GetUnsafePtr(),
            Count = slice.Length
        };
    }
    #endregion // UnityEngine.XR.ARCore
}
