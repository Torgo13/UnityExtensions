using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace PKGE
{
    /// <summary>
    /// A set of extension methods for collections
    /// </summary>
    public static class SwapCollectionExtensions
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Common/Swap.Extensions.cs
        #region UnityEngine.Rendering
        /// <summary>
        /// Tries to remove a range of elements from the list in the given range.
        /// </summary>
        /// <param name="list">The list to remove the range</param>
        /// <param name="from">From index</param>
        /// <param name="to">To index</param>
        /// <param name="error">The exception raised by the implementation</param>
        /// <typeparam name="TValue">The value type stored on the list</typeparam>
        /// <returns>True if it succeeded, false otherwise</returns>
        [CollectionAccess(CollectionAccessType.ModifyExistingContent)]
        [MustUseReturnValue]
        public static bool TrySwap<TValue>([DisallowNull] this IList<TValue> list, int from, int to,
            [NotNullWhen(false)] out Exception error)
        {
            error = null;
            if (list == null)
            {
                error = new ArgumentNullException(nameof(list));
            }
            else
            {
                if (from < 0 || from >= list.Count)
                    error = new ArgumentOutOfRangeException(nameof(from));
                if (to < 0 || to >= list.Count)
                    error = new ArgumentOutOfRangeException(nameof(to));
            }

            if (error != null)
                return false;

            // https://tearth.dev/posts/performance-of-the-different-ways-to-swap-two-values/
            (list[to], list[from]) = (list[from], list[to]);
            return true;
        }
        #endregion // UnityEngine.Rendering
    }
}
