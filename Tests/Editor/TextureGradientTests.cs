using NUnit.Framework;
using UnityEngine;

namespace UnityExtensions.Tests
{
    public class TextureGradientTests
    {
        /// <summary>
        /// Validates that the gradient's color and alpha keys are properly initialized.
        /// </summary>
        [Test]
        public void TextureGradient_CreatesGradientSuccessfully()
        {
            // Arrange
            var colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(Color.red, 0f),
                new GradientColorKey(Color.green, 0.5f),
                new GradientColorKey(Color.blue, 1f)
            };

            var alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.5f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            };

            // Act
            var textureGradient = new TextureGradient(colorKeys, alphaKeys);

            // Assert
            Assert.NotNull(textureGradient.colorKeys, "Color keys should not be null.");
            Assert.NotNull(textureGradient.alphaKeys, "Alpha keys should not be null.");
            Assert.AreEqual(3, textureGradient.colorKeys.Length, "Color keys count mismatch.");
            Assert.AreEqual(3, textureGradient.alphaKeys.Length, "Alpha keys count mismatch.");
        }

        /// <summary>
        /// Ensures the texture is generated correctly and matches the requested size.
        /// </summary>
        [Test]
        public void TextureGradient_GeneratesTextureCorrectly()
        {
            // Arrange
            var colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(Color.red, 0f),
                new GradientColorKey(Color.green, 0.5f),
                new GradientColorKey(Color.blue, 1f)
            };

            var alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(1f, 1f)
            };

            var textureGradient = new TextureGradient(colorKeys, alphaKeys, GradientMode.PerceptualBlend,
                ColorSpace.Linear, 256);

            // Act
            var texture = textureGradient.GetTexture();

            // Assert
            Assert.NotNull(texture, "Generated texture should not be null.");
            Assert.AreEqual(256, texture.width, "Texture width should match the requested size.");
            Assert.AreEqual(1, texture.height, "Texture height should be 1 as it's a gradient texture.");
        }

        /// <summary>
        /// Confirms the Evaluate method correctly interpolates the gradient colors at specified points.
        /// </summary>
        [Test]
        public void TextureGradient_EvaluatesColorsCorrectly()
        {
            // Arrange
            var colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(Color.red, 0f),
                new GradientColorKey(Color.green, 0.5f),
                new GradientColorKey(Color.blue, 1f)
            };

            var alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(1f, 1f)
            };

            var textureGradient = new TextureGradient(colorKeys, alphaKeys);

            // Act
            var colorAtStart = textureGradient.Evaluate(0f);
            var colorAtMiddle = textureGradient.Evaluate(0.5f);
            var colorAtEnd = textureGradient.Evaluate(1f);

            // Assert
            Assert.AreEqual(Color.red, colorAtStart, "Color at start should match the first color key.");
            Assert.AreEqual(Color.green, colorAtMiddle, "Color at the middle should match the middle color key.");
            Assert.AreEqual(Color.blue, colorAtEnd, "Color at the end should match the last color key.");
        }

        /// <summary>
        /// Verifies that marking the texture as dirty causes it to regenerate on the next call.
        /// </summary>
        [Test]
        public void TextureGradient_SetDirty_MarksTextureDirty()
        {
            // Arrange
            var textureGradient = new TextureGradient(new GradientColorKey[0], new GradientAlphaKey[0]);

            // Act
            textureGradient.SetDirty();

            // Assert
            // Since m_IsTextureDirty is private, we check indirectly by calling GetTexture
            var texture = textureGradient.GetTexture();
            Assert.NotNull(texture, "Texture should have been generated after marking as dirty.");
        }
    }
}
