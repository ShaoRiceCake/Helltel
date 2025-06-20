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
    [Tooltip("只对拥有此Tag的物体加强碰撞。留空则对所有碰撞生效。")]
    public string colliderTag; // <--- 补全缺失的变量

    [Header("可视化与调试")]
    public bool enableVisualization = true;
    public Color collidingParticleColor = Color.red;
    public Color anchorParticleColor = Color.blue;
    [Tooltip("勾选后，将在碰撞时于控制台打印当前激活的临时系绳数量。")]
    public bool logConstraintCount = true;

    private ObiSoftbody softbody;
    private ObiSolver solver;

    private List<Tuple<int, int>> tethersToApply = new List<Tuple<int, int>>();
    private Dictionary<int, Color> originalParticleColors = new Dictionary<int, Color>();
    private bool isInitialized = false;

    void Start()
    {
        softbody = GetComponent<ObiSoftbody>();
        if (softbody.solver != null)
        {
            solver = softbody.solver;
            solver.OnCollision += Solver_OnCollision;
            isInitialized = true;
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
        if (!isInitialized || !softbody.isLoaded) return;

        var tetherConstraintsData = softbody.GetConstraintsByType(Oni.ConstraintType.Tether) as ObiTetherConstraintsData;
        
        if (tetherConstraintsData == null) 
        {
            if (tethersToApply.Count > 0)
                Debug.LogWarning("TetherStiffener: 软体蓝图中未启用Tether约束，脚本无法工作。请在蓝图编辑器中启用Tether约束。", this);
            return;
        }
        
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
            
            float maxLength = Vector3.Distance(
                solver.positions[softbody.solverIndices[collidingParticle]],
                solver.positions[softbody.solverIndices[anchorParticle]]
            );

            newBatch.AddConstraint(new Vector2Int(collidingParticle, anchorParticle), maxLength, 1.0f);
            newBatch.stiffnesses[newBatch.constraintCount - 1] = tetherStiffness;
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
        if (!isInitialized || contacts.count == 0) return;

        tethersToApply.Clear();
        HashSet<int> collidingParticles = FindCollidingParticles(contacts);

        if (collidingParticles.Count == 0) return;

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

    private HashSet<int> FindCollidingParticles(ObiNativeContactList contacts)
    {
        HashSet<int> result = new HashSet<int>();
        for (int i = 0; i < contacts.count; i++)
        {
            Oni.Contact contact = contacts[i];

            int particleSolverIndex = contact.bodyA;
            int colliderIndex = contact.bodyB;

            if (!IsParticleFromOurSoftbody(particleSolverIndex))
            {
                particleSolverIndex = contact.bodyB;
                colliderIndex = contact.bodyA;
                if (!IsParticleFromOurSoftbody(particleSolverIndex)) continue;
            }

            var colliderHandles = ObiColliderWorld.GetInstance().colliderHandles;
            if (colliderIndex < 0 || colliderIndex >= colliderHandles.Count) continue;
            var otherColliderHandle = colliderHandles[colliderIndex];
            if (!otherColliderHandle.owner) continue;

            // *** 修正：重新加入碰撞体Tag过滤 ***
            if (!string.IsNullOrEmpty(colliderTag) && !otherColliderHandle.owner.CompareTag(colliderTag))
                continue;

            result.Add(solver.particleToActor[particleSolverIndex].indexInActor);
        }
        return result;
    }

    private int FindStableAnchor(int collidingParticleIndex, HashSet<int> allCollidingParticles, ObiConstraints<ObiShapeMatchingConstraintsBatch> shapeMatchingData)
    {
        for (int j = 0; j < shapeMatchingData.batchCount; ++j)
        {
            var batch = shapeMatchingData.batches[j] as ObiShapeMatchingConstraintsBatch;
            for (int i = 0; i < batch.activeConstraintCount; i++)
            {
                bool containsCollidingParticle = false;
                int firstStableNeighbor = -1;

                for (int k = 0; k < batch.numIndices[i]; ++k)
                {
                    int pIndex = batch.particleIndices[batch.firstIndex[i] + k];
                    if (pIndex == collidingParticleIndex)
                        containsCollidingParticle = true;
                    
                    if (!allCollidingParticles.Contains(pIndex))
                        firstStableNeighbor = pIndex;
                }

                if (containsCollidingParticle && firstStableNeighbor != -1)
                {
                    return firstStableNeighbor;
                }
            }
        }
        return -1;
    }
    
    private void UpdateColors()
    {
        if (!enableVisualization) return;
        RestoreAllParticleColors();
        foreach (var pair in tethersToApply)
        {
            SetParticleColor(pair.Item1, collidingParticleColor);
            SetParticleColor(pair.Item2, anchorParticleColor);
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