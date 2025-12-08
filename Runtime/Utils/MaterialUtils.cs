using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Pool;
using UnityObject = UnityEngine.Object;

#if INCLUDE_UGUI
using UnityEngine.UI;
#endif

namespace PKGE
{
    /// <summary>
    /// Runtime Material utilities.
    /// </summary>
    public static class MaterialUtils
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/MaterialUtils.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Clones and replaces the material assigned to a <see cref="Renderer"/>.
        /// </summary>
        /// <remarks>
        /// > [!WARNING]
        /// > You must call <see cref="CoreUtils.Destroy(UnityObject, bool, bool, bool, bool, float)"/> on this material object when done.
        /// </remarks>
        /// <seealso cref="Renderer.material"/>
        /// <param name="renderer">The renderer assigned the material to clone.</param>
        /// <returns>The cloned material.</returns>
        public static Material GetMaterialClone(Renderer renderer)
        {
            // The following is equivalent to renderer.material, but gets rid of the error messages in edit mode
            return renderer.material = UnityObject.Instantiate(renderer.sharedMaterial);
        }

#if INCLUDE_UGUI
        /// <summary>
        /// Clones and replaces the material assigned to a <see cref="Graphic"/>.
        /// </summary>
        /// <remarks>
        /// To use this function, your project must contain the
        /// [Unity UI package (com.unity.ugui)](https://docs.unity3d.com/Manual/com.unity.ugui.html).
        /// > [!WARNING]
        /// > You must call <see cref="CoreUtils.Destroy(UnityObject, bool, bool, bool, bool, float)"/> on this material object when done.
        /// </remarks>
        /// <seealso cref="Graphic.material"/>
        /// <param name="graphic">The Graphic object assigned the material to clone.</param>
        /// <returns>Cloned material</returns>
        public static Material GetMaterialClone(Graphic graphic)
        {
            // The following is equivalent to graphic.material, but gets rid of the error messages in edit mode
            return graphic.material = UnityObject.Instantiate(graphic.material);
        }
#endif

        /// <summary>
        /// Clones and replaces all materials assigned to a <see cref="Renderer"/>
        /// </summary>
        /// <remarks>
        /// > [!WARNING]
        /// > You must call <see cref="CoreUtils.Destroy(UnityObject, bool, bool, bool, bool, float)"/> on each cloned material object
        /// in the array when done.
        /// </remarks>
        /// <seealso cref="Renderer.materials"/>
        /// <param name="renderer">Renderer assigned the materials to clone and replace.</param>
        /// <returns>Cloned materials</returns>
        public static Material[] CloneMaterials(Renderer renderer)
        {
            var sharedMaterials = ListPool<Material>.Get();
            CloneMaterials(sharedMaterials, renderer);
            var sharedMaterialsArray = sharedMaterials.ToArray();
            ListPool<Material>.Release(sharedMaterials);
            return sharedMaterialsArray;
        }

        /// <summary>
        /// Clones and replaces all materials assigned to a <see cref="Renderer"/>
        /// </summary>
        /// <remarks>
        /// > [!WARNING]
        /// > You must call <see cref="CoreUtils.Destroy(UnityObject, bool, bool, bool, bool, float)"/> on each cloned material object in the array when done.
        /// </remarks>
        /// <seealso cref="Renderer.materials"/>
        /// <param name="sharedMaterials">Cloned materials.</param>
        /// <param name="renderer">Renderer assigned the materials to clone and replace.</param>
        public static void CloneMaterials(List<Material> sharedMaterials, Renderer renderer)
        {
            renderer.GetSharedMaterials(sharedMaterials);
            for (var i = 0; i < sharedMaterials.Count; i++)
            {
                sharedMaterials[i] = UnityObject.Instantiate(sharedMaterials[i]);
            }

            renderer.SetSharedMaterials(sharedMaterials);
        }

        /// <summary>
        /// Converts an RGB or RGBA formatted hex string to a <see cref="Color32"/> object.
        /// </summary>
        /// <param name="hex">The formatted string, with an optional "0x" or "#" prefix.</param>
        /// <returns>The color value represented by the formatted string.</returns>
        public static Color32 HexToColor(string hex)
        {
            int startIndex = 0;
            if (hex.StartsWith('#'))
                startIndex = 1;
            else if (hex.StartsWith("0x", StringComparison.Ordinal))
                startIndex = 2;
            
            var r = byte.Parse(hex.AsSpan(startIndex, 2), NumberStyles.HexNumber);
            var g = byte.Parse(hex.AsSpan(startIndex + 2, 2), NumberStyles.HexNumber);
            var b = byte.Parse(hex.AsSpan(startIndex + 4, 2), NumberStyles.HexNumber);
            var a = hex.Length == startIndex + 8
                ? byte.Parse(hex.AsSpan(startIndex + 6, 2), NumberStyles.HexNumber)
                : (byte)255;

            return new Color32(r, g, b, a);
        }

        /// <summary>
        /// Shift the hue of a color by a given amount.
        /// </summary>
        /// <remarks>The hue value wraps around to 0 if the shifted hue exceeds 1.0.</remarks>
        /// <param name="color">The input color.</param>
        /// <param name="shift">The amount of shift.</param>
        /// <returns>The output color.</returns>
        public static Color HueShift(Color color, float shift)
        {
            Vector3 hsv;
            Color.RGBToHSV(color, out hsv.x, out hsv.y, out hsv.z);
            hsv.x = Mathf.Repeat(hsv.x + shift, 1f);
            return Color.HSVToRGB(hsv.x, hsv.y, hsv.z);
        }
        #endregion // Unity.XR.CoreUtils
        
        //https://github.com/Unity-Technologies/com.unity.probuilder/blob/d09b723d5d286217529e9f34a507015046b2a8a2/Runtime/Core/MaterialUtility.cs
        #region UnityEngine.ProBuilder
        public static int GetMaterialCount(Renderer renderer)
        {
            var materials = ListPool<Material>.Get();
            renderer.GetSharedMaterials(materials);
            var materialsCount = materials.Count;
            ListPool<Material>.Release(materials);
            return materialsCount;
        }

        public static Material GetSharedMaterial(Renderer renderer, int index)
        {
            using var _0 = ListPool<Material>.Get(out var materials);
            renderer.GetSharedMaterials(materials);
            var count = materials.Count;
            if (count < 1)
                return null;

            return materials[Mathf.Clamp(index, 0, count - 1)];
        }
        #endregion // UnityEngine.ProBuilder
    }
}