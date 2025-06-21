using UnityEngine;
using Obi;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 通过动态调节 Pin 约束的柔度（Compliance）来模拟临时约束，以防止穿透。
/// (V8: 参数化调优版 - 暴露所有关键参数以供实验)
/// </summary>
[RequireComponent(typeof(ObiSoftbody))]
public class DynamicPinStiffener : MonoBehaviour
{
    [Header("核心功能配置")]
    [Tooltip("只对拥有此Tag的物体加强碰撞。留空则对所有碰撞生效。")]
    public string colliderTag;

    [Tooltip("预分配的 Pin 约束池大小。应大于场景中可能同时发生的最大碰撞点数。")]
    public int pinPoolSize = 32;

    [Header("触发机制")]
    [Tooltip("只有当粒子穿透碰撞体超过此深度时，才触发Pin约束。设为0则在任何接触时都触发。")]
    [Range(0, 0.1f)]
    public float minPenetrationDepth = 0f;
    
    [Tooltip("是否将约束应用到整个碰撞粒子所在的邻域（ShapeMatching簇），而不仅仅是单个粒子。")]
    public bool pinEntireNeighborhood = false;

    [Header("固定模式与强度")]
    [Tooltip("临时Pin约束的硬度 (0-1)。1为最强。")]
    [Range(0f, 1f)]
    public float pinStiffness = 1f;

    [Tooltip("是否同时约束粒子的朝向。仅当软体启用了Oriented Particles时有效。")]
    public bool constrainOrientation = false;
    
    [Tooltip("约束朝向的硬度 (0-1)。")]
    [Range(0f, 1f)]
    public float orientationStiffness = 1f;
    
    [Header("可视化与调试")]
    public bool enableVisualization = true;
    public Color pinnedParticleColor = Color.cyan;
    public Color neighborhoodParticleColor = Color.yellow;


    // 内部工作变量
    private ObiSoftbody softbody;
    private ObiSolver solver;
    private ObiPinConstraintsData pinConstraintsData;
    private ObiPinConstraintsBatch dynamicPinBatch;
    private ObiShapeMatchingConstraintsData shapeMatchingData;

    private struct DynamicPinInfo
    {
        public int particleSolverIndex;
        public ObiColliderBase collider;
        public Vector3 pinOffset;
    }
    private readonly List<DynamicPinInfo> pinsToApply = new List<DynamicPinInfo>();
    
    private readonly Dictionary<int, Color> originalParticleColors = new Dictionary<int, Color>();
    private readonly HashSet<int> neighborhoodParticlesToColor = new HashSet<int>();

    void OnEnable()
    {
        softbody = GetComponent<ObiSoftbody>();
        softbody.OnBlueprintLoaded += OnBlueprintLoaded;
        if (softbody.isLoaded)
            OnBlueprintLoaded(softbody, softbody.sourceBlueprint);
    }

    void OnDisable()
    {
        softbody.OnBlueprintLoaded -= OnBlueprintLoaded;
        RemoveDynamicBatch();
        RestoreAllParticleColors();
    }
    
    private void OnBlueprintLoaded(ObiActor actor, ObiActorBlueprint blueprint)
    {
        shapeMatchingData = softbody.GetConstraintsByType(Oni.ConstraintType.ShapeMatching) as ObiShapeMatchingConstraintsData;
        SetupDynamicBatch();
        SubscribeToSolver();
    }

    private void SubscribeToSolver()
    {
        if (solver != null) solver.OnCollision -= Solver_OnCollision;
        solver = softbody.solver;
        if (solver != null) solver.OnCollision += Solver_OnCollision;
    }

    private void SetupDynamicBatch()
    {
        pinConstraintsData = softbody.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiPinConstraintsData;

        if (pinConstraintsData == null)
        {
            Debug.LogError("DynamicPinStiffener: 软体上必须启用 'Pin Constraints' 才能工作。", this);
            enabled = false;
            return;
        }

        if (dynamicPinBatch == null)
        {
            dynamicPinBatch = pinConstraintsData.CreateBatch();
            for (int i = 0; i < pinPoolSize; ++i)
                dynamicPinBatch.AddConstraint(-1, null, Vector3.zero, Quaternion.identity, 1f, 1f);
            dynamicPinBatch.activeConstraintCount = pinPoolSize;
        }
    }

