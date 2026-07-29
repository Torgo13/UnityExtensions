#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace PKGE
{
    public static class StringBuilderExtensions
    {
        //https://github.com/Unity-Technologies/Graphics/blob/274b2c01bdceac862ed35742dcfa90e48e5f3248/Packages/com.unity.shadergraph/Editor/Utilities/StringBuilderExtensions.cs
        #region UnityEditor.ShaderGraph
        public static void AppendIndentedLines(this StringBuilder sb, ReadOnlySpan<char> lines, ReadOnlySpan<char> indentation)
        {
#if INCLUDE_STRINGBUILDER_EXTENSIONS
            System.Text.StringBuilderExtensions.EnsureRoom(sb, lines.Length);
#else
            _ = sb.EnsureCapacity(sb.Length + lines.Length);
#endif // INCLUDE_STRINGBUILDER_EXTENSIONS

            var charIndex = 0;
            while (charIndex < lines.Length)
            {
                var nextNewLineIndex = MemoryExtensions.IndexOf(lines.Slice(charIndex), Environment.NewLine, StringComparison.Ordinal);
                if (nextNewLineIndex == -1)
                {
                    nextNewLineIndex = lines.Length;
                }

                nextNewLineIndex += charIndex;
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

        public static StringBuilder Append(this StringBuilder stringBuilder, ReadOnlySpan<char> value, int startIndex)
        {
            int length = 0;
            (startIndex, length) = value.Length.CalculateLength(startIndex, length);

            return stringBuilder.Append(value.Slice(startIndex, length));
        }
        
        static bool IgnoreCase(StringComparison comparisonType = default)
        {
            return (int)comparisonType % 2 != 0;
        }
    }
}
