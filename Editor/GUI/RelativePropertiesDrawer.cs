using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PKGE.GUI.Editor
{
    /// <summary>
    /// Custom property drawer that renders automatically a set of properties of a given object
    /// </summary>
    public abstract class RelativePropertiesDrawer : PropertyDrawer
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Editor/RelativePropertiesDrawer.cs
        #region UnityEditor.Rendering
        /// <summary>
        /// The set of properties to draw the default <see cref="PropertyDrawer"/>.
        /// </summary>
        protected abstract string[] relativePropertiesNames { get; }

        /// <inheritdoc/>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            for (int i = 0; i < relativePropertiesNames.Length; ++i)
            {
                var relativeProperty = property.FindPropertyRelative(relativePropertiesNames[i]);
                if (relativeProperty == null)
                    continue;
                container.Add(new PropertyField(relativeProperty));
            }
            return container;
        }

        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            Rect? rect = null;
            for (int i = 0; i < relativePropertiesNames.Length; ++i)
            {
                var relativeProperty = property.FindPropertyRelative(relativePropertiesNames[i]);
                if (relativeProperty == null)
                    continue;

                var height = EditorGUI.GetPropertyHeight(relativeProperty, true) +
                             EditorGUIUtility.standardVerticalSpacing;
                rect = rect != null ?
                    new Rect(rect.Value.x, rect.Value.y + rect.Value.height + EditorGUIUtility.standardVerticalSpacing, rect.Value.width, height) :
                    new Rect(position.x, position.y + EditorGUIUtility.standardVerticalSpacing, position.width, height);
                EditorGUI.PropertyField(rect.Value, relativeProperty);
            }
            EditorGUI.EndProperty();
        }

        /// <inheritdoc/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.standardVerticalSpacing;
            for (int i = 0; i < relativePropertiesNames.Length; ++i)
            {
                var relativeProperty = property.FindPropertyRelative(relativePropertiesNames[i]);
                if (relativeProperty == null)
                    continue;

                height += EditorGUI.GetPropertyHeight(relativeProperty, true) + EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }
        #endregion // UnityEditor.Rendering
    }
}
