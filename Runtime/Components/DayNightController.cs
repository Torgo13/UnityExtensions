using System;
using UnityEngine;

namespace PKGE
{
    //https://github.com/Unity-Technologies/BoatAttack/blob/e4864ca4381d59e553fe43f3dac6a12500eee8c7/Assets/Scripts/Environment/DayNightController.cs
    #region BoatAttack
    /// <summary>
    /// Simple day/night system
    /// </summary>
    public class DayNightController : MonoBehaviour
    {
        private static DayNightController _instance;
        [Range(0, 1)]
        public float time = 0.5f; // the global 'time'

        private readonly float[] _presets = { 0.27f, 0.35f, 0.45f, 0.55f, 0.65f, 0.73f };
        private int _currentPreset;
        private const string PresetKey = "DayNight.TimePreset";

        public bool autoIncrement;
        public float speed = 1f;

        public static float GlobalTime;

        // Skybox
        [Header("Skybox Settings")]
        public Material skybox; // skybox reference
        public Gradient skyboxColour; // skybox tint over time
        public ReflectionProbe[] reflections;

        // Sunlight
        [Header("Sun Settings")]
        public Light sun; // sunlight
        public Gradient sunColour; // sunlight colour over time
        [Range(0, 360)]
        public float northHeading = 136; // north

        //Ambient light
        [Header("Ambient Lighting")]
        public Gradient ambientColour; // ambient light colour over time

        // Fog
        [Header("Fog Settings")]
        [GradientUsage(hdr: true)]
        public Gradient fogColour; // fog colour over time

        // vars
        private float _prevTime; // previous time

        Transform sunTransform;
        
        static readonly int Rotation = Shader.PropertyToID("_Rotation");
        static readonly int Tint = Shader.PropertyToID("_Tint");
        static readonly int NightFade = Shader.PropertyToID("_NightFade");

        void Awake()
        {
            _instance = this;
            _currentPreset = 2;
            SetTimeOfDay(_presets[_currentPreset], reflectionUpdate: true);
            _prevTime = time;
        }

        private void OnValidate()
        {
            if (sun == null)
            {
                var lights = FindObjectsOfType<Light>();
                foreach (var l in lights)
                {
                    if (l != null
                        && l.type == LightType.Directional)
                    {
                        sun = l;
                        sunTransform = sun.transform;
                        break;
                    }
                }

                if (sun == null)
                    return;
            }
            
            UpdateSun();
        }

        void Update()
        {
            if (autoIncrement)
            {
                var t = Mathf.PingPong(Time.time * speed, 1);
                time = t * 0.5f + 0.25f;
            }

            if (!Mathf.Approximately(time, _prevTime)) // check if time has changed
            {
                SetTimeOfDay(time);
            }
        }

        void UpdateSun()
        {
            var rotation = CalculateSunPosition(NormalizedDateTime(time), 56.0, 9.0);
            sunTransform.rotation = rotation;
            sunTransform.Rotate(new Vector3(0f, northHeading, 0f), Space.World);
            sun.color = sunColour.Evaluate(Mathf.Clamp01(Vector3.Dot(sunTransform.forward, Vector3.down)));
        }

        /// <summary>
        /// Sets the time of day
        /// </summary>
        /// <param name="t">Time in linear 0-1</param>
        /// <param name="reflectionUpdate">Update reflection probes</param>
        public void SetTimeOfDay(float t, bool reflectionUpdate = false)
        {
            //Debug.Log($"Setting time of day to:{t}, updating reflectionProbes:{reflectionUpdate}");
            time = t;
            _prevTime = t;

            if (reflectionUpdate && _instance.reflections?.Length > 0)
            {
                foreach (var probe in _instance.reflections)
                {
                    probe.RenderProbe();
                }
            }

            GlobalTime = time;
            
            // do update
            if (sun)
            {
                sun.color = sunColour.Evaluate(TimeToGradient(time));
            }
            
            if (skybox)
            {
                // update skybox
                skybox.SetFloat(Rotation, 85 + (time - 0.5f) * 20f); // rotate slightly for a moving cloud effect
                skybox.SetColor(Tint, skyboxColour.Evaluate(TimeToGradient(time)));
            }

            Shader.SetGlobalFloat(NightFade, Mathf.Clamp01(Mathf.Abs(time * 2f - 1f) * 3f - 1f));
            RenderSettings.fogColor = fogColour.Evaluate(TimeToGradient(time)); // update fog colour
            RenderSettings.ambientSkyColor = ambientColour.Evaluate(TimeToGradient(time)); // update ambient light colour
        }

