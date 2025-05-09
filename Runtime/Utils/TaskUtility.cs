using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityExtensions
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
        /// <typeparam name="TInput">The type of each item.</typeparam>
        /// <typeparam name="TOutput">The type of the result.</typeparam>
        /// <returns>An enumerable of the execution results.</returns>
        internal static IEnumerable<TOutput> RunTasks<TInput, TOutput>(
            List<TInput> items,
            Action<TInput, ConcurrentBag<TOutput>> action)
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
                    for (int j = 0; j < itemsPerTask; j++)
                    {
                        int index = j + itemsPerTask * i1;
                        if (index >= items.Count)
                            break;

                        action.Invoke(items[index], cb);
                    }
                });
            }

            Task.WaitAll(tasks);
            return cb;
        }
        #endregion // UnityEngine.GraphToolsFoundation.Overdrive
    }
}