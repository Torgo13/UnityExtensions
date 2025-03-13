using NUnit.Framework;
using UnityEngine;

namespace UnityExtensions.Unsafe.Tests
{
    public class SpriteUtilitiesTests
    {
        [Test]
        public void CreateCircleSprite_ShouldCreateSpriteWithCorrectDimensionsAndColors()
        {
            int radius = 16;
            Color32 color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
            var sprite = ColorExtensions.CreateCircleSprite(radius, color);

            // Verify the sprite's texture has the correct dimensions
            var texture = sprite.texture;
            Assert.AreEqual(radius * 2, texture.width);
            Assert.AreEqual(radius * 2, texture.height);

            // Verify the color of the pixels within the circle
            var textureData = texture.GetRawTextureData<Color32>();
            var halfWidth = radius;
            var rSquared = radius * radius;

            for (int y = -radius; y < radius; y++)
            {
                var currentHalfWidth = (int)Mathf.Sqrt(rSquared - y * y);

                for (int x = -currentHalfWidth; x < currentHalfWidth; x++)
                {
                    int index = (y + radius) * (radius * 2) + (x + radius);
                    Assert.AreEqual(color, textureData[index]);
                }
            }
        }

        [Test]
        public void CreateCircleSprite_ShouldHandleZeroRadius()
        {
            int radius = 0;
            Color32 color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
            var sprite = ColorExtensions.CreateCircleSprite(radius, color);

            // Verify the sprite's texture has the correct dimensions
            var texture = sprite.texture;
            Assert.AreEqual(0, texture.width);
            Assert.AreEqual(0, texture.height);
        }
    }
}
