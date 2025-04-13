using System.Collections.Generic;
using UnityEngine.Pool;

namespace UnityExtensions.Packages
{
    public static class StringExtensions
    {
        public static string Remove(this string str, List<char> removeChars)
        {
            using (StringBuilderPool.Get(out var sb))
            {
                sb.Append(str);

                foreach (char c in removeChars)
                {
                    sb.Remove(c);
                }

                return sb.ToString();
            }
        }
    }
}
