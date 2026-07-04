#nullable enable
using System;
using UnityEngine;

namespace PKGE
{
    /// <summary>
    /// Simple day/night system
    /// </summary>
    public class DayNightController : MonoBehaviour
    {
        //https://github.com/Unity-Technologies/BoatAttack/blob/e4864ca4381d59e553fe43f3dac6a12500eee8c7/Assets/Scripts/Environment/DayNightController.cs
        #region BoatAttack
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
        public Light? sun; // sunlight
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
        
        static readonly int Rotation = Shader.PropertyToID("_Rotation");
        static readonly int Tint = Shader.PropertyToID("_Tint");
        static readonly int NightFade = Shader.PropertyToID("_NightFade");

        #region MonoBehaviour
        void Awake()
        {
            _instance = this;
            _currentPreset = 2;
            SetTimeOfDay(_presets[_currentPreset], reflectionUpdate: true);
            _prevTime = time;
        }

        private void OnValidate()
        {
            if (sun == null && !LightUtils.GetDirectionalLight(out sun, FindObjectsInactive.Include))
                return;

            TimeOfDayUtils.UpdateSun(sun.transform, sun, sunColour, time, northHeading);
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
        #endregion // MonoBehaviour

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
                    _ = probe.RenderProbe();
                }
            }

            GlobalTime = time;
            
            // do update
            if (sun)
            {
                sun.color = sunColour.Evaluate(TimeOfDayUtils.TimeToGradient(time));
            }
            
            if (skybox)
            {
                // update skybox
                skybox.SetFloat(Rotation, 85 + (time - 0.5f) * 20f); // rotate slightly for a moving cloud effect
                skybox.SetColor(Tint, skyboxColour.Evaluate(TimeOfDayUtils.TimeToGradient(time)));
            }

            Shader.SetGlobalFloat(NightFade, Mathf.Clamp01(Mathf.Abs(time * 2f - 1f) * 3f - 1f));
            RenderSettings.fogColor = fogColour.Evaluate(TimeOfDayUtils.TimeToGradient(time)); // update fog colour
            RenderSettings.ambientSkyColor = ambientColour.Evaluate(TimeOfDayUtils.TimeToGradient(time)); // update ambient light colour
        }

        public static void SelectPreset(float input)
        {
            _instance._currentPreset += Mathf.RoundToInt(input);
            _instance._currentPreset = (int)Mathf.Repeat(_instance._currentPreset, _instance._presets.Length);
            PlayerPrefs.SetInt(PresetKey, _instance._currentPreset);
            _instance.SetTimeOfDay(_instance._presets[_instance._currentPreset], true);
        }
        #endregion // BoatAttack
    }
}
