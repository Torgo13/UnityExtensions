using UnityEditor;
using UnityEngine;
using UnityExtensions.Attributes;

namespace UnityExtensions.GUI.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    internal class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Editor/GUI/ReadOnlyPropertyDrawer.cs
        #region Unity.XR.CoreUtils.Editor
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
        #endregion // Unity.XR.CoreUtils.Editor
    }
}
