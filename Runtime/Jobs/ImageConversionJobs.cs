using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

#if INCLUDE_MATHEMATICS
using Unity.Mathematics;
using static Unity.Mathematics.math;
#else
using PKGE.Mathematics;
using static PKGE.Mathematics.math;
using float3 = UnityEngine.Vector3;
using float4 = UnityEngine.Vector4;
#endif // INCLUDE_MATHEMATICS

namespace PKGE.Packages
{
    public static class ImageConversionJobs
    {
        //https://github.com/needle-mirror/com.unity.xr.arcore/blob/595a566141f05d4d0ef96057cae1b474818e046e/Runtime/ImageConversionJobs.cs
        #region UnityEngine.XR.ARCore
        /// <exception cref="System.InvalidOperationException">Texture format is not supported</exception>
        public static JobHandle Schedule(
            NativeSlice<byte> inputImage,
            Vector2Int sizeInPixels,
            TextureFormat format,
            NativeArray<byte> grayscaleImage,
            JobHandle inputDeps)
        {
            int width = sizeInPixels.x;
            int height = sizeInPixels.y;

            if (format == TextureFormat.R8 || format == TextureFormat.Alpha8)
            {
                return new FlipVerticalJob
                {
                    width = width,
                    height = height,
                    grayscaleIn = inputImage,
                    grayscaleOut = grayscaleImage
                }.Schedule(height, 1, inputDeps);
            }

            // We'll have to convert it. Create an output buffer.
            if (format == TextureFormat.RGB24)
            {
                return new ConvertStridedToGrayscaleJob
                {
                    stride = 3,
                    width = width,
                    height = height,
                    colorImageIn = inputImage,
                    grayscaleImageOut = grayscaleImage
                }.Schedule(height, 1, inputDeps);
            }
            else if (format == TextureFormat.RGBA32)
            {
                return new ConvertStridedToGrayscaleJob
                {
                    stride = 4,
                    width = width,
                    height = height,
                    colorImageIn = inputImage,
                    grayscaleImageOut = grayscaleImage
                }.Schedule(height, 1, inputDeps);
            }
            else if (format == TextureFormat.ARGB32)
            {
                return new ConvertARGB32ToGrayscaleJob
                {
                    width = width,
                    height = height,
                    colorImageIn = inputImage,
                    grayscaleImageOut = grayscaleImage
                }.Schedule(height, 1, inputDeps);
            }
            else if (format == TextureFormat.BGRA32)
            {
                return new ConvertBGRA32ToGrayscaleJob
                {
                    width = width,
                    height = height,
                    colorImageIn = inputImage,
                    grayscaleImageOut = grayscaleImage
                }.Schedule(height, 1, inputDeps);
            }
            else if (format == TextureFormat.RFloat)
            {
                return new ConvertRFloatToGrayscaleJob
                {
                    width = width,
                    height = height,
                    rfloatIn = inputImage.SliceConvert<float>(),
                    grayscaleImageOut = grayscaleImage
                }.Schedule(height, 1, inputDeps);
            }
            else
            {
                throw new System.InvalidOperationException($"Texture format {format} is not supported.");
            }
        }
        #endregion // UnityEngine.XR.ARCore

        /// <summary>
        /// Calculate the number of mipmaps a <see cref="Texture2D"/> would have
        /// when created with mipChain as <see langword="true"/>.
        /// </summary>
        [return: Unity.Burst.CompilerServices.AssumeRange(1, sizeof(ushort))]
        public static int MipmapCount(
            [Unity.Burst.CompilerServices.AssumeRange(1, ushort.MaxValue)] int width,
            [Unity.Burst.CompilerServices.AssumeRange(1, ushort.MaxValue)] int height)
        {
            int mipmapCount = 0;
            int s = max(width, height);
            while (s > 1)
            {
                ++mipmapCount;
                s >>= 1;
            }

            //return (int)System.Math.Floor(System.Math.Log(s, 2.0)) + 1;
            //return (int)floor(log2(s)) + 1;

            return mipmapCount;
        }