    private void RemoveDynamicBatch()
    {
        if (solver != null) solver.OnCollision -= Solver_OnCollision;
        solver = null;

        if (pinConstraintsData != null && dynamicPinBatch != null)
        {
            pinConstraintsData.RemoveBatch(dynamicPinBatch);
            if (softbody.isLoaded) softbody.SetConstraintsDirty(Oni.ConstraintType.Pin);
        }
        dynamicPinBatch = null;
        pinConstraintsData = null;
    }

    void LateUpdate()
    {
        if (!softbody.isLoaded || dynamicPinBatch == null) return;

        bool needsUpdate = false;
        float activePositionalCompliance = 1f - pinStiffness;
        float activeOrientationCompliance = 1f - orientationStiffness;

        // Reset previous frame's active pins to be soft
        for (int i = 0; i < dynamicPinBatch.activeConstraintCount; ++i)
        {
            if (dynamicPinBatch.stiffnesses[i * 2] < 1f)
            {
                dynamicPinBatch.stiffnesses[i * 2] = 1f;
                dynamicPinBatch.stiffnesses[i * 2 + 1] = 1f;
                needsUpdate = true;
            }
        }
        
        // Activate new pins for the current frame
        int pinsToActivateCount = Mathf.Min(pinsToApply.Count, pinPoolSize);
        for (int i = 0; i < pinsToActivateCount; ++i)
        {
            var pinInfo = pinsToApply[i];
            dynamicPinBatch.particleIndices[i] = pinInfo.particleSolverIndex;
            dynamicPinBatch.pinBodies[i] = pinInfo.collider.Handle;
            dynamicPinBatch.colliderIndices[i] = pinInfo.collider.Handle.index;
            dynamicPinBatch.offsets[i] = pinInfo.pinOffset;
            
            dynamicPinBatch.stiffnesses[i * 2] = activePositionalCompliance;
            dynamicPinBatch.stiffnesses[i * 2 + 1] = constrainOrientation ? activeOrientationCompliance : 1f;
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            softbody.SetConstraintsDirty(Oni.ConstraintType.Pin);
        }

        UpdateColors();
        pinsToApply.Clear();
    }

    private void Solver_OnCollision(ObiSolver solver, ObiNativeContactList contacts)
    {
        pinsToApply.Clear();
        neighborhoodParticlesToColor.Clear();
        if (contacts.count == 0) return;

        var processedParticles = new HashSet<int>();
        var collidingParticlesInfo = new List<(int solverIndex, Oni.Contact contact)>();

        // Step 1: Filter contacts and gather initial collision info
        for (int i = 0; i < contacts.count; ++i)
        {
            Oni.Contact contact = contacts[i];
            if (contact.distance > -minPenetrationDepth) continue;

            int particleSolverIndex = GetParticleSolverIndexFromContact(contact);
            if (particleSolverIndex == -1) continue;

            var otherCollider = ObiColliderWorld.GetInstance().colliderHandles[GetColliderIndexFromContact(contact)].owner;
            if (otherCollider == null || !otherCollider.gameObject.activeInHierarchy) continue;
            if (!string.IsNullOrEmpty(colliderTag) && !otherCollider.CompareTag(colliderTag)) continue;
            
            collidingParticlesInfo.Add((particleSolverIndex, contact));
        }

        // Step 2: Process collisions, either individually or by neighborhood
        foreach(var info in collidingParticlesInfo)
        {
            if (pinsToApply.Count >= pinPoolSize) break;
            if (processedParticles.Contains(info.solverIndex)) continue;

            var otherCollider = ObiColliderWorld.GetInstance().colliderHandles[GetColliderIndexFromContact(info.contact)].owner;

            if (pinEntireNeighborhood && shapeMatchingData != null)
            {
                // Pin entire neighborhood
                int particleActorIndex = solver.particleToActor[info.solverIndex].indexInActor;
                int clusterIndex = FindClusterForParticle(particleActorIndex);
                if (clusterIndex != -1)
                {
                    var batch = shapeMatchingData.batches[0]; // Assuming one batch
                    int first = batch.firstIndex[clusterIndex];
                    int num = batch.numIndices[clusterIndex];

                    for (int k = 0; k < num; ++k)
                    {
                        int neighborActorIndex = batch.particleIndices[first + k];
                        int neighborSolverIndex = softbody.solverIndices[neighborActorIndex];
                        if (processedParticles.Contains(neighborSolverIndex) || pinsToApply.Count >= pinPoolSize) continue;
                        
                        // For neighbors, we approximate their pin position based on the original contact.
                        // A more accurate way would be to project them, but this is a good start.
                        Vector3 worldPos = solver.positions[neighborSolverIndex];
                        Vector3 pinOffset = otherCollider.transform.InverseTransformPoint(worldPos);
                        
                        if (float.IsNaN(pinOffset.x)) continue;
                        pinsToApply.Add(new DynamicPinInfo { particleSolverIndex = neighborSolverIndex, collider = otherCollider, pinOffset = pinOffset });
                        processedParticles.Add(neighborSolverIndex);
                        neighborhoodParticlesToColor.Add(neighborSolverIndex);
                    }
                }
            }
            else
            {
                // Pin just the single colliding particle
                Vector3 worldContactPoint = info.contact.pointB; 
                Vector3 pinOffset = otherCollider.transform.InverseTransformPoint(worldContactPoint);
            
                if (float.IsNaN(pinOffset.x)) continue;
                pinsToApply.Add(new DynamicPinInfo { particleSolverIndex = info.solverIndex, collider = otherCollider, pinOffset = pinOffset });
                processedParticles.Add(info.solverIndex);
            }
        }
    }
    
