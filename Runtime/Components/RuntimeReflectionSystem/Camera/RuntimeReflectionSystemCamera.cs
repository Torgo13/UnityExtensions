// Uncomment the line below to perform cubemap blending in the skybox with Skybox-Cubed-Blend.shader
// #define BLEND_SHADER

using UnityEngine;
using UnityEngine.Experimental.Rendering;
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
#endif // BLEND_SHADER
        
        static readonly int MipLevel = Shader.PropertyToID("_MipLevel");

        [SerializeField] Shader skyboxShader;
        public Material _skyboxMaterial;
        bool _createdMaterial;

        [SerializeField] Camera reflectionCamera;
        Transform _reflectionCameraTransform;
        
        Transform _mainCameraTransform;

        /// <summary>
        /// Array of three RenderTextures for reflectionCamera to render cubemaps into.
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
        
        AsyncGPUReadbackRequest _readbackRequest;
        
        /// <summary>
        /// Store the average colour of each cubemap face.
        /// </summary>
        public readonly Color32[] _ambientColours = new Color32[6];

        public bool resolutionScaleOverride;

        [Range(0.25f, 1.0f)]
        [SerializeField] float resolutionScale = 1.0f;

        /*
        /// <summary>Dynamic resolution on Vulkan only supports increments of 0.05 between 0.25 and 1.0</summary>
        float resolutionScale => integerScale / 20f;

        [Range(5, 20)]
        [SerializeField] int integerScale = 20;
        */

        /// <summary>The index of the current RenderTexture being rendered to in _renderTextures.</summary>
        int _index = -1;

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
            if (_skyboxMaterial == null)
            {
                if (skyboxShader == null)
#if BLEND_SHADER
                    skyboxShader = Shader.Find("Skybox/CubemapBlend");
#else
                    skyboxShader = Shader.Find("Skybox/GlossyReflection");
#endif // BLEND_SHADER

                _skyboxMaterial = CoreUtils.CreateEngineMaterial(skyboxShader);

                if (_skyboxMaterial == null)
                {
                    Destroy(this);
                    return;
                }

                _createdMaterial = true;
            }

#if BLEND_SHADER
            RenderSettings.skybox = _skyboxMaterial;
#endif // BLEND_SHADER

            if (reflectionCamera == null)
                reflectionCamera = GetComponentInChildren<Camera>();

            _reflectionCameraTransform = reflectionCamera.transform;
            PrepareNextCubemap();

            RenderTextureDescriptor desc = new RenderTextureDescriptor(Resolution, Resolution,
                RenderTextureFormat.DefaultHDR, depthBufferBits: 0, mipCount: 0, RenderTextureReadWrite.Linear)
            {
                dimension = TextureDimension.Cube,
                autoGenerateMips = false,
            };
            
            _renderTextures = new RenderTexture[ProbeCount];
            for (int i = 0; i < ProbeCount; i++)
            {
                _renderTextures[i] = new RenderTexture(desc);
                _renderTextures[i].hideFlags = HideFlags.HideAndDontSave;
            }

#if BLEND_SHADER
#else
            desc = new RenderTextureDescriptor(Resolution, Resolution,
                RenderTextureFormat.DefaultHDR, depthBufferBits: 0)
            {
                dimension = TextureDimension.Cube,
                useMipMap = true,
                autoGenerateMips = true,
            };
            
            _blendedTexture = new RenderTexture(desc);
            _blendedTexture.hideFlags = HideFlags.HideAndDontSave;
            
            // Take a full capture before applying it to the skybox
            reflectionCamera.RenderToCubemap(_blendedTexture);

            // Sampled with SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, uv, mipLevel)
            // or GlossyEnvironmentReflection(half3 reflectVector, half perceptualRoughness, half occlusion)
            RenderSettings.customReflectionTexture = _blendedTexture;
