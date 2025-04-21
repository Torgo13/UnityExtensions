using System.Runtime.CompilerServices;

namespace UnityExtensions.Unsafe
{
    public static class MathematicsExtensions
    {
        //https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/InputSystem/Utilities/NumberHelpers.cs
        #region InputSystem
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AlignToMultipleOf(this int number, int alignment)
        {
            var remainder = number % alignment;
            if (remainder == 0)
                return number;

            return number + alignment - remainder;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long AlignToMultipleOf(this long number, long alignment)
        {
            var remainder = number % alignment;
            if (remainder == 0)
                return number;

            return number + alignment - remainder;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AlignToMultipleOf(this uint number, uint alignment)
        {
            var remainder = number % alignment;
            if (remainder == 0)
                return number;

            return number + alignment - remainder;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float IntToNormalizedFloat(this int value, int minValue, int maxValue)
        {
            if (value <= minValue)
                return 0.0f;
            if (value >= maxValue)
                return 1.0f;

            // using double here because int.MaxValue is not representable in floats
            // as int.MaxValue = 2147483647 will become 2147483648.0 when casted to a float
            return (float)(((double)value - minValue) / ((double)maxValue - minValue));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NormalizedFloatToInt(this float value, int intMinValue, int intMaxValue)
        {
            if (value <= 0.0f)
                return intMinValue;
            if (value >= 1.0f)
                return intMaxValue;

            return (int)(value * ((double)intMaxValue - intMinValue) + intMinValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float UIntToNormalizedFloat(this uint value, uint minValue, uint maxValue)
        {
            if (value <= minValue)
                return 0.0f;

            if (value >= maxValue)
                return 1.0f;

            // using double here because uint.MaxValue is not representable in floats
            return (float)(((double)value - minValue) / ((double)maxValue - minValue));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint NormalizedFloatToUInt(this float value, uint uintMinValue, uint uintMaxValue)
        {
            if (value <= 0.0f)
                return uintMinValue;

            if (value >= 1.0f)
                return uintMaxValue;

            return (uint)(value * ((double)uintMaxValue - uintMinValue) + uintMinValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RemapUIntBitsToNormalizeFloatToUIntBits(this uint value, uint inBitSize, uint outBitSize)
        {
            var inMaxValue = (uint)((1UL << (int)inBitSize) - 1);
            var outMaxValue = (uint)((1UL << (int)outBitSize) - 1);

            var normFloat = UIntToNormalizedFloat(value, 0, inMaxValue);
            return NormalizedFloatToUInt(normFloat, 0, outMaxValue);
        }
        #endregion // InputSystem
    }
}
