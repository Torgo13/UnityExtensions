using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityExtensions
{
    public static class EnumerableExtensions
    {
        //https://github.com/Unity-Technologies/UnityCsReference/blob/b1cf2a8251cce56190f455419eaa5513d5c8f609/Editor/Mono/Scripting/ScriptCompilation/EnumerableExtensions.cs
        #region UnityEngine
        public static string SeparateWith(this IEnumerable<string> values, string separator)
        {
            return string.Join(separator, values);
        }

        public static (List<T> True, List<T> False) SplitBy<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            (List<T> True, List<T> False)result = (new List<T>(collection.Count), new List<T>(collection.Count));
            collection.SplitBy(predicate, result.True, result.False);
            return result;
        }
        #endregion // UnityEngine
        
        public static void SplitBy<T>(this ICollection<T> collection, Func<T, bool> predicate,
            List<T> True, List<T> False)
        {
            foreach (var item in collection)
            {
                if (predicate(item))
                    True.Add(item);
                else
                    False.Add(item);
            }
        }
        
        //https://github.com/needle-mirror/com.unity.film-internal-utilities/blob/2cfc425a6f0bf909732b9ca80f2385ea3ff92850/Runtime/Scripts/Extensions/EnumerableExtensions.cs
        #region Unity.FilmInternalUtilities
        //Returns -1 if not found
        public static int FindIndex<T>(this IEnumerable<T> collection, T elementToFind)
        {
            int i = 0;
            using var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                T obj = enumerator.Current;
                if (null != obj && obj.Equals(elementToFind))
                {
                    return i;
                }

                ++i;
            }

            return -1;
        }

        //Returns false with ret set to default(T) if not found
        public static bool FindElementAt<T>(this IEnumerable<T> collection, int index, out T ret)
        {
            int i = 0;
            using var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (i == index)
                {
                    ret = enumerator.Current;
                    return true;
                }

                ++i;
            }

            ret = default(T);
            return false;
        }

        public static void Loop<T>(this IEnumerable<T> collection, Action<T> eachAction)
        {
            using var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                eachAction(enumerator.Current);
            }
        }
        #endregion // Unity.FilmInternalUtilities
        
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
        
        public static double Sum(this double[] enumerable)
        {
            double sum = 0;
            
            foreach (double t in enumerable)
                sum += t;

            return sum;
        }
        
        public static float Sum(this float[] enumerable)
        {
            float sum = 0;
            
            foreach (float t in enumerable)
                sum += t;

            return sum;
        }

        public static int Max(this int[] enumerable)
        {
            int max = int.MaxValue;
            
            foreach (int t in enumerable)
                max = Math.Max(max, t);
            
            return max;
        }
    }
}
