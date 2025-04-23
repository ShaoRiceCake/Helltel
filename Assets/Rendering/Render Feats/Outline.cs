using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ItemOutlineFeature : ScriptableRendererFeature
{
    [SerializeField, Tooltip("Layer mask for objects to outline (User Layer 11 = 'Item')")]
    private LayerMask m_LayerMask = 1 << 11;

    [SerializeField] private Material m_OutlineMaterial;
    private OutlineRenderPass m_OutlineRenderPass;

    private bool IsMaterialValid => m_OutlineMaterial && m_OutlineMaterial.shader && m_OutlineMaterial.shader.isSupported;

    public override void Create()
    {
        if (!IsMaterialValid)
            return;

        m_OutlineRenderPass = new OutlineRenderPass(m_OutlineMaterial, m_LayerMask);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (m_OutlineRenderPass == null)
            return;

        renderer.EnqueuePass(m_OutlineRenderPass);
    }

    protected override void Dispose(bool disposing)
    {
        m_OutlineRenderPass?.Dispose();
    }

    class OutlineRenderPass : ScriptableRenderPass
    {
        private static readonly List<ShaderTagId> s_shaderTagIds = new List<ShaderTagId>(){
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
        };

        private static readonly int s_ShaderProp_OutlineMask = Shader.PropertyToID("_OutlineMask");

        private readonly Material m_OutlineMaterial;
        private readonly FilteringSettings m_FilteringSettings;
        private readonly MaterialPropertyBlock m_PropertyBlock;
        private RTHandle m_OutlineMaskRT;

        public OutlineRenderPass(Material outlineMaterial, LayerMask layerMask)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

            m_OutlineMaterial = outlineMaterial;
            // 改用物体图层
            m_FilteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);
            m_PropertyBlock = new MaterialPropertyBlock();
        }

        public void Dispose()
        {
            m_OutlineMaskRT?.Release();
            m_OutlineMaskRT = null;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ResetTarget();
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.msaaSamples = 1;
            desc.depthBufferBits = 0;
            desc.colorFormat = RenderTextureFormat.ARGB32;
            RenderingUtils.ReAllocateIfNeeded(ref m_OutlineMaskRT, desc, name: "_OutlineMaskRT");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("Outline Command");

            cmd.SetRenderTarget(m_OutlineMaskRT);
            cmd.ClearRenderTarget(true, true, Color.clear);

            var drawingSettings = CreateDrawingSettings(s_shaderTagIds, ref renderingData, SortingCriteria.None);
            var rendererListParams = new RendererListParams(renderingData.cullResults, drawingSettings, m_FilteringSettings);
            var list = context.CreateRendererList(ref rendererListParams);
            cmd.DrawRendererList(list);

            cmd.SetRenderTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
            m_PropertyBlock.SetTexture(s_ShaderProp_OutlineMask, m_OutlineMaskRT);
            cmd.DrawProcedural(Matrix4x4.identity, m_OutlineMaterial, 0, MeshTopology.Triangles, 3, 1, m_PropertyBlock);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}
