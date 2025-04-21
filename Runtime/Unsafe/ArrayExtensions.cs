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
            if (!array.IsCreated)
                throw new InvalidOperationException(nameof(array));
            if (startIndex < 0)
                throw new IndexOutOfRangeException(nameof(startIndex));
            if (startIndex + length >= array.Length)
                throw new IndexOutOfRangeException(nameof(length));

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

        public static unsafe int CalculateOffset<T>(IntPtr value, ref T baseValue)
            where T : struct
        {
            return (int)(value.ToInt64() - UnsafeExtensions.AddressOf(ref baseValue).ToInt64());
        }

        public static unsafe int CalculateOffset(IntPtr value, IntPtr baseValue)
        {
            return (int)(value.ToInt64() - baseValue.ToInt64());
        }
        #endregion // Unity.Physics

        //https://github.com/Unity-Technologies/InputSystem/blob/fb786d2a7d01b8bcb8c4218522e5f4b9afea13d7/Packages/com.unity.inputsystem/InputSystem/Utilities/ArrayHelpers.cs
        #region UnityEngine.InputSystem.Utilities
        public static unsafe void Resize<TValue>(ref NativeArray<TValue> array, int newSize, Allocator allocator)
            where TValue : struct
        {
            var oldSize = array.Length;
            if (oldSize == newSize)
                return;

            if (newSize == 0)
            {
                if (array.IsCreated)
                    array.Dispose();
                array = new NativeArray<TValue>();
                return;
            }

            var newArray = new NativeArray<TValue>(newSize, allocator);
            if (oldSize != 0)
            {
                // Copy contents from old array.
                UnsafeUtility.MemCpy(newArray.GetUnsafePtr(), array.GetUnsafeReadOnlyPtr(),
                    UnsafeUtility.SizeOf<TValue>() * (newSize < oldSize ? newSize : oldSize));
                array.Dispose();
            }
            array = newArray;
        }

        public static unsafe int GrowBy<TValue>(ref NativeArray<TValue> array, int count, Allocator allocator = Allocator.Persistent)
            where TValue : struct
        {
            var length = array.Length;
            if (length == 0)
            {
                array = new NativeArray<TValue>(count, allocator);
                return 0;
            }

            var newArray = new NativeArray<TValue>(length + count, allocator);
            // CopyFrom() expects length to match. Copy manually.
            UnsafeUtility.MemCpy(newArray.GetUnsafePtr(), array.GetUnsafeReadOnlyPtr(), (long)length * UnsafeUtility.SizeOf<TValue>());
            array.Dispose();
            array = newArray;

            return length;
        }

        public static unsafe void EraseAtWithCapacity<TValue>(NativeArray<TValue> array, ref int count, int index)
            where TValue : struct
        {
            Assert.IsTrue(array.IsCreated);
            Assert.IsTrue(count <= array.Length);
            Assert.IsTrue(index >= 0 && index < count);

            // If we're erasing from the beginning or somewhere in the middle, move
            // the array contents down from after the index.
            if (index < count - 1)
            {
                var elementSize = UnsafeUtility.SizeOf<TValue>();
                var arrayPtr = (byte*)array.GetUnsafePtr();

                UnsafeUtility.MemCpy(arrayPtr + elementSize * index, arrayPtr + elementSize * (index + 1),
                    (count - index - 1) * elementSize);
            }

            --count;
        }

        public static int AppendWithCapacity<TValue>(ref NativeArray<TValue> array, ref int count, TValue value,
            int capacityIncrement = 10, Allocator allocator = Allocator.Persistent)
            where TValue : struct
        {
            var capacity = array.Length;
            if (capacity == count)
                GrowBy(ref array, capacityIncrement > 1 ? capacityIncrement : 1, allocator);

            var index = count;
            array[index] = value;
            ++count;

            return index;
        }

        public static int GrowWithCapacity<TValue>(ref NativeArray<TValue> array, ref int count, int growBy,
            int capacityIncrement = 10, Allocator allocator = Allocator.Persistent)
            where TValue : struct
        {
            var length = array.Length;
            if (length < count + growBy)
            {
                if (capacityIncrement < growBy)
                    capacityIncrement = growBy;
                GrowBy(ref array, capacityIncrement, allocator);
            }

            var offset = count;
            count += growBy;
            return offset;
        }
        #endregion // UnityEngine.InputSystem.Utilities
    }
}
