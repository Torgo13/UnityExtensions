using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace PKGE.Tests
{
    public class CollectionExtensionsTests
    {
        /// <summary>
        /// Verifies that elements are inserted into the correct positions in presorted lists.
        /// </summary>
        [Test]
        public void AddSorted_AddsElementInCorrectPosition_InPresortedList()
        {
            // Arrange
            var list = new List<int> { 1, 3, 5, 7 };

            // Act
            list.AddSorted(4);

            // Assert
            Assert.AreEqual(new List<int> { 1, 3, 4, 5, 7 }, list,
                "AddSorted should insert the element at the correct position.");
        }

        /// <summary>
        /// Checks adding to an empty list.
        /// </summary>
        [Test]
        public void AddSorted_AddsElementToEmptyList()
        {
            // Arrange
            var list = new List<int>();

            // Act
            list.AddSorted(10);

            // Assert
            Assert.AreEqual(new List<int> { 10 }, list, "AddSorted should handle adding to an empty list.");
        }

        /// <summary>
        /// Checks handling of a null list.
        /// </summary>
        [Test]
        public void AddSorted_ThrowsException_IfListIsNull()
        {
            // Arrange
            List<int> list = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => list.AddSorted(10));
            Assert.AreEqual(nameof(list), ex.ParamName);
        }

        /// <summary>
        /// Confirms the correct number of elements are added.
        /// </summary>
        [Test]
        public void Fill_AddsCorrectNumberOfElements()
        {
            // Arrange
            var list = new List<int>();

            // Act
            list.Fill(5, 3);

            // Assert
            Assert.AreEqual(new List<int> { 5, 5, 5 }, list,
                "Fill should add the specified value the correct number of times.");
        }

        /// <summary>
        /// Ensures an exception is thrown if the list is null.
        /// </summary>
        [Test]
        public void Fill_ThrowsException_IfListIsNull()
        {
            // Arrange
            List<int> list = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => list.Fill(1, 5));
        }

        /// <summary>
        /// Validates that the method returns the smallest element.
        /// </summary>
        [Test]
        public void FirstOrDefaultSorted_ReturnsSmallestElement()
        {
            // Arrange
            var collection = new List<int> { 5, 2, 8, 1, 3 };

            // Act
            var result = collection.FirstOrDefaultSorted();

            // Assert
            Assert.AreEqual(1, result, "FirstOrDefaultSorted should return the smallest element in the collection.");
        }

        /// <summary>
        /// Ensures proper exception handling when the collection is null.
        /// </summary>
        [Test]
        public void FirstOrDefaultSorted_ThrowsException_IfCollectionIsNull()
        {
            // Arrange
            List<int> collection = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => collection.FirstOrDefaultSorted());
        }

        /// <summary>
        /// Verifies that the collection is serialized to a correct JSON-compatible string.
        /// </summary>
        [Test]
        public void SerializedView_ReturnsExpectedStringRepresentation()
        {
            // Arrange
            var collection = new List<int> { 1, 2, 3 };

            // Act
            var result = collection.SerializedView(i => i.ToString());

            // Assert
            Assert.AreEqual("[1,2,3]", result, "SerializedView should return a JSON-compatible string representation.");
        }

        /// <summary>
        /// Tests for exceptions when the collection is null.
        /// </summary>
        [Test]
        public void SerializedView_ThrowsException_IfCollectionIsNull()
        {
            // Arrange
            List<int> collection = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => collection.SerializedView(i => i.ToString()));
        }

        /// <summary>
        /// Tests for exceptions when the serialization function is null.
        /// </summary>
        [Test]
        public void SerializedView_ThrowsException_IfSerializeElementIsNull()
        {
            // Arrange
            var collection = new List<int> { 1, 2, 3 };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => collection.SerializedView(null));
        }

        /// <summary>
        /// Checks if the method correctly identifies the presence of an element.
        /// </summary>
        [Test]
        public void ContainsByEquals_ReturnsTrue_IfElementIsPresent()
        {
            // Arrange
            var collection = new List<int> { 1, 2, 3, 4, 5 };

            // Act
            var result = collection.ContainsByEquals(3);

            // Assert
            Assert.IsTrue(result, "ContainsByEquals should return true if the element is present in the collection.");
        }

        /// <summary>
        /// Checks if the method correctly identifies the absence of an element.
        /// </summary>
        [Test]
        public void ContainsByEquals_ReturnsFalse_IfElementIsAbsent()
        {
            // Arrange
            var collection = new List<int> { 1, 2, 3, 4, 5 };

            // Act
            var result = collection.ContainsByEquals(6);

            // Assert
            Assert.IsFalse(result,
                "ContainsByEquals should return false if the element is not present in the collection.");
        }

        /// <summary>
        /// Ensures exceptions are thrown if the collection is null.
        /// </summary>
        [Test]
        public void ContainsByEquals_ThrowsException_IfCollectionIsNull()
        {
            // Arrange
            List<int> collection = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => collection.ContainsByEquals(1));
        }
    }
}
