using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace UnityExtensions.Packages
{
    public static class ImageConversionJobs
    {
        //https://github.com/needle-mirror/com.unity.xr.arcore/blob/595a566141f05d4d0ef96057cae1b474818e046e/Runtime/ImageConversionJobs.cs
        #region UnityEngine.XR.ARCore
        /// <exception cref="System.InvalidOperationException">Texture format is not supported</exception>
        [BurstCompile]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMipOffset(int mipLevel, int width, int height)
        {
            int offset = 0;
            for (int i = 0; i < mipLevel; i++)
            {
                int pow2 = 1 << i;
                int mipWidth = width / pow2;
                int mipHeight = height / pow2;
                offset += mipWidth * mipHeight;
            }

            return offset;
        }

        /// <summary>
        /// Create a normal map from colour intensity.
        /// </summary>
        /// <remarks>
        /// Must use <see cref="Texture2D.Apply()"/> on <paramref name="normal"/>
        /// after calling <see cref="JobHandle.Complete"/>.
        /// </remarks>
        public static JobHandle CreateNormalMap(this Texture2D texture,
            out Texture2D normal, float normalStrength = 8f, JobHandle handle = default)
        {
            int width = texture.width;
            int height = texture.height;
            normal = new Texture2D(width, height, TextureFormat.RGB24,
                mipChain: false, linear: true, createUninitialized: true);

            handle = texture.GetPixelData32(out var input, out bool dispose, out _, handle);

            const float third = 1f / 3f;
            handle = new CreateNormalMapJob
            {
                scale = new Vector3(third, third, third),
                width = width,
                hn = height - 1,
                normalStrength = normalStrength,
                wrap = false,
                input = input,
                output = normal.GetPixelData<Color24>(mipLevel: 0),
            }.Schedule(width * height, handle);

            if (dispose)
                handle = new DisposeArrayJob<Color32> { Data = input, }.Schedule(handle);

            return handle;
        }

        /// <inheritdoc cref="CreateNormalMap"/>
        public static JobHandle CreateLayeredNormalmap(this Texture2D texture,
            out Texture2D normal, float normalStrength = 8f, JobHandle handle = default)
        {
            int width = texture.width;
            int height = texture.height;
            normal = new Texture2D(width, height, TextureFormat.RGB24,
                mipChain: true, linear: true, createUninitialized: true);

            handle = texture.GetPixelData32(out var input, out bool dispose, out int mipmapCount, handle);

            const float third = 1f / 3f;
            var scale = new Vector3(third, third, third);
            for (int i = 0; i < mipmapCount - 1; i++)
            {
                int pow2 = 1 << i;
                int mipWidth = width / pow2;
                int mipHeight = height / pow2;
                var output = normal.GetPixelData<Color24>(mipLevel: i);

                handle = new CreateNormalMapJob
                {
                    scale = scale,
                    width = mipWidth,
                    hn = mipHeight - 1,
                    offset = GetMipOffset(i, width, height),
                    normalStrength = normalStrength,// / pow2,
                    wrap = false,
                    input = input,
                    output = output,
                }.Schedule(output.Length, handle);
            }

            handle = new LayerMipsJob
            {
                rawTextureData = normal.GetRawTextureData<Color24>(),
                width = width,
                height = height,
                mipmapCountn = mipmapCount - 3,
            }.Schedule(width * height, handle);

            if (dispose)
                handle = new DisposeArrayJob<Color32> { Data = input, }.Schedule(handle);

            return handle;
        }

        /// <summary>
        /// Must use GetPixels32 when the texture is compressed.
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

            // Calculate number of mipmaps
            int s = min(width, height);
            mipmapCount = 0;
            while (s > 0)
            {
                ++mipmapCount;
                s /= 2;
            }

            // Calculate number pixels including mipmaps
            int length = 0;
            for (int i = 0; i < mipmapCount; i++)
            {
                int pow2 = 1 << i;
                length += size / (pow2 * pow2);
            }

            colour32 = new NativeArray<Color32>(length, Allocator.TempJob,
                NativeArrayOptions.UninitializedMemory);

            Color32[] pixels = texture.GetPixels32(miplevel: 0);
            NativeArray<Color32>.Copy(pixels, srcIndex: 0, colour32, dstIndex: 0, length: size);

            // Create mipmaps
            for (int i = 1; i < mipmapCount; i++)
            {
                int pow2 = 1 << i;
                int mipWidth = width / pow2;
                int mipHeight = height / pow2;

                handle = new CalculateMipsJob
                {
                    rawTextureData = colour32,
                    offsetn = GetMipOffset(i - 1, width, height),
                    offset = GetMipOffset(i, width, height),
                    mipWidth = mipWidth,
                }.ScheduleParallel(mipWidth * mipHeight,
                    innerloopBatchCount: 32, handle);
            }

            return handle;
        }
    }

    //https://github.com/needle-mirror/com.unity.xr.arcore/blob/595a566141f05d4d0ef96057cae1b474818e046e/Runtime/ImageConversionJobs.cs
    #region UnityEngine.XR.ARCore
    [BurstCompile]
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

    [BurstCompile]
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

    [BurstCompile]
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

    [BurstCompile]
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

    [BurstCompile]
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

    [BurstCompile(FloatMode = FloatMode.Fast)]
    public struct CalculateMipsJob : IJobFor
    {
        [ReadOnly] public int offsetn;
        [ReadOnly] public int offset;
        [ReadOnly] public int mipWidth;

        [NativeDisableParallelForRestriction]
        public NativeArray<Color32> rawTextureData;

        public void Execute(int index)
        {
            int x = index % mipWidth;
            int y = index / mipWidth;

            // Convert coordinates from source mip level to target mip level
            int tgtX = x + x;
            int tgtY = y + y;
            int tgtWidth = mipWidth + mipWidth;

            int tlIdx = offsetn + tgtY * tgtWidth + tgtX;
            int blIdx = offsetn + (tgtY + 1) * tgtWidth + tgtX;
            var tl = rawTextureData[tlIdx];
            var tr = rawTextureData[tlIdx + 1];
            var bl = rawTextureData[blIdx];
            var br = rawTextureData[blIdx + 1];

            float r = (tl.r + tr.r + bl.r + br.r) * 0.25f;
            float g = (tl.g + tr.g + bl.g + br.g) * 0.25f;
            float b = (tl.b + tr.b + bl.b + br.b) * 0.25f;
            float a = (tl.a + tr.a + bl.a + br.a) * 0.25f;

            rawTextureData[offset + index] = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
        }
    }

    [BurstCompile(FloatMode = FloatMode.Fast)]
    public struct LayerMipsJob : IJobFor
    {
        [ReadOnly] public int width;
        [ReadOnly] public int height;
        [ReadOnly] public int mipmapCountn;

        [NativeDisableParallelForRestriction]
        public NativeArray<Color24> rawTextureData;

        public void Execute(int index)
        {
            int x = index % width;
            int y = index / width;

            Color24 c = rawTextureData[index];
            float r = c.r;
            float g = c.g;
            float b = c.b;

            // TODO Bilinear filtering for higher mip levels
            for (int mip = 1; mip < mipmapCountn; mip++)
            {
                int pow2 = 1 << mip;
                int mipX = x / pow2;
                int mipY = y / pow2;
                int mipWidth = width / pow2;

                int offset = ImageConversionJobs.GetMipOffset(mip, width, height);
                int mipIndex = mad(mipY, mipWidth, mipX);
                c = rawTextureData[offset + mipIndex];

                r += c.r;
                g += c.g;
                b += c.b;
            }

            float scale = mipmapCountn * (float)byte.MaxValue;
            r /= scale;
            g /= scale;
            b /= scale;

            rawTextureData[index] = new Color24(r, g, b);
        }
    }

    [BurstCompile(FloatMode = FloatMode.Fast)]
    public struct CreateNormalMapJob : IJobFor
    {
        [ReadOnly] public float3 scale;
        [ReadOnly] public int width;
        [ReadOnly] public int hn;
        [ReadOnly] public int offset;
        [ReadOnly] public float normalStrength;
        [ReadOnly] public bool wrap;
        [ReadOnly] public NativeArray<Color32> input;
        [WriteOnly] public NativeArray<Color24> output;

        public void Execute(int index)
        {
            // Fix out of bounds array access on image edges
            int wn = width - 1;

            int x0 = index % width;
            int y0 = index / width;

            int xn = wrap ? mod(x0 - 1, wn) : max(0, x0 - 1);
            int yn = wrap ? mod(y0 - 1, hn) : max(0, y0 - 1);

            int xp = wrap ? mod(x0 + 1, wn) : min(wn, x0 + 1);
            int yp = wrap ? mod(y0 + 1, hn) : min(hn, y0 + 1);

            Color32 c_xn_yn = input[offset + mad(yn, width, xn)];
            Color32 c_x0_yn = input[offset + mad(yn, width, x0)];
            Color32 c_xp_yn = input[offset + mad(yn, width, xp)];

            Color32 c_xn_y0 = input[offset + mad(y0, width, xn)];
            Color32 c_xp_y0 = input[offset + mad(y0, width, xp)];

            Color32 c_xn_yp = input[offset + mad(yp, width, xn)];
            Color32 c_x0_yp = input[offset + mad(yp, width, x0)];
            Color32 c_xp_yp = input[offset + mad(yp, width, xp)];

            float f_xn_yn = dot(c_xn_yn, scale);
            float f_x0_yn = dot(c_x0_yn, scale);
            float f_xp_yn = dot(c_xp_yn, scale);

            float f_xn_y0 = dot(c_xn_y0, scale);
            float f_xp_y0 = dot(c_xp_y0, scale);

            float f_xn_yp = dot(c_xn_yp, scale);
            float f_x0_yp = dot(c_x0_yp, scale);
            float f_xp_yp = dot(c_xp_yp, scale);

            float edgeX =
                  (f_xn_yn - f_xp_yn) * 0.25f
                + (f_xn_y0 - f_xp_y0) * 0.50f
                + (f_xn_yp - f_xp_yp) * 0.25f;

            float edgeY =
                  (f_xn_yn - f_xn_yp) * 0.25f
                + (f_x0_yn - f_x0_yp) * 0.50f
                + (f_xp_yn - f_xp_yp) * 0.25f;

            float normX = edgeX * normalStrength;
            float normY = edgeY * normalStrength;

            // Normalise
            float normDot = normX * normX + normY * normY + 1.0f;
            float normRsqrt = rsqrt(normDot);

            normX *= normRsqrt;
            normY *= normRsqrt;
            normX = mad(normX, 0.5f, 0.5f);
            normY = mad(normY, 0.5f, 0.5f);
            float normZ = mad(normRsqrt, 0.5f, 0.5f);

            output[index] = new Color24(normX, normY, normZ);
        }

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
            const float maxValue = byte.MaxValue * byte.MaxValue;
            float a = c.a;
            float x = v.x * c.r * a / maxValue;
            float y = v.y * c.g * a / maxValue;
            float z = v.z * c.b * a / maxValue;
            return x * x + y * y + z * z;
        }

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
