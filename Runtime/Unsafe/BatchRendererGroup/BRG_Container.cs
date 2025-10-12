#if INCLUDE_RENDER_PIPELINE_CORE
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

#if INCLUDE_MATHEMATICS
using Unity.Mathematics;
#else
using PKGE.Mathematics;
using float4 = UnityEngine.Vector4;
#endif // INCLUDE_MATHEMATICS

namespace PKGE.Unsafe
{
    /// <summary>
    /// This class handle rendering of ground cells & debris using BRG.
    /// Both ground cells & debris could be rendered using the same GPU data layout:
    /// - obj2world matrix (3 * float4)
    /// - world2obj matrix (3 * float4)
    /// - color (1 * float4)
    /// so 7 float4 per mesh.
    /// </summary>
    /// <remarks>Do not forget data is stored in SoA.</remarks>
    [BurstCompile]
    public class BRG_Container
    {
        //https://github.com/Unity-Technologies/brg-shooter/blob/f55f6e985bf73b0a3c23b95030e890874e552c45/Assets/Scripts/BRG_Container.cs
        #region brg-shooter
        /// <summary>In GLES mode, BRG raw buffer is a constant buffer (UBO) </summary>
        private bool UseConstantBuffer => BatchRendererGroup.BufferTarget == BatchBufferTarget.ConstantBuffer;
        private bool m_castShadows;

        /// <summary>Maximum items in this container</summary>
        private int m_maxInstances;
        /// <summary>Current item count</summary>
        private int m_instanceCount;
        /// <summary>BRG raw window size</summary>
        private int m_alignedGPUWindowSize;
        /// <summary>Max instance per window</summary>
        private int m_maxInstancePerWindow;
        /// <summary>Amount of windows (1 in SSBO mode, n in UBO mode)</summary>
        private int m_windowCount;
        /// <summary>Total size of the raw buffer</summary>
        private int m_totalGpuBufferSize;
        /// <summary>System memory copy of the raw buffer</summary>
        private NativeArray<float4> m_sysmemBuffer;
        private bool m_initialized;
        /// <summary>Item size, in bytes</summary>
        private int m_instanceSize;
        /// <summary>One batchID per window</summary>
        private NativeArray<BatchID> m_batchIDs;
        private BatchMaterialID m_materialID;
        private BatchMeshID m_meshID;
        /// <summary>BRG object</summary>
        private BatchRendererGroup m_BatchRendererGroup;
        /// <summary>GPU raw buffer (could be SSBO or UBO)</summary>
        private GraphicsBuffer m_GPUPersistentInstanceData;

        /// <summary>
        /// Create a BRG object and allocate buffers.
        /// </summary>
        public bool Init(Mesh mesh, Material mat, int maxInstances, int instanceSize, bool castShadows)
        {
            // Create the BRG object, specifying the BRG callback
            m_BatchRendererGroup = new BatchRendererGroup(OnPerformCulling, IntPtr.Zero);

            m_instanceSize = instanceSize;
            m_instanceCount = 0;
            m_maxInstances = maxInstances;
            m_castShadows = castShadows;

            // BRG uses a large GPU buffer.
            // This is a RAW buffer on almost all platforms, and a constant buffer on GLES.
            // In case of constant buffer, we split it into several "windows"
            // of BatchRendererGroup.GetConstantBufferMaxWindowSize() bytes each
            if (UseConstantBuffer)
            {
                m_alignedGPUWindowSize = BatchRendererGroup.GetConstantBufferMaxWindowSize();
                m_maxInstancePerWindow = m_alignedGPUWindowSize / instanceSize;
                m_windowCount = (m_maxInstances + m_maxInstancePerWindow - 1) / m_maxInstancePerWindow;
                m_totalGpuBufferSize = m_windowCount * m_alignedGPUWindowSize;
                m_GPUPersistentInstanceData =
                    new GraphicsBuffer(GraphicsBuffer.Target.Constant, m_totalGpuBufferSize / 16, 16);
            }
            else
            {
                m_alignedGPUWindowSize = (m_maxInstances * instanceSize + 15) & (-16);
                m_maxInstancePerWindow = maxInstances;
                m_windowCount = 1;
                m_totalGpuBufferSize = m_windowCount * m_alignedGPUWindowSize;
                m_GPUPersistentInstanceData =
                    new GraphicsBuffer(GraphicsBuffer.Target.Raw, m_totalGpuBufferSize / 4, 4);
            }

            // In this sample there are 3 instanced properties: obj2world, world2obj and baseColor
            var batchMetadata = new NativeArray<MetadataValue>(3, Allocator.Temp,
                NativeArrayOptions.UninitializedMemory);

            // Batch metadata buffer
            int objectToWorldID = Shader.PropertyToID("unity_ObjectToWorld");
            int worldToObjectID = Shader.PropertyToID("unity_WorldToObject");
            int colorID = Shader.PropertyToID("_BaseColor");

            // Create system memory copy of big GPU raw buffer
            m_sysmemBuffer = new NativeArray<float4>(m_totalGpuBufferSize / 16, Allocator.Persistent,
                NativeArrayOptions.ClearMemory);

            // Register one kind of batch per "window" in the large BRG raw buffer
            m_batchIDs = new NativeArray<BatchID>(m_windowCount, Allocator.Persistent,
                NativeArrayOptions.UninitializedMemory);
            for (int b = 0; b < m_windowCount; b++)
            {
                batchMetadata[0] = CreateMetadataValue(objectToWorldID, 0, true); // matrices
                batchMetadata[1] = CreateMetadataValue(worldToObjectID, m_maxInstancePerWindow * 3 * 16, true); // inverse matrices
                batchMetadata[2] = CreateMetadataValue(colorID, m_maxInstancePerWindow * 3 * 2 * 16, true); // colors
                int offset = b * m_alignedGPUWindowSize;
                m_batchIDs[b] = m_BatchRendererGroup.AddBatch(batchMetadata, m_GPUPersistentInstanceData.bufferHandle,
                    (uint)offset, UseConstantBuffer ? (uint)m_alignedGPUWindowSize : 0);
            }

            // This metadata description array is no longer required
            batchMetadata.Dispose();

            // Setup very large bounds to ensure BRG is never culled
            m_BatchRendererGroup.SetGlobalBounds(new Bounds(default, new Vector3(1048576.0f, 1048576.0f, 1048576.0f)));

            // Register mesh and material
            if (mesh != null)
                m_meshID = m_BatchRendererGroup.RegisterMesh(mesh);
            if (mat != null)
                m_materialID = m_BatchRendererGroup.RegisterMaterial(mat);

            m_initialized = true;
            return true;
        }

