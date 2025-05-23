using System.Diagnostics;
using UnityEngine;

namespace UnityExtensions
{
    /// <summary>
    /// Utility class for managing the cursor.
    /// </summary>
    public static class CursorUtils
    {
        //https://github.com/Unity-Technologies/megacity-metro/blob/master/Assets/Scripts/Utils/UI/CursorUtils.cs
        #region Unity.MegacityMetro.UI
        [Conditional("UNITY_STANDALONE")]
        public static void ShowCursor()
        {
            // Ignore this script if we're on mobile
#if !(UNITY_ANDROID || UNITY_IPHONE)
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
#endif
        }

        [Conditional("UNITY_STANDALONE")]
        public static void HideCursor()
        {
            // Ignore this script if we're on mobile
#if !(UNITY_ANDROID || UNITY_IPHONE)
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
#endif
        }
        #endregion // Unity.MegacityMetro.UI
    }
}