using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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

        //https://github.com/needle-mirror/com.unity.graphtools.foundation/blob/0.11.2-preview/Editor/GraphElements/Utils/StringUtilsExtensions.cs
        #region UnityEditor.GraphToolsFoundation.Overdrive
        static readonly char NoDelimiter = '\0'; //invalid character

        internal static string ToKebabCase(this string text)
        {
            return ConvertCase(text, '-', char.ToLowerInvariant, char.ToLowerInvariant);
        }

        static readonly char[] k_WordDelimiters = { ' ', '-', '_' };

        static string ConvertCase(string text,
            char outputWordDelimiter,
            Func<char, char> startOfStringCaseHandler,
            Func<char, char> middleStringCaseHandler)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var builder = new StringBuilder();

            bool startOfString = true;
            bool startOfWord = true;
            bool outputDelimiter = true;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (k_WordDelimiters.Contains(c))
                {
                    if (c == outputWordDelimiter)
                    {
                        builder.Append(outputWordDelimiter);
                        //we disable the delimiter insertion
                        outputDelimiter = false;
                    }
                    startOfWord = true;
                }
                else if (!char.IsLetterOrDigit(c))
                {
                    startOfString = true;
                    startOfWord = true;
                }
                else
                {
                    if (startOfWord || char.IsUpper(c))
                    {
                        if (startOfString)
                        {
                            builder.Append(startOfStringCaseHandler(c));
                        }
                        else
                        {
                            if (outputDelimiter && outputWordDelimiter != NoDelimiter)
                            {
                                builder.Append(outputWordDelimiter);
                            }
                            builder.Append(middleStringCaseHandler(c));
                            outputDelimiter = true;
                        }
                        startOfString = false;
                        startOfWord = false;
                    }
                    else
                    {
                        builder.Append(c);
                    }
                }
            }

            return builder.ToString();
        }

        public static string WithUssElement(this string blockName, string elementName) => blockName + "__" + elementName;

        public static string WithUssModifier(this string blockName, string modifier) => blockName + "--" + modifier;
        #endregion // UnityEditor.GraphToolsFoundation.Overdrive

        //https://github.com/needle-mirror/com.unity.entities/blob/1.3.9/Unity.Entities.CodeGen/ListExtensions.cs
        #region Unity.Entities.CodeGen
        public static string SeparateBy(this IEnumerable<string> elements, string delimiter)
        {
            bool first = true;
            using var _0 = StringBuilderPool.Get(out var sb);
            foreach (var e in elements)
            {
                if (!first)
                    sb.Append(delimiter);
                sb.Append(e);
                first = false;
            }

            return sb.ToString();
        }

        public static string SeparateBySpace(this IEnumerable<string> elements) => elements.SeparateBy(" ");
        public static string SeparateByComma(this IEnumerable<string> elements) => elements.SeparateBy(",");
        #endregion // Unity.Entities.CodeGen

        //https://github.com/needle-mirror/com.unity.entities/blob/1.3.9/Unity.Entities.Editor/Extensions/StringExtensions.cs
        #region Unity.Entities.Editor
        static readonly Regex s_ToWordRegex = new Regex(@"[^\w]", RegexOptions.Compiled);
        static readonly Regex s_SplitCaseRegex = new Regex(@"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))");

        public static string SingleQuoted(this string value, bool onlyIfSpaces = false)
        {
            if (onlyIfSpaces && !value.Contains(' '))
                return value;

            return $"'{value.Trim('\'')}'";
        }

        public static string DoubleQuoted(this string value, bool onlyIfSpaces = false)
        {
            if (onlyIfSpaces && !value.Contains(' '))
                return value;

            return $"\"{value.Trim('\"')}\"";
        }

        public static string ToHyperLink(this string value, string key = null)
        {
            return string.IsNullOrEmpty(key) ? $"<a>{value}</a>" : $"<a {key}={value.DoubleQuoted()}>{value}</a>";
        }

        public static string ToIdentifier(this string value)
        {
            return s_ToWordRegex.Replace(value, "_");
        }

        public static string ToForwardSlash(this string value) => value.Replace('\\', '/');

        public static string ReplaceLastOccurrence(this string value, string oldValue, string newValue)
        {
            var index = value.LastIndexOf(oldValue);
            return index >= 0 ? value.Remove(index, oldValue.Length).Insert(index, newValue) : value;
        }

        /// <summary>
        /// Given a pascal case or camel case string this method will add spaces between the capital letters.
        ///
        /// e.g.
        /// "someField"    -> "Some Field"
        /// "layoutWidth"  -> "Layout Width"
        /// "TargetCount"  -> "Target Count"
        /// </summary>
        public static string SplitPascalCase(this string str)
        {
            str = s_SplitCaseRegex.Replace(str, " $1");
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }
        #endregion // Unity.Entities.Editor
    }
}
