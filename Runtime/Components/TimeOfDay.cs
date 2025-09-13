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
        [Tooltip("Time of day normalized between 0 and 24h. For example 6.5 amount to 6:30am.")]
        public float timeOfDay = 12f;

        [SerializeField]
        [Tooltip("Sets the speed at which the time of day passes.")]
        float timeSpeed = 1f;

        // Paris Office coordinates. 
        public float latitude = 48.83402f;
        public float longitude = 2.367259f;

        // Arbitrary date to have the sunset framed in the camera frustum. 
        readonly DateTime _date = new DateTime(2024, 4, 21).Date;
        DateTime _time;

        [SerializeField, HideInInspector]
	    private GUIStyle sliderStyle;

        private static TimeOfDay _instance;

	    private void OnEnable()
	    {
            _instance = this;
	    }

	    private void Awake()
        {
            GetHoursMinutesSecondsFromTimeOfDay(out var hours, out var minutes, out _);
            _time = _date + new TimeSpan(hours, minutes, 0);
        }

        private void OnValidate()
        {
            GetHoursMinutesSecondsFromTimeOfDay(out var hours, out var minutes, out var seconds);
            _time = _date + new TimeSpan(hours, minutes, seconds);
            
            SetSunPosition();
        }

        void Update()
        {
            timeOfDay += timeSpeed * Time.deltaTime;

            //This is for the variable to loop for easier use.
            if (timeOfDay > 24f)
                timeOfDay = 0f;

            if (timeOfDay < 0f)
                timeOfDay = 24f;

            SetSunPosition();
        }

        void SetSunPosition()
        {
            CalculateSunPosition(_time.DayOfYear, latitude * Mathf.Deg2Rad, timeOfDay,
                out var azi, out var alt);

            if (float.IsNaN(azi))
                azi = transform.localRotation.y;

            Vector3 angles = new Vector3(alt, azi, 0);
            transform.localRotation = Quaternion.Euler(angles);
        }

        private void CalculateSunPosition(int dayOfYear, float latRad, float localSolarTime,
            out float azimuth, out float altitude)
        {
            float declination = -23.45f * Mathf.Cos(Mathf.PI * 2f * (dayOfYear + 10) / 365f);

            float localHourAngle = 15f * (localSolarTime - 12f);
            localHourAngle *= Mathf.Deg2Rad;

            declination *= Mathf.Deg2Rad;

            float latSin = Mathf.Sin(latRad);
            float latCos = Mathf.Cos(latRad);

            float hourCos = Mathf.Cos(localHourAngle);

            float declinationSin = Mathf.Sin(declination);
            float declinationCos = Mathf.Cos(declination);

            float elevation = Mathf.Asin(declinationSin * latSin + declinationCos * latCos * hourCos);
            float elevationCos = Mathf.Cos(elevation);
            azimuth = Mathf.Acos((declinationSin * latCos - declinationCos * latSin * hourCos) / elevationCos);

            elevation *= Mathf.Rad2Deg;
            azimuth *= Mathf.Rad2Deg;

            if (localHourAngle >= 0f)
                azimuth = 360 - azimuth;

            altitude = elevation;
        }

        private void GetHoursMinutesSecondsFromTimeOfDay(out int hours, out int minutes, out int seconds)
        {
            hours = (int)timeOfDay;
            minutes = (int)((timeOfDay - hours) * 60f);
            seconds = (int)((timeOfDay - hours - (minutes / 60f)) * 60f * 60f);
        }

        #if UNITY_EDITOR
        void OnGUI()
        {
            DrawWindow();

            // Force repaint of game view
            Type type = ReflectionUtils.FindTypeByFullName("UnityEditor.GameView");
            UnityEditor.EditorUtility.SetDirty(UnityEditor.EditorWindow.GetWindow(type, false, null, false));
        }

        internal void DrawWindow()
        {
            UnityEditor.Handles.BeginGUI();

            const float windowHeight = 15 + 30;
            GUI.Window(0, new Rect(Screen.width * 0.1f, Screen.height * 0.89f, Screen.width * 0.8f,
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
}
