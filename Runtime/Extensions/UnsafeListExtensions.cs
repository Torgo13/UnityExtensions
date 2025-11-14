#if INCLUDE_COLLECTIONS
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using Unity.Collections.LowLevel.Unsafe;

namespace PKGE.Packages
{
    public static class UnsafeListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCapacity<T>(ref this UnsafeList<T> UnsafeList, int capacity) where T : unmanaged
        {
            Assert.IsTrue(UnsafeList.IsCreated);
            Assert.IsTrue(capacity > 0);

            if (UnsafeList.Capacity < capacity)
                UnsafeList.Capacity = capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureRoom<T>(ref this UnsafeList<T> UnsafeList, int room) where T : unmanaged
        {
            Assert.IsTrue(UnsafeList.IsCreated);
            Assert.IsTrue(room > 0);

            var capacity = UnsafeList.Length + room;
            if (UnsafeList.Capacity < capacity)
                UnsafeList.Capacity = capacity;
        }
    }
}
#endif // INCLUDE_COLLECTIONS
