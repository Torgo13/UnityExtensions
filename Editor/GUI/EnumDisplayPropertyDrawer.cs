using UnityEditor;
using UnityEngine;
using UnityExtensions.Attributes;

namespace UnityExtensions.GUI.Editor
{
    [CustomPropertyDrawer(typeof(EnumDisplayAttribute))]
    internal sealed class EnumDisplayPropertyDrawer : PropertyDrawer
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Editor/GUI/EnumDisplayPropertyDrawer.cs
        #region Unity.XR.CoreUtils.Editor
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var enumDisplayAttribute = (EnumDisplayAttribute)attribute;
            var currentEnumValue = property.intValue;
            property.intValue = EditorGUI.IntPopup(position, label.text, currentEnumValue, enumDisplayAttribute.Names, enumDisplayAttribute.Values);
        }
        #endregion // Unity.XR.CoreUtils.Editor
    }
}
