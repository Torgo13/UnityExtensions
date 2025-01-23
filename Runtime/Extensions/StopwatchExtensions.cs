#if !NET_4_6
using System.Diagnostics;

namespace UnityExtensions
{
    /// <summary>
    /// Extension methods for `System.Diagnostics.Stopwatch` objects.
    /// </summary>
    public static class StopwatchExtensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Extensions/StopwatchExtensions.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Restarts the stopwatch by stopping, resetting, and then starting it.
        /// </summary>
        /// <param name="stopwatch">The stopwatch to restart.</param>
        public static void Restart(this Stopwatch stopwatch)
        {
            stopwatch.Stop();
            stopwatch.Reset();
            stopwatch.Start();
        }
        #endregion // Unity.XR.CoreUtils
    }
}
#endif
