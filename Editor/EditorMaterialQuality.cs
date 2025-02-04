using UnityEngine.Rendering;

namespace UnityExtensions.Editor
{
    /// <summary>
    /// Editor MaterialQuality utility class.
    /// </summary>
    public static class EditorMaterialQuality
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Editor/Utilities/EditorMaterialQuality.cs
        #region UnityEditor.Rendering.Utilities
        /// <summary>
        /// Get the material quality levels enabled in a keyword set.
        /// </summary>
        /// <param name="keywordSet">Input keywords.</param>
        /// <returns>All available MaterialQuality levels in the keyword set.</returns>
        public static MaterialQuality GetMaterialQuality(this ShaderKeywordSet keywordSet)
        {
            var result = (MaterialQuality)0;
            for (var i = 0; i < MaterialQualityUtilities.Keywords.Length; ++i)
            {
                if (keywordSet.IsEnabled(MaterialQualityUtilities.Keywords[i]))
                    result |= (MaterialQuality)(1 << i);
            }

            return result;
        }
        #endregion // UnityEditor.Rendering.Utilities
    }
}