        /// <summary>
        /// Create a normal map from colour intensity.
        /// </summary>
        /// <remarks>
        /// Must use <see cref="Texture2D.Apply()"/> on <paramref name="normal"/>
        /// after calling <see cref="JobHandle.Complete"/>.
        /// </remarks>
        public static JobHandle CreateNormalMap(this Texture2D texture, out Texture2D normal,
            bool layered = false, bool wrap = false, float normalStrength = 8f, JobHandle handle = default)
        {
            int width = texture.width;
            int height = texture.height;
            normal = new Texture2D(width, height, TextureFormat.RGB24,
                mipChain: layered, linear: true, createUninitialized: true);

            handle = texture.GetPixelData32(out var input, out bool dispose, out int mipmapCount, handle);
            var normalData = normal.GetRawTextureData<Color24>();

            if (!layered)
                mipmapCount = 1;

            var scale = new float3(0.299f, 0.587f, 0.144f);
            for (int i = 0; i < mipmapCount; i++)
            {
                TextureUtils.GetMipData(i, width, height,
                    out int offset, out _, out int mipWidth, out int mipHeight);

                var output = normalData.GetSubArray(offset, mipWidth * mipHeight);
                var inputRO = input.GetSubArray(offset, input.Length - offset).AsReadOnly();

                handle = wrap
                    ? new CreateNormalMapWrapJob
                    {
                        scale = scale,
                        width = mipWidth,
                        hn = mipHeight - 1,
                        normalStrength = normalStrength,// / pow2,
                        input = inputRO,
                        output = output,
                    }.Schedule(output.Length, handle)
                    : new CreateNormalMapJob
                    {
                        scale = scale,
                        width = mipWidth,
                        hn = mipHeight - 1,
                        normalStrength = normalStrength,// / pow2,
                        input = inputRO,
                        output = output,
                    }.Schedule(output.Length, handle);
            }
            
            if (layered)
            {
                handle = new LayerMipsAverageJob
                {
                    rawTextureData = normalData,
                    width = width,
                    height = height,
                    mipmapCount = mipmapCount,
                }.Schedule(width * height, handle);
            }

            if (dispose)
                handle = input.Dispose(handle);

            return handle;
        }

