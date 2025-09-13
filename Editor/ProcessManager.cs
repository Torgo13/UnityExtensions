using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace PKGE.Editor
{
    public class ProcessManager
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.shaderanalysis/Editor/Internal/ProcessManager.cs
        #region UnityEditor.ShaderAnalysis.Internal
        public interface IProcess
        {
            ProcessStartInfo startInfo { get; }
            Process process { get; }
            bool hasStarted { get; }
        }

        class ProcessImpl : IProcess
        {
            readonly Action<IProcess> _preStart;
            readonly Action<IProcess> _postStart;

            public ProcessStartInfo startInfo { get; private set; }
            public Process process { get; private set; }
            public bool hasStarted { get { return process != null; } }

            public ProcessImpl(ProcessStartInfo startInfo, Action<IProcess> preStart, Action<IProcess> postStart)
            {
                this.startInfo = startInfo;
                _preStart = preStart;
                _postStart = postStart;
            }

            public void Start()
            {
                process = new Process();
                process.StartInfo = startInfo;
                process.EnableRaisingEvents = true;

                if (_preStart != null)
                    _preStart(this);

                process.Start();

                if (_postStart != null)
                    _postStart(this);
            }

            public void Cancel()
            {
                if (process != null && !process.HasExited)
                    process.Kill();
            }
        }

        const int MaxProcesses = 16;

        static readonly ProcessManager Instance = new ProcessManager(MaxProcesses);

        int _updateRefCount;
        readonly int _maxProcesses;
        readonly List<ProcessImpl> _pendingProcesses = new List<ProcessImpl>();
        readonly List<ProcessImpl> _runningProcesses = new List<ProcessImpl>();

        public static IProcess Enqueue(ProcessStartInfo startInfo, Action<IProcess> preStart, Action<IProcess> postStart)
        {
            return Instance.DoEnqueue(startInfo, preStart, postStart);
        }

        public static void Cancel(IProcess process)
        {
            Instance.DoCancel(process);
        }

        public ProcessManager(int maxProcesses)
        {
            _maxProcesses = maxProcesses;
        }

        IProcess DoEnqueue(ProcessStartInfo startInfo, Action<IProcess> preStart, Action<IProcess> postStart)
        {
            var impl = new ProcessImpl(startInfo, preStart, postStart);
            _pendingProcesses.Add(impl);

            if (_updateRefCount == 0)
                EditorUpdateManager.ToUpdate += Update;

            ++_updateRefCount;

            return impl;
        }

        void DoCancel(IProcess process)
        {
            var processImpl = (ProcessImpl)process;
            var pendingIndex = _pendingProcesses.IndexOf(processImpl);
            if (pendingIndex != -1)
            {
                _pendingProcesses.RemoveAt(pendingIndex);
                return;
            }

            var runningIndex = _runningProcesses.IndexOf(processImpl);
            if (runningIndex != -1)
            {
                _runningProcesses.RemoveAt(runningIndex);
                processImpl.Cancel();
            }
        }

        void Update()
        {
            for (var i = _runningProcesses.Count - 1; i >= 0; --i)
            {
                var proc = _runningProcesses[i];
                if (proc.process.HasExited)
                {
                    _runningProcesses.RemoveAt(i);

                    --_updateRefCount;
                    if (_updateRefCount == 0)
                        EditorUpdateManager.ToUpdate -= Update;
                }
            }

            var processToRun = Mathf.Min(_maxProcesses, _pendingProcesses.Count - _runningProcesses.Count);
            for (var i = 0; i < processToRun; i++)
            {
                var proc = _pendingProcesses[0];
                _pendingProcesses.RemoveAt(0);

                proc.Start();
                _runningProcesses.Add(proc);
            }
        }
        #endregion // UnityEditor.ShaderAnalysis.Internal
    }

    public static class ProcessManagerExtensions
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.shaderanalysis/Editor/Internal/ProcessManager.cs
        #region UnityEditor.ShaderAnalysis.Internal
        public static bool IsComplete(this ProcessManager.IProcess process)
        {
            return process != null && process.process != null && process.hasStarted && process.process.HasExited;
        }
        #endregion // UnityEditor.ShaderAnalysis.Internal
    }
}
