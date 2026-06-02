#if INCLUDE_COLLECTIONS
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using Unity.Collections.LowLevel.Unsafe;

namespace PKGE
{
    public static class UnsafeListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCapacity<T>(ref this UnsafeList<T> unsafeList, int capacity) where T : unmanaged
        {
            Assert.IsTrue(unsafeList.IsCreated);
            Assert.IsTrue(capacity > 0);

            if (unsafeList.Capacity < capacity)
                unsafeList.Capacity = capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureRoom<T>(ref this UnsafeList<T> unsafeList, int room) where T : unmanaged
        {
            Assert.IsTrue(unsafeList.IsCreated);
            Assert.IsTrue(room > 0);

            var capacity = unsafeList.Length + room;
            if (unsafeList.Capacity < capacity)
                unsafeList.Capacity = capacity;
        }

        //https://github.com/Unity-Technologies/Graphics/blob/2ecb711df890ca21a0817cf610ec21c500cb4bfe/Packages/com.unity.render-pipelines.universal/Runtime/UniversalRenderPipelineCore.cs
        #region UnityEngine.Rendering.Universal
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T UnsafeElementAt<T>(ref this UnsafeList<T> unsafeList, int index) where T : unmanaged
        {
            return ref unsafeList.UnsafeElementAtMutable(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T UnsafeElementAtMutable<T>(ref this UnsafeList<T> unsafeList, int index) where T : unmanaged
        {
            Assert.IsTrue(unsafeList.IsCreated);
            Assert.IsTrue(index < unsafeList.Capacity);

            if (index >= unsafeList.Length)
                unsafeList.Length = 1 + index;

            return ref unsafeList.ElementAt(index);
        }
        #endregion // UnityEngine.Rendering.Universal
    }
}
#endif // INCLUDE_COLLECTIONS
