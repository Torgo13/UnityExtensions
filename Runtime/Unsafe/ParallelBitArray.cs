using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace PKGE.Unsafe
{
    public struct ParallelBitArray
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/GPUDriven/Utilities/ParallelBitArray.cs
        #region UnityEngine.Rendering
        private readonly Allocator _allocator;
        private NativeArray<long> _bits;
        private int _length;

        public readonly int Length => _length;

        public bool IsCreated
        {
            get { return _bits.IsCreated; }
        }

        public ParallelBitArray(int length, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            _allocator = allocator;
            _bits = new NativeArray<long>((length + 63) / 64, allocator, options);
            _length = length;
        }

        public void Dispose()
        {
            _bits.Dispose();
            _length = 0;
        }

        public void Dispose(JobHandle inputDeps)
        {
            _ = _bits.Dispose(inputDeps);
            _length = 0;
        }

        public void Resize(int newLength)
        {
            int oldLength = _length;
            if (newLength == oldLength)
                return;

            int oldBitsLength = _bits.Length;
            int newBitsLength = (newLength + 63) / 64;
            if (newBitsLength != oldBitsLength)
            {
                var newBits = new NativeArray<long>(newBitsLength, _allocator, NativeArrayOptions.UninitializedMemory);
                if (_bits.IsCreated)
                {
                    NativeArray<long>.Copy(_bits, newBits, _bits.Length);
                    _bits.Dispose();
                }

                _bits = newBits;
            }

            // mask off bits past the length
            int validLength = System.Math.Min(oldLength, newLength);
            int validBitsLength = System.Math.Min(oldBitsLength, newBitsLength);
            for (int chunkIndex = validBitsLength; chunkIndex < _bits.Length; ++chunkIndex)
            {
                int validBitCount = System.Math.Max(validLength - 64 * chunkIndex, 0);
                if (validBitCount < 64)
                {
                    ulong validMask = (1ul << validBitCount) - 1;
                    _bits[chunkIndex] &= (long)validMask;
                }
            }

            _length = newLength;
        }

        public readonly void Set(int index, bool value)
        {
            Assert.IsTrue(0 <= index && index < _length);

            int entryIndex = index >> 6;
            ref long entry = ref _bits.UnsafeElementAtMutable(entryIndex);

            ulong bit = 1ul << (index & 0x3f);
            long andMask = (long)(~bit);
            long orMask = value ? (long)bit : 0;

            long oldEntry, newEntry;
            do
            {
                oldEntry = Interlocked.Read(ref entry);
                newEntry = (oldEntry & andMask) | orMask;
            } while (Interlocked.CompareExchange(ref entry, newEntry, oldEntry) != oldEntry);
        }

        public readonly bool Get(int index)
        {
            Assert.IsTrue(0 <= index && index < _length);

            int entryIndex = index >> 6;

            ulong bit = 1ul << (index & 0x3f);
            long checkMask = (long)bit;
            return (_bits[entryIndex] & checkMask) != 0;
        }

        public ulong GetChunk(int chunkIndex)
        {
            return (ulong)_bits[chunkIndex];
        }

        public void SetChunk(int chunkIndex, ulong chunkBits)
        {
            _bits[chunkIndex] = (long)chunkBits;
        }

        public readonly ulong InterlockedReadChunk(int chunkIndex)
        {
            return (ulong)Interlocked.Read(ref _bits.UnsafeElementAt(chunkIndex));
        }

        public readonly void InterlockedOrChunk(int chunkIndex, ulong chunkBits)
        {
            ref long entry = ref _bits.UnsafeElementAtMutable(chunkIndex);

            long oldEntry, newEntry;
            do
            {
                oldEntry = Interlocked.Read(ref entry);
                newEntry = oldEntry | (long)chunkBits;
            } while (Interlocked.CompareExchange(ref entry, newEntry, oldEntry) != oldEntry);
        }

        public int ChunkCount()
        {
            return _bits.Length;
        }

        public ParallelBitArray GetSubArray(int length)
        {
            ParallelBitArray array = new ParallelBitArray();
            array._bits = _bits.GetSubArray(0, (length + 63) / 64);
            array._length = length;
            return array;
        }

        public readonly NativeArray<long> GetBitsArray()
        {
            return _bits;
        }

        public void FillZeroes(int length)
        {
            length = System.Math.Min(length, _length);
            int chunkIndex = length / 64;
            int remainder = length & 63;

            _bits.FillArray(0, 0, chunkIndex);

            if(remainder > 0)
            {
                long lastChunkMask = (1L << remainder) - 1;
                _bits[chunkIndex] &= ~lastChunkMask;
            }
        }
        #endregion // UnityEngine.Rendering
    }
}
