using System.Runtime.CompilerServices;

namespace PKGE.Unsafe
{
    public static class ByteUtility
    {
        //https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/3417c4765f52f72d2384f2f7e65bd9d2d1dfd7ac/com.unity.netcode.gameobjects/Runtime/Serialization/ByteUtility.cs
        #region Unity.Netcode
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBit(byte bitField, ushort bitPosition)
        {
            return (bitField & (1 << bitPosition)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBit(ref byte bitField, ushort bitPosition, bool value)
        {
            bitField = (byte)((bitField & ~(1 << bitPosition)) | (value.ToByte() << bitPosition));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBit(ushort bitField, ushort bitPosition)
        {
            return (bitField & (1 << bitPosition)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBit(ref ushort bitField, ushort bitPosition, bool value)
        {
            bitField = (ushort)((bitField & ~(1 << bitPosition)) | (value.ToByte() << bitPosition));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBit(uint bitField, ushort bitPosition)
        {
            return (bitField & (1 << bitPosition)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBit(ref uint bitField, ushort bitPosition, bool value)
        {
            bitField = (uint)((bitField & ~(1 << bitPosition)) | ((uint)value.ToByte() << bitPosition));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBit(ulong bitField, ushort bitPosition)
        {
            return (bitField & (ulong)(1 << bitPosition)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBit(ref ulong bitField, ushort bitPosition, bool value)
        {
            bitField = ((bitField & (ulong)~(1 << bitPosition)) | ((ulong)value.ToByte() << bitPosition));
        }
        #endregion // Unity.Netcode

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBit(int bitField, int bitPosition)
        {
            return (bitField & (1 << bitPosition)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBit(ref int bitField, int bitPosition, bool value)
        {
            bitField = (bitField & ~(1 << bitPosition)) | (value.ToByte() << bitPosition);
        }
    }

    //https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/develop/com.unity.netcode.gameobjects/Runtime/Serialization/ByteUtility.cs
    #region Unity.Netcode
    public static class ByteExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe byte ToByte(this bool b) => *(byte*)&b;
    }
    #endregion // Unity.Netcode
}