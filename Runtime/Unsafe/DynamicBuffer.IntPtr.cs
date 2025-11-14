#if PKGE_USING_INTPTR
using System;
using System.Runtime.CompilerServices;

namespace PKGE.Unsafe
{
    public static class DynamicBufferExtensions
    {
        /// <inheritdoc cref="GetUnsafePtr"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe IntPtr GetIntPtr<T>(this DynamicBuffer<T> buffer) where T : unmanaged
        {
            return (IntPtr)buffer.GetUnsafePtr();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe IntPtr GetReadOnlyIntPtr<T>(this DynamicBuffer<T> buffer) where T : unmanaged
        {
            return (IntPtr)buffer.GetUnsafeReadOnlyPtr();
        }
    }
}
#endif // PKGE_USING_INTPTR
