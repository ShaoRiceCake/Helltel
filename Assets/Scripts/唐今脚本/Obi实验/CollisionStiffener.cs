using UnityEngine;
using Obi;
using System.Collections.Generic;
using System;

/// <summary>
/// (V3.4 最终修正版)
/// 修正了因辅助方法未同步更新数据结构导致的编译错误。
/// 所有部分（核心逻辑、辅助函数）现在都正确处理唯一的集群ID。
/// 这是用于集群碰撞强化的稳定、完整版本。
/// </summary>
[RequireComponent(typeof(ObiSoftbody))]
public class ClusteredCollisionStiffener : MonoBehaviour
{
    [Header("功能配置")]
    [Tooltip("临时距离约束的硬度 (0-1)。")]
    [Range(0f, 1f)]
    public float temporaryStiffness = 1f;
    [Tooltip("只对拥有此Tag的物体加强碰撞。留空则对所有碰撞生效。")]
    public string colliderTag;
    [Tooltip("预分配的距离约束池大小。")]
    public int constraintPoolSize = 1024;

    [Header("失效逻辑")]
    [Tooltip("设置一个集群在完全脱离接触后，需要等待多少帧才真正被销毁。这可以防止因物理计算波动导致的闪烁。")]
    public int deactivationDelayInFrames = 5;

    [Header("手动约束长度 (可选)")]
    [Tooltip("勾选后，将使用下面的手动设置值，而不是根据蓝图的静止位置。")]
    public bool overrideRestLength = false;
    [Tooltip("手动设置所有临时约束的静止长度。")]
    public float manualRestLength = 0.1f;

    [Header("可视化与调试")]
    public bool enableVisualization = true;
    public Color activeConstraintColor = new Color(1, 0.5f, 0);
    [Tooltip("勾选后，将在控制台打印约束集群的激活和销毁日志。")]
    public bool logActivity = false;
    
    // --- 内部数据结构 ---
    private ObiSoftbody softbody;
    private ObiSolver solver;
    private ObiDistanceConstraintsData distanceConstraintsData;
    private ObiDistanceConstraintsBatch dynamicBatch;
    private ObiShapeMatchingConstraintsData shapeMatchingConstraintsData;

    private class StiffenedCluster
    {
        public List<int> constraintBatchIndices = new List<int>();
        public int framesSinceLastCollision = 0;
    }

    private readonly Dictionary<Tuple<int, int>, StiffenedCluster> activeClusters = new Dictionary<Tuple<int, int>, StiffenedCluster>();
    private readonly Dictionary<Tuple<int, int>, int> constraintToBatchIndexMap = new Dictionary<Tuple<int, int>, int>();
    private readonly HashSet<int> collidingParticlesThisFrame = new HashSet<int>();
    private readonly Dictionary<int, Color> originalParticleColors = new Dictionary<int, Color>();

    #region Unity生命周期与Obi事件
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
        if (solver != null)
            solver.OnCollision -= Solver_OnCollision;
        
