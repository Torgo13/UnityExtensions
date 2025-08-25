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
#if PACKAGE_STRINGBUILDER_EXTENSIONS
            System.Text.StringBuilderExtensions.EnsureRoom(sb, lines.Length);
#endif // PACKAGE_STRINGBUILDER_EXTENSIONS

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

        public static StringBuilder Append(this StringBuilder stringBuilder, string value, int startIndex)
        {
            int length = 0;
            (startIndex, length) = value.Length.CalculateLength(startIndex, length);

            return stringBuilder.Append(value, startIndex, length);
        }
        
        static bool IgnoreCase(StringComparison comparisonType = default)
        {
            return (int)comparisonType % 2 != 0;
        }
    }
}
