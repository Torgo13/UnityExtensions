using UnityEngine;

namespace PKGE
{
    public static class RenderTextureExtensions
    {
        //https://github.com/needle-mirror/com.unity.film-internal-utilities/blob/2cfc425a6f0bf909732b9ca80f2385ea3ff92850/Runtime/Scripts/Extensions/RenderTextureExtensions.cs
        #region Unity.FilmInternalUtilities
        /// <summary>
        /// Clear the depth and the color of a RenderTexture using RGBA(0,0,0,0)
        /// </summary>
        /// <param name="rt">the target RenderTexture</param>
        public static void ClearAll(this RenderTexture rt)
        {
            rt.Clear(clearDepth: true, clearColor: true, Color.clear);
        }

        /// <summary>
        /// Clear a RenderTexture
        /// </summary>
        /// <param name="rt">the target RenderTexture</param>
        /// <param name="clearDepth">Should the depth buffer be cleared? </param>
        /// <param name="clearColor">Should the color buffer be cleared? </param>
        /// <param name="bgColor">The color to clear with, used only if clearColor is true. </param>
        public static void Clear(this RenderTexture rt, bool clearDepth, bool clearColor, Color bgColor)
        {
            RenderTexture prevRT = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(clearDepth, clearColor, bgColor);
            RenderTexture.active = prevRT;
        }
        #endregion // Unity.FilmInternalUtilities
    }
}

namespace TCGE
{
    public static class TextureExtensions
    {
        public static bool Uncompressed(TextureFormat format)
        {
            return format <= TextureFormat.R16
                || (format >= TextureFormat.RGBA4444 && format <= TextureFormat.RGB9e5Float)
                || (format >= TextureFormat.RG16 && format <= TextureFormat.R8)
                || (format >= TextureFormat.RG32 && format <= TextureFormat.RGBA64_SIGNED);
        }

        public static bool Compressed(TextureFormat format)
        {
            return (format >= TextureFormat.DXT1 && format <= TextureFormat.DXT5)
                || (format >= TextureFormat.BC4 && format <= TextureFormat.ASTC_12x12)
                || (format >= TextureFormat.ETC_RGB4Crunched && format <= TextureFormat.ASTC_HDR_12x12);
        }

