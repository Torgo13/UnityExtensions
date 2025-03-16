using System;
using System.Globalization;

namespace UnityExtensions
{
    public static class TimeSpanExtensions
    {
        //https://github.com/needle-mirror/com.unity.entities/blob/1.3.9/Unity.Entities.Editor/Extensions/TimeSpanExtensions.cs
        #region Unity.Entities.Editor
        public static string ToShortString(this TimeSpan timeSpan, uint decimals = 2)
        {
            if (timeSpan.TotalSeconds < 1.0)
            {
                return timeSpan.Milliseconds.ToString(CultureInfo.InvariantCulture) + "ms";
            }
            if (timeSpan.TotalMinutes < 1.0)
            {
                return timeSpan.TotalSeconds.ToString("F" + decimals, CultureInfo.InvariantCulture) + "s";
            }
            if (timeSpan.TotalHours < 1.0)
            {
                return timeSpan.TotalMinutes.ToString("F" + decimals, CultureInfo.InvariantCulture) + "m";
            }
            if (timeSpan.TotalDays < 1.0)
            {
                return timeSpan.TotalHours.ToString("F" + decimals, CultureInfo.InvariantCulture) + "h";
            }

            return timeSpan.TotalDays.ToString("F" + decimals, CultureInfo.InvariantCulture) + "d";
        }
        #endregion // Unity.Entities.Editor
    }
}