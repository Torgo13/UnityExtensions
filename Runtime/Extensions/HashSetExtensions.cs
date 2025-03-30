using System.Collections.Generic;

namespace UnityExtensions
{
    /// <summary>
    /// Extension methods for <see cref="HashSet{T}"/> objects.
    /// </summary>
    public static class HashSetExtensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Extensions/HashSetExtensions.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Remove any elements in this set that are in the set specified by <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// Equivalent to <see cref="HashSet{T}.ExceptWith(IEnumerable{T})"/>, but without any allocation.
        /// </remarks>
        /// <param name="self">The set from which to remove elements.</param>
        /// <param name="other">The set of elements to remove.</param>
        /// <typeparam name="T">The type contained in the set.</typeparam>
        public static void ExceptWithNonAlloc<T>(this HashSet<T> self, HashSet<T> other)
        {
            foreach (var entry in other)
                self.Remove(entry);
        }

        /// <summary>
        /// Gets the first element of a HashSet.
        /// </summary>
        /// <remarks>
        /// Equivalent to the <see cref="System.Linq"/> `.First()` method, but does not allocate.
        /// </remarks>
        /// <param name="set">Set to retrieve the element from</param>
        /// <typeparam name="T">Type contained in the set</typeparam>
        /// <returns>The first element in the set</returns>
        public static T First<T>(this HashSet<T> set)
        {
            var enumerator = set.GetEnumerator();
            var value = enumerator.MoveNext() ? enumerator.Current : default;
            enumerator.Dispose();
            return value;
        }
        #endregion // Unity.XR.CoreUtils
        
        //https://github.com/needle-mirror/com.unity.film-internal-utilities/blob/2cfc425a6f0bf909732b9ca80f2385ea3ff92850/Runtime/Scripts/Extensions/HashSetExtensions.cs
        #region Unity.FilmInternalUtilities
        public static void Loop<T>(this HashSet<T> collection, System.Action<T> eachAction)
        {
            using var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                eachAction(enumerator.Current);
            }
        }
        #endregion // Unity.FilmInternalUtilities
    }
}
