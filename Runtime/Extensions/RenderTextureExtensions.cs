using UnityEngine;

namespace UnityExtensions
{
    public static class RenderTextureExtensions
    {
        //https://github.com/needle-mirror/com.unity.film-internal-utilities/blob/2cfc425a6f0bf909732b9ca80f2385ea3ff92850/Runtime/Scripts/Extensions/RenderTextureExtensions.cs
        #region Unity.FilmInternalUtilities
        /// <summary>
        /// Clear the depth and the color of a RenderTexture using RGBA(0,0,0,0)
        /// </summary>
        /// <param name="rt">the target RenderTexture</param>
        public static void ClearAll(this RenderTexture rt)
        {
            rt.Clear(clearDepth: true, clearColor: true, Color.clear);
        }

        /// <summary>
        /// Clear a RenderTexture
        /// </summary>
        /// <param name="rt">the target RenderTexture</param>
        /// <param name="clearDepth">Should the depth buffer be cleared? </param>
        /// <param name="clearColor">Should the color buffer be cleared? </param>
        /// <param name="bgColor">The color to clear with, used only if clearColor is true. </param>
        public static void Clear(this RenderTexture rt, bool clearDepth, bool clearColor, Color bgColor)
        {
            RenderTexture prevRT = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(clearDepth, clearColor, bgColor);
            RenderTexture.active = prevRT;
        }
        #endregion // Unity.FilmInternalUtilities
    }
}
