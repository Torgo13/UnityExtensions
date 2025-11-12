// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;

namespace PKGE.Unsafe
{
    /// <summary>
    /// Helpers for working with the <see cref="Array"/> type.
    /// </summary>
    public static partial class ArrayExtensions
    {
        //https://github.com/CommunityToolkit/dotnet/blob/657c6971a8d42655c648336b781639ed96c2c49f/src/CommunityToolkit.HighPerformance/Extensions/ArrayExtensions.1D.cs
        #region CommunityToolkit.HighPerformance
        /// <summary>
        /// Returns a reference to an element at a specified index within a given <typeparamref name="T"/> array, with no bounds checks.
        /// </summary>
        /// <typeparam name="T">The type of elements in the input <typeparamref name="T"/> array instance.</typeparam>
        /// <param name="array">The input <typeparamref name="T"/> array instance.</param>
        /// <param name="i">The index of the element to retrieve within <paramref name="array"/>.</param>
        /// <returns>A reference to the element within <paramref name="array"/> at the index specified by <paramref name="i"/>.</returns>
        /// <remarks>This method doesn't do any bounds checks, therefore it is responsibility of the caller to ensure the <paramref name="i"/> parameter is valid.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T DangerousGetReferenceAt<T>(this T[] array, int i)
        {
            return ref array[i];
        }

        /// <summary>
        /// Checks whether or not a given <typeparamref name="T"/> array is covariant.
        /// </summary>
        /// <typeparam name="T">The type of items in the input <typeparamref name="T"/> array instance.</typeparam>
        /// <param name="array">The input <typeparamref name="T"/> array instance.</param>
        /// <returns>Whether or not <paramref name="array"/> is covariant.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCovariant<T>(this T[] array)
        {
            return default(T) is null && array.GetType() != typeof(T[]);
        }
        #endregion // CommunityToolkit.HighPerformance
    }
}
