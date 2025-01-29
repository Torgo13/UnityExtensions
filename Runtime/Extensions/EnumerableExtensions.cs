using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityExtensions
{
    public static class EnumerableExtensions
    {
        //https://github.com/needle-mirror/com.unity.purchasing/blob/5.0.0-pre.3/Runtime/Utilities/EnumerableExtensions.cs
        #region UnityEngine.Purchasing
        public static IEnumerable<T> NonNull<T>(this IEnumerable<T> enumerable) where T : class
        {
            return enumerable.Where(obj => obj != null);
        }

#nullable enable
        public static IEnumerable<T> IgnoreExceptions<T, TException>(this IEnumerable<T> enumerable,
            Action<TException>? onException = null) where TException : Exception
        {
            using var enumerator = enumerable.GetEnumerator();

            var hasNext = true;

            while (hasNext)
            {
                try
                {
                    hasNext = enumerator.MoveNext();
                }
                catch (TException ex)
                {
                    onException?.Invoke(ex);
                    continue;
                }

                if (hasNext)
                {
                    yield return enumerator.Current;
                }
            }
        }
#nullable disable
        #endregion // UnityEngine.Purchasing

        //https://github.com/needle-mirror/com.unity.graphtools.foundation/blob/0.11.2-preview/Runtime/Extensions/IEnumerableExtensions.cs
        #region UnityEngine.GraphToolsFoundation.Overdrive
        public static int IndexOf<T>(this IEnumerable<T> source, T element)
        {
            if (source is IList<T> list)
                return list.IndexOf(element);

            int i = 0;
            foreach (var x in source)
            {
                if (Equals(x, element))
                    return i;
                i++;
            }

            return -1;
        }
        #endregion // UnityEngine.GraphToolsFoundation.Overdrive
    }
}
