using System;
using TMPro;

namespace UnityExtensions
{
    public static class TextMeshProExtensions
    {
        public static void SetText(this TMP_Text text, ReadOnlySpan<char> arg0)
        {
            using (UnityEngine.Pool.StringBuilderPool.Get(out var sb))
            {
                text.SetText(sb.Append(arg0));
            }
        }

        public static void SetText(this TMP_Text text, Span<char> arg0)
        {
            using (UnityEngine.Pool.StringBuilderPool.Get(out var sb))
            {
                text.SetText(sb.Append(arg0));
            }
        }

        public static void SetTextAsSpan(this TMP_Text text, string arg0)
        {
            if (string.IsNullOrEmpty(arg0))
            {
                text.SetText(string.Empty);
                return;
            }

            text.SetText(arg0.AsSpan());
        }
        
        public static void SetTextAsSpan(this TMP_Text text, string arg0, int start)
        {
            if (string.IsNullOrEmpty(arg0))
            {
                text.SetText(string.Empty);
                return;
            }

            text.SetText(arg0.AsSpan(start, arg0.Length - start));
        }
        
        public static void SetTextAsSpan(this TMP_Text text, string arg0, int start, int length)
        {
            if (string.IsNullOrEmpty(arg0))
            {
                text.SetText(string.Empty);
                return;
            }

            text.SetText(arg0.AsSpan(start, length));
        }
    }
}
