using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
        /// <returns>An enumerable of the execution results.</returns>
        public static IEnumerable<TOutput> RunTasks<TInput, TOutput>(
            List<TInput> items,
            Action<TInput, ConcurrentBag<TOutput>> action,
            CancellationToken ct = default)
        {
            var cb = new ConcurrentBag<TOutput>();
            var count = Environment.ProcessorCount;
            var tasks = new Task[count];
            int itemsPerTask = (int)Math.Ceiling(items.Count / (double)count);

            for (int i = 0; i < count; i++)
            {
                int i1 = i;
                tasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < itemsPerTask && j + itemsPerTask * i1 < items.Count; j++)
                    {
                        int index = j + itemsPerTask * i1;
                        action.Invoke(items[index], cb);
                    }
                },
                cancellationToken: ct);
            }

            Task.WaitAll(tasks, cancellationToken: ct);
            return cb;
        }
        #endregion // UnityEngine.GraphToolsFoundation.Overdrive

        /// <inheritdoc cref="RunTasks{TInput, TOutput}(List{TInput}, Action{TInput, ConcurrentBag{TOutput}}, CancellationToken)"/>
        public static async ValueTask<IEnumerable<TOutput>> RunTasksAsync<TInput, TOutput>(
            List<TInput> items,
            Action<TInput, ConcurrentBag<TOutput>> action,
            CancellationToken ct = default)
        {
            var cb = new ConcurrentBag<TOutput>();
            await RunTasksAsync(items, cb, action, ct).ConfigureAwait(continueOnCapturedContext: true);
            return cb;
        }

        /// <inheritdoc cref="RunTasks{TInput, TOutput}(List{TInput}, Action{TInput, ConcurrentBag{TOutput}}, CancellationToken)"/>
        public static async ValueTask RunTasksAsync<TInput, TOutput>(
            List<TInput> items,
            ConcurrentBag<TOutput> cb,
            Action<TInput, ConcurrentBag<TOutput>> action,
            CancellationToken ct = default)
        {
            var count = Environment.ProcessorCount;
            using var _0 = UnityEngine.Pool.ListPool<Task>.Get(out var tasks);
            tasks.EnsureCapacity(count);
            int itemsPerTask = (int)Math.Ceiling(items.Count / (double)count);

            for (int i = 0; i < count; i++)
            {
                int i1 = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < itemsPerTask && j + itemsPerTask * i1 < items.Count; j++)
                    {
                        int index = j + itemsPerTask * i1;
                        action.Invoke(items[index], cb);
                    }
                },
                cancellationToken: ct));
            }

            await Task.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext: true);
        }
        
        /// <inheritdoc cref="RunTasks{TInput, TOutput}(List{TInput}, Action{TInput, ConcurrentBag{TOutput}}, CancellationToken)"/>
        public static async ValueTask RunTasksAsync<TInput>(
            List<TInput> items,
            Action<TInput> action,
            CancellationToken ct = default)
        {
            var count = Environment.ProcessorCount;
            using var _0 = UnityEngine.Pool.ListPool<Task>.Get(out var tasks);
            tasks.EnsureCapacity(count);
            int itemsPerTask = (int)Math.Ceiling(items.Count / (double)count);

            for (int i = 0; i < count; i++)
            {
                int i1 = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < itemsPerTask && j + itemsPerTask * i1 < items.Count; j++)
                    {
                        int index = j + itemsPerTask * i1;
                        action.Invoke(items[index]);
                    }
                },
                cancellationToken: ct));
            }

            await Task.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext: true);
        }
    }
}