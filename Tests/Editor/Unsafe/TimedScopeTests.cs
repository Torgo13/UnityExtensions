using NUnit.Framework;
using System.Threading;
using PKGE.Editor.Unsafe;

namespace PKGE.Unsafe.Tests
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
                Thread.Sleep(150); // Simulate work
            }

            // Assert
            Assert.That(duration, Is.GreaterThanOrEqualTo(100).And.LessThan(200),
                "Duration should reflect the elapsed time accurately.");
        }

        /// <summary>
        /// This test validates the TimedScope.From method using a NativeArray.
        /// </summary>
        [Test]
        public void TimedScope_From_UpdatesDuration()
        {
            // Arrange
            using var duration = new Unity.Collections.NativeArray<double>(1,
                Unity.Collections.Allocator.TempJob);

            // Act
            using (TimedScope.From(duration))
            {
                Thread.Sleep(150); // Simulate work
            }

            // Assert
            Assert.That(duration[0], Is.GreaterThanOrEqualTo(100).And.LessThan(200),
                "Duration should reflect the elapsed time accurately.");
        }
    }
}
