using System;
using Unity.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe
{
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("Length = {Length}")]
    [DebuggerTypeProxy(typeof(ArraySliceDebugView<>))]
    public unsafe struct ArraySlice<T> : System.IEquatable<ArraySlice<T>> where T : struct
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.universal/Runtime/2D/UTess2D/ArraySlice.cs
        #region UnityEngine.Rendering.Universal.UTess
        [NativeDisableUnsafePtrRestriction] internal byte* _buffer;
        internal int _stride;
        internal int _length;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal int _minIndex;
        internal int _maxIndex;
        internal AtomicSafetyHandle _safety;
#endif

        public ArraySlice(NativeArray<T> array, int start, int length)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), $"Slice start {start} < 0.");
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), $"Slice length {length} < 0.");
            if (start + length > array.Length)
                throw new ArgumentException(
                    $"Slice start + length ({start + length}) range must be <= array.Length ({array.Length})");
            _minIndex = 0;
            _maxIndex = length - 1;
            _safety = NativeArrayUnsafeUtility.GetAtomicSafetyHandle(array);
#endif

            _stride = UnsafeUtility.SizeOf<T>();
            var ptr = (byte*)array.GetUnsafePtr() + _stride * start;
            _buffer = ptr;
            _length = length;
        }

        public bool Equals(ArraySlice<T> other)
        {
            return _buffer == other._buffer && _stride == other._stride && _length == other._length;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ArraySlice<T> slice && Equals(slice);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_buffer;
                hashCode = (hashCode * 397) ^ _stride;
                hashCode = (hashCode * 397) ^ _length;
                return hashCode;
            }
        }

        public static bool operator==(ArraySlice<T> left, ArraySlice<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator!=(ArraySlice<T> left, ArraySlice<T> right)
        {
            return !left.Equals(right);
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        // These are double-whammy excluded to we can elide bounds checks in the Burst disassembly view
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckReadIndex(int index)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (index < _minIndex || index > _maxIndex)
                FailOutOfRangeError(index);
#endif
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckWriteIndex(int index)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (index < _minIndex || index > _maxIndex)
                FailOutOfRangeError(index);
#endif
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void FailOutOfRangeError(int index)
        {
            if (index < Length && (_minIndex != 0 || _maxIndex != Length - 1))
                throw new System.IndexOutOfRangeException(
                    $"Index {index} is out of restricted IJobParallelFor range [{_minIndex}...{_maxIndex}] in ReadWriteBuffer.\n" +
                    "ReadWriteBuffers are restricted to only read & write the element at the job index. " +
                    "You can use double buffering strategies to avoid race conditions due to " +
                    "reading & writing in parallel to the same elements from a job.");

            throw new System.IndexOutOfRangeException($"Index {index} is out of range of '{Length}' Length.");
        }

#endif

        public static ArraySlice<T> ConvertExistingDataToArraySlice(void* dataPointer, int stride, int length)
        {
            if (length < 0)
                throw new System.ArgumentException($"Invalid length of '{length}'. It must be greater than 0.",
                    nameof(length));
            if (stride < 0)
                throw new System.ArgumentException($"Invalid stride '{stride}'. It must be greater than 0.",
                    nameof(stride));

            var newSlice = new ArraySlice<T>
            {
                _stride = stride,
                _buffer = (byte*)dataPointer,
                _length = length,
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                _minIndex = 0,
                _maxIndex = length - 1,
#endif
            };

            return newSlice;
        }

        public T this[int index]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                CheckReadIndex(index);
#endif
                return UnsafeUtility.ReadArrayElementWithStride<T>(_buffer, index, _stride);
            }

            [WriteAccessRequired]
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                CheckWriteIndex(index);
#endif
                UnsafeUtility.WriteArrayElementWithStride(_buffer, index, _stride, value);
            }
        }

        private void* GetUnsafeReadOnlyPtr()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(_safety);
#endif
            return _buffer;
        }

        private void CopyTo(T[] array)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (Length != array.Length)
                throw new ArgumentException($"array.Length ({array.Length}) does not match the Length of this instance ({Length}).", nameof(array));
#endif
            GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            IntPtr addr = handle.AddrOfPinnedObject();

            var sizeOf = UnsafeUtility.SizeOf<T>();
            UnsafeUtility.MemCpyStride((byte*)addr, sizeOf, this.GetUnsafeReadOnlyPtr(), Stride, sizeOf, _length);

            handle.Free();
        }

        internal T[] ToArray()
        {
            var array = new T[Length];
            CopyTo(array);
            return array;
        }

        public int Stride => _stride;
        public int Length => _length;
        #endregion // UnityEngine.Rendering.Universal.UTess
    }

    /// <summary>
    /// DebuggerTypeProxy for <see cref="ArraySlice{T}"/>
    /// </summary>
    public sealed class ArraySliceDebugView<T> where T : struct
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.universal/Runtime/2D/UTess2D/ArraySlice.cs
        #region UnityEngine.Rendering.Universal.UTess
        ArraySlice<T> _slice;

        public ArraySliceDebugView(ArraySlice<T> slice)
        {
            _slice = slice;
        }

        public T[] Items
        {
            get { return _slice.ToArray(); }
        }
        #endregion // UnityEngine.Rendering.Universal.UTess
    }
}
