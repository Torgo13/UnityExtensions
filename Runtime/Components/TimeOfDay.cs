#nullable enable
using System;
using UnityEngine;

namespace PKGE
{
    [RequireComponent(typeof(Light))]
    [ExecuteInEditMode]
    public class TimeOfDay : MonoBehaviour
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Samples~/Environment%20Samples/Scripts/TimeOfDay.cs
        #region UnityEngine.Rendering.HighDefinition
        [Range(0f, 24f)]
        [Tooltip("Time of day normalized between 0 and 24h. For example, 6.5 amounts to 6:30am.")]
        public float timeOfDay = 12f;

        [SerializeField]
        [Tooltip("Sets the speed at which the time of day passes.")]
        float timeSpeed = 1f;

        // Paris Office coordinates. 
        public float latitude = 48.83402f;
        public float longitude = 2.367259f;

        // Arbitrary date to have the sunset framed in the camera frustum. 
        readonly DateTime _date = new DateTime(2024, 4, 21, 0, 0, 0, DateTimeKind.Utc).Date;

        [SerializeField, HideInInspector]
	    private GUIStyle? sliderStyle;

        private static TimeOfDay _instance = null!;

        #region MonoBehaviour
        private void OnEnable()
	    {
            _instance = this;
	    }

        private void OnValidate()
        {
            while (timeOfDay < 0f)
            {
                timeOfDay += 24f;
            }

            timeOfDay %= 24f;

#if UNITY_6000_3_OR_NEWER
            var t = transformHandle;
#else
            var t = transform;
#endif // UNITY_6000_3_OR_NEWER

            TimeOfDayUtils.SetSunPosition(t, _date.AddHours(timeOfDay), latitude, longitude);
        }

        void Update()
        {
            timeOfDay += timeSpeed * Time.deltaTime;

            //This is for the variable to loop for easier use.
            timeOfDay %= 24f;

#if UNITY_6000_3_OR_NEWER
            var t = transformHandle;
#else
            var t = transform;
#endif // UNITY_6000_3_OR_NEWER

            TimeOfDayUtils.SetSunPosition(t, _date.AddHours(timeOfDay), latitude, longitude);
        }
        #endregion // MonoBehaviour

        #if UNITY_EDITOR
        void OnGUI()
        {
            DrawWindow();

            // Force repaint of game view
            Type type = ReflectionUtils.FindTypeByFullName("UnityEditor.GameView")!;
            UnityEditor.EditorUtility.SetDirty(UnityEditor.EditorWindow.GetWindow(type, false, null, false));
        }

        internal void DrawWindow()
        {
            UnityEditor.Handles.BeginGUI();

            const float windowHeight = 15 + 30;
            _ = GUI.Window(0, new Rect(Screen.width * 0.1f, Screen.height * 0.89f, Screen.width * 0.8f,
                windowHeight), Window_StatusPanel, "", GUIStyle.none);

            UnityEditor.Handles.EndGUI();
        }

        private static void Window_StatusPanel(int windowID)
        {
            if (_instance == null)
                return;

            GUIStyle textStyle = new GUIStyle();
            textStyle.fontSize = 16;
            textStyle.normal.textColor = Color.white;
            textStyle.fontStyle = FontStyle.Bold;

            GUI.color = Color.white;
            UnityEditor.EditorGUI.BeginChangeCheck();
            GUI.Label(new Rect(Screen.width * 0.0f, 0, Screen.width * 0.1f, 30), "Midnight", textStyle);
            GUI.Label(new Rect(Screen.width * 0.39f, 0, Screen.width * 0.02f, 30), "Noon", textStyle);
            float timeOfDay = GUI.HorizontalSlider(
                new Rect(Screen.width * 0.015f, 25, Screen.width * 0.77f, 8),
                _instance.timeOfDay, 0.0F, 24.0F, _instance.sliderStyle,
                GUI.skin.horizontalSliderThumb);
            GUI.Label(new Rect(Screen.width * 0.7625f, 0, Screen.width * 0.1f, 30), "Midnight", textStyle);

            if (UnityEditor.EditorGUI.EndChangeCheck())
                _instance.timeOfDay = timeOfDay;
        }
        #endif
    }

    #if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(TimeOfDay))]
    public class DrawLineEditor : UnityEditor.Editor
    {
        void OnSceneGUI()
        {
            if (target is TimeOfDay t)
                t.DrawWindow();
        }
    }
    #endif
    #endregion // UnityEngine.Rendering.HighDefinition

    [Unity.Burst.BurstCompile]
    public static class TimeOfDayUtils
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Samples~/Environment%20Samples/Scripts/TimeOfDay.cs
        #region UnityEngine.Rendering.HighDefinition
        public static void SetSunPosition(Transform transform, DateTime dateTime,
            float latitude, float longitude = 0)
        {
            transform.localRotation = CalculateSunPosition(dateTime, latitude, longitude);
        }

