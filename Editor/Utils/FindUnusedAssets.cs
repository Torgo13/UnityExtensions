using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace UnityExtensions.Editor
{
    public class FindUnusedAssets : EditorWindow
    {
        //https://github.com/Unity-Technologies/Megacity-2019/blob/1d90090d6d23417c661e7937e283b77b8e1db29d/Assets/Scripts/Utils/Editor/UnusedAssetsChecker.cs
        #region Unity.Megacity.EditorTools
        private static readonly string[] assetTypes = { "Prefab", "AudioClip", "Texture", "Model", "Material" };
        private Dictionary<string, bool> usedAssets = new Dictionary<string, bool>();
        private List<string> unusedAssets = new List<string>();
        private List<string> excludedPaths = new List<string> { "Packages" };

        [MenuItem("Tools/Check Unused Assets")]
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

            string[] guids = AssetDatabase.FindAssets("t:Object", null);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (path == null)
                    continue;

                var type = AssetDatabase.GetMainAssetTypeAtPath(path);

                if (type == null)
                    continue;

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

                var sceneObject = SceneManager.GetSceneByName(subscene?.name);
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
            foreach (var component in gameObject.GetComponents<Component>())
            {
                SerializedObject so = new SerializedObject(component);
                var sp = so.GetIterator();
                while (sp.NextVisible(true))
                {
                    if (sp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (sp.objectReferenceValue != null)
                        {
                            string assetPath = AssetDatabase.GetAssetPath(sp.objectReferenceValue);
                            if (usedAssets.ContainsKey(assetPath))
                            {
                                usedAssets[assetPath] = true;
                            }
                        }
                    }
                }
            }

            foreach (Transform child in gameObject.transform)
            {
                CheckObject(child.gameObject);
            }
        }
        #endregion // Unity.Megacity.EditorTools
    }
}