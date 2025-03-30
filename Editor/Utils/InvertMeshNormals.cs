using UnityEditor;
using UnityEngine;

namespace UnityExtensions.Editor
{
    public class InvertMeshNormals : EditorWindow
    {
        //https://github.com/Unity-Technologies/Megacity-2019/blob/1d90090d6d23417c661e7937e283b77b8e1db29d/Assets/Scripts/Utils/Editor/InvertMeshNormals.cs
        #region Unity.Megacity.EditorTools
        [MenuItem("Tools/Invert Mesh Normals")]
        public static void ShowWindow()
        {
            GetWindow(typeof(InvertMeshNormals));
        }
        
        private Mesh mesh;

        private void OnGUI()
        {
            GUILayout.Label("Invert Mesh Normals", EditorStyles.boldLabel);

            mesh = (Mesh)EditorGUILayout.ObjectField("Mesh", mesh, typeof(Mesh), false);

            if (GUILayout.Button("Invert Normals") && mesh != null)
            {
                var invertedMesh = Instantiate(mesh);
                var normals = invertedMesh.normals;
                for (int i = 0; i < normals.Length; i++)
                {
                    normals[i] = -normals[i];
                }
                invertedMesh.normals = normals;

                AssetDatabase.CreateAsset(invertedMesh, "Assets/NewInvertedMesh.mesh");
                AssetDatabase.SaveAssets();
            }
        }
        #endregion // Unity.Megacity.EditorTools
    }
}
