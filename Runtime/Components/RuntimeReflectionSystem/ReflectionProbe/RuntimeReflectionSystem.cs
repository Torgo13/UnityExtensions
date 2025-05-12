using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace UnityExtensions
{
    public class RuntimeReflectionSystem : MonoBehaviour
    {
        static readonly int TexA = Shader.PropertyToID("_TexA");
        static readonly int TexB = Shader.PropertyToID("_TexB");
        static readonly int Blend = Shader.PropertyToID("_Blend");
        
        [SerializeField] Shader skyboxShader;
        Material _skyboxMaterial;

        [SerializeField] List<ReflectionProbe> reflectionProbes;
        
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

        // Is true if a reflection probe is currently rendering
        bool _isRendering;

        // The index of the current reflection probe in reflectionProbes
        int _index;

        // The renderId of the currently rendering reflection probe, used to check if it has completed
        int _renderId = -1;

        // How many frames the current reflection probe has been rendering for. Between 0 and 14
        int _frameCount;
        
        // Snapshot of Time.renderedFrameCount the last time ResetFrameCount() was called
        int _renderedFrameCount;

        // Number of reflection probes in reflectionProbes
        const int ProbeCount = 3;
        const int Resolution = 256;

        void Awake()
        {
            if (skyboxShader == null)
                skyboxShader = Shader.Find("Skybox/CubemapBlend");
            
            if (skyboxShader != null)
                _skyboxMaterial = new Material(skyboxShader);
            
            RenderSettings.skybox = _skyboxMaterial;
            
            //Init();
            RenderProbe();
        }

        void Update()
        {
            TickRealtimeProbes();
        }

        void OnDestroy()
        {
            if (_skyboxMaterial != null)
                CoreUtils.Destroy(_skyboxMaterial);
        }
        
        /// <summary>
        /// Initialise reflection probes
        /// </summary>
        void Init()
        {
            //while (reflectionProbes.Count < ProbeCount)
            for (int i = 0; i < ProbeCount; i++)
            {
                var go = new GameObject();
                //Object.DontDestroyOnLoad(go);
                go.transform.parent = transform;
                var probeComponent = go.AddComponent<ReflectionProbe>();

                probeComponent.resolution = Resolution;
                probeComponent.mode = ReflectionProbeMode.Realtime;
                probeComponent.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
                probeComponent.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
                probeComponent.clearFlags = ReflectionProbeClearFlags.Skybox;
                probeComponent.hdr = true;
                
                probeComponent.realtimeTexture = new RenderTexture(Resolution, Resolution, 0, RenderTextureFormat.ARGB32)
                {
                    memorylessMode = RenderTextureMemoryless.Color | RenderTextureMemoryless.Depth | RenderTextureMemoryless.MSAA,
                    useDynamicScale = true,
                };

                reflectionProbes.Add(probeComponent);
            }
        }

        /// <inheritdoc cref="ScriptableRuntimeReflectionSystem.TickRealtimeProbes"/>
        /// <remarks>
        /// <see cref="ScriptableRuntimeReflectionSystem.TickRealtimeProbes"/> will create GC.Alloc
        /// due to Unity binding code, because bool as return type is not handled properly.
        /// </remarks>
        public bool TickRealtimeProbes()
        {
            // Calculate how many frames have been rendered since RenderProbe() was last called
            _frameCount = Time.frameCount - _renderedFrameCount;

            // Blend between the previous and current reflection probe render textures
            // ReflectionProbeTimeSlicingMode.IndividualFaces takes 14 frames to complete
            float blend = _frameCount / 14f;
            RenderSettings.skybox.SetFloat(Blend, blend);

            _isRendering = _frameCount < 14;
            bool wasUpdated = !_isRendering;
            if (_isRendering)
                return wasUpdated;

            //ReflectionProbe.UpdateCachedState();
            //return base.TickRealtimeProbes();

            RenderSettings.skybox.SetTexture(TexA, reflectionProbes[PreviousIndex()].realtimeTexture);
            RenderSettings.skybox.SetTexture(TexB, reflectionProbes[_index].realtimeTexture);
            RenderSettings.skybox.SetFloat(Blend, 0f);

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
            var probe = reflectionProbes[_index];

            // Move the reflection probe to the position of the camera
            // Do not make the reflection probe a child of the camera, or it may move during time slicing
            probe.transform.position = CameraTransform.position;

            // Store the renderID of the reflection probe being rendered
            _renderId = probe.RenderProbe();
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