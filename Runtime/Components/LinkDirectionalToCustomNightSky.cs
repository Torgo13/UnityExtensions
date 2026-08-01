#nullable enable
using UnityEngine;

namespace PKGE
{
    [ExecuteAlways]
    public class LinkDirectionalToCustomNightSky : MonoBehaviour
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Samples~/FullscreenSamples/Scripts/LinkDirectionalToCustomNightSky.cs
        #region UnityEngine.Rendering
        [SerializeField] Material? skyMat;
        public bool update = true;
        [SerializeField] Light? mainLight;
        float _previousIntensity;
        Color _previousColor;
        static readonly int MoonlightForwardDirection = Shader.PropertyToID("_Moonlight_Forward_Direction");
        
        bool MainLight([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Light? light)
        {
            light = mainLight;
            if (light == null && LightUtils.GetDirectionalLight(out light))
            {
                mainLight = light;
                return true;
            }

            return false;
        }

        bool SkyMat([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Material? sky)
        {
            sky = skyMat;
            if (sky == null)
            {
                skyMat = sky = RenderSettings.skybox;
            }

            return sky != null;
        }

        void OnEnable()
        {
            if (MainLight(out var mainLight))
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
            if (MainLight(out var mainLight))
            {
                mainLight.intensity = _previousIntensity;
                mainLight.color = _previousColor;
            }
        }

        void Update()
        {
            if (!update)
                return;

            if (MainLight(out var mainLight) && SkyMat(out var skyMat))
            {
                //Sending the forward vector to the material           
                Vector3 dir = mainLight.transform.forward;
                skyMat.SetVector(MoonlightForwardDirection, (Vector4)dir);
            }
        }
        #endregion // UnityEngine.Rendering
    }
}
