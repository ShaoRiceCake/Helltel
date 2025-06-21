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

    [Tooltip("只对拥有此Tag的物体加强碰撞。留空则对所有碰撞生效。")]
    public string colliderTag;

    [Header("可视化与调试")]
    public bool enableVisualization = true;
    public Color activeConstraintColor = Color.red;
    [Tooltip("勾选后，将在碰撞时于控制台打印当前激活的临时约束数量。")]
    public bool logConstraintCount = true;

    private ObiSoftbody softbody;
    private ObiSolver solver;

    // --- 补全缺失的成员变量 ---
    private bool isInitialized = false;
    private HashSet<int> collidingParticlesInActor = new HashSet<int>();
    private List<Tuple<int, int>> constraintsToApply = new List<Tuple<int, int>>();
    private Dictionary<Tuple<int, int>, bool> activeTemporaryConstraints = new Dictionary<Tuple<int, int>, bool>();
    private Dictionary<int, Color> originalParticleColors = new Dictionary<int, Color>();
    private HashSet<int> particlesToColorThisFrame = new HashSet<int>();

    void Start()
    {
        softbody = GetComponent<ObiSoftbody>();
        if (softbody.solver != null)
        {
            solver = softbody.solver;
            solver.OnCollision += Solver_OnCollision;
            isInitialized = true; // 在Start中标记为已初始化
        }
        else
        {
            Debug.LogError("未能找到ObiSolver!");
        }
    }

    void OnDestroy()
    {
        if (solver != null)
            solver.OnCollision -= Solver_OnCollision;
        
        ClearTemporaryConstraints();
    }

    void LateUpdate()
    {
        if (!isInitialized || !softbody.isLoaded) return;

        var distanceConstraintsData = softbody.GetConstraintsByType(Oni.ConstraintType.Distance) as ObiDistanceConstraintsData;
        if (distanceConstraintsData == null) return;

        // 状态维持逻辑：仅当本帧有碰撞时才更新，无碰撞时才清除
        if (constraintsToApply.Count > 0)
        {
            ApplyNewConstraints(distanceConstraintsData);
        }
        else
        {
            if (activeTemporaryConstraints.Count > 0)
            {
                ClearTemporaryConstraints(distanceConstraintsData);
            }
        }
        
        constraintsToApply.Clear();
    }
    
    private void Solver_OnCollision(ObiSolver solver, ObiNativeContactList contacts)
    {
        if (!isInitialized || contacts.count == 0) return;

        constraintsToApply.Clear();
        particlesToColorThisFrame.Clear();
        collidingParticlesInActor.Clear();

        for (int i = 0; i < contacts.count; ++i)
        {
            Oni.Contact contact = contacts[i];
            
            int particleSolverIndex = contact.bodyA;
            int colliderIndex = contact.bodyB;

            if (!IsParticleFromOurSoftbody(particleSolverIndex))
            {
                particleSolverIndex = contact.bodyB;
                colliderIndex = contact.bodyA;
                if (!IsParticleFromOurSoftbody(particleSolverIndex))
                    continue; 
            }
            
            var colliderHandles = ObiColliderWorld.GetInstance().colliderHandles;
            if (colliderIndex < 0 || colliderIndex >= colliderHandles.Count) continue;
            var otherColliderHandle = colliderHandles[colliderIndex];
            if (!otherColliderHandle.owner) continue;

            if (!string.IsNullOrEmpty(colliderTag) && !otherColliderHandle.owner.CompareTag(colliderTag))
                continue;
            
            var pInActor = solver.particleToActor[particleSolverIndex];
            collidingParticlesInActor.Add(pInActor.indexInActor);
        }
        
        if (collidingParticlesInActor.Count > 0)
        {
            // 补全缺失的方法调用
            StrengthenCollidingNeighborhoods();
        }
    }

    // --- 补全缺失的完整方法 ---
    private void StrengthenCollidingNeighborhoods()
    {
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
                    int centerParticleIndex = batch.particleIndices[batch.firstIndex[i]];
                    for (int k = 1; k < batch.numIndices[i]; ++k)
                    {
                        int spokeParticleIndex = batch.particleIndices[batch.firstIndex[i] + k];
                        Tuple<int, int> pair = centerParticleIndex < spokeParticleIndex 
                            ? new Tuple<int, int>(centerParticleIndex, spokeParticleIndex) 
                            : new Tuple<int, int>(spokeParticleIndex, centerParticleIndex);
                        
                        constraintsToApply.Add(pair);
                        particlesToColorThisFrame.Add(centerParticleIndex);
                        particlesToColorThisFrame.Add(spokeParticleIndex);
                    }
                }
            }
        }
    }

    private void ApplyNewConstraints(ObiDistanceConstraintsData distanceConstraintsData)
    {
        // 找出真正需要新创建的约束（避免重复添加）
        bool needsRebuild = false;
        foreach (var pair in constraintsToApply)
        {
            if (!activeTemporaryConstraints.ContainsKey(pair))
            {
                activeTemporaryConstraints.Add(pair, true);
                needsRebuild = true;
            }
        }
        
        // 移除不再需要的约束
        List<Tuple<int, int>> toRemove = new List<Tuple<int, int>>();
        HashSet<Tuple<int, int>> requiredThisFrame = new HashSet<Tuple<int, int>>(constraintsToApply);
        foreach (var pair in activeTemporaryConstraints.Keys)
        {
            if (!requiredThisFrame.Contains(pair))
            {
                toRemove.Add(pair);
            }
        }

        foreach (var pair in toRemove)
        {
            activeTemporaryConstraints.Remove(pair);
            needsRebuild = true;
        }

        if (!needsRebuild) return;
        
        // ---- 重建约束 ----
        var newBatch = distanceConstraintsData.CreateBatch();
        foreach (var pair in activeTemporaryConstraints.Keys)
        {
            int p1 = pair.Item1;
            int p2 = pair.Item2;
            
            float restLength = Vector3.Distance(
                solver.positions[softbody.solverIndices[p1]],
                solver.positions[softbody.solverIndices[p2]]
            );

            newBatch.AddConstraint(new Vector2Int(p1, p2), restLength);
            newBatch.stiffnesses[newBatch.constraintCount - 1] = new Vector2(temporaryStiffness, temporaryStiffness);
        }
        
        distanceConstraintsData.Clear();
        newBatch.activeConstraintCount = activeTemporaryConstraints.Count;
        distanceConstraintsData.AddBatch(newBatch);
        softbody.SetConstraintsDirty(Oni.ConstraintType.Distance);
        
        UpdateColors();

        if (logConstraintCount)
        {
            Debug.Log($"<color=lime>约束更新: 本帧维持/新增了 {activeTemporaryConstraints.Count} 个临时距离约束。</color>");
        }
    }

    private void ClearTemporaryConstraints(ObiDistanceConstraintsData distanceConstraintsData = null)
    {
        if (softbody == null || !softbody.isLoaded) return;
        var constraints = distanceConstraintsData != null ? distanceConstraintsData : softbody.GetConstraintsByType(Oni.ConstraintType.Distance) as ObiDistanceConstraintsData;
        
        if (constraints != null)
        {
            constraints.Clear();
            softbody.SetConstraintsDirty(Oni.ConstraintType.Distance);
        }
        
        activeTemporaryConstraints.Clear();
        RestoreAllParticleColors();

        if (logConstraintCount)
        {
             Debug.Log("<color=red>碰撞结束: 所有临时约束已清除。</color>");
        }
    }
    
    private void UpdateColors()
    {
        if (!enableVisualization) return;
        RestoreAllParticleColors(); // 先恢复所有颜色
        foreach (var pair in activeTemporaryConstraints.Keys) // 再为当前激活的约束上色
        {
            int p1 = pair.Item1;
            int p2 = pair.Item2;
            particlesToColorThisFrame.Add(p1);

            int solverIndex1 = softbody.solverIndices[p1];
            if (!originalParticleColors.ContainsKey(solverIndex1))
                originalParticleColors[solverIndex1] = solver.colors[solverIndex1];
            solver.colors[solverIndex1] = activeConstraintColor;
            
            int solverIndex2 = softbody.solverIndices[p2];
            if (!originalParticleColors.ContainsKey(solverIndex2))
                originalParticleColors[solverIndex2] = solver.colors[solverIndex2];
            solver.colors[solverIndex2] = activeConstraintColor;
        }
        if (originalParticleColors.Count > 0)
            solver.colors.Upload();
    }

    private void RestoreAllParticleColors()
    {
        if (!enableVisualization || originalParticleColors.Count == 0) return;
        foreach (var pair in originalParticleColors)
        {
            if (pair.Key < solver.colors.count)
               solver.colors[pair.Key] = pair.Value;
        }
        if (originalParticleColors.Count > 0)
            solver.colors.Upload();
        originalParticleColors.Clear();
    }

    private bool IsParticleFromOurSoftbody(int particleSolverIndex)
    {
        if (particleSolverIndex < 0 || particleSolverIndex >= solver.particleToActor.Length) return false;
        var pInActor = solver.particleToActor[particleSolverIndex];
        return pInActor != null && pInActor.actor == softbody;
    }
}