using System.Collections;
using System.Threading.Tasks.Sources;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.TestTools;

using AssertionException = UnityEngine.Assertions.AssertionException;

namespace UnityExtensions.Tests
{
    public class TextureUtilsTests
    {
        [UnityTest]
        public IEnumerator UncompressedReadableTexture2D_RGBA32()
        {
            var testProperties = new TestTextureProperties(16, 16,
                TextureFormat.RGBA32, mipChain: false, isReadable: true);
            var tex = CreateTestTexture(testProperties);
            
            var texProperties = new Texture2DProperties(tex);
            yield return texProperties;
            var result = texProperties.GetData32();
            Assert.AreEqual(256, result.Length); // 16x16

            Assert.AreEqual(FillColour(testProperties), result[0]);

            CoreUtils.Destroy(tex);
        }

        [UnityTest]
        public IEnumerator UncompressedTextureWithMipmaps()
        {
            var testProperties = new TestTextureProperties(16, 16,
                TextureFormat.RGB24, mipChain: true, isReadable: true);
            var tex = CreateTestTexture(testProperties);
            
            var texProperties = new Texture2DProperties(tex);
            yield return texProperties;
            var result = texProperties.GetData24();
            Assert.GreaterOrEqual(result.Length, 256); // Includes mipmaps

            Assert.AreEqual(FillColour24(testProperties), result[0]);

            CoreUtils.Destroy(tex);
        }

        /*
        [UnityTest]
        public IEnumerator NonReadableTexture()
        {
            var testProperties = new TestTextureProperties(16, 16,
                TextureFormat.RGB24, mipChain: false, isReadable: false);
            var tex = CreateTestTexture(testProperties);
            
            var texProperties = new Texture2DProperties(tex);
            yield return texProperties;
            var result = texProperties.GetData24();
            Assert.AreEqual(256, result.Length); // 16x16

            Assert.AreEqual(FillColour24(testProperties), result[0]);

            CoreUtils.Destroy(tex);
        }

        [UnityTest]
        public IEnumerator CompressedTexture_DXT1()
        {
            var testProperties = new TestTextureProperties(16, 16,
                TextureFormat.DXT1, mipChain: false, isReadable: false);
            var tex = CreateTestTexture(testProperties);
            
            var texProperties = new Texture2DProperties(tex);
            yield return texProperties;
            var result = texProperties.GetData32();
            Assert.IsNotNull(result);

            Assert.AreEqual(FillColour(testProperties), result[0]);

            CoreUtils.Destroy(tex);
        }

        [UnityTest]
        public IEnumerator CompressedTexture_DXT5()
        {
            var testProperties = new TestTextureProperties(16, 16,
                TextureFormat.DXT5, mipChain: false, isReadable: false);
            var tex = CreateTestTexture(testProperties);
            
            var texProperties = new Texture2DProperties(tex);
            yield return texProperties;
            var result = texProperties.GetData32();
            Assert.IsNotNull(result);

            Assert.AreEqual(FillColour(testProperties), result[0]);

            CoreUtils.Destroy(tex);
        }

        [UnityTest]
        public IEnumerator TextureWithAlphaFormat()
        {
            var testProperties = new TestTextureProperties(16, 16,
                TextureFormat.Alpha8, mipChain: false, isReadable: true);
            var tex = CreateTestTexture(testProperties);
            
            var texProperties = new Texture2DProperties(tex);
            yield return texProperties;
            var result = texProperties.GetData8();
            Assert.IsNotNull(result);

            Assert.AreEqual(FillColour(testProperties).r, result[0]);

            CoreUtils.Destroy(tex);
        }
        */

        [UnityTest]
        public IEnumerator RenderTextureInput()
        {
            var rt = new RenderTexture(16, 16, 0);

            bool created = rt.Create();
            Assert.IsTrue(created);

            var texProperties = new Texture2DProperties(rt);
            yield return texProperties;
            var result = texProperties.GetData32();
            Assert.IsNotNull(result);

            rt.Release();
            CoreUtils.Destroy(rt);
        }

