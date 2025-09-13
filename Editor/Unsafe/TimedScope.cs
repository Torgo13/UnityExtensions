using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;

namespace PKGE.Editor.Unsafe
{
    /// <summary>
    /// Allows time measurements
    /// </summary>
    /// <example><code>
    /// double duration = 0;
    /// using (TimedScope.FromPtr(&amp;duration))
    /// {
    ///     // something to get the time
    /// }
    /// Debug.Log($"Duration: {duration}")
    /// </code></example>
    public readonly unsafe struct TimedScope : IDisposable
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Editor/Utilities/TimedScope.cs
        #region UnityEditor.Rendering
        static readonly ThreadLocal<Stopwatch> StopWatch = new ThreadLocal<Stopwatch>(() => new Stopwatch());

        readonly double* _durationMsPtr;

        TimedScope(double* durationMsPtr)
        {
            _durationMsPtr = durationMsPtr;
            StopWatch.Value.Start();
        }

        /// <summary>
        /// Dispose method to retrieve the time
        /// </summary>
        void IDisposable.Dispose()
        {
            StopWatch.Value.Stop();
            *_durationMsPtr = StopWatch.Value.Elapsed.TotalMilliseconds;
            StopWatch.Value.Reset();
        }

        /// <summary>
        /// Obtains a <see cref="TimedScope"/>.
        /// Safety: <paramref name="durationMsPtr"/> must be a non-null pointer to a valid memory location for a double.
        /// </summary>
        /// <param name="durationMsPtr">The location to write the duration in milliseconds to.</param>
        /// <returns>A <see cref="TimedScope"/></returns>
        public static TimedScope FromPtr([DisallowNull] double* durationMsPtr)
        {
            return new TimedScope(durationMsPtr);
        }

        /// <summary>
        /// Obtains a <see cref="TimedScope"/>
        /// </summary>
        /// <param name="durationMs">The location to write the duration in milliseconds to.</param>
        /// <returns>A <see cref="TimedScope"/></returns>
        public static TimedScope FromRef(ref double durationMs)
        {
            return new TimedScope((double*)UnsafeUtility.AddressOf(ref durationMs));
        }
        #endregion // UnityEditor.Rendering
    }
}