    // --- Helper and Visualization Methods ---

    private int FindClusterForParticle(int particleActorIndex)
    {
        if (shapeMatchingData == null || shapeMatchingData.batchCount == 0) return -1;
        var batch = shapeMatchingData.batches[0];
        for (int i = 0; i < batch.activeConstraintCount; ++i)
        {
            for (int k = 0; k < batch.numIndices[i]; ++k)
            {
                if (batch.particleIndices[batch.firstIndex[i] + k] == particleActorIndex)
                    return i; 
            }
        }
        return -1;
    }

    private void UpdateColors()
    {
        RestoreAllParticleColors();
        if (!enableVisualization) return;

        // Color neighborhood particles first
        foreach (int solverIndex in neighborhoodParticlesToColor)
        {
             if (!originalParticleColors.ContainsKey(solverIndex)) originalParticleColors[solverIndex] = solver.colors[solverIndex];
             solver.colors[solverIndex] = neighborhoodParticleColor;
        }

        // Color directly pinned particles, overriding neighborhood color if necessary
        foreach (var pinInfo in pinsToApply)
        {
            int solverIndex = pinInfo.particleSolverIndex;
            if (!originalParticleColors.ContainsKey(solverIndex)) originalParticleColors[solverIndex] = solver.colors[solverIndex];
            solver.colors[solverIndex] = pinnedParticleColor;
        }

        if (originalParticleColors.Count > 0)
            solver.colors.Upload();
    }

    private void RestoreAllParticleColors()
    {
        if (originalParticleColors.Count > 0 && solver != null)
        {
            foreach (var pair in originalParticleColors)
            {
                if (pair.Key >= 0 && pair.Key < solver.colors.count)
                   solver.colors[pair.Key] = pair.Value;
            }
            solver.colors.Upload();
        }
        originalParticleColors.Clear();
    }

    private int GetParticleSolverIndexFromContact(Oni.Contact contact)
    {
        if (IsParticleFromOurSoftbody(contact.bodyA)) return contact.bodyA;
        if (IsParticleFromOurSoftbody(contact.bodyB)) return contact.bodyB;
        return -1;
    }
    
    private int GetColliderIndexFromContact(Oni.Contact contact)
    {
        return IsParticleFromOurSoftbody(contact.bodyA) ? contact.bodyB : contact.bodyA;
    }

    private bool IsParticleFromOurSoftbody(int particleSolverIndex)
    {
        if (solver == null || particleSolverIndex < 0 || particleSolverIndex >= solver.particleToActor.Length) return false;
        var pInActor = solver.particleToActor[particleSolverIndex];
        return pInActor != null && pInActor.actor == softbody;
    }
}