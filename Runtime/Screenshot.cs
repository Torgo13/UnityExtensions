using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace PKGE
{
    public static class Screenshot
    {
        //https://github.com/Unity-Technologies/UnityLiveCapture/blob/4.0.1/Packages/com.unity.live-capture/Runtime/Core/Utilities/Screenshot.cs
        #region Unity.LiveCapture
        [JetBrains.Annotations.NotNull]
        public static Texture2D Take([System.Diagnostics.CodeAnalysis.NotNull] this Camera camera, float scale = 1f, bool hdr = false)
        {
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

            scale = Math.Clamp(scale, 0f, 1f);

            var width = Math.Max(1, (int)Math.Round(camera.pixelWidth * scale));
            var height = Math.Max(1, (int)Math.Round(camera.pixelHeight * scale));
            var prevCameraRenderTexture = camera.targetTexture;
            var renderTexture = RenderTexture.GetTemporary(width, height, depthBuffer: 16,
                hdr ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.ARGB32);

            camera.targetTexture = renderTexture;
            camera.Render();

            var prevRenderTexture = RenderTexture.active;
            RenderTexture.active = camera.targetTexture;

            var texture = new Texture2D(width, height, 
                hdr ? TextureFormat.RGBAFloat : TextureFormat.RGB24, mipChain: false,
                linear: hdr, createUninitialized: true);

            texture.ReadPixels(new Rect(0, 0, width, height), destX: 0, destY: 0, recalculateMipMaps: false);
            texture.Apply();

            camera.targetTexture = prevCameraRenderTexture;
            RenderTexture.active = prevRenderTexture;
            RenderTexture.ReleaseTemporary(renderTexture);

            return texture;
        }

        [JetBrains.Annotations.NotNull]
        public static string SaveAsPNG([System.Diagnostics.CodeAnalysis.NotNull] this Texture2D texture, string filename, [System.Diagnostics.CodeAnalysis.NotNull] string directory)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            filename = FileNameFormatter.Format(filename);
            var assetPath = $"{directory}/{filename}.png";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
#if UNITY_EDITOR
            assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(assetPath);
#endif
            var bytes = texture.EncodeToPNG();
            File.WriteAllBytes(assetPath, bytes);

            return assetPath;
        }
        #endregion // Unity.LiveCapture

        public static async ValueTask<string> SaveAsPNGAsync([System.Diagnostics.CodeAnalysis.NotNull] this Texture2D texture, string filename, [System.Diagnostics.CodeAnalysis.NotNull] string directory)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            filename = FileNameFormatter.Format(filename);
            var assetPath = $"{directory}/{filename}.png";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
#if UNITY_EDITOR
            assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(assetPath);
#endif
            var bytes = texture.EncodeToPNG();
            await File.WriteAllBytesAsync(assetPath, bytes).ConfigureAwait(continueOnCapturedContext: true);

            return assetPath;
        }

        #region EXR
        [JetBrains.Annotations.NotNull]
        public static string SaveAsEXR([System.Diagnostics.CodeAnalysis.NotNull] this Texture2D texture, string filename, [System.Diagnostics.CodeAnalysis.NotNull] string directory,
            Texture2D.EXRFlags flags = Texture2D.EXRFlags.None)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            filename = FileNameFormatter.Format(filename);
            var assetPath = $"{directory}/{filename}.exr";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
#if UNITY_EDITOR
            assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(assetPath);
#endif
            var bytes = texture.EncodeToEXR(texture.format.IsHDR() | flags);
            File.WriteAllBytes(assetPath, bytes);

            return assetPath;
        }

        public static async ValueTask<string> SaveAsEXRAsync([System.Diagnostics.CodeAnalysis.NotNull] this Texture2D texture, string filename, [System.Diagnostics.CodeAnalysis.NotNull] string directory,
            Texture2D.EXRFlags flags = Texture2D.EXRFlags.None)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            filename = FileNameFormatter.Format(filename);
            var assetPath = $"{directory}/{filename}.exr";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
#if UNITY_EDITOR
            assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(assetPath);
