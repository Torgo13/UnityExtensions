using NUnit.Framework;
using UnityEngine;

namespace UnityExtensions.Tests
{
    public class CreateTexture2DArrayAssetTests
    {
        private CreateTexture2DArrayAsset _creator;

        [SetUp]
        public void Setup()
        {
            // Create a MonoBehaviour instance for testing
            var gameObject = new GameObject("CreateTexture2DArrayAssetObject");
            _creator = gameObject.AddComponent<CreateTexture2DArrayAsset>();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up after tests
            Object.DestroyImmediate(_creator.gameObject);
        }

        /// <summary>
        /// Verifies the creation of a valid Texture2DArray when valid textures are provided.
        /// </summary>
        [Test]
        public void CreateTexture2DAsset_WithValidTextures_CreatesTextureArray()
        {
            // Setup textures
            const int width = 128;
            const int height = 128;
            var texture1 = new Texture2D(width, height, TextureFormat.ARGB32, false);
            var texture2 = new Texture2D(width, height, TextureFormat.ARGB32, false);
            _creator.textures = new[] { texture1, texture2 };

            // Invoke the method
            _creator.CreateTexture2DAsset();

            // Verify asset creation
            var arrayAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2DArray>("Assets/TextureArray.asset");
            Assert.IsNotNull(arrayAsset);
            Assert.AreEqual(width, arrayAsset.width);
            Assert.AreEqual(height, arrayAsset.height);
            Assert.AreEqual(2, arrayAsset.depth);
            UnityEditor.AssetDatabase.DeleteAsset("Assets/TextureArray.asset");
        }

        /// <summary>
        /// Verifies the creation of a valid CubemapArray when valid cubeMaps are provided.
        /// </summary>
        [Test]
        public void CreateCubeArrayAsset_WithValidCubeMaps_CreatesCubemapArray()
        {
            // Setup cubeMaps
            const int width = 128;
            var cubemap1 = new Cubemap(width, TextureFormat.ARGB32, false);
            var cubemap2 = new Cubemap(width, TextureFormat.ARGB32, false);
            _creator.cubeMaps = new[] { cubemap1, cubemap2 };

            // Invoke the method
            _creator.CreateCubeArrayAsset();

            // Verify asset creation
            var arrayAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<CubemapArray>("Assets/CubemapArray.asset");
            Assert.IsNotNull(arrayAsset);
            Assert.AreEqual(width, arrayAsset.width);
            Assert.AreEqual(2, arrayAsset.cubemapCount);
            UnityEditor.AssetDatabase.DeleteAsset("Assets/CubemapArray.asset");
        }

        /// <summary>
        /// Checks that no array is created if the textures array is empty.
        /// </summary>
        [Test]
        public void CreateTexture2DAsset_WithEmptyTextures_DoesNotCreateArray()
        {
            // Set textures to empty
            _creator.textures = new Texture2D[0];

            // Invoke the method
            _creator.CreateTexture2DAsset();

            // Verify no asset is created
            var arrayAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2DArray>("Assets/TextureArray.asset");
            Assert.IsNull(arrayAsset);
        }

        /// <summary>
        /// Ensures no array is created if the cubeMaps array is empty.
        /// </summary>
        [Test]
        public void CreateCubeArrayAsset_WithEmptyCubeMaps_DoesNotCreateArray()
        {
            // Set cubeMaps to empty
            _creator.cubeMaps = new Cubemap[0];

            // Invoke the method
            _creator.CreateCubeArrayAsset();

            // Verify no asset is created
            var arrayAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<CubemapArray>("Assets/CubemapArray.asset");
            Assert.IsNull(arrayAsset);
        }
    }
}
