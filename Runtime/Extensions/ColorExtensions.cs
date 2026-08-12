#nullable enable
using Unity.Collections;
using UnityEngine;

namespace TCGE
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Color24
    {
        public byte r;
        public byte g;
        public byte b;

        #region Properties
#pragma warning disable IDE1006 // Naming Styles
        public readonly byte a
        {
            get => byte.MaxValue;
            set { /**/ }
        }
#pragma warning restore IDE1006 // Naming Styles

        public float R
        {
            readonly get => r * (1f / byte.MaxValue);
            set => r = (byte)(value * byte.MaxValue);
        }

        public float G
        {
            readonly get => g * (1f / byte.MaxValue);
            set => g = (byte)(value * byte.MaxValue);
        }

        public float B
        {
            readonly get => b * (1f / byte.MaxValue);
            set => b = (byte)(value * byte.MaxValue);
        }

        public readonly float A
        {
            get => 1f;
            set { /**/ }
        }
        #endregion // Properties

        #region Constructors
        public Color24(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public Color24(float r, float g, float b)
        {
            this.r = (byte)(r * byte.MaxValue);
            this.g = (byte)(g * byte.MaxValue);
            this.b = (byte)(b * byte.MaxValue);
        }

        public Color24(Vector3 f, float scale = byte.MaxValue)
        {
            this.r = (byte)(f.x * scale);
            this.g = (byte)(f.y * scale);
            this.b = (byte)(f.z * scale);
        }

        public Color24(Vector4 f, float scale = byte.MaxValue)
        {
            this.r = (byte)(f.x * scale);
            this.g = (byte)(f.y * scale);
            this.b = (byte)(f.z * scale);
        }
        #endregion // Constructors

        #region Operators
        public static implicit operator Color24(Vector3 f) => new Color24(f);
        public static implicit operator Vector3(Color24 c) => new Vector3(c.R, c.G, c.B);

        public static implicit operator Color24(Vector4 f) => new Color24(f);
        public static implicit operator Vector4(Color24 c) => new Vector4(c.R, c.G, c.B, 1f);

        public static implicit operator Color24(Color32 c32) => new Color24(c32.r, c32.g, c32.b);
        public static implicit operator Color32(Color24 c24) => new Color32(c24.r, c24.g, c24.b, byte.MaxValue);

        public static implicit operator Color24(Color c) => new Color24(c.r, c.g, c.b);
        public static implicit operator Color(Color24 c24) => new Color32(c24.r, c24.g, c24.b, byte.MaxValue);

        #region System.Drawing
        public static implicit operator Color24(System.Drawing.Color f) => new Color24(f.R, f.G, f.B);
        public static implicit operator System.Drawing.Color(Color24 c) => System.Drawing.Color.FromArgb(c.r, c.g, c.b);
        #endregion // System.Drawing
        #endregion // Operators

#if INCLUDE_MATHEMATICS
        public Color24(Unity.Mathematics.float3 f, float scale = byte.MaxValue)
        {
            this.r = (byte)(f.x * scale);
            this.g = (byte)(f.y * scale);
            this.b = (byte)(f.z * scale);
        }

        public Color24(Unity.Mathematics.float4 f, float scale = byte.MaxValue)
        {
            this.r = (byte)(f.x * scale);
            this.g = (byte)(f.y * scale);
            this.b = (byte)(f.z * scale);
        }

        public static implicit operator Color24(Unity.Mathematics.float3 f) => new Color24(f);
        public static implicit operator Unity.Mathematics.float3(Color24 c) => new Unity.Mathematics.float3(c.R, c.G, c.B);

        public static implicit operator Color24(Unity.Mathematics.float4 f) => new Color24(f.x, f.y, f.z);
        public static implicit operator Unity.Mathematics.float4(Color24 c) => new Unity.Mathematics.float4(c.R, c.G, c.B, 1f);
#endif // INCLUDE_MATHEMATICS
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Color565
    {
        public ushort rgb;

        #region Properties
#pragma warning disable IDE1006 // Naming Styles
        public byte r
        {
            readonly get => (byte)(((rgb >> 8) & 0b1111_1000) | (rgb >> 13));
            set => rgb = (ushort)((rgb & 0b0000_0111_1111_1111) | ((value & 0b1111_1000) << 8));
        }

        public byte g
        {
            readonly get => (byte)(((rgb >> 3) & 0b1111_1100) | ((rgb >> 9) & 0b0000_0011));
            set => rgb = (ushort)((rgb & 0b1111_1000_0001_1111) | ((value & 0b1111_1100) << 3));
        }

        public byte b
        {
            readonly get => (byte)(((rgb << 3) & 0b1111_1000) | ((rgb >> 2) & 0b0000_0111));
            set => rgb = (ushort)((rgb & 0b1111_1111_1110_0000) | ((value & 0b1111_1000) >> 3));
        }

        public readonly byte a
        {
            get => byte.MaxValue;
            set { /**/ }
        }
#pragma warning restore IDE1006 // Naming Styles

        public float R
        {
            readonly get => (rgb >> 11) * (1f / 0b0001_1111);
            set => r = (byte)(value * byte.MaxValue);
        }

        public float G
        {
            readonly get => ((rgb >> 5) & 0b0011_1111) * (1f / 0b0011_1111);
            set => g = (byte)(value * byte.MaxValue);
        }

        public float B
        {
            readonly get => (rgb & 0b0001_1111) * (1f / 0b0001_1111);
            set => b = (byte)(value * byte.MaxValue);
        }

        public readonly float A
        {
            get => 1f;
            set { /**/ }
        }
        #endregion // Properties

        #region Constructors
        public Color565(byte r, byte g, byte b)
        {
            rgb = ColorExtensions.Color32ToUShort(new Color32(r, g, b, 0));
        }

        public Color565(float r, float g, float b)
        {
            rgb = ColorExtensions.Color32ToUShort(new Color(r, g, b));
        }

        public Color565(Vector3 f, float scale = byte.MaxValue)
        {
            f *= scale;
            rgb = ColorExtensions.Color32ToUShort(new Color(f.x, f.y, f.z));
        }

        public Color565(Vector4 f, float scale = byte.MaxValue)
        {
            f *= scale;
            rgb = ColorExtensions.Color32ToUShort(new Color(f.x, f.y, f.z));
        }
        #endregion // Constructors

        #region Operators
        public static implicit operator Color565(Vector3 f) => new Color565(f);
        public static implicit operator Vector3(Color565 c) => new Vector3(c.R, c.G, c.B);

        public static implicit operator Color565(Vector4 f) => new Color565(f);
        public static implicit operator Vector4(Color565 c) => new Vector4(c.R, c.G, c.B, 1f);

        public static implicit operator Color565(Color32 c32) => new Color565(c32.r, c32.g, c32.b);
        public static implicit operator Color32(Color565 c24) => new Color32(c24.r, c24.g, c24.b, byte.MaxValue);

        #region System.Drawing
        public static implicit operator Color565(System.Drawing.Color f) => new Color565(f.R, f.G, f.B);
        public static implicit operator System.Drawing.Color(Color565 c) => System.Drawing.Color.FromArgb(c.r, c.g, c.b);
        #endregion // System.Drawing
        #endregion // Operators

#if INCLUDE_MATHEMATICS
        public Color565(Unity.Mathematics.float3 f, float scale = byte.MaxValue)
        {
            f *= scale;
            rgb = ColorExtensions.Color32ToUShort(new Color(f.x, f.y, f.z));
        }

        public Color565(Unity.Mathematics.float4 f, float scale = byte.MaxValue)
        {
            f *= scale;
            rgb = ColorExtensions.Color32ToUShort(new Color(f.x, f.y, f.z));
        }

        public static implicit operator Color565(Unity.Mathematics.float3 f) => new Color565(f);
        public static implicit operator Unity.Mathematics.float3(Color565 c) => new Unity.Mathematics.float3(c.R, c.G, c.B);

        public static implicit operator Color565(Unity.Mathematics.float4 f) => new Color565(f.x, f.y, f.z);
        public static implicit operator Unity.Mathematics.float4(Color565 c) => new Unity.Mathematics.float4(c.R, c.G, c.B, 1f);
#endif // INCLUDE_MATHEMATICS
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Color332
    {
        public byte rgb;

        #region Properties
#pragma warning disable IDE1006 // Naming Styles
        public byte r
        {
            readonly get => (byte)((rgb & 0b1110_0000) | ((rgb >> 3) & 0b0001_1100) | (rgb >> 6));
            set => rgb = (byte)((rgb & 0b0001_1111) | (value & 0b1110_0000));
        }

        public byte g
        {
            readonly get => (byte)(((rgb << 3) & 0b1110_0000) | (rgb & 0b0001_1100) | ((rgb >> 3) & 0b0000_0011));
            set => rgb = (byte)((rgb & 0b1110_0011) | ((value & 0b1110_0000) >> 3));
        }

        public byte b
        {
            readonly get => (byte)(((rgb << 6) & 0b1100_0000) | ((rgb << 4) & 0b0011_0000) | ((rgb << 2) & 0b0000_1100) | (rgb & 0b0000_0011));
            set => rgb = (byte)((rgb & 0b1111_1100) | ((value & 0b1100_0000) >> 6));
        }

        public readonly byte a
        {
            get => byte.MaxValue;
            set { /**/ }
        }
#pragma warning restore IDE1006 // Naming Styles

        public float R
        {
            readonly get => (rgb >> 5) * (1f / 0b0000_0111);
            set => r = (byte)(value * byte.MaxValue);
        }

        public float G
        {
            readonly get => ((rgb >> 2) & 0b0000_0111) * (1f / 0b0000_0111);
            set => g = (byte)(value * byte.MaxValue);
        }

        public float B
        {
            readonly get => (rgb & 0b0000_0011) * (1f / 0b0000_0011);
            set => b = (byte)(value * byte.MaxValue);
        }

        public readonly float A
        {
            get => 1f;
            set { /**/ }
        }
        #endregion // Properties

        #region Constructors
        public Color332(byte r, byte g, byte b)
        {
            rgb = ColorExtensions.Color32ToByte(new Color32(r, g, b, 0));
        }

        public Color332(float r, float g, float b)
        {
            rgb = ColorExtensions.Color32ToByte(new Color(r, g, b));
        }

        public Color332(Vector3 f, float scale = byte.MaxValue)
        {
            f *= scale;
            rgb = ColorExtensions.Color32ToByte(new Color(f.x, f.y, f.z));
        }

        public Color332(Vector4 f, float scale = byte.MaxValue)
        {
            f *= scale;
            rgb = ColorExtensions.Color32ToByte(new Color(f.x, f.y, f.z));
        }
        #endregion // Constructors

        #region Operators
        public static implicit operator Color332(Vector3 f) => new Color332(f);
        public static implicit operator Vector3(Color332 c) => new Vector3(c.R, c.G, c.B);

        public static implicit operator Color332(Vector4 f) => new Color332(f);
        public static implicit operator Vector4(Color332 c) => new Vector4(c.R, c.G, c.B, 1f);

        public static implicit operator Color332(Color32 c32) => new Color332(c32.r, c32.g, c32.b);
        public static implicit operator Color32(Color332 c24) => new Color32(c24.r, c24.g, c24.b, byte.MaxValue);

        #region System.Drawing
        public static implicit operator Color332(System.Drawing.Color f) => new Color332(f.R, f.G, f.B);
        public static implicit operator System.Drawing.Color(Color332 c) => System.Drawing.Color.FromArgb(c.r, c.g, c.b);
        #endregion // System.Drawing
        #endregion // Operators

#if INCLUDE_MATHEMATICS
        public Color332(Unity.Mathematics.float3 f, float scale = byte.MaxValue)
        {
            f *= scale;
            rgb = ColorExtensions.Color32ToByte(new Color(f.x, f.y, f.z));
        }

        public Color332(Unity.Mathematics.float4 f, float scale = byte.MaxValue)
        {
            f *= scale;
            rgb = ColorExtensions.Color32ToByte(new Color(f.x, f.y, f.z));
        }

        public static implicit operator Color332(Unity.Mathematics.float3 f) => new Color332(f);
        public static implicit operator Unity.Mathematics.float3(Color332 c) => new Unity.Mathematics.float3(c.R, c.G, c.B);

        public static implicit operator Color332(Unity.Mathematics.float4 f) => new Color332(f.x, f.y, f.z);
        public static implicit operator Unity.Mathematics.float4(Color332 c) => new Unity.Mathematics.float4(c.R, c.G, c.B, 1f);
#endif // INCLUDE_MATHEMATICS
    }

    public static class ColorExtensions
    {
        #region System.Drawing
        /// <summary>
        /// Convert <see cref="UnityEngine.Color32"/> to <see cref="System.Drawing.Color"/>.
        /// </summary>
        public static System.Drawing.Color FromColor32(this Color32 colour) => System.Drawing.Color.FromArgb(colour.a, colour.r, colour.g, colour.b);

        /// <summary>
        /// Convert <see cref="System.Drawing.Color"/> to <see cref="UnityEngine.Color32"/>.
        /// </summary>
        public static Color32 ToColor32(this System.Drawing.Color colour) => new Color32(colour.R, colour.G, colour.B, colour.A);

        /// <summary>
        /// Convert <see cref="System.Drawing.KnownColor"/> to <see cref="UnityEngine.Color32"/>.
        /// </summary>
        public static Color32 FromKnownColor(System.Drawing.KnownColor knownColor) => System.Drawing.Color.FromKnownColor(knownColor).ToColor32();

        public const int NamedColorsCount = 1 + (int)System.Drawing.KnownColor.YellowGreen - (int)System.Drawing.KnownColor.AliceBlue;

        /// <summary>
        /// Convert <see cref="System.Drawing.KnownColor"/> index to <see cref="UnityEngine.Color32"/>.
        /// </summary>
        /// <param name="index">Pass 0 for the first valid colour.</param>
        /// <returns>The <see cref="System.Drawing.KnownColor"/> at <paramref name="index"/> converted to a <see cref="UnityEngine.Color32"/>,
        /// or <see cref="Color.magenta"/> if not in range.</returns>
        public static Color32 FromNamedColor(int index)
        {
            if (index < 0 || index >= NamedColorsCount)
                return (Color32)Color.magenta;

            // 1 is added as KnownColor enum has no value for 0
            return FromKnownColor(1 + System.Drawing.KnownColor.AliceBlue + index);
        }

        public static string ColorName(int index)
        {
            if (index < 0 || index >= NamedColorsCount)
                return string.Empty;

            // 1 is added as KnownColor enum has no value for 0
            return System.Drawing.Color.FromKnownColor(1 + System.Drawing.KnownColor.AliceBlue + index).Name;
        }

        /// <summary>
        /// Convert <see cref="System.Drawing.Color"/> to <see cref="System.Drawing.KnownColor"/>.
        /// </summary>
        /// <remarks>
        /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color.toknowncolor?view=netstandard-2.1"/>
        /// A predefined color is also called a known color and is represented by an element of the KnownColor enumeration.
        /// When the ToKnownColor method is applied to a Color structure that is created by using the FromArgb method,
        /// ToKnownColor returns 0, even if the ARGB value matches the ARGB value of a predefined color.
        /// ToKnownColor also returns 0 when it is applied to a Color structure that is created by using the FromName method with a string name that is not valid.
        /// </remarks>
        /// <returns>The equivalent <see cref="System.Drawing.KnownColor"/>, or 0 if no exact match was found.
        /// -1 is not used so that <see cref="System.Drawing.KnownColor"/> can be safely cast to a <see langword="byte"/>.</returns>
        public static System.Drawing.KnownColor ToNamedColor(this System.Drawing.Color color)
        {
            return color.ToKnownColor();
        }

        /// <summary>
        /// Convert <see cref="UnityEngine.Color32"/> to <see cref="System.Drawing.KnownColor"/>.
        /// </summary>
        /// <inheritdoc cref="ToNamedColor(System.Drawing.Color)"/>
        public static System.Drawing.KnownColor ToNamedColor(this Color32 colour)
        {
            return colour.FromColor32().ToKnownColor();
        }

        /// <inheritdoc cref="ToNamedColor(System.Drawing.Color)"/>
        public static System.Drawing.KnownColor ToNamedColorIgnoreAlpha(this System.Drawing.Color color)
        {
            for (var i = System.Drawing.KnownColor.AliceBlue; i <= System.Drawing.KnownColor.YellowGreen; i++)
            {
                System.Drawing.Color namedColor = System.Drawing.Color.FromKnownColor(i);
                if (color.R == namedColor.R && color.G == namedColor.G && color.B == namedColor.B)
                {
                    return i;
                }
            }

            return default;
        }

        /// <inheritdoc cref="ToNamedColor(Color32)"/>
        public static System.Drawing.KnownColor ToNamedColorIgnoreAlpha(this Color32 colour)
        {
            return colour.FromColor32().ToNamedColorIgnoreAlpha();
        }

        /// <inheritdoc cref="ToNamedColor(System.Drawing.Color)"/>
        public static System.Drawing.KnownColor ToNamedColorNearest(this System.Drawing.Color color)
        {
            return ((Color)color.ToColor32()).ToNamedColorNearest();
        }

        /// <inheritdoc cref="ToNamedColor(Color32)"/>
        public static System.Drawing.KnownColor ToNamedColorNearest(this Color colour)
        {
            float min = float.MaxValue;
            System.Drawing.KnownColor minIdx = 0;

            for (var i = System.Drawing.KnownColor.AliceBlue; i <= System.Drawing.KnownColor.YellowGreen; i++)
            {
                float distance = colour.Distance(System.Drawing.Color.FromKnownColor(i).ToColor32());
                if (distance < min)
                {
                    min = distance;
                    minIdx = i;
                }
            }

            return minIdx;
        }
        #endregion // System.Drawing

        #region Colour
        public static float Distance(this Color c0, Color c1)
        {
            Color.RGBToHSV(c0, out float h0, out float s0, out float v0);
            Color.RGBToHSV(c1, out float h1, out float s1, out float v1);

            if (h1 - h0 > 0.5f)
            {
                ++h0;
            }
            else if (h0 - h1 > 0.5f)
            {
                ++h1;
            }

            return Vector3.Distance(
                new Vector3(h0, s0, v0),
                new Vector3(h1, s1, v1));
        }

        /// <summary>
        /// Encodes Color32.RGB values to a byte, using 3:3:2 bits for RGB.
        /// </summary>
        /// <param name="c">The colour to encode.</param>
        /// <returns>Byte containing the encoded RGB values.</returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static byte Color32ToByte(Color32 c)
        {
            int r = c.r & 0b1110_0000;
            int g = c.g & 0b1110_0000;
            int b = c.b & 0b1100_0000;

            return (byte)(r | g >> 3 | b >> 6);
        }

        /// <summary>
        /// Decodes a byte to Color32.RGB values.
        /// </summary>
        /// <param name="b">Byte with RGB colours encoded in 3:3:2 bits.</param>
        /// <returns>Color32 containing the decoded values.</returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Color32 ByteToColor32(byte b)
        {
            return new Color32(
                (byte)(b & 0b1110_0000 | b >> 3 & 0b0001_1100 | b >> 6 & 0b0000_0011),
                (byte)(b << 3 & 0b1110_0000 | b & 0b1110_0000 | b >> 3 & 0b0000_0011),
                (byte)(b << 6 & 0b1100_0000 | b << 4 & 0b0011_0000 | b << 2 & 0b0000_1100 | b & 0b0000_0011),
                byte.MaxValue);
        }

        /// <summary>
        /// Encodes Color.RGB values to a byte, using 3:3:2 bits for RGB.
        /// </summary>
        /// <param name="c">The colour to encode.</param>
        /// <returns>Byte containing the encoded RGB values.</returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static byte ColorToByte(Color c)
        {
            int r = Mathf.RoundToInt(c.r * 0b1110_0000);
            int g = Mathf.RoundToInt(c.g * 0b0001_1100);
            int b = Mathf.RoundToInt(c.b * 0b0000_0011);

            return (byte)(r & 0b1110_0000 | g & 0b0001_1100 | b & 0b0000_0011);
        }

        /// <summary>
        /// Decodes a byte to Color.RGB values.
        /// </summary>
        /// <param name="b">Byte with RGB colours encoded in 3:3:2 bits.</param>
        /// <returns>Color containing the decoded values.</returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Color ByteToColor(byte b)
        {
            return new Color(
                (b & 0b1110_0000) / (float)0b1110_0000,
                (b & 0b0001_1100) / (float)0b0001_1100,
                (b & 0b0000_0011) / (float)0b0000_0011);
        }

        /// <summary>
        /// Encodes Color32.RGB values to a ushort, using 5:6:5 bits for RGB.
        /// </summary>
        /// <param name="c">The colour to encode.</param>
        /// <returns>Short containing the encoded RGB values.</returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ushort Color32ToUShort(Color32 c)
        {
            int r = c.r & 0b1111_1000;
            int g = c.g & 0b1111_1100;
            int b = c.b & 0b1111_1000;

            return (ushort)(r << 8 | g << 3 | b >> 3);
        }

        /// <summary>
        /// Decodes a ushort to Color32.RGB values.
        /// </summary>
        /// <param name="b">Short with RGB colours encoded in 5:6:5 bits.</param>
        /// <returns>Color32 containing the decoded values.</returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Color32 UShortToColor32(ushort b)
        {
            return new Color32(
                (byte)(b >> 8 & 0b1111_1000),
                (byte)(b >> 3 & 0b1111_1100),
                (byte)(b << 3 & 0b1111_1000),
                byte.MaxValue);
        }

        /// <summary>
        /// Encodes Color.RGB values to a byte, using 3:3:2 bits for RGB.
        /// </summary>
        /// <param name="c">The colour to encode.</param>
        /// <returns>Byte containing the encoded RGB values.</returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ushort ColorToUShort(Color c)
        {
            int r = Mathf.RoundToInt(c.r * 0b1111_1000_0000_0000);
            int g = Mathf.RoundToInt(c.g * 0b0000_0111_1110_0000);
            int b = Mathf.RoundToInt(c.b * 0b0000_0000_0001_1111);

            return (ushort)(r & 0b1111_1000_0000_0000 | g & 0b0000_0111_1110_0000 | b & 0b0000_0000_0001_1111);
        }

        /// <summary>
        /// Decodes a byte to Color.RGB values.
        /// </summary>
        /// <param name="b">Byte with RGB colours encoded in 3:3:2 bits.</param>
        /// <returns>Color containing the decoded values.</returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Color UShortToColor(ushort b)
        {
            return new Color(
                (b & 0b1111_1000_0000_0000) / (float)0b1111_1000_0000_0000,
                (b & 0b0000_0111_1110_0000) / (float)0b0000_0111_1110_0000,
                (b & 0b0000_0000_0001_1111) / (float)0b0000_0000_0001_1111);
        }

        public static int Color32ToInt(this Color32 color) => new Union4 { Color32 = color }.Int;
        public static Color32 IntToColor32(this int color) => new Union4 { Int = color }.Color32;

        public static uint Color32ToUInt(this Color32 color) => new Union4 { Color32 = color }.UInt;
        public static Color32 UIntToColor32(this uint color) => new Union4 { UInt = color }.Color32;
        #endregion // Colour

        #region ColourHex
        public static string GetColorHex(this Color32 color)
        {
            System.Span<char> hex = stackalloc char[8];

            return color.GetColorHexSpan(hex).ToString();
        }

        public static string GetColorTextCode(this Color32 color)
        {
            System.Span<char> hex = stackalloc char[17] { '<', 'c', 'o', 'l', 'o', 'r', '=', '#', 'R', 'r', 'G', 'g', 'B', 'b', 'A', 'a', '>' };

            _ = color.GetColorHexSpan(hex.Slice(start: 8, length: 8));

            return hex.ToString();
        }

        public static System.Span<char> GetColorHexSpan(this Color32 color, System.Span<char> hex)
        {
            (hex[0], hex[1]) = ByteToHex(color.r);
            (hex[2], hex[3]) = ByteToHex(color.g);
            (hex[4], hex[5]) = ByteToHex(color.b);
            (hex[6], hex[7]) = ByteToHex(color.a);

            return hex;
        }

        public static (char, char) ByteToHex(int b)
        {
            return (NibbleToHex(b >> 4), NibbleToHex(b & 15));
        }

        public static char NibbleToHex(int nibble)
        {
            return nibble switch
            {
                1 => '1',
                2 => '2',
                3 => '3',
                4 => '4',
                5 => '5',
                6 => '6',
                7 => '7',
                8 => '8',
                9 => '9',
                10 => 'A',
                11 => 'B',
                12 => 'C',
                13 => 'D',
                14 => 'E',
                15 => 'F',
                _ => '0',
            };
        }
        #endregion // ColourHex
    }
}

namespace PKGE
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
            return Mathf.Approximately(a.r, b.r) && Mathf.Approximately(a.g, b.g) && Mathf.Approximately(a.b, b.b) && Mathf.Approximately(a.a, b.a);
        }

		public static bool CompareRGB(this Color a, Color b)
		{
			return Mathf.Approximately(a.r, b.r) && Mathf.Approximately(a.g, b.g) && Mathf.Approximately(a.b, b.b);
		}

        public static Color32 Tint(this Color32 c1, Color32 c2)
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

        public static Color GammaToLinear(this Color c)
        {
            return new Color(GammaToLinear(c.r), GammaToLinear(c.g), GammaToLinear(c.b), c.a);
        }

        static float GammaToLinear(float v)
        {
            if (v <= 0.04045f)
                return v / 12.92f;

            if (v < 1.0f)
                return (float)System.Math.Pow((v + 0.055f) / 1.055f, 2.4f);

            if (Mathf.Approximately(v, 1.0f))
                return 1.0f;

            return (float)System.Math.Pow(v, 2.2f);
        }

        public static Color32 GammaToLinear(this Color32 c)
        {
            return new Color32(GammaToLinear(c.r), GammaToLinear(c.g), GammaToLinear(c.b), c.a);
        }

        static byte GammaToLinear(byte value)
        {
            float v = value / (float)byte.MaxValue;

            if (v <= 0.04045f)
                return (byte)(v / 12.92 * byte.MaxValue);

            if (v < 1.0f)
                return (byte)(System.Math.Pow((v + 0.055) / 1.055, 2.4) * byte.MaxValue);

            if (Mathf.Approximately(v, 1.0f))
                return byte.MaxValue;

            return (byte)(System.Math.Pow(v, 2.2) * byte.MaxValue);
        }

        public static Color MinAlpha(this Color c1, Color c2)
        {
            float a = c1.a < c2.a ? c1.a : c2.a;

            return new Color(c1.r, c1.g, c1.b, a);
        }
        #endregion // TMPro

        //https://github.com/Unity-Technologies/com.unity.search.extensions/blob/main/package-examples/Editor/ImageIndexing/ImageUtils.cs
        #region com.unity.search.extensions
        public static void RGBToXYZ(in Color rgb, out Vector3 xyz)
        {
            Vector3 scaledColors = default;
            for (var i = 0; i < 3; ++i)
            {
                if (rgb[i] > 0.04045f)
                    scaledColors[i] = (float)System.Math.Pow((rgb[i] + 0.055) / 1.055, 2.4);
                else
                    scaledColors[i] = rgb[i] / 12.92f;

                scaledColors[i] *= 100f;
            }

            xyz = new Vector3(
                scaledColors[0] * 0.4124f + scaledColors[1] * 0.3576f + scaledColors[2] * 0.1805f,
                scaledColors[0] * 0.2126f + scaledColors[1] * 0.7152f + scaledColors[2] * 0.0722f,
                scaledColors[0] * 0.0193f + scaledColors[1] * 0.1192f + scaledColors[2] * 0.9505f);
        }

        public static void XYZToCIELab(in Vector3 xyz, out Vector3 lab, in Vector3 reference)
        {
            Vector3 scaledXYZ = default;
            for (var i = 0; i < 3; ++i)
            {
                scaledXYZ[i] = xyz[i] / reference[i];
                if (scaledXYZ[i] > 0.008856f)
                    scaledXYZ[i] = (float)System.Math.Pow(scaledXYZ[i], 1 / 3.0);
                else
                    scaledXYZ[i] = (7.787f * scaledXYZ[i]) + (16f / 116f);
            }

            lab = new Vector3(
                (116f * scaledXYZ[1]) - 16f,
                500f * (scaledXYZ[0] - scaledXYZ[1]),
                200f * (scaledXYZ[1] - scaledXYZ[2]));
        }

        public static void RGBToYUV(in Color rgb, out Vector3 yuv)
        {
            yuv = new Vector3(
                0.299f * rgb.r + 0.587f * rgb.g + 0.114f * rgb.b,
                -0.14713f * rgb.r + -0.28886f * rgb.g + 0.436f * rgb.b,
                0.615f * rgb.r + -0.51499f * rgb.g + -0.10001f * rgb.b);
        }

        public static float DeltaECIE(Vector3 lab1, Vector3 lab2)
        {
            var diffs = new Vector3(lab1[0] - lab2[0], lab1[0] - lab2[0], lab1[0] - lab2[0]);
            return Mathf.Sqrt((diffs[0] * diffs[0]) + (diffs[1] * diffs[1]) + (diffs[2] * diffs[2]));
        }

        public static float DeltaE1994(Vector3 lab1, Vector3 lab2)
        {
            const float WHTL = 1.0f;
            const float WHTC = 1.0f;
            const float WHTH = 1.0f;

            var xC1 = Mathf.Sqrt((lab1[1] * lab1[1]) + (lab1[2] * lab1[2]));
            var xC2 = Mathf.Sqrt((lab2[1] * lab2[1]) + (lab2[2] * lab2[2]));
            var xDL = lab2[0] - lab1[0];
            var xDC = xC2 - xC1;

            var sum = 0f;
            for (var i = 0; i < 3; ++i)
            {
                var diff = lab1[0] - lab2[0];
                sum += diff * diff;
            }

            var xDE = Mathf.Sqrt(sum);

            var xDH = (xDE * xDE) - (xDL * xDL) - (xDC * xDC);
            if (xDH > 0)
            {
                xDH = Mathf.Sqrt(xDH);
            }
            else
            {
                xDH = 0;
            }

            var xSC = 1f + (0.045f * xC1);
            var xSH = 1f + (0.015f * xC1);
            xDL /= WHTL;
            xDC /= WHTC * xSC;
            xDH /= WHTH * xSH;

            return Mathf.Sqrt(xDL * xDL + xDC * xDC + xDH * xDH);
        }

        public static (Color, Color) GetMinMax(Texture2D image)
        {
            return GetMinMax(image.GetPixelData<Color>(mipLevel: 0));
        }

        public static (Color, Color) GetMinMax(NativeArray<Color> pixels)
        {
            var min = new Color(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Color(float.MinValue, float.MinValue, float.MinValue);
            foreach (var pixel in pixels)
            {
                for (var c = 0; c < 3; ++c)
                {
                    if (pixel[c] > max[c])
                        max[c] = pixel[c];

                    if (pixel[c] < min[c])
                        min[c] = pixel[c];
                }
            }

            return (min, max);
        }

        public static (Color32, Color32) GetMinMax32(Texture2D image)
        {
            return GetMinMax32(image.GetPixelData<Color32>(mipLevel: 0));
        }

        public static (Color32, Color32) GetMinMax32(NativeArray<Color32> pixels)
        {
            var min = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
            var max = new Color32(byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue);
            foreach (var pixel in pixels)
            {
                for (var c = 0; c < 3; ++c)
                {
                    if (pixel[c] > max[c])
                        max[c] = pixel[c];

                    if (pixel[c] < min[c])
                        min[c] = pixel[c];
                }
            }

            return (min, max);
        }
        #endregion // com.unity.search.extensions        
    }
}
