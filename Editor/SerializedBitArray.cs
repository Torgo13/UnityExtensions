using System;
using UnityEditor;

namespace UnityExtensions.Editor
{
    /// <summary>Serialisation of BitArray, Utility class</summary>
    public static class SerializedBitArrayUtilities
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Editor/Utilities/SerializedBitArray.cs
        #region UnityEditor.Rendering
        /// <summary>Construct a SerializedBitArrayAny of appropriate size</summary>
        /// <param name="serializedProperty">The SerializedProperty</param>
        /// <param name="targetSerializedObjects">An individual SerializedObject for each targetObject</param>
        /// <returns>A SerializedBitArrayAny</returns>
        public static SerializedBitArrayAny ToSerializedBitArray(this SerializedProperty serializedProperty, SerializedObject[] targetSerializedObjects)
        {
            if (!TryGetCapacityFromTypeName(serializedProperty, out uint capacity))
                throw new Exception("Cannot get SerializeBitArray's Capacity");
            return new SerializedBitArrayAny(serializedProperty, targetSerializedObjects, capacity);
        }

        /// <summary>Construct a SerializedBitArrayAny of appropriate size</summary>
        /// <param name="serializedProperty">The SerializedProperty</param>
        /// <returns>A SerializedBitArrayAny</returns>
        /// <remarks>Note that this variant doesn't properly support editing multiple targets, especially if there
        /// are several BitArrays on the target objects.</remarks>
        public static SerializedBitArrayAny ToSerializedBitArray(this SerializedProperty serializedProperty)
        {
            if (!TryGetCapacityFromTypeName(serializedProperty, out uint capacity))
                throw new Exception("Cannot get SerializeBitArray's Capacity");
            return new SerializedBitArrayAny(serializedProperty, capacity);
        }

        /// <summary>Try to construct a SerializedBitArray of appropriate size</summary>
        /// <param name="serializedProperty">The SerializedProperty</param>
        /// <param name="targetSerializedObjects">An individual SerializedObject for each targetObject</param>
        /// <param name="serializedBitArray">Out SerializedBitArray</param>
        /// <returns>True if construction was successful</returns>
        public static bool TryGetSerializedBitArray(this SerializedProperty serializedProperty, SerializedObject[] targetSerializedObjects, out SerializedBitArrayAny serializedBitArray)
        {
            serializedBitArray = null;
            if (!TryGetCapacityFromTypeName(serializedProperty, out uint capacity))
                return false;
            serializedBitArray = new SerializedBitArrayAny(serializedProperty, targetSerializedObjects, capacity);
            return true;
        }

        /// <summary>Try to construct a SerializedBitArray of appropriate size</summary>
        /// <param name="serializedProperty">The SerializedProperty</param>
        /// <param name="serializedBitArray">Out SerializedBitArray</param>
        /// <returns>True if construction was successful</returns>
        /// <remarks>Note that this variant doesn't properly support editing multiple targets, especially if there
        /// are several BitArrays on the target objects.</remarks>
        public static bool TryGetSerializedBitArray(this SerializedProperty serializedProperty, out SerializedBitArrayAny serializedBitArray)
        {
            serializedBitArray = null;
            if (!TryGetCapacityFromTypeName(serializedProperty, out uint capacity))
                return false;
            serializedBitArray = new SerializedBitArrayAny(serializedProperty, capacity);
            return true;
        }

