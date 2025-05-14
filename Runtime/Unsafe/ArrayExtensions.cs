using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine.Assertions;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe
{
    public static class ArrayExtensions
    {
        //https://github.com/Unity-Technologies/UnityCsReference/blob/b1cf2a8251cce56190f455419eaa5513d5c8f609/Runtime/Export/Unsafe/UnsafeUtility.cs
        #region Unity.Collections.LowLevel.Unsafe
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> GetByteSpanFromArray(this Array array, int elementSize)
        {
            if (array == null || array.Length == 0)
                return new Span<byte>();

            Assert.AreEqual(UnsafeUtility.SizeOf(array.GetType().GetElementType()), elementSize);

            var bArray = UnsafeUtility.As<Array, byte[]>(ref array);
            return new Span<byte>(UnsafeUtility.AddressOf(ref bArray[0]), array.Length * elementSize);
        }
        #endregion // Unity.Collections.LowLevel.Unsafe

        //https://github.com/needle-mirror/com.unity.physics/blob/master/Unity.Physics/Base/Containers/UnsafeEx.cs
        #region Unity.Physics
        public static unsafe int CalculateOffset<T, U>(ref T value, ref U baseValue)
            where T : struct
            where U : struct
        {
            return (int)((byte*)UnsafeUtility.AddressOf(ref value)
                         - (byte*)UnsafeUtility.AddressOf(ref baseValue));
        }

        public static unsafe int CalculateOffset<T>(void* value, ref T baseValue)
            where T : struct
        {
            return (int)((byte*)value - (byte*)UnsafeUtility.AddressOf(ref baseValue));
        }

        #region IntPtr
        public static unsafe int CalculateOffset<T>(IntPtr value, ref T baseValue)
            where T : struct
        {
            return (int)(value.ToInt64() - UnsafeExtensions.AddressOf(ref baseValue).ToInt64());
        }

        public static unsafe int CalculateOffset(IntPtr value, IntPtr baseValue)
        {
            return (int)(value.ToInt64() - baseValue.ToInt64());
        }
        #endregion // IntPtr
        #endregion // Unity.Physics
    }
}
