using System.Collections.Generic;
using Unity.Collections;

namespace PKGE
{
    /// <summary>
    /// Utility methods for working with <see cref="NativeArray{T}"/> objects.
    /// </summary>
    public static class NativeArrayUtils
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/NativeArrayUtils.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Ensure that this array is large enough to contain the given capacity.
        /// </summary>
        /// <remarks>
        /// If the array does not have sufficient capacity, it is disposed and a new, empty array is created.
        /// </remarks>
        /// <typeparam name="T">The type of array element.</typeparam>
        /// <param name="array">The array reference. Overwritten if the original array has insufficient capacity.</param>
        /// <param name="capacity">The minimum number of elements that the array must be able to contain.</param>
        /// <param name="allocator">The allocator to use when creating a new array, if needed.</param>
        /// <param name="options">The options to use when creating the new array, if needed.</param>
        public static void EnsureCapacity<T>(ref NativeArray<T> array, int capacity, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory) where T : struct
        {
            if (array.Length < capacity)
            {
                if (array.IsCreated)
                {
                    array.Dispose();
                }

                array = new NativeArray<T>(capacity, allocator, options);
            }
        }
        #endregion // Unity.XR.CoreUtils
        
        //https://github.com/needle-mirror/com.unity.xr.arfoundation/blob/8ced5e7002ad2e622a7968f0007ab0bf7298c137/Runtime/ARSubsystems/NativeCopyUtility.cs
        #region UnityEngine.XR.ARSubsystems
        /// <summary>
        /// Copies the contents of <paramref name="source"/> into the <c>NativeArray</c> <paramref name="destination"/>.
        /// The lengths of both collections must match.
        /// </summary>
        /// <typeparam name="T">The type of the <c>NativeArray</c> structs that will be copied</typeparam>
        /// <param name="source">The <c>IReadOnlyList</c> that provides the data</param>
        /// <param name="destination">The <c>NativeArray</c> that will be written to</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when there is a mismatch between
        /// <paramref name="source"/> and <paramref name="destination"/> sizes.</exception>
        public static void CopyFromReadOnlyList<T>(IReadOnlyList<T> source, NativeArray<T> destination)
            where T : struct
        {
            if (source.Count != destination.Length)
            {
                ThrowHelper.CopyFromReadOnlyList(source, destination);
                return;
            }

            for (var i = 0; i < source.Count; i++)
            {
                destination[i] = source[i];
            }
        }

        /// <summary>
        /// Copies the contents of <paramref name="source"/> into the <c>NativeArray</c> <paramref name="destination"/>.
        /// The lengths of both collections must match.
        /// </summary>
        /// <typeparam name="T">The type of the <c>NativeArray</c> structs that will be copied</typeparam>
        /// <param name="source">The <c>IReadOnlyCollection</c> that provides the data</param>
        /// <param name="destination">The <c>NativeArray</c> that will be written to</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when there is a mismatch between
        /// <paramref name="source"/> and <paramref name="destination"/> sizes.</exception>
        /// <remarks> Prefer IReadOnlyList over IReadOnlyCollection for copy performance where possible.</remarks>
        /// <seealso cref="CopyFromReadOnlyList{T}"/>
        public static void CopyFromReadOnlyCollection<T>(IReadOnlyCollection<T> source, NativeArray<T> destination)
            where T : struct
        {
            if (source.Count != destination.Length)
            {
                ThrowHelper.CopyFromReadOnlyCollection(source, destination);
                return;
            }

            var index = 0;
            foreach (var item in source)
            {
                destination[index] = item;
                index++;
            }
        }
        #endregion // UnityEngine.XR.ARSubsystems

        private static class ThrowHelper
        {
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
            public static void CopyFromReadOnlyList<T>(IReadOnlyList<T> source, NativeArray<T> destination)
                where T : struct
            {
                throw new System.ArgumentOutOfRangeException(nameof(destination),
                    $"{nameof(source)} count {source.Count} doesn't match {nameof(destination)} length {destination.Length}!");
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
            public static void CopyFromReadOnlyCollection<T>(IReadOnlyCollection<T> source, NativeArray<T> destination)
                where T : struct
            {
                throw new System.ArgumentOutOfRangeException(nameof(destination),
                    $"{nameof(source)} count {source.Count} doesn't match {nameof(destination)} length {destination.Length}!");
            }
        }
    }
}