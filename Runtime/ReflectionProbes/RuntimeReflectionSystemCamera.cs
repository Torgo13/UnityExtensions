// Uncomment the line below to perform cubemap blending in the skybox with Skybox-Cubed-Blend.shader
// #define BLEND_SHADER

using UnityEngine;
using UnityEngine.Rendering;

namespace UnityExtensions
{
    //https://docs.unity3d.com/Documentation/ScriptReference/Camera.RenderToCubemap.html
    public class RuntimeReflectionSystemCamera : MonoBehaviour
    {
#if BLEND_SHADER
        static readonly int TexA = Shader.PropertyToID("_TexA");
        static readonly int TexB = Shader.PropertyToID("_TexB");
        static readonly int Blend = Shader.PropertyToID("_Blend");
#else
        static readonly int Tex = Shader.PropertyToID("_Tex");
#endif // BLEND_SHADER
        
        [SerializeField] Shader skyboxShader;
        Material _skyboxMaterial;

        [SerializeField] Camera reflectionCamera;
        Transform _reflectionCameraTransform;

        /// <summary>
        /// Array of three RenderTextures for reflectionCamera to render CubeMaps into.
        /// </summary>
        /// <remarks>
        /// While one RenderTexture is being rendered to over six frames, the previous
        /// two completed RenderTextures will be blended to produce an interpolated cubemap.
        /// </remarks>
        RenderTexture[] _renderTextures;

#if BLEND_SHADER
#else
        /// <summary>
        /// Create a fourth RenderTexture for the blended result if it's needed for
        /// more than just the skybox.
        /// </summary>
        RenderTexture _blendedTexture;
#endif // BLEND_SHADER

        public bool resolutionScaleOverride;

        [Range(0.25f, 1.0f)]
        [SerializeField] float resolutionScale = 1.0f;

        /*
        /// <summary>Dynamic resolution on Vulkan only supports increments of 0.05 between 0.25 and 1.0</summary>
        float resolutionScale => integerScale / 20f;

        [Range(5, 20)]
        [SerializeField] int integerScale = 20;
        */

        Transform _cameraTransform;
        Transform CameraTransform
        {
            get
            {
                if (_cameraTransform == null)
                {
                    var mainCamera = Camera.main;
                    if (mainCamera != null)
                    {
                        _cameraTransform = mainCamera.transform;
                    }
                }

                return _cameraTransform;
            }
        }

        /// <summary>The index of the current RenderTexture being rendered to in _renderTextures.</summary>
        int _index;

        /// <summary>Snapshot of Time.frameCount the last time ResetFrameCount() was called.</summary>
        int _renderedFrameCount;

        /// <summary>Number of RenderTextures in _renderTextures.</summary>
        const int ProbeCount = 3;

        /// <summary>Resolution of each face of each RenderTexture cubemap.</summary>
        const int Resolution = 1024;

        /// <summary>Spread the cubemap capture over six frames by rendering one face per frame.</summary>
        const int BlendFrames = 6;

        #region MonoBehaviour

