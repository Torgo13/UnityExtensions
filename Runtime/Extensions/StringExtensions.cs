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

        public static string ToKebabCase(this string text)
        {
            return ConvertCase(text, '-', char.ToLowerInvariant, char.ToLowerInvariant);
        }

        static readonly char[] k_WordDelimiters = { ' ', '-', '_' };

        public static string ConvertCase(string text,
            char outputWordDelimiter,
            Func<char, char> startOfStringCaseHandler,
            Func<char, char> middleStringCaseHandler)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            using var _0 = StringBuilderPool.Get(out var builder);

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

        //https://github.com/needle-mirror/com.unity.graphtools.foundation/blob/0.11.2-preview/Runtime/Extensions/StringExtensions.cs
        #region UnityEngine.GraphToolsFoundation.Overdrive
        static readonly Regex k_CodifyRegex = new Regex("[^a-zA-Z0-9]", RegexOptions.Compiled);

        public static string CodifyString(this string str)
        {
            return k_CodifyRegex.Replace(str, "_");
        }
        #endregion // UnityEngine.GraphToolsFoundation.Overdrive

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

        //https://github.com/Unity-Technologies/Graphics/blob/95e018183e0f74dc34855606bf3287b41ee6e6ab/Packages/com.unity.render-pipelines.high-definition/Runtime/Core/Debugging/FrameSettingsFieldAttribute.cs
        #region UnityEngine.Rendering.HighDefinition
        /// <summary>Runtime alternative to UnityEditor.ObjectNames.NicifyVariableName. Only prefix 'm_' is not skipped.</summary>
        public static string CamelToPascalCaseWithSpace(this string text, bool preserveAcronyms = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            using var _0 = StringBuilderPool.Get(out var newText);
            newText.Append(char.ToUpper(text[0]));
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }
        #endregion // UnityEngine.Rendering.HighDefinition

        //https://github.com/Unity-Technologies/Graphics/blob/95e018183e0f74dc34855606bf3287b41ee6e6ab/Packages/com.unity.render-pipelines.core/Editor/StringExtensions.cs
        #region UnityEditor.Rendering
        private static readonly Regex k_InvalidRegEx = new(string.Format(@"([{0}]*\.+$)|([{0}]+)",
            Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()))), RegexOptions.Compiled, TimeSpan.FromSeconds(0.1));

        /// <summary>
        /// Replaces invalid characters for a filename or a directory with a given optional replacemenet string
        /// </summary>
        /// <param name="input">The input filename or directory</param>
        /// <param name="replacement">The replacement</param>
        /// <returns>The string with the invalid characters replaced</returns>
        public static string ReplaceInvalidFileNameCharacters(this string input, string replacement = "_") => k_InvalidRegEx.Replace(input, replacement);

        /// <summary>
        /// Checks if the given string ends with the given extension
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="extension">The extension</param>
        /// <returns>True if the extension is found on the string path</returns>
        public static bool HasExtension(this string input, string extension) =>
            input.EndsWith(extension, StringComparison.OrdinalIgnoreCase);


        /// <summary>
        /// Checks if a string contains any of the strings given in strings to check and early out if it does
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="stringsToCheck">List of strings to check</param>
        /// <returns>True if the input contains any of the strings from stringsToCheck; otherwise, false.</returns>
        public static bool ContainsAny(this string input, params string[] stringsToCheck)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            foreach (var value in stringsToCheck)
            {
                if (string.IsNullOrEmpty(value))
                    continue;

                if (input.Contains(value))
                    return true;
            }

            return false;
        }
        #endregion // UnityEditor.Rendering

        //https://github.com/Unity-Technologies/Graphics/blob/95e018183e0f74dc34855606bf3287b41ee6e6ab/Packages/com.unity.shadergraph/Editor/Utilities/StringBuilderExtensions.cs
        #region UnityEditor.ShaderGraph
        public static void AppendIndentedLines(this StringBuilder sb, string lines, string indentation)
        {
            sb.EnsureCapacity(sb.Length + lines.Length);
            var charIndex = 0;
            while (charIndex < lines.Length)
            {
                var nextNewLineIndex = lines.IndexOf(Environment.NewLine, charIndex, StringComparison.Ordinal);
                if (nextNewLineIndex == -1)
                {
                    nextNewLineIndex = lines.Length;
                }

                sb.Append(indentation);

                for (var i = charIndex; i < nextNewLineIndex; i++)
                {
                    sb.Append(lines[i]);
                }

                sb.AppendLine();

                charIndex = nextNewLineIndex + Environment.NewLine.Length;
            }
        }
        #endregion // UnityEditor.ShaderGraph
    }
}
