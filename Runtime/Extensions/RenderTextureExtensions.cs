using System.IO;
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
            RenderTexture prevRT = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(clearDepth: true, clearColor: true, Color.clear);
            RenderTexture.active = prevRT;
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

        /// <summary>
        /// Write a RenderTexture to a file.
        /// May forward exception
        /// </summary>
        /// <param name="rt">the target RenderTexture</param>
        /// <param name="outputFilePath">The path of the output file </param>
        /// <param name="textureFormat">The texture format of the output texture </param>
        /// <param name="isPNG">The file format of the output file. </param>
        /// <param name="isLinear">The color space of the output texture: linear or sRGB. </param>
        public static bool WriteToFile(this RenderTexture rt, string outputFilePath, TextureFormat textureFormat,
            bool isPNG = true, bool isLinear = false)
        {
            RenderTexture prevRenderTexture = RenderTexture.active;
            RenderTexture.active = rt;
            bool ret = false;

            Texture2D tempTex = new Texture2D(rt.width, rt.height, textureFormat , mipChain: false, isLinear);
            tempTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, recalculateMipMaps: false);
            tempTex.Apply();

            try
            {
                byte[] encodedData = isPNG ? tempTex.EncodeToPNG() : tempTex.EncodeToEXR();

                if (null != encodedData)
                {
                    File.WriteAllBytesAsync(outputFilePath, encodedData);
                    ret = true;
                }
            }
            finally
            {
                CoreUtils.Destroy(tempTex);
                RenderTexture.active = prevRenderTexture;
            }

            return ret;
        }
        #endregion // Unity.FilmInternalUtilities
    }
}
