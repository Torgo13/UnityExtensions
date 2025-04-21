using System;
using UnityEditor;

namespace UnityExtensions.Editor
{
    /// <summary>Base implementation of <see cref="IAsyncJob"/></summary>
    public abstract class AsyncJob : IAsyncJob
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.shaderanalysis/Editor/API/AsyncJob.cs
        #region UnityEditor.ShaderAnalysis
        protected int TaskId;
        bool _onCompleteLaunched;
        bool _isCancelled;
        Action<IAsyncJob> _onComplete;

        public abstract string name { get; }
        /// <inheritdoc cref="IAsyncJob.progress"/>
        public float progress { get; private set; }
        /// <inheritdoc cref="IAsyncJob.message"/>
        public string message { get; private set; }

        /// <inheritdoc cref="IAsyncJob.Tick"/>
        public bool Tick()
        {
#if UNITY_2020_1_OR_NEWER
            if (TaskId == 0)
            {
                TaskId = Progress.Start(name, message);
                Progress.RegisterCancelCallback(TaskId, CancelCallback);
            }
#endif
            return Internal_Tick();
        }

        protected abstract bool Internal_Tick();

        /// <inheritdoc cref="IAsyncJob.Cancel"/>
        public void Cancel()
        {
#if UNITY_2020_1_OR_NEWER
            _isCancelled = true;
            if (TaskId != 0)
                Progress.Cancel(TaskId);
            else
#endif
            Internal_Cancel();
        }

        protected abstract void Internal_Cancel();

        /// <inheritdoc cref="IAsyncJob.OnComplete(Action{IAsyncJob})"/>
        public void OnComplete(Action<IAsyncJob> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (_onCompleteLaunched)
                action(this);
            else
                _onComplete += action;
        }

        /// <summary>Set the progress of this job.</summary>
        /// <param name="progressArg">
        /// The progress of the job, it will be min maxed into range [0-1].
        /// If it is equal to <c>1.0f</c>, then job will be considered as completed.</param>
        /// <param name="messageArg">A descriptive message indicating the current job operation.</param>
        public void SetProgress(float progressArg, string messageArg)
        {
            progressArg = Math.Max(0, progressArg);
            progressArg = Math.Min(1, progressArg);

            progress = progressArg;
            message = messageArg;

#if UNITY_2020_1_OR_NEWER
            if (TaskId != 0)
                Progress.Report(TaskId, progress, message);
#endif

            if (progressArg >= 1 && !_onCompleteLaunched)
            {
                _onCompleteLaunched = true;
                _onComplete?.Invoke(this);

#if UNITY_2020_1_OR_NEWER
                // Don't remove task when cancelling
                if (TaskId != 0 && !_isCancelled)
                {
                    Progress.Remove(TaskId);
                    TaskId = 0;
                }
#endif
            }
        }

        bool CancelCallback()
        {
            _isCancelled = true;
            Internal_Cancel();
            return true;
        }

        internal void Fail()
        {
            Progress.Finish(TaskId, Progress.Status.Failed);
        }
        #endregion // UnityEditor.ShaderAnalysis
    }
}
