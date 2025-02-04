using System;
using UnityEditor;

namespace UnityExtensions.Editor
{
    /// <summary>
    /// Bool saved in EditorPref.
    /// </summary>
    public struct EditorPrefBool
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Editor/EditorPrefBool.cs
        #region UnityEditor.Rendering
        readonly string m_Key;

        /// <summary>
        /// Value of the boolean in editor preferences.
        /// </summary>
        public bool value
        {
            get => EditorPrefs.GetBool(m_Key);
            set => EditorPrefs.SetBool(m_Key, value);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">Key in the editor preferences.</param>
        /// <param name="defaultValue">Default value of the preference.</param>
        public EditorPrefBool(string key, bool defaultValue = false)
        {
            m_Key = key;

            //register key if not already there
            if (!EditorPrefs.HasKey(m_Key))
            {
                EditorPrefs.SetBool(m_Key, defaultValue);
            }
        }
        #endregion // UnityEditor.Rendering
    }
}