        static bool TryGetCapacityFromTypeName(SerializedProperty serializedProperty, out uint capacity)
        {
            capacity = 0u;
            const string baseTypeName = "BitArray";
            string type = serializedProperty.type;
            return type.StartsWith(baseTypeName)
                && uint.TryParse(type.AsSpan(baseTypeName.Length), out capacity);
        }
        #endregion // UnityEditor.Rendering
    }

    /// <summary>interface to handle generic SerializedBitArray</summary>
    public interface ISerializedBitArray
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Editor/Utilities/SerializedBitArray.cs
        #region UnityEditor.Rendering
        /// <summary>Capacity of the bitarray</summary>
        uint capacity { get; }
        /// <summary>Get the bit at given index</summary>
        /// <param name="bitIndex">The index</param>
        /// <returns>Bit value</returns>
        bool GetBitAt(uint bitIndex);
        /// <summary>Set the bit at given index</summary>
        /// <param name="bitIndex">The index</param>
        /// <param name="value">The value</param>
        void SetBitAt(uint bitIndex, bool value);
        /// <summary>Does the bit at given index have multiple different values?</summary>
        /// <param name="bitIndex">The index</param>
        /// <returns>True: Multiple different value</returns>
        bool HasBitMultipleDifferentValue(uint bitIndex);
        #endregion // UnityEditor.Rendering
    }

    /// <summary>Base class for SerializedBitArrays</summary>
    public sealed class SerializedBitArrayAny : ISerializedBitArray
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Editor/Utilities/SerializedBitArray.cs
        #region UnityEditor.Rendering
        //To only be used for bit isolation as internal operator only support 32 bit formats.
        //Why:
        // - We cannot access easily the root of the BitArray from the target (most of the time there is a non null serialization path to take)
        // - The BitArray is a struct making any reflection process to access it difficult especially if we want to modify a bit and not just read (potential copy issue)
        // - We are in Editor only here so we can use Unity serialization to do this access, using a bunch of SerializedProperty. We just need to keep them in sync.
        // - For the bit isolation:
        //      - If we want to isolate and only work on 1 bit, we want to modify it, whatever the data was in other bits.
        //      - If the data on other bits was different per targets before writing on 1 bit, it should still be different per targets.
        //      - Internal method HasMultipleDifferentValuesBitwise and SetBitAtIndexForAllTargetsImmediate is only supported for 32bits formats.
        //Todo: Ideally, if we move this BitArray to Unity, we can rewrite a little the HasMultipleDifferentValuesBitwise and SetBitAtIndexForAllTargetsImmediate to work on other format and thus we should not need this _serializedObject anymore.
        readonly SerializedProperty[] _serializedObject;

        /// <summary>Capacity of the bitarray</summary>
        public uint capacity { get; }

        internal SerializedBitArrayAny(SerializedProperty serializedProperty, SerializedObject[] targetSerializedObjects, uint capacity)
        {
            this.capacity = capacity;
            _serializedObject = new SerializedProperty[targetSerializedObjects.Length];
            for (int i = 0; i < targetSerializedObjects.Length; i++)
            {
                _serializedObject[i] = targetSerializedObjects[i].FindProperty(serializedProperty.propertyPath);
            }
        }

        // Old constructor that doesn't properly work with multiple target objects.
        internal SerializedBitArrayAny(SerializedProperty serializedProperty, uint capacity)
        {
            this.capacity = capacity;
            _serializedObject = new SerializedProperty[serializedProperty.serializedObject.targetObjects.Length];
            for (int i = 0; i < serializedProperty.serializedObject.targetObjects.Length; i++)
            {
                _serializedObject[i] = new SerializedObject(serializedProperty.serializedObject.targetObjects[i]).FindProperty(serializedProperty.propertyPath);
            }
        }

        //To update if we need container over 256bits
        ulong GetTargetValueUnverified(int targetIndex, int part)
            => part switch
            {
                0 => Unbox(_serializedObject[targetIndex].FindPropertyRelative(capacity <= 64 ? "data" : "data1").boxedValue),
                1 => Unbox(_serializedObject[targetIndex].FindPropertyRelative("data2")?.boxedValue ?? 0ul),
                2 => Unbox(_serializedObject[targetIndex].FindPropertyRelative("data3")?.boxedValue ?? 0ul),
                3 => Unbox(_serializedObject[targetIndex].FindPropertyRelative("data4")?.boxedValue ?? 0ul),
                _ => 0ul
            };

        void SetTargetValueUnverified(int targetIndex, int part, object value)
        {
            switch (part)
            {
                case 0: _serializedObject[targetIndex].FindPropertyRelative(capacity <= 64 ? "data" : "data1").boxedValue = value; break;
                case 1: _serializedObject[targetIndex].FindPropertyRelative("data2").boxedValue = value; break;
                case 2: _serializedObject[targetIndex].FindPropertyRelative("data3").boxedValue = value; break;
                case 3: _serializedObject[targetIndex].FindPropertyRelative("data4").boxedValue = value; break;
            }
        }

        //we cannot directly cast from boxed value to the ulong we want in C#, We first need to unbox in the true type
        ulong Unbox(object boxedValue)
            => capacity switch
            {
                8 => (byte)boxedValue,
                16 => (ushort)boxedValue,
                32 => (uint)boxedValue,
                64 => (ulong)boxedValue,
                _ => (ulong)boxedValue //any higher is a composition of ulong
            };

        bool ExtractBitFrom64BitsPart(ulong part, uint bitIndexInPart)
            => (part & (1ul << (int) (bitIndexInPart % 64))) != 0;

        void AssertInRange(uint bitIndex)
        {
            if (bitIndex >= capacity)
                throw new IndexOutOfRangeException("Index out of bound in BitArray" + capacity);
        }

        /// <summary>Does the bit at given index have multiple different values?</summary>
        /// <param name="partIndex">The index of the 64bits bucket to check</param>
        /// <returns>Bitwise discrepancy over 64 bits. If 1 : multiple different value for this bit.</returns>
        ulong HasBitMultipleDifferentValueBitwiseOver64Bits(int partIndex)
        {
            ulong diff = 0ul;
            var firstValue = GetTargetValueUnverified(0, partIndex);
            for (int i = 1; i < _serializedObject.Length; ++i)
                diff |= firstValue ^ GetTargetValueUnverified(i, partIndex);
            return diff;
        }

        /// <summary>Does the bit at given index have multiple different values</summary>
        /// <param name="bitIndex">The index</param>
        /// <returns>True: Multiple different value for the given bit index</returns>
        public bool HasBitMultipleDifferentValue(uint bitIndex)
        {
            AssertInRange(bitIndex);

            return ExtractBitFrom64BitsPart(HasBitMultipleDifferentValueBitwiseOver64Bits((int)bitIndex / 64), bitIndex % 64);
        }

        /// <summary>Get the bit at given index</summary>
        /// <param name="bitIndex">The index</param>
        /// <returns>Bit value</returns>
        public bool GetBitAt(uint bitIndex)
        {
            AssertInRange(bitIndex);
            if (HasBitMultipleDifferentValue(bitIndex))
                return default; //we cannot assess anything if different

            //As no different value on this bit, lets use the one from first target
            return ExtractBitFrom64BitsPart(GetTargetValueUnverified(0, (int)bitIndex / 64), bitIndex % 64);
        }

        /// <summary>Set the bit at given index</summary>
        /// <param name="bitIndex">The index</param>
        /// <param name="value">The value</param>
        public void SetBitAt(uint bitIndex, bool value)
        {
            // Update the serialized object to make sure we have the latest values.
            Update();

            AssertInRange(bitIndex);

            int part = (int)bitIndex / 64;
            int indexInPart = (int)bitIndex % 64;
            for (int i = 0; i < _serializedObject.Length; ++i)
            {
                ulong targetValue = GetTargetValueUnverified(i, part);
                if (value)
                    targetValue |= 1ul << indexInPart;
                else
                    targetValue &= ~(1ul << indexInPart);
                SetTargetValueUnverified(i, part, targetValue);
            }

            ResyncSerialization();
        }

        /// <summary>Sync again every serializedProperty</summary>
        void ResyncSerialization()
        {
            ApplyModifiedProperties();
            Update();
        }

        /// <summary>Sync the reflected value with target value change</summary>
        public void Update()
        {
            foreach (var property in _serializedObject)
                property.serializedObject.Update();
        }

        /// <summary>Apply the reflected value onto targets</summary>
        public void ApplyModifiedProperties()
        {
            foreach (var property in _serializedObject)
                property.serializedObject.ApplyModifiedProperties();
        }
        #endregion // UnityEditor.Rendering
    }
}
