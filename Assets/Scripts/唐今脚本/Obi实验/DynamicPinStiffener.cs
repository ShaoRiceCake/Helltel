using UnityEngine;
using Obi;
using System.Collections.Generic;

/// <summary>
/// (V16: 最终版 - 正确的 Burst Job 交互)
/// 修复了 IndexOutOfRangeException，实现了稳定、可控、高性能的动态Pin约束。
/// </summary>
[RequireComponent(typeof(ObiSoftbody))]
public class DynamicPinStiffener : MonoBehaviour
{
    [Header("核心功能配置")]
    [Tooltip("只对拥有此Tag的物体加强碰撞。留空则对所有碰撞生效。")]
    public string colliderTag;
    [Tooltip("可以同时存在的最大Pin约束数量。")]
    public int maxPins = 64;

    [Header("固定模式与强度")]
    [Tooltip("Pin约束的硬度 (0-1)。将直接影响粒子'粘'在碰撞体上的强度。")]
    [Range(0f, 1f)]
    public float pinStiffness = 1f;

    [Header("柔化挣脱 (可选)")]
    [Tooltip("启用后，当粒子远离其锚定点时，会自动释放约束。")]
    public bool enableDisengagement = false;
    [Tooltip("当粒子与其锚定点的距离超过此值时，释放Pin约束。")]
    public float disengagementDistance = 0.1f;

    [Header("可视化与调试")]
    public bool enableVisualization = true;
    public Color pinnedParticleColor = Color.cyan;
    
    // C# 层的状态追踪器，是唯一的数据源头。
    private class PinInfo { public bool IsActive = false; public int ParticleSolverIndex; public ObiColliderBase Collider; public Vector3 LocalOffset; }
    private PinInfo[] pinInfos;
    
    private ObiSoftbody softbody;
    private ObiSolver solver;
    private ObiPinConstraintsData pinConstraintsData;
    private ObiPinConstraintsBatch dynamicPinBatch;
    private readonly Dictionary<int, Color> originalParticleColors = new Dictionary<int, Color>();


    void OnEnable() { softbody = GetComponent<ObiSoftbody>(); softbody.OnBlueprintLoaded += OnBlueprintLoaded; if (softbody.isLoaded) OnBlueprintLoaded(softbody, softbody.sourceBlueprint); }
    void OnDisable() { softbody.OnBlueprintLoaded -= OnBlueprintLoaded; RemoveDynamicBatch(); RestoreAllParticleColors(); }
    private void OnBlueprintLoaded(ObiActor actor, ObiActorBlueprint blueprint) { SetupDynamicBatch(); SubscribeToSolver(); }
    private void SubscribeToSolver() { if (solver != null) solver.OnCollision -= Solver_OnCollision; solver = softbody.solver; if (solver != null) solver.OnCollision += Solver_OnCollision; }

    private void SetupDynamicBatch()
    {
        RemoveDynamicBatch();
        pinConstraintsData = softbody.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiPinConstraintsData;
        if (pinConstraintsData == null) { Debug.LogError($"[{this.name}] ...", this); enabled = false; return; }

        dynamicPinBatch = new ObiPinConstraintsBatch();
        pinConstraintsData.AddBatch(dynamicPinBatch); // 添加一个空的 batch
        
        // 初始化C#层的追踪器
        pinInfos = new PinInfo[maxPins];
        for (int i = 0; i < maxPins; ++i) 
            pinInfos[i] = new PinInfo();
    }

    private void RemoveDynamicBatch() 
    {
        if (solver != null) solver.OnCollision -= Solver_OnCollision; 
        if (pinConstraintsData != null && dynamicPinBatch != null) 
        {
            pinConstraintsData.RemoveBatch(dynamicPinBatch);
            if (softbody.isLoaded) softbody.SetConstraintsDirty(Oni.ConstraintType.Pin);
        }
        dynamicPinBatch = null; pinConstraintsData = null; 
    }

    void LateUpdate()
    {
        if (!softbody.isLoaded || dynamicPinBatch == null) return;

        // 步骤1：(可选) 脱离检测，更新 C# 层的 pinInfos 状态
        if (enableDisengagement)
        {
            for (int i = 0; i < maxPins; ++i)
            {
                if (pinInfos[i].IsActive)
                {
                    var info = pinInfos[i];
                    if (info.Collider == null || !info.Collider.gameObject.activeInHierarchy || 
                        Vector3.SqrMagnitude((Vector3)solver.positions[info.ParticleSolverIndex] - info.Collider.transform.TransformPoint(info.LocalOffset)) > disengagementDistance * disengagementDistance)                    {
                        info.IsActive = false; // 只更新C#状态
                    }
                }
            }
        }

        // 步骤2：根据 C# 层的状态，重建物理 Batch
        dynamicPinBatch.Clear(); // 清空物理Batch，准备重建
        
        float compliance = 1f - pinStiffness;
        foreach (var info in pinInfos)
        {
            if (info.IsActive)
            {
                dynamicPinBatch.AddConstraint(info.ParticleSolverIndex, info.Collider, info.LocalOffset, Quaternion.identity, compliance, 1f);
            }
        }

        // 步骤3：通知 Solver
        softbody.SetConstraintsDirty(Oni.ConstraintType.Pin);
        
        // 步骤4：更新颜色
        UpdateColors();
    }

