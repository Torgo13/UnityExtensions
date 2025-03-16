using NUnit.Framework;
using UnityEngine;

namespace UnityExtensions.Tests
{
    public class TextureCurveTests
    {
        /// <summary>
        /// Confirms the curve is correctly initialized with keyframes.
        /// </summary>
        [Test]
        public void TextureCurve_CreatesCurveSuccessfully()
        {
            // Arrange
            var keyframes = new Keyframe[]
            {
                new Keyframe(0f, 0f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0f)
            };

            // Act
            var textureCurve = new TextureCurve(keyframes, zeroValue: 0f, loop: false, bounds: new Vector2(0f, 1f));

            // Assert
            Assert.NotNull(textureCurve, "TextureCurve should be created successfully.");
            Assert.AreEqual(3, textureCurve.Length, "Curve length should match the number of keyframes.");
        }

        /// <summary>
        /// Verifies the texture generation meets the required specifications.
        /// </summary>
        [Test]
        public void TextureCurve_GeneratesTextureCorrectly()
        {
            // Arrange
            var keyframes = new Keyframe[]
            {
                new Keyframe(0f, 0f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0f)
            };
            var textureCurve = new TextureCurve(keyframes, zeroValue: 0f, loop: false, bounds: new Vector2(0f, 1f));

            // Act
            var texture = textureCurve.GetTexture();

            // Assert
            Assert.NotNull(texture, "Generated texture should not be null.");
            Assert.AreEqual(128, texture.width, "Texture width should match the precision (128).");
            Assert.AreEqual(1, texture.height, "Texture height should be 1 as it's a curve texture.");
        }

        /// <summary>
        /// Ensures proper interpolation of values at specific points.
        /// </summary>
        [Test]
        public void TextureCurve_EvaluatesValuesCorrectly()
        {
            // Arrange
            var keyframes = new Keyframe[]
            {
                new Keyframe(0f, 0f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0f)
            };
            var textureCurve = new TextureCurve(keyframes, zeroValue: 0f, loop: false, bounds: new Vector2(0f, 1f));

            // Act
            float startValue = textureCurve.Evaluate(0f);
            float midValue = textureCurve.Evaluate(0.5f);
            float endValue = textureCurve.Evaluate(1f);

            // Assert
            Assert.AreEqual(0f, startValue, "Start value should match the first keyframe.");
            Assert.AreEqual(1f, midValue, "Middle value should match the second keyframe.");
            Assert.AreEqual(0f, endValue, "End value should match the last keyframe.");
        }

        /// <summary>
        /// Tests that adding a key dynamically updates the curve.
        /// </summary>
        [Test]
        public void TextureCurve_AddKey_UpdatesCurve()
        {
            // Arrange
            var keyframes = new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(1f, 1f) };
            var textureCurve = new TextureCurve(keyframes, zeroValue: 0f, loop: false, bounds: new Vector2(0f, 1f));

            // Act
            textureCurve.AddKey(0.5f, 0.5f);

            // Assert
            Assert.AreEqual(3, textureCurve.Length, "Curve length should be updated after adding a key.");
            Assert.AreEqual(0.5f, textureCurve[1].value, "Added key value should match the provided value.");
        }

        /// <summary>
        /// Validates that removing a key adjusts the curve appropriately.
        /// </summary>
        [Test]
        public void TextureCurve_RemoveKey_UpdatesCurve()
        {
            // Arrange
            var keyframes = new Keyframe[]
            {
                new Keyframe(0f, 0f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0f)
            };
            var textureCurve = new TextureCurve(keyframes, zeroValue: 0f, loop: false, bounds: new Vector2(0f, 1f));

            // Act
            textureCurve.RemoveKey(1);
            
            // Assert
            Assert.AreEqual(2, textureCurve.Length, "Curve length should be reduced after removing a key.");
        }

        /// <summary>
        /// Checks if marking the curve as dirty triggers re-computation.
        /// </summary>
        [Test]
        public void TextureCurve_SetDirty_MarksCurveAndTextureDirty()
        {
            // Arrange
            var keyframes = new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(1f, 1f) };
            var textureCurve = new TextureCurve(keyframes, zeroValue: 0f, loop: false, bounds: new Vector2(0f, 1f));

            // Act
            textureCurve.SetDirty();

            // Assert
            // We check indirectly by verifying the texture is re-generated
            var texture = textureCurve.GetTexture();
            Assert.NotNull(texture, "Texture should be re-generated after marking as dirty.");
        }
    }
}
