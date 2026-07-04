#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CancellationToken = System.Threading.CancellationToken;

namespace PKGE
{
    /// <summary>
    /// Utility class to execute a task on multiple threads.
    /// </summary>
    public static class TaskUtility
    {
        //https://github.com/needle-mirror/com.unity.graphtools.foundation/blob/0.11.2-preview/Runtime/Utility/TaskUtility.cs
        #region UnityEngine.GraphToolsFoundation.Overdrive
        /// <summary>
        /// Run a task on a list of items on all processors. The list of item will be split equally across the processors.
        /// </summary>
        /// <param name="items">The list of items on which to execute <paramref name="action"/>.</param>
        /// <param name="action">The task to execute on each item of <paramref name="items"/>.</param>
        /// <param name="ct">Optional <see cref="CancellationToken"/>.</param>
        /// <typeparam name="TInput">The type of each item.</typeparam>
        /// <typeparam name="TOutput">The type of the result.</typeparam>
        /// <returns>An <see cref="System.Collections.Generic.IEnumerable{T}"/> of the execution results.</returns>
        public static IEnumerable<TOutput> RunTasks<TInput, TOutput>(
            List<TInput> items,
            Action<TInput, ConcurrentBag<TOutput>> action,
            CancellationToken ct = default)
        {
            var cb = new ConcurrentBag<TOutput>();

#if UNITY_WEBGL
            foreach (var item in items)
            {
                action.Invoke(item, cb);
            }
#else
            var count = Environment.ProcessorCount;
            var tasks = new System.Threading.Tasks.Task[count];
            int itemsPerTask = (int)Math.Ceiling(items.Count / (double)count);

            for (int i = 0; i < count; i++)
            {
                int i1 = i;
                tasks[i] = System.Threading.Tasks.Task.Run(() =>
                {
                    for (int j = 0; j < itemsPerTask && j + itemsPerTask * i1 < items.Count; j++)
                    {
                        int index = j + itemsPerTask * i1;
                        action.Invoke(items[index], cb);
                    }
                },
                cancellationToken: ct);
            }

            System.Threading.Tasks.Task.WaitAll(tasks, cancellationToken: ct);
#endif // UNITY_WEBGL

            return cb;
        }
        #endregion // UnityEngine.GraphToolsFoundation.Overdrive

        /// <inheritdoc cref="RunTasks{TInput, TOutput}(List{TInput}, Action{TInput, ConcurrentBag{TOutput}}, CancellationToken)"/>
        public static async System.Threading.Tasks.ValueTask<IEnumerable<TOutput>> RunTasksAsync<TInput, TOutput>(
            List<TInput> items,
            Action<TInput, ConcurrentBag<TOutput>?> action,
            ConcurrentBag<TOutput>? cb = null,
            CancellationToken ct = default)
        {
#if UNITY_WEBGL
            foreach (var item in items)
            {
                action.Invoke(item, cb);
            }
#else
            var count = Environment.ProcessorCount;
            using var _0 = UnityEngine.Pool.ListPool<System.Threading.Tasks.Task>.Get(out var tasks);
            tasks.EnsureCapacity(count);
            int itemsPerTask = (int)Math.Ceiling(items.Count / (double)count);

            for (int i = 0; i < count; i++)
            {
                int i1 = i;
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    for (int j = 0; j < itemsPerTask && j + itemsPerTask * i1 < items.Count; j++)
                    {
                        int index = j + itemsPerTask * i1;
                        action.Invoke(items[index], cb);
                    }
                },
                cancellationToken: ct));
            }

            await System.Threading.Tasks.Task.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext: true);
#endif // UNITY_WEBGL

            return cb ?? System.Linq.Enumerable.Empty<TOutput>();
        }


#if UNITY_6000_0_OR_NEWER
        /// <inheritdoc cref="RunTasks{TInput, TOutput}(List{TInput}, Action{TInput, ConcurrentBag{TOutput}}, CancellationToken)"/>
        public static async UnityEngine.Awaitable<IEnumerable<TOutput>> RunAsync<TInput, TOutput>(
            List<TInput> items,
            Action<TInput, ConcurrentBag<TOutput>?> action,
            ConcurrentBag<TOutput>? cb = null,
            CancellationToken ct = default)
        {
            var count = Environment.ProcessorCount;
            var tasks = UnityEngine.Pool.ListPool<UnityEngine.Awaitable>.Get();
            tasks.EnsureCapacity(count);
            int itemsPerTask = (int)Math.Ceiling(items.Count / (double)count);

            for (int i = 0; i < count; i++)
            {
                tasks.Add(RunAsync(items, action, cb, itemsPerTask, i, ct));
            }

            foreach (var task in tasks)
            {
                await task;
            }

            UnityEngine.Pool.ListPool<UnityEngine.Awaitable>.Release(tasks);
            return cb ?? System.Linq.Enumerable.Empty<TOutput>();
        }

        private static async UnityEngine.Awaitable RunAsync<TInput, TOutput>(
            List<TInput> items,
            Action<TInput, ConcurrentBag<TOutput>?> action,
            ConcurrentBag<TOutput>? cb,
            int itemsPerTask, int i,
            CancellationToken ct = default)
        {
            await UnityEngine.Awaitable.BackgroundThreadAsync();
            if (ct.IsCancellationRequested)
                return;

            for (int j = 0, itemsCount = items.Count; j < itemsPerTask && j + itemsPerTask * i < itemsCount; j++)
            {
                int index = j + itemsPerTask * i;
                action.Invoke(items[index], cb);
            }
        }
#endif // UNITY_6000_0_OR_NEWER
    }
}