using UnityEngine;
using Obi;
using System.Collections.Generic;

/// <summary>
/// (V15: 修正版 - 正确的约束池管理与生命周期)
/// 修复了Pin约束不生效的问题。
/// </summary>
[RequireComponent(typeof(ObiSoftbody))]
public class DynamicPinStiffener : MonoBehaviour
{
    [Header("核心功能配置")]
    [Tooltip("只对拥有此Tag的物体加强碰撞。留空则对所有碰撞生效。")]
    public string colliderTag;
    [Tooltip("预分配的 Pin 约束池大小。")]
    public int pinPoolSize = 64;

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
    
    // C# 层的状态追踪器，现在只用于柔化挣脱逻辑和颜色恢复
    private class PinInfo { public int ParticleSolverIndex = -1; public ObiColliderBase Collider = null; public Vector3 LocalOffset; }
    private PinInfo[] pinInfos;
    
    private ObiSoftbody softbody;
    private ObiSolver solver;
    private ObiPinConstraintsData pinConstraintsData;
    private ObiPinConstraintsBatch dynamicPinBatch;
    private readonly Dictionary<int, Color> originalParticleColors = new Dictionary<int, Color>();


    void OnEnable() 
    {
        softbody = GetComponent<ObiSoftbody>(); 
        softbody.OnBlueprintLoaded += OnBlueprintLoaded; 
        if (softbody.isLoaded) OnBlueprintLoaded(softbody, softbody.sourceBlueprint); 
    }

    void OnDisable() 
    {
        softbody.OnBlueprintLoaded -= OnBlueprintLoaded;
        RemoveDynamicBatch();
        RestoreAllParticleColors();
    }

    private void OnBlueprintLoaded(ObiActor actor, ObiActorBlueprint blueprint) 
    { 
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
        // 确保旧的Batch被正确移除
        RemoveDynamicBatch();
        
        pinConstraintsData = softbody.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiPinConstraintsData;
        if (pinConstraintsData == null) 
        { 
            Debug.LogError($"[{this.name}] ObiSoftbody上缺少PinConstraints组件。请添加一个Obi Pin Constraints。", this); 
            enabled = false; 
            return; 
        }

        // *** 关键修改 1: 正确的批处理初始化 ***
        dynamicPinBatch = new ObiPinConstraintsBatch();
        pinInfos = new PinInfo[pinPoolSize]; // C#状态追踪器

        for (int i = 0; i < pinPoolSize; ++i) 
        {
            // 为池中每个槽位添加一个“占位符”约束。
            // 粒子索引为-1是关键，它告诉Solver这是一个无效/待用的约束。
            dynamicPinBatch.AddConstraint(-1, null, Vector3.zero, Quaternion.identity, 1f, 1f);
            pinInfos[i] = new PinInfo();
        }
        
        // 设置活动约束数量为池的大小，让Solver知道这个批处理的总容量
        dynamicPinBatch.activeConstraintCount = pinPoolSize;
        
        // 将准备好的、但所有约束都无效的批处理添加到Actor
        pinConstraintsData.AddBatch(dynamicPinBatch);
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
        if (!softbody.isLoaded || dynamicPinBatch == null || !enableDisengagement) return;

        bool needsUpdate = false;
        for (int i = 0; i < pinPoolSize; ++i)
        {
            // 通过检查批处理中的粒子索引来判断槽位是否激活
            if (dynamicPinBatch.particleIndices[i] != -1)
            {
                var info = pinInfos[i];
                var targetCollider = info.Collider;

                if (targetCollider == null || !targetCollider.gameObject.activeInHierarchy)
                {
                    DeactivatePin(i);
                    needsUpdate = true;
                    continue;
                }
                
                Vector3 currentParticlePos = solver.positions[info.ParticleSolverIndex];
                Vector3 pinWorldPos = targetCollider.transform.TransformPoint(info.LocalOffset);

                if (Vector3.SqrMagnitude(currentParticlePos - pinWorldPos) > disengagementDistance * disengagementDistance)
                {
                    DeactivatePin(i);
                    needsUpdate = true;
                }
            }
        }

        if (needsUpdate)
        {
            softbody.SetConstraintsDirty(Oni.ConstraintType.Pin);
            UpdateColors();
        }
    }