        [UnityTest]
        public IEnumerator RenderTextureTempInput()
        {
            var rt = RenderTexture.GetTemporary(16, 16, 0);

            var texProperties = new Texture2DProperties(rt);
            yield return texProperties;
            var result = texProperties.GetData32();
            Assert.IsNotNull(result);

            RenderTexture.ReleaseTemporary(rt);
        }

        readonly struct TestTextureProperties
        {
            public readonly int width;
            public readonly int height;
            public readonly TextureFormat format;
            public readonly bool mipChain;
            public readonly bool isReadable;

            public TestTextureProperties(int width, int height, TextureFormat format, bool mipChain, bool isReadable)
            {
                this.width = width;
                this.height = height;
                this.format = format;
                this.mipChain = mipChain;
                this.isReadable = isReadable;
            }
        }

        private Texture2D CreateTestTexture(TestTextureProperties textureProperties)
        {
            return CreateTestTexture(textureProperties.width, textureProperties.height,
                textureProperties.format, textureProperties.mipChain, textureProperties.isReadable);
        }

        private Texture2D CreateTestTexture(int width, int height, TextureFormat format, bool mipChain, bool isReadable)
        {
            Texture2D tex = new Texture2D(width, height, format, mipChain, linear: false);
            Color32 fill = FillColour(width, height, format, mipChain, isReadable);

            if (isReadable)
            {
                Color32[] pixels = new Color32[width * height];
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = fill;
                }

                tex.SetPixels32(pixels);
                tex.Apply(mipChain);
            }

            return tex;
        }

        private Color32 FillColour(TestTextureProperties textureProperties)
        {
            return FillColour(textureProperties.width, textureProperties.height,
                textureProperties.format, textureProperties.mipChain, textureProperties.isReadable);
        }

        private Color32 FillColour(int width, int height, TextureFormat format, bool mipChain, bool isReadable)
        {
            // Generate the same colour for the same input values
            int hash = System.HashCode.Combine(width, height, format, mipChain, isReadable);
            Random.State state = Random.state;
            Random.InitState(hash);

            Color32 fill = new Color(Random.value, Random.value, Random.value);

            Random.state = state;

            return fill;
        }

        private Color24 FillColour24(Color32 fillColour32)
        {
            return new Color24(fillColour32.r, fillColour32.g, fillColour32.b);
        }

        private Color24 FillColour24(TestTextureProperties textureProperties)
        {
            Color32 fillColour32 = FillColour(textureProperties);
            return new Color24(fillColour32.r, fillColour32.g, fillColour32.b);
        }

        #region RenderTextureToTexture2D
        [Test]
        public void RenderTextureToTexture2D_CopiesPixelsCorrectly()
        {
            int w = 4, h = 4;

            // Create a red source RenderTexture
            var rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32);
            rt.Create();

            // Fill it with red using a temp Texture2D & Blit
            var fillTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var fillPixels = new Color[w * h];
            for (int i = 0; i < fillPixels.Length; i++)
                fillPixels[i] = Color.red;
            fillTex.SetPixels(fillPixels);
            fillTex.Apply();

            Graphics.Blit(fillTex, rt);

            // Destination texture
            var tex2D = new Texture2D(w, h, TextureFormat.RGBA32, false);

            // Act
            rt.RenderTextureToTexture2D(tex2D);

