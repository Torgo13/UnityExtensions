using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExtensions
{
    public static class ColorExtensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/MaterialUtils.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Shift the hue of a color by a given amount.
        /// </summary>
        /// <remarks>The hue value wraps around to 0 if the shifted hue exceeds 1.0.</remarks>
        /// <param name="color">The input color.</param>
        /// <param name="shift">The amount of shift.</param>
        /// <returns>The output color.</returns>
        public static Color HueShift(this Color color, float shift)
        {
            Vector3 hsv;
            Color.RGBToHSV(color, out hsv.x, out hsv.y, out hsv.z);
            hsv.x = Mathf.Repeat(hsv.x + shift, 1f);
            return Color.HSVToRGB(hsv.x, hsv.y, hsv.z);
        }
        #endregion // Unity.XR.CoreUtils
        
        //https://github.com/Unity-Technologies/com.unity.probuilder/blob/d09b723d5d286217529e9f34a507015046b2a8a2/Runtime/Core/SelectionPickerRenderer.cs
        #region UnityEngine.ProBuilder
        /// <summary>
        /// Decodes Color32.RGB values to a 32-bit unsigned int, using the RGB as the little bytes.
        /// </summary>
        /// <param name="color">The color to decode.</param>
        /// <returns>32-bit unsigned int containing the decoded RGB values.</returns>
        public static uint DecodeRGBA(this Color32 color)
        {
            uint r = color.r;
            uint g = color.g;
            uint b = color.b;
            uint a = color.a;

            return r << 24 | g << 16 | b << 8 | a;
        }

        /// <summary>
        /// Encodes the low 24 bits of a 32-bit unsigned int to Color32.RGB values.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns>Color32 containing the encoded values.</returns>
        public static Color32 EncodeRGBA(uint hash)
        {
            // skip using BitConverter.GetBytes since this is super simple
            // bit math, and allocating arrays for each conversion is expensive
            return new Color32(
                (byte)(hash >> 24 & 0xFF),
                (byte)(hash >> 16 & 0xFF),
                (byte)(hash >> 8 & 0xFF),
                (byte)(hash & 0xFF));
        }
        #endregion // UnityEngine.ProBuilder
        
        //https://github.com/needle-mirror/com.unity.textmeshpro/blob/75fef8b868509a5496d5d67bd912153a3aa149cc/Scripts/Runtime/TMPro_ExtensionMethods.cs#L106
        #region TMPro
        public static bool Compare(this Color32 a, Color32 b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        }

		public static bool CompareRGB(this Color32 a, Color32 b)
		{
			return a.r == b.r && a.g == b.g && a.b == b.b;
		}

		public static bool Compare(this Color a, Color b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        }


		public static bool CompareRGB(this Color a, Color b)
		{
			return a.r == b.r && a.g == b.g && a.b == b.b;
		}

        public static Color32 Multiply (this Color32 c1, Color32 c2)
        {
            byte r = (byte)((c1.r / 255f) * (c2.r / 255f) * 255);
            byte g = (byte)((c1.g / 255f) * (c2.g / 255f) * 255);
            byte b = (byte)((c1.b / 255f) * (c2.b / 255f) * 255);
            byte a = (byte)((c1.a / 255f) * (c2.a / 255f) * 255);

            return new Color32(r, g, b, a);
        }

        public static Color32 Tint (this Color32 c1, Color32 c2)
        {
            byte r = (byte)((c1.r / 255f) * (c2.r / 255f) * 255);
            byte g = (byte)((c1.g / 255f) * (c2.g / 255f) * 255);
            byte b = (byte)((c1.b / 255f) * (c2.b / 255f) * 255);
            byte a = (byte)((c1.a / 255f) * (c2.a / 255f) * 255);

            return new Color32(r, g, b, a);
        }

        public static Color32 Tint(this Color32 c1, float tint)
        {
            byte r = (byte)(Mathf.Clamp(c1.r / 255f * tint * 255, 0, 255));
            byte g = (byte)(Mathf.Clamp(c1.g / 255f * tint * 255, 0, 255));
            byte b = (byte)(Mathf.Clamp(c1.b / 255f * tint * 255, 0, 255));
            byte a = (byte)(Mathf.Clamp(c1.a / 255f * tint * 255, 0, 255));

            return new Color32(r, g, b, a);
        }

        public static Color32 GammaToLinear(this Color32 c)
        {
            return new Color32(GammaToLinear(c.r), GammaToLinear(c.g), GammaToLinear(c.b), c.a);
        }

        static byte GammaToLinear(byte value)
        {
            float v = value / 255f;

            if (v <= 0.04045f)
                return (byte)(v / 12.92f * 255f);

            if (v < 1.0f)
                return (byte)(Mathf.Pow((v + 0.055f) / 1.055f, 2.4f) * 255);

            if (v == 1.0f)
                return 255;

            return (byte)(Mathf.Pow(v, 2.2f) * 255);
        }

        public static Color MinAlpha(this Color c1, Color c2)
        {
            float a = c1.a < c2.a ? c1.a : c2.a;

            return new Color(c1.r, c1.g, c1.b, a);
        }
        #endregion // TMPro
        
        public static string GetColorHex(this Color32 color)
        {
            System.Span<char> buffer = stackalloc char[2]; // Enough for a byte
            using var _0 = UnityEngine.Pool.StringBuilderPool.Get(out var sb);
            
            color.r.ConvertToHex(buffer, padZeroes: true);
            sb.Append(buffer);
            color.g.ConvertToHex(buffer, padZeroes: true);
            sb.Append(buffer);
            color.b.ConvertToHex(buffer, padZeroes: true);
            sb.Append(buffer);
            color.a.ConvertToHex(buffer, padZeroes: true);
            sb.Append(buffer);
            
            return sb.ToString();
        }

        public static string GetColorTextCode(this Color32 color)
        {
            System.Span<char> buffer = stackalloc char[2]; // Enough for a byte
            using var _0 = UnityEngine.Pool.StringBuilderPool.Get(out var sb);
            sb.Append("<color=#");
            
            color.r.ConvertToHex(buffer, padZeroes: true);
            sb.Append(buffer);
            color.g.ConvertToHex(buffer, padZeroes: true);
            sb.Append(buffer);
            color.b.ConvertToHex(buffer, padZeroes: true);
            sb.Append(buffer);
            color.a.ConvertToHex(buffer, padZeroes: true);
            sb.Append(buffer);
            
            sb.Append('>');
            return sb.ToString();
        }
    }
}
