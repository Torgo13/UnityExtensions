using System;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;

namespace PKGE.Unsafe
{
    public static partial class ArrayExtensions
    {
        /// <exception cref="IndexOutOfRangeException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(this T[] array, int index) where T : struct
        {
            Assert.IsNotNull(array);

            return ref array[index];
        }

        //https://github.com/Unity-Technologies/UnityCsReference/blob/b1cf2a8251cce56190f455419eaa5513d5c8f609/Runtime/Export/Unsafe/UnsafeUtility.cs
        #region Unity.Collections.LowLevel.Unsafe
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> GetByteSpan<T>(this Span<T> array) where T : struct
        {
            if (array == null || array.Length == 0)
                return new Span<byte>();

            return System.Runtime.InteropServices.MemoryMarshal.Cast<T, byte>(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> GetByteSpanFromArray<T>(this T[] array, int elementSize = default) where T : struct
        {
            Assert.AreEqual(SizeOfCache<T>.Size, elementSize);

            return GetByteSpan(array.AsSpan());
        }
        #endregion // Unity.Collections.LowLevel.Unsafe

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this Array array)
        {
            return array == null || array.Length == 0;
        }
    }
}
