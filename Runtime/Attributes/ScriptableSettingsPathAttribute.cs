using System;

namespace UnityExtensions
{
    /// <summary>
    /// Allows a class inheriting from <see cref="ScriptableSettings{T}"/> to specify that its instance Asset
    /// should be saved under "Assets/[<see cref="Path"/>]/Resources/ScriptableSettings/".
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ScriptableSettingsPathAttribute : Attribute
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/Attributes/ScriptableSettingsPathAttribute.cs
        #region Unity.XR.CoreUtils.GUI
        readonly string m_Path;

        /// <summary>
        /// The path where this ScriptableSettings should be stored.
        /// </summary>
        public string Path => m_Path;

        /// <summary>
        /// Initialize a new ScriptableSettingsPathAttribute.
        /// </summary>
        /// <param name="path">The path where the ScriptableSettings should be stored.</param>
        public ScriptableSettingsPathAttribute(string path = "")
        {
            m_Path = path;
        }
        #endregion // Unity.XR.CoreUtils.GUI
    }
}
