using UnityEngine;
using Obi;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(ObiSoftbody))]
public class CollisionStiffener : MonoBehaviour
{
    [Header("功能配置")]
    [Tooltip("临时距离约束的硬度 (0-1)。")]
    [Range(0f, 1f)]
    public float temporaryStiffness = 1f;

    [Tooltip("预创建的约束池大小。")]
    public int constraintPoolSize = 200;

    [Tooltip("只对拥有此Tag的物体加强碰撞。留空则对所有碰撞生效。")]
    public string colliderTag;

    [Header("可视化配置")]
    public bool enableVisualization = true;
    public Color activeConstraintColor = Color.red;

    private ObiSoftbody softbody;
    private ObiSolver solver;
    private ObiDistanceConstraintsData distanceConstraintsData;

    private List<int> simplexParticlesBuffer = new List<int>();
    private HashSet<int> collidingParticlesInActor = new HashSet<int>();
    private Queue<int> availableConstraintIndicesInPool;
    private Dictionary<Tuple<int, int>, int> activeTemporaryConstraints;
    private List<Tuple<int, int>> constraintsToDeactivate = new List<Tuple<int, int>>();
    
    private Dictionary<int, Color> originalParticleColors;

    private bool isInitialized = false;
    private int poolBatchIndex = -1;

    void Start()
    {
        softbody = GetComponent<ObiSoftbody>();
        if (softbody.solver != null)
        {
            solver = softbody.solver;
            solver.OnCollision += Solver_OnCollision;
            StartCoroutine(Initialize());
        }
    }

    void OnDestroy()
    {
        if (solver != null)
            solver.OnCollision -= Solver_OnCollision;
        
        if (isInitialized && enableVisualization)
            RestoreAllParticleColors();
    }

    private IEnumerator Initialize()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        distanceConstraintsData = softbody.softbodyBlueprint.distanceConstraintsData;
        if (distanceConstraintsData == null)
        {
            Debug.LogError("ObiSoftbody 蓝图中不包含 DistanceConstraintsData!", this);
            yield break;
        }

        var poolBatch = distanceConstraintsData.CreateBatch();
        availableConstraintIndicesInPool = new Queue<int>(constraintPoolSize);
        for (int i = 0; i < constraintPoolSize; i++)
        {
            poolBatch.AddConstraint(new Vector2Int(0, 0), 1f);
            availableConstraintIndicesInPool.Enqueue(i);
        }
        poolBatch.activeConstraintCount = poolBatch.constraintCount;

        distanceConstraintsData.AddBatch(poolBatch);
        poolBatchIndex = distanceConstraintsData.batchCount - 1;

        softbody.SetConstraintsDirty(Oni.ConstraintType.Distance);

