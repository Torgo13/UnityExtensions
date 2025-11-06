using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace PKGE
{
    /// <summary>
    /// Extensions methods for the <see cref="GameObject"/> class.
    /// </summary>
    public static class GameObjectExtensions
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Extensions/GameObjectExtensions.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Sets the hide flags on this GameObject and all of its descendants.
        /// </summary>
        /// <remarks>
        /// This function overwrites the existing flags of a <see cref="GameObject"/> with those specified by <paramref name="hideFlags"/>.
        /// </remarks>
        /// <param name="gameObject">The GameObject at the root of the hierarchy to be modified.</param>
        /// <param name="hideFlags">Should the GameObjects be hidden, saved with the scene, or modifiable by the user?</param>
        public static void SetHideFlagsRecursively(this GameObject gameObject, HideFlags hideFlags)
        {
            gameObject.hideFlags = hideFlags;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetHideFlagsRecursively(hideFlags);
            }
        }

        /// <summary>
        /// Adds <paramref name="hideFlags"/> to the hide flags on this GameObject and all of its descendants.
        /// </summary>
        /// <remarks>
        /// This function combines the <paramref name="hideFlags"/> with the existing flags of a <see cref="GameObject"/>.
        /// </remarks>
        /// <param name="gameObject">The GameObject at the root of the hierarchy to be modified.</param>
        /// <param name="hideFlags">Should the GameObjects be hidden, saved with the scene or modifiable by the user?</param>
        public static void AddToHideFlagsRecursively(this GameObject gameObject, HideFlags hideFlags)
        {
            gameObject.hideFlags |= hideFlags;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.AddToHideFlagsRecursively(hideFlags);
            }
        }

        /// <summary>
        /// Sets the layer of this GameObject and all of its descendants.
        /// </summary>
        /// <param name="gameObject">The GameObject at the root of the hierarchy to be modified.</param>
        /// <param name="layer">The layer to recursively assign GameObjects to.</param>
        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }

        /// <summary>
        /// Sets the layer of this GameObject and adds to its HideFlags, and does the same for all of its descendants.
        /// </summary>
        /// <remarks>
        /// This function combines the <paramref name="hideFlags"/> with the existing flags of a <see cref="GameObject"/>.
        /// </remarks>
        /// <param name="gameObject">The GameObject at the root of the hierarchy to be modified.</param>
        /// <param name="layer">The layer to recursively assign GameObjects to.</param>
        /// <param name="hideFlags">Should the GameObjects be hidden, saved with the scene, or modifiable by the user?</param>
        public static void SetLayerAndAddToHideFlagsRecursively(this GameObject gameObject, int layer, HideFlags hideFlags)
        {
            gameObject.layer = layer;
            gameObject.hideFlags |= hideFlags;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetLayerAndAddToHideFlagsRecursively(layer, hideFlags);
            }
        }

        /// <summary>
        /// Sets the layer and HideFlags of this GameObject and all of its descendants.
        /// </summary>
        /// <remarks>
        /// This function overwrites the existing flags of a <see cref="GameObject"/> with those specified by <paramref name="hideFlags"/>.
        /// </remarks>
        /// <param name="gameObject">The GameObject at the root of the hierarchy to be modified.</param>
        /// <param name="layer">The layer to recursively assign GameObjects to.</param>
        /// <param name="hideFlags">Should the GameObjects be hidden, saved with the scene, or modifiable by the user?</param>
        public static void SetLayerAndHideFlagsRecursively(this GameObject gameObject, int layer, HideFlags hideFlags)
        {
            gameObject.layer = layer;
            gameObject.hideFlags = hideFlags;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetLayerAndHideFlagsRecursively(layer, hideFlags);
            }
        }

        /// <summary>
        /// Sets <see cref="MonoBehaviour.runInEditMode"/> for all MonoBehaviours on this GameObject and its children.
        /// </summary>
        /// <param name="gameObject">The GameObject at the root of the hierarchy to be modified.</param>
        /// <param name="enabled">The value to assign to runInEditMode.</param>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void SetRunInEditModeRecursively(this GameObject gameObject, bool enabled)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;

            var monoBehaviours = ListPool<MonoBehaviour>.Get();
            gameObject.GetComponents(monoBehaviours);
            foreach (var mb in monoBehaviours)
            {
                if (mb != null)
                {
                    if (enabled)
                        mb.StartRunInEditMode();
                    else
                        mb.StopRunInEditMode();
                }
            }

            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetRunInEditModeRecursively(enabled);
            }

            ListPool<MonoBehaviour>.Release(monoBehaviours);
