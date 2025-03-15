using System;
using System.Linq;
using UnityEngine.UIElements;

namespace UnityExtensions.Editor
{
    public static class UIToolkitUtility
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Editor/Core/Utilities/UIToolkitUtility.cs
        #region Unity.LiveCapture.Editor
        public static void SetDisplay(this VisualElement element, bool visible)
        {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static void MoveChildrenTo(this VisualElement from, VisualElement to)
        {
            while (from.childCount > 0)
            {
                var child = from.Children().First();
                child.RemoveFromHierarchy();
                to.Add(child);
            }
        }

        /// <summary>
        /// Convenience extension to get a callback after initial geometry creation, making it easier to use lambdas.
        /// Callback will only be called once. Works in inspectors and PropertyDrawers.
        /// </summary>
        public static void RegisterGeometryChangedEventCallbackOnce(this VisualElement owner, Action callback)
        {
            owner.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            return;
            
            void OnGeometryChanged(GeometryChangedEvent _)
            {
                owner.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged); // call only once
                callback();
            }
        }
        #endregion // Unity.LiveCapture.Editor
    }
}
