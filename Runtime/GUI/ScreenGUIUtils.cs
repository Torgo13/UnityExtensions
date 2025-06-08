using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace UnityExtensions
{
    /// <summary>
    /// Helpers for handling screen DPI in GUI.
    /// </summary>
    public static class ScreenGUIUtils
    {
        //https://github.com/Unity-Technologies/Graphics/blob/6feca3f03e6a8cb62665b394599d2d17d5848c65/Packages/com.unity.render-pipelines.core/Editor/CoreEditorUtils.cs
        #region UnityEditor.Rendering
        static readonly Func<float> GetGUIStatePixelsPerPoint = Expression.Lambda<Func<float>>(
            Expression.Property(null, typeof(GUIUtility).GetProperty("pixelsPerPoint",
                BindingFlags.NonPublic | BindingFlags.Static))).Compile();
        #endregion // UnityEditor.Rendering

        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/GUI/ScreenGUIUtils.cs
        #region Unity.XR.CoreUtils.GUI
        /// <summary>
        /// Gets the width of the screen, in points (pixels at 100% DPI).
        /// </summary>
        public static float PointWidth => Screen.width / GetGUIStatePixelsPerPoint();

        /// <summary>
        /// Gets the height of the screen, in points (pixels at 100% DPI).
        /// </summary>
        public static float PointHeight => Screen.height / GetGUIStatePixelsPerPoint();
        #endregion // Unity.XR.CoreUtils.GUI
    }
}
