
using UnityEngine;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine.Rendering;

namespace Obi
{

    public class IndirectRenderBatch<T> : IRenderBatch where T : struct
    {
        public ObiFluidRenderingPass renderPass { get; set; }

        public int firstRenderer;
        public int rendererCount = 1;

        public int firstParticle;
        public int particleCount;

        public Mesh mesh;
        public NativeArray<Vector4> vertexBuffer;
        public NativeArray<Vector4> colorBuffer;
        public NativeArray<Vector4> velocityBuffer;
        public NativeArray<int> indexBuffer;

        public GraphicsBuffer gpuVertexBuffer;
        public GraphicsBuffer gpuColorBuffer;
        public GraphicsBuffer gpuVelocityBuffer;
        public GraphicsBuffer gpuIndexBuffer;
        public GraphicsBuffer indirectCommandBuffer;
        public GraphicsBuffer vertexDispatchBuffer;

        public int vertexCount;
        public int triangleCount;

        private uint[] clearCommandBuffer = { 0, 0, 0, 0, 0 };

        public IndirectRenderBatch(int vertexCount, int triangleCount, bool compute = false)
        {
            this.vertexCount = vertexCount;
            this.triangleCount = triangleCount;

            mesh = new Mesh();

            mesh.SetVertexBufferParams(vertexCount, layout);

            if (!compute)
            {
                vertexBuffer = new NativeArray<Vector4>(vertexCount, Allocator.Persistent);
                colorBuffer = new NativeArray<Vector4>(vertexCount, Allocator.Persistent);
                velocityBuffer = new NativeArray<Vector4>(vertexCount, Allocator.Persistent);
                indexBuffer = new NativeArray<int>(triangleCount * 3, Allocator.Persistent);
            }
            else
            {
                gpuVertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vertexCount, Marshal.SizeOf<T>());
                gpuColorBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vertexCount, sizeof(float) * 4);
                gpuVelocityBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vertexCount, sizeof(float) * 4);
                gpuIndexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, triangleCount * 3, sizeof(int));
                indirectCommandBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
                vertexDispatchBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 5, sizeof(uint));
            }
        }

        public void Clear()
        {
            indirectCommandBuffer.SetData(clearCommandBuffer);
        }

        public void Initialize(bool compute = false)
        {
            renderPass?.UpdateFluidMaterial(compute);
        }

        public void Dispose()
        {
            if (vertexBuffer.IsCreated)
                vertexBuffer.Dispose();
            if (colorBuffer.IsCreated)
                colorBuffer.Dispose();
            if (velocityBuffer.IsCreated)
                velocityBuffer.Dispose();
            if (indexBuffer.IsCreated)
                indexBuffer.Dispose();

            gpuVertexBuffer?.Dispose();
            gpuColorBuffer?.Dispose();
            gpuVelocityBuffer?.Dispose();
            gpuIndexBuffer?.Dispose();
            indirectCommandBuffer?.Dispose();
            vertexDispatchBuffer?.Dispose();

            gpuVertexBuffer = null;
            gpuColorBuffer = null;
            gpuVelocityBuffer = null;
            gpuIndexBuffer = null;
            indirectCommandBuffer = null;
            vertexDispatchBuffer = null;

            GameObject.DestroyImmediate(mesh);
        }

        public bool TryMergeWith(IRenderBatch other)
        {
            var pbatch = other as IndirectRenderBatch<T>;
            if (pbatch != null)
            {
                if (CompareTo(pbatch) == 0)
                {
                    rendererCount += pbatch.rendererCount;
                    particleCount += pbatch.particleCount;
                    return true;
                }
            }
            return false;
        }

        public int CompareTo(IRenderBatch other)
        {
            var pbatch = other as IndirectRenderBatch<T>;
            int result = renderPass.GetInstanceID().CompareTo(pbatch.renderPass.GetInstanceID());
            return result;
        }

        VertexAttributeDescriptor[] layout =
        {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4)
        };

        struct BakedVertex
        {
            public Vector3 pos;
            public Vector3 nrm;
            public Color clr;
        };

        public void BakeMesh(int vertexCount, int triangleCount, ref Mesh bakedMesh)
        {
            // if the data is not available in the CPU (such as when the batch is intended for GPU use),
            // create CPU buffers and read back data from the GPU:
            if (vertexDispatchBuffer != null)
            {
                vertexBuffer = new NativeArray<Vector4>(vertexCount, Allocator.Persistent);
                colorBuffer = new NativeArray<Vector4>(vertexCount, Allocator.Persistent);
                indexBuffer = new NativeArray<int>(triangleCount * 3, Allocator.Persistent);
                AsyncGPUReadback.RequestIntoNativeArray(ref vertexBuffer, gpuVertexBuffer, vertexCount * 16, 0).WaitForCompletion();
                AsyncGPUReadback.RequestIntoNativeArray(ref colorBuffer, gpuColorBuffer, vertexCount * 16, 0).WaitForCompletion();
                AsyncGPUReadback.RequestIntoNativeArray(ref indexBuffer, gpuIndexBuffer, triangleCount * 3 * 4, 0).WaitForCompletion();
            }

            var decodedVertices = new NativeArray<BakedVertex>(vertexCount, Allocator.Persistent);

            for (int v = 0; v < vertexCount; ++v)
            {
                decodedVertices[v] = new BakedVertex
                {
                    pos = vertexBuffer[v],
                    nrm = ObiUtils.OctDecode(vertexBuffer[v].w), // decode octahedral-encoded normals:
                    clr = colorBuffer[v]
                };
            }

            bakedMesh.Clear();

            bakedMesh.SetVertexBufferParams(vertexCount, layout);
            bakedMesh.SetVertexBufferData(decodedVertices, 0, 0, vertexCount, 0, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

            bakedMesh.SetIndexBufferParams(triangleCount * 3, IndexFormat.UInt32);
            bakedMesh.SetIndexBufferData(indexBuffer, 0, 0, triangleCount * 3, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

            bakedMesh.subMeshCount = 1;
            SubMeshDescriptor subMeshDescriptor = new SubMeshDescriptor();
            subMeshDescriptor.indexCount = triangleCount * 3; // mesh triangle count.
            bakedMesh.SetSubMesh(0, subMeshDescriptor, MeshUpdateFlags.DontValidateIndices);

            if (vertexDispatchBuffer != null)
            {
                if (vertexBuffer.IsCreated)
                    vertexBuffer.Dispose();
                if (colorBuffer.IsCreated)
                    colorBuffer.Dispose();
                if (indexBuffer.IsCreated)
                    indexBuffer.Dispose();
            }

            decodedVertices.Dispose();
            bakedMesh.RecalculateBounds();

            return;

        }
    }

}