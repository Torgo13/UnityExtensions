using System;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityExtensions.Packages
{
    public static class StringBuilderExtensions
    {
        //https://github.com/Unity-Technologies/Graphics/blob/274b2c01bdceac862ed35742dcfa90e48e5f3248/Packages/com.unity.shadergraph/Editor/Utilities/StringBuilderExtensions.cs
        #region UnityEditor.ShaderGraph
        public static void AppendIndentedLines(this StringBuilder sb, string lines, string indentation)
        {
            sb.EnsureRoom(lines.Length);
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
        
        /// <summary>
        /// Removes all occurrences of specified characters from <see cref="System.Text.StringBuilder"/>.
        /// </summary>
        /// <param name="sb">A <see cref="System.Text.StringBuilder"/> to remove from.</param>
        /// <param name="removeChar">A Unicode character to remove.</param>
        /// <returns>
        /// Returns <see cref="System.Text.StringBuilder"/> without specified Unicode characters.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="sb"/> is null.</exception>
        public static StringBuilder Remove(this StringBuilder sb, char removeChar)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            var sbLength = sb.Length;
            for (int i = 0; i < sb.Length;)
            {
                if (removeChar == sb[i])
                    sb.Remove(i, 1);
                else
                    i++;
            }

            return sb;
        }
        
        /// <summary>
        /// Discard StringComparison to keep compatibility with string.Replace
        /// </summary>
        public static StringBuilder Replace(this StringBuilder stringBuilder, string oldValue, string newValue, StringComparison comparisonType)
        {
            return stringBuilder.Replace(oldValue, newValue);
        }

        public static StringBuilder Append(this StringBuilder stringBuilder, string value, int startIndex)
        {
            int length = 0;
            (startIndex, length) = value.Length.CalculateLength(startIndex, length);

            return stringBuilder.Append(value, startIndex, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder EnsureRoom(this StringBuilder stringBuilder, int room)
        {
            _ = stringBuilder.EnsureCapacity(stringBuilder.Length + room);
            return stringBuilder;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IgnoreCase(StringComparison comparisonType = default)
        {
            return UnsafeUtility.EnumToInt(comparisonType) % 2 != 0;
        }

        public static bool Contains(this StringBuilder stringBuilder, string value, bool ignoreCase = false)
        {
            return stringBuilder.IndexOf(value, ignoreCase) >= 0;
        }

        public static bool Contains(this StringBuilder stringBuilder, string value, StringComparison comparisonType)
        {
            return stringBuilder.IndexOf(value, IgnoreCase(comparisonType)) >= 0;
        }

        public static bool Contains(this StringBuilder stringBuilder, char value)
        {
            return stringBuilder.IndexOf(value) >= 0;
        }
    }
}