    private void Solver_OnCollision(ObiSolver solver, ObiNativeContactList contacts)
    {
        if (contacts.count == 0) return;

        bool needsUpdate = false;
        for (int i = 0; i < contacts.count; ++i)
        {
            Oni.Contact contact = contacts[i];
            int particleSolverIndex = GetParticleSolverIndexFromContact(contact);
            if (particleSolverIndex == -1 || IsParticleAlreadyPinned(particleSolverIndex)) continue;
            
            int freeSlotIndex = FindFreePinSlot();
            if (freeSlotIndex == -1) 
            {
                Debug.LogWarning("Pin约束池已满，无法创建新的约束。");
                break; 
            }

            var otherCollider = ObiColliderWorld.GetInstance().colliderHandles[GetColliderIndexFromContact(contact)].owner;
            if (otherCollider == null || !otherCollider.gameObject.activeInHierarchy) continue;
            if (!string.IsNullOrEmpty(colliderTag) && !otherCollider.CompareTag(colliderTag)) continue;
            
            Matrix4x4 bindMatrix = otherCollider.transform.worldToLocalMatrix * solver.transform.localToWorldMatrix;
            Vector3 pinOffset = bindMatrix.MultiplyPoint3x4(solver.positions[particleSolverIndex]);
            if (float.IsNaN(pinOffset.x)) continue;
            
            ActivatePin(freeSlotIndex, particleSolverIndex, otherCollider, pinOffset);
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            softbody.SetConstraintsDirty(Oni.ConstraintType.Pin);
            UpdateColors(); 
        }
    }

    private void ActivatePin(int slotIndex, int particleSolverIndex, ObiColliderBase collider, Vector3 offset)
    {
        // *** 关键修改 2: 正确的约束激活 ***
        // 直接修改批处理数组中的数据
        dynamicPinBatch.particleIndices[slotIndex] = particleSolverIndex;
        dynamicPinBatch.pinBodies[slotIndex] = collider.Handle;
        dynamicPinBatch.colliderIndices[slotIndex] = collider.Handle.index;
        dynamicPinBatch.offsets[slotIndex] = offset;
        
        // 1 - stiffness 正确地将硬度[0,1]映射到柔顺度[1,0]
        dynamicPinBatch.stiffnesses[slotIndex * 2] = 1f - pinStiffness; 
        dynamicPinBatch.stiffnesses[slotIndex * 2 + 1] = 1f; // 旋转方向的柔顺度，1表示完全自由

        // 更新C#层的追踪信息
        var info = pinInfos[slotIndex];
        info.ParticleSolverIndex = particleSolverIndex;
        info.Collider = collider;
        info.LocalOffset = offset;
    }

    private void DeactivatePin(int slotIndex)
    {
        // *** 关键修改 3: 正确的约束停用 ***
        // 必须将粒子索引设为-1，这才是真正“释放”了约束槽
        dynamicPinBatch.particleIndices[slotIndex] = -1;

        // 将柔顺度设回默认值(1)，虽然不是必须的，但是个好习惯
        dynamicPinBatch.stiffnesses[slotIndex * 2] = 1f;
        dynamicPinBatch.stiffnesses[slotIndex * 2 + 1] = 1f;
        
        // 清理C#层的追踪信息
        var info = pinInfos[slotIndex];
        info.ParticleSolverIndex = -1;
        info.Collider = null;
    }
    
    // *** 关键修改 4: 查找和判断逻辑的调整 ***
    private int FindFreePinSlot()
    {
        for (int i = 0; i < pinPoolSize; i++)
        {
            // 通过检查批处理中的粒子索引来查找空闲槽位
            if (dynamicPinBatch.particleIndices[i] == -1) 
                return i;
        }
        return -1; // 池已满
    }

    private bool IsParticleAlreadyPinned(int particleSolverIndex)
    {
        for (int i = 0; i < pinPoolSize; i++)
        {
            // 通过检查批处理中的粒子索引来判断是否已固定
            if (dynamicPinBatch.particleIndices[i] == particleSolverIndex) 
                return true;
        }
        return false;
    }

    // --- 颜色和辅助方法 (与之前版本基本相同) ---
    private void UpdateColors()
    {
        if (!enableVisualization || solver == null) return;
        
        // 先恢复所有颜色
        RestoreAllParticleColors();
        
        // 再为当前固定的粒子上色
        for(int i = 0; i < pinPoolSize; ++i)
        {
            int solverIndex = dynamicPinBatch.particleIndices[i];
            if(solverIndex != -1)
            {
                if(!originalParticleColors.ContainsKey(solverIndex))
                {
                    originalParticleColors[solverIndex] = solver.colors[solverIndex];
                }
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