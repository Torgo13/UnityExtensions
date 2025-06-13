using System;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe
{
    public static class ArrayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T UnsafeElementAtMutable<T>(this T[] array, int index) where T : struct
        {
            Assert.IsNotNull(array);
            Assert.IsTrue(index >= 0);
            Assert.IsTrue(index < array.Length);

            return ref array[index];
        }

        //https://github.com/Unity-Technologies/UnityCsReference/blob/b1cf2a8251cce56190f455419eaa5513d5c8f609/Runtime/Export/Unsafe/UnsafeUtility.cs
        #region Unity.Collections.LowLevel.Unsafe
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> GetByteSpanFromArray(this Array array, int elementSize)
        {
            if (array.IsNullOrEmpty())
                return new Span<byte>();

            Assert.AreEqual(UnsafeUtility.SizeOf(array.GetType().GetElementType()), elementSize);

            var bArray = UnsafeUtility.As<Array, byte[]>(ref array);
            return new Span<byte>(UnsafeUtility.AddressOf(ref bArray[0]), array.Length * elementSize);
        }
        #endregion // Unity.Collections.LowLevel.Unsafe

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this Array array)
        {
            return array == null || array.Length == 0;
        }
    }
}
