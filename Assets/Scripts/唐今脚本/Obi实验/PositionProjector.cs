using UnityEngine;
using Obi;
using System.Collections.Generic;

[RequireComponent(typeof(ObiSoftbody))]
public class PositionProjector : MonoBehaviour
{
    [Header("投影配置")]
    [Tooltip("投影强度(0-1)。1代表完全修正到刚性形态，0代表无效果。可以适当调低以获得“韧性”而非“刚性”的效果。")]
    [Range(0, 1)]
    public float projectionStrength = 1.0f;

    [Tooltip("只对拥有此Tag的物体加强碰撞。留空则对所有碰撞生效。")]
    public string colliderTag;

    [Header("可视化")]
    public bool enableVisualization = true;
    public Color projectedParticleColor = Color.red;

    private ObiSoftbody softbody;
    private ObiSolver solver;
    
    // 用于存储需要进行位置投影的粒子簇
    private HashSet<int> clustersToProject = new HashSet<int>();
    
    // 用于恢复颜色
    private Dictionary<int, Color> originalParticleColors = new Dictionary<int, Color>();
    private List<int> particlesToColorLastFrame = new List<int>();

    void Start()
    {
        softbody = GetComponent<ObiSoftbody>();
        if (softbody.solver != null)
        {
            solver = softbody.solver;
            // 我们在每次模拟“之后”进行修正，所以订阅OnSimulationEnd事件
            solver.OnSimulationEnd += Solver_OnSimulationEnd;
        }
    }

    void OnDestroy()
    {
        if (solver != null)
            solver.OnSimulationEnd -= Solver_OnSimulationEnd;
        
        // 确保退出时恢复所有颜色
        RestoreAllParticleColors();
    }
    
    // 在物理模拟结束后，渲染前，执行我们的强制位置修正
    private void Solver_OnSimulationEnd(ObiSolver solver, float timeToSimulate, float substepTime)
    {
        if (!enabled || !softbody.isLoaded || projectionStrength <= 0)
        {
            RestoreAllParticleColors();
            return;
        }

        // 步骤 1: 识别碰撞的粒子簇
        FindCollidingClusters();
        
        if (clustersToProject.Count == 0)
        {
            RestoreAllParticleColors();
            return;
        }

        // 步骤 2: 对每个需要修正的粒子簇执行位置投影
        var shapeMatching = softbody.GetConstraintsByType(Oni.ConstraintType.ShapeMatching) as ObiConstraints<ObiShapeMatchingConstraintsBatch>;
        if (shapeMatching == null) return;

        // 准备一个临时的颜色列表
        List<int> particlesToColorThisFrame = new List<int>();

        foreach (int clusterIndexInActor in clustersToProject)
        {
            // 注意：由于一个蓝图可能被多个Actor共享，batch的索引可能不直接对应
            // 这里我们简化处理，假设ShapeMatching只有一个batch
            if (shapeMatching.batchCount == 0) continue;
            var batch = shapeMatching.batches[0] as ObiShapeMatchingConstraintsBatch;

            // 获取这个簇在batch中的实际索引
            // (这是一个简化查找，实际项目中可能需要更鲁棒的查找方式)
            int constraintIndex = -1;
            for (int i = 0; i < batch.activeConstraintCount; ++i){
                 if (batch.firstIndex[i] == softbody.softbodyBlueprint.shapeMatchingConstraintsData.batches[0].firstIndex[clusterIndexInActor]){
                     constraintIndex = i;
                     break;
                 }
            }
            if (constraintIndex == -1) continue;
            
            // --- 执行投影的核心逻辑 ---
            ProjectCluster(batch, constraintIndex, particlesToColorThisFrame);
        }

        // 步骤 3: 更新颜色
        UpdateColors(particlesToColorThisFrame);
    }
    
    private void FindCollidingClusters()
    {
        clustersToProject.Clear();
        var contacts = solver.colliderContacts; // 直接从solver获取最新的接触点信息

        for (int i = 0; i < contacts.count; i++)
        {
            Oni.Contact contact = contacts[i];
            int particleSolverIndex = GetParticleSolverIndexFromContact(contact);

            if (particleSolverIndex != -1)
            {
                 // Tag 过滤
                int colliderIndex = (contact.bodyA == particleSolverIndex) ? contact.bodyB : contact.bodyA;
                var colliderHandles = ObiColliderWorld.GetInstance().colliderHandles;
                if (colliderIndex < 0 || colliderIndex >= colliderHandles.Count) continue;
                var otherColliderHandle = colliderHandles[colliderIndex];
                if (!otherColliderHandle.owner) continue;
                if (!string.IsNullOrEmpty(colliderTag) && !otherColliderHandle.owner.CompareTag(colliderTag)) continue;
                
                // 找到这个粒子属于哪个簇
                int particleActorIndex = solver.particleToActor[particleSolverIndex].indexInActor;
                int clusterIndex = FindClusterForParticle(particleActorIndex);
                if (clusterIndex != -1)
                {
                    clustersToProject.Add(clusterIndex);
                }
            }
        }
    }
    
