using UnityEngine;

namespace UnityExtensions
{
    /// <summary>
    /// Utilities for manipulating Textures.
    /// </summary>
    public static class TextureUtils
    {
        //https://github.com/needle-mirror/com.unity.xr.core-utils/blob/2.5.1/Runtime/TextureUtils.cs
        #region Unity.XR.CoreUtils
        /// <summary>
        /// Copy a given <see cref="RenderTexture"/> to a <see cref="Texture2D"/>.
        /// This method assumes that both textures exist and are the same size.
        /// </summary>
        /// <param name="renderTexture">The source <see cref="RenderTexture" />.</param>
        /// <param name="texture">The destination <see cref="Texture2D" />.</param>
        public static void RenderTextureToTexture2D(RenderTexture renderTexture, Texture2D texture)
        {
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            texture.Apply();
        }
        #endregion // Unity.XR.CoreUtils
    }
}