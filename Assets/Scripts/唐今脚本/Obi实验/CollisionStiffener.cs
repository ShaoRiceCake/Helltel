using UnityEngine;
using Obi;
using System.Collections.Generic;
using System;

/// <summary>
/// (V3.2 稳定版)
/// 引入“失效延迟”解决了持续碰撞时约束闪烁的问题。
/// 增加了详细的调试日志和手动设置约束长度的功能。
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

    // ... 内部数据结构 ...
    private ObiSoftbody softbody;
    private ObiSolver solver;
    private ObiDistanceConstraintsData distanceConstraintsData;
    private ObiDistanceConstraintsBatch dynamicBatch;
    private ObiShapeMatchingConstraintsData shapeMatchingConstraintsData;

    private class StiffenedCluster
    {
        public List<int> constraintBatchIndices = new List<int>();
        public int framesSinceLastCollision = 0; // 新增：用于追踪集群脱离接触的帧数
    }
    private readonly Dictionary<int, StiffenedCluster> activeClusters = new Dictionary<int, StiffenedCluster>();
    
    // ... 其他内部数据结构与 V3.1 相同 ...
    private readonly Dictionary<Tuple<int, int>, int> constraintToBatchIndexMap = new Dictionary<Tuple<int, int>, int>();
    private readonly HashSet<int> collidingParticlesThisFrame = new HashSet<int>();
    private readonly Dictionary<int, Color> originalParticleColors = new Dictionary<int, Color>();

    #region Unity生命周期与Obi事件
    // ... 与 V3.1 相同 ...
    void OnEnable() { softbody = GetComponent<ObiSoftbody>(); softbody.OnBlueprintLoaded += OnBlueprintLoaded; if (softbody.isLoaded) OnBlueprintLoaded(softbody, softbody.sourceBlueprint); }
    void OnDisable() { /* ... */ }
    private void OnBlueprintLoaded(ObiActor actor, ObiActorBlueprint blueprint) { /* ... */ }
    #endregion

    #region 核心逻辑 - [已升级]
    void LateUpdate()
    {
        if (solver == null || !softbody.isLoaded || shapeMatchingConstraintsData == null) return;

        bool needsRebuild = false;

        // 1. 激活逻辑：与之前相同
        for (int batchIndex = 0; batchIndex < shapeMatchingConstraintsData.batchCount; ++batchIndex)
        {
            var batch = shapeMatchingConstraintsData.batches[batchIndex];
            for (int i = 0; i < batch.activeConstraintCount; ++i)
            {
                int clusterId = i;
                if (activeClusters.ContainsKey(clusterId)) continue;

                bool isColliding = IsClusterColliding(batch, clusterId);
                
                if (isColliding)
                {
                    ActivateCluster(batch, clusterId);
                    needsRebuild = true;
                }
            }
        }

        // 2. 失效逻辑 - [已升级为带缓冲期的版本]
        List<int> clustersToRemove = null;
        foreach (var pair in activeClusters)
        {
            int clusterId = pair.Key;
            StiffenedCluster cluster = pair.Value;
            var batch = shapeMatchingConstraintsData.batches[0]; // 简化

            if (IsClusterColliding(batch, clusterId))
            {
                // 如果在碰撞，重置计时器
                cluster.framesSinceLastCollision = 0;
            }
            else
            {
                // 如果没碰撞，增加计时器
                cluster.framesSinceLastCollision++;
            }
            
            // 只有当脱离接触的时间超过了我们设定的延迟，才将其加入待移除列表
            if (cluster.framesSinceLastCollision > deactivationDelayInFrames)
            {
                if (clustersToRemove == null) clustersToRemove = new List<int>();
                clustersToRemove.Add(clusterId);
            }
        }
        
        if (clustersToRemove != null)
        {
            foreach(int clusterId in clustersToRemove)
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

    private void Solver_OnCollision(ObiSolver solver, ObiNativeContactList contacts) { /* 与 V3.1 相同 */ }
    #endregion

    #region 集群与约束管理 - [已升级]
    private void ActivateCluster(ObiShapeMatchingConstraintsBatch batch, int shapeIndexInBatch)
    {
        var newCluster = new StiffenedCluster();

        int centerParticleIndex = batch.particleIndices[batch.firstIndex[shapeIndexInBatch]];
        for (int k = 1; k < batch.numIndices[shapeIndexInBatch]; ++k)
        {
            if (dynamicBatch.activeConstraintCount >= constraintPoolSize) break;
            
            int spokeParticleIndex = batch.particleIndices[batch.firstIndex[shapeIndexInBatch] + k];
            var pair = new Tuple<int, int>(
                Math.Min(centerParticleIndex, spokeParticleIndex),
                Math.Max(centerParticleIndex, spokeParticleIndex)
            );
            
            if (constraintToBatchIndexMap.ContainsKey(pair)) continue;

            // --- 新增：手动设置约束长度的逻辑 ---
            float restLength;
            if (overrideRestLength)
            {
                restLength = manualRestLength;
            }
            else
            {
                // 默认使用蓝图的“金标准”
                restLength = Vector3.Distance(
                    solver.restPositions[softbody.solverIndices[pair.Item1]],
                    solver.restPositions[softbody.solverIndices[pair.Item2]]
                );
            }

            int batchIndex = AddConstraintToBatch(pair, restLength);
            newCluster.constraintBatchIndices.Add(batchIndex);
        }

        if (newCluster.constraintBatchIndices.Count > 0)
        {
            activeClusters.Add(shapeIndexInBatch, newCluster);
            if (logActivity)
            {
                Debug.Log($"<color=lime>约束集群激活</color>: ClusterID={shapeIndexInBatch}, 创建了 {newCluster.constraintBatchIndices.Count} 个约束。");
            }
        }
    }

    private void RemoveCluster(int clusterId)
    {
        if (activeClusters.TryGetValue(clusterId, out StiffenedCluster cluster))
        {
            if (logActivity)
            {
                Debug.Log($"<color=red>约束集群销毁</color>: ClusterID={clusterId}, 移除了 {cluster.constraintBatchIndices.Count} 个约束。");
            }
            for (int i = cluster.constraintBatchIndices.Count - 1; i >= 0; i--)
            {
                RemoveConstraintFromBatch(cluster.constraintBatchIndices[i]);
            }
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
            {
                return true;
            }
        }
        return false;
    }
    #endregion
    
    #region 底层批处理管理 (Swap and Pop) - [已修正]
    private void SetupDynamicBatch()
    {
        distanceConstraintsData = softbody.GetConstraintsByType(Oni.ConstraintType.Distance) as ObiDistanceConstraintsData;
        if (distanceConstraintsData == null) { enabled = false; return; }
        
        if (dynamicBatch != null)
            distanceConstraintsData.RemoveBatch(dynamicBatch);

        dynamicBatch = distanceConstraintsData.CreateBatch();
        if (dynamicBatch == null) { enabled = false; return; }

        dynamicBatch.activeConstraintCount = 0;
    }

    private int AddConstraintToBatch(Tuple<int, int> pair, float restLength)
    {
        int batchIndex = dynamicBatch.activeConstraintCount;

        // *** 修正点 1: 分别添加两个int，而不是一个Vector2Int ***
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
        // *** 修正点 2: 使用正确的索引访问粒子对 ***
        // 约束 'batchIndex' 的粒子位于 particleIndices 的 [batchIndex * 2] 和 [batchIndex * 2 + 1]
        var pairToRemove = new Tuple<int, int>(
            dynamicBatch.particleIndices[batchIndex * 2],
            dynamicBatch.particleIndices[batchIndex * 2 + 1]
        );
        constraintToBatchIndexMap.Remove(pairToRemove);

        dynamicBatch.activeConstraintCount--;
        int lastConstraintIndex = dynamicBatch.activeConstraintCount;

        if (batchIndex < lastConstraintIndex)
        {
            // 获取最后一个约束的粒子对
            var lastPair = new Tuple<int, int>(
                dynamicBatch.particleIndices[lastConstraintIndex * 2],
                dynamicBatch.particleIndices[lastConstraintIndex * 2 + 1]
            );

            // 用最后一个约束的数据覆盖当前要移除的槽位
            // 覆盖粒子索引
            dynamicBatch.particleIndices[batchIndex * 2] = lastPair.Item1;
            dynamicBatch.particleIndices[batchIndex * 2 + 1] = lastPair.Item2;
            
            // 覆盖其他约束属性
            dynamicBatch.restLengths[batchIndex] = dynamicBatch.restLengths[lastConstraintIndex];
            dynamicBatch.stiffnesses[batchIndex] = dynamicBatch.stiffnesses[lastConstraintIndex];

            // 更新被移动的约束在字典中的索引
            constraintToBatchIndexMap[lastPair] = batchIndex;
        }

        // *** 修正点 3: 从列表末尾移除正确数量的元素 ***
        // 移除两个粒子索引 和 一个其他属性
        dynamicBatch.particleIndices.RemoveRange(lastConstraintIndex * 2, 2);
        dynamicBatch.restLengths.RemoveAt(lastConstraintIndex);
        dynamicBatch.stiffnesses.RemoveAt(lastConstraintIndex);
    }
    #endregion

    #region 辅助与可视化
    // ... 这部分无需修改 ...
    private void UpdateColors()
    {
        RestoreAllParticleColors();
        var batch = shapeMatchingConstraintsData.batches[0];
        foreach (var pair in activeClusters)
        {
            int clusterId = pair.Key;
            for (int k = 0; k < batch.numIndices[clusterId]; ++k)
            {
                int actorIndex = batch.particleIndices[batch.firstIndex[clusterId] + k];
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