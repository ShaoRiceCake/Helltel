using UnityEngine;
using UnityEngine.Rendering;

namespace Obi
{
    public interface IFluidRenderSystem
    {
        void RenderVolume(CommandBuffer cmd, ObiFluidRenderingPass pass, ObiFluidSurfaceMesher renderer);
        void RenderSurface(CommandBuffer cmd, ObiFluidRenderingPass pass, ObiFluidSurfaceMesher renderer);
        void BakeMesh(ObiFluidSurfaceMesher renderer, ref Mesh mesh);
    }
}

