using UnityEngine;
using System;

namespace Obi
{
    [CreateAssetMenu(fileName = "fluid rendering pass", menuName = "Obi/Fluid Rendering Pass", order = 182)]
    public class ObiFluidRenderingPass : ScriptableObject
    {
        public enum MaterialType
        {
            Transparent,
            Opaque,
            Custom
        }


        [Header("Mesh generation")]
        [Delayed]
        public float voxelSize = 0.1f;

        [Range(0.001f, 1)]
        public float isosurface = 0.05f;

        [Range(0, 8)]
        public uint descentIterations = 4;
        [Range(0, 0.5f)]
        public float descentIsosurface = 0.1f;
        [Range(0, 1)]
        public float descentSpeed = 0.8f;

        [Range(0, 16)]
        public uint smoothingIterations = 2;
        [Range(0, 1)]
        public float smoothingIntensity = 1;

        [InspectorName("Bevel (2D only)")]
        [Range(0, 1)]
        public float bevel = 1f;

        [Header("Rendering")]

        public MaterialType materialType = MaterialType.Opaque;
        public Material fluidMaterial { get; private set; }
        public Material underwaterMaterial { get; private set; }
        public bool usesCustomMaterial => materialType == MaterialType.Custom;
        public bool usesOpaqueMaterial => materialType == MaterialType.Opaque;
        public bool usesTransparentMaterial => materialType == MaterialType.Transparent;

        [VisibleIf("usesCustomMaterial")]
        public Material customFluidMaterial = null;

        [VisibleIf("usesCustomMaterial")]
        public Material customUnderwaterMaterial = null;

        [VisibleIf("usesTransparentMaterial")]
        public Cubemap reflectionCubemap = null;

        [VisibleIf("usesCustomMaterial", true)]
        [MultiRange(0, 1)]
        public float smoothness = 0.9f;

        [VisibleIf("usesOpaqueMaterial")]
        [MultiRange(0, 1)]
        public float metallic = 0f;

        [VisibleIf("usesTransparentMaterial")]
        public Color turbidity = Color.clear;

        [VisibleIf("usesTransparentMaterial")]
        public float thickness = 1f;

        [VisibleIf("usesTransparentMaterial")]
        public float indexOfRefraction = 1.33f;

        [VisibleIf("usesCustomMaterial", true)]
        public Texture2D diffuseMap = null;

        [VisibleIf("usesTransparentMaterial")]
        public Color diffuseColor = Color.white;

        [VisibleIf("usesCustomMaterial", true)]
        public Texture2D normalMap = null;

        [VisibleIf("usesCustomMaterial", true)]
        [MultiRange(0, 1)]
        public float normalMapIntensity = 1;

        [VisibleIf("usesCustomMaterial", true)]
        public Vector2 normalMapVelocityRange = new Vector2(10,0);

        [VisibleIf("usesCustomMaterial", true)]
        public float tiling = 1;

        [VisibleIf("usesCustomMaterial", true)]
        public Texture2D noiseMap = null;

        [VisibleIf("usesCustomMaterial", true)]
        [MultiRange(0, 1)]
        public float noiseMapIntensity = 0.1f;

        [VisibleIf("usesCustomMaterial", true)]
        public float noiseMapTiling = 0.1f;

        [VisibleIf("usesCustomMaterial", true)]
        public float advectTimescale = 1;

        [VisibleIf("usesCustomMaterial", true)]
        public Vector4 advectJump = Vector4.zero;

        [VisibleIf("usesCustomMaterial", true)]
        [MultiRange(-0.5f, 0)]
        public float advectOffset = -0.5f;

        [VisibleIf("usesCustomMaterial", true)]
        [MultiRange(0, 128)]
        public float triplanarBlend = 16;

        [VisibleIf("usesOpaqueMaterial", true)]
        public bool underwaterRendering = false;


        public RenderBatchParams renderParameters = new RenderBatchParams(true);

        [LayerField]
        public int layer;

        [NonSerialized] public RendererSet<ObiFluidSurfaceMesher> renderers;

        public void OnEnable()
        {
            if (renderers == null)
                renderers = new RendererSet<ObiFluidSurfaceMesher>();
        }

        public void AddRenderer(ObiFluidSurfaceMesher renderer)
        {
            if (renderers == null)
                renderers = new RendererSet<ObiFluidSurfaceMesher>();

            renderers.AddRenderer(renderer);
        }
        public void RemoveRenderer(ObiFluidSurfaceMesher renderer)
        {
            if (renderers != null)
            {
                renderers.RemoveRenderer(renderer);
                if (renderers.Count == 0)                    DisposeOfFluidMaterial();
            }
        }

        public void DisposeOfFluidMaterial()
        {
            if (fluidMaterial != null)
            {
                DestroyImmediate(fluidMaterial);
                fluidMaterial = null;
            }
        }

        private void OnValidate()
        {
            indexOfRefraction = Mathf.Max(0, indexOfRefraction);
            voxelSize = Mathf.Max(0.005f, voxelSize);
            UpdateRenderers();
        }

        public void UpdateRenderers()
        {
            if (renderers != null)
                for (int i = 0; i < renderers.Count; ++i)
                    renderers[i].actor.SetRenderingDirty(Oni.RenderingSystemType.Fluid);
        }

        // called when setting up mesher system.
        public Material UpdateFluidMaterial(bool compute)
        {
            DisposeOfFluidMaterial();

            switch (materialType)
            {
                case MaterialType.Transparent:
                    {
                        fluidMaterial = compute ? Instantiate(Resources.Load<Material>("ObiMaterials/Fluid/Compute/IndirectFluidTransparent")) : Instantiate(Resources.Load<Material>("ObiMaterials/Fluid/Burst/BurstTransparent"));
                        underwaterMaterial = compute ? Instantiate(Resources.Load<Material>("ObiMaterials/Fluid/Compute/IndirectFluidUnderwater")) : Instantiate(Resources.Load<Material>("ObiMaterials/Fluid/Burst/BurstUnderwater"));
                    }
                    break;
                case MaterialType.Opaque:
                    {
                        fluidMaterial = compute ? Instantiate(Resources.Load<Material>("ObiMaterials/Fluid/Compute/IndirectFluidOpaque")) : Instantiate(Resources.Load<Material>("ObiMaterials/Fluid/Burst/BurstOpaque"));
                    }break;
                case MaterialType.Custom:
                    {
                        fluidMaterial = customFluidMaterial != null ? Instantiate(customFluidMaterial) : null;
                        underwaterMaterial = customUnderwaterMaterial != null ? Instantiate(customUnderwaterMaterial) : null;
                    }
                    break;
            }
            return fluidMaterial;
        }
    }
}
