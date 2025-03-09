using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System.Runtime.CompilerServices;
using UnityObject = UnityEngine.Object;

namespace UnityExtensions
{
    /// <summary>
    /// Set of utility functions for the Core Scriptable Render Pipeline Library
    /// </summary>
    public static class CoreUtils
    {
        //https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/Runtime/Utilities/CoreUtils.cs
        #region UnityEngine.Rendering
        /// <summary>
        /// List of look at matrices for cubemap faces.
        /// Ref: https://msdn.microsoft.com/en-us/library/windows/desktop/bb204881(v=vs.85).aspx
        /// </summary>
        static public readonly Vector3[] lookAtList =
        {
            new Vector3(1.0f, 0.0f, 0.0f),
            new Vector3(-1.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(0.0f, -1.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 0.0f, -1.0f),
        };

        /// <summary>
        /// List of up vectors for cubemap faces.
        /// Ref: https://msdn.microsoft.com/en-us/library/windows/desktop/bb204881(v=vs.85).aspx
        /// </summary>
        static public readonly Vector3[] upVectorList =
        {
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(0.0f, 0.0f, -1.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(0.0f, 1.0f, 0.0f),
        };

        static Cubemap m_BlackCubeTexture;
        /// <summary>
        /// Black cubemap texture.
        /// </summary>
        public static Cubemap blackCubeTexture
        {
            get
            {
                if (m_BlackCubeTexture == null)
                {
                    m_BlackCubeTexture = new Cubemap(1, GraphicsFormat.R8G8B8A8_SRGB, TextureCreationFlags.None);
                    for (int i = 0; i < 6; ++i)
                        m_BlackCubeTexture.SetPixel((CubemapFace)i, 0, 0, Color.black);
                    m_BlackCubeTexture.Apply();
                }

                return m_BlackCubeTexture;
            }
        }

        static Cubemap m_MagentaCubeTexture;
        /// <summary>
        /// Magenta cubemap texture.
        /// </summary>
        public static Cubemap magentaCubeTexture
        {
            get
            {
                if (m_MagentaCubeTexture == null)
                {
                    m_MagentaCubeTexture = new Cubemap(1, GraphicsFormat.R8G8B8A8_SRGB, TextureCreationFlags.None);
                    for (int i = 0; i < 6; ++i)
                        m_MagentaCubeTexture.SetPixel((CubemapFace)i, 0, 0, Color.magenta);
                    m_MagentaCubeTexture.Apply();
                }

                return m_MagentaCubeTexture;
            }
        }

        static CubemapArray m_MagentaCubeTextureArray;
        /// <summary>
        /// Black cubemap array texture.
        /// </summary>
        public static CubemapArray magentaCubeTextureArray
        {
            get
            {
                if (m_MagentaCubeTextureArray == null)
                {
                    m_MagentaCubeTextureArray = new CubemapArray(1, 1, GraphicsFormat.R32G32B32A32_SFloat, TextureCreationFlags.None);
                    for (int i = 0; i < 6; ++i)
                    {
                        Color[] colors = { Color.magenta };
                        m_MagentaCubeTextureArray.SetPixels(colors, (CubemapFace)i, 0);
                    }
                    m_MagentaCubeTextureArray.Apply();
                }

                return m_MagentaCubeTextureArray;
            }
        }

        static Cubemap m_WhiteCubeTexture;
        /// <summary>
        /// White cubemap texture.
        /// </summary>
        public static Cubemap whiteCubeTexture
        {
            get
            {
                if (m_WhiteCubeTexture == null)
                {
                    m_WhiteCubeTexture = new Cubemap(1, GraphicsFormat.R8G8B8A8_SRGB, TextureCreationFlags.None);
                    for (int i = 0; i < 6; ++i)
                        m_WhiteCubeTexture.SetPixel((CubemapFace)i, 0, 0, Color.white);
                    m_WhiteCubeTexture.Apply();
                }

                return m_WhiteCubeTexture;
            }
        }

        static RenderTexture m_EmptyUAV;
        /// <summary>
        /// Empty 1x1 texture usable as a dummy UAV.
        /// </summary>
        public static RenderTexture emptyUAV
        {
            get
            {
                if (m_EmptyUAV == null)
                {
                    m_EmptyUAV = new RenderTexture(1, 1, 0);
                    m_EmptyUAV.enableRandomWrite = true;
                    m_EmptyUAV.Create();
                }

                return m_EmptyUAV;
            }
        }

        static GraphicsBuffer m_EmptyBuffer;
        /// <summary>
        /// Empty 4-Byte buffer resource usable as a dummy.
        /// </summary>
        public static GraphicsBuffer emptyBuffer
        {
            get
            {
                if (m_EmptyBuffer == null || !m_EmptyBuffer.IsValid())
                {
                    m_EmptyBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, 1, sizeof(uint));
                }

                return m_EmptyBuffer;
            }
        }

        static Texture3D m_BlackVolumeTexture;
        /// <summary>
        /// Black 3D texture.
        /// </summary>
        public static Texture3D blackVolumeTexture
        {
            get
            {
                if (m_BlackVolumeTexture == null)
                {
                    Color[] colors = { Color.black };
                    m_BlackVolumeTexture = new Texture3D(1, 1, 1, GraphicsFormat.R8G8B8A8_SRGB, TextureCreationFlags.None);
                    m_BlackVolumeTexture.SetPixels(colors, 0);
                    m_BlackVolumeTexture.Apply();
                }

                return m_BlackVolumeTexture;
            }
        }

        internal static Texture3D m_WhiteVolumeTexture;

        /// <summary>
        /// White 3D texture.
        /// </summary>
        internal static Texture3D whiteVolumeTexture
        {
            get
            {
                if (m_WhiteVolumeTexture == null)
                {
                    Color[] colors = { Color.white };
                    m_WhiteVolumeTexture = new Texture3D(1, 1, 1, GraphicsFormat.R8G8B8A8_SRGB, TextureCreationFlags.None);
                    m_WhiteVolumeTexture.SetPixels(colors, 0);
                    m_WhiteVolumeTexture.Apply();
                }

                return m_WhiteVolumeTexture;
            }
        }

        /// <summary>
        /// Generate a name based on texture parameters.
        /// </summary>
        /// <param name="width">With of the texture.</param>
        /// <param name="height">Height of the texture.</param>
        /// <param name="format">Format of the texture.</param>
        /// <param name="dim">Dimension of the texture.</param>
        /// <param name="name">Base name of the texture.</param>
        /// <param name="mips">True if the texture has mip maps.</param>
        /// <param name="depth">Depth of the texture.</param>
        /// <returns>Generated names based on the provided parameters.</returns>
        public static string GetTextureAutoName(int width, int height, TextureFormat format, TextureDimension dim = TextureDimension.None, string name = "", bool mips = false, int depth = 0)
            => GetTextureAutoName(width, height, format.ToString(), dim, name, mips, depth);

        /// <summary>
        /// Generate a name based on texture parameters.
        /// </summary>
        /// <param name="width">With of the texture.</param>
        /// <param name="height">Height of the texture.</param>
        /// <param name="format">Graphics format of the texture.</param>
        /// <param name="dim">Dimension of the texture.</param>
        /// <param name="name">Base name of the texture.</param>
        /// <param name="mips">True if the texture has mip maps.</param>
        /// <param name="depth">Depth of the texture.</param>
        /// <returns>Generated names based on the provided parameters.</returns>
        public static string GetTextureAutoName(int width, int height, GraphicsFormat format, TextureDimension dim = TextureDimension.None, string name = "", bool mips = false, int depth = 0)
            => GetTextureAutoName(width, height, format.ToString(), dim, name, mips, depth);

        static string GetTextureAutoName(int width, int height, string format, TextureDimension dim = TextureDimension.None, string name = "", bool mips = false, int depth = 0)
        {
            string temp;
            if (depth == 0)
                temp = string.Format("{0}x{1}{2}_{3}", width, height, mips ? "_Mips" : "", format);
            else
                temp = string.Format("{0}x{1}x{2}{3}_{4}", width, height, depth, mips ? "_Mips" : "", format);
            temp = string.Format("{0}_{1}_{2}", name == "" ? "Texture" : name, (dim == TextureDimension.None) ? "" : dim.ToString(), temp);

            return temp;
        }

        /// <summary>
        /// Draws a full screen triangle.
        /// </summary>
        /// <param name="commandBuffer">CommandBuffer used for rendering commands.</param>
        /// <param name="material">Material used on the full screen triangle.</param>
        /// <param name="properties">Optional material property block for the provided material.</param>
        /// <param name="shaderPassId">Index of the material pass.</param>
        public static void DrawFullScreen(CommandBuffer commandBuffer, Material material,
            MaterialPropertyBlock properties = null, int shaderPassId = 0)
        {
            commandBuffer.DrawProcedural(Matrix4x4.identity, material, shaderPassId, MeshTopology.Triangles, 3, 1, properties);
        }

        /// <summary>
        /// Draws a full screen triangle.
        /// </summary>
        /// <param name="commandBuffer">CommandBuffer used for rendering commands.</param>
        /// <param name="material">Material used on the full screen triangle.</param>
        /// <param name="colorBuffer">RenderTargetIdentifier of the color buffer that needs to be set before drawing the full screen triangle.</param>
        /// <param name="properties">Optional material property block for the provided material.</param>
        /// <param name="shaderPassId">Index of the material pass.</param>
        public static void DrawFullScreen(CommandBuffer commandBuffer, Material material,
            RenderTargetIdentifier colorBuffer,
            MaterialPropertyBlock properties = null, int shaderPassId = 0)
        {
            commandBuffer.SetRenderTarget(colorBuffer, 0, CubemapFace.Unknown, -1);
            commandBuffer.DrawProcedural(Matrix4x4.identity, material, shaderPassId, MeshTopology.Triangles, 3, 1, properties);
        }

        /// <summary>
        /// Draws a full screen triangle.
        /// </summary>
        /// <param name="commandBuffer">CommandBuffer used for rendering commands.</param>
        /// <param name="material">Material used on the full screen triangle.</param>
        /// <param name="colorBuffer">RenderTargetIdentifier of the color buffer that needs to be set before drawing the full screen triangle.</param>
        /// <param name="depthStencilBuffer">RenderTargetIdentifier of the depth buffer that needs to be set before drawing the full screen triangle.</param>
        /// <param name="properties">Optional material property block for the provided material.</param>
        /// <param name="shaderPassId">Index of the material pass.</param>
        public static void DrawFullScreen(CommandBuffer commandBuffer, Material material,
            RenderTargetIdentifier colorBuffer, RenderTargetIdentifier depthStencilBuffer,
            MaterialPropertyBlock properties = null, int shaderPassId = 0)
        {
            commandBuffer.SetRenderTarget(colorBuffer, depthStencilBuffer, 0, CubemapFace.Unknown, -1);
            commandBuffer.DrawProcedural(Matrix4x4.identity, material, shaderPassId, MeshTopology.Triangles, 3, 1, properties);
        }

        /// <summary>
        /// Draws a full screen triangle.
        /// </summary>
        /// <param name="commandBuffer">CommandBuffer used for rendering commands.</param>
        /// <param name="material">Material used on the full screen triangle.</param>
        /// <param name="colorBuffers">RenderTargetIdentifier array of the color buffers that needs to be set before drawing the full screen triangle.</param>
        /// <param name="depthStencilBuffer">RenderTargetIdentifier of the depth buffer that needs to be set before drawing the full screen triangle.</param>
        /// <param name="properties">Optional material property block for the provided material.</param>
        /// <param name="shaderPassId">Index of the material pass.</param>
        public static void DrawFullScreen(CommandBuffer commandBuffer, Material material,
            RenderTargetIdentifier[] colorBuffers, RenderTargetIdentifier depthStencilBuffer,
            MaterialPropertyBlock properties = null, int shaderPassId = 0)
        {
            commandBuffer.SetRenderTarget(colorBuffers, depthStencilBuffer, 0, CubemapFace.Unknown, -1);
            commandBuffer.DrawProcedural(Matrix4x4.identity, material, shaderPassId, MeshTopology.Triangles, 3, 1, properties);
        }

        // Important: the first RenderTarget must be created with 0 depth bits!

        /// <summary>
        /// Draws a full screen triangle.
        /// </summary>
        /// <param name="commandBuffer">CommandBuffer used for rendering commands.</param>
        /// <param name="material">Material used on the full screen triangle.</param>
        /// <param name="colorBuffers">RenderTargetIdentifier array of the color buffers that needs to be set before drawing the full screen triangle.</param>
        /// <param name="properties">Optional material property block for the provided material.</param>
        /// <param name="shaderPassId">Index of the material pass.</param>
        public static void DrawFullScreen(CommandBuffer commandBuffer, Material material,
            RenderTargetIdentifier[] colorBuffers,
            MaterialPropertyBlock properties = null, int shaderPassId = 0)
        {
            // It is currently not possible to have MRT without also setting a depth target.
            // To work around this deficiency of the CommandBuffer.SetRenderTarget() API,
            // we pass the first color target as the depth target. If it has 0 depth bits,
            // no depth target ends up being bound.
            DrawFullScreen(commandBuffer, material, colorBuffers, colorBuffers[0], properties, shaderPassId);
        }

        // Color space utilities
        /// <summary>
        /// Converts the provided sRGB color to the current active color space.
        /// </summary>
        /// <param name="color">Input color.</param>
        /// <returns>Linear color if the active color space is ColorSpace.Linear, the original input otherwise.</returns>
        public static Color ConvertSRGBToActiveColorSpace(Color color)
        {
            return (QualitySettings.activeColorSpace == ColorSpace.Linear) ? color.linear : color;
        }

        /// <summary>
        /// Converts the provided linear color to the current active color space.
        /// </summary>
        /// <param name="color">Input color.</param>
        /// <returns>sRGB color if the active color space is ColorSpace.Gamma, the original input otherwise.</returns>
        public static Color ConvertLinearToActiveColorSpace(Color color)
        {
            return (QualitySettings.activeColorSpace == ColorSpace.Linear) ? color : color.gamma;
        }

        /// <summary>
        /// Creates a Material with the provided shader path.
        /// hideFlags will be set to HideFlags.HideAndDontSave.
        /// </summary>
        /// <param name="shaderPath">Path of the shader used for the material.</param>
        /// <returns>A new Material instance using the shader found at the provided path.</returns>
        public static Material CreateEngineMaterial(string shaderPath)
        {
            if (string.IsNullOrEmpty(shaderPath))
                throw new ArgumentException(nameof(shaderPath));

            Shader shader = Shader.Find(shaderPath);
            if (shader == null)
            {
                Debug.LogError("Cannot create required material because shader " + shaderPath + " could not be found");
                return null;
            }

            return CreateEngineMaterial(shader);
        }

        /// <summary>
        /// Creates a Material with the provided shader.
        /// hideFlags will be set to HideFlags.HideAndDontSave.
        /// </summary>
        /// <param name="shader">Shader used for the material.</param>
        /// <returns>A new Material instance using the provided shader.</returns>
        public static Material CreateEngineMaterial(Shader shader)
        {
            if (shader == null)
            {
                Debug.LogError("Cannot create required material because shader is null");
                return null;
            }


            return new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        /// <summary>
        /// Bitfield flag test.
        /// </summary>
        /// <typeparam name="T">Type of the enum flag.</typeparam>
        /// <param name="mask">Bitfield to test the flag against.</param>
        /// <param name="flag">Flag to be tested against the provided mask.</param>
        /// <returns>True if the flag is present in the mask.</returns>
        public static bool HasFlag<T>(T mask, T flag) where T : IConvertible
        {
            return (mask.ToUInt32(null) & flag.ToUInt32(null)) != 0;
        }

        /// <summary>
        /// Swaps two values.
        /// </summary>
        /// <typeparam name="T">Type of the values</typeparam>
        /// <param name="a">First value.</param>
        /// <param name="b">Second value.</param>
        public static void Swap<T>(ref T a, ref T b)
        {
            (b, a) = (a, b);
        }

        /// <summary>
        /// Set a global keyword using a CommandBuffer
        /// </summary>
        /// <param name="cmd">CommandBuffer on which to set the global keyword.</param>
        /// <param name="keyword">Keyword to be set.</param>
        /// <param name="state">Value of the keyword to be set.</param>
        public static void SetKeyword(CommandBuffer cmd, string keyword, bool state)
        {
            if (state)
                cmd.EnableShaderKeyword(keyword);
            else
                cmd.DisableShaderKeyword(keyword);
        }

        /// <summary>
        /// Set a local keyword on a ComputeShader using a CommandBuffer
        /// </summary>
        /// <param name="cmd">CommandBuffer on which to set the global keyword.</param>
        /// <param name="cs">Compute Shader on which to set the keyword.</param>
        /// <param name="keyword">Keyword to be set.</param>
        /// <param name="state">Value of the keyword to be set.</param>
        public static void SetKeyword(CommandBuffer cmd, ComputeShader cs, string keyword, bool state)
        {
            var kw = new LocalKeyword(cs, keyword);
            if (state)
                cmd.EnableKeyword(cs, kw);
            else
                cmd.DisableKeyword(cs, kw);
        }

        // Caution: such a call should not be use interleaved with command buffer command, as it is immediate
        /// <summary>
        /// Set a keyword immediately on a Material.
        /// </summary>
        /// <param name="material">Material on which to set the keyword.</param>
        /// <param name="keyword">Keyword to set on the material.</param>
        /// <param name="state">Value of the keyword to set on the material.</param>
        public static void SetKeyword(Material material, string keyword, bool state)
        {
            if (state)
                material.EnableKeyword(keyword);
            else
                material.DisableKeyword(keyword);
        }

        // Caution: such a call should not be use interleaved with command buffer command, as it is immediate
        /// <summary>
        /// Set a keyword immediately on a Material.
        /// </summary>
        /// <param name="material">Material on which to set the keyword.</param>
        /// <param name="keyword">Keyword to set on the material.</param>
        /// <param name="state">Value of the keyword to set on the material.</param>
        public static void SetKeyword(Material material, LocalKeyword keyword, bool state)
        {
            if (state)
                material.EnableKeyword(keyword);
            else
                material.DisableKeyword(keyword);
        }

        // Caution: such a call should not be use interleaved with command buffer command, as it is immediate
        /// <summary>
        /// Set a keyword immediately on a compute shader
        /// </summary>
        /// <param name="cs">Compute Shader on which to set the keyword.</param>
        /// <param name="keyword">Keyword to be set.</param>
        /// <param name="state">Value of the keyword to be set.</param>
        public static void SetKeyword(ComputeShader cs, string keyword, bool state)
        {
            if (state)
                cs.EnableKeyword(keyword);
            else
                cs.DisableKeyword(keyword);
        }

        /// <summary>
        /// Destroys a UnityObject safely.
        /// </summary>
        /// <param name="obj">Object to be destroyed.</param>
        public static void Destroy(UnityObject obj)
        {
            if (obj != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying && !UnityEditor.EditorApplication.isPaused)
                    UnityObject.Destroy(obj);
                else
                    UnityObject.DestroyImmediate(obj);
#else
                UnityObject.Destroy(obj);
#endif
            }
        }

        static IEnumerable<Type> m_AssemblyTypes;

        /// <summary>
        /// Returns all assembly types.
        /// </summary>
        /// <returns>The list of all assembly types of the current domain.</returns>
        public static IEnumerable<Type> GetAllAssemblyTypes()
        {
            if (m_AssemblyTypes == null)
            {
                m_AssemblyTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(t =>
                    {
                        // Ugly hack to handle mis-versioned dlls
                        var innerTypes = Type.EmptyTypes;
                        try
                        {
                            innerTypes = t.GetTypes();
                        }
                        catch { }
                        return innerTypes;
                    });
            }

            return m_AssemblyTypes;
        }

        /// <summary>
        /// Returns a list of types that inherit from the provided type.
        /// </summary>
        /// <typeparam name="T">Parent Type</typeparam>
        /// <returns>A list of types that inherit from the provided type.</returns>
        public static IEnumerable<Type> GetAllTypesDerivedFrom<T>()
        {
#if UNITY_EDITOR && UNITY_2019_2_OR_NEWER
            return UnityEditor.TypeCache.GetTypesDerivedFrom<T>();
#else
            return GetAllAssemblyTypes().Where(t => t.IsSubclassOf(typeof(T)));
#endif
        }

        /// <summary>
        /// Safely release a Graphics Buffer.
        /// </summary>
        /// <param name="buffer">Graphics Buffer that needs to be released.</param>
        public static void SafeRelease(GraphicsBuffer buffer)
        {
            if (buffer != null)
                buffer.Release();
        }

        /// <summary>
        /// Safely release a Compute Buffer.
        /// </summary>
        /// <param name="buffer">Compute Buffer that needs to be released.</param>
        public static void SafeRelease(ComputeBuffer buffer)
        {
            if (buffer != null)
                buffer.Release();
        }

        /// <summary>
        /// Creates a cube mesh.
        /// </summary>
        /// <param name="min">Minimum corner coordinates in local space.</param>
        /// <param name="max">Maximum corner coordinates in local space.</param>
        /// <returns>A new instance of a cube Mesh.</returns>
        public static Mesh CreateCubeMesh(Vector3 min, Vector3 max)
        {
            Mesh mesh = new Mesh();

            var vertices = UnityEngine.Pool.ListPool<Vector3>.Get();
            if (vertices.Capacity < 8)
                vertices.Capacity = 8;

            vertices.Add(new Vector3(min.x, min.y, min.z));
            vertices.Add(new Vector3(max.x, min.y, min.z));
            vertices.Add(new Vector3(max.x, max.y, min.z));
            vertices.Add(new Vector3(min.x, max.y, min.z));
            vertices.Add(new Vector3(min.x, min.y, max.z));
            vertices.Add(new Vector3(max.x, min.y, max.z));
            vertices.Add(new Vector3(max.x, max.y, max.z));
            vertices.Add(new Vector3(min.x, max.y, max.z));

            mesh.SetVertices(vertices);
            UnityEngine.Pool.ListPool<Vector3>.Release(vertices);

            var triangles = UnityEngine.Pool.ListPool<int>.Get();
            if (triangles.Capacity < 36)
                triangles.Capacity = 36;

            triangles.Add(0); triangles.Add(2); triangles.Add(1);
            triangles.Add(0); triangles.Add(3); triangles.Add(2);
            triangles.Add(1); triangles.Add(6); triangles.Add(5);
            triangles.Add(1); triangles.Add(2); triangles.Add(6);
            triangles.Add(5); triangles.Add(7); triangles.Add(4);
            triangles.Add(5); triangles.Add(6); triangles.Add(7);
            triangles.Add(4); triangles.Add(3); triangles.Add(0);
            triangles.Add(4); triangles.Add(7); triangles.Add(3);
            triangles.Add(3); triangles.Add(6); triangles.Add(2);
            triangles.Add(3); triangles.Add(7); triangles.Add(6);
            triangles.Add(4); triangles.Add(1); triangles.Add(5);
            triangles.Add(4); triangles.Add(0); triangles.Add(1);

            mesh.SetTriangles(triangles, 0);
            UnityEngine.Pool.ListPool<int>.Release(triangles);

            return mesh;
        }

        /// <summary>
        /// Returns true if "Post Processes" are enabled for the view associated with the given camera.
        /// </summary>
        /// <param name="camera">Input camera.</param>
        /// <returns>True if "Post Processes" are enabled for the view associated with the given camera.</returns>
        public static bool ArePostProcessesEnabled(Camera camera)
        {
            bool enabled = true;

#if UNITY_EDITOR
            if (camera.cameraType == CameraType.SceneView)
            {
                enabled = false;

                // Determine whether the "Post Processes" checkbox is checked for the current view.
                for (int i = 0; i < UnityEditor.SceneView.sceneViews.Count; i++)
                {
                    var sv = UnityEditor.SceneView.sceneViews[i] as UnityEditor.SceneView;

                    // Post-processing is disabled in scene view if either showImageEffects is disabled or we are
                    // rendering in wireframe mode.
                    if (sv.camera == camera &&
                        (sv.sceneViewState.imageEffectsEnabled && sv.cameraMode.drawMode != UnityEditor.DrawCameraMode.Wireframe))
                    {
                        enabled = true;
                        break;
                    }
                }
            }
#endif

            return enabled;
        }

        /// <summary>
        /// Returns true if "Animated Materials" are enabled for the view associated with the given camera.
        /// </summary>
        /// <param name="camera">Input camera.</param>
        /// <returns>True if "Animated Materials" are enabled for the view associated with the given camera.</returns>
        public static bool AreAnimatedMaterialsEnabled(Camera camera)
        {
            bool animateMaterials = true;

#if UNITY_EDITOR
            animateMaterials = Application.isPlaying; // For Game and VR views; Reflection views pass the parent camera

            if (camera.cameraType == CameraType.SceneView)
            {
                animateMaterials = false;

                // Determine whether the "Animated Materials" checkbox is checked for the current view.
                for (int i = 0; i < UnityEditor.SceneView.sceneViews.Count; i++) // Using a foreach on an ArrayList generates garbage ...
                {
                    var sv = UnityEditor.SceneView.sceneViews[i] as UnityEditor.SceneView;
#if UNITY_2020_2_OR_NEWER
                    if (sv.camera == camera && sv.sceneViewState.alwaysRefreshEnabled)
#else
                    if (sv.camera == camera && sv.sceneViewState.materialUpdateEnabled)
#endif
                    {
                        animateMaterials = true;
                        break;
                    }
                }
            }
            else if (camera.cameraType == CameraType.Preview)
            {
                // Enable for previews so the shader graph main preview works with time parameters.
                animateMaterials = true;
            }
            else if (camera.cameraType == CameraType.Reflection)
            {
                // Reflection cameras should be handled outside this function.
                // Debug.Assert(false, "Unexpected View type.");
            }

            // IMHO, a better solution would be:
            // A window invokes a camera render. The camera knows which window called it, so it can query its properies
            // (such as animated materials). This camera provides the space-time position. It should also be able
            // to access the rendering settings somehow. Using this information, it is then able to construct the
            // primary view with information about camera-relative rendering, LOD, time, rendering passes/features
            // enabled, etc. We then render this view. It can have multiple sub-views (shadows, reflections).
            // They inherit all the properties of the primary view, but also have the ability to override them
            // (e.g. primary cam pos and time are retained, matrices are modified, SSS and tessellation are disabled).
            // These views can then have multiple sub-views (probably not practical for games),
            // which simply amounts to a recursive call, and then the story repeats itself.
            //
            // TLDR: we need to know the caller and its status/properties to make decisions.
#endif

            return animateMaterials;
        }

        /// <summary>
        /// Returns true if "Scene Lighting" is enabled for the view associated with the given camera.
        /// </summary>
        /// <param name="camera">Input camera.</param>
        /// <returns>True if "Scene Lighting" is enabled for the view associated with the given camera.</returns>
        public static bool IsSceneLightingDisabled(Camera camera)
        {
            bool disabled = false;
#if UNITY_EDITOR
            if (camera.cameraType == CameraType.SceneView)
            {
                // Determine whether the "No Scene Lighting" checkbox is checked for the current view.
                for (int i = 0; i < UnityEditor.SceneView.sceneViews.Count; i++)
                {
                    var sv = UnityEditor.SceneView.sceneViews[i] as UnityEditor.SceneView;
                    if (sv.camera == camera && !sv.sceneLighting)
                    {
                        disabled = true;
                        break;
                    }
                }
            }
#endif
            return disabled;
        }

        /// <summary>
        /// Returns true if the "Light Overlap" scene view draw mode is enabled.
        /// </summary>
        /// <param name="camera">Input camera.</param>
        /// <returns>True if "Light Overlap" is enabled in the scene view associated with the input camera.</returns>
        public static bool IsLightOverlapDebugEnabled(Camera camera)
        {
            bool enabled = false;
#if UNITY_EDITOR
            if (camera.cameraType == CameraType.SceneView)
            {
                // Determine whether the "LightOverlap" mode is enabled for the current view.
                for (int i = 0; i < UnityEditor.SceneView.sceneViews.Count; i++)
                {
                    var sv = UnityEditor.SceneView.sceneViews[i] as UnityEditor.SceneView;
                    if (sv.camera == camera && sv.cameraMode.drawMode == UnityEditor.DrawCameraMode.LightOverlap)
                    {
                        enabled = true;
                        break;
                    }
                }
            }
#endif
            return enabled;
        }

        /// <summary>
        /// Returns true if "Fog" is enabled for the view associated with the given camera.
        /// </summary>
        /// <param name="camera">Input camera.</param>
        /// <returns>True if "Fog" is enabled for the view associated with the given camera.</returns>
        public static bool IsSceneViewFogEnabled(Camera camera)
        {
            bool fogEnable = true;

#if UNITY_EDITOR
            if (camera.cameraType == CameraType.SceneView)
            {
                fogEnable = false;

                var sceneViews = UnityEditor.SceneView.sceneViews;
                // Determine whether the "Animated Materials" checkbox is checked for the current view.
                for (int i = 0; i < sceneViews.Count; i++)
                {
                    if (sceneViews[i] is UnityEditor.SceneView sv && sv.camera == camera && sv.sceneViewState.fogEnabled)
                    {
                        fogEnable = true;
                        break;
                    }
                }
            }
#endif

            return fogEnable;
        }

        /// <summary>
        /// Returns true if any Scene view is using the Scene filtering.
        /// </summary>
        /// <returns>True if any Scene view is using the Scene filtering.</returns>
        public static bool IsSceneFilteringEnabled()
        {
#if UNITY_EDITOR && UNITY_2021_2_OR_NEWER
            var sceneViews = UnityEditor.SceneView.sceneViews;
            for (int i = 0; i < sceneViews.Count; i++)
            {
                if (sceneViews[i] is UnityEditor.SceneView sv && sv.isUsingSceneFiltering)
                    return true;
            }
#endif
            return false;
        }

#if UNITY_EDITOR
        static Func<int> s_GetSceneViewPrefabStageContextFunc = null;

        static Func<int> LoadSceneViewMethods()
        {
            var stageNavigatorManager = typeof(UnityEditor.SceneManagement.PrefabStage).Assembly.GetType("UnityEditor.SceneManagement.StageNavigationManager");
            var instance = stageNavigatorManager.GetProperty("instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
            var renderMode = stageNavigatorManager.GetProperty("contextRenderMode", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            var renderModeAccessor = System.Linq.Expressions.Expression.Property(System.Linq.Expressions.Expression.Property(null, instance), renderMode);
            var internalRenderModeLambda = System.Linq.Expressions.Expression.Lambda<Func<int>>(System.Linq.Expressions.Expression.Convert(renderModeAccessor, typeof(int)));
            return internalRenderModeLambda.Compile();
        }

        /// <summary>
        /// Returns true if the currently opened prefab stage context is set to Hidden.
        /// </summary>
        /// <returns>True if the currently opened prefab stage context is set to Hidden.</returns>
        public static bool IsSceneViewPrefabStageContextHidden()
        {
            s_GetSceneViewPrefabStageContextFunc ??= LoadSceneViewMethods();
            return s_GetSceneViewPrefabStageContextFunc() == 2; // 2 is hidden, see ContextRenderMode enum
        }
#else
        /// <summary>
        /// Returns true if the currently opened prefab stage context is set to Hidden.
        /// </summary>
        /// <returns>True if the currently opened prefab stage context is set to Hidden.</returns>
        public static bool IsSceneViewPrefabStageContextHidden() => false;
#endif

        /// <summary>
        /// Draw a renderer list.
        /// </summary>
        /// <param name="renderContext">Current Scriptable Render Context.</param>
        /// <param name="cmd">Command Buffer used for rendering.</param>
        /// <param name="rendererList">Renderer List to render.</param>
        public static void DrawRendererList(ScriptableRenderContext renderContext, CommandBuffer cmd, UnityEngine.Rendering.RendererList rendererList)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (!rendererList.isValid)
                throw new ArgumentException("Invalid renderer list provided to DrawRendererList");
#endif
            cmd.DrawRendererList(rendererList);
        }

        /// <summary>
        /// Compute a hash of texture properties.
        /// </summary>
        /// <param name="texture"> Source texture.</param>
        /// <returns>Returns hash of texture properties.</returns>
        public static int GetTextureHash(Texture texture)
        {
            int hash = texture.GetHashCode();

            unchecked
            {
#if UNITY_EDITOR
                hash = 23 * hash + texture.imageContentsHash.GetHashCode();
#endif
                hash = 23 * hash + texture.GetInstanceID().GetHashCode();
                hash = 23 * hash + texture.graphicsFormat.GetHashCode();
                hash = 23 * hash + texture.wrapMode.GetHashCode();
                hash = 23 * hash + texture.width.GetHashCode();
                hash = 23 * hash + texture.height.GetHashCode();
                hash = 23 * hash + texture.filterMode.GetHashCode();
                hash = 23 * hash + texture.anisoLevel.GetHashCode();
                hash = 23 * hash + texture.mipmapCount.GetHashCode();
                hash = 23 * hash + texture.updateCount.GetHashCode();
            }

            return hash;
        }

        // Hacker’s Delight, Second Edition page 66
        /// <summary>
        /// Branchless previous power of two.
        /// </summary>
        /// <param name="size">Starting size or number.</param>
        /// <returns>Previous power of two.</returns>
        public static int PreviousPowerOfTwo(int size)
        {
            if (size <= 0)
                return 0;

            size |= (size >> 1);
            size |= (size >> 2);
            size |= (size >> 4);
            size |= (size >> 8);
            size |= (size >> 16);
            return size - (size >> 1);
        }

        /// <summary>
        /// Gets the Mip Count for a given size
        /// </summary>
        /// <param name="size">The size to obtain the mip count</param>
        /// <returns>The mip count</returns>
        public static int GetMipCount(int size)
        {
            return Mathf.FloorToInt(Mathf.Log(size, 2.0f)) + 1;
        }

        /// <summary>
        /// Gets the Mip Count for a given size
        /// </summary>
        /// <param name="size">The size to obtain the mip count</param>
        /// <returns>The mip count</returns>
        public static int GetMipCount(float size)
        {
            return Mathf.FloorToInt(Mathf.Log(size, 2.0f)) + 1;
        }

        /// <summary>
        /// Divides one value by another and rounds up to the next integer.
        /// This is often used to calculate dispatch dimensions for compute shaders.
        /// </summary>
        /// <param name="value">The value to divide.</param>
        /// <param name="divisor">The value to divide by.</param>
        /// <returns>The value divided by the divisor rounded up to the next integer.</returns>
        public static int DivRoundUp(int value, int divisor)
        {
            return (value + (divisor - 1)) / divisor;
        }

        /// <summary>
        /// Get the last declared value from an enum Type
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <returns>Last value of the enum</returns>
        public static T GetLastEnumValue<T>() where T : Enum
            => typeof(T).GetEnumValues().Cast<T>().Last();

        internal static string GetCorePath()
            => "Packages/com.unity.render-pipelines.core/";

#if UNITY_EDITOR
        // This is required in Runtime assembly between #if UNITY_EDITOR
        /// <summary>
        /// AssetDataBase.FindAssets("t:&lt;type&gt;") load all asset in project to check the type.
        /// This utility function will try to filter at much possible before loading anything.
        /// This also works with Interface and inherited types.
        /// This will not find embedded sub assets.
        /// This still take times on big project so it must be only used in Editor context only.
        /// </summary>
        /// <typeparam name="T">Type or Interface to search</typeparam>
        /// <param name="extension">Extension of files to search in</param>
        /// <param name="allowSubTypes">Allows to retrieve type inheriting from T.</param>
        /// <returns>List of all asset of type T or implementing interface T.</returns>
        public static IEnumerable<T> LoadAllAssets<T>(string extension = "asset", bool allowSubTypes = true)
            where T : class
        {
            if (string.IsNullOrEmpty(extension))
                throw new ArgumentNullException(nameof(extension), "You must pass a valid extension");

            bool isInterface = typeof(T).IsInterface;
            if (!typeof(UnityObject).IsAssignableFrom(typeof(T)) && !isInterface)
                throw new Exception("T must be an interface or inherite UnityEngine.Object.");

            Func<Type, bool> needsLoad = (allowSubTypes || isInterface)
                ? (type) => typeof(T).IsAssignableFrom(type)
                : (type) => typeof(T) == type;

            string[] guids = UnityEditor.AssetDatabase.FindAssets($"glob:\"*.{extension}\"");
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                Type type = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(path);
                if (needsLoad(type))
                    yield return UnityEditor.AssetDatabase.LoadAssetAtPath(path, type) as T;
            }
        }

        /// <summary>
        /// Create any missing folders in the file path given.
        /// </summary>
        /// <param name="filePath">File or folder (ending with '/') path to ensure existence of each subfolder in. </param>
        public static void EnsureFolderTreeInAssetFilePath(string filePath)
        {
            var path = filePath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
            if (!path.StartsWith("Assets" + Path.DirectorySeparatorChar, StringComparison.CurrentCultureIgnoreCase))
                throw new ArgumentException($"Path should start with \"Assets/\". Got {filePath}.", filePath);
            var folderPath = Path.GetDirectoryName(path);

            if (!UnityEditor.AssetDatabase.IsValidFolder(folderPath))
            {
                var folderNames = folderPath.Split(Path.DirectorySeparatorChar);
                string rootPath = "";
                foreach (var folderName in folderNames)
                {
                    var newPath = rootPath + folderName;
                    if (!UnityEditor.AssetDatabase.IsValidFolder(newPath))
                        UnityEditor.AssetDatabase.CreateFolder(rootPath.TrimEnd(Path.DirectorySeparatorChar), folderName);
                    rootPath = newPath + Path.DirectorySeparatorChar;
                }
            }
        }
#endif

        /// <summary>
        /// Calcualte frustum corners at specified camera depth given projection matrix and depth z.
        /// </summary>
        /// <param name="proj"> Projection matrix used by the view frustrum. </param>
        /// <param name="z"> Z-depth from the camera origin at which the corners will be calculated. </param>
        /// <returns> Return conner vectors for left-bottom, right-bottm, right-top, left-top in view space. </returns>
        public static Vector3[] CalculateViewSpaceCorners(Matrix4x4 proj, float z)
        {
            Vector3[] outCorners = new Vector3[4];
            Matrix4x4 invProj = Matrix4x4.Inverse(proj);

            // We transform a point further than near plane and closer than far plane, for precision reasons.
            // In a perspective camera setup (near=0.1, far=1000), a point at 0.95 projected depth is about
            // 5 units from the camera.
            const float projZ = 0.95f;
            outCorners[0] = invProj.MultiplyPoint(new Vector3(-1, -1, projZ));
            outCorners[1] = invProj.MultiplyPoint(new Vector3(1, -1, projZ));
            outCorners[2] = invProj.MultiplyPoint(new Vector3(1, 1, projZ));
            outCorners[3] = invProj.MultiplyPoint(new Vector3(-1, 1, projZ));

            // Rescale vectors to have the desired z distance.
            for (int r = 0; r < 4; ++r)
                outCorners[r] *= z / (-outCorners[r].z);

            return outCorners;
        }

        /// <summary>
        /// Return the GraphicsFormat of DepthStencil RenderTarget preferred for the current platform.
        /// </summary>
        /// <returns>The GraphicsFormat of DepthStencil RenderTarget preferred for the current platform.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GraphicsFormat GetDefaultDepthStencilFormat()
        {
#if UNITY_SWITCH || UNITY_EMBEDDED_LINUX || UNITY_QNX || UNITY_ANDROID
            return GraphicsFormat.D24_UNorm_S8_UInt;
#else
            return GraphicsFormat.D32_SFloat_S8_UInt;
#endif
        }
        #endregion // UnityEngine.Rendering
    }
}