#if UNITY_6000_3_OR_NEWER
        public static void SetSunPosition(TransformHandle transform, DateTime dateTime,
            float latitude, float longitude = 0)
        {
            transform.localRotation = CalculateSunPosition(dateTime, latitude, longitude);
        }
#endif // UNITY_6000_3_OR_NEWER

        /// <param name="t">Time in linear 0-24</param>
        public static void GetHoursMinutesSecondsFromTimeOfDay(float timeOfDay,
            out int hours, out int minutes, out int seconds)
        {
            hours = (int)timeOfDay;
            minutes = (int)((timeOfDay - hours) * 60f);
            seconds = (int)((timeOfDay - hours - (minutes / 60f)) * 60f * 60f);
        }
        #endregion // UnityEngine.Rendering.HighDefinition

        //https://github.com/Unity-Technologies/BoatAttack/blob/e4864ca4381d59e553fe43f3dac6a12500eee8c7/Assets/Scripts/Environment/DayNightController.cs
        #region BoatAttack
        /// <param name="time">Time in linear 0-1</param>
        /// <param name="latitude">Degrees</param>
        /// <param name="longitude">Degrees</param>
        public static void UpdateSun(Transform sunTransform, Light? sun, Gradient? sunColour, double time, float northHeading,
            int year = 2000, int month = 1, int day = 1,
            double latitude = 56, double longitude = 9)
        {
            var rotation = CalculateSunPosition(NormalizedDateTime(time, year, month, day), latitude, longitude);
            sunTransform.rotation = rotation;
            sunTransform.Rotate(new Vector3(0f, northHeading, 0f), Space.World);

            if (sun != null && sunColour != null)
            {
                sun.color = sunColour.Evaluate(Mathf.Clamp01(Vector3.Dot(sunTransform.forward, Vector3.down)));
            }
        }

        /// <param name="latitude">Degrees</param>
        /// <param name="longitude">Degrees</param>
        public static Quaternion CalculateSunPosition(DateTime dateTime, double latitude, double longitude)
        {
            // Convert to UTC
            dateTime = dateTime.ToUniversalTime();

            CalculateSunPosition(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.TimeOfDay.TotalHours,
                latitude * Mathf.Deg2Rad, longitude, out Quaternion rot);

            return rot;
        }

        /// <inheritdoc cref="CalculateSunPosition(int, int, int, double, double, double, out double, out double)"/>
        public static void CalculateSunPosition(int year, int month, int day, double totalHours,
            double latitude, double longitude,
            out Quaternion rot)
        {
            CalculateSunPosition(year, month, day, totalHours,
                latitude, longitude,
                out double azimuth, out double altitude);

            rot = Quaternion.Euler(0f, (float)(azimuth * Mathf.Rad2Deg), 0f)
                * Quaternion.AngleAxis((float)(altitude * Mathf.Rad2Deg), Vector3.right);
        }

        /// <param name="year">UTC</param>
        /// <param name="latitude">Radians</param>
        /// <param name="longitude">Degrees</param>
        /// <param name="azimuth">Radians</param>
        /// <param name="altitude">Radians</param>
        [Unity.Burst.BurstCompile(FloatMode = Unity.Burst.FloatMode.Fast)]
        public static void CalculateSunPosition(int year, int month, int day, double totalHours,
            double latitude, double longitude,
            out double azimuth, out double altitude)
        {
            // Number of days from J2000.0.
            double julianDate = 367 * year -
                (int)(7.0 / 4.0 * (year +
                (int)((month + 9.0) / 12.0))) +
                (int)(275.0 * month / 9.0) +
                day - 730531.5;

            double julianCenturies = julianDate / 36525.0;

            // Sidereal Time
            double siderealTimeHours = 6.6974 + 2400.0513 * julianCenturies;

            double siderealTimeUt = siderealTimeHours + 366.2422 / 365.2422 * totalHours;

            double siderealTime = siderealTimeUt * 15 + longitude;

            // Refine to number of days (fractional) to specific time.
            julianCenturies += totalHours / (24 * 36525);

            // Solar Coordinates
            double meanLongitude = CorrectAngle(280.466 + 36000.77 * julianCenturies);

            double meanAnomaly = CorrectAngle(357.529 + 35999.05 * julianCenturies);

            double equationOfCenter = (1.915 - 0.005 * julianCenturies) *
                Math.Sin(meanAnomaly) + 0.02 * Math.Sin(2 * meanAnomaly);

            double ellipticalLongitude = CorrectAngle(Mathf.Rad2Deg * meanLongitude + equationOfCenter);
            var ellipticalLongitudeSin = Math.Sin(ellipticalLongitude);
            var ellipticalLongitudeCos = Math.Cos(ellipticalLongitude);

            double obliquity = (23.439 - 0.013 * julianCenturies) * Mathf.Deg2Rad;
            var obliquitySin = Math.Sin(obliquity);
            var obliquityCos = Math.Cos(obliquity);

            // Right Ascension
            double rightAscension = Math.Atan2(
                obliquityCos * ellipticalLongitudeSin,
                ellipticalLongitudeCos);

            double declination = Math.Asin(Math.Sin(rightAscension) * obliquitySin);

            // Horizontal Coordinates
            double hourAngle = CorrectAngle(siderealTime) - rightAscension;

            if (hourAngle > Math.PI)
            {
                hourAngle -= 2 * Math.PI;
            }

            var hourAngleSin = Math.Sin(hourAngle);
            var hourAngleCos = Math.Cos(hourAngle);
            var latitudeSin = Math.Sin(latitude);
            var latitudeCos = Math.Cos(latitude);
            var declinationSin = Math.Sin(declination);
            var declinationCos = Math.Cos(declination);
            var declinationTan = Math.Tan(declination);

            altitude = Math.Asin(latitudeSin * declinationSin
                + latitudeCos * declinationCos * hourAngleCos);

            // Nominator and denominator for calculating Azimuth
            // angle. Needed to test which quadrant the angle is in.
            double aziNom = -hourAngleSin;
            double aziDenom = declinationTan * latitudeCos - latitudeSin * hourAngleCos;

            azimuth = Math.Atan(aziNom / aziDenom);

            if (aziDenom < 0) // In 2nd or 3rd quadrant
            {
                azimuth += Math.PI;
            }
            else if (aziNom < 0) // In 4th quadrant
            {
                azimuth += 2 * Math.PI;
            }
        }

        /// <returns>Correct Angle in Radians</returns>
        private static double CorrectAngle(double angleInDegrees)
        {
            if (angleInDegrees < 0)
            {
                return Mathf.Deg2Rad * (360 - Math.Abs(angleInDegrees) % 360);
            }

            if (angleInDegrees > 360)
            {
                return Mathf.Deg2Rad * (angleInDegrees % 360);
            }

            return Mathf.Deg2Rad * angleInDegrees;
        }

        /// <param name="t">Time in linear 0-1</param>
        public static DateTime NormalizedDateTime(double t,
            int year = 2000, int month = 1, int day = 1)
        {
            return new DateTime(year, month, day).AddDays(t);
        }

        /// <param name="t">Time in linear 0-1</param>
        public static float TimeToGradient(float t)
        {
            return Math.Abs(t * 2f - 1f);
        }
        #endregion // BoatAttack
    }
}
