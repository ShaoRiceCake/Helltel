#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace Obi
{

    public class BurstFluidMesherSystem : RenderSystem<ObiFluidSurfaceMesher>, IFluidRenderSystem, ISurfaceChunkUser
    {
        public struct keyvalue
        {
            public int key;
            public int handle;
        }

        protected VertexAttributeDescriptor[] layout =
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 1),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4,1),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 4,2),
        };

        public Oni.RenderingSystemType typeEnum { get => Oni.RenderingSystemType.Fluid; }

        public RendererSet<ObiFluidSurfaceMesher> renderers { get; } = new RendererSet<ObiFluidSurfaceMesher>();
        public bool isSetup => activeParticles != null;

        static protected ProfilerMarker m_SetupRenderMarker = new ProfilerMarker("SetupSurfaceMeshing");
        static protected ProfilerMarker m_RenderMarker = new ProfilerMarker("SurfaceMeshing");

        protected ObiSolver m_Solver;
        protected List<IndirectRenderBatch<Vector4>> batchList = new List<IndirectRenderBatch<Vector4>>();

        protected NativeMultilevelGrid<int> grid;
        protected ObiNativeList<int> activeParticles;

        protected ObiNativeList<keyvalue> hashtable;
        protected ObiNativeList<int3> chunkCoords;

        protected ObiNativeList<int> voxelToVertex;

        protected ObiNativeList<float4> verts;

        // vertex/tri adjacency:
        protected ObiNativeList<int> vertexAdjacency;

        protected ObiNativeList<int> dispatchBuffer;

        // edge LUTs:
        protected NativeArray<int2> edges2D;
        protected NativeArray<int2> edges3D;
        protected NativeArray<int> edgeTable2D;
        protected NativeArray<int> edgeTable3D;

        private static readonly int chunkResolution = 4; // amount of voxels in width/height/depth
        private int batchCount;

        protected Material surface_Material;
        protected Material thickness_Material;
        protected LocalKeyword shader2DFeature;

        public uint usedChunkCount { private set; get; }

        protected Material CreateMaterial(Shader shader)
        {
            if (!shader || !shader.isSupported)
                return null;
            Material m = new Material(shader);
            m.hideFlags = HideFlags.HideAndDontSave;
            return m;
        }

        public BurstFluidMesherSystem(ObiSolver solver)
        {
            m_Solver = solver;

            if (surface_Material == null)
                surface_Material = CreateMaterial(Shader.Find("Hidden/BurstSurface"));

            if (thickness_Material == null)
                thickness_Material = CreateMaterial(Shader.Find("Hidden/BurstThickness"));

            shader2DFeature = new LocalKeyword(thickness_Material.shader, "MODE_2D");

            grid = new NativeMultilevelGrid<int>(1000, Allocator.Persistent);
            activeParticles = new ObiNativeList<int>();

            dispatchBuffer = new ObiNativeList<int>();
            dispatchBuffer.Add(0); // amount of chunks/voxels
            dispatchBuffer.Add(0); // amount of surface vertices
            dispatchBuffer.Add(0); // amount of surface quads

            edges2D = new NativeArray<int2>(new int2[]{
                new int2(0,1),
                new int2(2,3),
                new int2(0,2),
                new int2(1,3)
            },Allocator.Persistent);

            edges3D = new NativeArray<int2>(new int2[]{
                new int2(7,3),
                new int2(7,5),
                new int2(7,6),
                new int2(6,4),
                new int2(4,5),
                new int2(5,1),
                new int2(1,3),
                new int2(3,2),
                new int2(2,6),
                new int2(4,0),
                new int2(2,0),
                new int2(1,0)
            },Allocator.Persistent);

            edgeTable2D = new NativeArray<int>(new int[]{
                0,5,9,12,6,3,15,10,10,15,3,6,12,9,5,0
            }, Allocator.Persistent);

            edgeTable3D = new NativeArray<int>(new int[]{
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
            }, Allocator.Persistent);

            AllocateVoxels();

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

            grid.Dispose();
            activeParticles?.Dispose();
            dispatchBuffer?.Dispose();

            edges2D.Dispose();
            edges3D.Dispose();
            edgeTable2D.Dispose();
            edgeTable3D.Dispose();
        }

        protected virtual void Clear()
        {
            activeParticles.Clear();
        }

        private void AllocateVoxels()
        {
            int maxVoxels = (int)m_Solver.maxSurfaceChunks * (int)Mathf.Pow(chunkResolution, 3 - (int)m_Solver.parameters.mode);

            // memory consumption:
            // (8 + 12 + 64*(4+4) + 64*4*4) * 4 * 32768  = 203 Mb

            hashtable = new ObiNativeList<keyvalue>((int)m_Solver.maxSurfaceChunks);
            hashtable.ResizeUninitialized((int)m_Solver.maxSurfaceChunks);

            chunkCoords = new ObiNativeList<int3>((int)m_Solver.maxSurfaceChunks);
            chunkCoords.ResizeUninitialized((int)m_Solver.maxSurfaceChunks);

            voxelToVertex = new ObiNativeList<int>(maxVoxels);
            voxelToVertex.ResizeUninitialized(maxVoxels);

            vertexAdjacency = new ObiNativeList<int>(maxVoxels * 6);
            vertexAdjacency.ResizeUninitialized(maxVoxels * 6);

            verts = new ObiNativeList<float4>(maxVoxels);
            verts.ResizeUninitialized(maxVoxels);
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
            int maxVoxels = (int)m_Solver.maxSurfaceChunks * (int)Mathf.Pow(chunkResolution, 3 - (int)m_Solver.parameters.mode);

            // append new batches if necessary:
            for (int i = batchList.Count; i < renderers.Count; ++i)
                batchList.Add(new IndirectRenderBatch<Vector4>(maxVoxels, maxVoxels * 3 * 2, false)); // 3 quads per voxel

            for (int i = 0; i < renderers.Count; ++i)
            {
                batchList[i].firstRenderer = i;
                batchList[i].renderPass = renderers[i].pass;
                batchList[i].renderPass.renderParameters.layer = batchList[i].renderPass.layer;
                batchList[i].rendererCount = 1;
            }

            // sort batches:
            batchList.Sort(0,renderers.Count,Comparer<IndirectRenderBatch<Vector4>>.Default);

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
                batchList[i].Initialize();

            activeParticles.AsComputeBuffer<int>();
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

        public unsafe void Render()
        {
            usedChunkCount = 0;

            using (m_RenderMarker.Auto())
            {
                var solver = m_Solver.implementation as BurstSolverImpl;

                if (!solver.reducedBounds.IsCreated)
                    return;

                for (int i = 0; i < batchCount; ++i)
                {
                    var batch = batchList[i];
                    if (batch.particleCount == 0 || batch.renderPass == null)
                        continue;

                    float voxelSize = math.max(0.005f, batch.renderPass.voxelSize);
                    float isosurface = math.max(0, batch.renderPass.isosurface + 0.005f);

                    // Calculate bounding box:
                    float chunkSize = chunkResolution * voxelSize;
                    Bounds gridBounds = m_Solver.localBounds;
                    gridBounds.Expand((voxelSize + 2) * 2);
                    gridBounds.min = new Vector3(Mathf.Floor(gridBounds.min.x / chunkSize) * chunkSize, Mathf.Floor(gridBounds.min.y / chunkSize) * chunkSize, Mathf.Floor(gridBounds.min.z / chunkSize) * chunkSize);
                    gridBounds.max = new Vector3(Mathf.Ceil(gridBounds.max.x / chunkSize) * chunkSize, Mathf.Ceil(gridBounds.max.y / chunkSize) * chunkSize, Mathf.Ceil(gridBounds.max.z / chunkSize) * chunkSize);
                    Vector3Int gridRes = new Vector3Int(Mathf.CeilToInt(gridBounds.size.x / chunkSize), Mathf.CeilToInt(gridBounds.size.y / chunkSize), Mathf.CeilToInt(gridBounds.size.z / chunkSize));

                    dispatchBuffer[0] = 0;
                    dispatchBuffer[1] = 0;
                    dispatchBuffer[2] = 0;

                    int* dispatchPtr = (int*)dispatchBuffer.AsNativeArray<int>().GetUnsafePtr();

                    var clearJob = new ClearJob
                    {
                        hashtable = hashtable.AsNativeArray<keyvalue>(),
                    };
                    var handle = clearJob.Schedule(hashtable.count, 256);

                    var meshJob = new InsertChunks
                    {
                        particleIndices = activeParticles.AsNativeArray<int>(),
                        positions = solver.renderablePositions,
                        principalRadii = solver.renderableRadii,
                        fluidMaterial = solver.fluidMaterials,
                        normals = solver.normals,

                        hashtable = hashtable.AsNativeArray<keyvalue>(),
                        chunkCoords = chunkCoords.AsNativeArray<int3>(),

                        dispatchBuffer = dispatchBuffer.AsNativeArray<int>(),
                        voxelSize = voxelSize,
                        chunkGridOrigin = new float3(gridBounds.min.x, gridBounds.min.y, gridBounds.min.z),
                        chunkGridResolution = new int3(gridRes.x, gridRes.y, gridRes.z),

                        isosurface = isosurface, 
                        firstParticle = batch.firstParticle,
                        parameters = solver.abstraction.parameters
                    };
                    handle = meshJob.Schedule(batch.particleCount, 64, handle);

                    var buildGrid = new BuildGrid
                    {
                        grid = grid,
                        particleIndices = activeParticles.AsNativeArray<int>(),
                        positions = solver.renderablePositions,
                        radii = solver.renderableRadii,
                        fluidMaterial = solver.fluidMaterials,
                        solverBounds = solver.reducedBounds,
                        firstParticle = batch.firstParticle,
                        particleCount = batch.particleCount,
                        parameters = solver.abstraction.parameters
                    };
                    handle = buildGrid.Schedule(handle);
                    handle.Complete(); // Need to complete in order to read from populatedLevels.

                    // retrieve amount of chunks in use:
                    usedChunkCount = (uint)math.max(usedChunkCount, dispatchBuffer[0]);
                    if (usedChunkCount / (float)m_Solver.maxSurfaceChunks > 0.75f)
                        Debug.LogWarning("Hashtable usage should be below 50% for best performance. Increase max surface chunks in your ObiSolver.");

                    var sdfJob = new SampleSDF
                    {
                        grid = grid,
                        gridLevels = grid.populatedLevels.GetKeyArray(Allocator.TempJob),
                        chunkCoords = chunkCoords.AsNativeArray<int3>(),
                        positions = solver.renderablePositions,
                        velocities = solver.velocities,
                        angularVelocities = solver.angularVelocities,
                        principalRadii = solver.renderableRadii,
                        fluidMaterial = solver.fluidMaterials,
                        fluidData = solver.fluidData,
                        orientations = solver.renderableOrientations,
                        colors = solver.colors,
                        verts = verts.AsNativeArray<float4>(),
                        voxelVelocities = vertexAdjacency.AsNativeArray<int>().Reinterpret<float4>(4),
                        densityKernel = new Poly6Kernel(solver.abstraction.parameters.mode == Oni.SolverParameters.Mode.Mode2D),

                        voxelSize = voxelSize,
                        currentBatch = i + 1,
                        chunkGridOrigin = new float3(gridBounds.min.x, gridBounds.min.y, gridBounds.min.z),
                        solverBounds = solver.reducedBounds,
                        parameters = solver.abstraction.parameters,
                        isosurface = isosurface
                    };
                    sdfJob.Initialize();
                    handle = IJobParallelForDeferExtensions.Schedule(sdfJob, dispatchPtr, 1, handle);

                    var countJob = new VoxelCountFromChunkCount
                    {
                        dispatchBuffer = dispatchBuffer.AsNativeArray<int>(),
                        parameters = solver.abstraction.parameters
                    };
                    handle = countJob.Schedule(handle);

                    var surfaceJob = new CalculateSurface
                    {
                        voxelToVertex = voxelToVertex.AsNativeArray<int>(),
                        hashtable = hashtable.AsNativeArray<keyvalue>(),
                        chunkCoords = chunkCoords.AsNativeArray<int3>(),
                        edges = solver.abstraction.parameters.mode == Oni.SolverParameters.Mode.Mode2D ? edges2D : edges3D,
                        edgeTable = solver.abstraction.parameters.mode == Oni.SolverParameters.Mode.Mode2D ? edgeTable2D : edgeTable3D,

                        dispatchBuffer = dispatchBuffer.AsNativeArray<int>(),
                        verts = verts.AsNativeArray<float4>(),
                        voxelVelocities = vertexAdjacency.AsNativeArray<int>().Reinterpret<float4>(4),
                        outputVerts = batch.vertexBuffer.Reinterpret<float4>(),
                        outputColors = batch.colorBuffer.Reinterpret<float4>(),
                        outputVelocities = batch.velocityBuffer.Reinterpret<float4>(),

                        voxelSize = voxelSize,
                        bevel = batch.renderPass.bevel,
                        chunkGridOrigin = new float3(gridBounds.min.x, gridBounds.min.y, gridBounds.min.z),
                        chunkGridResolution = new int3(gridRes.x, gridRes.y, gridRes.z),
                        solverBounds = solver.reducedBounds,

                        descentIsosurface = batch.renderPass.descentIsosurface,
                        descentIterations = (int)batch.renderPass.descentIterations,
                        descentSpeed = batch.renderPass.descentSpeed,
                        isosurface = isosurface,

                        parameters = solver.abstraction.parameters
                    };
                    surfaceJob.Initialize();
                    handle = IJobParallelForDeferExtensions.Schedule(surfaceJob, dispatchPtr, 32, handle);

                    var triangulateJob = new Triangulate
                    {
                        voxelToVertex = voxelToVertex.AsNativeArray<int>(),
                        hashtable = hashtable.AsNativeArray<keyvalue>(),
                        chunkCoords = chunkCoords.AsNativeArray<int3>(),
                        edgeTable = solver.abstraction.parameters.mode == Oni.SolverParameters.Mode.Mode2D ? edgeTable2D : edgeTable3D,
                        quads = batch.indexBuffer,

                        vertexAdjacency = vertexAdjacency.AsNativeArray<int>(),
                        dispatchBuffer = dispatchBuffer.AsNativeArray<int>(),
                        verts = verts.AsNativeArray<float4>(),
                        outputVerts = batch.vertexBuffer.Reinterpret<float4>(),

                        chunkGridResolution = new int3(gridRes.x, gridRes.y, gridRes.z),
                        parameters = solver.abstraction.parameters
                    };
                    triangulateJob.Initialize();
                    handle = IJobParallelForDeferExtensions.Schedule(triangulateJob, &dispatchPtr[1], 32, handle);

                    var smoothingJob1 = new Smoothing
                    {
                        vertexAdjacency = vertexAdjacency.AsNativeArray<int>(),
                        smoothingFactor = batch.renderPass.smoothingIntensity,
                        verts = batch.vertexBuffer.Reinterpret<float4>(),
                        outputVerts = verts.AsNativeArray<float4>()
                    };

                    var smoothingJob2 = new Smoothing
                    {
                        vertexAdjacency = vertexAdjacency.AsNativeArray<int>(),
                        smoothingFactor = batch.renderPass.smoothingIntensity,
                        verts = verts.AsNativeArray<float4>(),
                        outputVerts = batch.vertexBuffer.Reinterpret<float4>()
                    };

                    for (int j = 0; j < batch.renderPass.smoothingIterations; ++j)
                    {
                        handle = IJobParallelForDeferExtensions.Schedule(smoothingJob1, &dispatchPtr[1], 128, handle);
                        handle = IJobParallelForDeferExtensions.Schedule(smoothingJob2, &dispatchPtr[1], 128, handle);
                    }

                    handle.Complete(); 

                    batch.mesh.SetVertexBufferParams(dispatchPtr[1], layout);
                    batch.mesh.SetIndexBufferParams(dispatchPtr[2] * 6, IndexFormat.UInt32);

                    batch.mesh.SetVertexBufferData(batch.vertexBuffer, 0, 0, dispatchPtr[1], 0, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
                    batch.mesh.SetVertexBufferData(batch.colorBuffer, 0, 0, dispatchPtr[1], 1, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
                    batch.mesh.SetVertexBufferData(batch.velocityBuffer, 0, 0, dispatchPtr[1], 2, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
                    batch.mesh.SetIndexBufferData(batch.indexBuffer, 0, 0, dispatchPtr[2] * 6, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);

                    batch.mesh.subMeshCount = 1;
                    SubMeshDescriptor subMeshDescriptor = new SubMeshDescriptor();
                    subMeshDescriptor.indexCount = dispatchPtr[2] * 6;
                    batch.mesh.SetSubMesh(0, subMeshDescriptor, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

                    var rp = batch.renderPass.renderParameters.ToRenderParams();
                    rp.material = batch.renderPass.fluidMaterial;
                    rp.worldBounds = m_Solver.bounds;

                    rp.matProps = new MaterialPropertyBlock();

                    if (batch.renderPass.diffuseMap != null)
                        rp.matProps.SetTexture("_Texture", batch.renderPass.diffuseMap);
                    if (batch.renderPass.normalMap != null)
                        rp.matProps.SetTexture("_NormalMap", batch.renderPass.normalMap);
                    if (batch.renderPass.reflectionCubemap != null)
                        rp.matProps.SetTexture("_ReflectionCubemap", batch.renderPass.reflectionCubemap);
                    if(batch.renderPass.noiseMap != null)
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

                    if(batch.renderPass.underwaterRendering && batch.renderPass.underwaterMaterial != null)
                    {
                        rp.material = batch.renderPass.underwaterMaterial;
                        Graphics.RenderMesh(rp, batch.mesh, 0, m_Solver.transform.localToWorldMatrix, m_Solver.transform.localToWorldMatrix);
                    }

                    rp.material = batch.renderPass.fluidMaterial;
                    Graphics.RenderMesh(rp, batch.mesh, 0, m_Solver.transform.localToWorldMatrix, m_Solver.transform.localToWorldMatrix);
                }
            }
        }

        public void RenderVolume(CommandBuffer cmd, ObiFluidRenderingPass pass, ObiFluidSurfaceMesher renderer)
        {
            for (int i = 0; i < batchCount; ++i)
            {
                var batch = batchList[i];

                if (batch.renderPass == pass)
                {
                    thickness_Material.SetKeyword(shader2DFeature, m_Solver.parameters.mode == Oni.SolverParameters.Mode.Mode2D);
                    cmd.DrawMesh(batch.mesh, m_Solver.transform.localToWorldMatrix, thickness_Material);
                }
            }
        }

        public void RenderSurface(CommandBuffer cmd, ObiFluidRenderingPass pass, ObiFluidSurfaceMesher renderer)
        {
            for (int i = 0; i < batchCount; ++i)
            {
                var batch = batchList[i];

                if (batch.renderPass == pass)
                {
                    thickness_Material.SetKeyword(shader2DFeature, m_Solver.parameters.mode == Oni.SolverParameters.Mode.Mode2D);
                    cmd.DrawMesh(batch.mesh, m_Solver.transform.localToWorldMatrix, surface_Material,0,1);
                    cmd.ClearRenderTarget(true, false, Color.clear);
                    cmd.DrawMesh(batch.mesh, m_Solver.transform.localToWorldMatrix, surface_Material,0,0);
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
                    batch.BakeMesh(batch.mesh.vertexCount, (int)batch.mesh.GetIndexCount(0) / 3, ref mesh);
                    return;
                }
            }
        }

        private static int voxelCoordToOffset(int3 coord, Oni.SolverParameters.Mode mode)
        {
            if (mode == Oni.SolverParameters.Mode.Mode2D)
                return (int)BurstMath.EncodeMorton2((uint2)coord.xy);
            return (int)BurstMath.EncodeMorton3((uint3)coord.xyz);
        }

        [BurstCompile]
        struct VoxelCountFromChunkCount : IJob
        {
            public NativeArray<int> dispatchBuffer;
            [ReadOnly] public Oni.SolverParameters parameters;

            public void Execute()
            {
                dispatchBuffer[0] *= (int)math.pow(chunkResolution, 3 - (int)parameters.mode);
            }
        }

        [BurstCompile]
        struct ClearJob : IJobParallelFor
        {
            [NativeDisableParallelForRestriction] public NativeArray<keyvalue> hashtable;

            public void Execute(int i)
            {
                // clear all chunks:
                hashtable[i] = new keyvalue
                {
                    key = -1,
                    handle = -1
                };
            }
        }

        [BurstCompile]
        unsafe struct InsertChunks : IJobParallelFor
        {
            [ReadOnly] public NativeArray<int> particleIndices;
            [ReadOnly] public NativeArray<float4> positions;
            [ReadOnly] public NativeArray<float4> principalRadii;
            [ReadOnly] public NativeArray<float4> fluidMaterial;
            [ReadOnly] public NativeArray<float4> normals;

            [NativeDisableParallelForRestriction] public NativeArray<keyvalue> hashtable;
            [NativeDisableParallelForRestriction] public NativeArray<int3> chunkCoords;

            [NativeDisableParallelForRestriction] public NativeArray<int> dispatchBuffer;

            [ReadOnly] public float voxelSize;
            [ReadOnly] public float3 chunkGridOrigin;
            [ReadOnly] public int3 chunkGridResolution;
            [ReadOnly] public float isosurface;

            [ReadOnly] public int firstParticle;

            [ReadOnly] public Oni.SolverParameters parameters;

            uint hash(uint k)
            {
                k ^= k >> 16;
                k *= 0x85ebca6b;
                k ^= k >> 13;
                k *= 0xc2b2ae35;
                k ^= k >> 16;
                return k % (uint)hashtable.Length;
            }

            int AllocateChunk(int3 coords)
            {
                int key = coords.x + coords.y * chunkGridResolution.x + coords.z * chunkGridResolution.x * chunkGridResolution.y;
                uint slot = hash((uint)key);

                keyvalue* arr = (keyvalue*)hashtable.GetUnsafePtr();
                int* dispatch = (int*)dispatchBuffer.GetUnsafePtr();

                for (int i = 0; i < hashtable.Length; ++i) // at most, check the entire table.
                {
                    int prev = Interlocked.CompareExchange(ref arr[(int)slot].key, key, -1); 

                    // allocate new chunk:
                    if (prev == -1) 
                    {
                        arr[(int)slot].handle = Interlocked.Add(ref dispatch[0],1) - 1;
                        chunkCoords[hashtable[(int)slot].handle] = coords;
                        return hashtable[(int)slot].handle;
                    }
                    // could not allocate chunk, since it already exists.
                    else if (prev == key) 
                    {
                        return -1;
                    }
                    // collision, try next slot.
                    else 
                        slot = (slot + 1) % (uint)hashtable.Length;
                }
                return -1; // could not allocate chunk, not enough space.
            }

            public void Execute(int i)
            {
                int p = particleIndices[firstParticle + i];

                //in 3D, only particles near the surface should spawn chunks:
                if (principalRadii[p].w <= 0.5f /*||
                    (parameters.mode == Oni.SolverParameters.Mode.Mode3D &&
                    math.dot(normals[p], normals[p]) < 0.0001f)*/)
                    return;

                // expand aabb by voxel size, since boundary voxels (at a chunks' 0 X/Y/Z) can't be triangulated.
                float radius = fluidMaterial[p].x + voxelSize;

                // calculate particle chunk span.
                float chunkSize = chunkResolution * voxelSize;
                int3 minCell = (int3)math.floor((positions[p].xyz - radius - chunkGridOrigin) / chunkSize);
                int3 maxCell = (int3)math.floor((positions[p].xyz + radius - chunkGridOrigin) / chunkSize);

                if (parameters.mode == Oni.SolverParameters.Mode.Mode2D)
                    minCell[2] = maxCell[2] = 0;

                for (int x = minCell[0]; x <= maxCell[0]; ++x)
                {
                    for (int y = minCell[1]; y <= maxCell[1]; ++y)
                    {
                        for (int z = minCell[2]; z <= maxCell[2]; ++z)
                        {
                            AllocateChunk(new int3(x, y, z));
                        }
                    }
                }
            }
        }

        /*  
        *  y         z
        *  ^        /     
        *  |
        *    6----7
        *   /|   /|
        *  2----3 |
        *  | 4--|-5
        *  |/   |/
        *  0----1   --> x
        * 
        */

        [BurstCompile]
        struct BuildGrid : IJob
        {
            [ReadOnly] public NativeArray<int> particleIndices;
            [ReadOnly] public NativeArray<float4> radii;
            [ReadOnly] public NativeArray<float4> positions;
            [ReadOnly] public NativeArray<float4> fluidMaterial;
            [ReadOnly] public NativeArray<BurstAabb> solverBounds;

            public NativeMultilevelGrid<int> grid;

            [ReadOnly] public Oni.SolverParameters parameters;
            [ReadOnly] public int firstParticle;
            [ReadOnly] public int particleCount;

            public void Execute()
            {
                grid.Clear();

                for (int p = 0; p < particleCount; ++p)
                {
                    int i = particleIndices[firstParticle + p];

                    if (radii[i].w <= 0.5f) // skip inactive particles.
                        continue;

                    int level = NativeMultilevelGrid<int>.GridLevelForSize(fluidMaterial[i].x);
                    float cellSize = NativeMultilevelGrid<int>.CellSizeOfLevel(level);
                    int4 cellCoord = new int4(GridHash.Quantize(positions[i].xyz - solverBounds[0].min.xyz, cellSize), level);

                    // if the solver is 2D, project to the z = 0 cell.
                    if (parameters.mode == Oni.SolverParameters.Mode.Mode2D) cellCoord[2] = 0;

                    // add to new cell:
                    int cellIndex = grid.GetOrCreateCell(cellCoord);
                    var newCell = grid.usedCells[cellIndex];
                    newCell.Add(i);
                    grid.usedCells[cellIndex] = newCell;
                }
            }
        }

        [BurstCompile]
        unsafe struct SampleSDF : IJobParallelForDefer
        {
            [ReadOnly] public NativeMultilevelGrid<int> grid;

            [DeallocateOnJobCompletion]
            [ReadOnly] public NativeArray<int> gridLevels;

            [ReadOnly] public NativeArray<int3> chunkCoords;
            [ReadOnly] public NativeArray<float4> positions;
            [ReadOnly] public NativeArray<float4> velocities;
            [ReadOnly] public NativeArray<float4> angularVelocities;
            [ReadOnly] public NativeArray<float4> principalRadii;
            [ReadOnly] public NativeArray<float4> fluidData;
            [ReadOnly] public NativeArray<float4> fluidMaterial;
            [ReadOnly] public NativeArray<quaternion> orientations;
            [ReadOnly] public NativeArray<float4> colors;

            [NativeDisableParallelForRestriction] public NativeArray<float4> verts;
            [NativeDisableParallelForRestriction] public NativeArray<float4> voxelVelocities;

            [ReadOnly] public Poly6Kernel densityKernel;
            [ReadOnly] public float voxelSize;
            [ReadOnly] public int currentBatch;
            [ReadOnly] public float3 chunkGridOrigin;
            [ReadOnly] public NativeArray<BurstAabb> solverBounds;

            [ReadOnly] public float isosurface;

            [ReadOnly] public Oni.SolverParameters parameters;

            private int voxelsInChunk;

            public void Initialize()
            {
                voxelsInChunk = (int)math.pow(chunkResolution, 3 - (int)parameters.mode); // 64 voxels in 3D, 16 in 2D.
            }

            public void Execute(int i)
            {
                // calculate chunk index:
                int chunkIndex = i;
                int firstVoxel = chunkIndex * voxelsInChunk;

                // reset vertices:
                float* w = stackalloc float[voxelsInChunk];
                for (int s = 0; s < voxelsInChunk; ++s)
                {
                    verts[firstVoxel + s] = float4.zero;
                    voxelVelocities[firstVoxel + s] = float4.zero;
                    w[s] = isosurface;
                }

                float3 minCornerCoords = chunkGridOrigin + (float3)chunkCoords[chunkIndex] * chunkResolution * voxelSize;
                float3 maxCornerCoords = minCornerCoords + voxelSize * (chunkResolution - 1);

                for (int l = 0; l < gridLevels.Length; ++l)
                {
                    float cellSize = NativeMultilevelGrid<int>.CellSizeOfLevel(gridLevels[l]);

                    int4 minCell = (int4)math.floor(new float4(minCornerCoords - solverBounds[0].min.xyz, 0) / cellSize) - 1;
                    int4 maxCell = (int4)math.floor(new float4(maxCornerCoords - solverBounds[0].min.xyz, 0) / cellSize) + 1;

                    if (parameters.mode == Oni.SolverParameters.Mode.Mode2D)
                        minCell[2] = maxCell[2] = 0;

                    for (int x = minCell[0]; x <= maxCell[0]; ++x)
                    {
                        for (int y = minCell[1]; y <= maxCell[1]; ++y)
                        {
                            for (int z = minCell[2]; z <= maxCell[2]; ++z)
                            {
                                if (grid.TryGetCellIndex(new int4(x, y, z, gridLevels[l]), out int cellIndex))
                                {
                                    int cellLength = grid.usedCells[cellIndex].Length;

                                    for (int s = 0; s < voxelsInChunk; ++s)
                                    {
                                        float3 samplePos;
                                        // calculate sampling position:
                                        if (parameters.mode == Oni.SolverParameters.Mode.Mode2D)
                                            samplePos = minCornerCoords + (float3)BurstMath.DecodeMorton2((uint)s) * voxelSize;
                                        else
                                            samplePos = minCornerCoords + (float3)BurstMath.DecodeMorton3((uint)s) * voxelSize;

                                        int vertIndex = firstVoxel + s;

                                        for (int k = 0; k < cellLength; ++k)
                                        {
                                            int n = grid.usedCells[cellIndex][k];

                                            float3 radii = fluidMaterial[n].x * (principalRadii[n].xyz / principalRadii[n].x); 

                                            // calculate vector from sample to particle center:
                                            float3 normal = samplePos - positions[n].xyz;
                                            if (parameters.mode == Oni.SolverParameters.Mode.Mode2D)
                                                normal[2] = 0;

                                            // only update distance if within anisotropic kernel radius:
                                            float maxDistance = radii.x + voxelSize * 1.42f;
                                            float r = math.dot(normal, normal);
                                            if (r <= maxDistance * maxDistance)
                                            {
                                                normal = math.mul(math.conjugate(orientations[n]), normal.xyz) / radii;
                                                float d = math.length(normal) * radii.x;

                                                // scale by volume (1 / normalized density)
                                                float weight = (1 / fluidData[n].x) * densityKernel.W(d, radii.x);

                                                w[s] -= weight;

                                                float w2 = 1 - math.saturate(r / (radii.x * radii.x));
                                                verts[vertIndex] += new float4(colors[n].xyz * w2, w2);
                                                voxelVelocities[vertIndex] += new float4(velocities[n].xyz, (math.asuint(angularVelocities[n].w) & 0x0000ffff) / 65535f) * w2;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                for (int s = 0; s < voxelsInChunk; ++s)
                {
                    float4 v = verts[firstVoxel + s];

                    voxelVelocities[firstVoxel + s] /= v.w;

                    v.y = BurstMath.PackFloatRGBA(v/v.w); 
                    v.x = w[s];
                    verts[firstVoxel + s] = v;
                }
            }
        }

        [BurstCompile]
        unsafe struct CalculateSurface : IJobParallelForDefer
        {
            [ReadOnly] public NativeArray<keyvalue> hashtable;
            [ReadOnly] public NativeArray<int3> chunkCoords;
            [ReadOnly] public NativeArray<int2> edges;
            [ReadOnly] public NativeArray<int> edgeTable;

            public NativeArray<int> voxelToVertex;
            [ReadOnly] public NativeArray<float4> voxelVelocities;
            [NativeDisableParallelForRestriction] public NativeArray<int> dispatchBuffer;
            [NativeDisableParallelForRestriction] public NativeArray<float4> verts;
            [NativeDisableParallelForRestriction] public NativeArray<float4> outputVerts;
            [NativeDisableParallelForRestriction] public NativeArray<float4> outputColors;
            [NativeDisableParallelForRestriction] public NativeArray<float4> outputVelocities;

            [ReadOnly] public float voxelSize;
            [ReadOnly] public float bevel;
            [ReadOnly] public float3 chunkGridOrigin;
            [ReadOnly] public int3 chunkGridResolution;
            [ReadOnly] public NativeArray<BurstAabb> solverBounds;

            [ReadOnly] public int descentIterations;
            [ReadOnly] public float descentSpeed;
            [ReadOnly] public float descentIsosurface;
            [ReadOnly] public float isosurface;
            [ReadOnly] public Oni.SolverParameters parameters;

            private int voxelsInChunk;
            private int verticesPerVoxel;
            private float3 dimensionMask;

            public void Initialize()
            {
                voxelsInChunk = (int)math.pow(chunkResolution, 3 - (int)parameters.mode); // 64 voxels in 3D, 16 in 2D.
                verticesPerVoxel = 8 / ((int)parameters.mode + 1); // 8 vertices in 3D, 4 in 2D.
                dimensionMask = parameters.mode == Oni.SolverParameters.Mode.Mode2D ? new float3(1, 1, 0) : new float3(1, 1, 1);
            }

            static readonly float3[] corners =
            {
                new float3(0, 0, 0),
                new float3(1, 0, 0),
                new float3(0, 1, 0),
                new float3(1, 1, 0),
                new float3(0, 0, 1),
                new float3(1, 0, 1),
                new float3(0, 1, 1),
                new float3(1, 1, 1) 
            };

            uint hash(uint k)
            {
                k ^= k >> 16;
                k *= 0x85ebca6b;
                k ^= k >> 13;
                k *= 0xc2b2ae35;
                k ^= k >> 16;
                return k % (uint)hashtable.Length;
            }

            int LookupChunk(int3 coords)
            {
                int key = coords.x + coords.y * chunkGridResolution.x + coords.z * chunkGridResolution.x * chunkGridResolution.y;
                uint slot = hash((uint)key);

                for (int i = 0; i < hashtable.Length; ++i) // at most, check the entire table.
                {
                    if (hashtable[(int)slot].key == key)
                    {
                        return hashtable[(int)slot].handle;
                    }
                    if (hashtable[(int)slot].key == -1)
                    {
                        return -1;
                    }

                    slot = (slot + 1) % (uint)hashtable.Length;
                }
                return -1;
            }

            int GetVoxelIndex(int3 chunkCrds, int3 voxelCoords)
            {
                bool3 mask = voxelCoords == chunkResolution;
                int chunk = LookupChunk(chunkCrds + (int3)mask);

                return chunk == -1 ? -1 : chunk * voxelsInChunk + voxelCoordToOffset(voxelCoords * (int3)!mask, parameters.mode);
            }

            float EvaluateSDF(float4 distancesA, float4 distancesB, in float3 nPos, out float3 normal)
            {
                // trilinear interpolation of distance:
                float4 x = distancesA + (distancesB - distancesA) * nPos[0];
                float2 y = x.xy + (x.zw - x.xy) * nPos[1];

                // gradient estimation:
                // x == 0
                float2 a = distancesA.xy + (distancesA.zw - distancesA.xy) * nPos[1];
                float x0 = a[0] + (a[1] - a[0]) * nPos[2];

                // x == 1
                a = distancesB.xy + (distancesB.zw - distancesB.xy) * nPos[1];
                float x1 = a[0] + (a[1] - a[0]) * nPos[2];

                // y == 0
                float y0 = x[0] + (x[1] - x[0]) * nPos[2];

                // y == 1
                float y1 = x[2] + (x[3] - x[2]) * nPos[2];

                normal = math.normalize(new float3(x1 - x0, y1 - y0, y[1] - y[0]));
                return y[0] + (y[1] - y[0]) * nPos[2];
            }

            public void Execute(int i)
            {
                // initialize voxel with invalid vertex:
                voxelToVertex[i] = -1;

                // calculate chunk index:
                int chunkIndex = i / voxelsInChunk;

                // get offset of voxel within chunk:
                int voxelOffset = i - chunkIndex * voxelsInChunk;
                int3 voxelCoords;

                if (parameters.mode == Oni.SolverParameters.Mode.Mode2D)
                    voxelCoords = (int3)BurstMath.DecodeMorton2((uint)voxelOffset);
                else
                    voxelCoords = (int3)BurstMath.DecodeMorton3((uint)voxelOffset);

                // get samples at voxel corners:
                float * samples = stackalloc float[8];
                float4* vcolor = stackalloc float4[8];
                float4* vvelo = stackalloc float4[8];
                int cornerMask = 0;

                // 8 vertices in 3D, 4 in 2D.
                for (int j = 0; j < verticesPerVoxel; ++j)
                {
                    int v = GetVoxelIndex(chunkCoords[chunkIndex], voxelCoords + (int3)corners[j]);

                    if (v == -1)
                        return;

                    samples[j] = verts[v].x;
                    vcolor[j] = BurstMath.UnpackFloatRGBA(verts[v].y);
                    vvelo[j] = voxelVelocities[v];
                    cornerMask |= samples[j] >= 0 ? (1 << j) : 0;
                }

                // store cornerMask in the upper half of the voxel data.
                // we need to access it using a reference since reading/writing the entire float4
                // from different threads (see below) leads to race condition.
                var vptr = (float4*)verts.GetUnsafePtr();
                vptr[i].z = cornerMask;

                // if the voxel does not intersect the surface, return:
                if ((parameters.mode == Oni.SolverParameters.Mode.Mode2D && cornerMask == 0xf) ||
                    (parameters.mode == Oni.SolverParameters.Mode.Mode3D && (cornerMask == 0 || cornerMask == 0xff)))
                    return;

                int edgeMask = edgeTable[cornerMask];

                // calculate vertex position using edge crossings:
                float3 normalizedPos = new float3(0, 0, 0);
                float4 color = float4.zero;
                float4 velocity = float4.zero;
                int intersections = 0;
                for (int j = 0; j < edges.Length; ++j)
                {
                    if ((edgeMask & (1 << j)) == 0)
                        continue;

                    int2 e = edges[j];
                    float t = -samples[e.x] / (samples[e.y] - samples[e.x]);

                    normalizedPos += math.lerp(corners[e.x], corners[e.y], t);
                    color += math.lerp(vcolor[e.x], vcolor[e.y], t);
                    velocity += math.lerp(vvelo[e.x], vvelo[e.y], t);
                    intersections++;
                }

                // intersections will always be > 0 in 3D.
                if (intersections > 0)
                {
                    normalizedPos /= intersections;
                    color /= intersections;
                    velocity /= intersections;
                }
                else // inner vertex in 2D.
                {
                    normalizedPos = new float3(0.5f, 0.5f, -bevel);
                    color = vcolor[0];
                    velocity = vvelo[0];
                }

                float4 distancesA = new float4(samples[0], samples[4], samples[2], samples[6]);
                float4 distancesB = new float4(samples[1], samples[5], samples[3], samples[7]);

                // gradient descent:
                float3 normal;
                for (int k = 0; k < descentIterations; ++k)
                {
                    float d = EvaluateSDF(distancesA, distancesB, normalizedPos, out normal);
                    normalizedPos -= descentSpeed * normal * (d + isosurface + descentIsosurface);
                }

                // final normal evaluation:
                EvaluateSDF(distancesA, distancesB, normalizedPos, out normal);

                // modify normal in 2D mode:
                if (parameters.mode == Oni.SolverParameters.Mode.Mode2D)
                    normal = math.lerp(new float3(0, 0, -1), new float3(normal.xy, -normal.z), bevel); // no bevel, flat normals

                // Append vertex:
                int* dispatch = (int*)dispatchBuffer.GetUnsafePtr();
                voxelToVertex[i] = Interlocked.Add(ref dispatch[1], 1) - 1;

                // write voxel index for this vertex (no race condition thanks to using raw ptr, see above).
                vptr[voxelToVertex[i]].w = i;

                float3 voxelCorner = chunkGridOrigin + (float3)(chunkCoords[chunkIndex] * chunkResolution + voxelCoords) * voxelSize;
                outputVerts[voxelToVertex[i]] = new float4(voxelCorner * dimensionMask + normalizedPos * voxelSize, BurstMath.OctEncode(normal));
                outputColors[voxelToVertex[i]] = color;
                outputVelocities[voxelToVertex[i]] = velocity;
            }
        }

        [BurstCompile]
        unsafe struct Triangulate : IJobParallelForDefer
        {
            [ReadOnly] public NativeArray<keyvalue> hashtable;
            [ReadOnly] public NativeArray<int> voxelToVertex;
            [ReadOnly] public NativeArray<int3> chunkCoords;
            [ReadOnly] public NativeArray<int> edgeTable;

            [ReadOnly] public NativeArray<float4> verts;
            [NativeDisableParallelForRestriction] public NativeArray<float4> outputVerts;
            [NativeDisableParallelForRestriction] public NativeArray<int> dispatchBuffer;
            [NativeDisableParallelForRestriction] public NativeArray<int> quads;
            [NativeDisableParallelForRestriction] public NativeArray<int> vertexAdjacency;

            [ReadOnly] public int3 chunkGridResolution;
            [ReadOnly] public Oni.SolverParameters parameters;

            private int voxelsInChunk;
            private int quadCount;
            private int adjacentCount;

            public void Initialize()
            {
                voxelsInChunk = (int)math.pow(chunkResolution, 3 - (int)parameters.mode); // 64 voxels in 3D, 16 in 2D.
                quadCount = 3 - (int)parameters.mode * 2; // 3 quads in 3D, 1 in 2D.
                adjacentCount = 6 - (int)parameters.mode * 2; // 6 adjacent voxels in 3D, 4 in 2D.
            }

            static readonly int3[] quadNeighborIndices =
            {
                new int3(1, 3, 2), // x   
                new int3(4, 5, 1), // y   
                new int3(2, 6, 4), // z  
            };

            static readonly int3[] quadWindingOrder = {
                new int3(0, 1 ,2),
                new int3(2, 1 ,0)
            };

            static readonly int3[] corners =
            {
                new int3(0, 0, 0),
                new int3(1, 0, 0),
                new int3(0, 1, 0),
                new int3(1, 1, 0),
                new int3(0, 0, 1),
                new int3(1, 0, 1), 
                new int3(0, 1, 1),
                new int3(1, 1, 1)
            };

            static readonly int4[] faceNeighborEdges =
            {
                new int4(0,1,5,6),
                new int4(0,2,7,8),
                new int4(4,5,9,11),
                new int4(3,8,9,10),
                new int4(6,7,10,11),
                new int4(1,2,3,4)
            };

            uint hash(uint k)
            {
                k ^= k >> 16;
                k *= 0x85ebca6b;
                k ^= k >> 13;
                k *= 0xc2b2ae35;
                k ^= k >> 16;
                return k % (uint)hashtable.Length;
            }

            int LookupChunk(int3 coords)
            {
                int key = coords.x + coords.y * chunkGridResolution.x + coords.z * chunkGridResolution.x * chunkGridResolution.y;
                uint slot = hash((uint)key);

                for (int i = 0; i < hashtable.Length; ++i) // at most, check the entire table.
                {
                    if (hashtable[(int)slot].key == key)
                    {
                        return hashtable[(int)slot].handle;
                    }
                    if (hashtable[(int)slot].key == -1)
                    {
                        return -1;
                    }

                    slot = (slot + 1) % (uint)hashtable.Length;
                }
                return -1;
            }

            int GetVoxelIndex(int3 chunkCrds, int3 voxelCoords)
            {
                bool3 b = voxelCoords < 0;
                int3 r = voxelCoords / chunkResolution;
                int3 mask = new int3(b.x ? -1 : r.x, b.y ? -1 : r.y, b.z ? -1 : r.z); 
                int chunk = LookupChunk(chunkCrds + mask);

                return chunk == -1 ? -1 : chunk * voxelsInChunk + voxelCoordToOffset((int3)BurstMath.nfmod(voxelCoords,chunkResolution), parameters.mode);
            }

            bool isSurfaceVertex(int cornerMask)
            {
                return cornerMask != 0 && cornerMask != 0xff;
            }

            public void Execute(int v0)
            {
                // get index of the voxel that spawned this vertex:
                int i = (int)verts[v0].w;

                // calculate chunk index and look up coordinates:
                int chunkIndex = i / voxelsInChunk;

                // get offset of voxel within chunk:
                int voxelOffset = i - chunkIndex * voxelsInChunk;
                int3 voxelCoords;

                if (parameters.mode == Oni.SolverParameters.Mode.Mode2D)
                    voxelCoords = (int3)BurstMath.DecodeMorton2((uint)voxelOffset);
                else
                    voxelCoords = (int3)BurstMath.DecodeMorton3((uint)voxelOffset);

                int cornerMask = (int)verts[i].z;
                int edgeMask = edgeTable[cornerMask];

                // get winding order using last bit of cornermask, which indicates corner sign:
                // in 2D, cornerMask >> 7 is always 0, so we get the second winding order.
                int3 windingOrder = (cornerMask >> 7) != 0 ? quadWindingOrder[0] : quadWindingOrder[1];

                // Retrieve adjacent voxels:
                bool currentSurface = isSurfaceVertex(cornerMask);
                int* adjacent = stackalloc int[6];
                for (int j = 0; j < adjacentCount; ++j)
                    adjacent[j] = GetVoxelIndex(chunkCoords[chunkIndex], voxelCoords + corners[j + 1]);

                //iterate over all potential quads
                for (int j = 0; j < quadCount; ++j)
                {
                    // if the edge is not crossing the surface, skip it (3D only)
                    if (parameters.mode == Oni.SolverParameters.Mode.Mode3D && (edgeMask & (1 << j)) == 0)
                        continue;

                    // calculate final neighbor indices:
                    int3 neighbors = new int3(quadNeighborIndices[j][windingOrder[0]] - 1,
                                              quadNeighborIndices[j][windingOrder[1]] - 1,
                                              quadNeighborIndices[j][windingOrder[2]] - 1);

                    // get vertex indices for all voxels involved: 
                    int v1 = voxelToVertex[adjacent[neighbors[0]]];
                    int v2 = voxelToVertex[adjacent[neighbors[1]]];
                    int v3 = voxelToVertex[adjacent[neighbors[2]]];

                    // if any of the vertices is invalid, skip the quad:
                    if (v1 == -1 || v2 == -1 || v3 == -1)
                        continue;

                    // append a new quad:
                    int* dispatch = (int*)dispatchBuffer.GetUnsafePtr();
                    int baseIndex = Interlocked.Add(ref dispatch[2], 1) - 1;
                    baseIndex *= 6;

                    // flip edge if necessary, to always use the shortest diagonal:
                    float3 diag1 = outputVerts[v0].xyz - outputVerts[v2].xyz;
                    float3 diag2 = outputVerts[v1].xyz - outputVerts[v3].xyz;

                    if (math.dot(diag1, diag1) > math.dot(diag2, diag2) * 1.1f)
                    {
                        quads[baseIndex] = v1;
                        quads[baseIndex + 1] = v2;
                        quads[baseIndex + 2] = v3;

                        quads[baseIndex + 3] = v0;
                        quads[baseIndex + 4] = v1;
                        quads[baseIndex + 5] = v3; 
                    }
                    else
                    {
                        quads[baseIndex] = v0;
                        quads[baseIndex + 1] = v1;
                        quads[baseIndex + 2] = v2;

                        quads[baseIndex + 3] = v3;
                        quads[baseIndex + 4] = v0;
                        quads[baseIndex + 5] = v2;
                    }
                }

                // Move adjacent voxel in Z axis to last position, so that 2D adjacent voxels are the first 4.
                adjacent[5] = adjacent[3];
                adjacent[2] = GetVoxelIndex(chunkCoords[chunkIndex], voxelCoords + new int3(0, -1, 0));
                adjacent[3] = GetVoxelIndex(chunkCoords[chunkIndex], voxelCoords + new int3(-1, 0, 0));
                adjacent[4] = GetVoxelIndex(chunkCoords[chunkIndex], voxelCoords + new int3(0, 0, -1));

                // initialize vertex adjacency to INVALID.
                for (int j = 0; j < 6; ++j)
                    vertexAdjacency[v0 * 6 + j] = -1;

                // Determine adjacent surface voxels for smoothing:
                bool isAdjacent;
                for (int j = 0; j < adjacentCount; ++j)
                {
                    if (adjacent[j] != -1)
                    {
                        int4 faceMask = new int4(1 << faceNeighborEdges[j].x,
                                                 1 << faceNeighborEdges[j].y,
                                                 1 << faceNeighborEdges[j].z,
                                                 1 << faceNeighborEdges[j].w);

                        // adjacent if this does not intersect the surface or both intersect the surface.
                        isAdjacent = (edgeMask == 0 || edgeTable[(int)verts[adjacent[j]].z] != 0) &&

                                     // in 3D mode, it should also intersect any of the face edges to be considered adjacent:
                                     (parameters.mode == Oni.SolverParameters.Mode.Mode2D || math.any(edgeMask & faceMask));

                        vertexAdjacency[v0 * 6 + j] = isAdjacent ? voxelToVertex[adjacent[j]] : -1;
                    }
                }
            }
        }

        [BurstCompile]
        unsafe struct Smoothing : IJobParallelForDefer
        {
            [ReadOnly] public NativeArray<int> vertexAdjacency;

            [ReadOnly] public NativeArray<float4> verts;
            [NativeDisableParallelForRestriction] public NativeArray<float4> outputVerts;

            [ReadOnly] public float smoothingFactor;

            public void Execute(int i)
            {
                float3 n = BurstMath.OctDecode(verts[i].w);

                float4 coord = new float4(verts[i].xyz, 1);
                float4 norm = new float4(n, 1);

                for (int j = 0; j < 6; ++j)
                {
                    int v = vertexAdjacency[i * 6 + j];
                    if (v != -1)
                    {
                        coord += new float4(verts[v].xyz, 1);
                        norm += new float4(BurstMath.OctDecode(verts[v].w), 1);
                    }
                }

                norm /= coord.w;
                coord.xyz /= coord.w;

                float3 p = math.lerp(verts[i].xyz, coord.xyz, smoothingFactor);
                n = math.normalize(math.lerp(n, norm.xyz, smoothingFactor));

                outputVerts[i] = new float4(p, BurstMath.OctEncode(n));
            }
        }

    }
}
#endif
