using System;

namespace UnityExtensions.Attributes
{
    /// <summary>
    /// Attribute used to customize UI display.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
    public class DisplayInfoAttribute : Attribute
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Common/CoreAttributes.cs
        #region UnityEngine.Rendering
        /// <summary>Display name used in UI.</summary>
        public string Name;
        /// <summary>Display order used in UI.</summary>
        public int Order;
        #endregion // UnityEngine.Rendering
    }

    /// <summary>
    /// Attribute used to customize UI display to allow properties only be visible when "Show Additional Properties" is selected
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class AdditionalPropertyAttribute : Attribute
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Common/CoreAttributes.cs
        #region UnityEngine.Rendering
        #endregion // UnityEngine.Rendering
    }

    /// <summary>
    /// Attribute used to hide enum values from Rendering Debugger UI
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class HideInDebugUIAttribute : Attribute
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Common/CoreAttributes.cs
        #region UnityEngine.Rendering
        #endregion // UnityEngine.Rendering
    }
}
