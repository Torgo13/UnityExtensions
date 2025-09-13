using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Pool;
using UnityObject = UnityEngine.Object;

namespace PKGE
{
    /// <summary>
    /// Utility methods for creating GameObjects.
    /// Allows systems to subscribe to <see cref="OnGameObjectInstantiated"/>.
    /// </summary>
    public static class GameObjectUtils
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/GameObjectUtils.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Called when a GameObject has been instantiated through the <see cref="GameObjectUtils"/> versions of
        /// <see cref="UnityObject.Instantiate(UnityObject)"/>.
        /// </summary>
        public static event Action<GameObject> OnGameObjectInstantiated;

        /// <summary>
        /// Creates a new GameObject and returns it.
        /// This method also calls <see cref="OnGameObjectInstantiated"/>.
        /// </summary>
        /// <returns>The new GameObject.</returns>
        public static GameObject Create()
        {
            var gameObject = new GameObject();
            OnGameObjectInstantiated?.Invoke(gameObject);
            return gameObject;
        }

        /// <summary>
        /// Creates a new GameObject and returns it.
        /// This method also calls <see cref="OnGameObjectInstantiated"/>.
        /// </summary>
        /// <param name="name">The name to be given to the new GameObject.</param>
        /// <returns>The new GameObject.</returns>
        public static GameObject Create(string name)
        {
            var gameObject = new GameObject(name);
            OnGameObjectInstantiated?.Invoke(gameObject);
            return gameObject;
        }

        /// <summary>
        /// Clones the GameObject, <paramref name="original"/>, and returns the clone.
        /// This method also calls <see cref="OnGameObjectInstantiated"/>.
        /// </summary>
        /// <seealso cref="UnityObject.Instantiate(UnityObject, Transform, bool)"/>
        /// <param name="original">An existing GameObject that you want to make a copy of.</param>
        /// <param name="parent">Parent <see cref="Transform"/> to assign to the new object.</param>
        /// <param name="worldPositionStays">Set <see langword="true"/> to instantiate the new object in world space,
        /// which places it in the same position as the cloned GameObject,
        /// or to offset the new object from <paramref name="parent"/>.</param>
        /// <returns>The instantiated clone.</returns>
        public static GameObject Instantiate(GameObject original, Transform parent = null, bool worldPositionStays = true)
        {
            var gameObject = UnityObject.Instantiate(original, parent, worldPositionStays);
            if (gameObject != null && OnGameObjectInstantiated != null)
                OnGameObjectInstantiated(gameObject);

            return gameObject;
        }

        /// <summary>
        /// Clones the GameObject, <paramref name="original"/>, and returns the clone.
        /// This method also calls <see cref="OnGameObjectInstantiated"/>.
        /// </summary>
        /// <seealso cref="UnityObject.Instantiate(UnityObject, Vector3, Quaternion)"/>
        /// <param name="original">An existing GameObject that you want to make a copy of.</param>
        /// <param name="position">Position for the new object.</param>
        /// <param name="rotation">Orientation of the new object.</param>
        /// <returns>The instantiated clone.</returns>
        public static GameObject Instantiate(GameObject original, Vector3 position, Quaternion rotation)
        {
            return Instantiate(original, null, position, rotation);
        }

        /// <summary>
        /// Clones the GameObject, <paramref name="original"/>, and returns the clone.
        /// This method also calls <see cref="OnGameObjectInstantiated"/>.
        /// </summary>
        /// <seealso cref="UnityObject.Instantiate(UnityObject, Vector3, Quaternion, Transform)"/>
        /// <param name="original">An existing GameObject that you want to make a copy of</param>
        /// <param name="position">Position for the new object.</param>
        /// <param name="rotation">Orientation of the new object.</param>
        /// <param name="parent">Parent that will be assigned to the new object</param>
        /// <returns>The instantiated clone</returns>
        public static GameObject Instantiate(GameObject original, Transform parent, Vector3 position, Quaternion rotation)
        {
            var gameObject = UnityObject.Instantiate(original, position, rotation, parent);
            if (gameObject != null && OnGameObjectInstantiated != null)
                OnGameObjectInstantiated(gameObject);

            return gameObject;
        }

