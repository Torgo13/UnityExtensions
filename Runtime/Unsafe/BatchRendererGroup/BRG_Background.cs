using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Profiling;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if INCLUDE_MATHEMATICS
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
#else
using PKGE.Mathematics;
using Random = System.Random;
using float3 = UnityEngine.Vector3;
using float4 = UnityEngine.Vector4;
#endif // INCLUDE_MATHEMATICS

namespace PKGE.Unsafe
{
    [BurstCompile]
    public class BRG_Background : MonoBehaviour
    {
#if INCLUDE_RENDER_PIPELINE_CORE
        //https://github.com/Unity-Technologies/brg-shooter/blob/f55f6e985bf73b0a3c23b95030e890874e552c45/Assets/Scripts/BRG_Background.cs
        #region brg-shooter
        public static BRG_Background gBackgroundManager;

        public Mesh m_mesh;
        public Material m_material;
        public bool m_castShadows;
        public float m_motionSpeed = 3.0f;
        public float m_motionAmplitude = 2.0f;
        public float m_spacingFactor = 1.0f;
        public bool m_debrisDebugTest = false;
        public float m_phaseSpeed1 = 1.0f;

        static readonly ProfilerMarker s_BackgroundGPUSetData = new ProfilerMarker("BRG_Background.GPUSetData");
        static readonly ProfilerMarker s_DebrisGPUSetData = new ProfilerMarker("BRG_Debris.GPUSetData");

        public int m_backgroundW = 30;
        public int m_backgroundH = 100;
        private const int kGpuItemSize = (3 * 2 + 1) * 16; //  GPU item size (2 * 4x3 matrices plus 1 color per item)

        private BRG_Container m_brgContainer;
        private JobHandle m_updateJobFence;

        private List<int> m_magnetCells = new List<int>();

        private int m_itemCount;
        private float m_phase = 0.0f;
        private float m_burstTimer = 0.0f;
        private uint m_slicePos;

        [StructLayout(LayoutKind.Sequential)]
        public struct BackgroundItem
        {
            public float x;
            public float hInitial;
            public float h; // scale
            public float phase;
            public int weight;
            public float magnetIntensity;
            public float flashTime;
            public float4 color;
        };

        public NativeArray<BackgroundItem> m_backgroundItems;

        public void Awake()
        {
            gBackgroundManager = this;
        }

        private int cellIdCompute(Vector3 pos)
        {
            float smoothScrolling = m_phase;
            // find background cell
            uint xc = (uint)(int)(pos.x);
            uint zc = (uint)(int)(pos.z + 0.5f + smoothScrolling);
            if ((xc < m_backgroundW) && (zc < m_backgroundH))
            {
                zc = (m_slicePos + zc) % (uint)m_backgroundH;
                return (int)(zc * m_backgroundW + xc);
            }
            else
                return -1;
        }

        /// <summary>
        /// If framerate drops, we may miss some cells if missile delta position is large,
        /// so the while loop will trigger any in-between cell.
        /// Most of the time the loop is a single iteration.
        /// </summary>
        public void SetMagnetCell(Vector3 previousPos, Vector3 pos)
        {
            int cellStart = cellIdCompute(previousPos);
            int cellEnd = cellIdCompute(pos);
            if ((cellStart >= 0) && (cellEnd >= 0))
            {
                while (cellStart <= cellEnd)
                {
                    m_magnetCells.Add(cellStart);
                    cellStart += m_backgroundW;
                }
            }
        }

        [BurstCompile]
        private static void InjectNewSlice(ref uint m_slicePos, int m_backgroundH, int m_backgroundW,
            ref NativeArray<BackgroundItem> m_backgroundItems)
        {
#if INCLUDE_MATHEMATICS
            var Random = new Random(
                math.max(1, new NativeReference<uint>(Allocator.Temp,
                    NativeArrayOptions.UninitializedMemory).Value));
#else
            var Random = new Random(new NativeArray<int>(1, Allocator.Temp,
                NativeArrayOptions.UninitializedMemory)[0]);
#endif // INCLUDE_MATHEMATICS

            m_slicePos++;

            int sPos0 = (int)((m_slicePos + m_backgroundH - 1) % m_backgroundH);
            sPos0 *= m_backgroundW;
            for (int x = 0; x < m_backgroundW; x++)
            {
                float scaleY = 40.0f;
                if (x > 0)
                {
                    float rnd = Random.NextFloat(0.0f, 1.0f);
                    float amp = 20.0f * (rnd + 1.0f);
                    scaleY = 1.0f + rnd;
                    scaleY += amp / (x + 1);
                }

                //scaleY = ((m_slicePos&1)!=0) ? 1.0f : 2.0f;
                //scaleY = (((x^m_slicePos)&1)!=0) ? 1.0f : 2.0f;

                //float fy = scaleY * 0.5f;

                //float sat = 0.6f + 0.2f + 0.2f * Mathf.Sin((m_slicePos + x) / 12.0f);
                float sat = (float)(0.6 + 0.2 + 0.2 * System.Math.Sin(((double)m_slicePos + x) / 12.0));

                //float value = UnityEngine.Random.Range(0.0f, 1.0f);
                //float sat = 0.5f;
                float value = 0.3f;

                if (x > 0 && x < m_backgroundW - 1
                    && Random.NextFloat(0.0f, 1.0f) > 0.8f)
                {
                    value = 1.0f;
                    sat = 1.0f;
                }

                Color col = Color.HSVToRGB((float)(((double)m_slicePos + x) / 400.0 % 1.0), sat, value);

                // write colors right after the 4x3 matrices
                BackgroundItem item = new BackgroundItem();
                item.x = x + 0.5f;
                item.hInitial = scaleY;
                item.phase = x * 0.5f;
                item.color = new float4(col.r, col.g, col.b, 1);
                item.weight = 0;
                item.flashTime = 0.0f;
                m_backgroundItems[sPos0] = item;

                sPos0++;
            }
        }

