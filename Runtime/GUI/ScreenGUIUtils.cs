#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace UnityExtensions.GUI
{
    /// <summary>
    /// Helpers for handling screen DPI in GUI.
    /// </summary>
    public static class ScreenGUIUtils
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/GUI/ScreenGUIUtils.cs
        #region Unity.XR.CoreUtils.GUI
        /// <summary>
        /// Gets the width of the screen, in points (pixels at 100% DPI).
        /// </summary>
        public static float PointWidth => Screen.width / EditorGUIUtility.pixelsPerPoint;

        /// <summary>
        /// Gets the height of the screen, in points (pixels at 100% DPI).
        /// </summary>
        public static float PointHeight => Screen.height / EditorGUIUtility.pixelsPerPoint;
        #endregion // Unity.XR.CoreUtils.GUI
    }
}
#endif