        /// <summary>
        /// Upload minimal GPU data according to "instanceCount".
        /// Because of SoA and this class is managing 3 BRG properties
        /// (2 matrices & 1 color), the last window could use up to 3 SetData.
        /// </summary>
        [BurstCompile]
        public bool UploadGpuData(int instanceCount)
        {
            if ((uint)instanceCount > (uint)m_maxInstances)
                return false;

            m_instanceCount = instanceCount;
            int completeWindows = m_instanceCount / m_maxInstancePerWindow;

            // Update all complete windows in one go
            if (completeWindows > 0)
            {
                int sizeInFloat4 = (completeWindows * m_alignedGPUWindowSize) / 16;
                m_GPUPersistentInstanceData.SetData(m_sysmemBuffer, 0, 0, sizeInFloat4);
            }

            // Then upload data for the last (incomplete) window
            int lastBatchId = completeWindows;
            int itemInLastBatch = m_instanceCount - m_maxInstancePerWindow * completeWindows;

            if (itemInLastBatch > 0)
            {
                int windowOffsetInFloat4 = (lastBatchId * m_alignedGPUWindowSize) / 16;
                int offsetMat1 = windowOffsetInFloat4; // + m_maxInstancePerWindow * 0;
                int offsetMat2 = windowOffsetInFloat4 + m_maxInstancePerWindow * 3;
                int offsetColor = windowOffsetInFloat4 + m_maxInstancePerWindow * 3 * 2;
                m_GPUPersistentInstanceData.SetData(m_sysmemBuffer, offsetMat1, offsetMat1,
                    itemInLastBatch * 3); // 3 float4 for obj2world
                m_GPUPersistentInstanceData.SetData(m_sysmemBuffer, offsetMat2, offsetMat2,
                    itemInLastBatch * 3); // 3 float4 for world2obj
                m_GPUPersistentInstanceData.SetData(m_sysmemBuffer, offsetColor, offsetColor,
                    itemInLastBatch * 1); // 1 float4 for color
            }

            return true;
        }

        /// <summary>
        /// Release all allocated buffers
        /// </summary>
        public void Shutdown()
        {
            if (m_initialized)
            {
                for (int b = 0; b < m_windowCount; b++)
                    m_BatchRendererGroup.RemoveBatch(m_batchIDs[b]);
                
                m_batchIDs.Dispose();

                m_BatchRendererGroup.UnregisterMaterial(m_materialID);
                m_BatchRendererGroup.UnregisterMesh(m_meshID);
                m_BatchRendererGroup.Dispose();
                m_GPUPersistentInstanceData.Dispose();
                m_sysmemBuffer.Dispose();
            }
            
            m_initialized = false;
        }

        /// <summary>
        /// Return the system memory buffer and the window size,
        /// so BRG_Background and BRG_Debris can fill the buffer with new content.
        /// </summary>
        public NativeArray<float4> GetSysmemBuffer(out int totalSize, out int alignedWindowSize)
        {
            totalSize = m_totalGpuBufferSize;
            alignedWindowSize = m_alignedGPUWindowSize;
            return m_sysmemBuffer;
        }

