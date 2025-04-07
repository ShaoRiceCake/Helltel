using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


namespace Obi
{
    public class ObiFluidBuiltInRendererFeature : MonoBehaviour
	{

        public ObiFluidRenderingPass[] passes;

        [Range(1, 4)]
        public int refractionDownsample = 1;

        [Range(1, 4)]
        public int thicknessDownsample = 1;

        [Min(0)]
        public float foamFadeDepth = 1.0f;

        protected Dictionary<Camera, CommandBuffer> cmdBuffers = new Dictionary<Camera, CommandBuffer>();

        private FluidRenderingUtils.FluidRenderTargets renderTargets;
        private Material m_TransmissionMaterial;

        protected Material CreateMaterial(Shader shader)
        {
            if (!shader || !shader.isSupported)
                return null;
            Material m = new Material(shader);
            m.hideFlags = HideFlags.HideAndDontSave;
            return m;
        }

        public void OnEnable()
        {
            Setup();
            Camera.onPreRender += SetupFluidRendering;
        }

        public void OnDisable()
        {
            Camera.onPreRender -= SetupFluidRendering;
            Cleanup();
        }

        protected void Setup()
        {

            if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
            {
                enabled = false;
                Debug.LogWarning("Obi Fluid Renderer not supported in this platform.");
                return;
            }

            m_TransmissionMaterial = CreateMaterial(Shader.Find("Hidden/AccumulateTransmission"));

            renderTargets = new FluidRenderingUtils.FluidRenderTargets();
            renderTargets.transmission = Shader.PropertyToID("_FluidThickness");
            renderTargets.refraction = Shader.PropertyToID("_CameraOpaqueTexture");
            renderTargets.foam = Shader.PropertyToID("_Foam");
            renderTargets.surface = Shader.PropertyToID("_TemporaryBuffer");
        }

		protected void Cleanup()
		{
            if (m_TransmissionMaterial != null)
                DestroyImmediate(m_TransmissionMaterial);

            foreach (var entry in cmdBuffers)
                if (entry.Key != null)
                    entry.Key.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, entry.Value);

            cmdBuffers.Clear();
        }

        private void SetupFluidRendering(Camera cam)
        {
            if (cmdBuffers.TryGetValue(cam, out CommandBuffer cmdBuffer))
            {
                UpdateFluidRenderingCommandBuffer(cam, cmdBuffer);
            }
            else
            {
                cmdBuffer = new CommandBuffer();
                cmdBuffer.name = "Render fluid";
                cmdBuffers[cam] = cmdBuffer;

                cam.forceIntoRenderTexture = true;
                cam.depthTextureMode |= DepthTextureMode.Depth;
                UpdateFluidRenderingCommandBuffer(cam, cmdBuffer);
                cam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cmdBuffer);
            }
        }

        public void UpdateFluidRenderingCommandBuffer(Camera cam, CommandBuffer cmd)
		{
			cmd.Clear();
	
			if (passes == null)
				return;

            // grab opaque contents:
            cmd.GetTemporaryRT(renderTargets.refraction, -refractionDownsample, -refractionDownsample, 0, FilterMode.Bilinear);
            cmd.Blit(BuiltinRenderTextureType.CameraTarget, renderTargets.refraction);

            // get temporary buffer with depth support:
            cmd.GetTemporaryRT(renderTargets.surface, -thicknessDownsample, -thicknessDownsample, 16, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);

            // get transmission buffer, color only:
            cmd.GetTemporaryRT(renderTargets.transmission, -thicknessDownsample, -thicknessDownsample, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
            cmd.SetRenderTarget(renderTargets.transmission);
            cmd.ClearRenderTarget(false, true, FluidRenderingUtils.transmissionBufferClear);

            // render each pass (there's only one mesh per pass) onto temp buffer to calculate its color and thickness.
            for (int i = 0; i < passes.Length; ++i)
            {
                if (passes[i] != null && passes[i].renderers.Count > 0)
                {
                    var fluidMesher = passes[i].renderers[0];
                    if (fluidMesher.actor.isLoaded)
                    {
                        cmd.SetRenderTarget(renderTargets.surface);
                        cmd.ClearRenderTarget(false, true, FluidRenderingUtils.thicknessBufferClear);

                        // fluid mesh renders absorption color and thickness onto temp buffer:
                        var renderSystem = fluidMesher.actor.solver.GetRenderSystem<ObiFluidSurfaceMesher>() as IFluidRenderSystem;
                        if (renderSystem != null)
                            renderSystem.RenderVolume(cmd, passes[i], fluidMesher);

                        // calculate transmission from thickness & absorption and accumulate onto transmission buffer.
                        cmd.SetGlobalFloat("_Thickness", passes[i].thickness);
                        cmd.Blit(renderTargets.surface, renderTargets.transmission, m_TransmissionMaterial, 0);
                    }
                }
            }

            // render fluid surface depth:
            cmd.SetRenderTarget(renderTargets.surface);
            cmd.ClearRenderTarget(true, true, Color.clear);
            for (int i = 0; i < passes.Length; ++i)
            {
                if (passes[i] != null && passes[i].renderers.Count > 0)
                {
                    var fluidMesher = passes[i].renderers[0];
                    if (fluidMesher.actor.isLoaded)
                    {
                        // fluid mesh renders surface onto surface buffer
                        var renderSystem = fluidMesher.actor.solver.GetRenderSystem<ObiFluidSurfaceMesher>() as IFluidRenderSystem;
                        if (renderSystem != null)
                            renderSystem.RenderSurface(cmd, passes[i], fluidMesher);
                    }
                }
            }

            // render foam, using distance to surface depth to modulate alpha:
            cmd.GetTemporaryRT(renderTargets.foam, -refractionDownsample, -refractionDownsample, 0, FilterMode.Bilinear);
            cmd.SetRenderTarget(renderTargets.foam);
            cmd.ClearRenderTarget(false, true, Color.clear);
            for (int i = 0; i < passes.Length; ++i)
            {
                for (int j = 0; j < passes[i].renderers.Count; ++j)
                {
                    if (passes[i].renderers[j].TryGetComponent(out ObiFoamGenerator foamGenerator))
                    {
                        var solver = passes[i].renderers[j].actor.solver;
                        if (solver != null)
                        {
                            var rend = solver.GetRenderSystem<ObiFoamGenerator>() as ObiFoamRenderSystem;

                            if (rend != null)
                            {
                                rend.renderBatch.material.SetFloat("_FadeDepth", foamFadeDepth);
                                rend.renderBatch.material.SetFloat("_VelocityStretching", solver.maxFoamVelocityStretch);
                                rend.renderBatch.material.SetFloat("_FadeIn", solver.foamFade.x);
                                rend.renderBatch.material.SetFloat("_FadeOut", solver.foamFade.y);
                                cmd.DrawMesh(rend.renderBatch.mesh, solver.transform.localToWorldMatrix, rend.renderBatch.material);
                            }
                        }
                    }
                }
            }
        }
    }
}

