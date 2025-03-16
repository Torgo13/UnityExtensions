using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace UnityExtensions.Editor
{
    public class TextureCombiner
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Editor/Core/TextureCombiner/TextureCombiner.cs
        #region UnityEditor.Rendering
        static Texture2D _midGrey;

        /// <summary>
        /// Returns a 1 by 1 mid-grey (0.5, 0.5, 0.5, 1) Texture.
        /// </summary>
        public static Texture2D midGrey
        {
            get
            {
                if (_midGrey == null)
                    _midGrey = TextureFromColor(Color.grey);

                return _midGrey;
            }
        }

        private static Dictionary<Color, Texture2D> _singleColorTextures;

        /// <summary>
        /// Returns a 1 by 1 Texture that is the color that you pass in.
        /// </summary>
        /// <param name="color">The color that Unity uses to create the Texture.</param>
        /// <returns></returns>
        public static Texture2D TextureFromColor(Color color)
        {
            if (color == Color.white) return Texture2D.whiteTexture;
            if (color == Color.black) return Texture2D.blackTexture;

            if (_singleColorTextures == null)
                _singleColorTextures = new Dictionary<Color, Texture2D>();

            bool makeTexture = !_singleColorTextures.ContainsKey(color);
            if (!makeTexture)
                makeTexture = _singleColorTextures[color] == null;

            if (makeTexture)
            {
                Texture2D tex = new Texture2D(1, 1, GraphicsFormat.R8G8B8A8_UNorm, TextureCreationFlags.None);
                tex.SetPixel(0, 0, color);
                tex.Apply();

                _singleColorTextures[color] = tex;
            }

            return _singleColorTextures[color];
        }

        /// <summary>
        /// Returns the Texture assigned to the property "propertyName" of "srcMaterial".
        /// If no matching property is found, or no Texture is assigned, returns a 1 by 1 Texture of "fallback" color.
        /// </summary>
        /// <param name="srcMaterial">The Material to get the Texture from.</param>
        /// <param name="propertyName">The name of the Texture property.</param>
        /// <param name="fallback">The fallback color that Unity uses to create a Texture if it could not find the Texture property on the Material.</param>
        /// <returns></returns>
        public static Texture GetTextureSafe(Material srcMaterial, string propertyName, Color fallback)
        {
            return GetTextureSafe(srcMaterial, propertyName, TextureFromColor(fallback));
        }

        /// <summary>
        /// Returns the Texture assigned to the property "propertyName" of "srcMaterial".
        /// If no matching property is found, or no Texture is assigned, returns the "fallback" Texture.
        /// </summary>
        /// <param name="srcMaterial">The Material to get the Texture from.</param>
        /// <param name="propertyName">The name of the Texture property.</param>
        /// <param name="fallback">The fallback color that Unity uses to create a Texture if it could not find the Texture property on the Material.</param>
        /// <returns></returns>
        public static Texture GetTextureSafe(Material srcMaterial, string propertyName, Texture fallback)
        {
            if (!srcMaterial.HasProperty(propertyName))
                return fallback;

            Texture tex = srcMaterial.GetTexture(propertyName);
            if (tex == null)
                return fallback;
            
            return tex;
        }

        /// <summary>
        /// Specifies whether the Texture has an alpha channel or not. Returns true if it does and false otherwise.
        /// </summary>
        /// <param name="tex">The Texture for this function to check.</param>
        /// <returns></returns>
        public static bool TextureHasAlpha(Texture2D tex)
        {
            if (tex == null) return false;

            return GraphicsFormatUtility.HasAlphaChannel(tex.graphicsFormat);
        }

        private readonly Texture _rSource;
        private readonly Texture _gSource;
        private readonly Texture _bSource;
        private readonly Texture _aSource;

        // Channels are : r=0, g=1, b=2, a=3, greyscale from rgb = 4
        // If negative, the chanel is inverted
        private readonly int _rChannel;
        private readonly int _gChannel;
        private readonly int _bChannel;
        private readonly int _aChannel;

        // Channels remapping
        private readonly Vector4[] _remappings =
        {
            new Vector4(0f, 1f, 0f, 0f),
            new Vector4(0f, 1f, 0f, 0f),
            new Vector4(0f, 1f, 0f, 0f),
            new Vector4(0f, 1f, 0f, 0f)
        };

        private readonly bool _bilinearFilter;

        /// <summary>
        /// Creates a TextureCombiner object.
        /// </summary>
        /// <param name="rSource">Source Texture for the RED output.</param>
        /// <param name="rChanel">Channel index to use for the RED output.</param>
        /// <param name="gSource">Source Texture for the GREEN output.</param>
        /// <param name="gChanel">Channel index to use for the GREEN output.</param>
        /// <param name="bSource">Source Texture for the BLUE output.</param>
        /// <param name="bChanel">Channel index to use for the BLUE output.</param>
        /// <param name="aSource">Source Texture for the ALPHA output.</param>
        /// <param name="aChanel">Channel index to use for the ALPHA output.</param>
        /// <param name="bilinearFilter">Use bilinear filtering when combining (default = true).</param>
        public TextureCombiner(Texture rSource, int rChanel, Texture gSource, int gChanel, Texture bSource, int bChanel, Texture aSource, int aChanel, bool bilinearFilter = true)
        {
            Assert.IsNotNull(rSource, nameof(rSource));
            Assert.IsNotNull(gSource, nameof(gSource));
            Assert.IsNotNull(bSource, nameof(bSource));
            Assert.IsNotNull(aSource, nameof(aSource));

            _rSource = rSource;
            _gSource = gSource;
            _bSource = bSource;
            _aSource = aSource;
            _rChannel = rChanel;
            _gChannel = gChanel;
            _bChannel = bChanel;
            _aChannel = aChanel;
            _bilinearFilter = bilinearFilter;
        }

        /// <summary>
        /// Set the remapping of a specific color channel.
        /// </summary>
        /// <param name="channel">Target color channel (Red:0, Green:1, Blue:2, Alpha:3).</param>
        /// <param name="min">Minimum input value mapped to 0 in output.</param>
        /// <param name="max">Maximum input value mapped to 1 in output.</param>
        public void SetRemapping(int channel, float min, float max)
        {
            if (channel > 3 || channel < 0) return;

            _remappings[channel].x = min;
            _remappings[channel].y = max;
        }

        /// <summary>
        /// Process the TextureCombiner.
        /// Unity creates the Texture Asset at the "savePath", and returns the Texture object.
        /// </summary>
        /// <param name="savePath">The path to save the Texture Asset to, relative to the Project folder.</param>
        /// <returns></returns>
        public Texture2D Combine(string savePath)
        {
            int xMin = int.MaxValue;
            int yMin = int.MaxValue;

            if (_rSource.width > 4 && _rSource.width < xMin) xMin = _rSource.width;
            if (_gSource.width > 4 && _gSource.width < xMin) xMin = _gSource.width;
            if (_bSource.width > 4 && _bSource.width < xMin) xMin = _bSource.width;
            if (_aSource.width > 4 && _aSource.width < xMin) xMin = _aSource.width;
            if (xMin == int.MaxValue) xMin = 4;

            if (_rSource.height > 4 && _rSource.height < yMin) yMin = _rSource.height;
            if (_gSource.height > 4 && _gSource.height < yMin) yMin = _gSource.height;
            if (_bSource.height > 4 && _bSource.height < yMin) yMin = _bSource.height;
            if (_aSource.height > 4 && _aSource.height < yMin) yMin = _aSource.height;
            if (yMin == int.MaxValue) yMin = 4;

            Texture2D combined = new Texture2D(xMin, yMin, GraphicsFormat.R32G32B32A32_SFloat, TextureCreationFlags.MipChain);
            combined.hideFlags = HideFlags.DontUnloadUnusedAsset;

            Material combinerMaterial = new Material(Shader.Find("Hidden/SRP_Core/TextureCombiner"));
            combinerMaterial.hideFlags = HideFlags.DontUnloadUnusedAsset;

            Dictionary<Texture, Texture> rawTextures = DictionaryPool<Texture, Texture>.Get();

            combinerMaterial.SetTexture("_RSource", GetRawTexture(rawTextures, _rSource));
            combinerMaterial.SetTexture("_GSource", GetRawTexture(rawTextures, _gSource));
            combinerMaterial.SetTexture("_BSource", GetRawTexture(rawTextures, _bSource));
            combinerMaterial.SetTexture("_ASource", GetRawTexture(rawTextures, _aSource));

            combinerMaterial.SetFloat("_RChannel", _rChannel);
            combinerMaterial.SetFloat("_GChannel", _gChannel);
            combinerMaterial.SetFloat("_BChannel", _bChannel);
            combinerMaterial.SetFloat("_AChannel", _aChannel);

            combinerMaterial.SetVector("_RRemap", _remappings[0]);
            combinerMaterial.SetVector("_GRemap", _remappings[1]);
            combinerMaterial.SetVector("_BRemap", _remappings[2]);
            combinerMaterial.SetVector("_ARemap", _remappings[3]);

            RenderTexture combinedRT = new RenderTexture(xMin, yMin, 0, GraphicsFormat.R32G32B32A32_SFloat);

            Graphics.Blit(Texture2D.whiteTexture, combinedRT, combinerMaterial);

            // Readback the render texture
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture.active = combinedRT;
            combined.ReadPixels(new Rect(0, 0, xMin, yMin), 0, 0, false);
            combined.Apply();
            RenderTexture.active = previousActive;

            byte[] bytes = Array.Empty<byte>();

            if (savePath.EndsWith("png"))
                bytes = combined.EncodeToPNG();
            else if (savePath.EndsWith("exr"))
                bytes = combined.EncodeToEXR();
            else if (savePath.EndsWith("jpg"))
                bytes = combined.EncodeToJPG();

            string systemPath = Path.Combine(Application.dataPath.Remove(Application.dataPath.Length - 6), savePath);
            File.WriteAllBytes(systemPath, bytes);

            Object.DestroyImmediate(combined);

            AssetDatabase.ImportAsset(savePath);

            TextureImporter combinedImporter = (TextureImporter)AssetImporter.GetAtPath(savePath);
            combinedImporter.sRGBTexture = false;
            combinedImporter.SaveAndReimport();

            if (savePath.EndsWith("exr"))
            {
                // The options for the platform string are: "Standalone", "iPhone", "Android", "WebGL", "Windows Store Apps", "PSP2", "PS4", "XboxOne", "Nintendo 3DS", "WiiU", "tvOS".
                combinedImporter.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Standalone", format = TextureImporterFormat.DXT5, overridden = true });
            }

            combined = AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);

            //cleanup "raw" textures
            foreach (KeyValuePair<Texture, Texture> prop in rawTextures)
            {
                if (prop.Key != prop.Value && AssetDatabase.Contains(prop.Value))
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(prop.Value));
            }

            Object.DestroyImmediate(combinerMaterial);

            DictionaryPool<Texture, Texture>.Release(rawTextures);

            return combined;
        }

        private Texture GetRawTexture(Dictionary<Texture, Texture> rawTextures, Texture original, bool sRGBFallback = false)
        {
            if (!rawTextures.ContainsKey(original))
            {
                string path = AssetDatabase.GetAssetPath(original);
                string rawPath = "Assets/raw_" + Path.GetFileName(path);
                bool isBuiltinResource = path.Contains("unity_builtin");

                if (!isBuiltinResource && AssetDatabase.Contains(original) && AssetDatabase.CopyAsset(path, rawPath))
                {
                    AssetDatabase.ImportAsset(rawPath);

                    TextureImporter rawImporter = (TextureImporter)AssetImporter.GetAtPath(rawPath);
                    rawImporter.textureType = TextureImporterType.Default;
                    rawImporter.mipmapEnabled = false;
                    rawImporter.isReadable = true;
                    rawImporter.filterMode = _bilinearFilter ? FilterMode.Bilinear : FilterMode.Point;
                    rawImporter.npotScale = TextureImporterNPOTScale.None;
                    rawImporter.wrapMode = TextureWrapMode.Clamp;

                    Texture2D originalTex2D = original as Texture2D;
                    rawImporter.sRGBTexture = originalTex2D == null ? sRGBFallback : ((TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(original))).sRGBTexture;

                    rawImporter.maxTextureSize = 8192;

                    rawImporter.textureCompression = TextureImporterCompression.Uncompressed;

                    rawImporter.SaveAndReimport();

                    rawTextures.Add(original, AssetDatabase.LoadAssetAtPath<Texture>(rawPath));
                }
                else
                    rawTextures.Add(original, original);
            }

            return rawTextures[original];
        }
        #endregion // UnityEditor.Rendering
    }
}