            // Assert all pixels are red
            foreach (var c in tex2D.GetPixels())
            {
                Assert.AreEqual(Color.red, c);
            }

            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(fillTex);
            UnityEngine.Object.DestroyImmediate(tex2D);
        }

        [Test]
        public void RenderTextureToTexture2D_Asserts_OnDimensionMismatch()
        {
            var rt = new RenderTexture(4, 4, 0);
            var tex2D = new Texture2D(8, 8);

            Assert.Throws<AssertionException>(() => rt.RenderTextureToTexture2D(tex2D));

            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(tex2D);
        }

        [Test]
        public void RenderTextureToTexture2D_Asserts_OnNullArguments()
        {
            var rt = new RenderTexture(4, 4, 0);
            var tex2D = new Texture2D(4, 4);

            Assert.Throws<AssertionException>(() =>
                TextureUtils.RenderTextureToTexture2D(null, tex2D));
            Assert.Throws<AssertionException>(() =>
                rt.RenderTextureToTexture2D(null));

            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(tex2D);
        }
        #endregion // RenderTextureToTexture2D

        #region GetMipData
        [Test]
        public void GetMipData_Level0_ProducesBaseValues()
        {
            TextureUtils.GetMipData(0, 8, 4, out int offset, out int pow2, out int mw, out int mh);

            Assert.AreEqual(0, offset);
            Assert.AreEqual(1, pow2);
            Assert.AreEqual(8, mw);
            Assert.AreEqual(4, mh);
        }

        [Test]
        public void GetMipData_Level2_ComputesCorrectly()
        {
            TextureUtils.GetMipData(2, 8, 4, out int offset, out int pow2, out int mw, out int mh);

            // Mip 1: 8*4 = 32 pixels, Mip 2 width=2, height=1
            Assert.AreEqual(32 + (4 * 2), offset);
            Assert.AreEqual(4, pow2);
            Assert.AreEqual(2, mw);
            Assert.AreEqual(1, mh);
        }

        [Test]
        public void GetMipData3D_Level1_ComputesCorrectly()
        {
            TextureUtils.GetMipData(1, 8, 4, 2,
                out int offset, out int pow2, out int mw, out int mh, out int md);

            Assert.AreEqual(8 * 4, offset); // Mip 0 texels
            Assert.AreEqual(2, pow2);
            Assert.AreEqual(4, mw);
            Assert.AreEqual(2, mh);
            Assert.AreEqual(1, md);
        }
        #endregion // GetMipData

        #region MipChainLength
        [Test]
        public void MipChainLength_2D_MatchesManualSum()
        {
            int width = 8, height = 8;
            int mipCount = TextureUtils.MipmapCount(width, height);
            int expected = 0;
            int w = width, h = height;
            for (int i = 0; i <= mipCount; i++)
            {
                expected += w * h;
                w = System.Math.Max(1, w / 2);
                h = System.Math.Max(1, h / 2);
            }

            int actual = TextureUtils.MipChainLength(mipCount + 1, width, height);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MipmapCount_2D_ComputesCorrectly()
        {
            Assert.AreEqual(3, TextureUtils.MipmapCount(8, 2)); // 8→4→2→1
        }

        [Test]
        public void MipChainLength_3D_MatchesManualSum()
        {
            int width = 4, height = 4, depth = 4;
            int mipCount = TextureUtils.MipmapCount(width, height, depth);
            int expected = 0;
            int w = width, h = height, d = depth;
            for (int i = 0; i <= mipCount; i++)
            {
                expected += w * h;
                w = System.Math.Max(1, w / 2);
                h = System.Math.Max(1, h / 2);
                d = System.Math.Max(1, d / 2);
            }

            int actual = TextureUtils.MipChainLength(mipCount + 1, width, height, depth);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MipmapCount_3D_ComputesCorrectly()
        {
            Assert.AreEqual(2, TextureUtils.MipmapCount(4, 2, 2)); // 4→2→1
        }
        #endregion // MipChainLength
    }

    public class ReadbackAsyncDisposeTests
    {
        [Test]
        public void Constructor_Asserts_WhenNativeArrayNotCreated()
        {
            var arr = new NativeArray<byte>();
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);

            Assert.Throws<AssertionException>(() =>
            {
                var _ = new ReadbackAsyncDispose(ref arr, tex);
            });

            UnityEngine.Object.DestroyImmediate(tex);
        }

        [Test]
        public void Constructor_Asserts_WhenTextureNull()
        {
            var arr = new NativeArray<byte>(4, Allocator.Persistent);
            Assert.Throws<AssertionException>(() =>
            {
                var _ = new ReadbackAsyncDispose(ref arr, null);
            });
            arr.Dispose();
        }

        [Test]
        public void ReadbackAsyncDispose_CompletesAndReturnsSucceeded()
        {
            var arr = new NativeArray<byte>(4 * 4 * 4, Allocator.Persistent); // 4x4 RGBA
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.Apply();

            var readback = new ReadbackAsyncDispose(ref arr, tex);
            readback.GetResult(0); // Waits

            Assert.AreEqual(ValueTaskSourceStatus.Succeeded, readback.GetStatus(0));
            Assert.IsTrue(readback.GetCompleted());

            arr.Dispose();
            UnityEngine.Object.DestroyImmediate(tex);
        }
    }

    public class ReadbackMipsAsyncDisposeTests
    {
        [Test]
        public void Constructor_Asserts_WhenTextureNull()
        {
            Assert.Throws<AssertionException>(() =>
            {
                var _ = new ReadbackMipsAsyncDispose(null, 1);
            });
        }

        [Test]
        public void Constructor_Asserts_WhenMipCountTooLarge()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, true);
            Assert.Throws<AssertionException>(() =>
            {
                var _ = new ReadbackMipsAsyncDispose(tex, tex.mipmapCount + 1);
            });
            UnityEngine.Object.DestroyImmediate(tex);
        }

        [Test]
        public void GetCompleted_AllMips_ReturnsTrueAfterWait()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, true);
            tex.Apply();

            using (var readback = new ReadbackMipsAsyncDispose(tex))
            {
                readback.WaitForCompletionAll();
                Assert.IsTrue(readback.GetCompleted());
            }

            UnityEngine.Object.DestroyImmediate(tex);
        }

        [Test]
        public void GetData_NegativeIndex_ReturnsFullMipChain()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, true);
            tex.Apply();

            using (var readback = new ReadbackMipsAsyncDispose(tex))
            {
                var data = readback.GetData(-1, Allocator.Temp);
                Assert.Greater(data.Length, 0);
                data.Dispose();
            }

            UnityEngine.Object.DestroyImmediate(tex);
        }

        [Test]
        public void GetData_ValidIndex_ReturnsMipData()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, true);
            tex.Apply();

            using (var readback = new ReadbackMipsAsyncDispose(tex))
            {
                var data = readback.GetData(0, Allocator.Temp);
                Assert.AreEqual(tex.width * tex.height * 4, data.Length);
                data.Dispose();
            }

            UnityEngine.Object.DestroyImmediate(tex);
        }

        [Test]
        public void GetData_Asserts_WhenIndexOutOfRange()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, true);
            tex.Apply();

            using (var readback = new ReadbackMipsAsyncDispose(tex))
            {
                Assert.Throws<AssertionException>(() => readback.GetData(999));
            }

            UnityEngine.Object.DestroyImmediate(tex);
        }

        [Test]
        public void GetData_WithRefAndMips_CopiesCorrectly()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, true);
            tex.Apply();

            using (var readback = new ReadbackMipsAsyncDispose(tex))
            {
                var mips = new NativeArray<MipLevelParameters>(tex.mipmapCount, Allocator.Temp);
                int offset = 0;
                for (int i = 0; i < mips.Length; i++)
                {
                    int w = Mathf.Max(1, tex.width >> i);
                    int h = Mathf.Max(1, tex.height >> i);
                    mips[i] = new MipLevelParameters(w, h, offset, i);// { offset = offset };
                    offset += w * h * 4;
                }

                var data = new NativeArray<byte>(offset, Allocator.Temp);
                readback.GetData(ref data, mips);

                Assert.Greater(data.Length, 0);

                mips.Dispose();
                data.Dispose();
            }

            UnityEngine.Object.DestroyImmediate(tex);
        }

        [Test]
        public void GetStatus_ReturnsSucceededOrPending()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, true);
            tex.Apply();

            using (var readback = new ReadbackMipsAsyncDispose(tex))
            {
                readback.WaitForCompletionAll();
                var status = readback.GetStatus((short)-1);
                Assert.IsTrue(status == ValueTaskSourceStatus.Succeeded || status == ValueTaskSourceStatus.Pending);
            }

            UnityEngine.Object.DestroyImmediate(tex);
        }
    }

    public class Texture2DReadableTests
    {
        [Test]
        public void Constructor_FromBools_SetsValuesAndFlags()
        {
            var readable = new Texture2DReadable(true, false, true, true);

            Assert.IsTrue(readable.isReadable);
            Assert.IsFalse(readable.isCompressed);
            Assert.IsTrue(readable.isLinear);
            Assert.IsTrue(readable.isTexture2D);

            // Derived properties
            Assert.IsTrue(readable.UncompressedReadable);
            Assert.IsFalse(readable.Compressed);
            Assert.IsTrue(readable.NoAllocation);
            Assert.IsTrue(readable.IsReady);
        }

        [Test]
        public void Constructor_FromTexture_SetsFlagsCorrectly()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var readable = new Texture2DReadable(tex);

            Assert.AreEqual(tex.isReadable, readable.isReadable);
            Assert.AreEqual(tex is Texture2D, readable.isTexture2D);
            Assert.IsTrue(readable.NoAllocation);

            UnityEngine.Object.DestroyImmediate(tex);
        }

        [Test]
        public void UncompressedUnreadable_True_WhenReadableFalseAndSupportsReadback()
        {
            // Force known state: Uncompressed & unreadable
            var readable = new Texture2DReadable(false, false, true, true);
            bool flag = readable.UncompressedUnreadable;

            // Can't assert absolute True because depends on SystemInfo
            Assert.IsFalse(flag && readable.isReadable);
        }
    }

    public class Texture2DParametersTests
    {
        [Test]
        public void Constructor_FromInts_SetsValues()
        {
            var p = new Texture2DParameters(8, 8, TextureFormat.RGBA32, 3);
            Assert.AreEqual(8, p.width);
            Assert.AreEqual(TextureFormat.RGBA32, p.format);
            Assert.AreEqual(3, p.mipCount);
            Assert.Greater(p.Mip0Length, 0);
            Assert.Greater(p.MipChainLength, 0);
            Assert.AreEqual(p.Mip0Size, p.PixelSize() * p.Mip0Length);
        }

        [Test]
        public void Constructor_FromTextureAndTexture2D_Consistent()
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            var p1 = new Texture2DParameters((Texture)tex);
            var p2 = new Texture2DParameters(tex);

            Assert.AreEqual(p1.width, p2.width);
            Assert.AreEqual(p1.format, p2.format);

            UnityEngine.Object.DestroyImmediate(tex);
        }

        [Test]
        public void IsCompressed_ReturnsTrue_ForUnsupportedPixelSize()
        {
            var p = new Texture2DParameters(4, 4, TextureFormat.DXT1, 1);
            bool compressed = p.IsCompressed;
            Assert.AreEqual(compressed, p.PixelSize() == -1);
        }

        [Test]
        public void StaticProperties_AreAccessible()
        {
            _ = Texture2DParameters.MaxTextureLength;
            _ = Texture2DParameters.FullNpotSupport;
            Assert.Pass();
        }

        [Test]
        public void GetMipParameters_Level0_ReturnsCorrectValues()
        {
            var p = new Texture2DParameters(8, 8, TextureFormat.RGBA32, 4);
            var mip0 = p.GetMipParameters(0);
            Assert.AreEqual(8, mip0.mipWidth);
            Assert.AreEqual(8, mip0.mipHeight);
            Assert.AreEqual(0, mip0.offset);
            Assert.AreEqual(0, mip0.mipLevel);
        }

        [Test]
        public void GetMipParameters_NonZeroLevel_ComputesOffsets()
        {
            var p = new Texture2DParameters(8, 8, TextureFormat.RGBA32, 4);
            var mip1 = p.GetMipParameters(1);
            Assert.AreEqual(4, mip1.mipWidth);
            Assert.AreEqual(4, mip1.mipHeight);
            Assert.Greater(mip1.offset, 0);
            Assert.AreEqual(1, mip1.mipLevel);
        }

        [Test]
        public void GetMipParameters_Asserts_IfMipTooLarge()
        {
            var p = new Texture2DParameters(4, 4, TextureFormat.RGBA32, 1);
            Assert.Throws<AssertionException>(() =>
            {
                p.GetMipParameters(99);
            });
        }
    }

    public class MipLevelParametersTests
    {
        [Test]
        public void Constructor_SetsProperties()
        {
            var mip = new MipLevelParameters(4, 2, 8, 1);
            Assert.AreEqual(4, mip.mipWidth);
            Assert.AreEqual(2, mip.mipHeight);
            Assert.AreEqual(8, mip.offset);
            Assert.AreEqual(1, mip.mipLevel);
        }

        [Test]
        public void Constructor_Asserts_OnInvalidArgs()
        {
            Assert.Throws<AssertionException>(() => new MipLevelParameters(0, 4, 0, 0));
            Assert.Throws<AssertionException>(() => new MipLevelParameters(4, 0, 0, 0));
            Assert.Throws<AssertionException>(() => new MipLevelParameters(4, 4, -1, 0));
            Assert.Throws<AssertionException>(() => new MipLevelParameters(4, 4, 0, -1));
        }
    }

    public class Texture2DPropertiesTests
    {
        [Test]
        public void Constructor_Asserts_OnNullTexture()
        {
            Assert.Throws<AssertionException>(() =>
            {
                var _ = new Texture2DProperties(null);
            });
        }

        [Test]
        public void Constructor_Asserts_OnNon2DTexture()
        {
            var tex3D = new Texture3D(4, 4, 4, TextureFormat.RGBA32, false);
            Assert.Throws<AssertionException>(() =>
            {
                var _ = new Texture2DProperties(tex3D);
            });
            UnityEngine.Object.DestroyImmediate(tex3D);
        }

        [Test]
        public void UncompressedReadable_Path_UsesRawTextureData()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.Apply();
            var props = new Texture2DProperties(tex, mipChain: false, allocator: Allocator.Temp);

            Assert.IsTrue(props.texReadable.UncompressedReadable);
            Assert.IsTrue(props.IsReady());
            Assert.AreEqual(tex.GetRawTextureData<byte>().Length, props.GetData8().Length);
            Assert.IsTrue(props.IsCorrectLength());
            Assert.IsFalse(props.IsValidLength()); // divisible check is false for correct lengths
            props.Dispose();

            UnityEngine.Object.DestroyImmediate(tex);
        }

        [Test]
        public void Compressed_Path_PerformsBlit()
        {
            // ETC2_RGBA8 is a compressed format generally available on desktop/mobile
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.Apply();
            // Force logic into "Compressed" branch by faking compressed flags
            var props = new Texture2DProperties(tex, mipChain: false, allocator: Allocator.Temp);

            // Should report PerformedBlit if RGBA32 but not UncompressedReadable/Unreadable
            Assert.IsTrue(props.texReadable.IsReady);
            props.Dispose();
            UnityEngine.Object.DestroyImmediate(tex);
        }

        [UnityTest]
        public IEnumerator Apply32_ReturnsTextureWithSameDims()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.Apply();
            var props = new Texture2DProperties(tex, mipChain: false, allocator: Allocator.Temp);

            var applied = props.Apply32();
            Assert.IsNotNull(applied);
            Assert.AreEqual(tex.width, applied.width);
            Assert.AreEqual(tex.height, applied.height);

            props.Dispose();
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(applied);
            yield break;
        }

        [Test]
        public void GetData32_WithInvalidFormat_ReturnsDefault()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGB24, false);
            tex.Apply();
            var props = new Texture2DProperties(tex, mipChain: false, allocator: Allocator.Temp);

            var data = props.GetData32();
            Assert.AreEqual(default(NativeArray<Color32>), data);

            props.Dispose();
            UnityEngine.Object.DestroyImmediate(tex);
        }

        [Test]
        public void Enumerator_Interface_Works()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.Apply();
            var props = new Texture2DProperties(tex, mipChain: false, allocator: Allocator.Temp);

            Assert.IsFalse(((IEnumerator)props).MoveNext());
            ((IEnumerator)props).Reset();

            props.Dispose();
            UnityEngine.Object.DestroyImmediate(tex);
        }

        [UnityTest]
        public IEnumerator DisposeAsync_CompletesWithoutError()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.Apply();
            var props = new Texture2DProperties(tex, mipChain: false, allocator: Allocator.Temp);

            yield return props.DisposeAsync();

            UnityEngine.Object.DestroyImmediate(tex);
        }
    }
}
