using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityExtensions
{
    public static class Screenshot
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/Core/Utilities/Screenshot.cs
        #region Unity.LiveCapture
        public static Texture2D Take(Camera camera, float scale = 1f, bool hdr = false)
        {
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

            scale = Math.Clamp(scale, 0f, 1f);

            var width = Math.Max(1, (int)Math.Round(camera.pixelWidth * scale));
            var height = Math.Max(1, (int)Math.Round(camera.pixelHeight * scale));
            var prevCameraRenderTexture = camera.targetTexture;
            var renderTexture = RenderTexture.GetTemporary(width, height, 0,
                hdr ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.ARGB32);

            camera.targetTexture = renderTexture;
            camera.Render();

            var prevRenderTexture = RenderTexture.active;
            RenderTexture.active = camera.targetTexture;

            var texture = new Texture2D(width, height, 
                hdr ? TextureFormat.RGBAFloat : TextureFormat.RGB24, mipChain: false);

            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0, recalculateMipMaps: false);
            texture.Apply(false);

            camera.targetTexture = prevCameraRenderTexture;
            RenderTexture.active = prevRenderTexture;
            RenderTexture.ReleaseTemporary(renderTexture);

            return texture;
        }

        public static string SaveAsPNG(Texture2D texture, string filename, string directory)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            var formatter = FileNameFormatter.Instance;
            filename = formatter.Format(filename);
            var assetPath = $"{directory}/{filename}.png";
            Directory.CreateDirectory(directory);
#if UNITY_EDITOR
            assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(assetPath);
#endif
            var bytes = texture.EncodeToPNG();
            File.WriteAllBytes(assetPath, bytes);

            return assetPath;
        }
        #endregion // Unity.LiveCapture

        public static async Task<string> SaveAsPNGAsync(Texture2D texture, string filename, string directory)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            var formatter = FileNameFormatter.Instance;
            filename = formatter.Format(filename);
            var assetPath = $"{directory}/{filename}.png";
            Directory.CreateDirectory(directory);
#if UNITY_EDITOR
            assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(assetPath);
#endif
            var bytes = texture.EncodeToPNG();
            await File.WriteAllBytesAsync(assetPath, bytes);

            return assetPath;
        }

        #region EXR
        public static string SaveAsEXR(Texture2D texture, string filename, string directory)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            var formatter = FileNameFormatter.Instance;
            filename = formatter.Format(filename);
            var assetPath = $"{directory}/{filename}.exr";
            Directory.CreateDirectory(directory);
#if UNITY_EDITOR
            assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(assetPath);
#endif
            var bytes = texture.EncodeToEXR(texture.format.IsHDR());
            File.WriteAllBytes(assetPath, bytes);

            return assetPath;
        }

        public static async Task<string> SaveAsEXRAsync(Texture2D texture, string filename, string directory)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            var formatter = FileNameFormatter.Instance;
            filename = formatter.Format(filename);
            var assetPath = $"{directory}/{filename}.exr";
            Directory.CreateDirectory(directory);
#if UNITY_EDITOR
            assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(assetPath);
#endif
            var bytes = texture.EncodeToEXR(texture.format.IsHDR());
            await File.WriteAllBytesAsync(assetPath, bytes);

            return assetPath;
        }
        #endregion // EXR

        static Texture2D.EXRFlags IsHDR(this TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.RHalf:
                case TextureFormat.RGHalf:
                case TextureFormat.RGBAHalf:
                case TextureFormat.RFloat:
                case TextureFormat.RGFloat:
                case TextureFormat.RGBAFloat:
                case TextureFormat.RGB9e5Float:
                case TextureFormat.BC6H:
                case TextureFormat.ASTC_HDR_4x4:
                case TextureFormat.ASTC_HDR_5x5:
                case TextureFormat.ASTC_HDR_6x6:
                case TextureFormat.ASTC_HDR_8x8:
                case TextureFormat.ASTC_HDR_10x10:
                case TextureFormat.ASTC_HDR_12x12:
                    return Texture2D.EXRFlags.OutputAsFloat;
                default:
                    return Texture2D.EXRFlags.None;
            }
        }
    }
}
