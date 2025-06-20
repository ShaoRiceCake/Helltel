using UnityEngine;
using Obi;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(ObiSoftbody))]
public class TetherStiffener : MonoBehaviour
{
    [Header("功能配置")]
    [Tooltip("临时系绳约束的硬度 (0-1)。设为1以获得最大强度。")]
    [Range(0f, 1f)]
    public float tetherStiffness = 1f;

    [Header("可视化与调试")]
    public bool enableVisualization = true;
    public Color collidingParticleColor = Color.red;
    public Color anchorParticleColor = Color.blue;
    [Tooltip("勾选后，将在碰撞时于控制台打印当前激活的临时系绳数量。")]
    public bool logConstraintCount = true;

    private ObiSoftbody softbody;
    private ObiSolver solver;

    // 存储本帧需要创建的系绳约束，Item1是碰撞粒子，Item2是锚点粒子
    private List<Tuple<int, int>> tethersToApply = new List<Tuple<int, int>>();
    
    // 用于恢复颜色的字典
    private Dictionary<int, Color> originalParticleColors = new Dictionary<int, Color>();

    void Start()
    {
        softbody = GetComponent<ObiSoftbody>();
        if (softbody.solver != null)
        {
            solver = softbody.solver;
            solver.OnCollision += Solver_OnCollision;
        }
    }

    void OnDestroy()
    {
        if (solver != null)
            solver.OnCollision -= Solver_OnCollision;
        ClearTemporaryTethers();
    }

    void LateUpdate()
    {
        if (softbody == null || !softbody.isLoaded) return;

        // 获取Actor端的系绳约束数据容器
        var tetherConstraintsData = softbody.GetConstraintsByType(Oni.ConstraintType.Tether) as ObiTetherConstraintsData;
        if (tetherConstraintsData == null) 
        {
             // 如果蓝图中没有，我们需要手动为Actor添加一个
            tetherConstraintsData = softbody.gameObject.AddComponent<ObiTetherConstraintsData>();
        }
        
        // 使用我们在之前版本中验证过的“实时增删”工作流
        tetherConstraintsData.Clear();
        RestoreAllParticleColors();

        if (tethersToApply.Count == 0)
        {
            if (softbody.isLoaded)
               softbody.SetConstraintsDirty(Oni.ConstraintType.Tether);
            return;
        }

        var newBatch = tetherConstraintsData.CreateBatch();

        foreach (var pair in tethersToApply)
        {
            int collidingParticle = pair.Item1;
            int anchorParticle = pair.Item2;
            
            // 系绳约束的长度是最大允许拉伸长度，我们设置为当前距离，不允许拉伸
            float maxLength = Vector3.Distance(
                solver.positions[softbody.solverIndices[collidingParticle]],
                solver.positions[softbody.solverIndices[anchorParticle]]
            );

            // AddConstraint(被约束的粒子, 锚点粒子/或-1代表固定点, 最大长度, 压缩/拉伸硬度)
            newBatch.AddConstraint(collidingParticle, anchorParticle, maxLength, tetherStiffness);
        }
        newBatch.activeConstraintCount = tethersToApply.Count;
        
        tetherConstraintsData.AddBatch(newBatch);
        softbody.SetConstraintsDirty(Oni.ConstraintType.Tether);
        
        UpdateColors();

        if (logConstraintCount)
        {
            Debug.Log($"<color=cyan>系绳约束更新: 创建了 {tethersToApply.Count} 个临时系绳。</color>");
        }

        tethersToApply.Clear();
    }

    private void Solver_OnCollision(ObiSolver solver, ObiNativeContactList contacts)
    {
        if (contacts.count == 0) return;

        tethersToApply.Clear();
        HashSet<int> collidingParticles = FindCollidingParticles(contacts);

        if (collidingParticles.Count == 0) return;

        // 对于每一个碰撞粒子，找到一个稳定的邻居作为锚点
        var dc = softbody.GetConstraintsByType(Oni.ConstraintType.ShapeMatching) as ObiConstraints<ObiShapeMatchingConstraintsBatch>;
        if (dc == null) return;

        foreach (int collidingParticleIndex in collidingParticles)
        {
            int anchorIndex = FindStableAnchor(collidingParticleIndex, collidingParticles, dc);
            if (anchorIndex != -1)
            {
                tethersToApply.Add(new Tuple<int, int>(collidingParticleIndex, anchorIndex));
            }
        }
    }

