using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using static UnityEngine.GraphicsBuffer;

#if (SRP_UNIVERSAL)
using UnityEngine.Rendering.Universal;
#endif

namespace Obi
{

    struct keyvalue
    {
        public uint key;
        public uint handle;
    }

    public class ComputeFluidMesherSystem : RenderSystem<ObiFluidSurfaceMesher>, IFluidRenderSystem, ISurfaceChunkUser
    {
        public Oni.RenderingSystemType typeEnum { get => Oni.RenderingSystemType.Fluid; }

        public RendererSet<ObiFluidSurfaceMesher> renderers { get; } = new RendererSet<ObiFluidSurfaceMesher>();
        public bool isSetup => activeParticles != null;

        static protected ProfilerMarker m_SetupRenderMarker = new ProfilerMarker("SetupSurfaceMeshing");
        static protected ProfilerMarker m_RenderMarker = new ProfilerMarker("SurfaceMeshing");
        static protected ProfilerMarker m_ChunkMarker = new ProfilerMarker("ChunkGeneration");
        static protected ProfilerMarker m_SDFMarker = new ProfilerMarker("BuildSDF");
        static protected ProfilerMarker m_SurfaceMarker = new ProfilerMarker("BuildSurface");
        static protected ProfilerMarker m_TriangulateMarker = new ProfilerMarker("Triangulation");
        static protected ProfilerMarker m_SmoothingMarker = new ProfilerMarker("LaplacianSmoothing");

        protected ObiSolver m_Solver;
        protected List<IndirectRenderBatch<Vector4>> batchList = new List<IndirectRenderBatch<Vector4>>();

        protected ObiNativeList<int> activeParticles;

        // chunks hashtable 
        protected GraphicsBuffer hashtable;
        protected GraphicsBuffer chunkCoords;

        // voxel data:
        protected GraphicsBuffer voxelToVertex;
        protected GraphicsBuffer trisDispatchBuffer;

        // geometry:
        protected GraphicsBuffer verts;

        // vertex/tri adjacency:
        protected GraphicsBuffer vertexAdjacency;

        // edge LUTs:
        protected GraphicsBuffer edges2D;
        protected GraphicsBuffer edges3D;
        protected GraphicsBuffer edgeTable2D;
        protected GraphicsBuffer edgeTable3D;

        private ComputeShader chunkShader;
        private int clearChunksKernel;
        private int clearGridKernel;
        private int insertKernel;
        private int sortKernel;
        private int gridPopulationKernel;

        private ComputeShader meshComputeShader;
        private int sdfKernel;
        private int surfaceKernel;
        private int triangulateKernel;
        private int smoothingKernel;

        private int fixArgsKernel;
        private int indirectDrawKernel;

        private const uint chunkResolution = 4; // amount of voxels in width/height/depth

        private uint[] clearDispatch = { 0, 1, 1, 0, 0 };
        private int batchCount;

        protected Material surface_Material;
        protected Material thickness_Material;
        protected LocalKeyword shader2DFeature;

        public uint usedChunkCount
        {
            get
            {
                uint max = 0;
                if (usedChunksPerBatch != null)
                {
                    for (int i = 0; i < usedChunksPerBatch.Length; ++i)
                        max = (uint)Mathf.Max(max, usedChunksPerBatch[i]);
                }
                return max;
            }
        }

        private uint[] usedChunksPerBatch;

        protected Material CreateMaterial(Shader shader)
        {
            if (!shader || !shader.isSupported)
                return null;
            Material m = new Material(shader);
            m.hideFlags = HideFlags.HideAndDontSave;
            return m;
        }

        public ComputeFluidMesherSystem(ObiSolver solver)
        {
            m_Solver = solver;

            if (surface_Material == null)
            {
#if (SRP_UNIVERSAL)
                if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset)
                    surface_Material = CreateMaterial(Shader.Find("Hidden/IndirectSurfaceURP"));
                else
#endif
                    surface_Material = CreateMaterial(Shader.Find("Hidden/IndirectSurface"));
            }

            if (thickness_Material == null)
            {
#if (SRP_UNIVERSAL)
                if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset)
                    thickness_Material = CreateMaterial(Shader.Find("Hidden/IndirectThicknessURP"));
                else
#endif
                    thickness_Material = CreateMaterial(Shader.Find("Hidden/IndirectThickness"));
            }

            shader2DFeature = new LocalKeyword(thickness_Material.shader, "MODE_2D");

            activeParticles = new ObiNativeList<int>();

            AllocateVoxels();

            trisDispatchBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 5, sizeof(uint));

            chunkShader = GameObject.Instantiate(Resources.Load<ComputeShader>("Compute/FluidMeshChunks"));
            clearChunksKernel = chunkShader.FindKernel("ClearChunks");
            clearGridKernel = chunkShader.FindKernel("ClearGrid");
            insertKernel = chunkShader.FindKernel("InsertChunks");
            sortKernel = chunkShader.FindKernel("SortParticles");
            gridPopulationKernel = chunkShader.FindKernel("FindPopulatedLevels");

            meshComputeShader = GameObject.Instantiate(Resources.Load<ComputeShader>("Compute/FluidSurfaceMeshBuilding"));
            sdfKernel = meshComputeShader.FindKernel("SampleSDF");
            surfaceKernel = meshComputeShader.FindKernel("CalculateSurface");
            triangulateKernel = meshComputeShader.FindKernel("Triangulate");
            smoothingKernel = meshComputeShader.FindKernel("Smoothing");

            fixArgsKernel = meshComputeShader.FindKernel("FixArgsBuffer");
            indirectDrawKernel = meshComputeShader.FindKernel("FillIndirectDrawBuffer");

            edges2D = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 4, 8);
            edges3D = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 12, 8);
            edgeTable2D = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 16, 4);
            edgeTable3D = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 256, 4);

            edges2D.SetData(new Vector2Int[]{
                new Vector2Int(0,1),
                new Vector2Int(2,3),
                new Vector2Int(0,2),
                new Vector2Int(1,3)
            });

            edges3D.SetData(new Vector2Int[]{
                new Vector2Int(7,3),
                new Vector2Int(7,5),
                new Vector2Int(7,6),
                new Vector2Int(6,4),
                new Vector2Int(4,5),
                new Vector2Int(5,1),
                new Vector2Int(1,3),
                new Vector2Int(3,2),
                new Vector2Int(2,6),
                new Vector2Int(4,0),
                new Vector2Int(2,0),
                new Vector2Int(1,0)
            });

            edgeTable2D.SetData(new int[]{
                0,5,9,12,6,3,15,10,10,15,3,6,12,9,5,0
            });

            edgeTable3D.SetData(new int[]{
                0,3584,2144,1632,1408,2944,3552,992,193,3777,2209,1697,1345,2881,3361,801,
                536,3096,2680,1144,1944,2456,4088,504,729,3289,2745,1209,1881,2393,3897,313,
                50,3634,2130,1618,1458,2994,3538,978,243,3827,2195,1683,1395,2931,3347,787,554,
                3114,2634,1098,1962,2474,4042,458,747,3307,2699,1163,1899,2411,3851,267,268,
                3852,2412,1900,1164,2700,3308,748,461,4045,2477,1965,1101,2637,3117,557,788,
                3348,2932,1396,1684,2196,3828,244,981,3541,2997,1461,1621,2133,3637,53,318,3902,
                2398,1886,1214,2750,3294,734,511,4095,2463,1951,1151,2687,3103,543,806,3366,
                2886,1350,1702,2214,3782,198,999,3559,2951,1415,1639,2151,3591,7,7,3591,2151,
                1639,1415,2951,3559,999,198,3782,2214,1702,1350,2886,3366,806,543,3103,2687,
                1151,1951,2463,4095,511,734,3294,2750,1214,1886,2398,3902,318,53,3637,2133,1621,
                1461,2997,3541,981,244,3828,2196,1684,1396,2932,3348,788,557,3117,2637,1101,
                1965,2477,4045,461,748,3308,2700,1164,1900,2412,3852,268,267,3851,2411,1899,
                1163,2699,3307,747,458,4042,2474,1962,1098,2634,3114,554,787,3347,2931,1395,
                1683,2195,3827,243,978,3538,2994,1458,1618,2130,3634,50,313,3897,2393,1881,1209,
                2745,3289,729,504,4088,2456,1944,1144,2680,3096,536,801,3361,2881,1345,1697,
                2209,3777,193,992,3552,2944,1408,1632,2144,3584,0
            });
        }

        public virtual void Dispose()
        {
            if (surface_Material != null)
                Object.DestroyImmediate(surface_Material);
            if (thickness_Material)
                Object.DestroyImmediate(thickness_Material);

            for (int i = 0; i < batchList.Count; ++i)
                batchList[i].Dispose();
            batchList.Clear();

            DisposeOfVoxels();

            trisDispatchBuffer?.Dispose();

            activeParticles?.Dispose();

            edges2D?.Dispose();
            edges3D?.Dispose();
            edgeTable2D?.Dispose();
            edgeTable3D?.Dispose();
        }

        protected virtual void Clear()
        {
            activeParticles.Clear();
        }

        private void AllocateVoxels()
        {
            uint maxVoxels = m_Solver.maxSurfaceChunks * (uint)Mathf.Pow(chunkResolution, 3 - (int)m_Solver.parameters.mode);

            hashtable = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)m_Solver.maxSurfaceChunks, 8);
            chunkCoords = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)m_Solver.maxSurfaceChunks, 12);

            voxelToVertex = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)maxVoxels, 4);

            // max 6 face neighbors per voxel
            vertexAdjacency = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)maxVoxels * 6, 4);
            verts = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)maxVoxels, 16);
        }

        private void DisposeOfVoxels()
        {
            hashtable?.Dispose();
            chunkCoords?.Dispose();

            voxelToVertex?.Dispose();
            vertexAdjacency?.Dispose();

            verts?.Dispose();
        }

        private void ReallocateVoxels()
        {
            // in case the amount of chunks allocated does not match
            // the amount requested by the solver, reallocate
            if (m_Solver.maxSurfaceChunks != hashtable.count)
            {
                for (int i = 0; i < batchList.Count; ++i)
                    batchList[i].Dispose();
                batchList.Clear();

                DisposeOfVoxels();
                AllocateVoxels();
            }
        }

        protected virtual void CreateBatches()
        {
            //return;
            uint maxVoxels = m_Solver.maxSurfaceChunks * (uint)Mathf.Pow(chunkResolution, 3 - (int)m_Solver.parameters.mode);

            // append new batches if necessary:
            for (int i = batchList.Count; i < renderers.Count; ++i)
                batchList.Add(new IndirectRenderBatch<Vector4>((int)maxVoxels, (int)maxVoxels * 3 * 2, true));

            // clear *all* batches' rendering command buffer to zero. This avoid attempting to render invalid data in RenderVolume/Surface callbacks.
            for (int i = 0; i < batchList.Count; ++i)
                batchList[i].Clear();

            for (int i = 0; i < renderers.Count; ++i)
            {
                batchList[i].firstRenderer = i;
                batchList[i].renderPass = renderers[i].pass;
                batchList[i].renderPass.renderParameters.layer = batchList[i].renderPass.layer;
                batchList[i].rendererCount = 1;
            }

            // sort batches:
            batchList.Sort(0, renderers.Count, Comparer<IndirectRenderBatch<Vector4>>.Default);

            int totalParticleCount = 0;
            for (int i = 0; i < renderers.Count; ++i)
            {
                var batch = batchList[i];
                var renderer = renderers[batch.firstRenderer];
                int actorParticleCount = renderer.actor.particleCount;

                batch.firstParticle = totalParticleCount;
                batch.particleCount = actorParticleCount;

                totalParticleCount += actorParticleCount;

                // add active particles here, respecting batch order:
                activeParticles.AddRange(renderer.actor.solverIndices, actorParticleCount);
            }
        }

        protected void CloseBatches()
        {
            // Initialize each batch:
            for (int i = 0; i < batchCount; ++i)
                batchList[i].Initialize(true);

            activeParticles.AsComputeBuffer<int>();

            System.Array.Resize(ref usedChunksPerBatch, batchCount);
        }

        public virtual void Setup()
        {
            using (m_SetupRenderMarker.Auto())
            {
                ReallocateVoxels();

                Clear();

                CreateBatches();

                batchCount = ObiUtils.MergeBatches(batchList, renderers.Count, false);

                CloseBatches();

            }
        }

        public virtual void Step()
        {
        }

        public void Render()
        {
            using (m_RenderMarker.Auto())
            {
                var solver = m_Solver.implementation as ComputeSolverImpl;

                if (solver.renderablePositionsBuffer != null &&
                    activeParticles.computeBuffer != null &&
                    solver.simplices != null &&
                    solver.particleGrid.offsetInCell != null &&
                    solver.renderablePositionsBuffer.count > 0)
                {
                    int voxelsInChunk = (int)Mathf.Pow(chunkResolution, 3 - (int)solver.abstraction.parameters.mode);

                    meshComputeShader.SetInt("mode", (int)m_Solver.parameters.mode);
                    chunkShader.SetInt("mode", (int)m_Solver.parameters.mode);

                    meshComputeShader.SetInt("maxChunks", (int)m_Solver.maxSurfaceChunks);
                    chunkShader.SetInt("maxChunks", (int)m_Solver.maxSurfaceChunks);

                    meshComputeShader.SetInt("maxCells", solver.particleGrid.maxCells);
                    chunkShader.SetInt("maxCells", solver.particleGrid.maxCells);

                    meshComputeShader.SetInt("instanceCount", XRSettings.enabled && XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePassInstanced ? 2 : 1);

                    for (int i = 0; i < batchCount; ++i)
                    {
                        var batch = batchList[i];
                        if (batch.particleCount == 0 || batch.renderPass == null)
                            continue;

                        float voxelSize = Mathf.Max(0.005f, batch.renderPass.voxelSize);
                        float isosurface = batch.renderPass.isosurface;

                        meshComputeShader.SetInt("currentBatch", i + 1);
                        chunkShader.SetInt("firstParticle", batch.firstParticle);
                        chunkShader.SetInt("particleCount", batch.particleCount);

                        meshComputeShader.SetFloat("isosurface", isosurface);
                        meshComputeShader.SetFloat("smoothing", batch.renderPass.smoothingIntensity);
                        meshComputeShader.SetFloat("bevel", batch.renderPass.bevel);

                        meshComputeShader.SetInt("descentIterations", (int)batch.renderPass.descentIterations);
                        meshComputeShader.SetFloat("descentIsosurface", batch.renderPass.descentIsosurface);
                        meshComputeShader.SetFloat("descentSpeed", batch.renderPass.descentSpeed);

                        chunkShader.SetFloat("isosurface", isosurface);
                        chunkShader.SetFloat("smoothing", batch.renderPass.smoothingIntensity);

                        meshComputeShader.SetFloat("voxelSize", voxelSize);
                        chunkShader.SetFloat("voxelSize", voxelSize);

                        // Calculate bounding box:
                        float chunkSize = chunkResolution * voxelSize;
                        Bounds gridBounds = m_Solver.localBounds;
                        gridBounds.Expand((voxelSize + 2/*+ isosurface + batch.renderPass.smoothingIntensity*/) * 2); // TODO: use grid largest grid cell size.
                        gridBounds.min = new Vector3(Mathf.Floor(gridBounds.min.x / chunkSize) * chunkSize, Mathf.Floor(gridBounds.min.y / chunkSize) * chunkSize, Mathf.Floor(gridBounds.min.z / chunkSize) * chunkSize);
                        gridBounds.max = new Vector3(Mathf.Ceil(gridBounds.max.x / chunkSize) * chunkSize, Mathf.Ceil(gridBounds.max.y / chunkSize) * chunkSize, Mathf.Ceil(gridBounds.max.z / chunkSize) * chunkSize);
                        Vector3Int gridRes = new Vector3Int(Mathf.CeilToInt(gridBounds.size.x / chunkSize), Mathf.CeilToInt(gridBounds.size.y / chunkSize), Mathf.CeilToInt(gridBounds.size.z / chunkSize));

                        meshComputeShader.SetFloats("chunkGridOrigin", gridBounds.min.x, gridBounds.min.y, gridBounds.min.z);
                        meshComputeShader.SetInts("chunkGridResolution", gridRes.x, gridRes.y, gridRes.z);

                        chunkShader.SetFloats("chunkGridOrigin", gridBounds.min.x, gridBounds.min.y, gridBounds.min.z);
                        chunkShader.SetInts("chunkGridResolution", gridRes.x, gridRes.y, gridRes.z);

                        batch.vertexDispatchBuffer.SetData(clearDispatch);
                        trisDispatchBuffer.SetData(clearDispatch);

                        using (m_ChunkMarker.Auto())
                        {
                            int threadGroups = ComputeMath.ThreadGroupCount((int)m_Solver.maxSurfaceChunks, 128);
                            chunkShader.SetBuffer(clearChunksKernel, "hashtable", hashtable);
                            chunkShader.Dispatch(clearChunksKernel, threadGroups, 1, 1);

                            threadGroups = ComputeMath.ThreadGroupCount(solver.particleGrid.maxCells, 128);
                            chunkShader.SetBuffer(clearGridKernel, "cellOffsets", solver.particleGrid.cellOffsets);
                            chunkShader.SetBuffer(clearGridKernel, "cellCounts", solver.particleGrid.cellCounts);
                            chunkShader.SetBuffer(clearGridKernel, "levelPopulation", solver.particleGrid.levelPopulation);
                            chunkShader.Dispatch(clearGridKernel, threadGroups, 1, 1);

                            threadGroups = ComputeMath.ThreadGroupCount(batch.particleCount, 128);
                            chunkShader.SetBuffer(insertKernel, "solverBounds", solver.reducedBounds);
                            chunkShader.SetBuffer(insertKernel, "cellOffsets", solver.particleGrid.cellOffsets);
                            chunkShader.SetBuffer(insertKernel, "cellCounts", solver.particleGrid.cellCounts);
                            chunkShader.SetBuffer(insertKernel, "gridHashToSortedIndex", solver.particleGrid.cellHashToMortonIndex);
                            chunkShader.SetBuffer(insertKernel, "offsetInCell", solver.particleGrid.offsetInCell);
                            chunkShader.SetBuffer(insertKernel, "levelPopulation", solver.particleGrid.levelPopulation);
                            chunkShader.SetBuffer(insertKernel, "hashtable", hashtable);
                            chunkShader.SetBuffer(insertKernel, "particleIndices", activeParticles.computeBuffer);
                            chunkShader.SetBuffer(insertKernel, "positions", solver.renderablePositionsBuffer);
                            chunkShader.SetBuffer(insertKernel, "principalRadii", solver.renderableRadiiBuffer);
                            chunkShader.SetBuffer(insertKernel, "fluidMaterial", solver.fluidMaterialsBuffer);
                            chunkShader.SetBuffer(insertKernel, "normals", solver.normalsBuffer);
                            chunkShader.SetBuffer(insertKernel, "chunkCoords", chunkCoords);
                            chunkShader.SetBuffer(insertKernel, "dispatchBuffer", batch.vertexDispatchBuffer);
                            chunkShader.Dispatch(insertKernel, threadGroups, 1, 1);

                            // retrieve amount of chunks in use:
                            int capturedIndex = i;
                            AsyncGPUReadback.Request(batch.vertexDispatchBuffer, 4, 16, (AsyncGPUReadbackRequest obj) =>
                            {
                                if (obj.done && !obj.hasError && capturedIndex < usedChunksPerBatch.Length)
                                {
                                    usedChunksPerBatch[capturedIndex] = obj.GetData<uint>()[0];
                                    if (usedChunksPerBatch[capturedIndex] / (float)m_Solver.maxSurfaceChunks > 0.75f)
                                        Debug.LogWarning("Hashtable usage should be below 50% for best performance. Increase max surface chunks in your ObiSolver.");
                                }
                            });

                            // find populated grid levels:
                            chunkShader.SetBuffer(gridPopulationKernel, "levelPopulation", solver.particleGrid.levelPopulation);
                            chunkShader.Dispatch(gridPopulationKernel, 1, 1, 1);

                            // prefix sum to build cell start array: 
                            solver.particleGrid.cellsPrefixSum.Sum(solver.particleGrid.cellCounts, solver.particleGrid.cellOffsets);

                            // sort particles:
                            chunkShader.SetBuffer(sortKernel, "solverBounds", solver.reducedBounds);
                            chunkShader.SetBuffer(sortKernel, "particleIndices", activeParticles.computeBuffer);
                            chunkShader.SetBuffer(sortKernel, "cellOffsets", solver.particleGrid.cellOffsets);
                            chunkShader.SetBuffer(sortKernel, "cellCounts", solver.particleGrid.cellCounts);
                            chunkShader.SetBuffer(sortKernel, "gridHashToSortedIndex", solver.particleGrid.cellHashToMortonIndex);
                            chunkShader.SetBuffer(sortKernel, "offsetInCell", solver.particleGrid.offsetInCell);
                            chunkShader.SetBuffer(sortKernel, "positions", solver.renderablePositionsBuffer);
                            chunkShader.SetBuffer(sortKernel, "velocities", solver.velocitiesBuffer);
                            chunkShader.SetBuffer(sortKernel, "angularVelocities", solver.angularVelocitiesBuffer);
                            chunkShader.SetBuffer(sortKernel, "principalRadii", solver.renderableRadiiBuffer);
                            chunkShader.SetBuffer(sortKernel, "fluidMaterial", solver.fluidMaterialsBuffer);
                            chunkShader.SetBuffer(sortKernel, "fluidData", solver.fluidDataBuffer);
                            chunkShader.SetBuffer(sortKernel, "orientations", solver.renderableOrientationsBuffer);
                            chunkShader.SetBuffer(sortKernel, "colors", solver.colorsBuffer);
                            chunkShader.SetBuffer(sortKernel, "sortedPositions", solver.particleGrid.sortedPositions);
                            chunkShader.SetBuffer(sortKernel, "sortedVelocities", solver.particleGrid.sortedFluidDataVel);
                            chunkShader.SetBuffer(sortKernel, "sortedPrincipalRadii", solver.particleGrid.sortedPrincipalRadii);
                            chunkShader.SetBuffer(sortKernel, "sortedOrientations", solver.particleGrid.sortedPrevPosOrientations);
                            chunkShader.SetBuffer(sortKernel, "sortedColors", solver.particleGrid.sortedUserDataColor);
                            chunkShader.Dispatch(sortKernel, threadGroups, 1, 1);

                        }

                        using (m_SDFMarker.Auto())
                        {
                            // multiply amount of chunks by amount of voxels:
                            meshComputeShader.SetInt("dispatchMultiplier", voxelsInChunk);
                            meshComputeShader.SetInt("countMultiplier", 1);
                            meshComputeShader.SetBuffer(fixArgsKernel, "dispatchBuffer", batch.vertexDispatchBuffer);
                            meshComputeShader.Dispatch(fixArgsKernel, 1, 1, 1);

                            meshComputeShader.SetBuffer(sdfKernel, "solverBounds", solver.reducedBounds);
                            meshComputeShader.SetBuffer(sdfKernel, "cellOffsets", solver.particleGrid.cellOffsets);
                            meshComputeShader.SetBuffer(sdfKernel, "cellCounts", solver.particleGrid.cellCounts);
                            meshComputeShader.SetBuffer(sdfKernel, "gridHashToSortedIndex", solver.particleGrid.cellHashToMortonIndex);
                            meshComputeShader.SetBuffer(sdfKernel, "levelPopulation", solver.particleGrid.levelPopulation);
                            meshComputeShader.SetBuffer(sdfKernel, "sortedPositions", solver.particleGrid.sortedPositions);
                            meshComputeShader.SetBuffer(sdfKernel, "sortedVelocities", solver.particleGrid.sortedFluidDataVel);
                            meshComputeShader.SetBuffer(sdfKernel, "sortedPrincipalRadii", solver.particleGrid.sortedPrincipalRadii);
                            meshComputeShader.SetBuffer(sdfKernel, "sortedOrientations", solver.particleGrid.sortedPrevPosOrientations);
                            meshComputeShader.SetBuffer(sdfKernel, "sortedColors", solver.particleGrid.sortedUserDataColor);

                            meshComputeShader.SetBuffer(sdfKernel, "dispatchBuffer", batch.vertexDispatchBuffer);
                            meshComputeShader.SetBuffer(sdfKernel, "hashtable", hashtable);
                            meshComputeShader.SetBuffer(sdfKernel, "verts", verts);
                            meshComputeShader.SetBuffer(sdfKernel, "voxelVelocities", vertexAdjacency);
                            meshComputeShader.SetBuffer(sdfKernel, "simplices", solver.simplices);
                            meshComputeShader.SetBuffer(sdfKernel, "chunkCoords", chunkCoords);
                            meshComputeShader.DispatchIndirect(sdfKernel, batch.vertexDispatchBuffer);
                        }

                        using (m_SurfaceMarker.Auto())
                        {
                            // multiply amount of chunks by amount of voxels:
                            meshComputeShader.SetInt("dispatchMultiplier", voxelsInChunk);
                            meshComputeShader.SetInt("countMultiplier", 0);
                            meshComputeShader.SetBuffer(fixArgsKernel, "dispatchBuffer", batch.vertexDispatchBuffer);
                            meshComputeShader.Dispatch(fixArgsKernel, 1, 1, 1);

                            meshComputeShader.SetBuffer(surfaceKernel, "edgeTable", solver.abstraction.parameters.mode == Oni.SolverParameters.Mode.Mode2D ? edgeTable2D : edgeTable3D);
                            meshComputeShader.SetBuffer(surfaceKernel, "edges", solver.abstraction.parameters.mode == Oni.SolverParameters.Mode.Mode2D ? edges2D : edges3D);
                            meshComputeShader.SetBuffer(surfaceKernel, "dispatchBuffer", batch.vertexDispatchBuffer);
                            meshComputeShader.SetBuffer(surfaceKernel, "hashtable", hashtable);
                            meshComputeShader.SetBuffer(surfaceKernel, "voxelToVertex", voxelToVertex);
                            meshComputeShader.SetBuffer(surfaceKernel, "voxelVelocities", vertexAdjacency);
                            meshComputeShader.SetBuffer(surfaceKernel, "chunkCoords", chunkCoords);
                            meshComputeShader.SetBuffer(surfaceKernel, "verts", verts);
                            meshComputeShader.SetBuffer(surfaceKernel, "colors", batch.gpuColorBuffer);
                            meshComputeShader.SetBuffer(surfaceKernel, "velocities", batch.gpuVelocityBuffer);
                            meshComputeShader.SetBuffer(surfaceKernel, "outputVerts", batch.gpuVertexBuffer);
                            meshComputeShader.DispatchIndirect(surfaceKernel, batch.vertexDispatchBuffer);
                        }

                        using (m_TriangulateMarker.Auto())
                        {
                            // calculate amount of threadgroups for surface voxels:
                            meshComputeShader.SetInt("dispatchMultiplier", 1);
                            meshComputeShader.SetBuffer(fixArgsKernel, "dispatchBuffer", batch.vertexDispatchBuffer);
                            meshComputeShader.Dispatch(fixArgsKernel, 1, 1, 1);

                            meshComputeShader.SetBuffer(triangulateKernel, "edgeTable", solver.abstraction.parameters.mode == Oni.SolverParameters.Mode.Mode2D ? edgeTable2D : edgeTable3D);
                            meshComputeShader.SetBuffer(triangulateKernel, "voxelToVertex", voxelToVertex);
                            meshComputeShader.SetBuffer(triangulateKernel, "hashtable", hashtable);
                            meshComputeShader.SetBuffer(triangulateKernel, "chunkCoords", chunkCoords);
                            meshComputeShader.SetBuffer(triangulateKernel, "dispatchBuffer", batch.vertexDispatchBuffer); // surface vertex count
                            meshComputeShader.SetBuffer(triangulateKernel, "dispatchBuffer2", trisDispatchBuffer); // quad count.
                            meshComputeShader.SetBuffer(triangulateKernel, "verts", verts);
                            meshComputeShader.SetBuffer(triangulateKernel, "vertexAdjacency", vertexAdjacency);
                            meshComputeShader.SetBuffer(triangulateKernel, "outputVerts", batch.gpuVertexBuffer);
                            meshComputeShader.SetBuffer(triangulateKernel, "quads", batch.gpuIndexBuffer);
                            meshComputeShader.DispatchIndirect(triangulateKernel, batch.vertexDispatchBuffer);
                        }

                        using (m_SmoothingMarker.Auto())
                        {
                            for (int j = 0; j < batch.renderPass.smoothingIterations; ++j)
                            {
                                meshComputeShader.SetBuffer(smoothingKernel, "vertexAdjacency", vertexAdjacency);
                                meshComputeShader.SetBuffer(smoothingKernel, "dispatchBuffer", batch.vertexDispatchBuffer); // surface vertex count
                                meshComputeShader.SetBuffer(smoothingKernel, "verts", batch.gpuVertexBuffer);
                                meshComputeShader.SetBuffer(smoothingKernel, "outputVerts", verts);
                                meshComputeShader.DispatchIndirect(smoothingKernel, batch.vertexDispatchBuffer);

                                meshComputeShader.SetBuffer(smoothingKernel, "verts", verts);
                                meshComputeShader.SetBuffer(smoothingKernel, "outputVerts", batch.gpuVertexBuffer);
                                meshComputeShader.DispatchIndirect(smoothingKernel, batch.vertexDispatchBuffer);
                            }
                        }

                        meshComputeShader.SetInt("dispatchMultiplier", 1); // multiply number of quads * 1 to obtain number of quads? duh.
                        meshComputeShader.SetInt("countMultiplier", 1);
                        meshComputeShader.SetBuffer(fixArgsKernel, "dispatchBuffer", trisDispatchBuffer);
                        meshComputeShader.Dispatch(fixArgsKernel, 1, 1, 1);

                        meshComputeShader.SetBuffer(indirectDrawKernel, "dispatchBuffer", trisDispatchBuffer);
                        meshComputeShader.SetBuffer(indirectDrawKernel, "indirectBuffer", batch.indirectCommandBuffer);
                        meshComputeShader.Dispatch(indirectDrawKernel, 1, 1, 1);


                        var rp = batch.renderPass.renderParameters.ToRenderParams();
                        rp.worldBounds = m_Solver.bounds;

                        rp.matProps = new MaterialPropertyBlock();
                        rp.matProps.SetBuffer("_Vertices", batch.gpuVertexBuffer);
                        rp.matProps.SetBuffer("_Colors", batch.gpuColorBuffer);
                        rp.matProps.SetBuffer("_Velocities", batch.gpuVelocityBuffer);
                        rp.matProps.SetMatrix("_ObjectToWorld", m_Solver.transform.localToWorldMatrix);

                        if (batch.renderPass.diffuseMap != null)
                            rp.matProps.SetTexture("_Texture", batch.renderPass.diffuseMap);
                        if (batch.renderPass.normalMap != null)
                            rp.matProps.SetTexture("_NormalMap", batch.renderPass.normalMap);
                        if (batch.renderPass.reflectionCubemap != null)
                            rp.matProps.SetTexture("_ReflectionCubemap", batch.renderPass.reflectionCubemap);
                        if (batch.renderPass.noiseMap != null)
                            rp.matProps.SetTexture("_AdvectionNoise", batch.renderPass.noiseMap);
                        
                        rp.matProps.SetFloat("_NormalMapIntensity", batch.renderPass.normalMapIntensity);
                        rp.matProps.SetVector("_NormalMapVelRange", batch.renderPass.normalMapVelocityRange);
                        rp.matProps.SetFloat("_NoiseMapIntensity", batch.renderPass.noiseMapIntensity);
                        rp.matProps.SetFloat("_NoiseMapTiling", batch.renderPass.noiseMapTiling);
                        rp.matProps.SetFloat("_Tiling", batch.renderPass.tiling);
                        rp.matProps.SetFloat("_TriplanarBlend", batch.renderPass.triplanarBlend);
                        rp.matProps.SetFloat("_Timescale", batch.renderPass.advectTimescale);
                        rp.matProps.SetVector("_Jump", batch.renderPass.advectJump);
                        rp.matProps.SetFloat("_Offset", batch.renderPass.advectOffset);
                        rp.matProps.SetFloat("_Smoothness", batch.renderPass.smoothness);
                        rp.matProps.SetFloat("_Metallic", batch.renderPass.metallic);
                        rp.matProps.SetFloat("_Thickness", batch.renderPass.thickness);
                        rp.matProps.SetFloat("_Refraction", batch.renderPass.indexOfRefraction);
                        rp.matProps.SetColor("_Turbidity", batch.renderPass.turbidity);
                        rp.matProps.SetColor("_DiffuseColor", batch.renderPass.diffuseColor);
                        rp.matProps.SetFloat("_SimulationTime", solver.abstraction.timeSinceSimulationStart);

                        if (batch.renderPass.underwaterRendering && batch.renderPass.underwaterMaterial != null)
                        {
                            rp.material = batch.renderPass.underwaterMaterial;
                            Graphics.RenderPrimitivesIndexedIndirect(rp, MeshTopology.Triangles, batch.gpuIndexBuffer, batch.indirectCommandBuffer);
                        }

                        rp.material = batch.renderPass.fluidMaterial;
                        Graphics.RenderPrimitivesIndexedIndirect(rp, MeshTopology.Triangles, batch.gpuIndexBuffer, batch.indirectCommandBuffer);
                    }
                }
            }
        }

        public void RenderVolume(CommandBuffer cmd, ObiFluidRenderingPass pass, ObiFluidSurfaceMesher renderer)
        {
            var solver = m_Solver.implementation as ComputeSolverImpl;

            if (solver.renderablePositionsBuffer != null &&
                activeParticles.computeBuffer != null &&
                solver.renderablePositionsBuffer.count > 0)
            {

                for (int i = 0; i < batchCount; ++i)
                {
                    var batch = batchList[i];

                    if (batch.renderPass == pass)
                    {
                        thickness_Material.SetKeyword(shader2DFeature, m_Solver.parameters.mode == Oni.SolverParameters.Mode.Mode2D);
                        cmd.SetGlobalBuffer("_Vertices", batch.gpuVertexBuffer);
                        cmd.SetGlobalBuffer("_Colors", batch.gpuColorBuffer);
                        cmd.SetGlobalBuffer("_Velocities", batch.gpuVelocityBuffer);
                        cmd.SetGlobalMatrix("_ObjectToWorld", m_Solver.transform.localToWorldMatrix);
                        cmd.DrawProceduralIndirect(batch.gpuIndexBuffer, m_Solver.transform.localToWorldMatrix, thickness_Material, 0, MeshTopology.Triangles, batch.indirectCommandBuffer);
                    }
                }
            }
        }

        public void RenderSurface(CommandBuffer cmd, ObiFluidRenderingPass pass, ObiFluidSurfaceMesher renderer)
        {
            var solver = m_Solver.implementation as ComputeSolverImpl;

            if (solver.renderablePositionsBuffer != null &&
                activeParticles.computeBuffer != null &&
                solver.renderablePositionsBuffer.count > 0)
            {

                for (int i = 0; i < batchCount; ++i)
                {
                    var batch = batchList[i];

                    if (batch.renderPass == pass)
                    {
                        cmd.SetGlobalBuffer("_Vertices", batch.gpuVertexBuffer);
                        cmd.SetGlobalBuffer("_Colors", batch.gpuColorBuffer);
                        cmd.SetGlobalBuffer("_Velocities", batch.gpuVelocityBuffer);
                        cmd.SetGlobalMatrix("_ObjectToWorld", m_Solver.transform.localToWorldMatrix);
                        cmd.DrawProceduralIndirect(batch.gpuIndexBuffer, m_Solver.transform.localToWorldMatrix, surface_Material, 1, MeshTopology.Triangles, batch.indirectCommandBuffer);
                        cmd.ClearRenderTarget(true, false, Color.clear);
                        cmd.DrawProceduralIndirect(batch.gpuIndexBuffer, m_Solver.transform.localToWorldMatrix, surface_Material, 0, MeshTopology.Triangles, batch.indirectCommandBuffer);
                    }
                }
            }
        }

        public void BakeMesh(ObiFluidSurfaceMesher renderer, ref Mesh mesh)
        {
            int index = renderers.IndexOf(renderer);

            for (int i = 0; i < batchList.Count; ++i)
            {
                var batch = batchList[i];
                if (index >= batch.firstRenderer && index < batch.firstRenderer + batch.rendererCount)
                {
                    // readback amount of vertices and amount of indices:
                    uint[] dsp = new uint[batch.vertexDispatchBuffer.count];
                    batch.vertexDispatchBuffer.GetData(dsp);

                    IndirectDrawIndexedArgs[] args = new IndirectDrawIndexedArgs[1];
                    batch.indirectCommandBuffer.GetData(args);

                    batch.BakeMesh((int)dsp[3], (int)args[0].indexCountPerInstance / 3, ref mesh);
                    return;
                }
            }
        }
    }
}

