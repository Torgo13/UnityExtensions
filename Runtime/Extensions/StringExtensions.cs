using System.Text;

namespace UnityExtensions
{
    /// <summary>
    /// Extension methods for <see cref="string"/> objects.
    /// </summary>
    public static class StringExtensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Extensions/StringExtensions.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Capitalizes the first letter of a string.
        /// </summary>
        /// <param name="str">String to be capitalized.</param>
        /// <returns>The new string.</returns>
        public static string FirstToUpper(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            if (str.Length == 1)
                return char.ToUpper(str[0]).ToString();

            return $"{char.ToUpper(str[0])}{str.Substring(1)}";
        }

        /// <summary>
        /// Inserts spaces into a string between words separated by uppercase letters. Numbers are treated as uppercase.
        /// </summary>
        /// <remarks>
        /// Examples:
        /// * "HelloWorld" -> "Hello World"
        /// * "HelloWORLDAgain" -> "Hello WORLD Again"
        /// </remarks>
        /// <param name="str">Input string.</param>
        /// <returns>Input string with spaces added.</returns>
        public static string InsertSpacesBetweenWords(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            using var _0 = StringBuilderPool.Get(out var k_StringBuilder);
            k_StringBuilder.Append(str[0]);

            var strLength = str.Length;
            for (var i = 0; i < strLength - 1; i++)
            {
                var thisChar = str[i];
                var nextChar = str[i + 1];

                var firstIsLower = char.IsLower(thisChar);
                var secondIsLower = char.IsLower(nextChar);

                // Need a space when lower case followed by upper case eg. aB -> a B
                var needsSpace = firstIsLower && !secondIsLower;

                if (i + 2 < strLength)
                {
                    // Also need space at the beginning of a word after an all-uppercase word eg. ABc -> A Bc
                    var nextNextChar = str[i + 2];
                    var thirdIsLower = char.IsLower(nextNextChar);
                    needsSpace |= !firstIsLower && !secondIsLower && thirdIsLower;
                }

                if (needsSpace)
                    k_StringBuilder.Append(' ');

                k_StringBuilder.Append(nextChar);
            }

            return k_StringBuilder.ToString();
        }
#endregion // Unity.XR.CoreUtils
    }
}