#endif
        }
        #endregion // Unity.XR.CoreUtils

        //https://github.com/needle-mirror/com.unity.entities/blob/7866660bdd3140414ffb634a962b4bad37887261/Unity.Entities.Hybrid/GameObjectConversion/UnityEngineExtensions.cs
        #region Unity.Entities.Conversion
        public static bool IsPrefab(this GameObject @this) =>
            !@this.scene.IsValid();

        public static bool IsAsset(this UnityEngine.Object @this) =>
            !(@this is GameObject) && !(@this is Component);

        public static bool IsActiveIgnorePrefab(this GameObject @this)
        {
            if (!@this.IsPrefab())
                return @this.activeInHierarchy;

            var parent = @this.transform;
            while (parent != null)
            {
                if (!parent.gameObject.activeSelf)
                    return false;

                parent = parent.parent;
            }

            return true;
        }

        public static bool IsComponentDisabled(this Component @this)
        {
            switch (@this)
            {
                case Renderer r:
                    return !r.enabled;
                case Collider c:
                    return !c.enabled;
                case LODGroup l:
                    return !l.enabled;
                case Behaviour b:
                    return !b.enabled;
            }

            return false;
        }

        public static bool GetComponentsBaking(this GameObject gameObject, List<Component> componentsCache)
        {
            int outputIndex = 0;
            gameObject.GetComponents(componentsCache);

            for (var i = 0; i != componentsCache.Count; i++)
            {
                var component = componentsCache[i];

                if (component == null)
                    Debug.LogWarning($"The referenced script is missing on {gameObject.name} (index {i} in components list)", gameObject);
                else
                {
                    componentsCache[outputIndex] = component;

                    outputIndex++;
                }
            }

            componentsCache.RemoveRange(outputIndex, componentsCache.Count - outputIndex);
            return true;
        }
        #endregion // Unity.Entities.Conversion

        /// <summary>
        /// Get the direct children GameObjects of this GameObject.
        /// </summary>
        /// <param name="go">The parent GameObject that we will want to get the child GameObjects on.</param>
        /// <param name="childGameObjects">The direct children of a GameObject.</param>
        /// <param name="recursive">Set to <see langword="true"/> to also get the descendents.</param>
        public static void GetChildGameObjects(this GameObject go, List<GameObject> childGameObjects,
            bool recursive = false)
        {
            var goTransform = go.transform;
            var childCount = goTransform.childCount;
            if (childCount == 0)
                return;

            childGameObjects.EnsureCapacity(childCount);
            for (var i = 0; i < childCount; i++)
            {
                childGameObjects.Add(goTransform.GetChild(i).gameObject);
                if (recursive)
                {
                    go.GetChildGameObjects(childGameObjects, recursive: true);
                }
            }
        }

        public static void GetChildInstanceIDs(this GameObject go, List<int> childInstanceIDs,
            bool recursive = false)
        {
            go.transform.GetChildInstanceIDs(childInstanceIDs, recursive);
        }

        public static void SetActiveRecursively(this GameObject go, bool active)
        {
            go.transform.SetActiveRecursively(active);
        }

        /// <summary>
        /// Gets a descendant GameObject with a specific name
        /// </summary>
        /// <param name="go">The parent object that is searched for a named child.</param>
        /// <param name="name">Name of child to be found.</param>
        /// <param name="found">True if a descendant GameObject with the specified name was found.</param>
        /// <returns>The returned child GameObject or null if no child is found.</returns>
        public static GameObject GetNamedChild(this GameObject go, string name, out bool found)
        {
            found = false;
            var transforms = ListPool<Transform>.Get();
            go.GetComponentsInChildren(transforms);
            GameObject foundObject = null;
            for (var i = 0; i < transforms.Count; i++)
            {
                if (transforms[i].name == name)
                {
                    found = true;
                    foundObject = transforms[i].gameObject;
                    break;
                }
            }

            ListPool<Transform>.Release(transforms);
            return foundObject;
        }

        public static void InstantiateGameObjects(this GameObject go, int count, List<GameObject> instances,
            UnityEngine.SceneManagement.Scene destinationScene = default)
        {
            Assert.IsTrue(count > 0);
            Assert.IsNotNull(go);
            Assert.IsNotNull(instances);

#if UNITY_6000_3_OR_NEWER
            var id = go.GetEntityId();
            var ids = new NativeArray<EntityId>(2 * count,
                Allocator.Temp, NativeArrayOptions.UninitializedMemory);
#else
            var id = go.GetInstanceID();
            var ids = new NativeArray<int>(2 * count,
                Allocator.Temp, NativeArrayOptions.UninitializedMemory);
#endif // UNITY_6000_3_OR_NEWER

            var instanceIDs = ids.GetSubArray(start: 0, length: count);
            var transformIDs = ids.GetSubArray(start: count, length: count);

            GameObject.InstantiateGameObjects(id,
                count, instanceIDs, transformIDs, destinationScene);

#if UNITY_6000_3_OR_NEWER
            Resources.EntityIdsToObjectList(instanceIDs, instances.As<GameObject, Object>());
#else
            Resources.InstanceIDToObjectList(instanceIDs, instances.As<GameObject, Object>());
#endif // UNITY_6000_3_OR_NEWER
        }
    }

    public static class AssetDatabaseExtensions
    {
        public static Object LoadAssetAtPath(string path)
        {
#if UNITY_EDITOR
            var type = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(path);
            return UnityEditor.AssetDatabase.LoadAssetAtPath(path, type);
#else
            return default;
#endif // UNITY_EDITOR
        }

        public static Object LoadAssetFromGUID(Union16 guid)
        {
#if UNITY_EDITOR
            return LoadAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(
                Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<Union16, UnityEditor.GUID>(ref guid)),
                GetMainAssetTypeFromGUID(guid));
#else
            return default;
#endif // UNITY_EDITOR
        }

        public static T LoadAssetFromGUID<T>(Union16 guid) where T : Object
        {
#if UNITY_EDITOR
            return LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(
                Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<Union16, UnityEditor.GUID>(ref guid)));
