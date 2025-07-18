using System;
using System.Runtime.CompilerServices;
using System.Text;

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
        /// Removes all occurrences of specified characters from <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="sb">A <see cref="StringBuilder"/> to remove from.</param>
        /// <param name="removeChar">A Unicode character to remove.</param>
        /// <returns>
        /// Returns <see cref="StringBuilder"/> without specified Unicode characters.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="sb"/> is null.</exception>
        public static StringBuilder Remove(this StringBuilder sb, char removeChar)
        {
            if (sb == null)
                throw new ArgumentNullException(nameof(sb));

            for (int i = 0; i < sb.Length;)
            {
                if (removeChar == sb[i])
                    _ = sb.Remove(i, 1);
                else
                    i++;
            }

            return sb;
        }
        
        /// <summary>
        /// Discard StringComparison to keep compatibility with string.Replace
        /// </summary>
        public static StringBuilder Replace(this StringBuilder stringBuilder, string oldValue, string newValue,
#pragma warning disable IDE0060 // Remove unused parameter
            StringComparison comparisonType)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            return stringBuilder.Replace(oldValue, newValue);
        }
        
        public static StringBuilder Replace(this StringBuilder stringBuilder, string oldValue, int newValue,
            bool ignoreCase = false)
        {
            int oldValueLength = oldValue?.Length ?? 0;
            if (oldValueLength == 0)
                return stringBuilder;
            
            int index = stringBuilder.IndexOf(oldValue, ignoreCase);
            while (index != -1)
            {
                _ = stringBuilder.Remove(index, oldValueLength);
                _ = stringBuilder.Insert(index, newValue);
                
                index = stringBuilder.IndexOf(oldValue, ignoreCase);
            }

            return stringBuilder;
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
            return (int)comparisonType % 2 != 0;
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
