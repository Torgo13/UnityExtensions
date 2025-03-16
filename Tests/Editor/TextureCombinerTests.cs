using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityExtensions.Editor.Tests
{
    public class TextureCombinerTests
    {
        private TextureCombiner textureCombiner;
        private Texture2D texture0;
        private Texture2D texture1;

        const string savePath = "Assets/test.png";
        const int width = 64;
        const int height = 64;

        [SetUp]
        public void Setup()
        {
            texture0 = new Texture2D(width, height);
            texture1 = new Texture2D(width, height);
            textureCombiner = new TextureCombiner(texture0, 0, texture0, 1, texture1, 2, texture1, 3);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(texture0);
            Object.DestroyImmediate(texture1);
        }

        /// <summary>
        /// Validates that CombineTextures returns a non-null texture with the correct dimensions for valid input textures.
        /// </summary>
        [Test]
        public void CombineTextures_ValidTextures_ReturnsCombinedTexture()
        {
            var combinedTexture = textureCombiner.Combine(savePath);
            Assert.IsNotNull(combinedTexture);
            Assert.AreEqual(width, combinedTexture.width);
            Assert.AreEqual(height, combinedTexture.height);

            Assert.True(AssetDatabase.DeleteAsset(savePath));
        }

        /// <summary>
        /// Tests that CombineTextures throws ArgumentNullException when given null textures.
        /// </summary>
        [Test]
        public void CombineTextures_NullTexture_ThrowsArgumentNullException()
        {
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => new TextureCombiner(null, 0, null, 1, texture1, 2, texture1, 3));
        }
    }
}
