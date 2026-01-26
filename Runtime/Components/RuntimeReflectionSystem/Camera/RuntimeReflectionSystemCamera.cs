// Uncomment the line below to perform cubemap blending in the skybox with Skybox-Cubed-Blend.shader
// #define BLEND_SHADER

using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

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

        [SerializeField] private Transform targetOverride;
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
                Reflection.OnDestroy();
                Reflection = null;
                CoreUtils.Destroy(this);
                return;
            }

            GetReflectionCamera();
            Reflection.Awake();
        }

        private void LateUpdate()
        {
            Reflection.ResolutionScale = resolutionScale;
            Reflection.TargetOverride = targetOverride;
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

            if (Reflection._reflectionCamera == null)
                Reflection.CreateReflectionCamera(transform);
        }
    }

    //https://docs.unity3d.com/Documentation/ScriptReference/Camera.RenderToCubemap.html
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

        public Transform TargetOverride { set => _mainCameraTransform = value; }
        private Transform _mainCameraTransform;
        private Vector3 _previousCameraPosition;
        private Vector3 _currentCameraPosition;

        /// <summary>
        /// Array of three RenderTextures for <see cref="_reflectionCamera"/> to render cubemaps into.
        /// </summary>
        /// <remarks>
        /// While one <see cref="RenderTexture"/> is being rendered to over six frames, the previous
        /// two completed RenderTextures will be blended to produce an interpolated cubemap.
        /// </remarks>
        internal RenderTexture[] _renderTextures;

