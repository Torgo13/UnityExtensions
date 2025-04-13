using NUnit.Framework;
using UnityEngine;

namespace UnityExtensions.Tests
{
    class ColorExtensionsTests
    {
        [Test]
        public void GetColorHex_WithBlackColor_ReturnsCorrectHex()
        {
            Color32 color = new Color32(0, 0, 0, 255); // Black color with full alpha
            string result = color.GetColorHex();
            Assert.AreEqual("000000FF", result);
        }

        [Test]
        public void GetColorHex_WithWhiteColor_ReturnsCorrectHex()
        {
            Color32 color = new Color32(255, 255, 255, 255); // White color with full alpha
            string result = color.GetColorHex();
            Assert.AreEqual("FFFFFFFF", result);
        }

        [Test]
        public void GetColorHex_WithRedColor_ReturnsCorrectHex()
        {
            Color32 color = new Color32(255, 0, 0, 255); // Red color with full alpha
            string result = color.GetColorHex();
            Assert.AreEqual("FF0000FF", result);
        }

        [Test]
        public void GetColorHex_WithGreenColor_ReturnsCorrectHex()
        {
            Color32 color = new Color32(0, 255, 0, 255); // Green color with full alpha
            string result = color.GetColorHex();
            Assert.AreEqual("00FF00FF", result);
        }

        [Test]
        public void GetColorHex_WithBlueColor_ReturnsCorrectHex()
        {
            Color32 color = new Color32(0, 0, 255, 255); // Blue color with full alpha
            string result = color.GetColorHex();
            Assert.AreEqual("0000FFFF", result);
        }

        [Test]
        public void GetColorHex_WithTransparentColor_ReturnsCorrectHex()
        {
            Color32 color = new Color32(0, 0, 0, 0); // Black color with zero alpha (transparent)
            string result = color.GetColorHex();
            Assert.AreEqual("00000000", result);
        }

        [Test]
        public void GetColorHex_WithCustomColor_ReturnsCorrectHex()
        {
            Color32 color = new Color32(123, 45, 67, 89); // Custom RGBA values
            string result = color.GetColorHex();
            Assert.AreEqual("7B2D4359", result);
        }
        
        [Test]
        public void GetColorTextCode_WithBlackColor_ReturnsCorrectHex()
        {
            Color32 color = new Color32(0, 0, 0, 255); // Black color with full alpha
            string result = color.GetColorTextCode();
            Assert.AreEqual("<color=#000000FF>", result);
        }

        [Test]
        public void GetColorTextCode_WithWhiteColor_ReturnsCorrectHex()
        {
            Color32 color = new Color32(255, 255, 255, 255); // White color with full alpha
            string result = color.GetColorTextCode();
            Assert.AreEqual("<color=#FFFFFFFF>", result);
        }

        [Test]
        public void GetColorTextCode_WithRedColor_ReturnsCorrectHex()
        {
            Color32 color = new Color32(255, 0, 0, 255); // Red color with full alpha
            string result = color.GetColorTextCode();
            Assert.AreEqual("<color=#FF0000FF>", result);
        }

        [Test]
        public void GetColorTextCode_WithGreenColor_ReturnsCorrectHex()
        {
            Color32 color = new Color32(0, 255, 0, 255); // Green color with full alpha
            string result = color.GetColorTextCode();
            Assert.AreEqual("<color=#00FF00FF>", result);
        }

        [Test]
        public void GetColorTextCode_WithBlueColor_ReturnsCorrectHex()
        {
            Color32 color = new Color32(0, 0, 255, 255); // Blue color with full alpha
            string result = color.GetColorTextCode();
            Assert.AreEqual("<color=#0000FFFF>", result);
        }

        [Test]
        public void GetColorTextCode_WithTransparentColor_ReturnsCorrectHex()
        {
            Color32 color = new Color32(0, 0, 0, 0); // Black color with zero alpha (transparent)
            string result = color.GetColorTextCode();
            Assert.AreEqual("<color=#00000000>", result);
        }

        [Test]
        public void GetColorTextCode_WithCustomColor_ReturnsCorrectHex()
        {
            Color32 color = new Color32(123, 45, 67, 89); // Custom RGBA values
            string result = color.GetColorTextCode();
            Assert.AreEqual("<color=#7B2D4359>", result);
        }
    }
}
