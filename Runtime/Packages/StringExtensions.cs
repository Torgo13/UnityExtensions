using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityExtensions
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