        activeTemporaryConstraints = new Dictionary<Tuple<int, int>, int>();
        originalParticleColors = new Dictionary<int, Color>();
        isInitialized = true;
    }

    void LateUpdate()
    {
        if (!isInitialized) return;

        constraintsToDeactivate.Clear();
        foreach (var entry in activeTemporaryConstraints)
        {
            if (!collidingParticlesInActor.Contains(entry.Key.Item1) && !collidingParticlesInActor.Contains(entry.Key.Item2))
            {
                constraintsToDeactivate.Add(entry.Key);
            }
        }

        if (constraintsToDeactivate.Count > 0)
            DeactivateTemporaryConstraints(constraintsToDeactivate);
        
        collidingParticlesInActor.Clear();
    }

    private void Solver_OnCollision(ObiSolver solver, ObiNativeContactList contacts)
    {
        if (!isInitialized || contacts.count == 0) return;
        
        Debug.Log($"<color=cyan>【LOG 1】: OnCollision 事件触发, 接触点数量: {contacts.count}</color>");

        collidingParticlesInActor.Clear();

        for (int i = 0; i < contacts.count; ++i)
        {
            Oni.Contact contact = contacts[i];
            
            // --- 关键修正：交换 bodyA 和 bodyB 的角色 ---
            // 我们现在假设 bodyA 是碰撞体，bodyB 是软体的单纯形
            var otherColliderHandle = ObiColliderWorld.GetInstance().colliderHandles[contact.bodyA];
            if (!otherColliderHandle.owner) continue;

            // --- 新增诊断日志 ---
            Debug.Log($"  <color=grey>接触点 {i}: Simplex Index (bodyB) = {contact.bodyB}, Collider Handle Index (bodyA) = {contact.bodyA}</color>");

            if (!string.IsNullOrEmpty(colliderTag) && !otherColliderHandle.owner.CompareTag(colliderTag))
                continue;

            simplexParticlesBuffer.Clear();
            GetParticlesFromSimplex(contact.bodyB, simplexParticlesBuffer); // 使用 bodyB 获取粒子

            // --- 新增诊断日志 ---
            if (simplexParticlesBuffer.Count > 0)
                Debug.Log($"  <color=grey>从Simplex {contact.bodyB} 中解析出 {simplexParticlesBuffer.Count} 个粒子。</color>");


            foreach (int particleSolverIndex in simplexParticlesBuffer)
            {
                if (particleSolverIndex < 0 || particleSolverIndex >= solver.particleToActor.Length) continue;

                var pInActor = solver.particleToActor[particleSolverIndex];
                if (pInActor != null && pInActor.actor == softbody)
                {
                    Debug.Log($"<color=yellow>【LOG 2】: 找到碰撞粒子! Actor Index: {pInActor.indexInActor}, Solver Index: {particleSolverIndex}</color>");
                    collidingParticlesInActor.Add(pInActor.indexInActor);
                }
            }
        }
        
        if (collidingParticlesInActor.Count > 0)
        {
            Debug.Log($"<color=orange>【LOG 3】: 准备强化 {collidingParticlesInActor.Count} 个碰撞粒子所在的邻域...</color>");
            StrengthenCollidingNeighborhoods();
        }
    }
    
    private void StrengthenCollidingNeighborhoods()
    {
        bool needsUpload = false;
        var dc = softbody.GetConstraintsByType(Oni.ConstraintType.ShapeMatching) as ObiConstraints<ObiShapeMatchingConstraintsBatch>;
        if (dc == null) return;

        for (int j = 0; j < dc.batchCount; ++j)
        {
            var batch = dc.batches[j] as ObiShapeMatchingConstraintsBatch;
            for (int i = 0; i < batch.activeConstraintCount; i++)
            {
                bool neighborhoodIsColliding = false;
                for (int k = 0; k < batch.numIndices[i]; ++k)
                {
                    int pIndex = batch.particleIndices[batch.firstIndex[i] + k];
                    if (collidingParticlesInActor.Contains(pIndex))
                    {
                        neighborhoodIsColliding = true;
                        break;
                    }
                }

                if (neighborhoodIsColliding)
                {
                    // 【调试日志 4】
                    Debug.Log($"<color=magenta>【LOG 4】: 找到碰撞邻域 (ShapeMatching约束 {i}), 准备激活内部约束。</color>");
                    int centerParticleIndex = batch.particleIndices[batch.firstIndex[i]];
                    for (int k = 1; k < batch.numIndices[i]; ++k)
                    {
                        int spokeParticleIndex = batch.particleIndices[batch.firstIndex[i] + k];
                        if (ActivateTemporaryConstraint(centerParticleIndex, spokeParticleIndex))
                            needsUpload = true;
                    }
                }
            }
        }

        if (needsUpload)
        {
            var solverConstraints = solver.GetConstraintsByType(Oni.ConstraintType.Distance) as ObiDistanceConstraintsData;
            var solverPoolBatch = solverConstraints.batches[poolBatchIndex];
            
            solverPoolBatch.particleIndices.Upload();
            solverPoolBatch.restLengths.Upload();
            solverPoolBatch.stiffnesses.Upload();
            
            if (enableVisualization)
                solver.colors.Upload();
        }
    }

    private bool ActivateTemporaryConstraint(int particleIndex1, int particleIndex2)
    {
        Tuple<int, int> key = particleIndex1 < particleIndex2 ? Tuple.Create(particleIndex1, particleIndex2) : Tuple.Create(particleIndex2, particleIndex1);
        if (activeTemporaryConstraints.ContainsKey(key)) return false;
        if (availableConstraintIndicesInPool.Count == 0) 
        {
            Debug.LogWarning("约束池耗尽，无法激活新的临时约束！");
            return false;
        }

        // 【调试日志 5】
        Debug.Log($"<color=green>【LOG 5】: 正在激活约束! 连接 Actor粒子 {particleIndex1} <-> {particleIndex2}。</color>");

        var solverConstraints = solver.GetConstraintsByType(Oni.ConstraintType.Distance) as ObiDistanceConstraintsData;
        var solverPoolBatch = solverConstraints.batches[poolBatchIndex];
        int offset = softbody.solverBatchOffsets[(int)Oni.ConstraintType.Distance][poolBatchIndex];

        int poolIndex = availableConstraintIndicesInPool.Dequeue();
        activeTemporaryConstraints.Add(key, poolIndex);

        int solverIndex = offset + poolIndex;
        int solverP1 = softbody.solverIndices[particleIndex1];
        int solverP2 = softbody.solverIndices[particleIndex2];

        float restLength = Vector3.Distance(solver.positions[solverP1], solver.positions[solverP2]);

        solverPoolBatch.particleIndices[solverIndex * 2] = solverP1;
        solverPoolBatch.particleIndices[solverIndex * 2 + 1] = solverP2;
        solverPoolBatch.restLengths[solverIndex] = restLength;
        solverPoolBatch.stiffnesses[solverIndex] = new Vector2(temporaryStiffness, temporaryStiffness);

        if (enableVisualization)
        {
            if (!originalParticleColors.ContainsKey(solverP1)) originalParticleColors[solverP1] = solver.colors[solverP1];
            if (!originalParticleColors.ContainsKey(solverP2)) originalParticleColors[solverP2] = solver.colors[solverP2];
            solver.colors[solverP1] = activeConstraintColor;
            solver.colors[solverP2] = activeConstraintColor;
        }

        return true;
    }

    // 其他辅助方法与上一版相同...
    private void DeactivateTemporaryConstraints(List<Tuple<int, int>> keysToDeactivate){/*...*/}
    private void RestoreAllParticleColors(){/*...*/}
    private void GetParticlesFromSimplex(int simplexIndex, List<int> outParticles){/*...*/}
}