        void Start()
        {
            m_itemCount = m_backgroundW * m_backgroundH;

            m_brgContainer = new BRG_Container();
            m_brgContainer.Init(m_mesh, m_material, m_itemCount, kGpuItemSize, m_castShadows);

            // setup positions & scale of each background elements
            m_backgroundItems = new NativeArray<BackgroundItem>(m_itemCount, Allocator.Persistent,
                NativeArrayOptions.ClearMemory);
            
            // fill a complete background buffer
            for (int i = 0; i < m_backgroundH; i++)
                InjectNewSlice(ref m_slicePos, m_backgroundH, m_backgroundW, ref m_backgroundItems);

            m_brgContainer.UploadGpuData(m_itemCount);

            if (Application.platform == RuntimePlatform.Android)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 300;
                Screen.SetResolution(1280, 720, FullScreenMode.ExclusiveFullScreen,
                    new RefreshRate() { numerator = 60, denominator = 1 });
            }
        }

        [BurstCompile]
        private struct UpdatePositionsJob : IJobFor
        {
            [WriteOnly] [NativeDisableParallelForRestriction]
            public NativeArray<float4> _sysmemBuffer;

            [NativeDisableParallelForRestriction] public NativeArray<BackgroundItem> backgroundItems;

            [ReadOnly] public float smoothScroll;
            [ReadOnly] public int slicePos;
            [ReadOnly] public int backgroundW;
            [ReadOnly] public int backgroundH;
            [ReadOnly] public float _dt;
            [ReadOnly] public float _phaseSpeed;
            [ReadOnly] public int _maxInstancePerWindow;
            [ReadOnly] public int _windowSizeInFloat4;

            public void Execute(int sliceIndex)
            {
                int slice = (sliceIndex + slicePos) % backgroundH;
                float pz = sliceIndex - smoothScroll;
                float phaseSpeed = _phaseSpeed * _dt;

                for (int x = 0; x < backgroundW; x++)
                {
                    int itemId = slice * backgroundW + x;
                    BackgroundItem item = backgroundItems[itemId];

                    float4 color = item.color;
                    if (item.flashTime > 0.0f)
                    {
                        color = math.lerp(color, new float4(1, 1, 1, 1), item.flashTime);
                    }

                    float waveY = item.hInitial + 0.5f + math.sin(item.phase) * 0.5f;
                    float scaleY = waveY;
                    if (item.magnetIntensity > 0.0f)
                    {
                        float alpha = Mathf.SmoothStep(0, 1, item.magnetIntensity);
                        scaleY += alpha * 1.5f;
                        color = math.lerp(color, color + new float4(1.0f, 0.3f, 0.3f, 0), alpha);
                        item.magnetIntensity -= _dt * 3.0f;
                    }

                    item.h = scaleY;

                    float phaseInc = (item.weight <= 0) ? phaseSpeed : phaseSpeed * 0.3f;
                    item.phase += phaseInc;

                    int windowId = System.Math.DivRem(slice * backgroundW + x, _maxInstancePerWindow, out int i);
                    int windowOffsetInFloat4 = windowId * _windowSizeInFloat4;

                    // compute the new current frame matrix
                    _sysmemBuffer[(windowOffsetInFloat4 + i * 3 + 0)] = new float4(1, 0, 0, 0);
                    _sysmemBuffer[(windowOffsetInFloat4 + i * 3 + 1)] = new float4(scaleY, 0, 0, 0);
                    _sysmemBuffer[(windowOffsetInFloat4 + i * 3 + 2)] = new float4(1, item.x, scaleY * 0.5f, pz);

                    // compute the new inverse matrix (note: shortcut use identity because aligned cubes normals aren't affected by any non uniform scale
                    _sysmemBuffer[(windowOffsetInFloat4 + _maxInstancePerWindow * 3 * 1 + i * 3 + 0)] =
                        new float4(1, 0, 0, 0);
                    _sysmemBuffer[(windowOffsetInFloat4 + _maxInstancePerWindow * 3 * 1 + i * 3 + 1)] =
                        new float4(1, 0, 0, 0);
                    _sysmemBuffer[(windowOffsetInFloat4 + _maxInstancePerWindow * 3 * 1 + i * 3 + 2)] =
                        new float4(1, 0, 0, 0);
                    item.flashTime -= _dt * 1.0f; // 1 second white flash

                    // update colors
                    _sysmemBuffer[windowOffsetInFloat4 + _maxInstancePerWindow * 3 * 2 + i] = color;

                    backgroundItems[itemId] = item;
                }
            }
        }

