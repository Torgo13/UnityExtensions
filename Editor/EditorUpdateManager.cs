using System;
using UnityEditor;

namespace PKGE.Editor
{
    /// <summary>
    /// Call update at regular interval, even when using executeMethod on the command line
    /// </summary>
    public static class EditorUpdateManager
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.shaderanalysis/Editor/Internal/EditorUpdateManager.cs
        #region UnityEditor.ShaderAnalysis.Internal
        public static Action? ToUpdate;

        [InitializeOnLoadMethod]
        static void Register()
        {
            EditorApplication.update += Tick;
        }

        /// <summary>
        /// Call this to tick
        /// </summary>
        public static void Tick() => ToUpdate?.Invoke();
        #endregion // UnityEditor.ShaderAnalysis.Internal
    }
}
