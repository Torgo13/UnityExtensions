using NUnit.Framework;
using System;

namespace UnityExtensions.Tests
{
    public class SortingTests
    {
        /// <summary>
        /// Validates that the method sorts an unsorted array correctly.
        /// </summary>
        [Test]
        public void QuickSort_SortsArrayCorrectly()
        {
            // Arrange
            var array = new[] { 5, 3, 8, 4, 1, 7, 2, 6 };
            var expected = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            // Act
            Sorting.QuickSort(array, (a, b) => a.CompareTo(b));

            // Assert
            Assert.AreEqual(expected, array, "QuickSort should sort the array in ascending order.");
        }

        /// <summary>
        /// Ensures the method handles empty arrays gracefully.
        /// </summary>
        [Test]
        public void QuickSort_SortsEmptyArray()
        {
            // Arrange
            var array = Array.Empty<int>();

            // Act
            Sorting.QuickSort(array, (a, b) => a.CompareTo(b));

            // Assert
            Assert.IsEmpty(array, "QuickSort should handle empty arrays without errors.");
        }

        /// <summary>
        /// Confirms the behavior for arrays with a single element.
        /// </summary>
        [Test]
        public void QuickSort_SortsSingleElementArray()
        {
            // Arrange
            var array = new[] { 42 };

            // Act
            Sorting.QuickSort(array, (a, b) => a.CompareTo(b));

            // Assert
            Assert.AreEqual(new[] { 42 }, array, "QuickSort should handle single-element arrays correctly.");
        }

        /// <summary>
        /// Tests sorting of a descending array to ascending order.
        /// </summary>
        [Test]
        public void QuickSort_SortsDescendingArray()
        {
            // Arrange
            var array = new[] { 8, 7, 6, 5, 4, 3, 2, 1 };
            var expected = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            // Act
            Sorting.QuickSort(array, (a, b) => a.CompareTo(b));

            // Assert
            Assert.AreEqual(expected, array, "QuickSort should sort a descending array into ascending order.");
        }

        /// <summary>
        /// Verifies that custom comparison logic works as expected.
        /// </summary>
        [Test]
        public void QuickSort_SortsWithCustomComparison()
        {
            // Arrange
            var array = new[] { "banana", "apple", "pineapple" };
            var expected = new[] { "pineapple", "banana", "apple" };

            // Act
            Sorting.QuickSort(array, (a, b) => b.Length.CompareTo(a.Length));

            // Assert
            Assert.AreEqual(expected, array, "QuickSort should sort the array using the custom comparison function.");
        }

        /// <summary>
        /// Similar to QuickSort, tests sorting functionality.
        /// </summary>
        [Test]
        public void InsertionSort_SortsArrayCorrectly()
        {
            // Arrange
            var array = new[] { 5, 3, 8, 4, 1, 7, 2, 6 };
            var expected = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            // Act
            Sorting.InsertionSort(array, (a, b) => a.CompareTo(b));

            // Assert
            Assert.AreEqual(expected, array, "InsertionSort should sort the array in ascending order.");
        }

        /// <summary>
        /// Ensures it correctly sorts a specific range within an array.
        /// </summary>
        [Test]
        public void InsertionSort_SortsPartiallySortedArray()
        {
            // Arrange
            var array = new[] { 1, 2, 3, 8, 5, 7 };
            var expected = new[] { 1, 2, 3, 5, 7, 8 };

            // Act
            Sorting.InsertionSort(array, 3, 5, (a, b) => a.CompareTo(b));

            // Assert
            Assert.AreEqual(expected, array, "InsertionSort should sort the specified range correctly.");
        }

        /// <summary>
        /// Ensures custom comparison logic works in InsertionSort.
        /// </summary>
        [Test]
        public void InsertionSort_SortsWithCustomComparison()
        {
            // Arrange
            var array = new[] { "banana", "apple", "pineapple" };
            var expected = new[] { "pineapple", "banana", "apple" };

            // Act
            Sorting.InsertionSort(array, (a, b) => b.Length.CompareTo(a.Length));

            // Assert
            Assert.AreEqual(expected, array,
                "InsertionSort should sort the array using the custom comparison function.");
        }
    }
}
