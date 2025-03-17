using NUnit.Framework;
using UnityEngine;

namespace UnityExtensions.Editor.Tests
{
    public class SpriteMeshGeneratorTests
    {
        private SpriteMeshGenerator _generator;

        [SetUp]
        public void Setup()
        {
            // Create a ScriptableObject instance for testing
            _generator = ScriptableObject.CreateInstance<SpriteMeshGenerator>();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up after tests
            Object.DestroyImmediate(_generator);
        }

        /// <summary>
        /// Check behavior when sprites is null or empty.
        /// </summary>
        [Test]
        public void OnValidate_SpritesArrayNullOrEmpty_DoesNotGenerateHash()
        {
            _generator.sprites = null;
            _generator.OnValidate();

            Assert.IsNull(_generator.hash);

            _generator.sprites = new Sprite[0];
            _generator.OnValidate();

            Assert.IsNull(_generator.hash);
        }

        /*
        /// <summary>
        /// Ensure the hash is updated and GenerateAndSaveMesh is invoked when sprites are valid.
        /// </summary>
        [Test]
        public void OnValidate_ValidSprites_UpdatesHashAndGeneratesMesh()
        {
            const int textureSize = 256;
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
                ColorSpace.Linear, textureSize);

            var texture = textureGradient.GetTexture();
            
            // Create mock sprites
            var sprite0 = Sprite.Create(texture, new Rect(0f, 0f, textureSize * 0.5f, 1),
                Vector2.zero);
            var sprite1 = Sprite.Create(texture, new Rect(textureSize * 0.5f, 0f, textureSize * 0.5f, 1),
                Vector2.zero);

            _generator.sprites = new[] { sprite0, sprite1 };

            // Simulate hash update and mesh generation
            _generator.OnValidate();

            Assert.IsNotNull(_generator.hash);
            Assert.IsTrue(_generator.hash.Length > 0);
        }
        */

        /*
        /// <summary>
        /// Verify that all sub-assets are removed except the main ScriptableObject.
        /// </summary>
        [Test]
        public void CleanSubAssets_RemovesOldAssets_LeavesSelfUntouched()
        {
            _generator.sprites = new Sprite[1]; // Example placeholder

            // Simulate adding assets
            var assetPath = AssetDatabase.GetAssetPath(_generator);
            AssetDatabase.AddObjectToAsset(ScriptableObject.CreateInstance<Sprite>(), assetPath);

            _generator.CleanSubAssets();

            var subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            Assert.IsTrue(subAssets.All(sub => sub == _generator));
        }
        */

        /// <summary>
        /// Confirm that a mesh is generated with the correct properties when given a valid Sprite.
        /// </summary>
        [Test]
        public void GenerateMeshFromSprite_ReturnsValidMesh()
        {
            const int textureSize = 256;
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
                ColorSpace.Linear, textureSize);

            var texture = textureGradient.GetTexture();
            
            // Create mock sprite
            var sprite0 = Sprite.Create(texture, new Rect(0f, 0f, textureSize, 1), Vector2.zero);
                //AssetDatabase.LoadAssetAtPath<Sprite>("path/to/sprite0.png");

            var mesh = sprite0.GenerateMeshFromSprite();

            Assert.IsNotNull(mesh);
            Assert.AreEqual(mesh.name, sprite0.name.Replace('.', '_'));
            Assert.IsTrue(mesh.vertices.Length > 0);
            Assert.IsTrue(mesh.triangles.Length > 0);
        }
    }
}
