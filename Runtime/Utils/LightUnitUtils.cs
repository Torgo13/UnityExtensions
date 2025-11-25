using System;
using UnityEngine;

namespace PKGE
{
    public static class LightUtils
    {
        public static bool GetDirectionalLight(out Light sun, out Transform sunTransform)
        {
            sun = RenderSettings.sun;
            if (sun != null)
            {
                sunTransform = sun.transform;
                return true;
            }

            sun = UnityEngine.Object.FindAnyObjectByType<Light>();
            if (sun != null
                && sun.type == LightType.Directional)
            {
                sunTransform = sun.transform;
                return true;
            }

#if UNITY_2022_3_OR_NEWER
            Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
#else
            Light[] lights = UnityEngine.Object.FindObjectsOfType<Light>();
#endif // UNITY_2022_3_OR_NEWER

            if (lights == null)
            {
                sunTransform = null;
                return false;
            }

            foreach (var light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    sunTransform = light.transform;
                    return true;
                }
            }

            sunTransform = null;
            return false;
        }
    }

    /// <summary>
    /// Light Unit Utils contains functions and definitions to facilitate conversion between different light intensity units.
    /// </summary>
    public static class LightUnitUtils
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Utilities/LightUnitUtils.cs
        #region UnityEngine.Rendering
        static float luminanceToEvFactor => Mathf.Log(100f / ColorUtils.LightMeterCalibrationConstant, 2);

        static float evToLuminanceFactor => -luminanceToEvFactor;

        /// <summary>
        /// The solid angle of a full sphere in steradians.
        /// </summary>
        public const float SphereSolidAngle = 4.0f * Mathf.PI;

        /// <summary>
        /// Get the solid angle of a Point light.
        /// </summary>
        /// <returns>4 * Pi steradians.</returns>
        public static float GetSolidAngleFromPointLight()
        {
            return SphereSolidAngle;
        }

        /// <summary>
        /// Get the solid angle of a Spotlight.
        /// </summary>
        /// <param name="spotAngle">The spot angle in degrees.</param>
        /// <returns>Solid angle in steradians.</returns>
        public static float GetSolidAngleFromSpotLight(float spotAngle)
        {
            double angle = Math.PI * spotAngle / 180.0;
            double solidAngle = 2.0 * Math.PI * (1.0 - Math.Cos(angle * 0.5));
            return (float)solidAngle;
        }

        /// <summary>
        /// Get the solid angle of a Pyramid light.
        /// </summary>
        /// <param name="spotAngle">The spot angle in degrees.</param>
        /// <param name="aspectRatio">The aspect ratio of the pyramid.</param>
        /// <returns>Solid angle in steradians.</returns>
        public static float GetSolidAngleFromPyramidLight(float spotAngle, float aspectRatio)
        {
            if (aspectRatio < 1.0f)
            {
                aspectRatio = (float)(1.0 / aspectRatio);
            }

            double angleA = Math.PI * spotAngle / 180.0;
            double length = Math.Tan(0.5 * angleA) * aspectRatio;
            double angleB = Math.Atan(length) * 2.0;
            double solidAngle = 4.0 * Math.Asin(Math.Sin(angleA * 0.5) * Math.Sin(angleB * 0.5));
            return (float)solidAngle;
        }

        /// <exception cref="ArgumentException">Thrown if solid angle is undefined for lights of type
        /// <paramref name="lightType"/>.</exception>
        internal static float GetSolidAngle(LightType lightType, bool spotReflector, float spotAngle, float aspectRatio)
        {
            return lightType switch
            {
                LightType.Spot => spotReflector ? GetSolidAngleFromSpotLight(spotAngle) : SphereSolidAngle,
                LightType.Point => GetSolidAngleFromPointLight(),
                _ => throw new ArgumentException("Solid angle is undefined for lights of type " + EnumValues<LightType>.Name(lightType))
            };
        }

        /// <summary>
        /// Get the projected surface area of a Rectangle light.
        /// </summary>
        /// <param name="rectSizeX">The width of the rectangle.</param>
        /// <param name="rectSizeY">The height of the rectangle.</param>
        /// <returns>Surface area.</returns>
        public static float GetAreaFromRectangleLight(float rectSizeX, float rectSizeY)
        {
            return Mathf.Abs(rectSizeX * rectSizeY) * Mathf.PI;
        }

        /// <summary>
        /// Get the projected surface area of a Rectangle light.
        /// </summary>
        /// <param name="rectSize">The size of the rectangle.</param>
        /// <returns>Projected surface area.</returns>
        public static float GetAreaFromRectangleLight(Vector2 rectSize)
        {
            return GetAreaFromRectangleLight(rectSize.x, rectSize.y);
        }

        /// <summary>
        /// Get the projected surface area of a Disc light.
        /// </summary>
        /// <param name="discRadius">The radius of the disc.</param>
        /// <returns>Projected surface area.</returns>
        public static float GetAreaFromDiscLight(float discRadius)
        {
            return discRadius * discRadius * Mathf.PI * Mathf.PI;
        }

        /// <summary>
        /// Get the projected surface area of a Tube light.
        /// </summary>
        /// <remarks>Note that Tube lights have no physical surface area.
        /// Instead, this method returns a value suitable for Nits&lt;=&gt;Lumen unit conversion.
        /// </remarks>
        /// <param name="tubeLength">The length of the tube.</param>
        /// <returns>4 * Pi * (tube length).</returns>
        public static float GetAreaFromTubeLight(float tubeLength)
        {
            // Line lights expect radiance (W / (sr * m^2)) in the shader.
            // In the UI, we specify luminous flux (power) in lumens.
            // First, it needs to be converted to radiometric units (radian flux, W).
            //
            // Then we must recall how to compute power from radiance:
            //
            // radiance = differential_power / (differential_projected_area * differential_solid_angle),
            // radiance = differential_power / (differential_area * differential_solid_angle * <N, L>),
            // power = Integral{area, Integral{hemisphere, radiance * <N, L>}}.
            //
            // Unlike line lights, our line lights have no surface area, so the integral becomes:
            //
            // power = Integral{length, Integral{sphere, radiance}}.
            //
            // For an isotropic line light, radiance is constant, therefore:
            //
            // power = length * (4 * Pi) * radiance,
            // radiance = power / (length * (4 * Pi)).

            return Mathf.Abs(tubeLength) * 4.0f * Mathf.PI;
        }

        /// <summary>
        /// Convert intensity in Lumen to Candela.
        /// </summary>
        /// <param name="lumen">Intensity in Lumen.</param>
        /// <param name="solidAngle">Light solid angle in steradians.</param>
        /// <returns>Intensity in Candela.</returns>
        public static float LumenToCandela(float lumen, float solidAngle)
        {
            return lumen / solidAngle;
        }

        /// <summary>
        /// Convert intensity in Candela to Lumen.
        /// </summary>
        /// <param name="candela">Intensity in Candela.</param>
        /// <param name="solidAngle">Light solid angle in steradians.</param>
        /// <returns>Intensity in Lumen.</returns>
        public static float CandelaToLumen(float candela, float solidAngle)
        {
            return candela * solidAngle;
        }

        /// <summary>
        /// Convert intensity in Lumen to Nits.
        /// </summary>
        /// <param name="lumen">Intensity in Lumen.</param>
        /// <param name="area">Projected surface area of the light source.</param>
        /// <returns>Intensity in Nits.</returns>
        public static float LumenToNits(float lumen, float area)
        {
            return lumen / area;
        }

        /// <summary>
        /// Convert intensity in Nits to Lumen.
        /// </summary>
        /// <param name="nits">Intensity in Nits.</param>
        /// <param name="area">Projected surface area of the light source.</param>
        /// <returns>Intensity in Lumen.</returns>
        public static float NitsToLumen(float nits, float area)
        {
            return nits * area;
        }

        /// <summary>
        /// Convert intensity in Lux to Candela.
        /// </summary>
        /// <param name="lux">Intensity in Lux.</param>
        /// <param name="distance">Distance between light and surface.</param>
        /// <returns>Intensity in Candela.</returns>
        public static float LuxToCandela(float lux, float distance)
        {
            return lux * (distance * distance);
        }

        /// <summary>
        /// Convert intensity in Candela to Lux.
        /// </summary>
        /// <param name="candela">Intensity in Lux.</param>
        /// <param name="distance">Distance between light and surface.</param>
        /// <returns>Intensity in Lux.</returns>
        public static float CandelaToLux(float candela, float distance)
        {
            return candela / (distance * distance);
        }

        /// <summary>
        /// Convert intensity in Ev100 to Nits.
        /// </summary>
        /// <param name="ev100">Intensity in Ev100.</param>
        /// <returns>Intensity in Nits.</returns>
        public static float Ev100ToNits(float ev100)
        {
            return Mathf.Pow(2.0f, ev100 + evToLuminanceFactor);
        }

        /// <summary>
        /// Convert intensity in Nits to Ev100.
        /// </summary>
        /// <param name="nits">Intensity in Nits.</param>
        /// <returns>Intensity in Ev100.</returns>
        public static float NitsToEv100(float nits)
        {
            return Mathf.Log(nits, 2) + luminanceToEvFactor;
        }

        /// <summary>
        /// Convert intensity in Ev100 to Candela.
        /// </summary>
        /// <param name="ev100">Intensity in Ev100.</param>
        /// <returns>Intensity in Candela.</returns>
        public static float Ev100ToCandela(float ev100)
        {
            return Ev100ToNits(ev100);
        }

        /// <summary>
        /// Convert intensity in Candela to Ev100.
        /// </summary>
        /// <param name="candela">Intensity in Candela.</param>
        /// <returns>Intensity in Ev100.</returns>
        public static float CandelaToEv100(float candela)
        {
            return NitsToEv100(candela);
        }
        #endregion // UnityEngine.Rendering
    }
}