        JobHandle UpdatePositions(float smoothScroll, float dt, JobHandle jobFence)
        {
            NativeArray<float4> sysmemBuffer = m_brgContainer.GetSysmemBuffer(out _, out int alignedWindowSize);

            UpdatePositionsJob myJob = new UpdatePositionsJob()
            {
                _sysmemBuffer = sysmemBuffer,
                backgroundItems = m_backgroundItems,
                smoothScroll = smoothScroll,
                slicePos = (int)m_slicePos,
                backgroundW = m_backgroundW,
                backgroundH = m_backgroundH,
                _dt = dt,
                _phaseSpeed = m_phaseSpeed1,
                _maxInstancePerWindow = alignedWindowSize / kGpuItemSize,
                _windowSizeInFloat4 = alignedWindowSize / 16,
            };

            jobFence = myJob.ScheduleParallel(m_backgroundH, 4, jobFence); // 4 slices per job
            return jobFence;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            m_phase += dt * m_motionSpeed;
            m_burstTimer -= dt;

            while (m_phase >= 1.0f)
            {
                InjectNewSlice(ref m_slicePos, m_backgroundH, m_backgroundW, ref m_backgroundItems);
                m_phase -= 1.0f;
            }

            if (Input.touchCount == 3)
            {
                if ((Input.GetTouch(0).phase == TouchPhase.Began) &&
                    (Input.GetTouch(1).phase == TouchPhase.Began) &&
                    (Input.GetTouch(2).phase == TouchPhase.Began))
                    m_debrisDebugTest = !m_debrisDebugTest;
            }

            if (Input.GetKeyDown(KeyCode.F7))
                m_debrisDebugTest = !m_debrisDebugTest;

            // read back the cell list with magnet and set intensity to 1
            int magnetCellsCount = m_magnetCells.Count;
            if (magnetCellsCount > 0)
            {
                for (int i = 0; i < magnetCellsCount; i++)
                {
                    int cellId = m_magnetCells[i];
                    BackgroundItem item = m_backgroundItems[cellId];
                    item.magnetIntensity = 1.0f;
                    m_backgroundItems[cellId] = item;
                }

                m_magnetCells.Clear();
            }

            JobHandle jobFence = new JobHandle();

            if (BRG_Debris.gDebrisManager != null)
            {
                if (m_debrisDebugTest
                    && m_burstTimer <= 0.0f)
                {
#if INCLUDE_MATHEMATICS
                    var Random = new Random(
                        math.max(1, new NativeReference<uint>(Allocator.Temp,
                            NativeArrayOptions.UninitializedMemory).Value));
#else
                    var Random = new Random(new NativeArray<int>(1, Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory)[0]);
#endif // INCLUDE_MATHEMATICS
#if true
                    for (int e = 0; e < 8; e++)
                    {
                        var pos = new float3(m_backgroundW / 2 + Random.Range(-8.0f, 8.0f), 16.0f,
                            m_backgroundH / 2 + Random.Range(-20.0f, 20.0f));
                        BRG_Debris.gDebrisManager.GenerateBurstOfDebris(pos, 512, Random.Range(0.0f, 1.0f));
                    }
#else
                    BRG_Debris.gDebrisManager.GenerateBurstOfDebris(new Vector3(m_backgroundW / 2, 16.0f, m_backgroundH / 2), 512*8, Random.Range(0.0f, 1.0f));
#endif
                    m_burstTimer = 3.0f;
                }

                jobFence = BRG_Debris.gDebrisManager.AddPhysicsUpdateJob(m_backgroundItems, (int)m_slicePos,
                    (uint)m_backgroundW, (uint)m_backgroundH, dt, dt * m_motionSpeed, m_phase);
            }

            m_updateJobFence = UpdatePositions(m_phase, dt, jobFence);
        }

        private void LateUpdate()
        {
            m_updateJobFence.Complete();

            // upload ground cells
            s_BackgroundGPUSetData.Begin();
            m_brgContainer.UploadGpuData(m_itemCount);
            s_BackgroundGPUSetData.End();

            // upload debris GPU data
            s_DebrisGPUSetData.Begin();
            BRG_Debris.gDebrisManager.UploadGpuData();
            s_DebrisGPUSetData.End();
        }

        private void OnDestroy()
        {
            if (m_brgContainer != null)
                m_brgContainer.Shutdown();

            m_backgroundItems.Dispose();
        }
        #endregion // brg-shooter
#endif // INCLUDE_RENDER_PIPELINE_CORE
    }
}