    private void Solver_OnCollision(ObiSolver solver, ObiNativeContactList contacts)
    {
        if (contacts.count == 0) return;

        for (int i = 0; i < contacts.count; ++i)
        {
            Oni.Contact contact = contacts[i];
            int particleSolverIndex = GetParticleSolverIndexFromContact(contact);
            if (particleSolverIndex == -1 || IsParticleAlreadyPinned(particleSolverIndex)) continue;
            
            int freeSlotIndex = FindFreePinSlot();
            if (freeSlotIndex == -1) break; 

            var otherCollider = ObiColliderWorld.GetInstance().colliderHandles[GetColliderIndexFromContact(contact)].owner;
            if (otherCollider == null || !otherCollider.gameObject.activeInHierarchy) continue;
            if (!string.IsNullOrEmpty(colliderTag) && !otherCollider.CompareTag(colliderTag)) continue;
            
            Matrix4x4 bindMatrix = otherCollider.transform.worldToLocalMatrix * solver.transform.localToWorldMatrix;
            Vector3 pinOffset = bindMatrix.MultiplyPoint3x4(solver.positions[particleSolverIndex]);
            if (float.IsNaN(pinOffset.x)) continue;
            
            // 激活 C# 层的槽位，真正的物理约束将在 LateUpdate 中创建
            var info = pinInfos[freeSlotIndex];
            info.IsActive = true;
            info.ParticleSolverIndex = particleSolverIndex;
            info.Collider = otherCollider;
            info.LocalOffset = pinOffset;
        }
    }
    
    private int FindFreePinSlot()
    {
        for (int i = 0; i < maxPins; i++)
        {
            if (!pinInfos[i].IsActive) return i;
        }
        return -1;
    }

    private bool IsParticleAlreadyPinned(int particleSolverIndex)
    {
        foreach (var info in pinInfos)
        {
            if (info.IsActive && info.ParticleSolverIndex == particleSolverIndex) 
                return true;
        }
        return false;
    }

    // --- 颜色和辅助方法 ---
    private void UpdateColors()
    {
        if (!enableVisualization || solver == null) return;
        
        // 建立当前帧需要上色的粒子列表
        var currentPinnedParticles = new HashSet<int>();
        foreach (var info in pinInfos)
        {
            if (info.IsActive)
            {
                currentPinnedParticles.Add(info.ParticleSolverIndex);
            }
        }

        // 恢复不再被 Pin 的粒子的颜色
        var keysToRemove = new List<int>();
        foreach (var pair in originalParticleColors)
        {
            if (!currentPinnedParticles.Contains(pair.Key))
            {
                if (pair.Key >= 0 && pair.Key < solver.colors.count)
                    solver.colors[pair.Key] = pair.Value;
                keysToRemove.Add(pair.Key);
            }
        }
        foreach (var key in keysToRemove)
        {
            originalParticleColors.Remove(key);
        }

        // 为新被 Pin 的粒子上色
        foreach (int solverIndex in currentPinnedParticles)
        {
            if (!originalParticleColors.ContainsKey(solverIndex))
            {
                originalParticleColors[solverIndex] = solver.colors[solverIndex];
                solver.colors[solverIndex] = pinnedParticleColor;
            }
        }

        solver.colors.Upload();
    }
    
    private void RestoreAllParticleColors()
    {
        if(originalParticleColors.Count > 0 && solver != null)
        {
            foreach(var p in originalParticleColors)
            {
                if(p.Key >= 0 && p.Key < solver.colors.count)
                    solver.colors[p.Key] = p.Value;
            }
            solver.colors.Upload();
        }
        originalParticleColors.Clear();
    }
    
    private int GetParticleSolverIndexFromContact(Oni.Contact contact) { if(IsParticleFromOurSoftbody(contact.bodyA)) return contact.bodyA; if(IsParticleFromOurSoftbody(contact.bodyB)) return contact.bodyB; return -1; }
    private int GetColliderIndexFromContact(Oni.Contact contact) { return IsParticleFromOurSoftbody(contact.bodyA) ? contact.bodyB : contact.bodyA; }
    private bool IsParticleFromOurSoftbody(int particleSolverIndex) { if(solver == null || particleSolverIndex < 0 || particleSolverIndex >= solver.particleToActor.Length) return false; var p = solver.particleToActor[particleSolverIndex]; return p != null && p.actor == softbody; }
}