using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace PKGE
{
    /// <summary>
    /// Replicates some functionalities of SRP CoreUtils.
    /// </summary>
    /// <remarks>
    /// Introduced since SRP is an optional dependency.
    /// </remarks>
    public static class AdditionalCoreUtils
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/Core/Utilities/AdditionalCoreUtils.cs
        #region Unity.LiveCapture
        /// <summary>
        /// Creates and returns a reference to an empty GameObject.
        /// </summary>
        /// <remarks>
        /// This is a temporary workaround method. You might fail to create GameObjects via the `new GameObject()` method in some circumstances,
        /// for example when you invoke it in OnEnable through a component that you just added manually in the Inspector window,
        /// depending on the Editor configuration.
        /// See https://fogbugz.unity3d.com/f/cases/1196137/.
        /// </remarks>
        public static GameObject CreateEmptyGameObject()
        {
            var result = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // Strip all components but the transform to get an empty game object.
            List<Component> components = ListPool<Component>.Get();
            result.GetComponents(components);
            foreach (var component in components)
            {
                if (component is Transform)
                    continue;

                Object.DestroyImmediate(component);
            }

            ListPool<Component>.Release(components);
            return result;
        }
        #endregion // Unity.LiveCapture
    }
}