        /// <summary>
        /// <see cref="Texture2D.GetPixels32(int)"/> is used when the input <see cref="Texture2D"/>
        /// is not <see cref="TextureFormat.RGBA32"/>, which allocates a <see cref="Color32"/>[].
        /// </summary>
        public static JobHandle GetPixelData32(this Texture2D texture, out NativeArray<Color32> colour32,
            out bool dispose, out int mipmapCount, JobHandle handle = default)
        {
            mipmapCount = texture.mipmapCount;
            bool convertColour = texture.format != TextureFormat.RGBA32;
            dispose = convertColour || mipmapCount <= 1;
            if (!dispose)
            {
                colour32 = texture.GetRawTextureData<Color32>();
                return default;
            }

            int width = texture.width;
            int height = texture.height;
            int size = width * height;

            mipmapCount = MipmapCount(width, height);
            int length = TextureUtils.MipChainLength(mipmapCount, width, height);
            colour32 = new NativeArray<Color32>(length, Allocator.TempJob,
                NativeArrayOptions.UninitializedMemory);

            if (convertColour)
            {
                Color32[] pixels = texture.GetPixels32(miplevel: 0);
                NativeArray<Color32>.Copy(pixels, srcIndex: 0, colour32, dstIndex: 0, length: size);
            }
            else
            {
                NativeArray<Color32> pixels = texture.GetPixelData<Color32>(mipLevel: 0);
                NativeArray<Color32>.Copy(pixels, srcIndex: 0, colour32, dstIndex: 0, length: size);
            }

            // Width and height of the previous (larger) mip level
            int mipWidthn = width;
            int mipHeightn = height;
            int offset = width * height;
            int offsetn = 0;

            // Create mipmaps
            for (int i = 1; i < mipmapCount; i++)
            {
                int pow2 = 1 << i;
                int mipWidth = width / pow2;
                int mipHeight = height / pow2;

                handle = new CalculateMipsJob
                {
                    rawTextureData = colour32,
                    offset = offset,
                    offsetn = offsetn,
                    mipWidth = mipWidth,
                    mipWidthn = mipWidthn,
                    mipHeightn = mipHeightn,
                }.Schedule(mipWidth * mipHeight, handle);

                mipWidthn = mipWidth;
                mipHeightn = mipHeight;
                offsetn = offset;
                offset += mipWidth * mipHeight;
            }

            return handle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MaxUnlikely(int x, int y)
        {
#if INCLUDE_BURST
            return Unity.Burst.CompilerServices.Hint.Unlikely(x > y) ? x : y;
#else
            return x > y ? x : y;
#endif // INCLUDE_BURST
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MinUnlikely(int x, int y)
        {
#if INCLUDE_BURST
            return Unity.Burst.CompilerServices.Hint.Unlikely(x < y) ? x : y;
#else
            return x < y ? x : y;
#endif // INCLUDE_BURST
        }

        /// <remarks><see href="https://github.com/Unity-Technologies/Graphics/blob/a44ef5e6307acc343ba19066be9f82e08bf14546/Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl#L305"/></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 BlendNormal(in float3 n1, in float3 n2)
        {
#if INCLUDE_MATHEMATICS
            float4 n = mad(n1.xyzz, new float4(2, 2, 2, -2), new float4(-1, -1, -1, 1));
            float3 n0 = mad(n2, 2, -1);
            float3 r;
            r.x = dot(n.zxx, n0.xyz);
            r.y = dot(n.yzy, n0.xyz);
            r.z = dot(n.xyw, -n0.xyz);
#else
            float4 n = mad(new float4(n1.x, n1.y, n1.z, n1.z), new float4(2, 2, 2, -2), new float4(-1, -1, -1, 1));
            float3 n0 = mad(n2, 2, -1);
            float3 r;
            r.x = dot(new float3(n.z, n.x, n.x), n0);
            r.y = dot(new float3(n.y, n.z, n.y), n0);
            r.z = dot(new float3(n.x, n.y, n.w), -n0);
#endif // INCLUDE_MATHEMATICS
            return normalize(r);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color24 NormalMap(in float3 scale, int width, float normalStrength,
            in NativeArray<Color32>.ReadOnly input, int x0, int y0, int xn, int yn, int xp, int yp)
        {
            // Obtain the colors of the eight surrounding pixels
            Color32 c_xn_yn = input[mad(yn, width, xn)];
            Color32 c_x0_yn = input[mad(yn, width, x0)];
            Color32 c_xp_yn = input[mad(yn, width, xp)];

            Color32 c_xn_y0 = input[mad(y0, width, xn)];
            Color32 c_xp_y0 = input[mad(y0, width, xp)];

            Color32 c_xn_yp = input[mad(yp, width, xn)];
            Color32 c_x0_yp = input[mad(yp, width, x0)];
            Color32 c_xp_yp = input[mad(yp, width, xp)];

            // Average the colour values
            float f_xn_yn = dot(c_xn_yn, scale);
            float f_x0_yn = dot(c_x0_yn, scale);
            float f_xp_yn = dot(c_xp_yn, scale);

            float f_xn_y0 = dot(c_xn_y0, scale);
            float f_xp_y0 = dot(c_xp_y0, scale);

            float f_xn_yp = dot(c_xn_yp, scale);
            float f_x0_yp = dot(c_x0_yp, scale);
            float f_xp_yp = dot(c_xp_yp, scale);

            // Calculate the horizontal and vertical gradients
            float edgeX =
                  (f_xn_yn - f_xp_yn) * 0.25f
                + (f_xn_y0 - f_xp_y0) * 0.50f
                + (f_xn_yp - f_xp_yp) * 0.25f;

            float edgeY =
                  (f_xn_yn - f_xn_yp) * 0.25f
                + (f_x0_yn - f_x0_yp) * 0.50f
                + (f_xp_yn - f_xp_yp) * 0.25f;

            // Apply the scale factor
            float normX = edgeX * normalStrength;
            float normY = edgeY * normalStrength;

            // Convert the vector to the normal map colour
            float normDot = normX * normX + normY * normY + 1.0f;
            float normRsqrt = rsqrt(normDot);

            normX *= normRsqrt;
            normY *= normRsqrt;
            normX = mad(normX, 0.5f, 0.5f);
            normY = mad(normY, 0.5f, 0.5f);
            float normZ = mad(normRsqrt, 0.5f, 0.5f);

            return new Color24(normX, normY, normZ);

#pragma warning disable IDE1006 // Naming Styles
            /// <summary>
            /// Take the dot product of a <see cref="Color32"/> and a
            /// <see cref="Unity.Mathematics.float3"/> by normalising the colour byte values.
            /// </summary>
            /// <remarks>
            /// Also multiplies <paramref name="c"/> by its alpha channel
            /// so that transparent pixels are treated as black.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float dot(Color32 c, float3 v)
            {
                const float normalise = 1f / (byte.MaxValue * byte.MaxValue);
                float a = c.a * normalise;
                float x = v.x * c.r * a;
                float y = v.y * c.g * a;
                float z = v.z * c.b * a;
                return x * x + y * y + z * z;
            }
#pragma warning restore IDE1006 // Naming Styles
        }

        public static Color24 GetHigherMipColor(int width, int height, int x, int y, int mip, ref int offset,
            in NativeArray<Color24>.ReadOnly rawTextureData)
        {
            int pow2 = 1 << mip;
            int mipWidth = width / pow2;
            int mipHeight = height / pow2;
            int mipX = MinUnlikely(MaxUnlikely(0, mipWidth - 1), x / pow2);
            int mipY = MinUnlikely(MaxUnlikely(0, mipHeight - 1), y / pow2);

            int mipIndex = mad(mipY, mipWidth, mipX);
            Color24 c = rawTextureData[offset + mipIndex];
            offset += mipWidth * mipHeight;

            return c;
        }

        public static Color24 GetHigherMipColor(int width, int height, int x, int y, int mip,
            in NativeArray<Color24>.ReadOnly rawTextureData)
        {
            TextureUtils.GetMipData(mip, width, height,
                out int offset, out int pow2, out int mipWidth, out int mipHeight);

            int mipX = MinUnlikely(MaxUnlikely(0, mipWidth - 1), x / pow2);
            int mipY = MinUnlikely(MaxUnlikely(0, mipHeight - 1), y / pow2);

            int mipIndex = mad(mipY, mipWidth, mipX);
            return rawTextureData[offset + mipIndex];
        }
    }

    //https://github.com/needle-mirror/com.unity.xr.arcore/blob/595a566141f05d4d0ef96057cae1b474818e046e/Runtime/ImageConversionJobs.cs
    #region UnityEngine.XR.ARCore
    [Unity.Burst.BurstCompile]
    public struct FlipVerticalJob : IJobParallelFor
    {
        public int width;
        public int height;

        [ReadOnly]
        [NativeDisableParallelForRestriction]
        public NativeSlice<byte> grayscaleIn;

        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<byte> grayscaleOut;

        public void Execute(int row)
        {
            int inputOffset = (height - 1 - row) * width;
            int outputOffset = row * width;
            int lastOffset = outputOffset + width;
            while (outputOffset < lastOffset)
            {
                grayscaleOut[outputOffset++] = grayscaleIn[inputOffset++];
            }
        }
    }

    [Unity.Burst.BurstCompile]
    public struct ConvertRFloatToGrayscaleJob : IJobParallelFor
    {
        public int width;
        public int height;

        [ReadOnly]
        public NativeSlice<float> rfloatIn;

        [WriteOnly]
        public NativeArray<byte> grayscaleImageOut;

        public void Execute(int row)
        {
            int inputOffset = (height - 1 - row) * width;
            int outputOffset = row * width;
            int lastOffset = outputOffset + width;
            while (outputOffset < lastOffset)
            {
                grayscaleImageOut[outputOffset++] = (byte)(rfloatIn[inputOffset++] * 255);
            }
        }
    }

    [Unity.Burst.BurstCompile]
    public struct ConvertBGRA32ToGrayscaleJob : IJobParallelFor
    {
        public int width;
        public int height;

        [ReadOnly]
        [NativeDisableParallelForRestriction]
        public NativeSlice<byte> colorImageIn;

        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<byte> grayscaleImageOut;

        public void Execute(int row)
        {
            int colorImageOffset = (height - 1 - row) * width * 4;
            int grayImageOffset = row * width;
            int lastOffset = grayImageOffset + width;
            while (grayImageOffset < lastOffset)
            {
                grayscaleImageOut[grayImageOffset++] = (byte)(
                    colorImageIn[colorImageOffset    ] * 0.11f +
                    colorImageIn[colorImageOffset + 1] * 0.59f +
                    colorImageIn[colorImageOffset + 2] * 0.3f);

                colorImageOffset += 4;
            }
        }
    }

    [Unity.Burst.BurstCompile]
    public struct ConvertARGB32ToGrayscaleJob : IJobParallelFor
    {
        public int width;
        public int height;

        [ReadOnly]
        [NativeDisableParallelForRestriction]
        public NativeSlice<byte> colorImageIn;

        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<byte> grayscaleImageOut;

        public void Execute(int row)
        {
            int colorImageOffset = (height - 1 - row) * width * 4;
            int grayImageOffset = row * width;
            int lastOffset = grayImageOffset + width;
            while (grayImageOffset < lastOffset)
            {
                //ARGB so need to account for ALPHA index.
                grayscaleImageOut[grayImageOffset++] = (byte)(
                    colorImageIn[colorImageOffset + 1] * 0.3f +
                    colorImageIn[colorImageOffset + 2] * 0.59f +
                    colorImageIn[colorImageOffset + 3] * 0.11f);

                colorImageOffset += 4;
            }
        }
    }

    [Unity.Burst.BurstCompile]
    public struct ConvertStridedToGrayscaleJob : IJobParallelFor
    {
        public int stride;
        public int width;
        public int height;

        // NB: NativeDisableParallelForRestriction to allow
        // us to read and write to indices other than the
        // one passed into the Execute method. This is because
        // we interpret the index as the row number and process
        // and entire row at a time. This takes about 75% the
        // time of doing it one pixel at a time.

        [ReadOnly]
        [NativeDisableParallelForRestriction]
        public NativeSlice<byte> colorImageIn;

        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<byte> grayscaleImageOut;

        public void Execute(int row)
        {
            int colorImageOffset = (height - 1 - row) * width * stride;
            int grayImageOffset = row * width;
            int lastOffset = grayImageOffset + width;
            while (grayImageOffset < lastOffset)
            {
                grayscaleImageOut[grayImageOffset++] = (byte)(
                    colorImageIn[colorImageOffset    ] * 0.3f +
                    colorImageIn[colorImageOffset + 1] * 0.59f +
                    colorImageIn[colorImageOffset + 2] * 0.11f);

                colorImageOffset += stride;
            }
        }
    }
    #endregion // UnityEngine.XR.ARCore

    [Unity.Burst.BurstCompile(FloatMode = Unity.Burst.FloatMode.Fast)]
    public struct CalculateMipsJob : IJobFor
    {
        [ReadOnly] public int offset;
        [ReadOnly] public int offsetn;
        [ReadOnly] public int mipWidth;
        [ReadOnly] public int mipWidthn;
        [ReadOnly] public int mipHeightn;

        [NativeDisableParallelForRestriction]
        public NativeArray<Color32> rawTextureData;

        public void Execute(int index)
        {
            int y = System.Math.DivRem(index, mipWidth, out int x);

            // Convert coordinates from source mip level to target mip level
            int wn = mipWidthn - 1;
            int hn = mipHeightn - 1;
            int x0 = min(wn, x + x);
            int y0 = min(hn, y + y);
            int x1 = min(wn, x0 + 1);
            int y1 = min(hn, y0 + 1);

            Color32 tl = rawTextureData[offsetn + mad(y0, mipWidthn, x0)];
            Color32 tr = rawTextureData[offsetn + mad(y0, mipWidthn, x1)];
            Color32 bl = rawTextureData[offsetn + mad(y1, mipWidthn, x0)];
            Color32 br = rawTextureData[offsetn + mad(y1, mipWidthn, x1)];

            byte r = (byte)((tl.r + tr.r + bl.r + br.r) >> 2);
            byte g = (byte)((tl.g + tr.g + bl.g + br.g) >> 2);
            byte b = (byte)((tl.b + tr.b + bl.b + br.b) >> 2);
            byte a = (byte)((tl.a + tr.a + bl.a + br.a) >> 2);

            rawTextureData[offset + index] = new Color32(r, g, b, a);
        }
    }

    [Unity.Burst.BurstCompile(FloatMode = Unity.Burst.FloatMode.Fast)]
    public struct LayerMipsJob : IJobFor
    {
        [ReadOnly] public int width;
        [ReadOnly] public int height;
        [ReadOnly] public int mipmapCount;

        [NativeDisableParallelForRestriction]
        public NativeArray<Color24> rawTextureData;

        public void Execute(int index)
        {
            int y = System.Math.DivRem(index, width, out int x);

            const float scale = 1f / byte.MaxValue;

            Color24 c = rawTextureData[index];
            float3 rgb = new float3(c.r, c.g, c.b) * scale;

            var rawTextureDataRO = rawTextureData.AsReadOnly();

            // TODO Bilinear filtering for higher mip levels
            for (int mip = 1; mip < mipmapCount; mip++)
            {
                c = ImageConversionJobs.GetHigherMipColor(width, height, x, y, mip, rawTextureDataRO);
                rgb = ImageConversionJobs.BlendNormal(rgb, new float3(c.r, c.g, c.b) * scale);
            }

            rawTextureData[index] = new Color24(rgb);
        }
    }

    [Unity.Burst.BurstCompile(FloatMode = Unity.Burst.FloatMode.Fast)]
    public struct LayerMipsAverageJob : IJobFor
    {
        [ReadOnly] public int width;
        [ReadOnly] public int height;
        [ReadOnly] public int mipmapCount;

        [NativeDisableParallelForRestriction]
        public NativeArray<Color24> rawTextureData;

        public void Execute(int index)
        {
            int y = System.Math.DivRem(index, width, out int x);

            Color24 c = rawTextureData[index];
            float3 rgb = new float3(c.r, c.g, c.b);

            var rawTextureDataRO = rawTextureData.AsReadOnly();

            // TODO Bilinear filtering for higher mip levels
            for (int mip = 1; mip < mipmapCount; mip++)
            {
                c = ImageConversionJobs.GetHigherMipColor(width, height, x, y, mip, rawTextureDataRO);
                rgb += new float3(c.r, c.g, c.b);
            }

            rgb *= rcp(mipmapCount * byte.MaxValue);

            rawTextureData[index] = new Color24(rgb);
        }
    }

    [Unity.Burst.BurstCompile(FloatMode = Unity.Burst.FloatMode.Fast)]
    public struct CreateNormalMapJob : IJobFor
    {
        [ReadOnly] public float3 scale;
        [ReadOnly] public int width;
        [ReadOnly] public int hn;
        [ReadOnly] public float normalStrength;
        [ReadOnly] public NativeArray<Color32>.ReadOnly input;
        [WriteOnly] public NativeArray<Color24> output;

        public void Execute(int index)
        {
            // Fix out of bounds array access on image edges
            int wn = width - 1;

            int y0 = System.Math.DivRem(index, width, out int x0);

            int xn = max(0, x0 - 1);
            int yn = max(0, y0 - 1);

            int xp = min(wn, x0 + 1);
            int yp = min(hn, y0 + 1);

            output[index] = ImageConversionJobs.NormalMap(scale, width, normalStrength,
                input, x0, y0, xn, yn, xp, yp);
        }
    }

    [Unity.Burst.BurstCompile(FloatMode = Unity.Burst.FloatMode.Fast)]
    public struct CreateNormalMapWrapJob : IJobFor
    {
        [ReadOnly] public float3 scale;
        [ReadOnly] public int width;
        [ReadOnly] public int hn;
        [ReadOnly] public float normalStrength;
        [ReadOnly] public NativeArray<Color32>.ReadOnly input;
        [WriteOnly] public NativeArray<Color24> output;

        public void Execute(int index)
        {
            // Fix out of bounds array access on image edges
            int wn = width - 1;

            int y0 = System.Math.DivRem(index, width, out int x0);

            int xn = mod(x0 - 1, wn);
            int yn = mod(y0 - 1, hn);

            int xp = mod(x0 + 1, wn);
            int yp = mod(y0 + 1, hn);

            output[index] = ImageConversionJobs.NormalMap(scale, width, normalStrength,
                input, x0, y0, xn, yn, xp, yp);
        }

#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// Integer modulus function to allow texture coordinates
        /// to wrap around to the other side.
        /// </summary>
        /// <remarks>
        /// Does not support integer values that are less than -<paramref name="b"/>.
        /// </remarks>
        /// <param name="a">Input integer value.</param>
        /// <param name="b">Maximum value (i.e. texture width - 1).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int mod(int a, int b)
        {
            return (a + b) % b;
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}