    // 辅助方法：从碰撞数据中找出所有属于我们软体的碰撞粒子
    private HashSet<int> FindCollidingParticles(ObiNativeContactList contacts)
    {
        HashSet<int> result = new HashSet<int>();
        for (int i = 0; i < contacts.count; i++)
        {
            int particleSolverIndex = contacts[i].bodyA;
            if (!IsParticleFromOurSoftbody(particleSolverIndex))
            {
                particleSolverIndex = contacts[i].bodyB;
                if (!IsParticleFromOurSoftbody(particleSolverIndex)) continue;
            }
            result.Add(solver.particleToActor[particleSolverIndex].indexInActor);
        }
        return result;
    }

    // 辅助方法：为指定的碰撞粒子，从其邻居团中找到一个未碰撞的锚点
    private int FindStableAnchor(int collidingParticleIndex, HashSet<int> allCollidingParticles, ObiConstraints<ObiShapeMatchingConstraintsBatch> shapeMatchingData)
    {
        for (int j = 0; j < shapeMatchingData.batchCount; ++j)
        {
            var batch = shapeMatchingData.batches[j] as ObiShapeMatchingConstraintsBatch;
            for (int i = 0; i < batch.activeConstraintCount; i++)
            {
                bool containsCollidingParticle = false;
                int firstStableNeighbor = -1;

                // 遍历邻居团的粒子
                for (int k = 0; k < batch.numIndices[i]; ++k)
                {
                    int pIndex = batch.particleIndices[batch.firstIndex[i] + k];
                    if (pIndex == collidingParticleIndex)
                        containsCollidingParticle = true;
                    
                    // 如果这个粒子不是碰撞粒子，它可以作为锚点
                    if (!allCollidingParticles.Contains(pIndex))
                        firstStableNeighbor = pIndex;
                }

                // 如果这个邻居团包含了我们的目标碰撞粒子，并且我们找到了一个稳定邻居
                if (containsCollidingParticle && firstStableNeighbor != -1)
                {
                    return firstStableNeighbor; // 立刻返回找到的第一个稳定锚点
                }
            }
        }
        return -1; // 未找到合适的锚点
    }
    
    // 颜色更新与恢复、约束清理、粒子归属判断等辅助方法
    private void UpdateColors()
    {
        if (!enableVisualization) return;
        RestoreAllParticleColors();
        foreach (var pair in tethersToApply)
        {
            int collidingParticle = pair.Item1;
            int anchorParticle = pair.Item2;
            
            SetParticleColor(collidingParticle, collidingParticleColor);
            SetParticleColor(anchorParticle, anchorParticleColor);
        }
        if (tethersToApply.Count > 0) solver.colors.Upload();
    }

    private void SetParticleColor(int actorIndex, Color color)
    {
        int solverIndex = softbody.solverIndices[actorIndex];
        if (!originalParticleColors.ContainsKey(solverIndex))
            originalParticleColors[solverIndex] = solver.colors[solverIndex];
        solver.colors[solverIndex] = color;
    }

    private void RestoreAllParticleColors()
    {
        if (!enableVisualization || originalParticleColors.Count == 0) return;
        foreach (var pair in originalParticleColors)
            if (pair.Key < solver.colors.count)
               solver.colors[pair.Key] = pair.Value;
        if (originalParticleColors.Count > 0) solver.colors.Upload();
        originalParticleColors.Clear();
    }
    
    private void ClearTemporaryTethers()
    {
        if (softbody == null || !softbody.isLoaded) return;
        var tetherConstraintsData = softbody.GetConstraintsByType(Oni.ConstraintType.Tether) as ObiTetherConstraintsData;
        if (tetherConstraintsData != null)
        {
            tetherConstraintsData.Clear();
            softbody.SetConstraintsDirty(Oni.ConstraintType.Tether);
        }
        RestoreAllParticleColors();
    }

    private bool IsParticleFromOurSoftbody(int particleSolverIndex)
    {
        if (particleSolverIndex < 0 || particleSolverIndex >= solver.particleToActor.Length) return false;
        var pInActor = solver.particleToActor[particleSolverIndex];
        return pInActor != null && pInActor.actor == softbody;
    }
}