#if BLEND_SHADER
#else
        /// <summary>
        /// Create a fourth <see cref="RenderTexture"/> for the blended result if it's needed for
        /// more than just the skybox.
        /// </summary>
        internal RenderTexture _blendedTexture;

        private AsyncGPUReadbackRequest _readbackRequest;

        /// <summary>
        /// Store the average <see cref="Color32"/> of each cubemap face.
        /// </summary>
        public NativeArray<Color32> ambientColours;

        /// <summary>
        /// The weighted <see cref="Color"/> in the direction of the main camera.
        /// </summary>
        public Color adaptiveColour;

        /// <summary>
        /// The weighted <see cref="Color"/> in the direction of the sun.
        /// </summary>
        public Color skyColour;

        /// <summary>
        /// The ratio between the sky colour intensity in the direction of the sun
        /// and the opposite direction.
        /// </summary>
        public float skyRatio;

        Unity.Jobs.JobHandle handle;
        NativeArray<float> equatorArray;
        NativeArray<float> skyEquatorArray;
        NativeArray<Color> adaptiveColourRef;
        NativeArray<Color> skyColourRef;
        NativeArray<Color> invSkyColorRef;
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

        /// <summary>When true, one <see cref="CubemapFace"/> is updated per frame. Otherwise, all six are updated each frame.</summary>
        public bool timeSlice = true;

        /// <summary>Increase the mip level of the skybox <see cref="Cubemap"/> by one to filter out high frequency noise.</summary>
        public bool noiseReduction = true;

        /// <summary>When <see langword="false"/>, uses the equator colour for the ground colour.</summary>
        public bool groundColour;

        /// <summary>Remove the blue tint from the ambient colour.</summary>
        public bool removeBlue;

        /// <summary>The index of the current RenderTexture being rendered to in <see cref="_renderTextures"/>.</summary>
        internal int _index = -1;

        /// <summary>Number of frames that have been rendered since the last time <see cref="ResetFrameCount"/> was called.</summary>
        internal int _renderedFrameCount;

        /// <summary>Number of RenderTextures in <see cref="_renderTextures"/>.</summary>
        private const int ProbeCount = 3;

        /// <summary>Resolution of each face of each <see cref="RenderTexture"/> cubemap.</summary>
        private const int Resolution = 1024;

        /// <summary>Spread the <see cref="Cubemap"/> capture over six frames by rendering one face per frame.</summary>
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
                format, depthBufferBits: 0, mipCount: 1, readWrite)
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

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += UpdateCustomReflectionTexture;

            _skyboxMaterial.SetTexture(Tex, _blendedTexture);

            ambientColours = new NativeArray<Color32>(6, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            equatorArray = new NativeArray<float>(4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            skyEquatorArray = new NativeArray<float>(4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            adaptiveColourRef = new NativeArray<Color>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            skyColourRef = new NativeArray<Color>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            invSkyColorRef = new NativeArray<Color>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            adaptiveColourRef[0] = skyColourRef[0] = invSkyColorRef[0] = Color.gray;
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
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= UpdateCustomReflectionTexture;

            DestroyRenderTexture(_blendedTexture);
            _blendedTexture = null;

            handle.Complete();
            ambientColours.Dispose();
            equatorArray.Dispose();
            skyEquatorArray.Dispose();
            adaptiveColourRef.Dispose();
            skyColourRef.Dispose();
            invSkyColorRef.Dispose();
#endif // BLEND_SHADER

            if (_createdMaterial)
                CoreUtils.Destroy(ref _skyboxMaterial, skipNullCheck: true);

            if (_createdReflectionCamera)
                CoreUtils.Destroy(ref _reflectionCamera, skipNullCheck: true, destroyGameObject: true);

            if (_renderTextures != null)
            {
                foreach (var rt in _renderTextures)
                {
                    DestroyRenderTexture(rt);
                }

                System.Array.Clear(_renderTextures, index: 0, length: _renderTextures.Length);
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

            if (resolutionScaleOverride)
                ScalableBufferManager.ResizeBuffers(scaleFactor, scaleFactor);
            else
                scaleFactor = ScalableBufferManager.widthScaleFactor;

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

            // Return if the current cubemap still has more faces to render
            bool finishedRendering = ++_renderedFrameCount >= BlendFrames;
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

                _renderedFrameCount = 0;
                updated = true;
            }

            // Render a single cubemap face
            _ = _reflectionCamera.RenderToCubemap(_renderTextures[_index], NeighbouringFace(_renderedFrameCount));

            // Blend between the previous and current camera render textures
            float blend = _renderedFrameCount / (float)BlendFrames;

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

        static int NeighbouringFace(int faceMask)
        {
            return (CubemapFace)faceMask switch
            {
                CubemapFace.NegativeX => 1 << 0,
                CubemapFace.NegativeZ => 1 << 1,
                CubemapFace.NegativeY => 1 << 2,
                CubemapFace.PositiveX => 1 << 3,
                CubemapFace.PositiveZ => 1 << 4,
                CubemapFace.PositiveY => 1 << 5,
                _ => 63,
            };
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
            {
                _previousCameraPosition = _currentCameraPosition;
                _currentCameraPosition = _mainCameraTransform.position;
                return true;
            }

            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                _mainCameraTransform = mainCamera.transform;
                _previousCameraPosition = _currentCameraPosition;
                _currentCameraPosition = _mainCameraTransform.position;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Move the reflection camera to the position of the main camera.
        /// If <see cref="timeSlice"/> is <see langword="true"/>, extrapolate the position so that
        /// the capture is performed where the camera is likely to be six frames later.
        /// </summary>
        /// <remarks>
        /// Do not make the reflection camera a child of the main camera when <see cref="timeSlice"/> is <see langword="true"/>.
        /// The reflection camera should remain in the same position while all six cubemap faces are captured.
        /// </remarks>
        internal void UpdateReflectionCameraPosition()
        {
            if (FindMainCamera())
            {
                const float t = 2f;
                _reflectionCameraTransform.position = timeSlice
                    ? Vector3.LerpUnclamped(_previousCameraPosition, _currentCameraPosition, t)
                    : _currentCameraPosition;
            }
        }

        /// <summary>
        /// Override the skybox <see cref="Material"/> for the scene or the camera.
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
            _renderedFrameCount = 0;
        }

        /// <summary>
        /// Get the index of the next <see cref="RenderTexture"/> in <see cref="_renderTextures"/>.
        /// </summary>
        /// <returns>Index of the next <see cref="RenderTexture"/>.</returns>
        internal int NextIndex()
        {
            return (_index + 1) % ProbeCount;
        }

        /// <summary>
        /// Get the index of the previous <see cref="RenderTexture"/> in <see cref="_renderTextures"/>.
        /// </summary>
        /// <returns>Index of the previous <see cref="RenderTexture"/>.</returns>
        internal int PreviousIndex()
        {
            return (_index + ProbeCount - 1) % ProbeCount;
        }

        /// <summary>
        /// Calculate the mipmap level to sample the skybox <see cref="Cubemap"/>.
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
        /// <returns><see langword="false"/> if any of the RenderTextures could not be recreated.</returns>
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
        /// <returns><see langword="true"/> if <paramref name="rt"/> is created.</returns>
        private static bool CreateRenderTexture([JetBrains.Annotations.NotNull] RenderTexture rt)
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
                CoreUtils.Destroy(rt, skipNullCheck: true);
            }
        }

        #region Ambient

#if BLEND_SHADER
#else
        /// <summary>
        /// Update <see cref="RenderSettings.customReflectionTexture"/> when the <see cref="UnityEngine.SceneManagement.Scene"/> changes.
        /// </summary>
        internal void UpdateCustomReflectionTexture(UnityEngine.SceneManagement.Scene unloadedScene, UnityEngine.SceneManagement.Scene loadedScene)
        {
            RenderSettings.customReflectionTexture = _blendedTexture;
            UpdateSkybox();
        }

        /// <summary>
        /// Sample the highest mipmap level of each <see cref="Cubemap"/> face.
        /// Apply the colours to <see cref="RenderSettings"/>.
        /// </summary>
        /// <remarks>
        /// Instead of providing a callback function, avoid allocating by checking each frame if it has completed.
        /// </remarks>
        internal void UpdateAmbient()
        {
            _readbackRequest = AsyncGPUReadback.Request(_blendedTexture,
                mipIndex: _blendedTexture.mipmapCount - 1,
                x: 0, width: 1, y: 0, height: 1, z: 0, depth: 6,
                TextureFormat.RGBA32);
        }

        /// <summary>
        /// Callback after <see cref="AsyncGPUReadback"/> has completed.
        /// </summary>
        internal void GPUReadbackRequest()
        {
            handle.Complete();
            handle = default;

            adaptiveColour = adaptiveColourRef[0];
            skyColour = skyColourRef[0];
            float denominator = invSkyColorRef[0].grayscale;
            skyRatio = denominator > float.Epsilon ? skyColour.grayscale / denominator : 1;

            Color equator = equatorArray.Reinterpret<Color>(sizeof(float))[0];

            RenderSettings.ambientSkyColor = (Color)ambientColours[(int)CubemapFace.PositiveY];
            RenderSettings.ambientEquatorColor = groundColour
                ? equator
                : skyEquatorArray.Reinterpret<Color>(sizeof(float))[0];
            RenderSettings.ambientGroundColor = groundColour
                ? (Color)ambientColours[(int)CubemapFace.NegativeY]
                : equator;

            bool sunFound = LightUtils.GetDirectionalLight(out Light sun, out Transform sunTransform);

            for (int i = 0; i < ambientColours.Length; i++)
            {
                ambientColours[i] = _readbackRequest.GetData<Color32>(layer: i)[0];
            }

            if (removeBlue)
            {
                Color sunColour = default;
                float colorTemperature = 5000f;
                bool useColorTemperature = true;

                if (sunFound)
                {
                    colorTemperature = sun.colorTemperature;
                    useColorTemperature = sun.useColorTemperature;

                    if (!useColorTemperature)
                    {
                        sunColour = sun.color;
                    }
                }

                if (useColorTemperature)
                {
                    sunColour = Mathf.CorrelatedColorTemperatureToRGB(colorTemperature);
                }

                handle = Unity.Jobs.IJobForExtensions.Schedule(new RemoveBlueJob
                {
                    ambientColours = ambientColours,
                    sunColour = sunColour,
                }, 6, handle);
            }

            // Store the sum of the four equator faces as RGBA channels
            // Disposed in AverageColoursJob
            var temp = new NativeArray<float>(4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            handle = Unity.Jobs.IJobForExtensions.Schedule(new AmbientColoursJob
            {
                temp = temp,
                ambientColours = ambientColours.Reinterpret<uint>(),
            }, 4, handle);

            handle = Unity.Jobs.IJobForExtensions.Schedule(new AverageColoursJob
            {
                equator = equatorArray,
                skyEquator = skyEquatorArray,
                temp = temp,
                ambientColours = ambientColours.Reinterpret<uint>(),
            }, 4, handle);

            var sampleHandles = new NativeArray<Unity.Jobs.JobHandle>(3, Allocator.Temp);
            
            // Forward is +Z, index 4
            if (FindMainCamera())
            {
                sampleHandles[0] = Unity.Jobs.IJobExtensions.Schedule(new SampleCubemapBilinearJob
                {
                    output = adaptiveColourRef,
                    colours = ambientColours,
                    forward = _mainCameraTransform.forward,
                }, handle);
            }
            
            if (sunFound)
            {
                Vector3 sunForward = sunTransform.forward;
                
                sampleHandles[1] = Unity.Jobs.IJobExtensions.Schedule(new SampleCubemapBilinearJob
                {
                    output = skyColourRef,
                    colours = ambientColours,
                    forward = sunForward,
                }, handle);

                sampleHandles[2] = Unity.Jobs.IJobExtensions.Schedule(new SampleCubemapBilinearJob
                {
                    output = invSkyColorRef,
                    colours = ambientColours,
                    forward = -sunForward,
                }, handle);
            }

            handle = Unity.Jobs.JobHandle.CombineDependencies(sampleHandles);
            sampleHandles.Dispose();
        }

        /// <summary>
        /// Extract the <see cref="Color32"/> channel value without using a <see langword="switch"/> statement.
        /// </summary>
        /// <remarks>Assumes little-endian architecture, where r is byte 0 and b is byte 2.</remarks>
        /// <param name="colour"><see cref="Color32"/> reinterpreted as a <see langword="uint"/>.</param>
        /// <param name="channel">Use values 0 - 3 for r, g, b or a.</param>
        /// <returns><paramref name="channel"/> of <paramref name="colour"/> (Range: 0 - 255)
        /// without casting it back to a <see langword="byte"/>.</returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        internal static uint Color32Channel(uint colour, int channel)
        {
            const int byteSizeBits = 8 * sizeof(byte); // sizeof(byte) is 1
            const uint byteMaxValue = byte.MaxValue;

            return (colour >> (byteSizeBits * channel)) & byteMaxValue;
        }

        /// <summary>
        /// Replace the ambient colour of each face with the colour of the sun
        /// scaled by the average light intensity of the face.
        /// </summary>
        /// <remarks>This can replace the unnatural blue hue that can occur
        /// due to the sky occupying a larger area of the cubemap while
        /// providing less light than the sun.</remarks>
        [Unity.Burst.BurstCompile(Unity.Burst.FloatPrecision.Low, Unity.Burst.FloatMode.Fast)]
        internal struct RemoveBlueJob : Unity.Jobs.IJobFor
        {
            [NativeFixedLength(6)][NativeMatchesParallelForLength] public NativeArray<Color32> ambientColours;
            [ReadOnly] public Color sunColour;

            public void Execute(int index)
            {
                // Increase grayscale value to ensure overall intensity is about equal to the unmodified colours
                const float scale = 1.2f;
                ambientColours[index] = (Color32)(sunColour * (scale * ((Color)ambientColours[index]).grayscale));
            }
        }

        /// <summary>
        /// Calculate the sum of the skybox face colours at the horizon.
        /// Each channel is returned with a value ranging from 0f - 1020f (4 * byte.MaxValue).
        /// </summary>
        /// <remarks>The returned value is averaged and scaled in <see cref="AverageColoursJob"/>.</remarks>
        [Unity.Burst.BurstCompile(Unity.Burst.FloatPrecision.Low, Unity.Burst.FloatMode.Fast)]
        internal struct AmbientColoursJob : Unity.Jobs.IJobFor
        {
            [WriteOnly][NativeFixedLength(4)][NativeMatchesParallelForLength] public NativeArray<float> temp;
            [ReadOnly][NativeFixedLength(6)] public NativeArray<uint> ambientColours;

            public void Execute(int index)
            {
                temp[index]
                    = Color32Channel(ambientColours[(int)CubemapFace.PositiveX], index)
                    + Color32Channel(ambientColours[(int)CubemapFace.NegativeX], index)
                    + Color32Channel(ambientColours[(int)CubemapFace.PositiveZ], index)
                    + Color32Channel(ambientColours[(int)CubemapFace.NegativeZ], index);
            }
        }

        /// <summary>
        /// Calculate the average of the four colours at the horizon.
        /// </summary>
        /// <remarks><c>ambientColours[(int)CubemapFace.PositiveY]</c> cannot be passed in directly
        /// as its <see cref="NativeArray{Color32}"/> is being used by other jobs.</remarks>
        /// <param name="equator">Average colour at the horizon.</param>
        /// <param name="skyEquator">Average colour above the horizon.</param>
        /// <param name="temp">The output from <see cref="AmbientColoursJob"/>.</param>
        /// <param name="ambientColours">The ambient colours reinterpreted from <see cref="Color32"/> to <see langword="uint"/>.</param>
        [Unity.Burst.BurstCompile(Unity.Burst.FloatPrecision.Low, Unity.Burst.FloatMode.Fast)]
        internal struct AverageColoursJob : Unity.Jobs.IJobFor
        {
            [WriteOnly][NativeFixedLength(4)][NativeMatchesParallelForLength] public NativeArray<float> equator;
            [WriteOnly][NativeFixedLength(4)][NativeMatchesParallelForLength] public NativeArray<float> skyEquator;
            [ReadOnly][NativeFixedLength(4)][NativeMatchesParallelForLength][DeallocateOnJobCompletion] public NativeArray<float> temp;
            [ReadOnly][NativeFixedLength(6)] public NativeArray<uint> ambientColours;

            public void Execute(int index)
            {
                const float equatorScale = 1f / (4 * byte.MaxValue);
                const float skyEquatorScale = 1f / (6 * byte.MaxValue);
                const float ambientScale = 2f * skyEquatorScale;

                float sky = ambientScale * Color32Channel(ambientColours[(int)CubemapFace.PositiveY], index);

                skyEquator[index] = skyEquatorScale * temp[index] + sky;
                equator[index] = equatorScale * temp[index];
            }
        }

        /// <summary>
        /// Sum the weighted contribution of each <see cref="CubemapFace"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="Vector3.Distance(Vector3, Vector3)"/> calculates the difference between two vectors.
        /// The direction is inverted to calculate the similarity instead.
        /// The similarity is 2 in the same direction, 0 in the opposite direction
        /// and sqrt(2) when perpendicular.
        /// </remarks>
        [Unity.Burst.BurstCompile(Unity.Burst.FloatPrecision.Low, Unity.Burst.FloatMode.Fast)]
        internal struct SampleCubemapBilinearJob : Unity.Jobs.IJob
        {
            [WriteOnly][NativeFixedLength(1)] public NativeArray<Color> output;
            [ReadOnly][NativeFixedLength(6)] public NativeArray<Color32> colours;
            [ReadOnly] public Vector3 forward;

            public void Execute()
            {
                Color sum
                    = (Color)colours[(int)CubemapFace.PositiveX] * Vector3.Distance(Vector3.left, forward)
                    + (Color)colours[(int)CubemapFace.NegativeX] * Vector3.Distance(Vector3.right, forward)
                    + (Color)colours[(int)CubemapFace.PositiveY] * Vector3.Distance(Vector3.down, forward)
                    + (Color)colours[(int)CubemapFace.NegativeY] * Vector3.Distance(Vector3.up, forward)
                    + (Color)colours[(int)CubemapFace.PositiveZ] * Vector3.Distance(Vector3.back, forward)
                    + (Color)colours[(int)CubemapFace.NegativeZ] * Vector3.Distance(Vector3.forward, forward);

                // Scale the output channels to 0 - 1
                output[0] = sum * 0.13060193748f; // rcp(2 + 4 * sqrt(2));
            }
        }
#endif // BLEND_SHADER

        #endregion // Ambient
    }
}
