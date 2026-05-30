using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace TCGE
{
    public static class AwaitableExtensions
    {
#if UNITY_6000_0_OR_NEWER
        public static async Awaitable WhenAll(IEnumerable<Awaitable> tasks,
            CancellationToken ct = default)
        {
            foreach (var task in tasks)
            {
                await task;
                if (ct.IsCancellationRequested)
                    return;
            }
        }

        public static async Awaitable WhenAll(IEnumerable<Task> tasks,
            CancellationToken ct = default)
        {
            foreach (var task in tasks)
            {
                await task;
                if (ct.IsCancellationRequested)
                    return;
            }
        }

        public static async Awaitable WhenAll(IEnumerable<ValueTask> tasks,
            CancellationToken ct = default)
        {
            foreach (var task in tasks)
            {
                await task;
                if (ct.IsCancellationRequested)
                    return;
            }
        }
#else
        public static async Task WhenAll(IEnumerable<Task> tasks,
            CancellationToken ct = default)
        {
            foreach (var task in tasks)
            {
                await task;
                if (ct.IsCancellationRequested)
                    return;
            }
        }

        public static async ValueTask WhenAll(IEnumerable<ValueTask> tasks,
            CancellationToken ct = default)
        {
            foreach (var task in tasks)
            {
                await task;
                if (ct.IsCancellationRequested)
                    return;
            }
        }
#endif // UNITY_6000_0_OR_NEWER

#if UNITY_6000_0_OR_NEWER
        public static async Awaitable RunOnThreadPool(this Awaitable action,
            CancellationToken cancellationToken = default)
        {
            await Awaitable.BackgroundThreadAsync();
            if (cancellationToken.IsCancellationRequested)
                return;

            await action;
        }

        public static async Awaitable RunOnThreadPool(System.Action action,
            CancellationToken cancellationToken = default)
        {
            await Awaitable.BackgroundThreadAsync();
            if (cancellationToken.IsCancellationRequested)
                return;

            action();
        }

        public static async Awaitable RunOnThreadPool(Task action,
            CancellationToken cancellationToken = default)
        {
            await Awaitable.BackgroundThreadAsync();
            if (cancellationToken.IsCancellationRequested)
                return;

            await action;
        }

        public static async Awaitable RunOnThreadPool(ValueTask action,
            CancellationToken cancellationToken = default)
        {
            await Awaitable.BackgroundThreadAsync();
            if (cancellationToken.IsCancellationRequested)
                return;

            await action;
        }
#else
        public static async Task RunOnThreadPool(System.Action action,
            CancellationToken ct)
        {
            try
            {
                await Task.Run(action, ct);
            }
            catch (System.OperationCanceledException) { }
        }

        public static async Task RunOnThreadPool(System.Action action)
        {
            await Task.Run(action);
        }

        public static async Task RunOnThreadPool(Task action,
            CancellationToken ct)
        {
            try
            {
                await Task.Run(async () => await action, ct);
            }
            catch (System.OperationCanceledException) { }
        }

        public static async Task RunOnThreadPool(Task action)
        {
            await Task.Run(async () => await action);
        }

        public static async Task RunOnThreadPool(ValueTask action,
            CancellationToken ct)
        {
            try
            {
                await Task.Run(async () => await action, ct);
            }
            catch (System.OperationCanceledException) { }
        }

        public static async Task RunOnThreadPool(ValueTask action)
        {
            await Task.Run(async () => await action);
        }
#endif // UNITY_6000_0_OR_NEWER
    }
}

namespace PKGE
{
    public static class AwaitableExtensions
    {
#if UNITY_6000_0_OR_NEWER
        //https://docs.unity3d.com/Documentation/Manual/async-awaitable-examples.html
        #region Unity
        public static async Task AsTask([System.Diagnostics.CodeAnalysis.NotNull] this Awaitable a)
        {
#if SAFETY
            try
            {
                await a;
            }
            catch (System.Exception e)
            {
                if (e is not System.OperationCanceledException)
                    Debug.LogException(e);
            }
#else
            await a;
#endif // SAFETY
        }

        public static async Task<T> AsTask<T>([System.Diagnostics.CodeAnalysis.NotNull] this Awaitable<T> a)
        {
#if SAFETY
            try
            {
                return await a;
            }
            catch (System.Exception e)
            {
                if (e is not System.OperationCanceledException)
                    Debug.LogException(e);
            }

            return default;
#else
            return await a;
#endif // SAFETY
        }
        #endregion // Unity
#endif // UNITY_6000_0_OR_NEWER
    }
}