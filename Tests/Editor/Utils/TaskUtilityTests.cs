using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PKGE.Tests
{
    public class TaskUtilityTests
    {
        /// <summary>
        /// Validates that tasks are properly distributed and executed across multiple threads,
        /// with results aggregated into the ConcurrentBag.
        /// </summary>
        [Test]
        public void RunTasks_DistributesWorkAcrossThreads()
        {
            // Arrange
            var items = Enumerable.Range(1, 100).ToList();
            var results = new ConcurrentBag<int>();

            // Act
            var outputs = TaskUtility.RunTasks<int, int>(
                items,
                (item, resultBag) =>
                {
                    resultBag.Add(item * item); // Square the number
                });

            // Assert
            Assert.AreEqual(items.Count, outputs.Count(), "The output count should match the number of input items.");
            foreach (var item in items)
            {
                Assert.Contains(item * item, outputs.ToList(), "Each squared value should be present in the results.");
            }
        }

        /// <summary>
        /// Ensures that the method handles an empty input list gracefully by returning an empty result set.
        /// </summary>
        [Test]
        public void RunTasks_HandlesEmptyInputList()
        {
            // Arrange
            var items = new List<int>();
            var results = new ConcurrentBag<int>();

            // Act
            var outputs = TaskUtility.RunTasks<int, int>(
                items,
                (item, resultBag) =>
                {
                    resultBag.Add(item * 2); // Double the number
                });

            // Assert
            Assert.IsEmpty(outputs, "The output should be empty for an empty input list.");
        }

        /// <summary>
        /// Confirms that items are processed correctly using multiple threads.
        /// </summary>
        [Test]
        public void RunTasks_ProcessesItemsCorrectly_WithMultipleThreads()
        {
            // Arrange
            var items = Enumerable.Range(1, 50).ToList();
            var results = new ConcurrentBag<string>();

            // Act
            var outputs = TaskUtility.RunTasks<int, string>(
                items,
                (item, resultBag) => { resultBag.Add($"Processed-{item}"); });

            // Assert
            Assert.AreEqual(items.Count, outputs.Count(),
                "The number of results should match the number of input items.");
            foreach (var item in items)
            {
                Assert.Contains($"Processed-{item}", outputs.ToList(),
                    "Each processed string should match the expected format.");
            }
        }

        /// <summary>
        /// Validates that custom logic (like filtering) is executed correctly for each item.
        /// </summary>
        [Test]
        public void RunTasks_SupportsCustomActions()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5 };
            var results = new ConcurrentBag<int>();

            // Act
            var outputs = TaskUtility.RunTasks<int, int>(
                items,
                (item, resultBag) =>
                {
                    if (item % 2 == 0) // Only add even numbers
                    {
                        resultBag.Add(item);
                    }
                });

            // Assert
            var expectedResults = new List<int> { 2, 4 };
            Assert.AreEqual(expectedResults.Count, outputs.Count(),
                "The number of results should match the filtered even numbers.");
            foreach (var expected in expectedResults)
            {
                Assert.Contains(expected, outputs.ToList(), "Each even number should be present in the results.");
            }
        }

        /// <summary>
        /// Ensures that any exceptions thrown within the tasks are aggregated and properly reported.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        [Test]
        public void RunTasks_HandlesExceptionsGracefully()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3 };
            var results = new ConcurrentBag<int>();

            // Act & Assert
            Assert.Throws<AggregateException>(() =>
            {
                TaskUtility.RunTasks<int, int>(
                    items,
                    (item, resultBag) =>
                    {
                        if (item == 2)
                        {
                            throw new InvalidOperationException("Test exception for item 2.");
                        }

                        resultBag.Add(item * 3);
                    });
            }, "An AggregateException should be thrown if any task encounters an exception.");
        }
    }
}
