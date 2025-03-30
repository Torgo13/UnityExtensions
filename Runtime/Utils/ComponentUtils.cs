using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityExtensions
{
    /// <summary>
    /// Special utility class for getting components in the editor without allocations.
    /// </summary>
    /// <typeparam name="T">The type of component for which to be searched.</typeparam>
    public static class ComponentUtils<T>
        where T : Component
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/ComponentUtils.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Gets a single component of type T using the non-allocating GetComponents API.
        /// </summary>
        /// <param name="gameObject">The GameObject from which to get the component.</param>
        /// <returns>The component, if one exists.</returns>
        public static T GetComponent(GameObject gameObject)
        {
            var foundComponent = default(T);
            List<T> retrievalList = ListPool<T>.Get();
            gameObject.GetComponents(retrievalList);
            if (retrievalList.Count > 0)
                foundComponent = retrievalList[0];

            ListPool<T>.Release(retrievalList);
            return foundComponent;
        }

        /// <summary>
        /// Gets a single component of type T using the non-allocating GetComponentsInChildren API.
        /// </summary>
        /// <param name="gameObject">The GameObject from which to get the component.</param>
        /// <returns>The component, if one exists.</returns>
        public static T GetComponentInChildren(GameObject gameObject)
        {
            var foundComponent = default(T);
            List<T> retrievalList = ListPool<T>.Get();
            gameObject.GetComponentsInChildren(retrievalList);
            if (retrievalList.Count > 0)
                foundComponent = retrievalList[0];

            ListPool<T>.Release(retrievalList);
            return foundComponent;
        }
        #endregion // Unity.XR.CoreUtils

        /// <summary>
        /// Gets a single component of type T using the non-allocating GetComponents API.
        /// </summary>
        /// <param name="gameObject">The GameObject from which to get the component.</param>
        /// <param name="foundComponent">Gets a single component of type T using the non-allocating GetComponents API.</param>
        /// <returns><see langword="true"/> if one exists.</returns>
        public static bool TryGetComponent(GameObject gameObject, out T foundComponent)
        {
            foundComponent = null;
            using var _0 = ListPool<T>.Get(out var retrievalList);
            gameObject.GetComponents(retrievalList);
            if (retrievalList.Count > 0)
            {
                foundComponent = retrievalList[0];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a single component of type T using the non-allocating GetComponentsInChildren API.
        /// </summary>
        /// <param name="gameObject">The GameObject from which to get the component.</param>
        /// <param name="foundComponent">Gets a single component of type T using the non-allocating GetComponentsInChildren API.</param>
        /// <returns><see langword="true"/> if one exists.</returns>
        public static bool TryGetComponentInChildren(GameObject gameObject, out T foundComponent)
        {
            foundComponent = null;
            using var _0 = ListPool<T>.Get(out var retrievalList);
            gameObject.GetComponentsInChildren(retrievalList);
            if (retrievalList.Count > 0)
            {
                foundComponent = retrievalList[0];
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Utility class for working with Components.
    /// </summary>
    public static class ComponentUtils
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/ComponentUtils.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Gets a component from a GameObject. Optionally, adds a new component and returns it
        /// if a component of the specified type does not already exist.
        /// </summary>
        /// <param name="gameObject">The parent GameObject.</param>
        /// <param name="add">Whether to add a new component of the given type, if one does not already exist.</param>
        /// <typeparam name="T">The type of component to get or add.</typeparam>
        /// <returns>The new or retrieved component.</returns>
        public static T GetOrAddIf<T>(GameObject gameObject, bool add) where T : Component
        {
            var component = gameObject.GetComponent<T>();
#if UNITY_EDITOR
            if (add && component == null)
                component = UnityEditor.Undo.AddComponent<T>(gameObject);
#else
            if (add && component == null)
                component = gameObject.AddComponent<T>();
#endif

            return component;
        }
        #endregion // Unity.XR.CoreUtils
    }
}
