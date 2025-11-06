using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine.Assertions;
using PKGE.Packages;

namespace PKGE.Unsafe
{
    public static class ListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(this List<T> list, int index) where T : struct
        {
            Assert.IsNotNull(list);
            Assert.IsTrue(index >= 0);
            Assert.IsTrue(index < list.Count);

            return ref NoAllocHelpers.ExtractArrayFromList(list)[index];
        }

#if INCLUDE_COLLECTIONS
        //https://github.com/needle-mirror/com.unity.collections/blob/feee1d82af454e1023e3e04789fce4d30fc1d938/Unity.Collections/ListExtensions.cs
        #region Unity.Collections
        /// <summary>
        /// Returns a <see cref="Unity.Collections.NativeList{T}"/> that is a copy of this <see cref="System.Collections.Generic.List{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to copy.</param>
        /// <param name="allocator">The allocator to use.</param>
        /// <returns>A copy of this list.</returns>
        public static NativeList<T> ToNativeList<T>(this List<T> list,
            AllocatorManager.AllocatorHandle allocator) where T : unmanaged
        {
            Assert.IsNotNull(list);

            var container = new NativeList<T>(list.Count, allocator);
            container.ResizeUninitialized(container.Capacity);
            list.AsSpan().CopyTo(container.AsSpan());

            return container;
        }

        /// <summary>
        /// Returns a <see cref="Unity.Collections.NativeArray{T}"/> that is a copy of this <see cref="System.Collections.Generic.List{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to copy.</param>
        /// <param name="allocator">The allocator to use.</param>
        /// <returns>An array that is a copy of this list.</returns>
        public static NativeArray<T> ToNativeArray<T>(this List<T> list,
            AllocatorManager.AllocatorHandle allocator) where T : unmanaged
        {
            Assert.IsNotNull(list);

            var container = CollectionHelper.CreateNativeArray<T>(list.Count, allocator, NativeArrayOptions.UninitializedMemory);
            list.AsSpan().CopyTo(container.AsSpan());

            return container;
        }
        #endregion // Unity.Collections

        public static void ResetListContents<T>(this List<T> list, NativeList<T> nativeList)
            where T : unmanaged
        {
            NoAllocHelpers.ResetListContents(list, nativeList.AsReadOnlySpan());
        }
#endif // INCLUDE_COLLECTIONS
    }
}