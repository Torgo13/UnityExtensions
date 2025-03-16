using NUnit.Framework;
using UnityEngine;

namespace UnityExtensions.Tests
{
    public class TileLayoutUtilsTests
    {
        /// <summary>
        /// Verifies that decomposition fails when the tile size is larger than the rectangle dimensions.
        /// </summary>
        [Test]
        public void TryLayoutByTiles_ReturnsFalse_WhenTileSizeExceedsRect()
        {
            // Arrange
            var rect = new RectInt(0, 0, 10, 10);
            uint tileSize = 20;

            // Act
            bool success = TileLayoutUtils.TryLayoutByTiles(rect, tileSize, out var main, out var topRow,
                out var rightCol, out var topRight);

            // Assert
            Assert.IsFalse(success, "Layout should fail when the tile size exceeds the rectangle's dimensions.");
            Assert.AreEqual(default(RectInt), main);
            Assert.AreEqual(default(RectInt), topRow);
            Assert.AreEqual(default(RectInt), rightCol);
            Assert.AreEqual(default(RectInt), topRight);
        }

        /// <summary>
        /// Confirms that the method correctly decomposes the rectangle into tiles when possible.
        /// </summary>
        [Test]
        public void TryLayoutByTiles_DecomposesRectCorrectly()
        {
            // Arrange
            var rect = new RectInt(0, 0, 20, 20);
            uint tileSize = 10;

            // Act
            bool success = TileLayoutUtils.TryLayoutByTiles(rect, tileSize, out var main, out var topRow,
                out var rightCol, out var topRight);

            // Assert
            Assert.IsTrue(success, "Layout should succeed when the rectangle's dimensions accommodate the tile size.");
            Assert.AreEqual(new RectInt(0, 0, 20, 20), main,
                "Main area should cover the entire rectangle when tiles fully fit.");
            Assert.AreEqual(new RectInt(0, 20, 20, 0), topRow,
                "Top row should have zero height when the rectangle's height matches the tile size.");
            Assert.AreEqual(new RectInt(20, 0, 0, 20), rightCol,
                "Right column should have zero width when the rectangle's width matches the tile size.");
            Assert.AreEqual(new RectInt(20, 20, 0, 0), topRight, "Top right corner should be empty.");
        }

        /// <summary>
        /// Ensures failure when the tile size exceeds the rectangle height for row decomposition.
        /// </summary>
        [Test]
        public void TryLayoutByRow_ReturnsFalse_WhenTileSizeExceedsRectHeight()
        {
            // Arrange
            var rect = new RectInt(0, 0, 20, 10);
            uint tileSize = 15;

            // Act
            bool success = TileLayoutUtils.TryLayoutByRow(rect, tileSize, out var main, out var other);

            // Assert
            Assert.IsFalse(success, "Layout should fail when the tile size exceeds the rectangle's height.");
            Assert.AreEqual(default(RectInt), main);
            Assert.AreEqual(default(RectInt), other);
        }

        /// <summary>
        /// Verifies that the row decomposition works properly when the rectangle can be divided.
        /// </summary>
        [Test]
        public void TryLayoutByRow_DecomposesRectCorrectly()
        {
            // Arrange
            var rect = new RectInt(0, 0, 20, 25);
            uint tileSize = 10;

            // Act
            bool success = TileLayoutUtils.TryLayoutByRow(rect, tileSize, out var main, out var other);

            // Assert
            Assert.IsTrue(success, "Layout should succeed when the rectangle's height accommodates the tile size.");
            Assert.AreEqual(new RectInt(0, 0, 20, 20), main, "Main area should consist of rows fitting the tile size.");
            Assert.AreEqual(new RectInt(0, 20, 20, 5), other, "Other area should cover the remaining height.");
        }

        /// <summary>
        /// Checks for failure when the tile size exceeds the rectangle width for column decomposition.
        /// </summary>
        [Test]
        public void TryLayoutByCol_ReturnsFalse_WhenTileSizeExceedsRectWidth()
        {
            // Arrange
            var rect = new RectInt(0, 0, 10, 20);
            uint tileSize = 15;

            // Act
            bool success = TileLayoutUtils.TryLayoutByCol(rect, tileSize, out var main, out var other);

            // Assert
            Assert.IsFalse(success, "Layout should fail when the tile size exceeds the rectangle's width.");
            Assert.AreEqual(default(RectInt), main);
            Assert.AreEqual(default(RectInt), other);
        }

        /// <summary>
        /// Confirms correct column decomposition when the rectangle can be divided.
        /// </summary>
        [Test]
        public void TryLayoutByCol_DecomposesRectCorrectly()
        {
            // Arrange
            var rect = new RectInt(0, 0, 25, 20);
            uint tileSize = 10;

            // Act
            bool success = TileLayoutUtils.TryLayoutByCol(rect, tileSize, out var main, out var other);

            // Assert
            Assert.IsTrue(success, "Layout should succeed when the rectangle's width accommodates the tile size.");
            Assert.AreEqual(new RectInt(0, 0, 20, 20), main,
                "Main area should consist of columns fitting the tile size.");
            Assert.AreEqual(new RectInt(20, 0, 5, 20), other, "Other area should cover the remaining width.");
        }
    }
}
