/*
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineDissolveRenderFeature : ScriptableRendererFeature
{
    class OutlineDissolvePass : ScriptableRenderPass
    {
        private Material material;
        private RenderTargetIdentifier source;
        private RTHandle tempTexture;

        public OutlineDissolvePass(Material mat)
        {
            this.material = mat;
        }

        public void Setup(RenderTargetIdentifier source)
        {
            this.source = source;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("OutlineDissolvePass");

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            // 获取临时渲染纹理
            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, opaqueDesc, FilterMode.Bilinear);

            // 原始图像和蒙版合成并写进去临时RT
            cmd.Blit(source, new RenderTargetIdentifier(tempTexture.rt), material);
            // 再Blit到屏幕输出
            cmd.Blit(new RenderTargetIdentifier(tempTexture.rt), source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (cmd == null) return;
            // 释放临时渲染纹理
            RTHandles.Release(tempTexture);
        }
    }

    [System.Serializable]
    public class OutlineDissolveSettings
    {
        public Material dissolveMaterial;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public OutlineDissolveSettings settings = new OutlineDissolveSettings();
    OutlineDissolvePass pass;

    public override void Create()
    {
        if (settings.dissolveMaterial == null)
        {
            Debug.LogError("Missing dissolve material in OutlineDissolveRenderFeature");
            return;
        }
        pass = new OutlineDissolvePass(settings.dissolveMaterial)
        {
            renderPassEvent = settings.renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.dissolveMaterial == null) return;
        RenderTargetIdentifier customTarget = Resources.Load<RenderTexture>("Render Tex/New Render Texture");
        pass.Setup(customTarget);
        renderer.EnqueuePass(pass);
    }
}    
*/