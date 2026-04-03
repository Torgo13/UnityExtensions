using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace PKGE.Editor
{
    public class FindUnusedAssets : EditorWindow
    {
        //https://github.com/Unity-Technologies/Megacity-2019/blob/1d90090d6d23417c661e7937e283b77b8e1db29d/Assets/Scripts/Utils/Editor/UnusedAssetsChecker.cs
        #region Unity.Megacity.EditorTools
        private static readonly string[] assetTypes = { "Prefab", "AudioClip", "Texture", "Model", "Material" };
        private Dictionary<string, bool> usedAssets = new Dictionary<string, bool>();
        private List<string> unusedAssets = new List<string>();
        private List<string> excludedPaths = new List<string> { "Packages" };

        [MenuItem("Assets/AssetDatabase/Check Unused Assets")]
        static void Init()
        {
            FindUnusedAssets window = (FindUnusedAssets)EditorWindow.GetWindow(typeof(FindUnusedAssets));
            window.Show();
        }

        void OnGUI()
        {
            if (GUILayout.Button("Check Unused Assets"))
            {
                CheckUnusedAssets();
            }

            if (unusedAssets.Count > 0)
            {
                GUILayout.Label("Unused Assets:");
                foreach (string assetPath in unusedAssets)
                {
                    GUILayout.Label(assetPath);
                }

                GUIUtility.systemCopyBuffer = string.Join(System.Environment.NewLine, unusedAssets);
            }
            else
            {
                GUILayout.Label("No unused assets found.");
            }
        }

        void CheckUnusedAssets()
        {
            usedAssets.Clear();
            unusedAssets.Clear();

#if UNITY_6000_3_OR_NEWER
            GUID[] guids = AssetDatabase.FindAssetGUIDs("t:Object", searchInFolders: null);
            foreach (GUID guid in guids)
#else
            string[] guids = AssetDatabase.FindAssets("t:Object", null);
            foreach (string guid in guids)
#endif // UNITY_6000_3_OR_NEWER
            {
#if UNITY_6000_3_OR_NEWER
                var type = AssetDatabase.GetMainAssetTypeFromGUID(guid);
                if (type == null)
                    continue;

                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path))
                    continue;
#else
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path))
                    continue;

                var type = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (type == null)
                    continue;
#endif // UNITY_6000_3_OR_NEWER

                string assetType = type.Name;
                if (assetTypes.Contains(assetType) && !excludedPaths.Any(p => path.Contains(p)))
                {
                    usedAssets[path] = false;
                }
            }

            foreach (string scenePath in EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path))
            {
                EditorUtility.DisplayProgressBar("Checking scenes for unused assets", scenePath, 0f);
                var sceneObject = SceneManager.GetSceneByPath(scenePath);

                if (!sceneObject.IsValid())
                    continue;

                foreach (GameObject gameObject in sceneObject.GetRootGameObjects())
                {
                    CheckObject(gameObject);
                }
            }

            string[] subscenePaths = AssetDatabase.FindAssets("t:SubScene");
            foreach (string subscenePath in subscenePaths)
            {
                EditorUtility.DisplayProgressBar("Checking subscenes for unused assets", subscenePath, 0f);
                SceneAsset subscene =
                    AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(subscenePath), typeof(SceneAsset)) as
                        SceneAsset;

                var sceneObject = SceneManager.GetSceneByName(subscene != null ? subscene.name : null);
                if (!sceneObject.IsValid())
                    continue;

                foreach (GameObject gameObject in sceneObject.GetRootGameObjects())
                {
                    CheckObject(gameObject);
                }
            }

            EditorUtility.ClearProgressBar();

            var list = new List<string>(usedAssets.Keys);
            foreach (var usedAssetPath in list)
            {
                if (!usedAssets[usedAssetPath])
                {
                    unusedAssets.Add(usedAssetPath);
                }
            }
        }

        void CheckObject(GameObject gameObject)
        {
            CheckTransform(gameObject.transform);
        }

        void CheckTransform(Transform t)
        {
            var components = UnityEngine.Pool.ListPool<Component>.Get();
            t.GetComponents(components);
            foreach (var component in components)
            {
                SerializedObject so = new SerializedObject(component);
                var sp = so.GetIterator();
                while (sp.NextVisible(true))
                {
                    if (sp.propertyType != SerializedPropertyType.ObjectReference)
                        continue;

                    string assetPath = AssetDatabase.GetAssetPath(sp.objectReferenceValue);
                    if (!string.IsNullOrEmpty(assetPath) && usedAssets.ContainsKey(assetPath))
                    {
                        usedAssets[assetPath] = true;
                    }
                }
            }

            UnityEngine.Pool.ListPool<Component>.Release(components);

            foreach (Transform child in t)
            {
                CheckTransform(child);
            }
        }
#endregion // Unity.Megacity.EditorTools
    }
}