        /// <summary>
        /// Clones the Game Object <paramref name="original"/> and copies the hide flags of each Game Object
        /// in its hierarchy to the corresponding Game Object in the copy's hierarchy.
        /// </summary>
        /// <seealso cref="UnityObject.Instantiate(UnityObject, Transform)"/>
        /// <param name="original">The Game Object to make a copy of</param>
        /// <param name="parent">Optional parent that will be assigned to the clone of the original Game Object</param>
        /// <returns>The clone of the original Game Object</returns>
        public static GameObject CloneWithHideFlags(GameObject original, Transform parent = null)
        {
            var copy = UnityObject.Instantiate(original, parent);
            CopyHideFlagsRecursively(original, copy);
            return copy;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Clones the Prefab Game Object <paramref name="prefab"/> and copies the hide flags of each Game Object
        /// in its hierarchy to the corresponding Game Object in the copy's hierarchy.
        /// </summary>
        /// <seealso cref="PrefabUtility.InstantiatePrefab(UnityObject, Transform)"/>
        /// <param name="prefab">The Prefab Game Object to make a copy of</param>
        /// <param name="parent">Optional parent that will be assigned to the clone of the original Game Object</param>
        /// <returns>The clone of the original Game Object</returns>
        public static GameObject ClonePrefabWithHideFlags(GameObject prefab, Transform parent = null)
        {
            var copy = UnityEditor.PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            CopyHideFlagsRecursively(prefab, copy);
            return copy;
        }
#endif

        static void CopyHideFlagsRecursively(GameObject copyFrom, GameObject copyTo)
        {
            CopyHideFlagsRecursively(copyFrom.transform, copyTo.transform);
        }

        static void CopyHideFlagsRecursively(Transform copyFrom, Transform copyTo)
        {
            copyTo.hideFlags = copyFrom.hideFlags;
            for (var i = 0; i < copyFrom.childCount; ++i)
            {
                CopyHideFlagsRecursively(copyFrom.GetChild(i), copyTo.GetChild(i));
            }
        }

        /// <summary>
        /// Searches for a component in a scene with a 3-step process, getting more comprehensive with each step
        /// At edit time will find *all* objects in the scene, even if they are disabled
        /// At play time, will be unable to find disabled objects that are not a child of desiredSource
        /// </summary>
        /// <typeparam name="T">The type of component to find in the scene</typeparam>
        /// <param name="desiredSource">The Game Object we expect to be a parent or owner of the component</param>
        /// <returns>A component of the desired type, or NULL if no component was located</returns>
        public static T ExhaustiveComponentSearch<T>(GameObject desiredSource) where T : Component
        {
            var foundObject = default(T);

            // We check in the following order
            // - Location we expect the object to be
            // - The entire scene
            // - All loaded assets (Editor Only)
            if (desiredSource != null)
                foundObject = desiredSource.GetComponentInChildren<T>(true);

            if (foundObject == null)
            {
#if UNITY_2022_3_OR_NEWER
                foundObject = UnityObject.FindAnyObjectByType<T>();
#else
                foundObject = UnityObject.FindObjectOfType<T>();
#endif
            }

            if (foundObject != null)
                return foundObject;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var matchingObjects = Resources.FindObjectsOfTypeAll<T>();
                foreach (var possibleMatch in matchingObjects)
                {
                    if (!UnityEditor.EditorUtility.IsPersistent(possibleMatch))
                    {
                        foundObject = possibleMatch;
                        break;
                    }
                }
            }
#endif
            return foundObject;
        }

        /// <summary>
        /// Searches for a component in a scene with a 3-step process, getting more comprehensive with each step
        /// At edit time will find *all* objects in the scene, even if they are disabled
        /// At play time, will be unable to find disabled objects that are not a child of desiredSource
        /// </summary>
        /// <typeparam name="T">The type of component to find in the scene</typeparam>
        /// <param name="desiredSource">The GameObject we expect to be a parent or owner of the component</param>
        /// <param name="tag">The tag this component must have to match</param>
        /// <returns>A component of the desired type, or NULL if no component was located</returns>
        public static T ExhaustiveTaggedComponentSearch<T>(GameObject desiredSource, string tag) where T : Component
        {
            var foundObject = default(T);

            // We check in the following order
            // - Location we expect the object to be
            // - The entire scene
            // - All loaded assets (Editor Only)
            if (desiredSource != null)
            {
                var matchingObjects = ListPool<T>.Get();
                desiredSource.GetComponentsInChildren(includeInactive: true, matchingObjects);
                foreach (var possibleMatch in matchingObjects)
                {
                    if (possibleMatch.gameObject.CompareTag(tag))
                    {
                        foundObject = possibleMatch;
                        break;
                    }
                }

                ListPool<T>.Release(matchingObjects);
            }

            if (foundObject == null)
            {
                var matchingObjects = GameObject.FindGameObjectsWithTag(tag);
                foreach (var possibleMatch in matchingObjects)
                {
                    if (possibleMatch.TryGetComponent(out foundObject))
                    {
                        break;
                    }
                }
            }

            if (foundObject == null)
            {
#if UNITY_2022_3_OR_NEWER
                foundObject = UnityObject.FindAnyObjectByType<T>();
#else
                foundObject = UnityObject.FindObjectOfType<T>();
#endif
            }

#if UNITY_EDITOR
            if (foundObject == null && !Application.isPlaying)
            {
                var loadedMatchingObjects = Resources.FindObjectsOfTypeAll<T>();
                foreach (var possibleMatch in loadedMatchingObjects)
                {
                    if (!UnityEditor.EditorUtility.IsPersistent(possibleMatch) && possibleMatch.gameObject.CompareTag(tag))
                    {
                        foundObject = possibleMatch;
                        break;
                    }
                }
            }
#endif
            return foundObject;
        }

