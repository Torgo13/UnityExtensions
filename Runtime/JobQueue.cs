using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PKGE
{
    /// <example>
    /// <see href="https://github.com/Unity-Technologies/HLODSystem/blob/master/com.unity.hlod/Editor/TerrainHLODCreator.cs#L294"/>
    /// </example>
    /// <remarks><code>
    /// private JobQueue m_queue;
    /// private WorkingObject CreateBakedTerrain(string name, Bounds bounds, Heightmap heightmap, int distance, bool isLeaf)
    /// {
    ///     WorkingObject wo = new WorkingObject(Allocator.Persistent);
    ///     m_queue.EnqueueJob(() =>
    ///     {
    ///         WorkingMesh mesh = CreateBakedGeometry(name, heightmap, bounds, distance);
    ///         wo.SetMesh(mesh);
    ///     });
    ///     m_queue.EnqueueJob(() =>
    ///     {
    ///         WorkingMaterial material = CreateBakedMaterial(name, bounds, isLeaf);
    ///         wo.Materials.Add(material);
    ///     });
    ///     return wo;
    /// }
    /// </code></remarks>
    public class JobQueue : IDisposable
    {
        //https://github.com/Unity-Technologies/HLODSystem/blob/master/com.unity.hlod/Editor/Utils/JobQueue.cs
        #region Unity.HLODSystem.Utils
        public JobQueue(int threadCount)
        {
            _workers = new Worker[threadCount];
            for (int i = 0; i < threadCount; ++i)
            {
                _workers[i] = new Worker(this);
            }
        }
        
        public void EnqueueMainThreadJob(Action job)
        {
            lock (_mainThreadJobs)
            {
                _mainThreadJobs.Enqueue(job);
            }
        }

        public void EnqueueJob(Action job)
        {
            lock (_jobs)
            {
                _jobs.Enqueue(job);
            }
        }

        private Action DequeueMainThreadJob()
        {
            lock (_mainThreadJobs)
            {
                if (_mainThreadJobs.Count == 0)
                    return null;

                return _mainThreadJobs.Dequeue();
            }
        }
        private Action DequeueJob()
        {
            lock (_jobs)
            {
                if (_jobs.Count == 0)
                    return null;

                return _jobs.Dequeue();
            }
        }        

        public IEnumerator WaitFinish()
        {
            bool isFinish = false;
            while (isFinish == false)
            {
                while (true)
                {
                    Action mainThreadJob = DequeueMainThreadJob();
                    if (mainThreadJob == null)
                        break;

                    mainThreadJob.Invoke();
                }

                if (_jobs.Count > 0)
                {
                    yield return null;
                    continue;
                }

                isFinish = true;
                for (int i = 0; i < _workers.Length; ++i)
                {
                    if (_workers[i].IsException())
                    {
                        throw new Exception("Exception from worker thread.");
                    }

                    if (_workers[i].IsWorking())
                    {
                        isFinish = false;
                    }
                }

                if (isFinish == false)
                    yield return null;
            }
        }

        public async ValueTask WaitFinishAsync()
        {
            bool isFinish = false;
            while (isFinish == false)
            {
                while (true)
                {
                    Action mainThreadJob = DequeueMainThreadJob();
                    if (mainThreadJob == null)
                        break;

                    mainThreadJob.Invoke();
                }

                if (_jobs.Count > 0)
                {
                    await Task.Yield();
                    continue;
                }

                isFinish = true;
                for (int i = 0; i < _workers.Length; ++i)
                {
                    if (_workers[i].IsException())
                    {
                        throw new Exception("Exception from worker thread.");
                    }

                    if (_workers[i].IsWorking())
                    {
                        isFinish = false;
                    }
                }

                if (isFinish == false)
                    await Task.Yield();
            }
        }

        public void Dispose()
        {
            for ( int i = 0; i < _workers.Length; ++i )
            {
                _workers[i].Stop();
            }

            _workers = null;
        }

        private Worker[] _workers;
        
        private readonly Queue<Action> _mainThreadJobs = new Queue<Action>();
        private readonly Queue<Action> _jobs = new Queue<Action>();
        
        #region worker
        class Worker
        {
            private readonly JobQueue _queue;
            
            private readonly Thread _thread;
            
            private bool _terminated;
            private bool _working;
            private bool _exception;

            public Worker(JobQueue queue)
            {
                _queue = queue;
                _thread = new Thread(Run);
                _thread.Start();
                
                _terminated = false;
                _working = false;
                _exception = false;
            }

            public void Stop()
            {
                _terminated = true;
            }

            public bool IsException()
            {
                return _exception;
            }
            
            public bool IsWorking()
            {
                return _working;
            }

            private void Run()
            {
                while (_terminated == false)
                {
                    try
                    {
                        _working = true;
                        Action job = _queue.DequeueJob();
                        if (job == null)
                        {
                            _working = false;
                            Thread.Sleep(100);
                            continue;
                        }

                        job.Invoke();
                        _working = false;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        _exception = true;
                    }
                    finally
                    {
                        _working = false;
                    }
                }
            }
        }
        #endregion
        #endregion // Unity.HLODSystem.Utils
    }
}