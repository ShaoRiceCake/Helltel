using UnityEngine;
using UnityEngine.Rendering;

namespace Obi
{

    [AddComponentMenu("Physics/Obi/Obi Fluid Surface Mesher", 1000)]
    [ExecuteInEditMode]
    [RequireComponent(typeof(ObiActor))]
    public class ObiFluidSurfaceMesher : MonoBehaviour, ObiActorRenderer<ObiFluidSurfaceMesher>
    {
        public ObiFluidRenderingPass pass;

        public ObiActor actor { get; private set; }

        public void Awake()
        {
            actor = GetComponent<ObiActor>();
        }

        public void OnEnable()
        {
            ((ObiActorRenderer<ObiFluidSurfaceMesher>)this).EnableRenderer();

            if (pass != null)
                pass.AddRenderer(this);
        }

        public void OnDisable()
        {
            if (pass != null)
                pass.RemoveRenderer(this);

            ((ObiActorRenderer<ObiFluidSurfaceMesher>)this).DisableRenderer();
        }

        public void OnValidate()
        {
            ((ObiActorRenderer<ObiFluidSurfaceMesher>)this).SetRendererDirty(Oni.RenderingSystemType.Fluid);
        }

        RenderSystem<ObiFluidSurfaceMesher> ObiRenderer<ObiFluidSurfaceMesher>.CreateRenderSystem(ObiSolver solver)
        {
            switch (solver.backendType)
            {

#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
                case ObiSolver.BackendType.Burst: return new BurstFluidMesherSystem(solver);
#endif
                case ObiSolver.BackendType.Compute:
                default:

                    if (SystemInfo.supportsComputeShaders)
                        return new ComputeFluidMesherSystem(solver);
                    return null;
            }
        }
    }
}