#undef INCLUDE_UGUI

using NUnit.Framework;
using UnityEngine;

#if INCLUDE_UGUI
using UnityEngine.UI;
#endif

namespace PKGE.Editor.Tests
{
    class MaterialExtensionsTests
    {
        GameObject m_GameObject;
        Renderer m_Renderer;

#if INCLUDE_UGUI
        Graphic m_Graphic;
#endif

        Material m_Clone;

        [OneTimeSetUp]
        public void Setup()
        {
            m_GameObject = new GameObject("renderer object");
            var shader = Shader.Find("Legacy Shaders/Diffuse"); //Shader.Find("Standard");

            m_Renderer = m_GameObject.AddComponent<MeshRenderer>();
            m_Renderer.sharedMaterial = new Material(shader);

#if INCLUDE_UGUI
            m_Graphic = m_GameObject.AddComponent<Unity.XR.CoreUtils.Tests.TestImage>();
            m_Graphic.material = m_Renderer.sharedMaterial;
#endif
        }

        [Test]
        public void GetMaterialClone_ClonesRendererSharedMaterial()
        {
            m_Clone = MaterialUtils.GetMaterialClone(m_Renderer);
            Assert.AreEqual(m_Renderer.sharedMaterial, m_Clone);
            CoreUtils.Destroy(m_Clone);
        }

#if INCLUDE_UGUI
        [Test]
        public void GetMaterialClone_ClonesGraphicMaterial()
        {
            m_Clone = MaterialUtils.GetMaterialClone(m_Graphic);
            Assert.AreEqual(m_Graphic.material, m_Clone);
            CoreUtils.Destroy(m_Clone);
        }
#endif