#endif
            var bytes = texture.EncodeToEXR(texture.format.IsHDR() | flags);
            await File.WriteAllBytesAsync(assetPath, bytes).ConfigureAwait(continueOnCapturedContext: true);

            return assetPath;
        }
        #endregion // EXR

        [JetBrains.Annotations.NotNull]
        public static string Save([System.Diagnostics.CodeAnalysis.NotNull] this Texture2D texture, string filename, [System.Diagnostics.CodeAnalysis.NotNull] string directory,
            TextureFileType fileType = TextureFileType.Auto, Texture2D.EXRFlags flags = Texture2D.EXRFlags.None)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            _ = texture.IsHDR(ref fileType, ref flags);

            var assetPath = CreatePath(filename, directory, fileType.GetTextureExtension());
            var bytes = texture.Encode(fileType, flags);
            File.WriteAllBytes(assetPath, bytes);

            return assetPath;
        }

        public static async ValueTask<string> SaveAsync([System.Diagnostics.CodeAnalysis.NotNull] this Texture2D texture, string filename, [System.Diagnostics.CodeAnalysis.NotNull] string directory,
            TextureFileType fileType = TextureFileType.Auto, Texture2D.EXRFlags flags = Texture2D.EXRFlags.None)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            _ = texture.IsHDR(ref fileType, ref flags);

            var assetPath = CreatePath(filename, directory, fileType.GetTextureExtension());
            var bytes = texture.Encode(fileType, flags);
            await File.WriteAllBytesAsync(assetPath, bytes).ConfigureAwait(continueOnCapturedContext: true);

            return assetPath;
        }

        static string CreatePath(string filename, [System.Diagnostics.CodeAnalysis.NotNull] string directory, string textureExtension)
        {
            filename = FileNameFormatter.Format(filename);
            var assetPath = $"{directory}/{filename}.{textureExtension}";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
#if UNITY_EDITOR
            assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(assetPath);
#endif
            return assetPath;
        }

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

        #region TextureFileType
        /// <remarks>
        /// Use EXR 32-bit float for HDR textures
        /// </remarks>
        /// <param name="texture">Input <see cref="Texture"/>.</param>
        /// <param name="fileType">Supported <see cref="Texture"/> file types.</param>
        /// <param name="flags">Additional flags for when <paramref name="fileType"/> is <see cref="TextureFileType.EXR"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="fileType"/> is HDR.</returns>
        static bool IsHDR(this Texture2D texture, ref TextureFileType fileType, ref Texture2D.EXRFlags flags)
        {
            if ((fileType == TextureFileType.Auto || fileType == TextureFileType.EXR)
                && texture.format.IsHDR() == Texture2D.EXRFlags.OutputAsFloat)
            {
                fileType = TextureFileType.EXR;
                flags |= Texture2D.EXRFlags.OutputAsFloat;
                return true;
            }

            return false;
        }

        public enum TextureFileType
        {
            Auto,
            PNG,
            JPG,
            EXR,
            TGA,
        }

        [JetBrains.Annotations.NotNull]
        public static string GetTextureExtension(this TextureFileType textureFile)
        {
            switch (textureFile)
            {
                default:
                case TextureFileType.PNG:
                    return "png";
                case TextureFileType.JPG:
                    return "jpg";
                case TextureFileType.EXR:
                    return "exr";
                case TextureFileType.TGA:
                    return "tga";
            }
        }

        static byte[] Encode(this Texture2D texture,
            TextureFileType fileType = TextureFileType.Auto,
            Texture2D.EXRFlags flags = Texture2D.EXRFlags.None,
            int quality = 75)
        {
            switch (fileType)
            {
                default:
                case TextureFileType.PNG:
                    return texture.EncodeToPNG();
                case TextureFileType.JPG:
                    return texture.EncodeToJPG(quality);
                case TextureFileType.EXR:
                    return texture.EncodeToEXR(flags);
                case TextureFileType.TGA:
                    return texture.EncodeToTGA();
            }
        }
        #endregion // TextureFileType
    }
}
