using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Assertions;
using Interlocked = System.Threading.Interlocked;

namespace PKGE
{
    public struct ParallelBitArray : INativeDisposable
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/GPUDriven/Utilities/ParallelBitArray.cs
        #region UnityEngine.Rendering
        private readonly AllocatorManager.AllocatorHandle _allocator;
        private NativeList<long> _bits;

        public readonly bool IsCreated
        {
            get { return _bits.IsCreated; }
        }

        public ParallelBitArray(int length, AllocatorManager.AllocatorHandle allocator)
        {
            _allocator = allocator;
            _bits = new NativeList<long>((length + 63) / 64, allocator);
        }

        public void Dispose()
        {
            _bits.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            return _bits.Dispose(inputDeps);
        }

        public void Resize(int newLength)
        {
            int oldLength = _bits.Length;
            if (newLength == oldLength)
                return;

            int oldBitsLength = _bits.Capacity;
            int newBitsLength = (newLength + 63) / 64;
            if (newBitsLength != oldBitsLength)
            {
                _bits.ResizeUninitialized(newBitsLength);
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
        }

        public void Set(int index, bool value)
        {
            Assert.IsTrue(0 <= index && index < _bits.Length);

            int entryIndex = index >> 6;
            ref long entry = ref _bits.ElementAt(entryIndex);

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
            Assert.IsTrue(0 <= index && index < _bits.Length);

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

        public ulong InterlockedReadChunk(int chunkIndex)
        {
            return (ulong)Interlocked.Read(ref _bits.ElementAt(chunkIndex));
        }

        public void InterlockedOrChunk(int chunkIndex, ulong chunkBits)
        {
            ref long entry = ref _bits.ElementAt(chunkIndex);

            long oldEntry, newEntry;
            do
            {
                oldEntry = Interlocked.Read(ref entry);
                newEntry = oldEntry | (long)chunkBits;
            } while (Interlocked.CompareExchange(ref entry, newEntry, oldEntry) != oldEntry);
        }

        public readonly int ChunkCount()
        {
            return _bits.Length;
        }

        public NativeArray<long> GetBitsArray()
        {
            return _bits.AsArray();
        }

        public void FillZeroes(int length)
        {
            length = System.Math.Min(length, _bits.Length);
            int chunkIndex = length / 64;
            int remainder = length & 63;

            _bits.AsArray().AsSpan().Slice(0, chunkIndex).Fill(0);

            if (remainder > 0)
            {
                long lastChunkMask = (1L << remainder) - 1;
                _bits[chunkIndex] &= ~lastChunkMask;
            }
        }
        #endregion // UnityEngine.Rendering

        public JobHandle FillZeroesJob(int length, JobHandle handle = default)
        {
            if (_allocator <= Allocator.Temp)
            {
                FillZeroes(length);
                return handle;
            }

            length = System.Math.Min(length, _bits.Length);
            int chunkIndex = length / 64;
            int remainder = length & 63;

            if (remainder > 0)
            {
                long lastChunkMask = (1L << remainder) - 1;
                _bits[chunkIndex] &= ~lastChunkMask;
            }

            return new SetArrayJob<long>
            {
                src = 0,
                dst = _bits.AsArray(),
            }.Schedule(chunkIndex, handle);
        }
    }
}