        // normally you can directly assert equality on Colors, but
        // creating them based on the float coming from this results in mismatches due to rounding
        static void AssertColorsEqual(Color expected, Color actual)
        {
            const float tolerance = 0.334f;
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(tolerance));
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(tolerance));
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(tolerance));
            Assert.That(actual.a, Is.EqualTo(expected.a).Within(tolerance));
        }

        [TestCase("#000000", 0f, 0f, 0f, 1f)]                   // rgb: 0, 0, 0
        [TestCase("#002244", 0f, 0.133f, 0.267f, 1f)]           // rgb: 136, 221, 102
        [TestCase("#4488BBBB", 0.267f, 0.533f, 0.733f, 0.733f)] // rgba: 68, 136, 187, 187
        [TestCase("#FFFFFF", 1f, 1f, 1f, 1f)]                   // rgb: 255, 255, 255
        public void HexToColor_DoesValidConversion(string hex, float r, float g, float b, float a)
        {
            AssertColorsEqual(new Color(r, g, b, a), MaterialUtils.HexToColor(hex));
        }

        //https://github.com/Unity-Technologies/com.unity.probuilder/blob/master/Tests/Editor/Picking/ColorEncoding.cs
        #region com.unity.probuilder

        [TestCase(0xFFFFFFFFU, 255, 255, 255, 255)] // k_HexColorWhite
        [TestCase(0xFFU,         0,   0,   0, 255)] // k_HexColorBlack
        [TestCase(0x7D7D7DFFU, 125, 125, 125, 255)] // k_HexColorMiddle
        [TestCase(0xFF0000FFU, 255,   0,   0, 255)] // k_HexColorRed
        [TestCase(0xFF00FFFFU, 255,   0, 255, 255)] // k_HexColorPink
        [TestCase(0xFE00FFFFU, 254,   0, 255, 255)] // k_HexColorOffPink
        [TestCase(0x010101FFU,   1,   1,   1, 255)] // k_HexColorGray
        public void HexToColor32_DoesValidConversion(uint hex, byte r, byte g, byte b, byte a)
        {
            Assert.AreEqual(hex, ColorExtensions.DecodeRGBA(new Color32(r, g, b, a)));
            Assert.AreEqual(new Color32(r, g, b, a), ColorExtensions.EncodeRGBA(hex));

            unchecked
            {
                Assert.AreNotEqual(hex + 1, ColorExtensions.DecodeRGBA(new Color32(r, g, b, a)));
                Assert.AreNotEqual(new Color32(r, g, b, a), ColorExtensions.EncodeRGBA(hex + 1));
                Assert.AreNotEqual(hex - 1, ColorExtensions.DecodeRGBA(new Color32(r, g, b, a)));
                Assert.AreNotEqual(new Color32(r, g, b, a), ColorExtensions.EncodeRGBA(hex - 1));
            }
        }

        #endregion // com.unity.probuilder

        [TestCase(0b111_111_11, 255, 255, 255, 255)] // k_HexColorWhite
        [TestCase(0b000_000_00,   0,   0,   0, 255)] // k_HexColorBlack
        [TestCase(0b100_100_10, 128, 128, 128, 255)]
        [TestCase(0b011_011_01, 127, 127, 127, 255)]
        [TestCase(0b011_011_01, 125, 125, 125, 255)] // k_HexColorMiddle
        [TestCase(0b111_000_00, 255,   0,   0, 255)] // k_HexColorRed
        [TestCase(0b111_000_11, 255,   0, 255, 255)] // k_HexColorPink
        [TestCase(0b111_000_11, 254,   0, 255, 255)] // k_HexColorOffPink
        [TestCase(0b000_000_00,   1,   1,   1, 255)] // k_HexColorGray
        public void ByteToColor32_DoesValidConversion(byte hex, byte r, byte g, byte b, byte a)
        {
            Assert.AreEqual(hex, ColorUtils.Color32ToByte(new Color32(r, g, b, a)));
            //Assert.AreEqual(new Color32(r, g, b, a), ColorUtils.ByteToColor32(hex));

            unchecked
            {
                Assert.AreNotEqual((byte)(hex + 1), ColorUtils.Color32ToByte(new Color32(r, g, b, a)));
                //Assert.AreNotEqual(new Color32(r, g, b, a), ColorUtils.ByteToColor32((byte)(hex + 1)));
                Assert.AreNotEqual((byte)(hex - 1), ColorUtils.Color32ToByte(new Color32(r, g, b, a)));
                //Assert.AreNotEqual(new Color32(r, g, b, a), ColorUtils.ByteToColor32((byte)(hex - 1)));
            }
        }

        [TestCase(0b111_111_11, 255, 255, 255, 255)] // k_HexColorWhite
        [TestCase(0b000_000_00, 0, 0, 0, 255)] // k_HexColorBlack
        //[TestCase(0b100_100_10, 128, 128, 128, 255)]
        [TestCase(0b011_011_01, 127, 127, 127, 255)]
        [TestCase(0b011_011_01, 125, 125, 125, 255)] // k_HexColorMiddle
        [TestCase(0b111_000_00, 255, 0, 0, 255)] // k_HexColorRed
        [TestCase(0b111_000_11, 255, 0, 255, 255)] // k_HexColorPink
        //[TestCase(0b111_000_11, 254, 0, 255, 255)] // k_HexColorOffPink
        [TestCase(0b000_000_00, 1, 1, 1, 255)] // k_HexColorGray
        public void ByteToColor_DoesValidConversion(byte hex, byte r, byte g, byte b, byte a)
        {
            Assert.AreEqual(hex, ColorUtils.ColorToByte(new Color32(r, g, b, a)));
            //Assert.AreEqual(new Color32(r, g, b, a), ColorUtils.ByteToColor(hex));

            unchecked
            {
                Assert.AreNotEqual((byte)(hex + 1), ColorUtils.ColorToByte(new Color32(r, g, b, a)));
                //Assert.AreNotEqual(new Color32(r, g, b, a), ColorUtils.ByteToColor((byte)(hex + 1)));
                Assert.AreNotEqual((byte)(hex - 1), ColorUtils.ColorToByte(new Color32(r, g, b, a)));
                //Assert.AreNotEqual(new Color32(r, g, b, a), ColorUtils.ByteToColor((byte)(hex - 1)));
            }
        }

        [TearDown]
        public void Cleanup() { }
    }
}
