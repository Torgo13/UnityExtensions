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
        
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Utilities/ArrayExtensions.cs
        #region UnityEngine.Rendering
        /// <summary>
        /// Fills an array with the same value.
        /// </summary>
        /// <typeparam name="T">The type of the array</typeparam>
        /// <param name="array">Target array to fill</param>
        /// <param name="value">Value to fill</param>
        /// <param name="startIndex">Start index to fill</param>
        /// <param name="length">The number of entries to write, or -1 to fill until the end of the array</param>
        public static void FillArray<T>(this ref NativeArray<T> array, in T value, int startIndex = 0, int length = -1)
            where T : unmanaged
        {
            Assert.IsTrue(startIndex >= 0);
            Assert.IsTrue(startIndex + length < array.Length);

            unsafe
            {
                T* ptr = (T*)array.GetUnsafePtr();

                int endIndex = length == -1 ? array.Length : startIndex + length;

                for (int i = startIndex; i < endIndex; ++i)
                    ptr[i] = value;
            }
        }
        #endregion // UnityEngine.Rendering

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
        #endregion // Unity.Physics
    }
}
