using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe
{
    public static class NativeCopyUtility
    {
        //https://github.com/needle-mirror/com.unity.xr.arfoundation/blob/master/Runtime/ARSubsystems/NativeCopyUtility.cs
        #region UnityEngine.XR.ARSubsystems
        /// <summary>
        /// Creates a <c>NativeArray</c> from a pointer by first copying <paramref name="length"/>
        /// <paramref name="defaultT"/>s into the <c>NativeArray</c>, and then overwriting the
        /// data in the array with <paramref name="source"/>, assuming each element in <paramref name="source"/>
        /// is <paramref name="sourceElementSize"/> bytes.
        /// </summary>
        /// <remarks>
        /// This is useful for native inter-operations with structs that might change over time. This allows
        /// new fields to be added to the C# struct without breaking data obtained from data calls.
        /// </remarks>
        /// <typeparam name="T">The type of struct to copy.</typeparam>
        /// <param name="defaultT">A default version of <typeparamref name="T"/>, which will be used to first fill the array
        /// before copying from <paramref name="source"/>.</param>
        /// <param name="source">A pointer to a contiguous block of data of size <paramref name="sourceElementSize"/> * <paramref name="length"/>.</param>
        /// <param name="sourceElementSize">The size of one element in <paramref name="source"/>.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <param name="allocator">The allocator to use when creating the <c>NativeArray</c>.</param>
        /// <returns>
        /// A new <c>NativeArray</c> populating with <paramref name="defaultT"/> and <paramref name="source"/>.
        /// The caller owns the memory.
        /// </returns>
        public static unsafe NativeArray<T> PtrToNativeArrayWithDefault<T>(
            T defaultT,
            void* source,
            int sourceElementSize,
            int length,
            Allocator allocator) where T : struct
        {
            var array = CreateArrayFilledWithValue(defaultT, length, allocator);

            // Then overwrite with the source data, which may have a different size
            UnsafeUtility.MemCpyStride(
                destination: array.GetUnsafePtr(),
                destinationStride: UnsafeUtility.SizeOf<T>(),
                source: source,
                sourceStride: sourceElementSize,
                elementSize: sourceElementSize,
                count: length);

            return array;
        }

        /// <summary>
        /// Fills <paramref name="array"/> with repeated copies of <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">The type of the <c>NativeArray</c>. Must be a <c>struct</c>.</typeparam>
        /// <param name="array">The array to fill.</param>
        /// <param name="value">The value with which to fill the array.</param>
        public static unsafe void FillArrayWithValue<T>(NativeArray<T> array, T value) where T : struct
        {
            // Early out if array is zero, or iOS will crash in MemCpyReplicate.
            if (array.Length == 0)
                return;

            UnsafeUtility.MemCpyReplicate(
                array.GetUnsafePtr(),
                UnsafeUtility.AddressOf(ref value),
                UnsafeUtility.SizeOf<T>(),
                array.Length);
        }

        /// <summary>
        /// Creates a new array allocated with <paramref name="allocator"/> and initialized with <paramref name="length"/>
        /// copies of <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">The type of the <c>NativeArray</c> to create. Must be a <c>struct</c>.</typeparam>
        /// <param name="value">The value with which to fill the array.</param>
        /// <param name="length">The length of the array to create.</param>
        /// <param name="allocator">The allocator with which to create the <c>NativeArray</c>.</param>
        /// <returns>A new <c>NativeArray</c> initialized with copies of <paramref name="value"/>.</returns>
        public static NativeArray<T> CreateArrayFilledWithValue<T>(T value, int length, Allocator allocator) where T : struct
        {
            var array = new NativeArray<T>(length, allocator, NativeArrayOptions.UninitializedMemory);
            FillArrayWithValue(array, value);
            return array;
        }
        #endregion // UnityEngine.XR.ARSubsystems
    }

    public static class NativeArrayExtensions
    {
        //https://github.com/Unity-Technologies/com.unity.formats.alembic/blob/main/com.unity.formats.alembic/Runtime/Scripts/Misc/RuntimeUtils.cs
        #region UnityEngine.Formats.Alembic.Importer
        public static unsafe void* GetPointer<T>(this NativeArray<T> array) where T : struct
        {
            return array.Length == 0 ? null : array.GetUnsafePtr();
        }
        #endregion // UnityEngine.Formats.Alembic.Importer
        
        public static unsafe void* GetReadOnlyPointer<T>(this NativeArray<T> array) where T : struct
        {
            return array.Length == 0 ? null : array.GetUnsafeReadOnlyPtr();
        }
        
        //https://github.com/Unity-Technologies/Graphics/blob/2ecb711df890ca21a0817cf610ec21c500cb4bfe/Packages/com.unity.render-pipelines.universal/Runtime/UniversalRenderPipelineCore.cs
        #region UnityEngine.Rendering.Universal
        /// <summary>
        /// IMPORTANT: Make sure you do not write to the value! There are no checks for this!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T UnsafeElementAt<T>(this NativeArray<T> array, int index) where T : struct
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafeReadOnlyPtr(), index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T UnsafeElementAtMutable<T>(this NativeArray<T> array, int index) where T : struct
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), index);
        }
        #endregion // UnityEngine.Rendering.Universal
        
        /// <summary>
        /// IMPORTANT: Make sure you do not write to the value! There are no checks for this!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T UnsafeElementAt<T>(this NativeList<T> nativeList, int index) where T : unmanaged
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(nativeList.GetUnsafeReadOnlyPtr(), index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T UnsafeElementAtMutable<T>(this NativeList<T> nativeList, int index) where T : unmanaged
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(nativeList.GetUnsafePtr(), index);
        }
        
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

        public static unsafe int GrowBy<TValue>(ref NativeArray<TValue> array, int count, Allocator allocator)
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
            UnsafeUtility.MemCpy(newArray.GetUnsafePtr(), array.GetUnsafeReadOnlyPtr(),
                (long)length * UnsafeUtility.SizeOf<TValue>());
            array.Dispose();
            array = newArray;

            return length;
        }

        public static unsafe void EraseAtWithCapacity<TValue>(this NativeArray<TValue> array, ref int count, int index)
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
    }
}
