using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Collections;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace PKGE
{
    /// <summary>
    /// Extension methods for <see cref="string"/> objects.
    /// </summary>
    public static class StringExtensions
    {
        //https://github.com/Unity-Technologies/UnityCsReference/blob/b1cf2a8251cce56190f455419eaa5513d5c8f609/Modules/UIBuilder/Editor/Utilities/StringExtensions/StringExtensions.cs
        #region Unity.UI.Builder
        public static string RemoveExtraWhitespace(this string str)
        {
            using var _0 = StringBuilderPool.Get(out var builder);
            var strSpan = str.AsSpan().Trim();

            var space = false;

            for (var i = 0; i < strSpan.Length; ++i)
            {
                var c = strSpan[i];
                if (c == ' ')
                {
                    if (space)
                        continue;

                    space = true;
                    builder.Append(' ');
                }
                else
                {
                    space = false;
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }
        #endregion // Unity.UI.Builder
        
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

            using var _0 = StringBuilderPool.Get(out var sb);
            sb.Append(char.ToUpper(str[0]));
            sb.Append(str.AsSpan(1, str.Length - 1));
            return sb.ToString();
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

            using var _0 = StringBuilderPool.Get(out var stringBuilder);
            stringBuilder.Append(str[0]);

            var strLength = str.Length;
            for (var i = 0; i < strLength - 1; i++)
            {
                var thisChar = str[i];
                var nextChar = str[i + 1];

                var firstIsLower = char.IsLower(thisChar);
                var secondIsLower = char.IsLower(nextChar);

                // Need a space when lower case followed by upper case e.g. aB -> a B
                var needsSpace = firstIsLower && !secondIsLower;

                if (i + 2 < strLength)
                {
                    // Also need space at the beginning of a word after an all-uppercase word e.g. ABc -> A Bc
                    var nextNextChar = str[i + 2];
                    var thirdIsLower = char.IsLower(nextNextChar);
                    needsSpace |= !firstIsLower && !secondIsLower && thirdIsLower;
                }

                if (needsSpace)
                    stringBuilder.Append(' ');

                stringBuilder.Append(nextChar);
            }

            return stringBuilder.ToString();
        }
        #endregion // Unity.XR.CoreUtils

        //https://github.com/needle-mirror/com.unity.xr.arcore/blob/595a566141f05d4d0ef96057cae1b474818e046e/Runtime/StringExtensions.cs
        #region UnityEngine.XR.ARCore
        /// <exception cref="System.ArgumentNullException">@string</exception>
        public static NativeArray<byte> ToBytes(this string @string, Encoding encoding = null, Allocator allocator = Allocator.Temp)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            if (encoding == null)
                encoding = Encoding.Default;

            var byteCount = encoding.GetByteCount(@string);
            var bytes = new NativeArray<byte>(byteCount + 1, allocator);
            _ = encoding.GetBytes(@string, bytes.AsSpan());

            return bytes;
        }
        #endregion // UnityEngine.XR.ARCore
        
        //https://github.com/needle-mirror/com.unity.graphtools.foundation/blob/0.11.2-preview/Editor/GraphElements/Utils/StringUtilsExtensions.cs
        #region UnityEditor.GraphToolsFoundation.Overdrive
        private const char NoDelimiter = '\0'; //invalid character

        public static string ToKebabCase(this string text)
        {
            return ConvertCase(text, '-', char.ToLowerInvariant, char.ToLowerInvariant);
        }

        static readonly char[] WordDelimiters = { ' ', '-', '_' };

        /// <exception cref="ArgumentNullException"></exception>
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
                if (WordDelimiters.Contains(c))
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
        static readonly Regex CodifyRegex = new Regex("[^a-zA-Z0-9]", RegexOptions.Compiled);

        public static string CodifyString(this string str)
        {
            return CodifyRegex.Replace(str, "_");
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

        public static string SeparateBy(this IEnumerable<string> elements, char delimiter)
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

        public static string SeparateBySpace(this IEnumerable<string> elements) => elements.SeparateBy(' ');
        public static string SeparateByComma(this IEnumerable<string> elements) => elements.SeparateBy(',');
        #endregion // Unity.Entities.CodeGen

        //https://github.com/needle-mirror/com.unity.entities/blob/1.3.9/Unity.Entities.Editor/Extensions/StringExtensions.cs
        #region Unity.Entities.Editor
        static readonly Regex ToWordRegex = new Regex(@"[^\w]", RegexOptions.Compiled);
        static readonly Regex SplitCaseRegex = new Regex(@"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))");

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
            return ToWordRegex.Replace(value, "_");
        }

        public static string ToForwardSlash(this string value) => value.Replace('\\', '/');

        public static string ReplaceLastOccurrence(this string value, string oldValue, string newValue)
        {
            var index = value.LastIndexOf(oldValue, StringComparison.CurrentCulture);
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
            str = SplitCaseRegex.Replace(str, " $1");
            return char.ToUpper(str[0]) + str.Substring(1);
        }
        #endregion // Unity.Entities.Editor

        //https://github.com/Unity-Technologies/Graphics/blob/95e018183e0f74dc34855606bf3287b41ee6e6ab/Packages/com.unity.render-pipelines.high-definition/Runtime/Core/Debugging/FrameSettingsFieldAttribute.cs
        #region UnityEngine.Rendering.HighDefinition
        /// <summary>Runtime alternative to UnityEditor.ObjectNames.NicifyVariableName. Only prefix 'm_' is not skipped.</summary>
        public static string CamelToPascalCaseWithSpace(this string text, bool preserveAcronyms = true)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            using var _0 = StringBuilderPool.Get(out var newText);
            newText.Append(char.ToUpper(text[0]));

            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                {
                    if ((text[i - 1] != ' ' && char.IsLetter(text[i - 1]) && !char.IsUpper(text[i - 1]))
                        || (preserveAcronyms && char.IsUpper(text[i - 1])
                                             && i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                    {
                        newText.Append(' ');
                    }
                }

                char previous = text[i - 1];
                newText.Append(previous == ' ' || !char.IsLetterOrDigit(previous) ? char.ToUpper(text[i]) : text[i]);
            }

            return newText.ToString();
        }
        #endregion // UnityEngine.Rendering.HighDefinition
        
        /// <summary>Runtime alternative to UnityEditor.ObjectNames.NicifyVariableName. Only prefix 'm_' is not skipped.</summary>
        public static string CamelToSentenceCaseWithSpace(this string text, bool preserveAcronyms = true)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            using var _0 = StringBuilderPool.Get(out var newText);
            newText.Append(char.ToUpper(text[0]));

            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                {
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1]))
                        || (preserveAcronyms && char.IsUpper(text[i - 1])
                                             && i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                    {
                        newText.Append(' ');
                    }
                }

                newText.Append(text[i]);
            }

            return newText.ToString();
        }

        //https://github.com/Unity-Technologies/Graphics/blob/95e018183e0f74dc34855606bf3287b41ee6e6ab/Packages/com.unity.render-pipelines.core/Editor/StringExtensions.cs
        #region UnityEditor.Rendering
        private static readonly Regex InvalidRegEx = new Regex(string.Format(@"([{0}]*\.+$)|([{0}]+)",
            Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()))), RegexOptions.Compiled, TimeSpan.FromSeconds(0.1));

        /// <summary>
        /// Replaces invalid characters for a filename or a directory with a given optional replacement string
        /// </summary>
        /// <param name="input">The input filename or directory</param>
        /// <param name="replacement">The replacement</param>
        /// <returns>The string with the invalid characters replaced</returns>
        public static string ReplaceInvalidFileNameCharacters(this string input, string replacement = "_") => InvalidRegEx.Replace(input, replacement);

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

        //https://github.com/Unity-Technologies/FPSSample/blob/6b8b27aca3690de9e46ca3fe5780af4f0eff5faa/Assets/Scripts/Utils/StringExtensionMethods.cs
        #region FPSSample
        public static string AfterLast(this string str, string sub)
        {
            var idx = str.LastIndexOf(sub);
            return idx < 0 ? string.Empty : str.Substring(idx + sub.Length);
        }

        public static string BeforeLast(this string str, string sub)
        {
            var idx = str.LastIndexOf(sub);
            return idx < 0 ? string.Empty : str.Substring(0, idx);
        }

        public static string AfterFirst(this string str, string sub)
        {
            var idx = str.IndexOf(sub);
            return idx < 0 ? string.Empty : str.Substring(idx + sub.Length);
        }

        public static string BeforeFirst(this string str, string sub)
        {
            var idx = str.IndexOf(sub);
            return idx < 0 ? string.Empty : str.Substring(0, idx);
        }

        public static int PrefixMatch(this string str, string prefix)
        {
            int l = 0, slen = str.Length, plen = prefix.Length;
            while (l < slen && l < plen)
            {
                if (str[l] != prefix[l])
                    break;

                l++;
            }

            return l;
        }
        #endregion // FPSSample

        //https://docs.unity3d.com/2022.3/Documentation/Manual/UnderstandingPerformanceStringsAndText.html
        #region Unity Documentation
        #region StartsWithOrdinal
        public static bool StartsWithOrdinal(this string a, string b)
        {
            if (b == null)
                throw new ArgumentNullException(nameof(b));

            if (b.Length == 1)
                return a.StartsWithOrdinal(b[0]);

            int aLen = a.Length;
            int bLen = b.Length;

            int ap = 0;
            int bp = 0;

            while (ap < aLen && bp < bLen && a[ap] == b[bp])
            {
                ap++;
                bp++;
            }

            return bp == bLen;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWithOrdinal(this string a, char b)
        {
            return a[0] == b;
        }

        public static bool StartsWithOrdinal(this string a, string b, bool ignoreCase)
        {
            if (b == null)
                throw new ArgumentNullException(nameof(b));

            if (b.Length == 1)
                return a.StartsWithOrdinal(b[0], ignoreCase);

            int aLen = a.Length;
            int bLen = b.Length;

            int ap = 0;
            int bp = 0;

            if (ignoreCase == false)
            {
                while (ap < aLen && bp < bLen && a[ap] == b[bp])
                {
                    ap++;
                    bp++;
                }
            }
            else
            {
                while (ap < aLen && bp < bLen && char.ToLower(a[ap]) == char.ToLower(b[bp]))
                {
                    ap++;
                    bp++;
                }
            }

            return bp == bLen;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWithOrdinal(this string a, char b, bool ignoreCase)
        {
            if (ignoreCase)
                return char.ToLower(a[0]) == char.ToLower(b);

            return a[0] == b;
        }
        #endregion // StartsWithOrdinal
        #region EndsWithOrdinal
        public static bool EndsWithOrdinal(this string a, string b)
        {
            if (b == null)
                throw new ArgumentNullException(nameof(b));

            if (b.Length == 1)
                return a.EndsWithOrdinal(b[0]);

            int ap = a.Length - 1;
            int bp = b.Length - 1;

            while (ap >= 0 && bp >= 0 && a[ap] == b[bp])
            {
                ap--;
                bp--;
            }

            return bp < 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWithOrdinal(this string a, char b)
        {
            return a[^1] == b;
        }

        public static bool EndsWithOrdinal(this string a, string b, bool ignoreCase)
        {
            if (b.Length == 1)
                return a.EndsWithOrdinal(b[0], ignoreCase);

            int ap = a.Length - 1;
            int bp = b.Length - 1;

            if (ignoreCase == false)
            {
                while (ap >= 0 && bp >= 0 && a[ap] == b[bp])
                {
                    ap--;
                    bp--;
                }
            }
            else
            {
                while (ap >= 0 && bp >= 0 && char.ToLower(a[ap]) == char.ToLower(b[bp]))
                {
                    ap--;
                    bp--;
                }
            }

            return bp < 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWithOrdinal(this string a, char b, bool ignoreCase)
        {
            if (ignoreCase)
                return char.ToLower(a[^1]) == char.ToLower(b);

            return a[^1] == b;
        }
        #endregion // EndsWithOrdinal
        #endregion // Unity Documentation
        
        #region ContainsOrdinal
        public static bool ContainsOrdinal(this string str, string value)
        {
            if (value.Length == 1)
                return str.ContainsOrdinal(value[0]);

            return str.IndexOfOrdinal(value) >= 0;
        }

        public static bool ContainsOrdinal(this string str, char value)
        {
            foreach (char c in str)
            {
                if (c == value)
                    return true;
            }

            return false;
        }

        public static bool ContainsOrdinal(this string str, string value, bool ignoreCase)
        {
            if (value.Length == 1)
                return str.ContainsOrdinal(value[0], ignoreCase);

            return str.IndexOfOrdinal(value, ignoreCase) >= 0;
        }

        public static bool ContainsOrdinal(this string str, char value, bool ignoreCase)
        {
            if (ignoreCase == false)
            {
                foreach (char c in str)
                {
                    if (c == value)
                        return true;
                }
            }
            else
            {
                value = char.ToLower(value);

                foreach (char c in str)
                {
                    if (char.ToLower(c) == value)
                        return true;
                }
            }

            return false;
        }
        #endregion // ContainsOrdinal
        #region IndexOfOrdinal
        public static int IndexOfOrdinal(this string str, string value)
        {
            if (value == string.Empty)
                return 0;

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length == 1)
                return str.IndexOfOrdinal(value[0]);

            int num3;
            int length = value.Length;
            int num2 = str.Length - length;

            for (int i = 0; i <= num2; i++)
            {
                if (str[i] == value[0])
                {
                    num3 = 1;
                    while ((num3 < length) && (str[i + num3] == value[num3]))
                    {
                        num3++;
                    }

                    if (num3 == length)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public static int IndexOfOrdinal(this string str, string value, int startIndex)
        {
            return str.IndexOfOrdinal(value, startIndex, str.Length - startIndex);
        }

        public static int IndexOfOrdinal(this string str, string value, int startIndex, int count)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (str.Length != 0 && (startIndex < 0 || startIndex >= str.Length))
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (count < 0 || startIndex + count > str.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (value == string.Empty)
                return startIndex;

            if (value.Length == 1)
                return str.IndexOfOrdinal(value[0]);

            int num3;
            int length = value.Length;
            int num2 = startIndex + count - length;

            for (int i = startIndex; i <= num2; i++)
            {
                if (str[i] == value[0])
                {
                    num3 = 1;
                    while ((num3 < length) && (str[i + num3] == value[num3]))
                    {
                        num3++;
                    }

                    if (num3 == length)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public static int IndexOfOrdinal(this string str, string value, bool ignoreCase)
        {
            if (value.Length == 1)
                return str.IndexOfOrdinal(value[0], ignoreCase);

            int num3;
            int length = value.Length;
            int num2 = str.Length - length;

            if (ignoreCase == false)
            {
                for (int i = 0; i <= num2; i++)
                {
                    if (str[i] == value[0])
                    {
                        num3 = 1;
                        while ((num3 < length) && (str[i + num3] == value[num3]))
                        {
                            num3++;
                        }

                        if (num3 == length)
                        {
                            return i;
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j <= num2; j++)
                {
                    if (char.ToLower(str[j]) == char.ToLower(value[0]))
                    {
                        num3 = 1;
                        while ((num3 < length) && (char.ToLower(str[j + num3]) == char.ToLower(value[num3])))
                        {
                            num3++;
                        }

                        if (num3 == length)
                        {
                            return j;
                        }
                    }
                }
            }

            return -1;
        }

        public static int IndexOfOrdinal(this string str, string value, int startIndex, bool ignoreCase)
        {
            return str.IndexOfOrdinal(value, startIndex, str.Length - startIndex, ignoreCase);
        }

        public static int IndexOfOrdinal(this string str, string value, int startIndex, int count, bool ignoreCase)
        {
            if (value == string.Empty)
                return startIndex;

            if (value.Length == 1)
                return str.IndexOfOrdinal(value[0], ignoreCase);

            int num3;
            int length = value.Length;
            int num2 = startIndex + count - length;

            if (ignoreCase == false)
            {
                for (int i = startIndex; i <= num2; i++)
                {
                    if (str[i] == value[0])
                    {
                        num3 = 1;
                        while ((num3 < length) && (str[i + num3] == value[num3]))
                        {
                            num3++;
                        }

                        if (num3 == length)
                        {
                            return i;
                        }
                    }
                }
            }
            else
            {
                for (int j = startIndex; j <= num2; j++)
                {
                    if (char.ToLower(str[j]) == char.ToLower(value[0]))
                    {
                        num3 = 1;
                        while ((num3 < length) && (char.ToLower(str[j + num3]) == char.ToLower(value[num3])))
                        {
                            num3++;
                        }

                        if (num3 == length)
                        {
                            return j;
                        }
                    }
                }
            }

            return -1;
        }

        public static int IndexOfOrdinal(this string str, char value)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == value)
                    return i;
            }

            return -1;
        }

        public static int IndexOfOrdinal(this string str, char value, int startIndex)
        {
            return str.IndexOfOrdinal(value, startIndex, str.Length - startIndex);
        }

        public static int IndexOfOrdinal(this string str, char value, int startIndex, int count)
        {
            if (str.Length != 0 && (startIndex < 0 || startIndex >= str.Length))
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (count < 0 || startIndex + count > str.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            for (int i = startIndex; i < startIndex + count; i++)
            {
                if (str[i] == value)
                    return i;
            }

            return -1;
        }

        public static int IndexOfOrdinal(this string str, char value, bool ignoreCase)
        {
            if (ignoreCase == false)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] == value)
                        return i;
                }
            }
            else
            {
                value = char.ToLower(value);

                for (int i = 0; i < str.Length; i++)
                {
                    if (char.ToLower(str[i]) == value)
                        return i;
                }
            }

            return -1;
        }
        #endregion // IndexOfOrdinal

        #region LastIndexOfOrdinal
        /// <summary>
        /// Reports the zero-based index of the last occurrence of the specified Unicode
        /// character within this instance.
        /// </summary>
        /// <param name="str">A <see cref="string"/> to search.</param>
        /// <param name="value">A Unicode character to seek.</param>
        /// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false.</param>
        /// <returns>
        /// The zero-based index position of <paramref name="value"/> if that character is found, or -1
        /// if it is not.
        /// </returns>
        public static int LastIndexOfOrdinal(this string str, char value, bool ignoreCase = false)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            return LastIndexOfOrdinal(str, value, str.Length - 1, str.Length, ignoreCase);
        }

        /// <summary>
        /// Reports the zero-based index position of the last occurrence of the specified Unicode character 
        /// in a substring within this instance. The search starts at a specified character position and 
        /// proceeds backward toward the beginning of the <see cref="string"/> 
        /// for a specified number of character positions.
        /// </summary>
        /// <param name="str">A <see cref="string"/> to search.</param>
        /// <param name="value">A Unicode character to seek.</param>
        /// <param name="startIndex">
        /// The starting position of the search. The search proceeds from <paramref name="startIndex"/> toward the beginning 
        /// of this instance.
        /// </param>
        /// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false.</param>
        /// <returns>
        /// The zero-based index position of <paramref name="value"/> if that character is found, or -1
        /// if it is not.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The current instance <see cref="string.Length"/> does not equal 0, 
        /// and <paramref name="startIndex"/> is less than zero or greater than or equal to the length of this instance.
        /// </exception>
        public static int LastIndexOfOrdinal(this string str, char value, int startIndex, bool ignoreCase = false)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));
            if (str.Length != 0 && startIndex < 0)
                throw new ArgumentOutOfRangeException();
            if (str.Length != 0 && startIndex >= str.Length)
                throw new ArgumentOutOfRangeException();

            return str.LastIndexOfOrdinal(value, startIndex, startIndex + 1, ignoreCase);
        }

        /// <summary>
        /// Reports the zero-based index of the last occurrence of the specified Unicode
        /// character in this <see cref="string"/>. The search starts 
        /// at a specified character position and examines a specified number of character positions.
        /// </summary>
        /// <param name="str">A <see cref="string"/> to search.</param>
        /// <param name="value">A Unicode character to seek.</param>
        /// <param name="startIndex">The search starting position.</param>
        /// <param name="count">The number of character positions to examine.</param>
        /// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false.</param>
        /// <returns>
        /// The zero-based index position of <paramref name="value"/> if that character is found, or -1 
        /// if it is not.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The current instance <see cref="string.Length"/> does not equal 0, 
        /// and <paramref name="startIndex"/> is less than zero or greater than or equal to the length of this instance.
        /// -or-The current instance <see cref="string.Length"/> 
        /// does not equal 0, and <paramref name="startIndex"/> - <paramref name="count"/> + 1 is less than zero.
        /// </exception>
        public static int LastIndexOfOrdinal(this string str, char value, int startIndex, int count, bool ignoreCase = false)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));
            if (str.Length != 0 && startIndex < 0)
                throw new ArgumentOutOfRangeException();
            if (str.Length != 0 && startIndex >= str.Length)
                throw new ArgumentOutOfRangeException();
            if (str.Length != 0 && count < 0)
                throw new ArgumentOutOfRangeException();
            if (str.Length != 0 && startIndex - count + 1 < 0)
                throw new ArgumentOutOfRangeException();

            if (str.Length == 0 || count == 0)
                return -1;

            if (ignoreCase)
            {
                value = char.ToLower(value);

                for (int i = startIndex; i > startIndex - count; i--)
                {
                    if (char.ToLower(str[i]) == value)
                    {
                        return i;
                    }
                }
            }
            else
            {
                for (int i = startIndex; i > startIndex - count; i--)
                {
                    if (str[i] == value)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }
        #endregion // LastIndexOfOrdinal

        /// <summary>
        /// Remove all occurrences of char c.
        /// </summary>
        public static string RemoveChar(this string str, char c)
        {
            // If c wasn't found, return the original string
            if (string.IsNullOrEmpty(str) || str.IndexOfOrdinal(c) == -1)
                return str;

            using (StringBuilderPool.Get(out var sb))
            {
                int start = 0;

                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] == c)
                    {
                        // Append the chunk that does not contain the target character
                        if (start < i)
                        {
                            _ = sb.Append(str.AsSpan(start, i - start));
                        }

                        start = i + 1;
                    }
                }

                // Append any remaining part after the last occurrence
                if (start < str.Length)
                {
                    _ = sb.Append(str.AsSpan(start, str.Length - start));
                }

                return sb.ToString();
            }
        }

        public static string RemoveWhiteSpace(this string str)
        {
            // If c wasn't found, return the original string
            if (string.IsNullOrEmpty(str))
                return str;

            using (StringBuilderPool.Get(out var sb))
            {
                int start = 0;

                for (int i = 0; i < str.Length; i++)
                {
                    if (char.IsWhiteSpace(str[i]))
                    {
                        // Append the chunk that does not contain the target character
                        if (start < i)
                        {
                            _ = sb.Append(str.AsSpan(start, i - start));
                        }

                        start = i + 1;
                    }
                }

                // Append any remaining part after the last occurrence
                if (start < str.Length)
                {
                    _ = sb.Append(str.AsSpan(start, str.Length - start));
                }

                return sb.ToString();
            }
        }

        //https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Convert.cs
        #region dotnet
        /// <summary>
        /// Compute the number of bytes encoded in the specified Base 64 char array:
        /// Walk the entire input counting white spaces and padding chars, then compute result length
        /// based on 3 bytes per 4 chars.
        /// </summary>
        /// <param name="input">String to be converted from Base64.</param>
        /// <returns>Length of the byte array needed to allocate.</returns>
        public static int FromBase64_ComputeResultLength(this string input)
        {
            const uint intEq = '=';
            const uint intSpace = ' ';

            Assert.IsNotNull(input);
            Assert.IsTrue(0 <= input.Length);

            int usefulInputLength = input.Length;
            int padding = 0;

            foreach (uint c in input)
            {
                // We want to be as fast as possible and filter out spaces with as few comparisons as possible.
                // We end up accepting a number of illegal chars as legal white-space chars.
                // This is ok: as soon as we hit them during actual decode we will recognise them as illegal and throw.
                if (c <= intSpace)
                    usefulInputLength--;
                else if (c == intEq)
                {
                    usefulInputLength--;
                    padding++;
                }
            }

            Assert.IsTrue(0 <= usefulInputLength);

            // For legal input, we can assume that 0 <= padding < 3. But it may be more for illegal input.
            // We will notice it at decode when we see a '=' at the wrong place.
            Assert.IsTrue(0 <= padding);

            // Perf: reuse the variable that stored the number of '=' to store the number of bytes encoded by the
            // last group that contains the '=':
            if (padding != 0)
            {
                if (padding == 1)
                    padding = 2;
                else if (padding == 2)
                    padding = 1;
                //else
                //throw new FormatException(SR.Format_BadBase64Char);
            }

            // Done:
            return (usefulInputLength / 4) * 3 + padding;
        }
        #endregion // dotnet

        //https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Convert.cs
        #region dotnet
        public static int ToBase64_CalculateAndValidateOutputLength(int inputLength, bool insertLineBreaks)
        {
            const uint Base64LineBreakPosition = 76;

            // the base length - we want integer division here, at most 4 more chars for the remainder
            uint outlen = ((uint)inputLength + 2) / 3 * 4;

            if (outlen == 0)
                return 0;

            if (insertLineBreaks)
            {
                (uint newLines, uint remainder) = DivRem(outlen, Base64LineBreakPosition);
                if (remainder == 0)
                {
                    --newLines;
                }

                outlen += newLines * 2; // the number of line break chars we'll add, "\r\n"
            }

            // If we overflow an int then we cannot allocate enough
            // memory to output the value so throw
            if (outlen > int.MaxValue)
                throw new OutOfMemoryException();

            return (int)outlen;
        }

        /// <summary>Produces the quotient and the remainder of two unsigned 32-bit numbers.</summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>The quotient and the remainder of the specified numbers.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static (uint Quotient, uint Remainder) DivRem(uint left, uint right)
        {
            uint quotient = left / right;
            return (quotient, left - (quotient * right));
        }
        #endregion // dotnet
    }
}

namespace PKGE.Packages
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

#if INCLUDE_STRINGBUILDER_EXTENSIONS
            if (trimEnd)
            {
                System.Text.StringBuilderExtensions.TrimEnd(sb);
            }

            return sb.ToString();
#else
            return sb.ToString().TrimEnd();
#endif // INCLUDE_STRINGBUILDER_EXTENSIONS
        }
    }
}
