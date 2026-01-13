#if INCLUDE_COLLECTIONS
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using Unity.Collections.LowLevel.Unsafe;

namespace PKGE.Packages
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
    }
}
#endif // INCLUDE_COLLECTIONS
