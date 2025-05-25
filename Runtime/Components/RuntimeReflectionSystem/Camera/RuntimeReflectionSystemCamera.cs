// Uncomment the line below to perform cubemap blending in the skybox with Skybox-Cubed-Blend.shader
// #define BLEND_SHADER

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace UnityExtensions
{
    //https://docs.unity3d.com/Documentation/ScriptReference/Camera.RenderToCubemap.html
    public class RuntimeReflectionSystemCamera : MonoBehaviour
    {
#if BLEND_SHADER
        readonly int TexA = Shader.PropertyToID("_TexA");
        readonly int TexB = Shader.PropertyToID("_TexB");
        readonly int Blend = Shader.PropertyToID("_Blend");
#else
        readonly int Tex = Shader.PropertyToID("_Tex");
#endif // BLEND_SHADER

        readonly int MipLevel = Shader.PropertyToID("_MipLevel");

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

        /// <summary>When true, one cubemap face is updated per frame. Otherwise, all six are updated each frame.</summary>
        public bool timeSlice = true;

        /// <summary>Increase the mip level of the skybox cubemap by one to filter out high frequency noise.</summary>
        public bool noiseReduction = true;

        /// <summary>Set to false to use the equator colour for the ground colour.</summary>
        public bool groundColour;

        /// <summary>Remove the blue tint from the ambient colour.</summary>
        public bool removeBlue;

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
                    skyboxShader = Shader.Find("Skybox/CubemapSimple");
#endif // BLEND_SHADER

                _skyboxMaterial = CoreUtils.CreateEngineMaterial(skyboxShader);

                if (_skyboxMaterial == null)
                {
                    CoreUtils.Destroy(this);
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
                _ = _renderTextures[i].Create();
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
            _ = _blendedTexture.Create();

            // Take a full capture before applying it to the skybox
            _ = reflectionCamera.RenderToCubemap(_blendedTexture);

            // Sampled with SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, uv, mipLevel)
            // or GlossyEnvironmentReflection(half3 reflectVector, half perceptualRoughness, half occlusion)
            UpdateCustomReflectionTexture(unloadedScene: default, loadedScene: default);

            SceneManager.activeSceneChanged += UpdateCustomReflectionTexture;

            _skyboxMaterial.SetTexture(Tex, _blendedTexture);
#endif // BLEND_SHADER
        }

        void LateUpdate()
        {
            if (_readbackRequest.done && !_readbackRequest.hasError)
                GPUReadbackRequest();

            float scaleFactor = resolutionScaleOverride
                ? resolutionScale
                : ScalableBufferManager.widthScaleFactor;

            if (resolutionScaleOverride)
                ScalableBufferManager.ResizeBuffers(resolutionScale, resolutionScale);

            _skyboxMaterial.SetFloat(MipLevel, GetMipLevel(scaleFactor));

            if (!EnsureCreated())
                return;

            if (!timeSlice)
            {
                UpdateReflectionCameraPosition();
                _ = reflectionCamera.RenderToCubemap(_blendedTexture);
                UpdateAmbient();
                return;
            }

            _ = TickRealtimeProbes();
        }

        void OnDestroy()
        {
            SceneManager.activeSceneChanged -= UpdateCustomReflectionTexture;

            if (_createdMaterial)
                CoreUtils.Destroy(_skyboxMaterial);

            if (_renderTextures != null)
            {
                for (int i = 0; i < _renderTextures.Length; i++)
                {
                    if (_renderTextures[i] != null)
                    {
                        _renderTextures[i].Release();
                        CoreUtils.Destroy(_renderTextures[i]);
                    }
                }
            }

#if BLEND_SHADER
#else
            if (_blendedTexture != null)
            {
                _blendedTexture.Release();
                CoreUtils.Destroy(_blendedTexture);
            }
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
            _ = reflectionCamera.RenderToCubemap(_renderTextures[_index], 1 << frameCount);

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

        /// <summary>
        /// Calculate the mipmap level to sample the skybox cubemap.
        /// </summary>
        /// <remarks>
        /// This function approximates the following formula,
        /// but since the mip level must be an integer a range check is used instead.
        /// <code>
        /// mipLevel = 1 - 1 / System.Math.Log(2, scaleFactor);
        /// </code>
        /// A higher mip level is used when noiseReduction is true
        /// to ensure that high frequency noise is blurred together.
        /// </remarks>
        /// <returns>The mip level, which will be rounded to the nearest integer in the shader.</returns>
        int GetMipLevel(float scaleFactor)
        {
            int mipLevel = 2;

            if (scaleFactor > 0.7f)
                mipLevel = 1;
            else if (scaleFactor < 0.32f)
                mipLevel = 3;

            if (noiseReduction)
                mipLevel = Mathf.Clamp(mipLevel + 1, 2, 3);

            return mipLevel;
        }

        /// <summary>
        /// <see cref="RenderTexture"/> contents can become "lost" on certain events, like loading a new level,
        /// the system going to a screensaver mode, in and out of fullscreen and so on.
        /// When that happens, existing render textures will become "not yet created" again.
        /// Check for that with the <see cref="RenderTexture.IsCreated"/> function.
        /// <see href="https://docs.unity3d.com/ScriptReference/RenderTexture.html"/>
        /// </summary>
        /// <returns>False if any of the RenderTextures could not be recreated.</returns>
        bool EnsureCreated()
        {
            bool created = true;
            foreach (var rt in _renderTextures)
            {
                if (!rt.IsCreated())
                    created &= rt.Create();
            }

#if BLEND_SHADER
#else
            if (!_blendedTexture.IsCreated())
                created &= _blendedTexture.Create();
#endif // BLEND_SHADER

            return created;
        }

        #region Ambient

        /// <summary>
        /// Update customReflectionTexture when the scene changes.
        /// </summary>
        void UpdateCustomReflectionTexture(Scene unloadedScene, Scene loadedScene)
        {
            RenderSettings.customReflectionTexture = _blendedTexture;
        }

#if BLEND_SHADER
#else
        /// <summary>
        /// Sample the highest mipmap level of each cubemap face.
        /// Apply the colours to RenderSettings.
        /// </summary>
        void UpdateAmbient()
        {
            _readbackRequest = AsyncGPUReadback.Request(_blendedTexture, 
                mipIndex: _blendedTexture.mipmapCount - 1,
                x: 0, width: 1, y: 0, height: 1, z: 0, depth: 6,
                GraphicsFormat.R8G8B8A8_UNorm);
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
            const int negativeY = (int)CubemapFace.NegativeY;
            const int positiveZ = (int)CubemapFace.PositiveZ;
            const int negativeZ = (int)CubemapFace.NegativeZ;

            for (int i = 0; i < _ambientColours.Length; i++)
            {
                _ambientColours[i] = _readbackRequest.GetData<Color32>(layer: i)[0];
            }

            if (removeBlue)
            {
                for (int i = 0; i < _ambientColours.Length; i++)
                {
                    _ambientColours[i] = new Color32(_ambientColours[i].r, _ambientColours[i].g,
                        0, _ambientColours[i].a);
                }
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
            RenderSettings.ambientEquatorColor = equator;
            RenderSettings.ambientGroundColor = groundColour
                ? _ambientColours[negativeY]
                : equator;
        }
        
        #endregion // Ambient
    }
}