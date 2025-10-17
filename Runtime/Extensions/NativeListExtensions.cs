#if INCLUDE_COLLECTIONS
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using Unity.Collections;

namespace PKGE.Packages
{
    public static class NativeListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCapacity<T>(this NativeList<T> nativeList, int capacity) where T : unmanaged
        {
            Assert.IsTrue(nativeList.IsCreated);
            Assert.IsTrue(capacity > 0);

            if (nativeList.Capacity < capacity)
                nativeList.Capacity = capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureRoom<T>(this NativeList<T> nativeList, int room) where T : unmanaged
        {
            Assert.IsTrue(nativeList.IsCreated);
            Assert.IsTrue(room > 0);

            var capacity = nativeList.Length + room;
            if (nativeList.Capacity < capacity)
                nativeList.Capacity = capacity;
        }
    }
}
#endif // INCLUDE_COLLECTIONS