        if (softbody != null && softbody.isLoaded && distanceConstraintsData != null && dynamicBatch != null)
        {
            distanceConstraintsData.RemoveBatch(dynamicBatch);
            softbody.SetConstraintsDirty(Oni.ConstraintType.Distance);
        }
        RestoreAllParticleColors();
    }

    private void OnBlueprintLoaded(ObiActor actor, ObiActorBlueprint blueprint)
    {
        if (solver != null)
            solver.OnCollision -= Solver_OnCollision;
        solver = softbody.solver;
        if (solver != null)
            solver.OnCollision += Solver_OnCollision;
        
        shapeMatchingConstraintsData = softbody.GetConstraintsByType(Oni.ConstraintType.ShapeMatching) as ObiShapeMatchingConstraintsData;
        if (shapeMatchingConstraintsData == null)
        {
            Debug.LogError("本脚本依赖 Obi Shape Matching Constraints 来定义粒子簇。请为软体添加该组件。", this);
            enabled = false;
            return;
        }
        SetupDynamicBatch();
    }
    #endregion

    #region 核心逻辑
    void LateUpdate()
    {
        if (solver == null || !softbody.isLoaded || shapeMatchingConstraintsData == null) return;

        bool needsRebuild = false;

        for (int batchIndex = 0; batchIndex < shapeMatchingConstraintsData.batchCount; ++batchIndex)
        {
            var batch = shapeMatchingConstraintsData.batches[batchIndex];
            for (int i = 0; i < batch.activeConstraintCount; ++i)
            {
                var clusterId = new Tuple<int, int>(batchIndex, i);
                if (activeClusters.ContainsKey(clusterId)) continue;
                if (IsClusterColliding(batch, i))
                {
                    ActivateCluster(batch, i, clusterId);
                    needsRebuild = true;
                }
            }
        }

        List<Tuple<int, int>> clustersToRemove = null;
        foreach (var pair in activeClusters)
        {
            var clusterId = pair.Key;
            StiffenedCluster cluster = pair.Value;
            var batch = shapeMatchingConstraintsData.batches[clusterId.Item1];

            if (IsClusterColliding(batch, clusterId.Item2))
            {
                cluster.framesSinceLastCollision = 0;
            }
            else
            {
                cluster.framesSinceLastCollision++;
            }
            
            if (cluster.framesSinceLastCollision > deactivationDelayInFrames)
            {
                if (clustersToRemove == null) clustersToRemove = new List<Tuple<int, int>>();
                clustersToRemove.Add(clusterId);
            }
        }
        
        if (clustersToRemove != null)
        {
            foreach(var clusterId in clustersToRemove)
            {
                RemoveCluster(clusterId);
                needsRebuild = true;
            }
        }

        if (needsRebuild)
        {
            softbody.SetConstraintsDirty(Oni.ConstraintType.Distance);
            if (enableVisualization) UpdateColors();
        }

        collidingParticlesThisFrame.Clear();
    }

    private void Solver_OnCollision(ObiSolver solver, ObiNativeContactList contacts)
    {
        if (contacts.count == 0) return;
        for (int i = 0; i < contacts.count; ++i)
        {
            int particleSolverIndex = GetParticleSolverIndexFromContact(contacts[i]);
            if (particleSolverIndex == -1) continue;
            var collider = ObiColliderWorld.GetInstance().colliderHandles[GetColliderIndexFromContact(contacts[i])].owner;
            if (collider == null || (!string.IsNullOrEmpty(colliderTag) && !collider.CompareTag(colliderTag))) continue;
            collidingParticlesThisFrame.Add(solver.particleToActor[particleSolverIndex].indexInActor);
        }
    }
    #endregion

    #region 集群与约束管理
    private void ActivateCluster(ObiShapeMatchingConstraintsBatch batch, int shapeIndexInBatch, Tuple<int, int> clusterId)
    {
        var newCluster = new StiffenedCluster();
        int centerParticleIndex = batch.particleIndices[batch.firstIndex[shapeIndexInBatch]];
        for (int k = 1; k < batch.numIndices[shapeIndexInBatch]; ++k)
        {
            if (dynamicBatch.activeConstraintCount >= constraintPoolSize) break;
            int spokeParticleIndex = batch.particleIndices[batch.firstIndex[shapeIndexInBatch] + k];
            var pair = new Tuple<int, int>(Math.Min(centerParticleIndex, spokeParticleIndex), Math.Max(centerParticleIndex, spokeParticleIndex));
            if (constraintToBatchIndexMap.ContainsKey(pair)) continue;

            float restLength = overrideRestLength ? manualRestLength : 
                Vector3.Distance(solver.restPositions[softbody.solverIndices[pair.Item1]], solver.restPositions[softbody.solverIndices[pair.Item2]]);

            int batchIndex = AddConstraintToBatch(pair, restLength);
            newCluster.constraintBatchIndices.Add(batchIndex);
        }

        if (newCluster.constraintBatchIndices.Count > 0)
        {
            activeClusters.Add(clusterId, newCluster);
            if (logActivity)
                Debug.Log($"<color=lime>约束集群激活</color>: ClusterID={clusterId}, 创建了 {newCluster.constraintBatchIndices.Count} 个约束。");
        }
    }

    private void RemoveCluster(Tuple<int, int> clusterId)
    {
        if (activeClusters.TryGetValue(clusterId, out StiffenedCluster cluster))
        {
            if (logActivity)
                Debug.Log($"<color=red>约束集群销毁</color>: ClusterID={clusterId}, 移除了 {cluster.constraintBatchIndices.Count} 个约束。");
            for (int i = cluster.constraintBatchIndices.Count - 1; i >= 0; i--)
                RemoveConstraintFromBatch(cluster.constraintBatchIndices[i]);
            activeClusters.Remove(clusterId);
        }
    }

    private bool IsClusterColliding(ObiShapeMatchingConstraintsBatch batch, int shapeIndexInBatch)
    {
        if (shapeIndexInBatch >= batch.activeConstraintCount) return false;
        for (int k = 0; k < batch.numIndices[shapeIndexInBatch]; ++k)
        {
            int particleActorIndex = batch.particleIndices[batch.firstIndex[shapeIndexInBatch] + k];
            if (collidingParticlesThisFrame.Contains(particleActorIndex))
                return true;
        }
        return false;
    }
    #endregion
    
    #region 底层批处理管理 (Swap and Pop)
    private void SetupDynamicBatch()
    {
        distanceConstraintsData = softbody.GetConstraintsByType(Oni.ConstraintType.Distance) as ObiDistanceConstraintsData;
        if (distanceConstraintsData == null) { enabled = false; return; }
        if (dynamicBatch != null) distanceConstraintsData.RemoveBatch(dynamicBatch);
        dynamicBatch = distanceConstraintsData.CreateBatch();
        if (dynamicBatch == null) { enabled = false; return; }
        dynamicBatch.activeConstraintCount = 0;
    }

    private int AddConstraintToBatch(Tuple<int, int> pair, float restLength)
    {
        int batchIndex = dynamicBatch.activeConstraintCount;
        dynamicBatch.particleIndices.Add(pair.Item1);
        dynamicBatch.particleIndices.Add(pair.Item2);
        dynamicBatch.restLengths.Add(restLength);
        dynamicBatch.stiffnesses.Add(new Vector2(temporaryStiffness, temporaryStiffness));
        dynamicBatch.activeConstraintCount++;
        constraintToBatchIndexMap[pair] = batchIndex;
        return batchIndex;
    }

    private void RemoveConstraintFromBatch(int batchIndex)
    {
        var pairToRemove = new Tuple<int, int>(dynamicBatch.particleIndices[batchIndex * 2], dynamicBatch.particleIndices[batchIndex * 2 + 1]);
        constraintToBatchIndexMap.Remove(pairToRemove);
        dynamicBatch.activeConstraintCount--;
        int lastConstraintIndex = dynamicBatch.activeConstraintCount;
        if (batchIndex < lastConstraintIndex)
        {
            var lastPair = new Tuple<int, int>(dynamicBatch.particleIndices[lastConstraintIndex * 2], dynamicBatch.particleIndices[lastConstraintIndex * 2 + 1]);
            dynamicBatch.particleIndices[batchIndex * 2] = lastPair.Item1;
            dynamicBatch.particleIndices[batchIndex * 2 + 1] = lastPair.Item2;
            dynamicBatch.restLengths[batchIndex] = dynamicBatch.restLengths[lastConstraintIndex];
            dynamicBatch.stiffnesses[batchIndex] = dynamicBatch.stiffnesses[lastConstraintIndex];
            constraintToBatchIndexMap[lastPair] = batchIndex;
        }
        dynamicBatch.particleIndices.RemoveRange(lastConstraintIndex * 2, 2);
        dynamicBatch.restLengths.RemoveAt(lastConstraintIndex);
        dynamicBatch.stiffnesses.RemoveAt(lastConstraintIndex);
    }
    #endregion

    #region 辅助与可视化 - [已修正]
    private void UpdateColors()
    {
        RestoreAllParticleColors();
        foreach (var pair in activeClusters)
        {
            var clusterId = pair.Key;
            int batchIndex = clusterId.Item1;
            int shapeIndexInBatch = clusterId.Item2;
            var batch = shapeMatchingConstraintsData.batches[batchIndex];

            for (int k = 0; k < batch.numIndices[shapeIndexInBatch]; ++k)
            {
                int actorIndex = batch.particleIndices[batch.firstIndex[shapeIndexInBatch] + k];
                int solverIndex = softbody.solverIndices[actorIndex];
                if (!originalParticleColors.ContainsKey(solverIndex))
                    originalParticleColors[solverIndex] = solver.colors[solverIndex];
                solver.colors[solverIndex] = activeConstraintColor;
            }
        }
        if (originalParticleColors.Count > 0)
            solver.colors.Upload();
    }

    private void RestoreAllParticleColors()
    {
        if (originalParticleColors.Count == 0 || solver == null) return;
        foreach (var pair in originalParticleColors)
        {
            if (pair.Key >= 0 && pair.Key < solver.colors.count)
                solver.colors[pair.Key] = pair.Value;
        }
        if (originalParticleColors.Count > 0)
            solver.colors.Upload();
        originalParticleColors.Clear();
    }
    
    private int GetParticleSolverIndexFromContact(Oni.Contact contact) { return IsParticleFromOurSoftbody(contact.bodyA) ? contact.bodyA : (IsParticleFromOurSoftbody(contact.bodyB) ? contact.bodyB : -1); }
    private int GetColliderIndexFromContact(Oni.Contact contact) { return IsParticleFromOurSoftbody(contact.bodyA) ? contact.bodyB : contact.bodyA; }
    private bool IsParticleFromOurSoftbody(int particleSolverIndex) { if (solver == null || particleSolverIndex < 0 || particleSolverIndex >= solver.particleToActor.Length) return false; var p = solver.particleToActor[particleSolverIndex]; return p != null && p.actor == softbody; }
    #endregion
}