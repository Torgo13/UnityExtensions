using NUnit.Framework;
using System;
using System.IO;
using UnityEngine;

namespace PKGE.Tests
{
    public class ScreenshotTests
    {
        /// <summary>
        /// Ensures that passing a null camera throws an appropriate exception.
        /// </summary>
        [Test]
        public void Screenshot_Take_ThrowsArgumentNullException_WhenCameraIsNull()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => Screenshot.Take(null));
            Assert.AreEqual("camera", ex.ParamName, "The exception should indicate the parameter name 'camera'.");
        }

        /// <summary>
        /// Validates that the returned texture has dimensions matching the scaled camera resolution.
        /// </summary>
        [Test]
        public void Screenshot_Take_ReturnsTexture_WithExpectedDimensions()
        {
            // Arrange
            var camera = new GameObject("TestCamera").AddComponent<Camera>();
            //camera.pixelWidth = 1920;
            //camera.pixelHeight = 1080;
            const float scale = 0.5f;

            // Act
            var texture = Screenshot.Take(camera, scale);

            // Assert
            Assert.NotNull(texture, "The returned texture should not be null.");
            Assert.AreEqual(Mathf.RoundToInt(camera.pixelWidth * scale), texture.width, "Texture width should match the scaled camera width.");
            Assert.AreEqual(Mathf.RoundToInt(camera.pixelHeight * scale), texture.height, "Texture height should match the scaled camera height.");

            UnityEngine.Object.DestroyImmediate(camera.gameObject);
        }

        #region PNG
        /// <summary>
        /// Confirms that passing a null texture to SaveAsPNG throws an exception.
        /// </summary>
        [Test]
        public void Screenshot_SaveAsPNG_ThrowsArgumentNullException_WhenTextureIsNull()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => Screenshot.SaveAsPNG(null, "screenshot", "Assets"));
            Assert.AreEqual("texture", ex.ParamName, "The exception should indicate the parameter name 'texture'.");
        }

        /// <summary>
        /// Verifies that the PNG is saved to the correct location and with the correct name.
        /// </summary>
        [Test]
        public void Screenshot_SaveAsPNG_SavesFileToExpectedLocation()
        {
            // Arrange
            var directory = Path.Combine(Application.temporaryCachePath, "TestScreenshots");
            var filename = "test_screenshot";
            var texture = new Texture2D(100, 100);

            // Act
            var path = Screenshot.SaveAsPNG(texture, filename, directory);

            // Assert
            Assert.IsTrue(File.Exists(path), "The file should exist at the specified path.");
            Assert.IsTrue(path.EndsWith($"{filename}.png"),
                "The saved file should have the correct filename and extension.");

            // Cleanup
            File.Delete(path);
            Directory.Delete(directory);
        }

        /// <summary>
        /// Tests the combined functionality of Take and SaveAsPNG, ensuring the pipeline works end-to-end.
        /// </summary>
        [Test]
        public void Screenshot_TakeAndSaveAsPNG_CombinesCorrectly()
        {
            // Arrange
            var camera = new GameObject("TestCamera").AddComponent<Camera>();
            //camera.pixelWidth = 1920;
            //camera.pixelHeight = 1080;
            var directory = Path.Combine(Application.temporaryCachePath, "TestScreenshots");
            var filename = "combined_test_screenshot";

            // Act
            var texture = Screenshot.Take(camera);
            var path = Screenshot.SaveAsPNG(texture, filename, directory);

            // Assert
            Assert.NotNull(texture, "The texture from Screenshot.Take should not be null.");
            Assert.IsTrue(File.Exists(path), "The saved file should exist at the specified path.");

            // Cleanup
            File.Delete(path);
            Directory.Delete(directory);
            UnityEngine.Object.DestroyImmediate(camera.gameObject);
        }
        #endregion // PNG

        #region EXR
        /// <summary>
        /// Confirms that passing a null texture to SaveAsEXR throws an exception.
        /// </summary>
        [Test]
        public void Screenshot_SaveAsEXR_ThrowsArgumentNullException_WhenTextureIsNull()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => Screenshot.SaveAsEXR(null, "screenshot", "Assets"));
            Assert.AreEqual("texture", ex.ParamName, "The exception should indicate the parameter name 'texture'.");
        }

        /// <summary>
        /// Verifies that the EXR is saved to the correct location and with the correct name.
        /// </summary>
        [Test]
        public void Screenshot_SaveAsEXR_SavesFileToExpectedLocation()
        {
            // Arrange
            var directory = Path.Combine(Application.temporaryCachePath, "TestScreenshots");
            var filename = "test_screenshot";
            var texture = new Texture2D(100, 100);

            // Act
            var path = Screenshot.SaveAsEXR(texture, filename, directory);

            // Assert
            Assert.IsTrue(File.Exists(path), "The file should exist at the specified path.");
            Assert.IsTrue(path.EndsWith($"{filename}.exr"),
                "The saved file should have the correct filename and extension.");

            // Cleanup
            File.Delete(path);
            Directory.Delete(directory);
        }

        /// <summary>
        /// Tests the combined functionality of Take and SaveAsEXR, ensuring the pipeline works end-to-end.
        /// </summary>
        [Test]
        public void Screenshot_TakeAndSaveAsEXR_CombinesCorrectly()
        {
            // Arrange
            var camera = new GameObject("TestCamera").AddComponent<Camera>();
            //camera.pixelWidth = 1920;
            //camera.pixelHeight = 1080;
            var directory = Path.Combine(Application.temporaryCachePath, "TestScreenshots");
            var filename = "combined_test_screenshot";

            // Act
            var texture = Screenshot.Take(camera, hdr: true);
            var path = Screenshot.SaveAsEXR(texture, filename, directory);

            // Assert
            Assert.NotNull(texture, "The texture from Screenshot.Take should not be null.");
            Assert.IsTrue(File.Exists(path), "The saved file should exist at the specified path.");

            // Cleanup
            File.Delete(path);
            Directory.Delete(directory);
            UnityEngine.Object.DestroyImmediate(camera.gameObject);
        }
        #endregion // EXR
    }
}
