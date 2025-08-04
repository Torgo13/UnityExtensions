using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace UnityExtensions
{
    /// <summary>
    /// Ensure that an <see cref="AsyncGPUReadbackRequest"/> does not outlive
    /// the <see cref="NativeArray{T}"/> it is writing to.
    /// </summary>
    /// <remarks>
    /// Assumes that textures either have a single mip level or a full mip chain.
    /// </remarks>
    public struct Texture2DProperties : IEnumerator, IDisposable, IAsyncDisposable
    {
        /// <summary>Only used to store a reference to the original <see cref="Texture2D"/>.</summary>
        public readonly Texture2D tex;

        /// <summary>If <see cref="tex"/> is compressed, <see cref="Texture2DParameters.format"/>
        /// is changed to an uncompressed format. Otherwise, if <see cref="AsyncGPUReadbackRequest"/>
        /// is used, <see cref="Texture2DParameters.mipCount"/> is set to one.</summary>
        public readonly Texture2DParameters texParams;

        public readonly bool isReadable;
        public readonly bool isCompressed;
        public readonly bool isLinear;

        /// <returns>An preallocated <see cref="NativeArray{byte}"/>
        /// if <see cref="AsyncGPUReadback"/> is in progress.</returns>
        private NativeArray<byte> data;
        private NativeArray<MipLevelParameters> mipParams;
        private readonly ReadbackAsyncDispose readback;
        private readonly Texture2D tempTex;

        /// <summary>Ensure that only either <see cref="Dispose"/> or <see cref="DisposeAsync"/>
        /// is called once total on disposal.</summary>
        private bool disposed;

        private readonly bool NoAllocation => !isCompressed && isReadable;
        private readonly bool PerformedReadback => !isCompressed && !isReadable && supportsAsyncGPUReadback == SupportsAsyncGPUReadback.True;
        private readonly bool PerformedBlit => !NoAllocation && !PerformedReadback;

        #region SupportsAsyncGPUReadback
        enum SupportsAsyncGPUReadback : sbyte
        {
            Unchecked = -1,
            False,
            True,
        }

        /// <summary>Only check <see cref="SystemInfo.supportsAsyncGPUReadback"/>
        /// a single time across all instances as it doesn't change at runtime.</summary>
        private static SupportsAsyncGPUReadback supportsAsyncGPUReadback = SupportsAsyncGPUReadback.Unchecked;
        #endregion // SupportsAsyncGPUReadback

        /// <param name="tex">The original <see cref="Texture2D"/> to be kept but not modified.</param>
        /// <param name="mipChain">Set to <see langword="true"/> to keep any existing mip chain.</param>
        /// <param name="allocator">Only change to <see cref="Allocator.TempJob"/> if the
        /// <see cref="AsyncGPUReadbackRequest"/> and job will complete within four frames.</param>
        public Texture2DProperties(Texture2D tex, bool mipChain = true,
            Allocator allocator = Allocator.Persistent)
        {
            this.tex = tex;
            var inputParams = new Texture2DParameters(tex);
            
            isReadable = tex.isReadable;
            isCompressed = inputParams.IsCompressed;
            isLinear = !tex.isDataSRGB;
            disposed = false;

            if (supportsAsyncGPUReadback == SupportsAsyncGPUReadback.Unchecked)
            {
                // Set supportsAsyncGPUReadback for the first and only time
                supportsAsyncGPUReadback = SystemInfo.supportsAsyncGPUReadback
                    ? SupportsAsyncGPUReadback.True
                    : SupportsAsyncGPUReadback.False;
            }

            if (!isCompressed && isReadable)
            {
                tempTex = default;
                data = mipChain
                    ? tex.GetRawTextureData<byte>()         // Allocator.None
                    : tex.GetPixelData<byte>(mipLevel: 0);  // Allocator.None
                readback = default;
                texParams = mipChain
                    ? inputParams // Use original texture parameters
                    : new Texture2DParameters(inputParams.width, inputParams.height, inputParams.format, mipCount: 1);
            }
            else if (!isCompressed && supportsAsyncGPUReadback == SupportsAsyncGPUReadback.True)
            {
                mipChain = false;

                tempTex = default;
                data = new NativeArray<byte>(inputParams.Mip0Length,    // Empty NativeArray until
                    allocator, NativeArrayOptions.UninitializedMemory); // AsyncGPUReadbackRequest is completed 
                readback = new ReadbackAsyncDispose(ref data, tex);
                texParams = new Texture2DParameters(inputParams.width, inputParams.height,
                    inputParams.format, mipCount: 1); // AsyncGPUReadback only supports one mip level per request
            }
            else // Compressed texture formats require a blit to an uncompressed RGBA32 format
            {
                tempTex = new Texture2D(inputParams.width, inputParams.height,
                    TextureFormat.RGBA32, mipChain, isLinear, createUninitialized: true);
                data = Blit(tex, tempTex, inputParams, isLinear);   // Allocator.None
                readback = default;
                texParams = new Texture2DParameters(inputParams.width, inputParams.height,  // Blit can convert into any
                    TextureFormat.RGBA32, mipChain ? inputParams.mipCount : 1);             // supported uncompressed format
            }

            mipParams = mipChain
                ? InitialiseMipParameters(texParams, allocator)
                : default;
        }

        private static NativeArray<byte> Blit(Texture2D tex, Texture2D tempTex,
            Texture2DParameters inputParams, bool isLinear)
        {
            RenderTexture tempRT = RenderTexture.GetTemporary(inputParams.width, inputParams.height,
                depthBuffer: 0, isLinear ? GraphicsFormat.R8G8B8A8_UNorm : GraphicsFormat.R8G8B8A8_SRGB);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tempRT;

            // Copy texture data on the GPU from the non-readable Texture2D into the temporary RenderTexture
            Graphics.Blit(tex, tempRT);

            // Copy texture data from the active RenderTexture into the temporary readable Texture2D
            tempTex.ReadPixels(new Rect(0, 0, inputParams.width, inputParams.height), 0, 0,
                recalculateMipMaps: tempTex.mipmapCount > 1);

            // MipMaps can also be recalculated in Texture2D.Apply()
            //tempTex.Apply(updateMipmaps: tempTex.mipmapCount > 1, makeNoLongerReadable: false);

            // data will continue to refer to the readable CPU data of tempTex until Dispose is called
            NativeArray<byte> rawTextureData = tempTex.GetRawTextureData<byte>();   // Allocator.None

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tempRT);

            return rawTextureData;
        }

        private static NativeArray<MipLevelParameters> InitialiseMipParameters(
            Texture2DParameters texParams, Allocator allocator)
        {
            var mipParameters = new NativeArray<MipLevelParameters>(texParams.mipCount,
                allocator, NativeArrayOptions.UninitializedMemory);

            for (int i = 0; i < mipParameters.Length; i++)
            {
                TextureUtils.GetMipData(mipLevel: i, texParams.width, texParams.height,
                    out int offset, out _, out int mipWidth, out int mipHeight);
                mipParameters[i] = new MipLevelParameters(mipWidth, mipHeight, offset, mipLevel: i);
            }

            return mipParameters;
        }

        #region Properties
        #region GetData
        /// <param name="mipLevel">Request a specific mip level,
        /// or use -1 to return all present mipmaps.</param>
        /// <param name="waitForCompletion">Set to <see langword="true"/>
        /// to force the <see cref="AsyncGPUReadbackRequest"/> to complete.</param>
        /// <returns><see langword="default"/> if <see cref="texParams.format"/>
        /// does not match the type for this function, or if
        /// <see cref="AsyncGPUReadbackRequest"/>is still pending and
        /// <paramref name="waitForCompletion"/> is not <see langword="true"/></returns>
        public readonly NativeArray<Color32> GetData32(int mipLevel = -1, bool waitForCompletion = false)
        {
            if (texParams.format != TextureFormat.RGBA32)
                return default;

            if (!IsReady(waitForCompletion))
                return default;

            var rawTextureData = data.Reinterpret<Color32>(texParams.PixelLength());

            if (mipLevel == -1 || texParams.mipCount == 1)
                return rawTextureData; // Return entire mip chain

            if (mipLevel >= mipParams.Length)
                return default;

            var mipParameters = mipParams[mipLevel];

            return rawTextureData.GetSubArray(mipParameters.offset,
                mipParameters.mipWidth * mipParameters.mipHeight);
        }

        /// <inheritdoc cref="GetData32(int, bool)"/>
        public readonly NativeArray<Color24> GetData24(int mipLevel = -1, bool waitForCompletion = false)
        {
            if (texParams.format != TextureFormat.RGB24)
                return default;

            if (!IsReady(waitForCompletion))
                return default;

            var rawTextureData = data.Reinterpret<Color24>(texParams.PixelLength());

            if (mipLevel == -1 || texParams.mipCount == 1)
                return rawTextureData; // Return entire mip chain

            if (mipLevel >= mipParams.Length)
                return default;

            var mipParameters = mipParams[mipLevel];

            return rawTextureData.GetSubArray(mipParameters.offset,
                mipParameters.mipWidth * mipParameters.mipHeight);
        }

        /// <inheritdoc cref="data"/>
        public readonly NativeArray<byte> GetData8()
        {
            if (!IsReady())
                return default;

            return data;
        }
        #endregion // GetData

        public readonly Texture2D Apply32(bool updateMipmaps = true, bool makeNoLongerReadable = false)
        {
            if (texParams.format != TextureFormat.RGBA32)
                return default;

            bool mipChain = texParams.mipCount != 1;

            Texture2D output = new Texture2D(texParams.width, texParams.height,
                texParams.format, mipChain, isLinear, createUninitialized: true);

            var rawTextureData = data.Reinterpret<Color32>(texParams.PixelLength());

            // Only copy mip 0 if Texture2D.Apply() will update the mipmaps anyway
            // or the original data does not contain multiple mip levels.
            if (updateMipmaps || !mipChain)
            {
                var mipData = mipChain
                    ? rawTextureData.GetSubArray(0, texParams.width * texParams.height)
                    : rawTextureData;   // rawTextureData is the full size of mip 0
                output.SetPixelData(mipData, mipLevel: 0);
            }
            else
            {
                foreach (var mipParam in mipParams)
                {
                    var mipData = rawTextureData.GetSubArray(mipParam.offset, mipParam.mipWidth * mipParam.mipHeight);
                    output.SetPixelData(mipData, mipParam.mipLevel);
                }
            }

            output.Apply(updateMipmaps, makeNoLongerReadable);
            return output;
        }

        public readonly Texture2D Apply24(bool updateMipmaps = true, bool makeNoLongerReadable = false)
        {
            if (texParams.format != TextureFormat.RGB24)
                return default;

            bool mipChain = texParams.mipCount != 1;

            Texture2D output = new Texture2D(texParams.width, texParams.height,
                texParams.format, texParams.mipCount != 1, isLinear, createUninitialized: true);

            var rawTextureData = data.Reinterpret<Color24>(texParams.PixelLength());

            // Only copy mip 0 if Texture2D.Apply() will update the mipmaps anyway
            // or the original data does not contain multiple mip levels.
            if (updateMipmaps || !mipChain)
            {
                var mipData = mipChain
                    ? rawTextureData.GetSubArray(0, texParams.width * texParams.height)
                    : rawTextureData;   // rawTextureData is the full size of mip 0
                output.SetPixelData(mipData, mipLevel: 0);
            }
            else
            {
                foreach (var mipParam in mipParams)
                {
                    var mipData = rawTextureData.GetSubArray(mipParam.offset, mipParam.mipWidth * mipParam.mipHeight);
                    output.SetPixelData(mipData, mipParam.mipLevel);
                }
            }

            output.Apply(updateMipmaps, makeNoLongerReadable);
            return output;
        }

        /// <returns><see langword="true"/> if <see cref="data"/>
        /// currently contains valid <see cref="Texture2D"/> data.</returns>
        public readonly bool IsReady(bool waitForCompletion = false)
        {
            if (isCompressed || isReadable)
                return true; // Already readable or Blit has been performed in the constructor

            if (waitForCompletion)
                readback.GetResult(token: 0);

            return readback.GetCompleted();
        }

        private static void GetMipParameters(Texture2DParameters texParams, int mipLevel,
            out int offset, out int mipLength)
        {
            offset = 0;
            int mipWidth = texParams.width;
            int mipHeight = texParams.height;

            Assert.IsTrue(mipWidth > 0);
            Assert.IsTrue(mipHeight > 0);
            Assert.IsTrue(mipLevel >= -1);

            // Get the properties of the given mipmap level
            // They are already correct for mip 0
            if (mipLevel > 0)
            {
                Assert.IsTrue(mipLevel < TextureUtils.MipmapCount(mipWidth, mipHeight),
                    "Provided mipLevel is too large for the texture dimensions.");

                TextureUtils.GetMipData(mipLevel, texParams.width, texParams.height,
                    out offset, out _, out mipWidth, out mipHeight);
            }

            mipLength = mipWidth * mipHeight;
        }

        /// <returns><see langword="true"/> if the length of <see cref="data"/>
        /// matches the calculated mip chain length for its <see cref="TextureFormat"/>.</returns>
        public readonly bool IsCorrectLength(bool mipChain = false)
        {
            return data.Length == (mipChain ? texParams.Mip0Length : texParams.MipChainLength);
        }

        /// <returns><see langword="true"/> if the length of <see cref="data"/>
        /// is valid for the given <paramref name="format"/>.</returns>
        public readonly bool IsValidLength(TextureFormat format)
        {
            return (data.Length % Texture2DParameters.PixelLength(format)) != 0;
        }

        /// <inheritdoc cref="IsValidLength(TextureFormat)"/>
        public readonly bool IsValidLength()
        {
            return IsValidLength(texParams.format);
        }
        #endregion // Properties

        #region IEnumerator
        public readonly object Current => null;
        public readonly bool MoveNext() => PerformedReadback && readback.GetStatus(token: 0) == ValueTaskSourceStatus.Pending;
        public readonly void Reset() {}
        #endregion // IEnumerator

        #region Dispose
        /// <summary>
        /// If <see cref="MoveNext"/> returns <see langword="false"/>
        /// then <see cref="Dispose"/> can be called without performing a
        /// synchronous GPU readback request.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            if (PerformedReadback && readback.GetStatus(token: 0) == ValueTaskSourceStatus.Pending)
                readback.GetResult(token: 0); // Performs synchronous GPU readback request

            DisposeCore();
        }

        public async ValueTask DisposeAsync()
        {
            if (disposed)
                return;

            disposed = true;

            // Return to the main thread after calling ConfigureAwait
            // to call other Unity functions
            if (PerformedReadback && readback.GetStatus(token: 0) == ValueTaskSourceStatus.Pending)
                await new ValueTask(readback, token: 0).ConfigureAwait(continueOnCapturedContext: true);

            DisposeCore();
        }

        private void DisposeCore()
        {
            if (PerformedReadback) // && data.IsCreated)
                data.Dispose();

            if (PerformedBlit)
                CoreUtils.Destroy(tempTex, skipNullCheck: true);

            if (texParams.mipCount > 1)
                mipParams.Dispose();
        }
        #endregion // Dispose
    }

    /// <summary>
    /// Store Texture2D parameters in a struct to avoid internal Unity calls.
    /// </summary>
    /// <remarks>
    /// Create a new struct if any of the stored parameters change
    /// on the source texture.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Texture2DParameters
    {
        public readonly int width;
        public readonly int height;
        public readonly TextureFormat format;
        public readonly int mipCount;

        public Texture2DParameters(int width, int height, TextureFormat format, int mipCount)
        {
            this.width = width;
            this.height = height;
            this.format = format;
            this.mipCount = mipCount;
        }

        public Texture2DParameters(Texture2D tex)
        {
            width = tex.width;
            height = tex.height;
            format = tex.format;
            mipCount = tex.mipmapCount;
        }

        // Use size to refer to the number of elements
        // Use length to refer to the number of bytes

        public readonly int Mip0Size => width * height;
        public readonly int MipChainSize => TextureUtils.MipChainSize(mipCount, width, height);

        public readonly int Mip0Length => PixelLength() * Mip0Size;
        public readonly int MipChainLength => PixelLength() * MipChainSize;
        public readonly bool IsCompressed => PixelLength() == -1;
        public readonly int PixelLength() => PixelLength(format);

        /// <remarks><see href="https://docs.unity3d.com/ScriptReference/SystemInfo.SupportsTextureFormat.html"/></remarks>
        /// <exception cref="ArgumentException">Failed SupportsTextureFormat; format is not a valid TextureFormat</exception>
        public readonly bool SupportsTextureFormat => SystemInfo.SupportsTextureFormat(format);

        /// <remarks><see href="https://docs.unity3d.com/ScriptReference/SystemInfo-maxTextureSize.html"/></remarks>
        /// <summary>Unity only supports textures up to a size of 16_384,
        /// even if <see cref="SystemInfo.maxTextureSize"/> returns a larger size.</summary>
        public static int MaxTextureSize => Math.Min(16_384, SystemInfo.maxTextureSize);

        /// <remarks><see href="https://docs.unity3d.com/Documentation/ScriptReference/NPOTSupport.Restricted.html"/></remarks>
        /// <summary>If <see langword="false"/>, limited NPOT support: no mipmaps and clamp wrap mode will be forced.
        /// If NPOT <see cref="Texture"/> does have mipmaps it will be upscaled/padded at loading time.</summary>
        public static bool FullNpotSupport => SystemInfo.npotSupport == NPOTSupport.Full;

        /// <remarks>4 bytes per pixel has no issues with array length.
        /// The maximum byte count is 16_384 * 16_384 * 4 = 1_073_741_824,
        /// which is less than <see cref="int.MaxValue"/> of 2_147_483_647.</remarks>
        /// <returns>The number of bytes for the given <paramref name="format"/>,
        /// otherwise -1 for any compressed <see cref="TextureFormat"/>.</returns>
        public static int PixelLength(TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.R8:
                case TextureFormat.Alpha8:
                    return 1;
                case TextureFormat.RGBA4444:
                case TextureFormat.ARGB4444:
                case TextureFormat.RGB565:
                case TextureFormat.R16:
                case TextureFormat.RHalf:
                    return 2;
                case TextureFormat.RGB24:
                    return 3;
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.BGRA32:
                case TextureFormat.RG32:
                case TextureFormat.RGHalf:
                case TextureFormat.RFloat:
                    return 4;
                case TextureFormat.RGB48:
                    return 6;
                case TextureFormat.RGBA64:
                case TextureFormat.RGBAHalf:
                case TextureFormat.RGFloat:
                    return 8;
                case TextureFormat.RGBAFloat:
                    return 16;
                default:
                    return -1;
            }
        }

        public static bool HasAlpha(TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.Alpha8:
                case TextureFormat.RGBA4444:
                case TextureFormat.ARGB4444:
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.BGRA32:
                case TextureFormat.RGBA64:
                case TextureFormat.RGBAHalf:
                case TextureFormat.RGBAFloat:
                    return true;
                case TextureFormat.R8:
                case TextureFormat.RGB565:
                case TextureFormat.R16:
                case TextureFormat.RHalf:
                case TextureFormat.RGB24:
                case TextureFormat.RG32:
                case TextureFormat.RGHalf:
                case TextureFormat.RFloat:
                case TextureFormat.RGB48:
                case TextureFormat.RGFloat:
                default:
                    return false;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct MipLevelParameters
    {
        public readonly int mipWidth;
        public readonly int mipHeight;
        public readonly int offset;
        public readonly int mipLevel;

        public MipLevelParameters(int mipWidth, int mipHeight, int offset, int mipLevel)
        {
            this.mipWidth = mipWidth;
            this.mipHeight = mipHeight;
            this.offset = offset;
            this.mipLevel = mipLevel;
        }
    }

    /// <summary>
    /// An <see cref="AsyncGPUReadbackRequest"/> wrapper implementing <see cref="IValueTaskSource"/>
    /// so it can be used in the constructor for <see cref="ValueTask"/>.
    /// </summary>
    public readonly struct ReadbackAsyncDispose : IValueTaskSource
    {
        private readonly AsyncGPUReadbackRequest readbackRequest;

        /// <remarks>
        /// <see cref="AsyncGPUReadback.RequestIntoNativeArray"/> takes <paramref name="output"/>
        /// as a reference, but it calls <see cref="AsyncRequestNativeArrayData.CreateAndCheckAccess"/>
        /// which then takes it by value.
        /// </remarks>
        public ReadbackAsyncDispose(ref NativeArray<byte> output, Texture src)
        {
            readbackRequest = AsyncGPUReadback.RequestIntoNativeArray(ref output, src, mipIndex: 0);
        }

        /// <summary>Request a region of a <see cref="Texture2D"/> or <see cref="Texture3D"/>.</summary>
        public ReadbackAsyncDispose(ref NativeArray<byte> output, Texture src,
            int x, int width, int y, int height, int z = 0, int depth = 1)
        {
            readbackRequest = AsyncGPUReadback.RequestIntoNativeArray(ref output, src, mipIndex: 0,
                x, width, y, height, z, depth,
                dstFormat: src.isDataSRGB ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm);
        }

        public readonly bool GetCompleted() => GetStatus(token: 0) == ValueTaskSourceStatus.Succeeded;

        #region IValueTaskSource
        public readonly void GetResult(short token)
        {
            readbackRequest.WaitForCompletion();
        }

        /// <summary>There is no way to cancel an <see cref="AsyncGPUReadbackRequest"/> so
        /// <see cref="ValueTaskSourceStatus.Canceled"/> can never be returned.</summary>
        public readonly ValueTaskSourceStatus GetStatus(short token)
        {
            if (readbackRequest.hasError)
                return ValueTaskSourceStatus.Faulted;

            if (readbackRequest.done)
                return ValueTaskSourceStatus.Succeeded;
            
            return ValueTaskSourceStatus.Pending;
        }

        public readonly void OnCompleted(Action<object> continuation,
            object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            continuation.Invoke(state);
        }
        #endregion // IValueTaskSource
    }

    /// <summary>
    /// Utilities for manipulating Textures.
    /// </summary>
    public static class TextureUtils
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/TextureUtils.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Copy a given <see cref="RenderTexture"/> to a <see cref="Texture2D"/>.
        /// This method assumes that both textures exist and are the same size.
        /// </summary>
        /// <param name="renderTexture">The source <see cref="RenderTexture" />.</param>
        /// <param name="texture">The destination <see cref="Texture2D" />.</param>
        public static void RenderTextureToTexture2D(this RenderTexture renderTexture, Texture2D texture)
        {
            Assert.IsNotNull(renderTexture);
            Assert.IsNotNull(texture);
            Assert.AreEqual(renderTexture.width, texture.width);
            Assert.AreEqual(renderTexture.height, texture.height);

            var temp = RenderTexture.active;
            
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            texture.Apply();
            
            RenderTexture.active = temp;
        }
        #endregion // Unity.XR.CoreUtils

        /// <summary>
        /// Calculates the starting index of a mip level given the
        /// original width and height of the texture.
        /// Assumes the full mip chain is present.
        /// </summary>
        /// <remarks>
        /// Provide <paramref name="mipLevel"/> + 1 to get the total number
        /// of pixels including that mip level.
        /// </remarks>
        /// <param name="mipLevel">Mipmap level ranging from 0 to mipCount - 1.</param>
        /// <param name="width">Full width of the texture.</param>
        /// <param name="height">Full height of the texture.</param>
        /// <returns>
        /// The starting index of the specified <paramref name="mipLevel"/>.
        /// </returns>
        public static void GetMipData(int mipLevel, int width, int height,
            out int offset, out int pow2, out int mipWidth, out int mipHeight)
        {
            offset = 0;
            pow2 = 1;
            mipWidth = width;
            mipHeight = height;

            for (int i = 1; i <= mipLevel; i++)
            {
                offset += mipWidth * mipHeight;
                pow2 = 1 << i;
                mipWidth = width / pow2;
                mipHeight = height / pow2;
            }
        }

        /// <summary>
        /// Calculates the total number of elements a texture with a mip chain would require.
        /// </summary>
        /// <param name="mipmapCount">Use <see cref="Texture.mipmapCount"/>
        /// to calculate the full mip chain.</param>
        /// <param name="width">The original width of the texture at mip 0.</param>
        /// <param name="height">The original height of the texture at mip 0.</param>
        /// <returns>The number of elements required for all given mip levels.</returns>
        public static int MipChainSize(int mipmapCount, int width, int height)
        {
            GetMipData(mipmapCount, width, height,
                out int offset, out _, out _, out _);

            return offset;
        }

        /// <inheritdoc cref="MipChainSize(int, int, int)"/>
        public static int MipChainSize(Texture2D tex)
        {
            return MipChainSize(tex.mipmapCount, tex.width, tex.height);
        }

        public static int MipmapCount(int width, int height)
        {
            int mipmapCount = 0;
            int s = Math.Max(width, height);
            while (s > 1)
            {
                ++mipmapCount;
                s >>= 1;
            }

            return mipmapCount;
        }
    }
}