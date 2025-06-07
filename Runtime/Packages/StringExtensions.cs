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
                foreach (char c in str)
                {
                    if (!removeChars.Contains(c))
                    {
                        _ = sb.Append(c);
                    }
                }

                return sb.ToString();
            }
        }
        
        public static string RemoveEmptyLines(this string text, bool trimEnd = false)
        {
            using var _0 = StringBuilderPool.Get(out var sb);
            bool isPreviousLineEmpty = true;
            foreach (char c in text)
            {
                if (c == '\n' || c == '\r')
                {
                    var sbLength = sb.Length;
                    if (sbLength > 0 && sb[sbLength - 1] != '\n')
                    {
                        sb.Append(c);
                    }

                    isPreviousLineEmpty = true;
                }
                else if (!char.IsWhiteSpace(c) || !isPreviousLineEmpty)
                {
                    sb.Append(c);
                    isPreviousLineEmpty = false;
                }
            }

#if PACKAGE_STRINGBUILDER_EXTENSIONS
            if (trimEnd)
            {
                System.Text.StringBuilderExtensions.TrimEnd(sb);
            }

            return sb.ToString();
#else
            return sb.ToString().TrimEnd();
#endif // PACKAGE_STRINGBUILDER_EXTENSIONS
        }
    }
}
