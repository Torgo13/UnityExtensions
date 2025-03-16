using NUnit.Framework;
using System.Threading;
using UnityExtensions.Editor.Unsafe;

namespace UnityExtensions.Unsafe.Tests
{
    class TimedScopeTests
    {
        /// <summary>
        /// This test uses the TimedScope.FromPtr method, passing a pointer to a double.
        /// It verifies that the elapsed duration is recorded correctly.
        /// </summary>
        [Test]
        public unsafe void TimedScope_FromPtr_UpdatesDuration()
        {
            // Arrange
            double duration = 0;
            double* durationPtr = &duration;

            // Act
            using (TimedScope.FromPtr(durationPtr))
            {
                Thread.Sleep(100); // Simulate work
            }

            // Assert
            Assert.That(duration, Is.GreaterThanOrEqualTo(100).And.LessThan(200),
                "Duration should reflect the elapsed time accurately.");
        }

        /// <summary>
        /// This test validates the TimedScope.FromRef method using a ref parameter.
        /// </summary>
        [Test]
        public void TimedScope_FromRef_UpdatesDuration()
        {
            // Arrange
            double duration = 0;

            // Act
            using (TimedScope.FromRef(ref duration))
            {
                Thread.Sleep(100); // Simulate work
            }

            // Assert
            Assert.That(duration, Is.GreaterThanOrEqualTo(100).And.LessThan(200),
                "Duration should reflect the elapsed time accurately.");
        }
    }
}
