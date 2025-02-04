using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Unsafe
{
    public static class ArrayExtensions
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Utilities/ArrayExtensions.cs
        #region UnityEngine.Rendering
        /// <summary>
        /// Fills an array with the same value.
        /// </summary>
        /// <typeparam name="T">The type of the array</typeparam>
        /// <param name="array">Target array to fill</param>
        /// <param name="value">Value to fill</param>
        /// <param name="startIndex">Start index to fill</param>
        /// <param name="length">The number of entries to write, or -1 to fill until the end of the array</param>
        public static void FillArray<T>(this ref NativeArray<T> array, in T value, int startIndex = 0, int length = -1) where T : unmanaged
        {
            Assert.IsTrue(startIndex >= 0);

            unsafe
            {
                T* ptr = (T*)array.GetUnsafePtr<T>();

                int endIndex = length == -1 ? array.Length : startIndex + length;

                for (int i = startIndex; i < endIndex; ++i)
                    ptr[i] = value;
            }
        }
        #endregion // UnityEngine.Rendering
    }
}
