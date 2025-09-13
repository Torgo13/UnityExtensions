#if !NET_4_6
using System.Diagnostics;

namespace PKGE
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

        //https://github.com/Unity-Technologies/FPSSample/blob/6b8b27aca3690de9e46ca3fe5780af4f0eff5faa/Assets/Scripts/Utils/StopwatchExtensions.cs
        #region FPSSample
        public static double GetTicksDeltaAsMilliseconds(this Stopwatch stopWatch, long previousTicks)
        {
            return stopWatch.GetTicksDeltaAsSeconds(previousTicks) * 1000;
        }

        public static double GetTicksDeltaAsSeconds(this Stopwatch stopWatch, long previousTicks)
        {
            return (double)(stopWatch.ElapsedTicks - previousTicks) / Stopwatch.Frequency;
        }
        #endregion // FPSSample
    }
}
#endif