#endif // BLEND_SHADER
        }

        void LateUpdate()
        {
            if (_readbackRequest.done && !_readbackRequest.hasError)
                GPUReadbackRequest();
            
            if (resolutionScaleOverride)
                ScalableBufferManager.ResizeBuffers(resolutionScale, resolutionScale);
            
            _skyboxMaterial.SetFloat(MipLevel, 1f / ScalableBufferManager.widthScaleFactor);

            TickRealtimeProbes();
        }

        void OnDestroy()
        {
            if (_createdMaterial && _skyboxMaterial != null)
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
            bool updated = false;
            
            // Calculate how many frames have been rendered since PrepareNextCubemap() was last called
            int frameCount = Time.frameCount - _renderedFrameCount;
            
            // Return if the current cubemap still has more faces to render
            bool finishedRendering = frameCount >= BlendFrames;
            if (finishedRendering)
            {
                //ReflectionProbe.UpdateCachedState();
                //return base.TickRealtimeProbes();

#if BLEND_SHADER
                _skyboxMaterial.SetTexture(TexA, _renderTextures[PreviousIndex()]);
                _skyboxMaterial.SetTexture(TexB, _renderTextures[_index]);
                _skyboxMaterial.SetFloat(Blend, 0f);
                
                // Update reflection texture
                RenderSettings.customReflectionTexture = _renderTextures[_index];
#else
                UpdateAmbient();
#endif // BLEND_SHADER

                PrepareNextCubemap();

                frameCount = 0;
                updated = true;
            }

            // Render a single cubemap face
            reflectionCamera.RenderToCubemap(_renderTextures[_index], 1 << frameCount);

            // Blend between the previous and current camera render textures
            float blend = frameCount / (float)BlendFrames;
#if BLEND_SHADER
            _skyboxMaterial.SetFloat(Blend, blend);
#else
            // With three RenderTextures, NextIndex() is equivalent to the index before PreviousIndex()
            // Requires six draw calls
            ReflectionProbe.BlendCubemap(_renderTextures[NextIndex()], _renderTextures[PreviousIndex()],
                blend, _blendedTexture);
#endif // BLEND_SHADER
            
            return updated;
        }

        void PrepareNextCubemap()
        {
            UpdateReflectionCameraPosition();
            ResetFrameCount();
            _index = NextIndex();
        }

        /// <summary>
        /// Move the reflection camera to the position of the main camera.
        /// </summary>
        /// <remarks>
        /// Do not make the reflection camera a child of the main camera, or it may move during time slicing.
        /// </remarks>
        void UpdateReflectionCameraPosition()
        {
            bool foundMainCamera = _mainCameraTransform != null;
            if (!foundMainCamera)
            {
                var mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    _mainCameraTransform = mainCamera.transform;
                    foundMainCamera = true;
                }
            }
                
            if (foundMainCamera)
            {
                _reflectionCameraTransform.position = _mainCameraTransform.position;
            }
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

        #region Ambient
        
#if BLEND_SHADER
#else
        /// <summary>
        /// Sample the highest mipmap level of each cubemap face.
        /// Apply the colours to RenderSettings.
        /// </summary>
        void UpdateAmbient()
        {
            _readbackRequest = AsyncGPUReadback.Request(_blendedTexture, mipIndex: _blendedTexture.mipmapCount - 1,
                x: 0, width: 1, y: 0, height: 1, z: 0, depth: 6, GraphicsFormat.R8G8B8A8_UNorm);
        }
#endif // BLEND_SHADER

        /// <summary>
        /// Callback after AsyncGPUReadback has completed.
        /// </summary>
        void GPUReadbackRequest()
        {
            // Cache the indices of the CubemapFace enum to avoid boxing
            const int positiveX = (int)CubemapFace.PositiveX;
            const int negativeX = (int)CubemapFace.NegativeX;
            const int positiveY = (int)CubemapFace.PositiveY;
#if GROUND_COLOUR
            const int negativeY = (int)CubemapFace.NegativeY;
#endif // GROUND_COLOUR
            const int positiveZ = (int)CubemapFace.PositiveZ;
            const int negativeZ = (int)CubemapFace.NegativeZ;

            for (int i = 0; i < _ambientColours.Length; i++)
            {
                _ambientColours[i] = _readbackRequest.GetData<Color32>(layer: i)[0];
            }
            
            // Get the average of the four colours at the horizon
            // Use a Vector4 to avoid colours being clamped to 1
            Vector4 equator = new Vector4(0, 0, 0, 1);
            for (int i = 0; i < 3; i++)
            {
                equator[i] = _ambientColours[positiveX][i] + _ambientColours[negativeX][i]
                    + _ambientColours[positiveZ][i] + _ambientColours[negativeZ][i];
                
                equator[i] /= 4 * byte.MaxValue;
            }
            
            RenderSettings.ambientSkyColor = _ambientColours[positiveY];
#if GROUND_COLOUR
            RenderSettings.ambientGroundColor = _ambientColours[negativeY];
#else
            RenderSettings.ambientGroundColor = equator;
#endif // GROUND_COLOUR
            RenderSettings.ambientEquatorColor = equator;
        }
        
#endregion // Ambient
    }
}