#else
            return default;
#endif // UNITY_EDITOR
        }

        public static Object LoadAssetFromGUID(string guid)
        {
#if UNITY_EDITOR
            return LoadAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
#else
            return default;
#endif // UNITY_EDITOR
        }

        public static T LoadAssetFromGUID<T>(string guid) where T : Object
        {
#if UNITY_EDITOR
            return LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
#else
            return default;
#endif // UNITY_EDITOR
        }

        #region UnityEditor
        public static string GetAssetPath(Object assetObject)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetPath(assetObject);
#else
            return string.Empty;
#endif // UNITY_EDITOR
        }

        public static string GetAssetPath(int instanceID)
        {
#if UNITY_EDITOR
#if UNITY_6000_3_OR_NEWER
            return UnityEditor.AssetDatabase.GetAssetPath((EntityId)instanceID);
#else
            return UnityEditor.AssetDatabase.GetAssetPath(instanceID);
#endif // UNITY_6000_3_OR_NEWER
#else
            return string.Empty;
#endif // UNITY_EDITOR
        }

        public static Object LoadAssetAtPath(string path, System.Type type)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath(path, type);
#else
            return default;
#endif // UNITY_EDITOR
        }

        public static T LoadAssetAtPath<T>(string assetPath) where T : Object
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
            return default;