        /// <summary>
        /// helper function to create the 32bits metadata value.
        /// Bit 31 means properties have a different value per instance.
        /// </summary>
        static MetadataValue CreateMetadataValue(int nameID, int gpuOffset, bool isPerInstance)
        {
            const uint kIsPerInstanceBit = 0x80000000;
            return new MetadataValue
            {
                NameID = nameID,
                Value = (uint)gpuOffset | (isPerInstance ? (kIsPerInstanceBit) : 0),
            };
        }

        /// <summary>
        /// Main BRG entry point per frame. In this sample we won't use BatchCullingContext as we don't need culling.
        /// This callback is responsible to fill cullingOutput with all draw commands we need to render all the items.
        /// </summary>
        private unsafe JobHandle OnPerformCulling(BatchRendererGroup rendererGroup, BatchCullingContext cullingContext,
            BatchCullingOutput cullingOutput, IntPtr userContext)
        {
            if (!m_initialized)
                return new JobHandle();

            var drawCommands = new BatchCullingOutputDrawCommands();

            // Calculate the amount of draw commands required in case of UBO mode (one draw command per window)
            int drawCommandCount = (m_instanceCount + m_maxInstancePerWindow - 1) / m_maxInstancePerWindow;
            int maxInstancePerDrawCommand = m_maxInstancePerWindow;
            drawCommands.drawCommandCount = drawCommandCount;

            // Allocate a single BatchDrawRange. All draw commands will refer to this BatchDrawRange.
            drawCommands.drawRangeCount = 1;
            //drawCommands.drawRanges = Malloc<BatchDrawRange>(1);
            drawCommands.drawRanges = (BatchDrawRange*)new NativeArray<BatchDrawRange>(1,
                Allocator.TempJob, NativeArrayOptions.UninitializedMemory).GetUnsafePtr();
            drawCommands.drawRanges[0] = new BatchDrawRange
            {
                drawCommandsBegin = 0,
                drawCommandsCount = (uint)drawCommandCount,
                filterSettings = new BatchFilterSettings
                {
                    renderingLayerMask = 1,
                    layer = 0,
                    motionMode = MotionVectorGenerationMode.Camera,
                    shadowCastingMode = m_castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off,
                    receiveShadows = true,
                    staticShadowCaster = false,
                    allDepthSorted = false
                }
            };

            if (drawCommands.drawCommandCount > 0)
            {
                // As culling isn't required, the visibility int array buffer will always be {0,1,2,3,...} for each draw command.
                // Allocate maxInstancePerDrawCommand and fill it.
                int visibilityArraySize = maxInstancePerDrawCommand;
                if (m_instanceCount < visibilityArraySize)
                    visibilityArraySize = m_instanceCount;

                //drawCommands.visibleInstances = Malloc<int>((uint)visibilityArraySize);
                drawCommands.visibleInstances = (int*)new NativeArray<int>(visibilityArraySize,
                    Allocator.TempJob, NativeArrayOptions.UninitializedMemory).GetUnsafePtr();

                // As frustum culling isn't required in this context, fill the visibility array with {0,1,2,3,...}
                for (int i = 0; i < visibilityArraySize; i++)
                    drawCommands.visibleInstances[i] = i;

                // Allocate the BatchDrawCommand array (drawCommandCount entries)
                // In SSBO mode, drawCommandCount will be 1
                //drawCommands.drawCommands = Malloc<BatchDrawCommand>((uint)drawCommandCount);
                drawCommands.drawCommands = (BatchDrawCommand*)new NativeArray<BatchDrawCommand>(drawCommandCount,
                    Allocator.TempJob, NativeArrayOptions.UninitializedMemory).GetUnsafePtr();
                int left = m_instanceCount;
                for (int b = 0; b < drawCommandCount; b++)
                {
                    int inBatchCount = left > maxInstancePerDrawCommand ? maxInstancePerDrawCommand : left;
                    drawCommands.drawCommands[b] = new BatchDrawCommand
                    {
                        visibleOffset = 0, // all draw command is using the same {0,1,2,3...} visibility int array
                        visibleCount = (uint)inBatchCount,
                        batchID = m_batchIDs[b],
                        materialID = m_materialID,
                        meshID = m_meshID,
                        submeshIndex = 0,
                        splitVisibilityMask = 0xff,
                        flags = BatchDrawCommandFlags.None,
                        sortingPosition = 0
                    };
                    left -= inBatchCount;
                }
            }

            cullingOutput.drawCommands[0] = drawCommands;
            drawCommands.instanceSortingPositions = null;
            drawCommands.instanceSortingPositionFloatCount = 0;

            return new JobHandle();
        }
        #endregion // brg-shooter
    }
}
#endif // INCLUDE_RENDER_PIPELINE_CORE
