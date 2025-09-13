using UnityEngine;
using UnityEditor;

namespace PKGE.GUI.Editor
{
    [CustomPropertyDrawer(typeof(Quaternion))]
    internal class QuaternionPropertyDrawer : PropertyDrawer
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Editor/QuaternionPropertyDrawer.cs
        #region UnityEditor.Rendering
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var euler = property.quaternionValue.eulerAngles;
            EditorGUI.BeginChangeCheck();
            var w = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = true;
            euler = EditorGUI.Vector3Field(position, label, euler);
            EditorGUIUtility.wideMode = w;
            if (EditorGUI.EndChangeCheck())
                property.quaternionValue = Quaternion.Euler(euler);
        }
        #endregion // UnityEditor.Rendering
    }
}
