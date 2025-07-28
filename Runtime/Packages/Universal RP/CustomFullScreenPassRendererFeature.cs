#if URP_14
using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityExtensions.Packages
{
    using CoreUtils = UnityEngine.Rendering.CoreUtils;
    using ProfilingSampler = UnityEngine.Rendering.ProfilingSampler;
    using ProfilingScope = UnityEngine.Rendering.ProfilingScope;

    /// <inheritdoc/>
    public class CustomFullScreenPassRendererFeature : FullScreenPassRendererFeature
    {
        /// <inheritdoc cref="FullScreenPassRendererFeature.InjectionPoint"/>
        public RenderPassEvent injectionPass;

        private FullScreenRenderPass m_FullScreenPass;

        /// <inheritdoc/>
        public override void Create()
        {
            m_FullScreenPass = new FullScreenRenderPass(name);
        }

        /// <inheritdoc/>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (UniversalRenderer.IsOffscreenDepthTexture(in renderingData.cameraData) || renderingData.cameraData.cameraType == CameraType.Preview || renderingData.cameraData.cameraType == CameraType.Reflection)
                return;

            if (passMaterial == null)
            {
                Debug.LogWarningFormat("The full screen feature \"{0}\" will not execute - no material is assigned. Please make sure a material is assigned for this feature on the renderer asset.", name);
                return;
            }

            if (passIndex < 0 || passIndex >= passMaterial.passCount)
            {
                Debug.LogWarningFormat("The full screen feature \"{0}\" will not execute - the pass index is out of bounds for the material.", name);
                return;
            }

            m_FullScreenPass.renderPassEvent = injectionPass;
            m_FullScreenPass.ConfigureInput(requirements);
            m_FullScreenPass.SetupMembers(passMaterial, passIndex, fetchColorBuffer, bindDepthStencilAttachment);

            renderer.EnqueuePass(m_FullScreenPass);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            m_FullScreenPass.Dispose();
        }

        static class ShaderPropertyId
        {
            public static readonly int blitTexture = Shader.PropertyToID("_BlitTexture");
            public static readonly int blitScaleBias = Shader.PropertyToID("_BlitScaleBias");
        }

        internal class FullScreenRenderPass : ScriptableRenderPass
        {
            private Material m_Material;
            private int m_PassIndex;
            private bool m_CopyActiveColor;
            private bool m_BindDepthStencilAttachment;
            private RTHandle m_CopiedColor;

            private static MaterialPropertyBlock s_SharedPropertyBlock = new MaterialPropertyBlock();

#if CUSTOM_URP
#else
            private static readonly ParameterExpression param = Expression.Parameter(typeof(object), "instance");

            private static readonly Func<object, CommandBuffer> getCommandBufferDelegate =
                Expression.Lambda<Func<object, CommandBuffer>>(Expression.Convert(
                    Expression.Field(Expression.Convert(param, typeof(RenderingData)),
                    typeof(RenderingData).GetField("commandBuffer", BindingFlags.Instance | BindingFlags.NonPublic)),
                    typeof(CommandBuffer)), param).Compile();
#endif // CUSTOM_URP

            public FullScreenRenderPass(string passName)
            {
                profilingSampler = new ProfilingSampler(passName);
            }

            public void SetupMembers(Material material, int passIndex, bool copyActiveColor, bool bindDepthStencilAttachment)
            {
                m_Material = material;
                m_PassIndex = passIndex;
                m_CopyActiveColor = copyActiveColor;
                m_BindDepthStencilAttachment = bindDepthStencilAttachment;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                // FullScreenPass manages its own RenderTarget.
                // ResetTarget here so that ScriptableRenderer's active attachement can be invalidated when processing this ScriptableRenderPass.
                ResetTarget();

                if (m_CopyActiveColor)
                    ReAllocate(renderingData.cameraData.cameraTargetDescriptor);
            }

            internal void ReAllocate(RenderTextureDescriptor desc)
            {
                desc.msaaSamples = 1;
                const int depthBufferBits = (int)DepthBits.None;
                desc.depthBufferBits = depthBufferBits;
                RenderingUtils.ReAllocateIfNeeded(ref m_CopiedColor, desc, name: "_FullscreenPassColorCopy");
            }

            public void Dispose()
            {
                m_CopiedColor?.Release();
            }

            private static void ExecuteCopyColorPass(CommandBuffer cmd, RTHandle sourceTexture)
            {
                Blitter.BlitTexture(cmd, sourceTexture, new Vector4(1, 1, 0, 0), 0.0f, false);
            }

            private static void ExecuteMainPass(CommandBuffer cmd, RTHandle sourceTexture, Material material, int passIndex)
            {
                s_SharedPropertyBlock.Clear();
                if(sourceTexture != null)
                    s_SharedPropertyBlock.SetTexture(ShaderPropertyId.blitTexture, sourceTexture);

                // We need to set the "_BlitScaleBias" uniform for user materials with shaders relying on core Blit.hlsl to work
                s_SharedPropertyBlock.SetVector(ShaderPropertyId.blitScaleBias, new Vector4(1, 1, 0, 0));

                cmd.DrawProcedural(Matrix4x4.identity, material, passIndex, MeshTopology.Triangles, 3, 1, s_SharedPropertyBlock);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                ref var cameraData = ref renderingData.cameraData;
#if CUSTOM_URP
                var cmd = renderingData.cmd;
#else
                var cmd = getCommandBufferDelegate(renderingData);
#endif // CUSTOM_URP

                using (new ProfilingScope(cmd, profilingSampler))
                {
                    if (m_CopyActiveColor)
                    {
                        CoreUtils.SetRenderTarget(cmd, m_CopiedColor);
                        ExecuteCopyColorPass(cmd, cameraData.renderer.cameraColorTargetHandle);
                    }

                    if(m_BindDepthStencilAttachment)
                        CoreUtils.SetRenderTarget(cmd, cameraData.renderer.cameraColorTargetHandle, cameraData.renderer.cameraDepthTargetHandle);
                    else
                        CoreUtils.SetRenderTarget(cmd, cameraData.renderer.cameraColorTargetHandle);

                    ExecuteMainPass(cmd, m_CopyActiveColor ? m_CopiedColor : null, m_Material, m_PassIndex);
                }
            }
        }
    }
}
#endif // URP_14
