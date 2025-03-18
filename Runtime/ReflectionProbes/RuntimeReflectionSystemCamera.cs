using UnityEngine;
using UnityEngine.Rendering;

namespace UnityExtensions
{
    public class RuntimeReflectionSystemCamera : MonoBehaviour
    {
        static readonly int TexA = Shader.PropertyToID("_TexA");
        static readonly int TexB = Shader.PropertyToID("_TexB");
        static readonly int Blend = Shader.PropertyToID("_Blend");
        
        [SerializeField] Shader skyboxShader;
        Material _skyboxMaterial;

        [SerializeField] Camera reflectionCamera;
        
        RenderTexture[] _renderTextures;

        // Dynamic resolution on Vulkan only supports increments of 0.05 between 0.25 and 1.0
        //[Range(0.25f, 1.0f)]
        //[SerializeField] float resolutionScale = 1.0f;
        float resolutionScale => integerScale / 20f;

        [Range(5, 20)]
        [SerializeField] int integerScale = 20;
        
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

        // Is true if a camera is currently rendering
        bool _isRendering;

        // The index of the current camera in cameras
        int _index;

        // How many frames the current camera has been rendering for. Between 0 and BlendFrames
        int _frameCount;
        
        // Snapshot of Time.renderedFrameCount the last time ResetFrameCount() was called
        int _renderedFrameCount;

        // Number of camera in cameras
        const int ProbeCount = 3;
        const int Resolution = 1024;
        const int BlendFrames = 6;

        void Awake()
        {
            if (skyboxShader == null)
                skyboxShader = Shader.Find("Skybox/CubemapBlend");
            
            if (skyboxShader != null)
                _skyboxMaterial = new Material(skyboxShader);
            
            RenderSettings.skybox = _skyboxMaterial;
            
            RenderTextureDescriptor desc = new RenderTextureDescriptor(Resolution, Resolution, RenderTextureFormat.DefaultHDR, 0)
            {
                dimension = TextureDimension.Cube,
                useDynamicScale = false, // Avoid issue when calling ScalableBufferManager.ResizeBuffers() 
                autoGenerateMips = true,
            };
            
            _renderTextures = new RenderTexture[ProbeCount];
            for (int i = 0; i < ProbeCount; i++)
            {
                _renderTextures[i] = new RenderTexture(desc);
            }
            
            RenderProbe();
        }

        void Update()
        {
            ScalableBufferManager.ResizeBuffers(resolutionScale, resolutionScale);
            TickRealtimeProbes();
        }

        void OnDestroy()
        {
            if (_skyboxMaterial != null)
                DestroyImmediate(_skyboxMaterial);
        }

        /// <inheritdoc cref="UnityEngine.Experimental.Rendering.ScriptableRuntimeReflectionSystem.TickRealtimeProbes"/>
        /// <remarks>
        /// <see cref="UnityEngine.Experimental.Rendering.ScriptableRuntimeReflectionSystem.TickRealtimeProbes"/> will create GC.Alloc
        /// due to Unity binding code, because bool as return type is not handled properly.
        /// </remarks>
        public bool TickRealtimeProbes()
        {
            // Calculate how many frames have been rendered since RenderProbe() was last called
            _frameCount = Time.frameCount - _renderedFrameCount;
            reflectionCamera.RenderToCubemap(_renderTextures[_index], 1 << _frameCount);

            // Blend between the previous and current camera render textures
            float blend = _frameCount / (float)BlendFrames;
            RenderSettings.skybox.SetFloat(Blend, blend);

            _isRendering = _frameCount < BlendFrames;
            bool wasUpdated = !_isRendering;
            if (_isRendering)
                return wasUpdated;

            //ReflectionProbe.UpdateCachedState();
            //return base.TickRealtimeProbes();

            RenderSettings.skybox.SetTexture(TexA, _renderTextures[PreviousIndex()]);
            RenderSettings.skybox.SetTexture(TexB, _renderTextures[_index]);
            RenderSettings.skybox.SetFloat(Blend, 0f);
            
            RenderSettings.customReflectionTexture = _renderTextures[_index];

            _index = NextIndex();

            RenderProbe();

            return wasUpdated;
        }

        void ResetFrameCount()
        {
            _renderedFrameCount = Time.frameCount;
        }

        void RenderProbe()
        {
            // Move the reflection camera to the position of the main camera
            // Do not make the reflection camera a child of the main camera, or it may move during time slicing
            reflectionCamera.transform.position = CameraTransform.position;

            // Render the first cubemap face
            reflectionCamera.RenderToCubemap(_renderTextures[_index], 1);
            _isRendering = true;
            ResetFrameCount();
        }

        int NextIndex()
        {
            return (_index + 1) % ProbeCount;
        }

        int PreviousIndex()
        {
            return (_index + ProbeCount - 1) % ProbeCount;
        }
    }
}