#endif // UNITY_EDITOR
        }

        public static Object LoadMainAssetAtPath(string assetPath)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetPath);
#else
            return default;
#endif // UNITY_EDITOR
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void InstanceIDsToGUIDs(NativeArray<int> instanceIDs, NativeArray<Union16> guidsOut)
        {
#if UNITY_EDITOR
#if UNITY_6000_3_OR_NEWER
            UnityEditor.AssetDatabase.EntityIdsToGUIDs(instanceIDs.Reinterpret<EntityId>(), guidsOut.Reinterpret<UnityEditor.GUID>());
#else
            UnityEditor.AssetDatabase.InstanceIDsToGUIDs(instanceIDs, guidsOut.Reinterpret<UnityEditor.GUID>());
#endif // UNITY_6000_3_OR_NEWER
#endif // UNITY_EDITOR
        }

        public static System.Type GetMainAssetTypeAtPath(string assetPath)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(assetPath);
#else
            return default;
#endif // UNITY_EDITOR
        }

        public static System.Type GetMainAssetTypeFromGUID(Union16 guid)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetMainAssetTypeFromGUID(
                Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<Union16, UnityEditor.GUID>(ref guid));
#else
            return default;
#endif // UNITY_EDITOR
        }

        public static System.Type GetTypeFromPathAndFileID(string assetPath, long localIdentifierInFile)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetTypeFromPathAndFileID(assetPath, localIdentifierInFile);
#else
            return default;
#endif // UNITY_EDITOR
        }

        public static bool IsMainAssetAtPathLoaded(string assetPath)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.IsMainAssetAtPathLoaded(assetPath);
#else
            return default;
#endif // UNITY_EDITOR
        }

        public static string GUIDToAssetPath(string guid)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
#else
            return string.Empty;
#endif // UNITY_EDITOR
        }

        public static string GUIDToAssetPath(Union16 guid)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GUIDToAssetPath(
                Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<Union16, UnityEditor.GUID>(ref guid));
#else
            return string.Empty;
#endif // UNITY_EDITOR
        }

        public static Union16 GUIDFromAssetPath(string path)
        {
#if UNITY_EDITOR
            var guid = UnityEditor.AssetDatabase.GUIDFromAssetPath(path);
            return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<UnityEditor.GUID, Union16>(ref guid);
#else
            return default;
#endif // UNITY_EDITOR
        }

        public static string AssetPathToGUID(string path)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.AssetPathToGUID(path);
#else
            return default;
#endif // UNITY_EDITOR
        }

        public static Object[] LoadAllAssetsAtPath(string assetPath)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath);
#else
            return default;
#endif // UNITY_EDITOR
        }

        public static Object[] LoadAllAssetsFromGUID(string guid)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAllAssetsAtPath(
                UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
#else
            return default;
#endif // UNITY_EDITOR
        }

        public static Object[] LoadAllAssetsFromGUID(Union16 guid)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAllAssetsAtPath(
                GUIDToAssetPath(guid));
#else
            return default;
#endif // UNITY_EDITOR
        }

        public static Object[] LoadAllAssetRepresentationsAtPath(string assetPath)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
#else
            return default;
#endif // UNITY_EDITOR
        }

        public static Object[] LoadAllAssetRepresentationsFromGUID(string guid)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(
                UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
#else
            return default;
#endif // UNITY_EDITOR
        }

        public static Object[] LoadAllAssetRepresentationsFromGUID(Union16 guid)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(
                GUIDToAssetPath(guid));
#else
            return default;
#endif // UNITY_EDITOR
        }
        #endregion // UnityEditor
    }
}
