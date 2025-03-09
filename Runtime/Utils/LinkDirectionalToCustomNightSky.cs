using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExtensions
{
    [ExecuteAlways]
    public class LinkDirectionalToCustomNightSky : MonoBehaviour
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Samples~/FullscreenSamples/Scripts/LinkDirectionalToCustomNightSky.cs
        #region UnityEngine.Rendering
        public Material SkyMat;
        Vector3 Dir;
        public bool update = true;
        [SerializeField] Light mainLight;
        float previousIntensity;
        Color previousColor;
        private static readonly int MoonlightForwardDirection = Shader.PropertyToID("_Moonlight_Forward_Direction");

        void OnEnable()
        {
            if (mainLight == null)
            {
                //Find a directional light
                var lights = FindObjectsOfType<Light>();
                foreach (var light in lights)
                {
                    if (light.type == LightType.Directional)
                    {
                        mainLight = light;
                        break;
                    }
                }
            }

            if (mainLight != null)
            {
                //This is to force the mainlight to specific intensity and color for good presentation
                previousIntensity = mainLight.intensity;
                previousColor = mainLight.color;
                mainLight.intensity = 1000f;
                mainLight.color = new Color(0.5f, 0.75f, 1f, 1f);
            }
        }

        void OnDisable()
        {
            //Reverting the forced values
            if (mainLight != null)
            {
                mainLight.intensity = previousIntensity;
                mainLight.color= previousColor;
            }
        }

        void Update()
        {
            if (update
                && mainLight != null)
            {
                //Sending the forward vector to the material           
                Dir = mainLight.gameObject.transform.forward;
                SkyMat.SetVector(MoonlightForwardDirection, Dir);
            }
        }
        #endregion // UnityEngine.Rendering
    }
}
