using System;
using System.Collections.Generic;
using UnityExtensions.Editor;
using UnityEditor;
using UnityEngine;
using UnityExtensions.Attributes;

namespace UnityExtensions.GUI.Editor
{
    [CustomPropertyDrawer(typeof(FlagsPropertyAttribute))]
    internal sealed class FlagsPropertyDrawer : PropertyDrawer
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Editor/GUI/FlagsPropertyDrawer.cs
        #region Unity.XR.CoreUtils.Editor
        // Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        static readonly List<string> ValidEnumNames = new List<string>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var type = EditorUtils.SerializedPropertyToType(property);
            ValidEnumNames.Clear();
            var names = property.enumNames;
            var length = names.Length;
            var values = (int[])Enum.GetValues(type);
            for (var i = 0; i < length; i++)
            {
                if (MathUtility.IsPositivePowerOfTwo(values[i]))
                    ValidEnumNames.Add(names[i]);
            }

            property.intValue = EditorUtils.MaskField(position, label, property.intValue, ValidEnumNames.ToArray(), type);
        }
        #endregion // Unity.XR.CoreUtils.Editor
    }
}
