using UnityEngine;

namespace PKGE
{
    [ExecuteAlways]
    public class LinkDirectionalToCustomNightSky : MonoBehaviour
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Samples~/FullscreenSamples/Scripts/LinkDirectionalToCustomNightSky.cs
        #region UnityEngine.Rendering
        [SerializeField] Material skyMat;
        public bool update = true;
        [SerializeField] Light mainLight;
        Transform _mainLightTransform;
        float _previousIntensity;
        Color _previousColor;
        static readonly int MoonlightForwardDirection = Shader.PropertyToID("_Moonlight_Forward_Direction");
        
        Material SkyMat
        {
            get
            {
                if (skyMat == null)
                {
                    skyMat = RenderSettings.skybox;
                }
                return skyMat;
            }
        }

        void OnEnable()
        {
            if (mainLight == null)
            {
                //Find a directional light
#if UNITY_6000_3_OR_NEWER
                var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
#else
                var lights = FindObjectsOfType<Light>();
#endif // UNITY_6000_3_OR_NEWER
                foreach (var l in lights)
                {
                    if (l.type == LightType.Directional)
                    {
                        mainLight = l;
                        _mainLightTransform = mainLight.transform;
                        break;
                    }
                }
            }

            if (mainLight != null)
            {
                //Force the mainLight to specific intensity and color to approximate the Sun
                _previousIntensity = mainLight.intensity;
                _previousColor = mainLight.color;
                mainLight.intensity = 1000f;
                mainLight.color = new Color(0.5f, 0.75f, 1f, 1f);
            }
        }

        void OnDisable()
        {
            //Reverting the forced values
            if (mainLight != null)
            {
                mainLight.intensity = _previousIntensity;
                mainLight.color = _previousColor;
            }
        }

        void Update()
        {
            if (update
                && _mainLightTransform != null)
            {
                //Sending the forward vector to the material           
                var dir = _mainLightTransform.forward;
                SkyMat.SetVector(MoonlightForwardDirection, dir);
            }
        }
        #endregion // UnityEngine.Rendering
    }
}