        public static Unity.Collections.NativeArray<Color32> AsColor32(this Texture2D tex, out bool dispose)
        {
            const Unity.Collections.Allocator allocator = Unity.Collections.Allocator.Persistent;
            const Unity.Collections.NativeArrayOptions options = Unity.Collections.NativeArrayOptions.UninitializedMemory;

            TextureFormat format = tex.format;
            dispose = format != TextureFormat.RGBA32 && format != TextureFormat.ARGB32 && format != TextureFormat.BGRA32;
            Unity.Collections.NativeArray<Color32> rgba32;

            if (format == TextureFormat.Alpha8)
            {
                var a8 = tex.GetPixelData<byte>(mipLevel: 0);
                rgba32 = new Unity.Collections.NativeArray<Color32>(a8.Length, allocator, options);

                for (int i = a8.Length - 1; i >= 0; i--)
                {
                    rgba32[i] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, a8[i]);
                }
            }
            else if (format == TextureFormat.ARGB4444)
            {
                var argb16 = tex.GetPixelData<ushort>(mipLevel: 0);
                rgba32 = new Unity.Collections.NativeArray<Color32>(argb16.Length, allocator, options);

                for (int i = argb16.Length - 1; i >= 0; i--)
                {
                    int a = (argb16[i] & 0b_1111_0000_0000_0000) >> 8;
                    int r = (argb16[i] & 0b_0000_1111_0000_0000) >> 8;
                    int g = argb16[i] & 0b_0000_0000_1111_0000;
                    int b = argb16[i] & 0b_0000_0000_0000_1111;

                    rgba32[i] = new Color32(
                        (byte)(r | (r << 4)),
                        (byte)(g | (g >> 4)),
                        (byte)(b | (b << 4)),
                        (byte)(a | (a >> 4)));
                }
            }
            else if (format == TextureFormat.RGB24)
            {
                var rgb24 = tex.GetPixelData<Color24>(mipLevel: 0);
                rgba32 = new Unity.Collections.NativeArray<Color32>(rgb24.Length, allocator, options);

                for (int i = rgb24.Length - 1; i >= 0; i--)
                {
                    rgba32[i] = (Color32)rgb24[i];
                }
            }
            else if (format == TextureFormat.RGBA32)
            {
                rgba32 = tex.GetPixelData<Color32>(mipLevel: 0);
            }
            else if (format == TextureFormat.ARGB32)
            {
                var argb32 = tex.GetPixelData<Color32>(mipLevel: 0);
                for (int i = argb32.Length - 1; i >= 0; i--)
                {
                    argb32[i] = new Color32(argb32[i].g, argb32[i].b, argb32[i].a, argb32[i].r);
                }
                    
                rgba32 = argb32;
            }
            else if (format == TextureFormat.RGB565)
            {
                var rgb16 = tex.GetPixelData<Color565>(mipLevel: 0);
                rgba32 = new Unity.Collections.NativeArray<Color32>(rgb16.Length, allocator, options);

                for (int i = rgb16.Length - 1; i >= 0; i--)
                {
                    rgba32[i] = (Color32)rgb16[i];
                }
            }
            else if (format == TextureFormat.R16)
            {
                var r16 = tex.GetPixelData<ushort>(mipLevel: 0);
                rgba32 = new Unity.Collections.NativeArray<Color32>(r16.Length, allocator, options);

                for (int i = r16.Length - 1; i >= 0; i--)
                {
                    rgba32[i] = new Color32((byte)(r16[i] >> 8), 0, 0, byte.MaxValue);
                }
            }
            else if (format == TextureFormat.RGBA4444)
            {
                var rgba16 = tex.GetPixelData<ushort>(mipLevel: 0);
                rgba32 = new Unity.Collections.NativeArray<Color32>(rgba16.Length, allocator, options);

                for (int i = rgba16.Length - 1; i >= 0; i--)
                {
                    int r = (rgba16[i] & 0b_1111_0000_0000_0000) >> 8;
                    int g = (rgba16[i] & 0b_0000_1111_0000_0000) >> 8;
                    int b = rgba16[i] & 0b_0000_0000_1111_0000;
                    int a = rgba16[i] & 0b_0000_0000_0000_1111;

                    rgba32[i] = new Color32(
                        (byte)(r | (r << 4)),
                        (byte)(g | (g >> 4)),
                        (byte)(b | (b << 4)),
                        (byte)(a | (a >> 4)));
                }
            }
            else if (format == TextureFormat.BGRA32)
            {
                var bgra32 = tex.GetPixelData<Color32>(mipLevel: 0);
                for (int i = bgra32.Length - 1; i >= 0; i--)
                {
                    bgra32[i] = new Color32(bgra32[i].b, bgra32[i].g, bgra32[i].r, bgra32[i].a);
                }

                rgba32 = bgra32;
            }
#if INCLUDE_MATHEMATICS
            else if (format == TextureFormat.RHalf)
            {
                var rHalf = tex.GetPixelData<TCGE.Mathematics.Union2>(mipLevel: 0);
                rgba32 = new Unity.Collections.NativeArray<Color32>(rHalf.Length, allocator, options);

                for (int i = rHalf.Length - 1; i >= 0; i--)
                {
                    rgba32[i] = new Color(rHalf[i].Half, 0, 0);
                }
            }
            else if (format == TextureFormat.RGHalf)
            {
                var rgHalf = tex.GetPixelData<TCGE.Mathematics.Union4>(mipLevel: 0);
                rgba32 = new Unity.Collections.NativeArray<Color32>(rgHalf.Length, allocator, options);

                for (int i = rgHalf.Length - 1; i >= 0; i--)
                {
                    rgba32[i] = new Color(rgHalf[i]._0.Half, rgHalf[i]._2.Half, 0);
                }
            }
            else if (format == TextureFormat.RGBAHalf)
            {
                var rgbaFloat = tex.GetPixelData<TCGE.Mathematics.Union8>(mipLevel: 0);
                rgba32 = new Unity.Collections.NativeArray<Color32>(rgbaFloat.Length, allocator, options);

                for (int i = rgbaFloat.Length - 1; i >= 0; i--)
                {
                    rgba32[i] = new Color(
                        rgbaFloat[i]._0._0.Half,
                        rgbaFloat[i]._0._2.Half,
                        rgbaFloat[i]._4._0.Half,
                        rgbaFloat[i]._4._2.Half);
                }
            }
#endif // INCLUDE_MATHEMATICS
            else if (format == TextureFormat.RFloat)
            {
                var rFloat = tex.GetPixelData<float>(mipLevel: 0);
                rgba32 = new Unity.Collections.NativeArray<Color32>(rFloat.Length, allocator, options);

                for (int i = rFloat.Length - 1; i >= 0; i--)
                {
                    rgba32[i] = new Color32((byte)(rFloat[i] * byte.MaxValue), 0, 0, byte.MaxValue);
                }
            }
            else if (format == TextureFormat.RGFloat)
            {
                var rgFloat = tex.GetPixelData<Vector2>(mipLevel: 0);
                rgba32 = new Unity.Collections.NativeArray<Color32>(rgFloat.Length, allocator, options);

                for (int i = rgFloat.Length - 1; i >= 0; i--)
                {
                    rgba32[i] = new Color32(
                        (byte)(rgFloat[i].x * byte.MaxValue),
                        (byte)(rgFloat[i].y * byte.MaxValue),
                        0,
                        byte.MaxValue);
                }
            }
            else if (format == TextureFormat.RGBAFloat)
            {
                var rgbaFloat = tex.GetPixelData<Color>(mipLevel: 0);
                rgba32 = new Unity.Collections.NativeArray<Color32>(rgbaFloat.Length, allocator, options);

                for (int i = rgbaFloat.Length - 1; i >= 0; i--)
                {
                    rgba32[i] = (Color32)rgbaFloat[i];
                }
            }
            else if (format == TextureFormat.RG16)
            {
                var rg16 = tex.GetPixelData<Union2>(mipLevel: 0);
                rgba32 = new Unity.Collections.NativeArray<Color32>(rg16.Length, allocator, options);

                for (int i = rg16.Length - 1; i >= 0; i--)
                {
                    rgba32[i] = new Color32(rg16[i]._0.Byte, rg16[i]._1.Byte, 0, byte.MaxValue);
                }
            }
            else if (format == TextureFormat.R8)
            {
                var r8 = tex.GetPixelData<byte>(mipLevel: 0);
                rgba32 = new Unity.Collections.NativeArray<Color32>(r8.Length, allocator, options);

                for (int i = r8.Length - 1; i >= 0; i--)
                {
                    rgba32[i] = new Color32(r8[i], 0, 0, byte.MaxValue);
                }
            }
            else if (format == TextureFormat.RG32)
            {
                var rg32 = tex.GetPixelData<Union4>(mipLevel: 0);
                rgba32 = new Unity.Collections.NativeArray<Color32>(rg32.Length, allocator, options);

                for (int i = rg32.Length - 1; i >= 0; i--)
                {
                    rgba32[i] = new Color32(
                        rg32[i]._0._0.Byte,
                        rg32[i]._2._0.Byte,
                        0,
                        byte.MaxValue);
                }
            }
            else if (format == TextureFormat.RGB48)
            {
                var rgb48 = tex.GetPixelData<Union6>(mipLevel: 0);
                rgba32 = new Unity.Collections.NativeArray<Color32>(rgb48.Length, allocator, options);

                for (int i = rgb48.Length - 1; i >= 0; i--)
                {
                    rgba32[i] = new Color32(
                        (byte)(rgb48[i].U2_0.UShort >> 8),
                        (byte)(rgb48[i].U2_2.UShort >> 8),
                        (byte)(rgb48[i].U2_4.UShort >> 8),
                        byte.MaxValue);
                }
            }
            else if (format == TextureFormat.RGBA64)
            {
                var rgba64 = tex.GetPixelData<Union8>(mipLevel: 0);
                rgba32 = new Unity.Collections.NativeArray<Color32>(rgba64.Length, allocator, options);

                for (int i = rgba64.Length - 1; i >= 0; i--)
                {
                    rgba32[i] = new Color32(
                        rgba64[i]._0._0._0.Byte,
                        rgba64[i]._0._2._0.Byte,
                        rgba64[i]._4._0._0.Byte,
                        rgba64[i]._4._2._0.Byte);
                }
            }
            else // TODO Handle signed 8-bit and 16-bit formats
            {
                rgba32 = new Unity.Collections.NativeArray<Color32>(tex.GetPixels32(miplevel: 0), allocator);
            }

            return rgba32;
        }
    }
}
