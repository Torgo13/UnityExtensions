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
    public struct Texture2DProperties : IEnumerator, IDisposable, IAsyncDisposable
    {
        /// <summary>Only used to store a reference to the original <see cref="Texture2D"/>.</summary>
        public readonly Texture2D tex;

        /// <summary>If <see cref="tex"/> is compressed, <see cref="Texture2DParameters.format"/>
        /// is changed to an uncompressed format. Otherwise, if <see cref="AsyncGPUReadbackRequest"/>
        /// is used, <see cref="Texture2DParameters.mipCount"/> is set to one.</summary>
        public readonly Texture2DParameters parameters;

        public readonly bool isReadable; // Moved from Texture2DParameters to remain at 16 bytes
        public readonly bool isCompressed;

        /// <returns><see langword="default"/> if <see cref="AsyncGPUReadback"/> is in progress.</returns>
        private NativeArray<byte> data;
        private readonly ReadbackAsyncDispose readback;

        /// <summary>Ensure that only either <see cref="Dispose"/> or <see cref="DisposeAsync"/>
        /// is called once total on disposal.</summary>
        private bool disposed;

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

        public Texture2DProperties(Texture2D tex)
        {
            this.tex = tex;
            var inputParams = new Texture2DParameters(tex.width, tex.height, tex.format, tex.mipmapCount);
            isReadable = tex.isReadable;
            isCompressed = inputParams.IsCompressed;
            disposed = false;

            if (supportsAsyncGPUReadback == SupportsAsyncGPUReadback.Unchecked)
            {
                supportsAsyncGPUReadback = SystemInfo.supportsAsyncGPUReadback // Set supportsAsyncGPUReadback for the first and only time
                    ? SupportsAsyncGPUReadback.True
                    : SupportsAsyncGPUReadback.False;
            }

            if (!isCompressed && isReadable)
            {
                data = this.tex.GetRawTextureData<byte>(); // Allocator.None
                readback = default;
                parameters = inputParams; // Use original texture parameters
            }
            else if (!isCompressed && supportsAsyncGPUReadback == SupportsAsyncGPUReadback.True)
            {
                data = new NativeArray<byte>(inputParams.Mip0Length,
                    Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                readback = new ReadbackAsyncDispose(ref data, this.tex);
                parameters = new Texture2DParameters(inputParams.width, inputParams.height,
                    inputParams.format, mipCount: 1); // AsyncGPUReadback only supports one mip level per request
            }
            else // Compressed texture formats require a blit to an uncompressed format
            {
                Texture2D tempTex = new Texture2D(inputParams.width, inputParams.height,
                    TextureFormat.RGBA32, inputParams.mipCount != 1, linear: false, createUninitialized: true);
                RenderTexture tempRT = RenderTexture.GetTemporary(inputParams.width, inputParams.height,
                    depthBuffer: 0, GraphicsFormat.R8G8B8A8_SRGB);
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = tempRT;

                // Copy texture data on the GPU from the non-readable Texture2D into the temporary RenderTexture
                Graphics.Blit(this.tex, tempRT);

                // Copy texture data from the active RenderTexture into the temporary readable Texture2D
                // MipMaps are recalculated anyway in Texture2D.Apply()
                tempTex.ReadPixels(new Rect(0, 0, inputParams.width, inputParams.height), 0, 0, recalculateMipMaps: false);
                tempTex.Apply(updateMipmaps: true, makeNoLongerReadable: false);

                // Create a new NativeArray to copy the data into
                // as GetRawTextureData will no longer refer to Texture2D tempTex after it is destroyed
                data = new NativeArray<byte>(tempTex.GetRawTextureData<byte>(), Allocator.Persistent);

                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(tempRT);
                CoreUtils.Destroy(tempTex, skipNullCheck: true);

                readback = default;
                parameters = new Texture2DParameters(inputParams.width, inputParams.height,
                    TextureFormat.RGBA32, inputParams.mipCount); // Blit can convert into nearly any uncompressed format
            }
        }

        #region Properties
        #region GetData
        /// <inheritdoc cref="data"/>
        public readonly NativeArray<Color32> ColorData32(int mipLevel = -1)
        {
            if (parameters.format != TextureFormat.RGBA32)
                return default;

            if (!IsReady())
                return default;

            var rawTextureData = data.Reinterpret<Color32>(parameters.PixelLength());

            if (mipLevel == -1)
                return rawTextureData; // Return entire mip chain

            GetMipParameters(parameters, mipLevel,
                out int offset, out int mipLength);

            return rawTextureData.GetSubArray(offset, mipLength);
        }

        /// <inheritdoc cref="data"/>
        public readonly NativeArray<Color24> ColorData24(int mipLevel = -1)
        {
            if (parameters.format != TextureFormat.RGB24)
                return default;

            if (!IsReady())
                return default;

            var rawTextureData = data.Reinterpret<Color24>(parameters.PixelLength());

            if (mipLevel == -1)
                return rawTextureData; // Return entire mip chain

            GetMipParameters(parameters, mipLevel,
                out int offset, out int mipLength);

            return rawTextureData.GetSubArray(offset, mipLength);
        }

        /// <inheritdoc cref="data"/>
        public readonly NativeArray<byte> ColorData8()
        {
            if (!IsReady())
                return default;

            return data;
        }
        #endregion // GetData

        /// <returns><see langword="true"/> if <see cref="data"/>
        /// currently contains valid <see cref="Texture2D"/> data.</returns>
        public readonly bool IsReady()
        {
            if (isCompressed)
                return true; // Blit has been performed in the constructor

            return isReadable || readback.GetCompleted();
        }

        private static void GetMipParameters(Texture2DParameters parameters, int mipLevel,
            out int offset, out int mipLength)
        {
            offset = 0;
            int mipWidth = parameters.width;
            int mipHeight = parameters.height;

            Assert.IsTrue(mipWidth > 0);
            Assert.IsTrue(mipHeight > 0);
            Assert.IsTrue(mipLevel >= -1);

            // Get the properties of the given mipmap level
            // They are already correct for mip 0
            if (mipLevel > 0)
            {
                Assert.IsTrue(mipLevel < TextureUtils.MipmapCount(mipWidth, mipHeight),
                    "Provided mipLevel is too large for the texture dimensions.");

                TextureUtils.GetMipData(mipLevel, parameters.width, parameters.height,
                    out offset, out _, out mipWidth, out mipHeight);
            }

            mipLength = mipWidth * mipHeight;
        }

        /// <returns><see langword="true"/> if the length of <see cref="data"/>
        /// matches the calculated mip chain length for its <see cref="TextureFormat"/>.</returns>
        public readonly bool IsCorrectLength(bool mipChain = false)
        {
            return data.Length == (mipChain ? parameters.Mip0Length : parameters.MipChainLength);
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
            return IsValidLength(parameters.format);
        }
        #endregion // Properties

        #region IEnumerator
        public readonly object Current => null;
        public readonly bool MoveNext() => !isReadable && readback.GetStatus(token: 0) == ValueTaskSourceStatus.Pending;
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

            bool disposeRequired = isCompressed || !isReadable;
            if (disposeRequired)
            {
                if (readback.GetStatus(token: 0) == ValueTaskSourceStatus.Pending)
                    readback.GetResult(token: 0); // Performs synchronous GPU readback request

                if (data.IsCreated)
                    data.Dispose();
            }

            disposed = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (disposed)
                return;

            bool disposeRequired = isCompressed || !isReadable;
            if (disposeRequired)
            {
                // Return to the main thread after calling ConfigureAwait
                // to call other Unity functions
                if (readback.GetStatus(token: 0) == ValueTaskSourceStatus.Pending)
                    await new ValueTask(readback, token: 0).ConfigureAwait(continueOnCapturedContext: true);

                if (data.IsCreated)
                    data.Dispose();
            }

            disposed = true;
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
        /// <summary>Unity only supports textures up to a size of 16384,
        /// even if <see cref="SystemInfo.maxTextureSize"/> returns a larger size.</summary>
        public static int MaxTextureSize => Math.Min(16384, SystemInfo.maxTextureSize);

        /// <remarks><see href="https://docs.unity3d.com/Documentation/ScriptReference/NPOTSupport.Restricted.html"/></remarks>
        /// <summary>If <see langword="false"/>, limited NPOT support: no mipmaps and clamp wrap mode will be forced.
        /// If NPOT <see cref="Texture"/> does have mipmaps it will be upscaled/padded at loading time.</summary>
        public static bool FullNpotSupport => SystemInfo.npotSupport == NPOTSupport.Full;

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