        void Awake()
        {
            if (skyboxShader == null)
#if BLEND_SHADER
                skyboxShader = Shader.Find("Skybox/CubemapBlend");
#else
                skyboxShader = Shader.Find("Skybox/Cubemap");
#endif // BLEND_SHADER

            if (skyboxShader != null)
            {
                _skyboxMaterial = new Material(skyboxShader);
                _skyboxMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            
            RenderSettings.skybox = _skyboxMaterial;

            if (reflectionCamera == null)
                reflectionCamera = GetComponentInChildren<Camera>();

            _reflectionCameraTransform = reflectionCamera.transform;

            RenderTextureDescriptor desc = new RenderTextureDescriptor(Resolution, Resolution,
                RenderTextureFormat.DefaultHDR, depthBufferBits: 0)
            {
                dimension = TextureDimension.Cube,
                useDynamicScale = false, // Avoid issue when calling ScalableBufferManager.ResizeBuffers() 
                autoGenerateMips = true,
            };
            
            _renderTextures = new RenderTexture[ProbeCount];
            for (int i = 0; i < ProbeCount; i++)
            {
                _renderTextures[i] = new RenderTexture(desc);
                _renderTextures[i].hideFlags = HideFlags.HideAndDontSave;
            }

#if BLEND_SHADER
#else
            _blendedTexture = new RenderTexture(desc);
            _blendedTexture.hideFlags = HideFlags.HideAndDontSave;
            _skyboxMaterial.SetTexture(Tex, _blendedTexture);
#endif // BLEND_SHADER

            RenderNextCubemap();
        }

        void LateUpdate()
        {
            if (resolutionScaleOverride)
                ScalableBufferManager.ResizeBuffers(resolutionScale, resolutionScale);

            TickRealtimeProbes();
        }

        void OnDestroy()
        {
            if (_skyboxMaterial != null)
                DestroyImmediate(_skyboxMaterial);

            if (_renderTextures != null)
            {
                for (int i = 0; i < _renderTextures.Length; i++)
                {
                    if (_renderTextures[i] != null)
                        DestroyImmediate(_renderTextures[i]);
                }
            }

#if BLEND_SHADER
#else
            if (_blendedTexture != null)
                DestroyImmediate(_blendedTexture);
#endif // BLEND_SHADER
        }

        #endregion // MonoBehaviour

        /// <inheritdoc cref="UnityEngine.Experimental.Rendering.ScriptableRuntimeReflectionSystem.TickRealtimeProbes"/>
        /// <remarks>
        /// <see cref="UnityEngine.Experimental.Rendering.ScriptableRuntimeReflectionSystem.TickRealtimeProbes"/>
        /// will create GC.Alloc due to Unity binding code, because bool as return type is not handled properly.
        /// </remarks>
        /// <returns>True if the camera finished rendering all six faces and RenderSettings has been updated.</returns>
        public bool TickRealtimeProbes()
        {
            // Calculate how many frames have been rendered since RenderNextCubemap() was last called
            var frameCount = Time.frameCount - _renderedFrameCount;

            // Render a single cubemap face
            reflectionCamera.RenderToCubemap(_renderTextures[_index], 1 << frameCount);

            // Blend between the previous and current camera render textures
            float blend = frameCount / (float)BlendFrames;
#if BLEND_SHADER
            _skyboxMaterial.SetFloat(Blend, blend);
#else
            // With three RenderTextures, NextIndex() is equivalent to the index before PreviousIndex()
            ReflectionProbe.BlendCubemap(_renderTextures[NextIndex()], _renderTextures[PreviousIndex()],
                blend, _blendedTexture);
#endif // BLEND_SHADER

            // Return if the current cubemap still has more faces to render
            bool isRendering = frameCount < BlendFrames;
            if (isRendering)
                return false;

            //ReflectionProbe.UpdateCachedState();
            //return base.TickRealtimeProbes();

#if BLEND_SHADER
            _skyboxMaterial.SetTexture(TexA, _renderTextures[PreviousIndex()]);
            _skyboxMaterial.SetTexture(TexB, _renderTextures[_index]);
            _skyboxMaterial.SetFloat(Blend, 0f);
#endif // BLEND_SHADER

            // Update reflection texture
#if BLEND_SHADER
            RenderSettings.customReflectionTexture = _renderTextures[_index];
#else
            RenderSettings.customReflectionTexture = _blendedTexture;
#endif // BLEND_SHADER

            _index = NextIndex();
            RenderNextCubemap();

            return true;
        }

        void RenderNextCubemap()
        {
            // Move the reflection camera to the position of the main camera
            // Do not make the reflection camera a child of the main camera, or it may move during time slicing
            _reflectionCameraTransform.position = CameraTransform.position;

            // Render the first cubemap face
            reflectionCamera.RenderToCubemap(_renderTextures[_index], 1);
            ResetFrameCount();
        }

        /// <summary>
        /// Reset the timer used to measure how many frames the camera has been rendering for.
        /// </summary>
        void ResetFrameCount()
        {
            _renderedFrameCount = Time.frameCount;
        }

        /// <summary>
        /// Get the index of the next RenderTexture in _renderTextures.
        /// </summary>
        /// <returns>Index of the next RenderTexture.</returns>
        int NextIndex()
        {
            return (_index + 1) % ProbeCount;
        }

        /// <summary>
        /// Get the index of the previous RenderTexture in _renderTextures.
        /// </summary>
        /// <returns>Index of the previous RenderTexture.</returns>
        int PreviousIndex()
        {
            return (_index + ProbeCount - 1) % ProbeCount;
        }
    }
}