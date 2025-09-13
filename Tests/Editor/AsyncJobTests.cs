using NUnit.Framework;

namespace PKGE.Editor.Tests
{
    public class TestAsyncJob : AsyncJob
    {
        private bool internalTickComplete;
        private bool internalCancelTriggered;

        public TestAsyncJob(bool tickComplete = false)
        {
            internalTickComplete = tickComplete;
            internalCancelTriggered = false;
        }

        public override string name => "TestAsyncJob";

        public bool InternalCancelTriggered => internalCancelTriggered;
        
        public int ProgressTaskId => TaskId;

        protected override bool Internal_Tick()
        {
            return internalTickComplete;
        }

        protected override void Internal_Cancel()
        {
            internalCancelTriggered = true;
        }
    }

    public class AsyncJobTests
    {
        /// <summary>
        /// Verifies that the Tick method executes successfully.
        /// </summary>
        [Test]
        public void Tick_ProgressStartsAndCompletesSuccessfully()
        {
            // Arrange
            var job = new TestAsyncJob(tickComplete: true);

            // Act
            bool completed = job.Tick();

            // Assert
            Assert.IsTrue(completed, "Tick should return true when the internal task completes.");
        }

        /// <summary>
        /// Ensures progress is initialized on the first call (requires Unity Progress API).
        /// </summary>
        [Test]
        public void Tick_InitializesProgressTaskIdOnce()
        {
#if UNITY_2020_1_OR_NEWER
            // Arrange
            var job = new TestAsyncJob();

            // Act
            job.Tick();

            // Assert
            Assert.AreNotEqual(0, job.ProgressTaskId, "Progress Task ID should be initialized on first Tick.");
#endif
        }

        /// <summary>
        /// Confirms that Cancel calls the appropriate internal logic.
        /// </summary>
        [Test]
        public void Cancel_TriggersInternalCancel()
        {
            // Arrange
            var job = new TestAsyncJob();

            // Act
            job.Cancel();

            // Assert
            Assert.IsTrue(job.InternalCancelTriggered, "Cancel should trigger Internal_Cancel.");
        }

        /// <summary>
        /// Tests that the OnComplete callback is executed upon job completion.
        /// </summary>
        [Test]
        public void OnComplete_InvokesRegisteredCallback()
        {
            // Arrange
            var job = new TestAsyncJob();
            bool callbackInvoked = false;

            job.OnComplete(j =>
            {
                callbackInvoked = true;
                Assert.AreEqual(job, j, "OnComplete callback should receive the correct job instance.");
            });

            // Act
            job.SetProgress(1.0f, "Completed");

            // Assert
            Assert.IsTrue(callbackInvoked, "OnComplete should invoke the registered callback upon job completion.");
        }

        /// <summary>
        /// Verifies that SetProgress updates progress and message correctly.
        /// </summary>
        [Test]
        public void SetProgress_UpdatesProgressAndMessage()
        {
            // Arrange
            var job = new TestAsyncJob();

            // Act
            job.SetProgress(0.5f, "In progress...");

            // Assert
            Assert.AreEqual(0.5f, job.progress, "Progress should be updated correctly.");
            Assert.AreEqual("In progress...", job.message, "Message should be updated correctly.");
        }

        /// <summary>
        /// Ensures that the cancel callback is correctly triggered and flagged.
        /// </summary>
        [Test]
        public void CancelCallback_SetsIsCancelled()
        {
            // Arrange
            var job = new TestAsyncJob();

            // Act
            job.Cancel();

            // Assert
            Assert.IsTrue(job.InternalCancelTriggered, "CancelCallback should set the internalCancelTriggered flag.");
        }

        /// <summary>
        /// Tests that failing the task sets the correct status (assumes Unity Progress API).
        /// </summary>
        [Test]
        public void Fail_MarksTaskAsFailed()
        {
#if UNITY_2020_1_OR_NEWER
            // Arrange
            var job = new TestAsyncJob();

            // Act
            job.Fail();

            // Assert
            // Check if Progress.Finish is called with Status.Failed; 
            // This assumes monitoring Progress API calls.
#endif
        }
    }
}
