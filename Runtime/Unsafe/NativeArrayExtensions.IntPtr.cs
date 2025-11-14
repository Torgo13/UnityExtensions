#if PKGE_USING_INTPTR
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PKGE.Unsafe
{
    public static partial class NativeCopyUtility
    {
        //https://github.com/needle-mirror/com.unity.xr.arfoundation/blob/master/Runtime/ARSubsystems/NativeCopyUtility.cs
        #region UnityEngine.XR.ARSubsystems
        public static unsafe NativeArray<T> PtrToNativeArrayWithDefault<T>(
            T defaultT,
            IntPtr source,
            int sourceElementSize,
            int length,
            Allocator allocator) where T : struct
        {
            return PtrToNativeArrayWithDefault(defaultT, (void*)source, sourceElementSize, length, allocator);
        }
        #endregion // UnityEngine.XR.ARSubsystems
    }

    public static partial class NativeArrayExtensions
    {
        //https://github.com/Unity-Technologies/UnityCsReference/blob/4b463aa72c78ec7490b7f03176bd012399881768/Runtime/Export/NativeArray/NativeArray.cs#L1024
        #region Unity.Collections.LowLevel.Unsafe
        /// <inheritdoc cref="ConvertExistingDataToNativeArray{T}(Span{T}, Allocator)"/>
        public static unsafe NativeArray<T> ConvertExistingDataToNativeArray<T>(IntPtr dataPointer, int length,
            Allocator allocator = Allocator.None) where T : struct
        {
            Assert.AreNotEqual(IntPtr.Zero, dataPointer);

            return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>((void*)dataPointer, length, allocator);
        }
        #endregion // Unity.Collections.LowLevel.Unsafe
        
        #region UnityEngine.Formats.Alembic.Importer
        public static unsafe IntPtr GetIntPtr<T>(this NativeArray<T> array) where T : struct
        {
            return array.IsCreated ? (IntPtr)array.GetUnsafePtr() : IntPtr.Zero;
        }

        public static unsafe IntPtr GetReadOnlyIntPtr<T>(this NativeArray<T> array) where T : struct
        {
            return array.IsCreated ? (IntPtr)array.GetUnsafeReadOnlyPtr() : IntPtr.Zero;
        }
        #endregion // UnityEngine.Formats.Alembic.Importer
    }
}
#endif // PKGE_USING_INTPTR