    private void ProjectCluster(ObiShapeMatchingConstraintsBatch batch, int constraintIndex, List<int> particlesToColor)
    {
        // --- 1. 计算当前质心和静息质心 ---
        Vector3 currentCentroid = Vector3.zero;
        Vector3 restCentroid = Vector3.zero;
        int numParticles = batch.numIndices[constraintIndex];
        
        List<int> particleActorIndices = new List<int>();
        for (int k = 0; k < numParticles; ++k)
        {
            int pActorIndex = batch.particleIndices[batch.firstIndex[constraintIndex] + k];
            particleActorIndices.Add(pActorIndex);
            
            currentCentroid += (Vector3)solver.positions[softbody.solverIndices[pActorIndex]];
            restCentroid += (Vector3)softbody.softbodyBlueprint.restPositions[pActorIndex];
        }
        currentCentroid /= numParticles;
        restCentroid /= numParticles;

        // --- 2. 遍历簇内每个粒子，计算并设置其理想位置 ---
        foreach (int pActorIndex in particleActorIndices)
        {
            Vector3 restRelativePos = (Vector3)softbody.softbodyBlueprint.restPositions[pActorIndex] - restCentroid;
            
            // 理想位置 = 当前质心 + 静息状态下的相对位置
            Vector3 idealPosition = currentCentroid + restRelativePos;
            
            int pSolverIndex = softbody.solverIndices[pActorIndex];
            Vector3 currentPosition = solver.positions[pSolverIndex];

            // 使用 Lerp 进行投影，strength 为1则为完全强制修正
            Vector3 projectedPosition = Vector3.Lerp(currentPosition, idealPosition, projectionStrength);

            // 强制覆盖求解器计算出的位置！
            solver.positions[pSolverIndex] = projectedPosition;

            // 记录需要变色的粒子
            if (enableVisualization)
                particlesToColor.Add(pSolverIndex);
        }
    }

    // --- 辅助方法 ---
    private int FindClusterForParticle(int particleActorIndex)
    {
        var shapeMatchingData = softbody.softbodyBlueprint.shapeMatchingConstraintsData;
        if (shapeMatchingData == null) return -1;

        // 假设只有一个batch
        var batch = shapeMatchingData.batches[0];
        for (int i = 0; i < batch.activeConstraintCount; ++i)
        {
            for (int k = 0; k < batch.numIndices[i]; ++k)
            {
                if (batch.particleIndices[batch.firstIndex[i] + k] == particleActorIndex)
                {
                    return i; // 返回的是簇在蓝图约束数据中的索引
                }
            }
        }
        return -1;
    }

    private int GetParticleSolverIndexFromContact(Oni.Contact contact)
    {
        if (IsParticleFromOurSoftbody(contact.bodyA)) return contact.bodyA;
        if (IsParticleFromOurSoftbody(contact.bodyB)) return contact.bodyB;
        return -1;
    }
    
    private bool IsParticleFromOurSoftbody(int particleSolverIndex)
    {
        if (particleSolverIndex < 0 || particleSolverIndex >= solver.particleToActor.Length) return false;
        var pInActor = solver.particleToActor[particleSolverIndex];
        return pInActor != null && pInActor.actor == softbody;
    }

    // --- 颜色管理 ---
    private void UpdateColors(List<int> particlesToColorNow)
    {
        if (!enableVisualization) return;

        // 恢复上一帧变色的粒子
        foreach (int solverIndex in particlesToColorLastFrame)
        {
            if (originalParticleColors.TryGetValue(solverIndex, out Color originalColor))
            {
                solver.colors[solverIndex] = originalColor;
            }
        }

        // 为本帧的粒子上色
        foreach (int solverIndex in particlesToColorNow)
        {
            if (!originalParticleColors.ContainsKey(solverIndex))
                originalParticleColors[solverIndex] = solver.colors[solverIndex];
            solver.colors[solverIndex] = projectedParticleColor;
        }

        if (particlesToColorLastFrame.Count > 0 || particlesToColorNow.Count > 0)
            solver.colors.Upload();

        particlesToColorLastFrame = new List<int>(particlesToColorNow);
    }
    
    private void RestoreAllParticleColors()
    {
        if (!enableVisualization || originalParticleColors.Count == 0) return;
        foreach (var pair in originalParticleColors)
            if (pair.Key < solver.colors.count)
               solver.colors[pair.Key] = pair.Value;
        if (originalParticleColors.Count > 0) solver.colors.Upload();
        originalParticleColors.Clear();
        particlesToColorLastFrame.Clear();
    }
}