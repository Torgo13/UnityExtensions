using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExtensions
{
    public static class ArrayExtensions
    {
        public static bool Contains<T>(this T[] array, T item)
        {
            if (array == null)
                return false;

            foreach (var element in array)
            {
                if (element.Equals(item))
                    return true;
            }

            return false;
        }
    }
}
