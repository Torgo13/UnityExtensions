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

namespace PKGE
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
        /// <summary>Only used to store a reference to the original <see cref="Texture"/>.</summary>
        public readonly Texture tex;

        /// <summary>If <see cref="tex"/> is compressed, <see cref="Texture2DParameters.format"/>
        /// is changed to an uncompressed format. Otherwise, if <see cref="AsyncGPUReadbackRequest"/>
        /// is used, <see cref="Texture2DParameters.mipCount"/> is set to one.</summary>
        public readonly Texture2DParameters texParams;
        public readonly Texture2DReadable texReadable;

        /// <returns>A preallocated <see cref="NativeArray{byte}"/>
        /// if <see cref="AsyncGPUReadback"/> is in progress.</returns>
        private NativeArray<byte> data;
        private NativeArray<MipLevelParameters> mipParams;
        private readonly ReadbackAsyncDispose readback;
        private readonly Texture2D tempTex;

        /// <summary>Ensure that only either <see cref="Dispose"/> or <see cref="DisposeAsync"/>
        /// is called once total on disposal.</summary>
        private bool disposed;

        /// <param name="tex">The original <see cref="Texture"/> to be kept but not modified.</param>
        /// <param name="mipChain">Set to <see langword="true"/> to keep any existing mip chain.</param>
        /// <param name="allocator">Only change to <see cref="Allocator.TempJob"/> if the
        /// <see cref="AsyncGPUReadbackRequest"/> and job will complete within four frames.</param>
        public Texture2DProperties(Texture tex, bool mipChain = true,
            Allocator allocator = Allocator.Persistent)
        {
            Assert.IsNotNull(tex);
            Assert.AreEqual(TextureDimension.Tex2D, tex.dimension);

            this.tex = tex;
            var inputParams = new Texture2DParameters(tex);
            texReadable = new Texture2DReadable(tex);
            disposed = false;
            
            if (inputParams.mipCount == 1)
                mipChain = false;
            
            if (texReadable.UncompressedReadable)
            {
                tempTex = default;
                Texture2D tex2D = (Texture2D)tex;
                data = mipChain
                    ? tex2D.GetRawTextureData<byte>()         // Allocator.None
                    : tex2D.GetPixelData<byte>(mipLevel: 0);  // Allocator.None
                readback = default;
                texParams = mipChain
                    ? inputParams // Use original texture parameters
                    : new Texture2DParameters(inputParams.width, inputParams.height, inputParams.format, mipCount: 1);
            }
            else if (texReadable.UncompressedUnreadable)
            {
                mipChain = false;

                tempTex = default;
                data = new NativeArray<byte>(inputParams.Mip0Size,      // Empty NativeArray until
                    allocator, NativeArrayOptions.UninitializedMemory); // AsyncGPUReadbackRequest is completed 
                readback = new ReadbackAsyncDispose(ref data, tex);
                texParams = new Texture2DParameters(inputParams.width, inputParams.height,
                    inputParams.format, mipCount: 1); // AsyncGPUReadback only supports one mip level per request
            }
            else // Compressed texture formats require a blit to an uncompressed RGBA32 format
            {
                tempTex = new Texture2D(inputParams.width, inputParams.height,
                    TextureFormat.RGBA32, mipChain, texReadable.isLinear, createUninitialized: true);
                data = Blit(tex, tempTex, inputParams, texReadable.isLinear);   // Allocator.None
                readback = default;
                texParams = new Texture2DParameters(inputParams.width, inputParams.height,  // Blit can convert into any
                    TextureFormat.RGBA32, mipChain ? inputParams.mipCount : 1);             // supported uncompressed format
            }

            mipParams = mipChain && texParams.mipCount > 1
                ? InitialiseMipParameters(texParams, allocator)
                : default;
        }

        private static NativeArray<byte> Blit(Texture tex, Texture2D tempTex,
            Texture2DParameters inputParams, bool isLinear)
        {
            RenderTexture tempRT = RenderTexture.GetTemporary(inputParams.width, inputParams.height,
                depthBuffer: 0, isLinear ? GraphicsFormat.R8G8B8A8_UNorm : GraphicsFormat.R8G8B8A8_SRGB);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tempRT;

            // Copy texture data on the GPU from the non-readable Texture into the temporary RenderTexture
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
        /// <returns><see langword="default"/> if <see cref="Texture2DParameters.format"/>
        /// does not match the type for this function, or if
        /// <see cref="AsyncGPUReadbackRequest"/> is still pending and
        /// <paramref name="waitForCompletion"/> is not <see langword="true"/></returns>
        public readonly NativeArray<Color32> GetData32(int mipLevel = -1, bool waitForCompletion = false)
        {
            if (texParams.format != TextureFormat.RGBA32)
                return default;

            if (!IsReady(waitForCompletion))
                return default;

            // ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable
            var rawTextureData = data.Reinterpret<Color32>(sizeof(byte));
            // ReSharper restore PossiblyImpureMethodCallOnReadonlyVariable

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

            // ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable
            var rawTextureData = data.Reinterpret<Color24>(sizeof(byte));
            // ReSharper restore PossiblyImpureMethodCallOnReadonlyVariable

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
                texParams.format, mipChain, texReadable.isLinear, createUninitialized: true);

            // ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable
            var rawTextureData = data.Reinterpret<Color32>(sizeof(byte));
            // ReSharper restore PossiblyImpureMethodCallOnReadonlyVariable

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
                texParams.format, texParams.mipCount != 1, texReadable.isLinear, createUninitialized: true);

            // ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable
            var rawTextureData = data.Reinterpret<Color24>(sizeof(byte));
            // ReSharper restore PossiblyImpureMethodCallOnReadonlyVariable

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
            if (texReadable.IsReady)
                return true; // Already readable or Blit has been performed in the constructor

            if (waitForCompletion)
                readback.GetResult(token: 0);

            return readback.GetCompleted();
        }

        /// <returns><see langword="true"/> if the length of <see cref="data"/>
        /// matches the calculated mip chain length for its <see cref="TextureFormat"/>.</returns>
        public readonly bool IsCorrectLength(bool mipChain = false)
        {
            return data.Length == (mipChain ? texParams.MipChainSize : texParams.Mip0Size);
        }

        /// <returns><see langword="true"/> if the length of <see cref="data"/>
        /// is valid for the given <paramref name="format"/>.</returns>
        public readonly bool IsValidLength(TextureFormat format)
        {
            return (data.Length % Texture2DParameters.PixelSize(format)) != 0;
        }

        /// <inheritdoc cref="IsValidLength(TextureFormat)"/>
        public readonly bool IsValidLength()
        {
            return IsValidLength(texParams.format);
        }
        #endregion // Properties

        #region IEnumerator
        public readonly object Current => null;
        public readonly bool MoveNext() => texReadable.PerformedReadback && readback.GetStatus(token: 0) == ValueTaskSourceStatus.Pending;
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

            if (texReadable.PerformedReadback && readback.GetStatus(token: 0) == ValueTaskSourceStatus.Pending)
                readback.GetResult(token: 0); // Performs a synchronous GPU readback request

            DisposeCore();
        }

        public async ValueTask DisposeAsync()
        {
            if (disposed)
                return;

            disposed = true;

            // Return to the main thread after calling ConfigureAwait
            // to call other Unity functions
            if (texReadable.PerformedReadback && readback.GetStatus(token: 0) == ValueTaskSourceStatus.Pending)
                await new ValueTask(readback, token: 0).ConfigureAwait(continueOnCapturedContext: true);

            DisposeCore();
        }

        private void DisposeCore()
        {
            if (texReadable.PerformedReadback) // && data.IsCreated)
                data.Dispose();

            if (texReadable.PerformedBlit)
                CoreUtils.Destroy(tempTex, skipNullCheck: true);

            if (texParams.mipCount > 1)
                mipParams.Dispose();
        }
        #endregion // Dispose
    }

    /// <summary>
    /// Store the readable properties of the provided <see cref="Texture"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="supportsAsyncGPUReadback"/> is static because checking it
    /// requires an internal call which does not change at runtime.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Texture2DReadable
    {
        #region Fields
        /// <summary><see langword="true"/> if the <see cref="Texture"/> is marked readable
        /// and contains a CPU copy.</summary>
        public readonly bool isReadable;
        /// <summary><see langword="true"/> if the <see cref="Texture"/> uses a compressed format
        /// so colour values cannot be individually accessed on the CPU.</summary>
        public readonly bool isCompressed;
        /// <summary><see langword="true"/> if the <see cref="Texture"/> uses a linear colour format,
        /// typically used when it does not store colour data.</summary>
        public readonly bool isLinear;
        /// <summary><see langword="true"/> if the <see cref="Texture"/> is a <see cref="Texture2D"/>
        /// rather than a <see cref="RenderTexture"/>.</summary>
        public readonly bool isTexture2D;
        #endregion // Fields
        
        #region Constructors
        public Texture2DReadable(bool isReadable, bool isCompressed, bool isLinear, bool isTexture2D)
        {
            this.isReadable =  isReadable;
            this.isCompressed = isCompressed;
            this.isLinear = isLinear;
            this.isTexture2D = isTexture2D;
            
            CheckSupportsAsyncGPUReadback();
        }
        
        public Texture2DReadable(Texture tex)
        {
            this.isReadable =  tex.isReadable;
            this.isCompressed = Texture2DParameters.PixelSize(tex.graphicsFormat) == -1;
            this.isLinear = !tex.isDataSRGB;
            this.isTexture2D = tex is Texture2D;

            CheckSupportsAsyncGPUReadback();
        }
        #endregion // Constructors
        
        #region SupportsAsyncGPUReadback
        private enum SupportsAsyncGPUReadback : sbyte
        {
            Unchecked = -1,
            False,
            True,
        }

        /// <summary>Only check <see cref="SystemInfo.supportsAsyncGPUReadback"/>
        /// a single time across all instances as it doesn't change at runtime.</summary>
        private static SupportsAsyncGPUReadback supportsAsyncGPUReadback = SupportsAsyncGPUReadback.Unchecked;

        /// <summary>Set supportsAsyncGPUReadback for the first and only time.</summary>
        private static void CheckSupportsAsyncGPUReadback()
        {
            if (supportsAsyncGPUReadback == SupportsAsyncGPUReadback.Unchecked)
            {
                // Set supportsAsyncGPUReadback for the first and only time
                supportsAsyncGPUReadback = SystemInfo.supportsAsyncGPUReadback
                    ? SupportsAsyncGPUReadback.True
                    : SupportsAsyncGPUReadback.False;
            }
        }
        #endregion // SupportsAsyncGPUReadback

        #region Accessors
        /// <summary><see cref="Texture2D.GetPixelData{T}"/> and <see cref="Texture2D.GetRawTextureData"/>
        /// can only be called if the <see cref="Texture"/> is not compressed, is marked readable
        /// and is not a <see cref="RenderTexture"/>.</summary>
        public bool UncompressedReadable => !isCompressed && isReadable && isTexture2D;
        /// <summary>An <see cref="AsyncGPUReadback"/> is required if the <see cref="Texture"/> is not
        /// compressed and <see cref="supportsAsyncGPUReadback"/> is <see langword="true"/>.</summary>
        public bool UncompressedUnreadable => !isCompressed && !isReadable && supportsAsyncGPUReadback == SupportsAsyncGPUReadback.True;
        /// <summary>A <see cref="Graphics.Blit(Texture, RenderTexture)"/> is required if the <see cref="Texture"/> is
        /// compressed or <see cref="supportsAsyncGPUReadback"/> is <see langword="false"/></summary>
        public bool Compressed => !UncompressedReadable && !UncompressedUnreadable;
        
        public bool NoAllocation => !isCompressed && isReadable;
        public bool PerformedReadback => !isCompressed && !isReadable && supportsAsyncGPUReadback == SupportsAsyncGPUReadback.True;
        public bool PerformedBlit => !NoAllocation && !PerformedReadback;
        public bool IsReady => isCompressed || isReadable;
        #endregion // Accessors
    }
    
    /// <summary>
    /// Store Texture parameters in a struct to avoid internal Unity calls.
    /// </summary>
    /// <remarks>
    /// Create a new struct if any of the stored parameters change
    /// on the source texture.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Texture2DParameters
    {
        #region Fields
        public readonly int width;
        public readonly int height;
        public readonly TextureFormat format;
        public readonly int mipCount;
        #endregion // Fields

        #region Constructors
        public Texture2DParameters(int width, int height, TextureFormat format, int mipCount)
        {
            Assert.IsTrue(width > 0);
            Assert.IsTrue(height > 0);
            Assert.IsTrue(Enum.IsDefined(typeof(TextureFormat), format));
            Assert.IsTrue(mipCount > 0);
            
            this.width = width;
            this.height = height;
            this.format = format;
            this.mipCount = mipCount;
        }

        public Texture2DParameters(Texture tex)
        {
            width = tex.width;
            height = tex.height;
            format = tex is Texture2D tex2D
                ? tex2D.format
                : GraphicsFormatUtility.GetTextureFormat(tex.graphicsFormat);
            mipCount = tex.mipmapCount;
        }

        public Texture2DParameters(Texture2D tex)
        {
            width = tex.width;
            height = tex.height;
            format = tex.format;
            mipCount = tex.mipmapCount;
        }
        #endregion // Constructors

        // Use length to refer to the number of elements
        // Use size to refer to the number of bytes

        #region Accessors
        /// <summary>Number of elements in mip 0.</summary>
        public int Mip0Length => width * height;
        /// <summary>Number of elements in mip chain.</summary>
        public int MipChainLength => TextureUtils.MipChainLength(mipCount, width, height);

        /// <summary>Number of bytes in mip 0.</summary>
        public int Mip0Size => PixelSize() * Mip0Length;
        /// <summary>Number of bytes in mip chain.</summary>
        public int MipChainSize => PixelSize() * MipChainLength;

        /// <summary>Number of bytes per pixel.</summary>
        /// <returns>-1 if a compressed format is used.</returns>
        public int PixelSize() => PixelSize(format);
        public bool IsCompressed => PixelSize() == -1;

        /// <remarks><see href="https://docs.unity3d.com/ScriptReference/SystemInfo.SupportsTextureFormat.html"/></remarks>
        /// <exception cref="ArgumentException">Failed SupportsTextureFormat; format is not a valid TextureFormat</exception>
        public bool SupportsTextureFormat => SystemInfo.SupportsTextureFormat(format);

        /// <remarks><see href="https://docs.unity3d.com/ScriptReference/SystemInfo-maxTextureSize.html"/></remarks>
        /// <summary>Unity only supports textures up to a size of 16_384,
        /// even if <see cref="SystemInfo.maxTextureSize"/> returns a larger size.</summary>
        public static int MaxTextureLength => Math.Min(16_384, SystemInfo.maxTextureSize);

        /// <remarks><see href="https://docs.unity3d.com/Documentation/ScriptReference/NPOTSupport.Restricted.html"/></remarks>
        /// <summary>If <see langword="false"/>, limited NPOT support: no mipmaps and clamp wrap mode will be forced.
        /// If NPOT <see cref="Texture"/> does have mipmaps it will be upscaled/padded at loading time.</summary>
        public static bool FullNpotSupport => SystemInfo.npotSupport == NPOTSupport.Full;
        #endregion // Accessors

        public MipLevelParameters GetMipParameters(int mipLevel)
        {
            int offset = 0;
            int mipWidth = width;
            int mipHeight = height;

            // Get the properties of the given mipmap level
            // They are already correct for mip 0
            if (mipLevel > 0)
            {
                Assert.IsTrue(mipLevel < TextureUtils.MipmapCount(mipWidth, mipHeight),
                    "Provided mipLevel is too large for the texture dimensions.");

                TextureUtils.GetMipData(mipLevel, width, height,
                    out offset, out _, out mipWidth, out mipHeight);
            }

            return new MipLevelParameters(mipWidth, mipHeight, offset, mipLevel);
        }

        /// <remarks>4 bytes per pixel has no issues with array length.
        /// The maximum byte count is 16_384 * 16_384 * 4 = 1_073_741_824,
        /// which is less than <see cref="int.MaxValue"/> of 2_147_483_647.</remarks>
        /// <returns>The number of bytes for the given <paramref name="format"/>,
        /// otherwise -1 for any compressed <see cref="TextureFormat"/>.</returns>
        public static int PixelSize(TextureFormat format)
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
        
        /// <inheritdoc cref="PixelSize(TextureFormat)"/>
        public static int PixelSize(GraphicsFormat format)
        {
            switch (format)
            {
                case GraphicsFormat.R8_SRGB:    // 1,
                case GraphicsFormat.R8_UNorm:   // 5,
                case GraphicsFormat.R8_SNorm:   // 9,
                case GraphicsFormat.R8_UInt:    // 13, // 0x0000000D
                case GraphicsFormat.R8_SInt:    // 17, // 0x00000011
                case GraphicsFormat.S8_UInt:    // 95, // 0x0000005F
                    return 1;
                case GraphicsFormat.R8G8_SRGB:  // 2,
                case GraphicsFormat.R8G8_UNorm: // 6,
                case GraphicsFormat.R8G8_SNorm: // 10, // 0x0000000A
                case GraphicsFormat.R8G8_UInt:  // 14, // 0x0000000E
                case GraphicsFormat.R8G8_SInt:  // 18, // 0x00000012
                case GraphicsFormat.R16_UNorm:  // 21, // 0x00000015
                case GraphicsFormat.R16_SNorm:  // 25, // 0x00000019
                case GraphicsFormat.R16_UInt:   // 29, // 0x0000001D
                case GraphicsFormat.R16_SInt:   // 33, // 0x00000021
                case GraphicsFormat.R16_SFloat: // 45, // 0x0000002D
                case GraphicsFormat.R4G4B4A4_UNormPack16:   // 66, // 0x00000042
                case GraphicsFormat.B4G4R4A4_UNormPack16:   // 67, // 0x00000043
                case GraphicsFormat.R5G6B5_UNormPack16:     // 68, // 0x00000044
                case GraphicsFormat.B5G6R5_UNormPack16:     // 69, // 0x00000045
                case GraphicsFormat.R5G5B5A1_UNormPack16:   // 70, // 0x00000046
                case GraphicsFormat.B5G5R5A1_UNormPack16:   // 71, // 0x00000047
                case GraphicsFormat.A1R5G5B5_UNormPack16:   // 72, // 0x00000048
                case GraphicsFormat.D16_UNorm:              // 90, // 0x0000005A
                    return 2;
                case GraphicsFormat.R8G8B8_SRGB:        // 3,
                case GraphicsFormat.R8G8B8_UNorm:       // 7,
                case GraphicsFormat.R8G8B8_SNorm:       // 11, // 0x0000000B
                case GraphicsFormat.R8G8B8_UInt:        // 15, // 0x0000000F
                case GraphicsFormat.R8G8B8_SInt:        // 19, // 0x00000013
                case GraphicsFormat.B8G8R8_SRGB:        // 56, // 0x00000038
                case GraphicsFormat.B8G8R8_UNorm:       // 58, // 0x0000003A
                case GraphicsFormat.B8G8R8_SNorm:       // 60, // 0x0000003C
                case GraphicsFormat.B8G8R8_UInt:        // 62, // 0x0000003E
                case GraphicsFormat.B8G8R8_SInt:        // 64, // 0x00000040
                case GraphicsFormat.D16_UNorm_S8_UInt:  // 151, // 0x00000097
                    return 3;
                case GraphicsFormat.R8G8B8A8_SRGB:      // 4,
                case GraphicsFormat.R8G8B8A8_UNorm:     // 8,
                case GraphicsFormat.R8G8B8A8_SNorm:     // 12, // 0x0000000C
                case GraphicsFormat.R8G8B8A8_UInt:      // 16, // 0x00000010
                case GraphicsFormat.R8G8B8A8_SInt:      // 20, // 0x00000014
                case GraphicsFormat.R16G16_UNorm:       // 22, // 0x00000016
                case GraphicsFormat.R16G16_SNorm:       // 26, // 0x0000001A
                case GraphicsFormat.R16G16_UInt:        // 30, // 0x0000001E
                case GraphicsFormat.R16G16_SInt:        // 34, // 0x00000022
                case GraphicsFormat.R32_UInt:           // 37, // 0x00000025
                case GraphicsFormat.R32_SInt:           // 41, // 0x00000029
                case GraphicsFormat.R16G16_SFloat:      // 46, // 0x0000002E
                case GraphicsFormat.R32_SFloat:         // 49, // 0x00000031
                case GraphicsFormat.B8G8R8A8_SRGB:      // 57, // 0x00000039
                case GraphicsFormat.B8G8R8A8_UNorm:     // 59, // 0x0000003B
                case GraphicsFormat.B8G8R8A8_SNorm:     // 61, // 0x0000003D
                case GraphicsFormat.B8G8R8A8_UInt:      // 63, // 0x0000003F
                case GraphicsFormat.B8G8R8A8_SInt:      // 65, // 0x00000041
                case GraphicsFormat.E5B9G9R9_UFloatPack32:      // 73, // 0x00000049
                case GraphicsFormat.B10G11R11_UFloatPack32:     // 74, // 0x0000004A
                case GraphicsFormat.A2B10G10R10_UNormPack32:    // 75, // 0x0000004B
                case GraphicsFormat.A2B10G10R10_UIntPack32:     // 76, // 0x0000004C
                case GraphicsFormat.A2B10G10R10_SIntPack32:     // 77, // 0x0000004D
                case GraphicsFormat.A2R10G10B10_UNormPack32:    // 78, // 0x0000004E
                case GraphicsFormat.A2R10G10B10_UIntPack32:     // 79, // 0x0000004F
                case GraphicsFormat.A2R10G10B10_SIntPack32:     // 80, // 0x00000050
                case GraphicsFormat.A2R10G10B10_XRSRGBPack32:   // 81, // 0x00000051
                case GraphicsFormat.A2R10G10B10_XRUNormPack32:  // 82, // 0x00000052
                case GraphicsFormat.R10G10B10_XRSRGBPack32:     // 83, // 0x00000053
                case GraphicsFormat.R10G10B10_XRUNormPack32:    // 84, // 0x00000054
                case GraphicsFormat.D24_UNorm:          // 91, // 0x0000005B
                case GraphicsFormat.D24_UNorm_S8_UInt:  // 92, // 0x0000005C
                case GraphicsFormat.D32_SFloat:         // 93, // 0x0000005D
                    return 4;
                case GraphicsFormat.R16G16B16_UNorm:            // 23, // 0x00000017
                case GraphicsFormat.R16G16B16_SNorm:            // 27, // 0x0000001B
                case GraphicsFormat.R16G16B16_UInt:             // 31, // 0x0000001F
                case GraphicsFormat.R16G16B16_SInt:             // 35, // 0x00000023
                case GraphicsFormat.R16G16B16_SFloat:           // 47, // 0x0000002F
                    return 6;
                case GraphicsFormat.R16G16B16A16_UNorm: // 24, // 0x00000018
                case GraphicsFormat.R16G16B16A16_SNorm: // 28, // 0x0000001C
                case GraphicsFormat.R16G16B16A16_UInt:  // 32, // 0x00000020
                case GraphicsFormat.R16G16B16A16_SInt:  // 36, // 0x00000024
                case GraphicsFormat.R32G32_UInt:        // 38, // 0x00000026
                case GraphicsFormat.R32G32_SInt:        // 42, // 0x0000002A
                case GraphicsFormat.R16G16B16A16_SFloat:// 48, // 0x00000030
                case GraphicsFormat.R32G32_SFloat:      // 50, // 0x00000032
                case GraphicsFormat.A10R10G10B10_XRSRGBPack32:  // 85, // 0x00000055
                case GraphicsFormat.A10R10G10B10_XRUNormPack32: // 86, // 0x00000056
                case GraphicsFormat.D32_SFloat_S8_UInt: // 94, // 0x0000005E
                    return 8;
                case GraphicsFormat.R32G32B32_UInt:     // 39, // 0x00000027
                case GraphicsFormat.R32G32B32_SInt:     // 43, // 0x0000002B
                case GraphicsFormat.R32G32B32_SFloat:   // 51, // 0x00000033
                    return 12;
                case GraphicsFormat.R32G32B32A32_UInt:      // 40, // 0x00000028
                case GraphicsFormat.R32G32B32A32_SInt:      // 44, // 0x0000002C
                case GraphicsFormat.R32G32B32A32_SFloat:    // 52, // 0x00000034
                    return 16;
                default: // All compressed formats, including YUV2
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
            Assert.IsTrue(mipWidth > 0);
            Assert.IsTrue(mipHeight > 0);
            Assert.IsTrue(offset >= 0);
            Assert.IsTrue(mipLevel >= 0);

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
        private readonly AsyncGPUReadbackRequest _readbackRequest;

        /// <remarks>
        /// <see cref="AsyncGPUReadback.RequestIntoNativeArray"/> takes <paramref name="output"/>
        /// as a reference, but it calls <see cref="AsyncRequestNativeArrayData.CreateAndCheckAccess"/>
        /// which then takes it by value.
        /// </remarks>
        public ReadbackAsyncDispose(ref NativeArray<byte> output, Texture src)
        {
            Assert.IsTrue(output.IsCreated);
            Assert.IsNotNull(src);
            
            _readbackRequest = AsyncGPUReadback.RequestIntoNativeArray(ref output, src, mipIndex: 0);
        }

        /// <summary>Request a region of a a <see cref="Texture2D"/>, a <see cref="Texture3D"/>
        /// or a <see cref="RenderTexture"/>.</summary>
        public ReadbackAsyncDispose(ref NativeArray<byte> output, Texture src,
            int x, int width, int y, int height, int z = 0, int depth = 1)
        {
            _readbackRequest = AsyncGPUReadback.RequestIntoNativeArray(ref output, src, mipIndex: 0,
                x, width, y, height, z, depth,
                dstFormat: src.isDataSRGB ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm);
        }

        public bool GetCompleted() => GetStatus(token: 0) == ValueTaskSourceStatus.Succeeded;

        #region IValueTaskSource
        public void GetResult(short token)
        {
            // ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable
            _readbackRequest.WaitForCompletion();
            // ReSharper restore PossiblyImpureMethodCallOnReadonlyVariable
        }

        /// <summary>There is no way to cancel an <see cref="AsyncGPUReadbackRequest"/> so
        /// <see cref="ValueTaskSourceStatus.Canceled"/> can never be returned.</summary>
        public ValueTaskSourceStatus GetStatus(short token)
        {
            if (_readbackRequest.hasError)
                return ValueTaskSourceStatus.Faulted;

            if (_readbackRequest.done)
                return ValueTaskSourceStatus.Succeeded;
            
            return ValueTaskSourceStatus.Pending;
        }

        public void OnCompleted(Action<object> continuation,
            object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            continuation.Invoke(state);
        }
        #endregion // IValueTaskSource
    }
    
    /// <inheritdoc cref="ReadbackAsyncDispose"/>
    public readonly struct ReadbackMipsAsyncDispose : IValueTaskSource, IDisposable, IAsyncDisposable
    {
        private readonly NativeArray<AsyncGPUReadbackRequest> _readbackRequests;

        #region Constructors
        /// <inheritdoc cref="ReadbackAsyncDispose(ref NativeArray{byte}, Texture)"/>
        public ReadbackMipsAsyncDispose(Texture src, int mipCount)
        {
            Assert.IsNotNull(src);
            Assert.IsTrue(mipCount <= src.mipmapCount);

            _readbackRequests = InitialiseReadbackRequests(src, mipCount);
        }

        /// <inheritdoc cref="ReadbackAsyncDispose(ref NativeArray{byte}, Texture)"/>
        public ReadbackMipsAsyncDispose(Texture src)
        {
            Assert.IsNotNull(src);

            _readbackRequests = InitialiseReadbackRequests(src, src.mipmapCount);
        }

        /// <inheritdoc cref="ReadbackAsyncDispose(ref NativeArray{byte}, Texture, int, int, int, int, int, int)"/>
        public ReadbackMipsAsyncDispose(Texture src, int mipCount,
            int x, int width, int y, int height, int z = 0, int depth = 1)
        {
            Assert.IsNotNull(src);
            Assert.IsTrue(mipCount <= src.mipmapCount);

            _readbackRequests = InitialiseReadbackRequests(src, mipCount,
                x, width, y, height, z, depth);
        }

        /// <inheritdoc cref="ReadbackAsyncDispose(ref NativeArray{byte}, Texture, int, int, int, int, int, int)"/>
        public ReadbackMipsAsyncDispose(Texture src,
            int x, int width, int y, int height, int z = 0, int depth = 1)
        {
            Assert.IsNotNull(src);

            _readbackRequests = InitialiseReadbackRequests(src, src.mipmapCount,
                x, width, y, height, z, depth);
        }
        #endregion // Constructors

        static NativeArray<AsyncGPUReadbackRequest> InitialiseReadbackRequests(Texture src, int mipCount)
        {
            var readbackRequests = new NativeArray<AsyncGPUReadbackRequest>(mipCount,
                Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            for (int i = 0; i < readbackRequests.Length; i++)
            {
                readbackRequests[i] = AsyncGPUReadback.Request(src, mipIndex: i);
            }

            return readbackRequests;
        }

        static NativeArray<AsyncGPUReadbackRequest> InitialiseReadbackRequests(Texture src, int mipCount,
            int x, int width, int y, int height, int z = 0, int depth = 1)
        {
            var readbackRequests = new NativeArray<AsyncGPUReadbackRequest>(mipCount,
                Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            for (int i = 0; i < readbackRequests.Length; i++)
            {
                readbackRequests[i] = AsyncGPUReadback.Request(src, mipIndex: i,
                    x, width, y, height, z, depth,
                    dstFormat: src.isDataSRGB ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm);
            }

            return readbackRequests;
        }

        public bool GetCompleted(short token) => GetStatus(token) == ValueTaskSourceStatus.Succeeded;

        public bool GetCompleted()
        {
            bool completed = true;
            for (short i = 0; i < _readbackRequests.Length; i++)
            {
                completed &= GetCompleted(token: i);
            }
            
            return completed;
        }

        public void WaitForCompletionAll()
        {
            foreach (var request in _readbackRequests)
            {
                request.WaitForCompletion();
            }
        }

        public NativeArray<byte> GetData(int mipIndex, Allocator allocator = Allocator.Persistent)
        {
            WaitForCompletionAll();

            if (mipIndex < 0)
            {
                // Temporarily store the sizes for each mip level in bytes
                NativeArray<int> mipLevelSizes = new NativeArray<int>(_readbackRequests.Length,
                    Allocator.Temp, NativeArrayOptions.UninitializedMemory);

                int length = 0;
                for (int i = 0; i < _readbackRequests.Length; i++)
                {
                    int layerDataSize = _readbackRequests[i].layerDataSize;
                    mipLevelSizes[i] = layerDataSize;
                    length += layerDataSize;
                }

                // Allocate a NativeArray<byte> to store the entire mip chain
                NativeArray<byte> mipChainData = new NativeArray<byte>(length,
                    allocator, NativeArrayOptions.ClearMemory);

                for (int i = 0; i < _readbackRequests.Length; i++)
                {
                    int mipLevelOffset = 0;
                    for (int j = 0; j < i; j++)
                    {
                        mipLevelOffset += mipLevelSizes[j];
                    }

                    _readbackRequests[i].GetData<byte>(layer: 0)
                        .CopyTo(mipChainData.GetSubArray(mipLevelOffset, mipLevelSizes[i]));
                }

                return mipChainData;
            }

            Assert.IsTrue(mipIndex < _readbackRequests.Length);

            return _readbackRequests[mipIndex].GetData<byte>(layer: 0);
        }
        
        public void GetData(ref NativeArray<byte> data, NativeArray<MipLevelParameters> mips)
        {
            WaitForCompletionAll();
            for (int i = 0; i < _readbackRequests.Length; i++)
            {
                var mipData = _readbackRequests[i].GetData<byte>();
                NativeArray<byte>.Copy(src: mipData, srcIndex: 0,
                    dst: data, dstIndex: mips[i].offset, length: mipData.Length);
            }
        }

        #region IDisposable
        /// <summary>
        /// Wait until <see cref="GetCompleted()"/> returns <see langword="true"/> to avoid synchronously
        /// completing every <see cref="AsyncGPUReadbackRequest"/> of <see cref="_readbackRequests"/>.
        /// </summary>
        public void Dispose()
        {
            WaitForCompletionAll();
            // ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable
            _readbackRequests.Dispose();
            // ReSharper restore PossiblyImpureMethodCallOnReadonlyVariable
        }
        #endregion // IDisposable

        #region IAsyncDisposable
        public async ValueTask DisposeAsync()
        {
            foreach (var request in _readbackRequests)
            {
                if (!request.done)
                {
                    await Task.Yield();
                }
            }

            // ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable
            _readbackRequests.Dispose();
            // ReSharper restore PossiblyImpureMethodCallOnReadonlyVariable
        }
        #endregion // IAsyncDisposable

        #region IValueTaskSource
        public void GetResult(short token)
        {
            if (token < 0)
            {
                WaitForCompletionAll();
                return;
            }
            
            Assert.IsTrue(token < _readbackRequests.Length);
            _readbackRequests[token].WaitForCompletion();
        }

        /// <inheritdoc cref="ReadbackAsyncDispose.GetStatus(short)"/>
        public ValueTaskSourceStatus GetStatus(short token)
        {
            if (token < 0)
            {
                var faulted = ValueTaskSourceStatus.Pending;
                var succeeded = ValueTaskSourceStatus.Pending;
                foreach (var request in _readbackRequests)
                {
                    if (request.hasError)
                        faulted = ValueTaskSourceStatus.Faulted;
                    else if (request.done)
                        succeeded = ValueTaskSourceStatus.Succeeded;
                }
                
                if (faulted == ValueTaskSourceStatus.Faulted)
                    return faulted;
                
                if (succeeded == ValueTaskSourceStatus.Succeeded)
                    return succeeded;
                
                return ValueTaskSourceStatus.Pending;
            }
            
            Assert.IsTrue(token < _readbackRequests.Length);

            if (_readbackRequests[token].hasError)
                return ValueTaskSourceStatus.Faulted;

            if (_readbackRequests[token].done)
                return ValueTaskSourceStatus.Succeeded;
            
            return ValueTaskSourceStatus.Pending;
        }

        public void OnCompleted(Action<object> continuation,
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

        #region Texture2D
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
        /// <param name="offset">The starting index of the specified <paramref name="mipLevel"/>.</param>
        /// <param name="pow2">2^<paramref name="mipLevel"/>.</param>
        /// <param name="mipWidth">The width of the mip level.</param>
        /// <param name="mipHeight">The height of the mip level.</param>
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
        public static int MipChainLength(int mipmapCount, int width, int height)
        {
            GetMipData(mipmapCount, width, height,
                out int offset, out _, out _, out _);

            return offset;
        }

        /// <inheritdoc cref="MipChainLength(int, int, int)"/>
        public static int MipChainLength(Texture2D tex) => MipChainLength(tex.mipmapCount, tex.width, tex.height);
        
        /// <summary>Calculate the required number of mipmaps for the given
        /// <see cref="width"/> and <see cref="height"/>.</summary>
        /// <param name="width">Width of the texture.</param>
        /// <param name="height">Height of the texture.</param>
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

        /// <inheritdoc cref="MipmapCount(int, int)"/>
        public static int MipmapCount(Texture2D tex) => MipmapCount(tex.width, tex.height);
        #endregion // Texture2D
        
        #region Texture3D
        /// <inheritdoc cref="GetMipData(int, int, int, out int, out int, out int, out int)"/>
        public static void GetMipData(int mipLevel, int width, int height, int depth,
            out int offset, out int pow2, out int mipWidth, out int mipHeight, out int mipDepth)
        {
            offset = 0;
            pow2 = 1;
            mipWidth = width;
            mipHeight = height;
            mipDepth = depth;

            for (int i = 1; i <= mipLevel; i++)
            {
                offset += mipWidth * mipHeight;
                pow2 = 1 << i;
                mipWidth = width / pow2;
                mipHeight = height / pow2;
                mipDepth = depth / pow2;
            }
        }

        /// <inheritdoc cref="MipChainLength(int, int, int)"/>
        public static int MipChainLength(int mipmapCount, int width, int height, int depth)
        {
            GetMipData(mipmapCount, width, height, depth,
                out int offset, out _, out _, out _, out _);

            return offset;
        }

        /// <inheritdoc cref="MipChainLength(int, int, int, int)"/>
        public static int MipChainLength(Texture3D tex) => MipChainLength(tex.mipmapCount, tex.width, tex.height, tex.depth);

        /// <inheritdoc cref="MipmapCount(int, int)"/>
        public static int MipmapCount(int width, int height, int depth)
        {
            int mipmapCount = 0;
            int s = Math.Max(Math.Max(width, height), depth);
            while (s > 1)
            {
                ++mipmapCount;
                s >>= 1;
            }

            return mipmapCount;
        }

        /// <inheritdoc cref="MipmapCount(int, int, int)"/>
        public static int MipmapCount(Texture3D tex) => MipmapCount(tex.width, tex.height, tex.depth);
        #endregion // Texture3D
    }
}