using UnityEngine;

namespace PKGE.Editor
{
    public static class UIUtils
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.shaderanalysis/Editor/Internal/UIUtils.cs
        #region UnityEditor.ShaderAnalysis.Internal
        const int k_MaxLogSize = ushort.MaxValue / 4 - 5;
        static GUIContent s_TextContent = new GUIContent();

        public static GUIContent Text(string text)
        {
            s_TextContent.text = text;
            return s_TextContent;
        }

        public static GUIContent Text(string format, params object[] args)
        {
            s_TextContent.text = string.Format(format, args);
            return s_TextContent;
        }

        public static string ClampText(string text)
        {
            return text.Length > k_MaxLogSize
                ? text.Substring(0, k_MaxLogSize)
                : text;
        }
        #endregion // UnityEditor.ShaderAnalysis.Internal
    }
}
