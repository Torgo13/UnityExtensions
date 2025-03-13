using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace UnityExtensions.Unsafe
{
    public static class NativeListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRange<T>(ref this NativeList<T> list, T[] array) where T : unmanaged
        {
            fixed (T* p = array)
            {
                list.AddRange(p, array.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void AddRange<T>(ref this NativeList<T> list, T[] array, int count) where T : unmanaged
        {
            fixed (T* p = array)
            {
                list.AddRange(p, count);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(ref this NativeList<T> list, List<T> managedList) where T : unmanaged
        {
            list.AddRange(NoAllocHelpers.ExtractArrayFromList(managedList), managedList.Count);
        }
    }
}