        /// <summary>
        /// Retrieves the first component of the given type in a scene
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve</typeparam>
        /// <param name="scene">The scene to search</param>
        /// <returns>The first component found in the active scene, or null if none exists</returns>
        public static T GetComponentInScene<T>(Scene scene) where T : Component
        {
            using var _0 = ListPool<GameObject>.Get(out var gameObjects);
            scene.GetRootGameObjects(gameObjects);
            foreach (var gameObject in gameObjects)
            {
                var component = gameObject.GetComponentInChildren<T>();
                if (component)
                    return component;
            }

            return null;
        }

        /// <summary>
        /// Retrieves all components of the given type in a scene
        /// </summary>
        /// <typeparam name="T">The type of components to retrieve</typeparam>
        /// <param name="scene">The scene to search</param>
        /// <param name="components">List that will be filled out with components retrieved</param>
        /// <param name="includeInactive">Should Components on inactive GameObjects be included in the found set?</param>
        public static void GetComponentsInScene<T>(Scene scene, List<T> components, bool includeInactive = false)
            where T : Component
        {
            var gameObjects = ListPool<GameObject>.Get();
            var children = ListPool<T>.Get();
            scene.GetRootGameObjects(gameObjects);
            foreach (var gameObject in gameObjects)
            {
                if (!includeInactive && !gameObject.activeInHierarchy)
                    continue;

                children.Clear();
                gameObject.GetComponentsInChildren(includeInactive, children);
                components.AddRange(children);
            }

            ListPool<GameObject>.Release(gameObjects);
            ListPool<T>.Release(children);
        }

        /// <summary>
        /// Retrieves the first component of the given type in the active scene
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve</typeparam>
        /// <returns>The first component found in the active scene, or null if none exists</returns>
        public static T GetComponentInActiveScene<T>() where T : Component
        {
            return GetComponentInScene<T>(SceneManager.GetActiveScene());
        }

        /// <summary>
        /// Retrieves all components of the given type in the active scene
        /// </summary>
        /// <typeparam name="T">The type of components to retrieve</typeparam>
        /// <param name="components">List that will be filled out with components retrieved</param>
        /// <param name="includeInactive">Should Components on inactive GameObjects be included in the found set?</param>
        public static void GetComponentsInActiveScene<T>(List<T> components, bool includeInactive = false)
            where T : Component
        {
            GetComponentsInScene(SceneManager.GetActiveScene(), components, includeInactive);
        }

        /// <summary>
        /// Retrieves all components of the given type in all loaded scenes
        /// </summary>
        /// <typeparam name="T">The type of components to retrieve</typeparam>
        /// <param name="components">List that will be filled out with components retrieved</param>
        /// <param name="includeInactive">Should Components on inactive GameObjects be included in the found set?</param>
        public static void GetComponentsInAllScenes<T>(List<T> components, bool includeInactive = false)
            where T : Component
        {
            var sceneCount = SceneManager.sceneCount;
            for (var i = 0; i < sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                    GetComponentsInScene(scene, components, includeInactive);
            }
        }

        /// <summary>
        /// Get the direct children GameObjects of this GameObject.
        /// </summary>
        /// <param name="go">The parent GameObject that we will want to get the child GameObjects on.</param>
        /// <param name="childGameObjects">The direct children of a GameObject.</param>
        public static void GetChildGameObjects(this GameObject go, List<GameObject> childGameObjects)
        {
            var goTransform = go.transform;
            var childCount = goTransform.childCount;
            if (childCount == 0)
                return;

            childGameObjects.EnsureCapacity(childCount);
            for (var i = 0; i < childCount; i++)
            {
                childGameObjects.Add(goTransform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Gets a descendant GameObject with a specific name
        /// </summary>
        /// <param name="go">The parent object that is searched for a named child.</param>
        /// <param name="name">Name of child to be found.</param>
        /// <returns>The returned child GameObject or null if no child is found.</returns>
        public static GameObject GetNamedChild(this GameObject go, string name)
        {
            List<Transform> transforms = ListPool<Transform>.Get();
            go.GetComponentsInChildren(transforms);
            Transform foundObject = null;
            foreach (var currentTransform in transforms)
            {
                if (string.Equals(currentTransform.name, name, StringComparison.Ordinal))
                {
                    foundObject = currentTransform;
                    break;
                }
            }
            
            ListPool<Transform>.Release(transforms);

            if (foundObject != null)
                return foundObject.gameObject;

            return null;
        }
        #endregion // Unity.XR.CoreUtils
    }
}
