using System;
using TMPro;
using UnityEngine.Pool;
using static Unity.Mathematics.math;

namespace UnityExtensions.Packages
{
    public static class TextMeshProExtensions
    {
        public static void SetText(this TMP_Text text, ReadOnlySpan<char> arg0)
        {
            using (StringBuilderPool.Get(out var sb))
            {
                text.SetText(sb.Append(arg0));
            }
        }

        public static void SetText(this TMP_Text text, Span<char> arg0)
        {
            using (StringBuilderPool.Get(out var sb))
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
        
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/ecad5ff79b1fa55162c23108029609b16e9ffe6d/InternalPackages/com.unity.touch-framework/Runtime/Scripts/InputFieldUtils.cs
        #region Unity.TouchFramework
        public static void AssignText(this TMP_InputField input, int value)
        {
            input.text = value.ToString();
        }

        public static void AssignText(this TMP_InputField input, float value, string format = "F1")
        {
            input.text = value.ToString(format);
        }

        public static bool ValidateRange(this TMP_InputField input, int min, int max, int defaultValue, out int output)
        {
            if (int.TryParse(input.text, out var rawValue))
            {
                var validValue = clamp(rawValue, min, max);
                if (validValue != rawValue)
                    AssignText(input, validValue);

                output = validValue;
                return true;
            }

            output = defaultValue;
            AssignText(input, defaultValue);
            return false;
        }

        public static bool ValidateRange(this TMP_InputField input, float min, float max, float defaultValue, out float output)
        {
            if (float.TryParse(input.text, out var rawValue))
            {
                var validValue = clamp(rawValue, min, max);
                if (validValue < min || validValue > max)
                    AssignText(input, validValue);

                output = validValue;
                return true;
            }

            output = defaultValue;
            AssignText(input, defaultValue);
            return false;
        }
        #endregion // Unity.TouchFramework
    }
}
