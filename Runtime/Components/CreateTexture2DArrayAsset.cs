using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace UnityExtensions
{
    //https://github.com/Unity-Technologies/BoatAttack/blob/e4864ca4381d59e553fe43f3dac6a12500eee8c7/Assets/Scripts/Editor/CreateTexture2DArrayAsset.cs
    #region BoatAttack
    [ExecuteInEditMode]
    public class CreateTexture2DArrayAsset : MonoBehaviour
    {
#if UNITY_EDITOR
        public bool mipmaps;
        public Texture2D[] textures = Array.Empty<Texture2D>();
        public Cubemap[] cubeMaps = Array.Empty<Cubemap>();

        [ContextMenu("Create Texture2D Array asset")]
        internal void CreateTexture2DAsset()
        {
            if (!Validate(textures))
                return;
            
            Texture2DArray array = new Texture2DArray(textures[0].width, textures[0].height, textures.Length,
                textures[0].format, mipmaps);
            
            for (int i = 0; i < textures.Length; i++)
                array.SetPixels(textures[i].GetPixels(), arrayElement: i);

            array.Apply(updateMipmaps: !mipmaps, makeNoLongerReadable: true);
            UnityEditor.AssetDatabase.CreateAsset(array, "Assets/TextureArray.asset");
        }

        [ContextMenu("Create Cubemap Array asset")]
        internal void CreateCubeArrayAsset()
        {
            if (!Validate(cubeMaps))
                return;

            CubemapArray array = new CubemapArray(cubeMaps[0].width, cubeMaps.Length,
                cubeMaps[0].format, mipmaps);

#if CUSTOM_MIPMAPS
            int mipLevel = mipmaps ? cubeMaps[0].mipmapCount : 1;

            for (int i = 0; i < 6; i++) //iterate for each cube face
            {
                for (int j = 0; j < cubeMaps.Length; j++)
                {
                    for (int m = 0; m < mipLevel; m++)
                    {
                        CubemapFace face = (CubemapFace)i;
                        array.SetPixels(cubeMaps[j].GetPixels(face, miplevel: m), face, arrayElement: j, miplevel: m);
                    }
                }
            }
#else
            for (int i = 0; i < 6; i++) //iterate for each cube face
            {
                for (int j = 0; j < cubeMaps.Length; j++)
                {
                    CubemapFace face = (CubemapFace)i;
                    array.SetPixels(cubeMaps[j].GetPixels(face), face, arrayElement: j);
                }
            }
#endif // CUSTOM_MIPMAPS

            array.Apply(updateMipmaps: !mipmaps, makeNoLongerReadable: true);
            UnityEditor.AssetDatabase.CreateAsset(array, "Assets/CubemapArray.asset");
        }

        bool Validate(Texture[] textures)
        {
            if (textures == null || textures.Length == 0)
                return false;
            
            bool allValid = true;

            var width = textures[0].width;
            var height = textures[0].height;
            var graphicsFormat = textures[0].graphicsFormat;

            for (int i = 1; i < textures.Length; i++)
            {
                bool matchWidth = textures[i].width == width;
                bool matchHeight = textures[i].height == height;
                bool matchFormat = textures[i].graphicsFormat == graphicsFormat;
                if (!matchWidth
                    || !matchHeight
                    || !matchFormat)
                {
                    allValid = false;
                    Debug.LogWarning($"Texture {textures[i]} does not match {textures[0]}.");
                    if (!matchWidth)
                        Debug.LogWarning($"{textures[i]} width: {textures[i].width}, {textures[0]} width: {width}.");
                    if (!matchHeight)
                        Debug.LogWarning($"{textures[i]} height: {textures[i].height}, {textures[0]} height: {height}.");
                    if (!matchFormat)
                        Debug.LogWarning($"{textures[i]} format: {textures[i].graphicsFormat}, {textures[0]} format: {graphicsFormat}.");
                }
            }
            
            return allValid;
        }
#endif // UNITY_EDITOR
    }
    #endregion // BoatAttack
}
