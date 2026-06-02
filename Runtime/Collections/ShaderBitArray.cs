#nullable enable
using UnityEngine;
using UnityEngine.Assertions;

namespace PKGE
{
    /// <summary>
    /// A dynamic array of bits backed by a managed array of floats,
    /// since that's what Unity Shader constant API offers.
    /// </summary>
    /// <example><code>
    /// ShaderBitArray bits;
    /// bits.Resize(8);
    /// bits[0] = true;
    /// cmd.SetGlobalFloatArray("_BitArray", bits.data);
    /// bits.Clear();
    /// </code></example>
    public struct ShaderBitArray
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.universal/Runtime/ShaderBitArray.cs
        #region UnityEngine.Rendering.Universal
        const int BitsPerElement = 32;
        const int ElementShift = 5;
        const int ElementMask = (1 << ElementShift) - 1;

        private float[]? _data;

        public readonly int elemLength => _data == null ? 0 : _data.Length;
        public readonly int bitCapacity => elemLength * BitsPerElement;
        public readonly float[]? data => _data;

        public void Resize(int bitCount)
        {
            if (bitCapacity > bitCount)
                return;

            int newElemCount = ((bitCount + (BitsPerElement - 1)) / BitsPerElement);
            if (newElemCount == _data?.Length)
                return;

            var newData = new float[newElemCount];
            if (_data != null)
            {
                for (int i = 0; i < _data.Length; i++)
                    newData[i] = _data[i];
            }

            _data = newData;
        }

        public readonly void Clear()
        {
            if (_data == null)
                return;

            for (int i = 0; i < _data.Length; i++)
                _data[i] = 0;
        }

        private static void GetElementIndexAndBitOffset(int index, out int elemIndex, out int bitOffset)
        {
            elemIndex = index >> ElementShift;
            bitOffset = index & ElementMask;
        }

        readonly
        public bool this[int index]
        {
            get
            {
                GetElementIndexAndBitOffset(index, out var elemIndex, out var bitOffset);

                if (_data == null || elemIndex >= _data.Length)
                    return false;

                var uintElem = new Union4 { Float = _data[elemIndex] }.UInt;
                return (uintElem & (1u << bitOffset)) != 0u;
            }
            set
            {
                GetElementIndexAndBitOffset(index, out var elemIndex, out var bitOffset);
                
                if (_data == null || elemIndex >= _data.Length)
                    return;

                var uintElem = new Union4 { Float = _data[elemIndex] }.UInt;
                if (value)
                    uintElem |= 1u << bitOffset;
                else
                    uintElem &= ~(1u << bitOffset);

                _data[elemIndex] = new Union4 { UInt = uintElem }.Float;
            }
        }

        public override readonly string ToString()
        {
            const int maxCapacity = 4096;
            Assert.IsTrue(bitCapacity < maxCapacity, $"Bit string is too long. It was truncated to {maxCapacity} elements.");
            int len = System.Math.Min(bitCapacity, maxCapacity);
            System.Span<char> buf = stackalloc char[len];
            for (int i = 0; i < len; i++)
            {
                buf[i] = this[i] ? '1' : '0';
            }

            return new string(buf);
        }
        #endregion // UnityEngine.Rendering.Universal
    }
}
