using System;
using System.Diagnostics;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace UnityExtensions
{
    public static class TextureUtilities
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Runtime/Utilities/HDTextureUtilities.cs
        #region UnityEngine.Rendering.HighDefinition
        /// <exception cref="ArgumentException">Thrown if <paramref name="target"/> is not a
        /// Texture2D, a RenderTexture or a Cubemap.</exception>
        public static void WriteTextureToDisk(Texture target, string filePath)
        {
            var rt = target as RenderTexture;
            if (rt != null)
            {
                target = RenderTextureToTexture(rt);
            }

            var t2D = target as Texture2D;
            if (t2D != null)
            {
                var bytes = t2D.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);
                CreateParentDirectoryIfMissing(filePath);
                File.WriteAllBytes(filePath, bytes);
                return;
            }

            var cube = target as Cubemap;
            if (cube != null)
            {
                t2D = new Texture2D(cube.width * 6, cube.height, GraphicsFormat.R16G16B16A16_SFloat,
                    TextureCreationFlags.DontInitializePixels);
                var cmd = new CommandBuffer { name = "CopyCubemapToTexture2D" };
                for (int i = 0; i < 6; ++i)
                {
                    cmd.CopyTexture(
                        cube, i, 0, 0, 0, cube.width, cube.height,
                        t2D, 0, 0, cube.width * i, 0
                    );
                }

                Graphics.ExecuteCommandBuffer(cmd);
                var bytes = t2D.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);
                CreateParentDirectoryIfMissing(filePath);
                File.WriteAllBytes(filePath, bytes);
                return;
            }

            throw new ArgumentException("Texture target is not a Texture2D, a RenderTexture or a Cubemap.");
        }

        // Write to disk via the Unity Asset Pipeline rather than File.WriteAllBytes.
        [Conditional("UNITY_EDITOR")]
        public static void WriteTextureToAsset(Texture target, string filePath)
        {
#if UNITY_EDITOR
            var rt = target as RenderTexture;
            if (rt == null)
                return;

            CreateParentDirectoryIfMissing(filePath);
            UnityEditor.AssetDatabase.CreateAsset(RenderTextureToTexture(rt), filePath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        /// <summary>
        /// Export a render texture to a texture2D.
        /// <list type="bullet">
        /// <item>Cubemap will be exported in a Texture2D of size (size * 6, size) and with a layout +X,-X,+Y,-Y,+Z,-Z</item>
        /// <item>Texture2D will be copied to a Texture2D</item>
        /// </list>
        /// </summary>
        /// <param name="source">RenderTexture with a TextureDimension of either Tex2D or Cube.</param>
        /// <returns>The copied texture.</returns>
        public static Texture2D CopyRenderTextureToTexture2D(RenderTexture source)
        {
            Assert.IsTrue(source.dimension is TextureDimension.Tex2D or TextureDimension.Cube);

            return (Texture2D)RenderTextureToTexture(source);
        }

        /// <exception cref="ArgumentException">Thrown if <paramref name="source"/> is not a
        /// Texture2D, Texture3D or Cubemap.</exception>
        private static Texture RenderTextureToTexture(RenderTexture source)
        {
            GraphicsFormat format = source.graphicsFormat;

            switch (source.dimension)
            {
                case TextureDimension.Cube:
                {
                    var resolution = source.width;

                    var result = RenderTexture.GetTemporary(resolution * 6, resolution, 
                        0, source.format);
                    var cmd = new CommandBuffer();
                    for (var i = 0; i < 6; ++i)
                        cmd.CopyTexture(source, i, 0, 0, 0, resolution, 
                            resolution, result, 0, 0, i * resolution, 0);
                    Graphics.ExecuteCommandBuffer(cmd);

                    var t2D = new Texture2D(resolution * 6, resolution, format,
                        TextureCreationFlags.DontInitializePixels);
                    var a = RenderTexture.active;
                    RenderTexture.active = result;
                    t2D.ReadPixels(new Rect(0, 0, 6 * resolution, resolution), 0, 0, recalculateMipMaps: false);
                    RenderTexture.active = a;
                    RenderTexture.ReleaseTemporary(result);
                    cmd.Dispose();

                    return t2D;
                }
                case TextureDimension.Tex2D:
                {
                    var resolution = source.width;
                    var result = new Texture2D(resolution, resolution, format,
                        TextureCreationFlags.DontInitializePixels);

                    Graphics.SetRenderTarget(source, 0);
                    result.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                    result.Apply();
                    Graphics.SetRenderTarget(null);

                    return result;
                }
                case TextureDimension.Tex3D:
                {
                    var result = new Texture3D(source.width, source.height, source.volumeDepth, format,
                        TextureCreationFlags.DontInitializePixels);

                    // Determine the number of bytes elements that need to be read based on the texture format.
                    int stagingMemorySize = (int)GraphicsFormatUtility.GetBlockSize(format)
                                            * source.width * source.height * source.volumeDepth;

                    // Staging memory for the readback.
                    var stagingReadback = new NativeArray<byte>(stagingMemorySize, Allocator.Persistent,
                        NativeArrayOptions.UninitializedMemory);

                    // Async-readbacks do not work if the RT resource is not registered with the graphics API backend.
                    Assert.IsTrue(source.IsCreated());

                    // Request and wait for the GPU data to transfer into staging memory.
                    var request = AsyncGPUReadback.RequestIntoNativeArray(ref stagingReadback, source, 0, format);
                    request.WaitForCompletion();

                    // Finally transfer the staging memory into the texture asset.
                    result.SetPixelData(stagingReadback, 0);

                    // Free the staging memory.
                    stagingReadback.Dispose();

                    return result;
                }
                default:
                    throw new ArgumentException("Texture target is not a Texture2D, a RenderTexture or a Cubemap.");
            }
        }
        #endregion // UnityEngine.Rendering.HighDefinition

        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Runtime/Utilities/HDBakingUtilities.cs
        #region UnityEngine.Rendering.HighDefinition
        public static void CreateParentDirectoryIfMissing(string path)
        {
            var fileInfo = new FileInfo(path);
            Assert.IsNotNull(fileInfo.Directory);
            
            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();
        }
        #endregion // UnityEngine.Rendering.HighDefinition
    }
}
