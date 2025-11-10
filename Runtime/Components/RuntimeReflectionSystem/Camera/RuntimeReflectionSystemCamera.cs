// Uncomment the line below to perform cubemap blending in the skybox with Skybox-Cubed-Blend.shader
// #define BLEND_SHADER

using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace PKGE
{
    public class RuntimeReflectionSystemCamera : MonoBehaviour
    {
        public ReflectionSystem Reflection { get; internal set; }

        [SerializeField] private Shader skyboxShader;
        [SerializeField] internal Material skyboxMaterial;
        [SerializeField] internal Camera reflectionCamera;
        [SerializeField] internal ComputeShader texture2DArrayLerp;
        [SerializeField, Range(0.25f, 1.0f)] private float resolutionScale = 0.5f;

#if BLEND_SHADER
#else
        public NativeArray<Color32> AmbientColours => Reflection.ambientColours;
        public Color AdaptiveColour => Reflection.adaptiveColour;
        public Color SkyColour => Reflection.skyColour;
        public float SkyRatio => Reflection.skyRatio;
#endif // BLEND_SHADER

        [SerializeField] private bool skyboxOverride;
        [SerializeField] private bool cameraSkyboxOverride;
        [SerializeField] private bool resolutionScaleOverride;
        [SerializeField] private bool timeSlice = true;
        [SerializeField] private bool noiseReduction = true;
        [SerializeField] private bool groundColour;
        [SerializeField] private bool removeBlue;

        #region MonoBehaviour

        private void Awake()
        {
            Reflection ??= new ReflectionSystem(skyboxShader, skyboxMaterial, reflectionCamera, texture2DArrayLerp);

            if (!Reflection.InitialiseMaterial())
            {
                CoreUtils.Destroy(this);
                return;
            }

            GetReflectionCamera();
            Reflection.Awake();
        }

        private void LateUpdate()
        {
            Reflection.ResolutionScale = resolutionScale;
            Reflection.skyboxOverride = skyboxOverride;
            Reflection.cameraSkyboxOverride = cameraSkyboxOverride;
            Reflection.resolutionScaleOverride = resolutionScaleOverride;
            Reflection.timeSlice = timeSlice;
            Reflection.noiseReduction = noiseReduction;
            Reflection.groundColour = groundColour;
            Reflection.removeBlue = removeBlue;

            Reflection.LateUpdate();
        }

        private void OnDestroy()
        {
            Reflection?.OnDestroy();
        }

        #endregion // MonoBehaviour

        internal void GetReflectionCamera()
        {
            // Check if the field has already been assigned
            if (Reflection._reflectionCamera != null)
                return;

            // Check if it is on a child GameObject
            // It should be on a child because it will update its position to match the camera
            Reflection._reflectionCamera = GetComponentInChildren<Camera>(includeInactive: true);
            if (Reflection._reflectionCamera != null)
                return;

            Reflection.CreateReflectionCamera(transform);
        }
    }

    //https://docs.unity3d.com/Documentation/ScriptReference/Camera.RenderToCubemap.html
    [Unity.Burst.BurstCompile]
    public class ReflectionSystem
    {
        private readonly int Tex = Shader.PropertyToID("_Tex");
        private readonly int MipLevel = Shader.PropertyToID("_MipLevel");

#if BLEND_SHADER
        private readonly int TexB = Shader.PropertyToID("_TexB");
        private readonly int Blend = Shader.PropertyToID("_Blend");
#endif // BLEND_SHADER

        private const string shaderName =
#if BLEND_SHADER
            "Skybox/CubemapBlend";
#else
            "Skybox/CubemapSimple";
#endif // BLEND_SHADER

        private Shader _skyboxShader;
        internal Material _skyboxMaterial;
        private bool _createdMaterial;

        internal Camera _reflectionCamera;
        internal Transform _reflectionCameraTransform;
        private bool _createdReflectionCamera;

        private Transform _mainCameraTransform;

        /// <summary>
        /// Array of three RenderTextures for reflectionCamera to render cubemaps into.
        /// </summary>
        /// <remarks>
        /// While one RenderTexture is being rendered to over six frames, the previous
        /// two completed RenderTextures will be blended to produce an interpolated cubemap.
        /// </remarks>
        internal RenderTexture[] _renderTextures;

#if BLEND_SHADER
#else
        /// <summary>
        /// Create a fourth RenderTexture for the blended result if it's needed for
        /// more than just the skybox.
        /// </summary>
        internal RenderTexture _blendedTexture;

        private AsyncGPUReadbackRequest _readbackRequest;

        /// <summary>
        /// Store the average colour of each cubemap face.
        /// </summary>
        public NativeArray<Color32> ambientColours;

        /// <summary>
        /// The weighted colour in the direction of the camera.
        /// </summary>
        public Color adaptiveColour;

        /// <summary>
        /// The weighted colour in the direction of the sun.
        /// </summary>
        public Color skyColour;

        /// <summary>
        /// The ratio between the sky colour intensity in the direction of the sun
        /// and the opposite direction.
        /// </summary>
        public float skyRatio;
#endif // BLEND_SHADER

        /// <summary>
        /// If the current platform supports Compute Shaders,
        /// use one to blend cubemaps in a single render pass.
        /// </summary>
        private bool supportsComputeShaders;

        internal ComputeShader _texture2DArrayLerp;

        public bool skyboxOverride;
        public bool cameraSkyboxOverride;

        public bool resolutionScaleOverride;

        private float resolutionScale = 0.5f;
        public float ResolutionScale
        {
            get { return resolutionScale; }
            set { resolutionScale = System.Math.Clamp(value, 0.25f, 1.0f); }
        }

        /// <summary>When true, one cubemap face is updated per frame. Otherwise, all six are updated each frame.</summary>
        public bool timeSlice = true;

        /// <summary>Increase the mip level of the skybox cubemap by one to filter out high frequency noise.</summary>
        public bool noiseReduction = true;

        /// <summary>When <see langword="false"/>, uses the equator colour for the ground colour.</summary>
        public bool groundColour;

        /// <summary>Remove the blue tint from the ambient colour.</summary>
        public bool removeBlue;

        /// <summary>The index of the current RenderTexture being rendered to in _renderTextures.</summary>
        internal int _index = -1;

        /// <summary>Snapshot of Time.frameCount the last time ResetFrameCount() was called.</summary>
        internal int _renderedFrameCount;

        /// <summary>Number of RenderTextures in _renderTextures.</summary>
        private const int ProbeCount = 3;

        /// <summary>Resolution of each face of each RenderTexture cubemap.</summary>
        private const int Resolution = 1024;

        /// <summary>Spread the cubemap capture over six frames by rendering one face per frame.</summary>
        private const int BlendFrames = 6;

        public ReflectionSystem(Shader skyboxShader = null, Material skyboxMaterial = null, Camera reflectionCamera = null, ComputeShader texture2DArrayLerp = null)
        {
            _skyboxShader = skyboxShader;
            _skyboxMaterial = skyboxMaterial;
            _reflectionCamera = reflectionCamera;
            _texture2DArrayLerp = texture2DArrayLerp;
        }

        #region MonoBehaviour

        public void Awake()
        {
            _reflectionCameraTransform = _reflectionCamera.transform;
            PrepareNextCubemap();

            const RenderTextureFormat format = RenderTextureFormat.DefaultHDR;
            const RenderTextureReadWrite readWrite = RenderTextureReadWrite.Linear;

            var desc = new RenderTextureDescriptor(Resolution, Resolution,
                format, depthBufferBits: 0, mipCount: 0, readWrite)
            {
                dimension = TextureDimension.Cube,
                autoGenerateMips = false,
            };

            if (_renderTextures == null || _renderTextures.Length != ProbeCount)
                _renderTextures = new RenderTexture[ProbeCount];

            for (int i = 0; i < ProbeCount; i++)
            {
                _renderTextures[i] = new RenderTexture(desc);
                _renderTextures[i].hideFlags = HideFlags.HideAndDontSave;
                _ = _renderTextures[i].Create();
            }

            supportsComputeShaders = SystemInfo.supportsComputeShaders
                & _texture2DArrayLerp != null;

#if BLEND_SHADER
            UpdateSkybox();
#else
            desc = new RenderTextureDescriptor(Resolution, Resolution,
                format, depthBufferBits: 0, mipCount: -1, readWrite)
            {
                dimension = TextureDimension.Cube,
                useMipMap = true,
                autoGenerateMips = true,
                enableRandomWrite = supportsComputeShaders,
            };

            _blendedTexture = new RenderTexture(desc);
            _blendedTexture.hideFlags = HideFlags.HideAndDontSave;
            _ = _blendedTexture.Create();

            // Take a full capture before applying it to the skybox
            _ = _reflectionCamera.RenderToCubemap(_blendedTexture);
            UpdateAmbient();

            // Usually sampled with SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, uv, mipLevel)
            // or GlossyEnvironmentReflection(half3 reflectVector, half perceptualRoughness, half occlusion)
            // Cubemap must be sampled directly in Skybox shaders as unity_SpecCube0 is undefined
            UpdateCustomReflectionTexture(unloadedScene: default, loadedScene: default);

            SceneManager.activeSceneChanged += UpdateCustomReflectionTexture;

            _skyboxMaterial.SetTexture(Tex, _blendedTexture);

            ambientColours = new NativeArray<Color32>(6, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
#endif // BLEND_SHADER
        }

        public void LateUpdate()
        {
            Render(timeSlice, resolutionScaleOverride, resolutionScale);
        }

        public void OnDestroy()
        {
#if BLEND_SHADER
#else
            SceneManager.activeSceneChanged -= UpdateCustomReflectionTexture;

            DestroyRenderTexture(_blendedTexture);

            ambientColours.Dispose();
#endif // BLEND_SHADER

            if (_createdMaterial)
                CoreUtils.Destroy(_skyboxMaterial);

            if (_createdReflectionCamera)
                CoreUtils.Destroy(_reflectionCamera.gameObject);

            if (_renderTextures != null)
            {
                foreach (var rt in _renderTextures)
                {
                    DestroyRenderTexture(rt);
                }
            }
        }

        #endregion // MonoBehaviour

        public void Render(bool timeSlice, bool resolutionScaleOverride = false, float scaleFactor = 1)
        {
#if BLEND_SHADER
#else
            if (_readbackRequest.done && !_readbackRequest.hasError)
                GPUReadbackRequest();
#endif // BLEND_SHADER

            // Only update when time is progressing
            if (Time.timeScale <= float.Epsilon)
                return;

            if (!resolutionScaleOverride)
                scaleFactor = ScalableBufferManager.widthScaleFactor;

            if (resolutionScaleOverride)
                ScalableBufferManager.ResizeBuffers(scaleFactor, scaleFactor);

            _skyboxMaterial.SetFloat(MipLevel, GetMipLevel(scaleFactor));

            if (!EnsureCreated())
                return;

#if BLEND_SHADER
#else
            if (!timeSlice)
            {
                UpdateReflectionCameraPosition();
                _ = _reflectionCamera.RenderToCubemap(_blendedTexture);
                UpdateAmbient();
                return;
            }
#endif // BLEND_SHADER

            _ = TickRealtimeProbes();
        }

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
                _skyboxMaterial.SetTexture(Tex, _renderTextures[PreviousIndex()]);
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
            _ = _reflectionCamera.RenderToCubemap(_renderTextures[_index], 1 << frameCount);

            // Blend between the previous and current camera render textures
            float blend = frameCount / (float)BlendFrames;

#if BLEND_SHADER
            _skyboxMaterial.SetFloat(Blend, blend);
#else
            /*
            if (supportsComputeShaders)
            {
                CubemapBlendCompute.Blend(_texture2DArrayLerp,
                    _renderTextures[NextIndex()], _renderTextures[PreviousIndex()], _blendedTexture, blend);
            }
            else
            */
            {
                // With three RenderTextures, NextIndex() is equivalent to the index before PreviousIndex()
                // Requires six draw calls
                ReflectionProbe.BlendCubemap(_renderTextures[NextIndex()], _renderTextures[PreviousIndex()],
                    blend, _blendedTexture);
            }
#endif // BLEND_SHADER

            return updated;
        }

        internal bool InitialiseMaterial()
        {
            if (_skyboxMaterial != null)
                return true;

            if (_skyboxShader == null)
                _skyboxShader = Shader.Find(shaderName);

            _skyboxMaterial = CoreUtils.CreateEngineMaterial(_skyboxShader);

            if (_skyboxMaterial == null)
                return false;

            _createdMaterial = true;
            return true;
        }

        internal void PrepareNextCubemap()
        {
            UpdateReflectionCameraPosition();
            ResetFrameCount();
            _index = NextIndex();
        }

        public void CreateReflectionCamera(Transform transform)
        {
            var reflectionCameraGO = new GameObject();
            reflectionCameraGO.transform.parent = transform;
            _reflectionCamera = reflectionCameraGO.AddComponent<Camera>();
            _createdReflectionCamera = true;
        }

        internal bool FindMainCamera()
        {
            if (_mainCameraTransform != null)
                return true;

            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                _mainCameraTransform = mainCamera.transform;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Move the reflection camera to the position of the main camera.
        /// </summary>
        /// <remarks>
        /// Do not make the reflection camera a child of the main camera, or it may move during time slicing.
        /// </remarks>
        internal void UpdateReflectionCameraPosition()
        {
            if (FindMainCamera())
            {
                _reflectionCameraTransform.position = _mainCameraTransform.position;
            }
        }

        /// <summary>
        /// Override the skybox material for the scene or the camera.
        /// </summary>
        internal void UpdateSkybox()
        {
            if (skyboxOverride)
                RenderSettings.skybox = _skyboxMaterial;

            if (cameraSkyboxOverride)
                UpdateCameraSkybox();
        }

        internal void UpdateCameraSkybox()
        {
            if (FindMainCamera()
                && _mainCameraTransform.TryGetComponent<Skybox>(out var skybox))
            {
                skybox.material = _skyboxMaterial;
            }
        }

        /// <summary>
        /// Reset the timer used to measure how many frames the camera has been rendering for.
        /// </summary>
        internal void ResetFrameCount()
        {
            _renderedFrameCount = Time.frameCount;
        }

        /// <summary>
        /// Get the index of the next RenderTexture in _renderTextures.
        /// </summary>
        /// <returns>Index of the next RenderTexture.</returns>
        internal int NextIndex()
        {
            return (_index + 1) % ProbeCount;
        }

        /// <summary>
        /// Get the index of the previous RenderTexture in _renderTextures.
        /// </summary>
        /// <returns>Index of the previous RenderTexture.</returns>
        internal int PreviousIndex()
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
        internal int GetMipLevel(float scaleFactor)
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
        internal bool EnsureCreated()
        {
            bool created = true;
            foreach (var rt in _renderTextures)
            {
                created &= CreateRenderTexture(rt);
            }

#if BLEND_SHADER
#else
            created &= CreateRenderTexture(_blendedTexture);
#endif // BLEND_SHADER

            return created;
        }

        /// <summary>
        /// Check if <paramref name="rt"/> has already been created,
        /// and if not attempt to create it.
        /// </summary>
        /// <returns>True if <paramref name="rt"/> is created.</returns>
        private static bool CreateRenderTexture(RenderTexture rt)
        {
            if (rt.IsCreated())
                return true;

            return rt.Create();
        }

        /// <summary>
        /// Release and destroy a <see cref="RenderTexture"/>.
        /// </summary>
        private static void DestroyRenderTexture(RenderTexture rt)
        {
            if (rt != null)
            {
                rt.Release();
                CoreUtils.Destroy(ref rt, skipNullCheck: true);
            }
        }

        #region Ambient

#if BLEND_SHADER
#else
        /// <summary>
        /// Update customReflectionTexture when the scene changes.
        /// </summary>
        internal void UpdateCustomReflectionTexture(Scene unloadedScene, Scene loadedScene)
        {
            RenderSettings.customReflectionTexture = _blendedTexture;
            UpdateSkybox();
        }

        /// <summary>
        /// Sample the highest mipmap level of each cubemap face.
        /// Apply the colours to RenderSettings.
        /// </summary>
        /// <remarks>
        /// Instead of providing a callback function, check each frame if it has completed.
        /// </remarks>
        internal void UpdateAmbient()
        {
            _readbackRequest = AsyncGPUReadback.Request(_blendedTexture,
                mipIndex: _blendedTexture.mipmapCount - 1,
                x: 0, width: 1, y: 0, height: 1, z: 0, depth: 6,
                TextureFormat.RGBA32);
        }

        /// <summary>
        /// Callback after AsyncGPUReadback has completed.
        /// </summary>
        internal void GPUReadbackRequest()
        {
            for (int i = 0; i < ambientColours.Length; i++)
            {
                ambientColours[i] = _readbackRequest.GetData<Color32>(layer: i)[0];
            }

            bool sunFound = LightUtils.GetDirectionalLight(out var sun, out var sunTransform);

            if (removeBlue)
            {
                Color sunColour;

                if (sunFound)
                {
                    sunColour = sun.useColorTemperature
                        ? Mathf.CorrelatedColorTemperatureToRGB(sun.colorTemperature)
                        : sun.color;
                }
                else
                {
                    sunColour = Mathf.CorrelatedColorTemperatureToRGB(5000f);
                }

                for (int i = 0; i < ambientColours.Length; i++)
                {
                    ambientColours[i] = sunColour * ((Color)ambientColours[i]).grayscale;
                }
            }

            AverageColours(ref ambientColours, out var equator, out var skyEquator);

            RenderSettings.ambientSkyColor = ambientColours[(int)CubemapFace.PositiveY];
            RenderSettings.ambientEquatorColor = groundColour
                ? equator
                : skyEquator;
            RenderSettings.ambientGroundColor = groundColour
                ? ambientColours[(int)CubemapFace.NegativeY]
                : equator;

            // Forward is +Z, index 4
            if (FindMainCamera())
            {
                SampleCubemapBilinear(ambientColours, _mainCameraTransform.forward, out adaptiveColour);
            }

            if (!sunFound)
            {
                return;
            }

            Vector3 sunForward = sunTransform.forward;
            SampleCubemapBilinear(ambientColours, sunForward, out skyColour);
            SampleCubemapBilinear(ambientColours, -sunForward, out var invSkyColor);
            float denominator = invSkyColor.grayscale;

            skyRatio = denominator > float.Epsilon ? skyColour.grayscale / denominator : 1;
        }
        
        /// <summary>
        /// Calculate the average of the four colours at the horizon.
        /// </summary>
        /// <remarks>
        /// Use <see cref="Vector4"/> instead of <see cref="Color"/> to avoid colour values being clamped to 1.
        /// </remarks>
        /// <param name="ambientColours">Input colours.</param>
        /// <param name="equator">Average colour at the horizon.</param>
        /// <param name="skyEquator">Average colour above the horizon.</param>
        [Unity.Burst.BurstCompile]
        internal static void AverageColours(ref NativeArray<Color32> ambientColours,
            out Vector4 equator, out Vector4 skyEquator)
        {
            const float equatorScale = 1f / (4 * byte.MaxValue);
            const float skyEquatorScale = 1f / (6 * byte.MaxValue);
            
            equator = default;
            skyEquator = default;
            
            for (int i = 0; i < 4; i++)
            {
                var temp = ambientColours[(int)CubemapFace.PositiveX][i]
                          + ambientColours[(int)CubemapFace.NegativeX][i]
                          + ambientColours[(int)CubemapFace.PositiveZ][i]
                          + ambientColours[(int)CubemapFace.NegativeZ][i];

                equator[i] = equatorScale * temp;
                skyEquator[i] = skyEquatorScale * (temp + (0.5f * ambientColours[(int)CubemapFace.PositiveY][i]));
            }
        }

        /// <summary>
        /// Sum the weighted contribution of each cubemap face.
        /// </summary>
        /// <remarks>
        /// <see cref="Vector3.Distance"/> calculates the difference between two vectors.
        /// The direction is inverted to calculate the similarity instead.
        /// </remarks>
        [Unity.Burst.BurstCompile]
        internal static void SampleCubemapBilinear(in NativeArray<Color32> colours, in Vector3 forward,
            out Color sum)
        {
            const float scale = 1f / 6;
            sum = (Color)colours[(int)CubemapFace.NegativeX] * Vector3.Distance(Vector3.right, forward)
                + (Color)colours[(int)CubemapFace.PositiveX] * Vector3.Distance(Vector3.left, forward)
                + (Color)colours[(int)CubemapFace.NegativeY] * Vector3.Distance(Vector3.up, forward)
                + (Color)colours[(int)CubemapFace.PositiveY] * Vector3.Distance(Vector3.down, forward)
                + (Color)colours[(int)CubemapFace.NegativeZ] * Vector3.Distance(Vector3.forward, forward)
                + (Color)colours[(int)CubemapFace.PositiveZ] * Vector3.Distance(Vector3.back, forward);

            sum *= scale;
        }
#endif // BLEND_SHADER

        #endregion // Ambient
    }
}
