using UnityEngine;
using UnityEngine.Pool;

namespace UnityExtensions
{
    public static class MaterialExtensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/MaterialUtils.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Adds a material to this renderer's array of shared materials.
        /// </summary>
        /// <param name="renderer">The renderer on which to add the material.</param>
        /// <param name="material">The material to add.</param>
        public static void AddMaterial(this Renderer renderer, Material material)
        {
            var materials = ListPool<Material>.Get();
            renderer.GetSharedMaterials(materials);
            materials.Add(material);
            renderer.SetSharedMaterials(materials);
            ListPool<Material>.Release(materials);
        }
        #endregion // Unity.XR.CoreUtils
    }
}