        public static Quaternion CalculateSunPosition(DateTime dateTime, double latitude, double longitude)
        {
            // Convert to UTC
            dateTime = dateTime.ToUniversalTime();

            // Number of days from J2000.0.
            double julianDate = 367 * dateTime.Year -
                (int)(7.0 / 4.0 * (dateTime.Year +
                                   (int)((dateTime.Month + 9.0) / 12.0))) +
                (int)(275.0 * dateTime.Month / 9.0) +
                dateTime.Day - 730531.5;

            double julianCenturies = julianDate / 36525.0;

            // Sidereal Time
            double siderealTimeHours = 6.6974 + 2400.0513 * julianCenturies;

            double siderealTimeUt = siderealTimeHours +
                366.2422 / 365.2422 * dateTime.TimeOfDay.TotalHours;

            double siderealTime = siderealTimeUt * 15 + longitude;

            // Refine to number of days (fractional) to specific time.
            julianDate += dateTime.TimeOfDay.TotalHours / 24.0;
            julianCenturies = julianDate / 36525.0;

            // Solar Coordinates
            double meanLongitude = CorrectAngle(Mathf.Deg2Rad *
                (280.466 + 36000.77 * julianCenturies));

            double meanAnomaly = CorrectAngle(Mathf.Deg2Rad *
                (357.529 + 35999.05 * julianCenturies));

            double equationOfCenter = Mathf.Deg2Rad * ((1.915 - 0.005 * julianCenturies) *
                Math.Sin(meanAnomaly) + 0.02 * Math.Sin(2 * meanAnomaly));

            double ellipticalLongitude =
                CorrectAngle(meanLongitude + equationOfCenter);

            double obliquity = (23.439 - 0.013 * julianCenturies) * Mathf.Deg2Rad;

            // Right Ascension
            double rightAscension = Math.Atan2(
                Math.Cos(obliquity) * Math.Sin(ellipticalLongitude),
                Math.Cos(ellipticalLongitude));

            double declination = Math.Asin(
                Math.Sin(rightAscension) * Math.Sin(obliquity));

            // Horizontal Coordinates
            double hourAngle = CorrectAngle(siderealTime * Mathf.Deg2Rad) - rightAscension;

            if (hourAngle > Math.PI)
            {
                hourAngle -= 2 * Math.PI;
            }

            double altitude = Math.Asin(Math.Sin(latitude * Mathf.Deg2Rad) *
                Math.Sin(declination) + Math.Cos(latitude * Mathf.Deg2Rad) *
                Math.Cos(declination) * Math.Cos(hourAngle));

            // Nominator and denominator for calculating Azimuth
            // angle. Needed to test which quadrant the angle is in.
            double aziNom = -Math.Sin(hourAngle);
            double aziDenom =
                Math.Tan(declination) * Math.Cos(latitude * Mathf.Deg2Rad) -
                Math.Sin(latitude * Mathf.Deg2Rad) * Math.Cos(hourAngle);

            double azimuth = Math.Atan(aziNom / aziDenom);

            if (aziDenom < 0) // In 2nd or 3rd quadrant
            {
                azimuth += Math.PI;
            }
            else if (aziNom < 0) // In 4th quadrant
            {
                azimuth += 2 * Math.PI;
            }

            Quaternion rot = Quaternion.Euler(0f, (float)azimuth * Mathf.Rad2Deg, 0f);
            rot *= Quaternion.AngleAxis((float)(altitude * Mathf.Rad2Deg), Vector3.right);

            return rot;
        }

        private static double CorrectAngle(double angleInRadians)
        {
            if (angleInRadians < 0)
            {
                return 2 * Math.PI - Math.Abs(angleInRadians) % (2 * Math.PI);
            }
            
            if (angleInRadians > 2 * Math.PI)
            {
                return angleInRadians % (2 * Math.PI);
            }
            
            return angleInRadians;
        }

        static DateTime NormalizedDateTime(float t)
        {
            var hour = (int)Mathf.Repeat(t * 24, 24); // 0-24
            var minute = (int)Mathf.Repeat(t * 24 * 60, 60); //0-60
            return new DateTime(2021, 03, 23, hour, minute, 0);
        }

        static
        float TimeToGradient(float t)
        {
            return Mathf.Abs(t * 2f - 1f);
        }

        public static void SelectPreset(float input)
        {
            _instance._currentPreset += Mathf.RoundToInt(input);
            _instance._currentPreset = (int)Mathf.Repeat(_instance._currentPreset, _instance._presets.Length);
            PlayerPrefs.SetInt(PresetKey, _instance._currentPreset);
            _instance.SetTimeOfDay(_instance._presets[_instance._currentPreset], true);
        }
    }
    #endregion // BoatAttack
}