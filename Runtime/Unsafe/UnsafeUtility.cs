// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityExtensions.Unsafe
{
    public static partial class UnsafeUtility
    {
        //https://github.com/Unity-Technologies/UnityCsReference/blob/6000.1/Runtime/Export/Unsafe/UnsafeUtility.cs
        #region Unity.Collections.LowLevel.Unsafe
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> GetByteSpanFromArray(System.Array array, int elementSize)
        {
            if (array == null || array.Length == 0)
                return new Span<byte>();

            System.Diagnostics.Debug.Assert(Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf(array.GetType().GetElementType()) == elementSize);

            var bArray = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<System.Array, byte[]>(ref array);
            return new Span<byte>(Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf(ref bArray[0]), array.Length * elementSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> GetByteSpanFromList<T>(List<T> list) where T : struct
        {
            return MemoryMarshal.AsBytes(NoAllocHelpers.ExtractArrayFromList(list).AsSpan());
        }
        #endregion // Unity.Collections.LowLevel.Unsafe

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T As<T>(object from) where T : class
        {
            return System.Runtime.CompilerServices.Unsafe.As<T>(from);
        }
    }
}