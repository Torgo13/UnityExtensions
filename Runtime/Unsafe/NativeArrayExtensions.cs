using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PKGE.Unsafe
{
    public static partial class NativeArrayExtensions
    {
#if PKGE_USING_UNSAFE
        //https://github.com/Unity-Technologies/com.unity.formats.alembic/blob/main/com.unity.formats.alembic/Runtime/Scripts/Misc/RuntimeUtils.cs
        #region UnityEngine.Formats.Alembic.Importer
        public static unsafe void* GetPtr<T>(this NativeArray<T> array) where T : struct
        {
            return array.IsCreated ? array.GetUnsafePtr() : null;
        }

        public static unsafe void* GetReadOnlyPtr<T>(this NativeArray<T> array) where T : struct
        {
            return array.IsCreated ? array.GetUnsafeReadOnlyPtr() : null;
        }
        #endregion // UnityEngine.Formats.Alembic.Importer
#endif // PKGE_USING_UNSAFE

        //https://github.com/Unity-Technologies/Graphics/blob/2ecb711df890ca21a0817cf610ec21c500cb4bfe/Packages/com.unity.render-pipelines.universal/Runtime/UniversalRenderPipelineCore.cs
        #region UnityEngine.Rendering.Universal
        /// <summary>
        /// IMPORTANT: Make sure you do not write to the value! There are no checks for this!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T UnsafeElementAt<T>(this NativeArray<T> array, int index) where T : struct
        {
            Assert.IsTrue(index >= 0);
            Assert.IsTrue(index < array.Length);

            return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafeReadOnlyPtr(), index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T UnsafeElementAtMutable<T>(this NativeArray<T> array, int index) where T : struct
        {
            Assert.IsTrue(index >= 0);
            Assert.IsTrue(index < array.Length);
            
            return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), index);
        }
        #endregion // UnityEngine.Rendering.Universal

        //https://github.com/Unity-Technologies/InputSystem/blob/fb786d2a7d01b8bcb8c4218522e5f4b9afea13d7/Packages/com.unity.inputsystem/InputSystem/Utilities/ArrayHelpers.cs
        #region UnityEngine.InputSystem.Utilities
        public static void EraseAtWithCapacity<TValue>(this NativeArray<TValue> array, ref int count, int index)
            where TValue : struct
        {
            Assert.IsTrue(array.IsCreated);
            Assert.IsTrue(count <= array.Length);
            Assert.IsTrue(index >= 0 && index < count);

            // If we're erasing from the beginning or somewhere in the middle, move
            // the array contents down from after the index.
            if (index < count - 1)
            {
                var elementSize = SizeOfCache<TValue>.Size;
                unsafe
                {
                    var arrayPtr = (byte*)array.GetUnsafePtr();

                    UnsafeUtility.MemCpy(arrayPtr + elementSize * index, arrayPtr + elementSize * (index + 1),
                        (count - index - 1) * elementSize);
                }
            }

            --count;
        }
        #endregion // UnityEngine.InputSystem.Utilities
    }

    public static class NativeReferenceExtensions
    {
        //https://github.com/Unity-Technologies/Graphics/blob/2ecb711df890ca21a0817cf610ec21c500cb4bfe/Packages/com.unity.render-pipelines.universal/Runtime/UniversalRenderPipelineCore.cs
        #region UnityEngine.Rendering.Universal
        /// <summary>
        /// IMPORTANT: Make sure you do not write to the value! There are no checks for this!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T AsRef<T>(this NativeReference<T> reference) where T : unmanaged
        {
            return ref UnsafeUtility.AsRef<T>(reference.GetUnsafeReadOnlyPtr());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T AsRefMutable<T>(this NativeReference<T> reference) where T : unmanaged
        {
            return ref UnsafeUtility.AsRef<T>(reference.GetUnsafePtr());
        }
        #endregion // UnityEngine.Rendering.Universal
    }
}
