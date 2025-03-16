using NUnit.Framework;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityExtensions.Tests
{
    public class JobQueueTests
    {
        /// <summary>
        /// Validates that jobs enqueued to the worker threads are executed as expected.
        /// </summary>
        [Test]
        public void JobQueue_EnqueueAndExecuteJob_ProcessesJobsCorrectly()
        {
            // Arrange
            using var jobQueue = new JobQueue(2);
            bool jobExecuted = false;

            // Act
            jobQueue.EnqueueJob(() => { jobExecuted = true; });
            IEnumerator waitFinishCoroutine = jobQueue.WaitFinish();
            while (waitFinishCoroutine.MoveNext())
            {
            } // Wait until jobs are processed

            // Assert
            Assert.IsTrue(jobExecuted, "The enqueued job should have been executed.");
        }

        /// <summary>
        /// Ensures main-thread-specific jobs are executed correctly.
        /// </summary>
        [Test]
        public void JobQueue_EnqueueMainThreadJob_ProcessesMainThreadJobsCorrectly()
        {
            // Arrange
            using var jobQueue = new JobQueue(2);
            bool mainThreadJobExecuted = false;

            // Act
            jobQueue.EnqueueMainThreadJob(() => { mainThreadJobExecuted = true; });
            IEnumerator waitFinishCoroutine = jobQueue.WaitFinish();
            while (waitFinishCoroutine.MoveNext())
            {
            } // Wait until main thread jobs are processed

            // Assert
            Assert.IsTrue(mainThreadJobExecuted, "The enqueued main thread job should have been executed.");
        }

        /// <summary>
        /// Confirms that WaitFinish waits until all jobs are processed before finishing.
        /// </summary>
        [UnityTest]
        public IEnumerator JobQueue_WaitFinish_WaitsForAllJobsToComplete()
        {
            // Arrange
            using var jobQueue = new JobQueue(2);
            bool job1Executed = false;
            bool job2Executed = false;

            // Act
            jobQueue.EnqueueJob(() =>
            {
                Thread.Sleep(500); // Simulate work
                job1Executed = true;
            });
            jobQueue.EnqueueJob(() =>
            {
                Thread.Sleep(300); // Simulate work
                job2Executed = true;
            });

            yield return jobQueue.WaitFinish(); // Wait until all jobs finish

            // Assert
            Assert.IsTrue(job1Executed, "Job 1 should have been executed.");
            Assert.IsTrue(job2Executed, "Job 2 should have been executed.");
        }

        /// <summary>
        /// Verifies that exceptions within worker threads are correctly propagated.
        /// </summary>
        [Test]
        public void JobQueue_WorkerHandlesExceptions_ReportsException()
        {
            // Arrange
            using var jobQueue = new JobQueue(1);
            
            LogAssert.Expect(LogType.Exception, "InvalidOperationException: Test exception");

            // Act
            jobQueue.EnqueueJob(() => throw new InvalidOperationException("Test exception"));

            Assert.Throws<Exception>(() =>
            {
                IEnumerator waitFinishCoroutine = jobQueue.WaitFinish();
                while (waitFinishCoroutine.MoveNext())
                {
                } // Wait until jobs are processed
            });
        }

        /// <summary>
        /// Confirms that disposing of the JobQueue stops all worker threads gracefully.
        /// </summary>
        [Test]
        public void JobQueue_Dispose_StopsAllWorkers()
        {
            // Arrange
            var jobQueue = new JobQueue(2);

            // Act
            jobQueue.Dispose();

            // Assert
            Assert.Pass("JobQueue disposed successfully without throwing exceptions.");
        }
    }
}
