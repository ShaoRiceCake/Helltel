using UnityEngine;
using Obi;
using System.Collections.Generic;

/// <summary>
/// (V16: 最终健壮版 - 紧凑数组管理)
/// 修复了 Burst Job 中的 IndexOutOfRangeException 错误。
/// 使用 "Swap and Pop" 算法来高效管理约束池，确保传递给求解器的数据始终是有效的。
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
    
    // C# 层的状态追踪器
    private class PinInfo { public ObiColliderBase Collider = null; public Vector3 LocalOffset; }
    private PinInfo[] pinInfos;
    
    private ObiSoftbody softbody;
    private ObiSolver solver;
    private ObiPinConstraintsData pinConstraintsData;
    private ObiPinConstraintsBatch dynamicPinBatch;
    
    // *** 关键修改 1: 引入新的数据结构来追踪 ***
    // 用于快速查找粒子是否已固定，以及它在批处理中的索引
    private Dictionary<int, int> particleToBatchIndex = new Dictionary<int, int>();
    private readonly Dictionary<int, Color> originalParticleColors = new Dictionary<int, Color>();

    void OnEnable() { softbody = GetComponent<ObiSoftbody>(); softbody.OnBlueprintLoaded += OnBlueprintLoaded; if (softbody.isLoaded) OnBlueprintLoaded(softbody, softbody.sourceBlueprint); }
    void OnDisable() { softbody.OnBlueprintLoaded -= OnBlueprintLoaded; RemoveDynamicBatch(); RestoreAllParticleColors(); }
    private void OnBlueprintLoaded(ObiActor actor, ObiActorBlueprint blueprint) { SetupDynamicBatch(); SubscribeToSolver(); }
    private void SubscribeToSolver() { if (solver != null) solver.OnCollision -= Solver_OnCollision; solver = softbody.solver; if (solver != null) solver.OnCollision += Solver_OnCollision; }

    private void SetupDynamicBatch()
    {
        RemoveDynamicBatch();
        pinConstraintsData = softbody.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiPinConstraintsData;
        if (pinConstraintsData == null) { Debug.LogError($"[{this.name}] ObiSoftbody上缺少PinConstraints组件。请添加一个Obi Pin Constraints。", this); enabled = false; return; }

        dynamicPinBatch = new ObiPinConstraintsBatch();
        pinInfos = new PinInfo[pinPoolSize];
        for (int i = 0; i < pinPoolSize; ++i) 
        {
            // 预先分配内存，但此时它们都是无效的
            dynamicPinBatch.AddConstraint(-1, null, Vector3.zero, Quaternion.identity, 1f, 1f);
            pinInfos[i] = new PinInfo();
        }
        
        // *** 关键修改 2: 初始活动数量为0 ***
        // 我们从一个空的活动约束列表开始
        dynamicPinBatch.activeConstraintCount = 0;
        
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
        particleToBatchIndex.Clear();
    }

    void LateUpdate()
    {
        if (!softbody.isLoaded || dynamicPinBatch == null || !enableDisengagement) return;

        bool needsUpdate = false;
        // 倒序遍历以安全地在循环中移除元素
        for (int i = dynamicPinBatch.activeConstraintCount - 1; i >= 0; --i)
        {
            var info = pinInfos[i];
            var targetCollider = info.Collider;
            var particleSolverIndex = dynamicPinBatch.particleIndices[i];

            if (targetCollider == null || !targetCollider.gameObject.activeInHierarchy)
            {
                DeactivatePin(i);
                needsUpdate = true;
                continue;
            }
                
            Vector3 currentParticlePos = solver.positions[particleSolverIndex];
            Vector3 pinWorldPos = targetCollider.transform.TransformPoint(info.LocalOffset);

            if (Vector3.SqrMagnitude(currentParticlePos - pinWorldPos) > disengagementDistance * disengagementDistance)
            {
                DeactivatePin(i);
                needsUpdate = true;
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
        if (contacts.count == 0 || dynamicPinBatch == null) return;

        bool needsUpdate = false;
        for (int i = 0; i < contacts.count; ++i)
        {
            // 池已满，停止处理
            if (dynamicPinBatch.activeConstraintCount >= pinPoolSize)
            {
                 Debug.LogWarning("Pin约束池已满，无法创建新的约束。");
                 break;
            }

            Oni.Contact contact = contacts[i];
            int particleSolverIndex = GetParticleSolverIndexFromContact(contact);
            if (particleSolverIndex == -1 || particleToBatchIndex.ContainsKey(particleSolverIndex)) continue;
            
            var otherCollider = ObiColliderWorld.GetInstance().colliderHandles[GetColliderIndexFromContact(contact)].owner;
            if (otherCollider == null || !otherCollider.gameObject.activeInHierarchy) continue;
            if (!string.IsNullOrEmpty(colliderTag) && !otherCollider.CompareTag(colliderTag)) continue;
            
            Matrix4x4 bindMatrix = otherCollider.transform.worldToLocalMatrix * solver.transform.localToWorldMatrix;
            Vector3 pinOffset = bindMatrix.MultiplyPoint3x4(solver.positions[particleSolverIndex]);
            if (float.IsNaN(pinOffset.x)) continue;
            
            ActivatePin(particleSolverIndex, otherCollider, pinOffset);
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            softbody.SetConstraintsDirty(Oni.ConstraintType.Pin);
            UpdateColors(); 
        }
    }

    // *** 关键修改 3: 激活逻辑 ***
    private void ActivatePin(int particleSolverIndex, ObiColliderBase collider, Vector3 offset)
    {
        // 新的约束总是在活动列表的末尾
        int slotIndex = dynamicPinBatch.activeConstraintCount;

        dynamicPinBatch.particleIndices[slotIndex] = particleSolverIndex;
        dynamicPinBatch.pinBodies[slotIndex] = collider.Handle;
        dynamicPinBatch.colliderIndices[slotIndex] = collider.Handle.index;
        dynamicPinBatch.offsets[slotIndex] = offset;
        dynamicPinBatch.stiffnesses[slotIndex * 2] = 1f - pinStiffness; 
        dynamicPinBatch.stiffnesses[slotIndex * 2 + 1] = 1f;

        var info = pinInfos[slotIndex];
        info.Collider = collider;
        info.LocalOffset = offset;
        
        // 更新追踪字典和活动数量
        particleToBatchIndex[particleSolverIndex] = slotIndex;
        dynamicPinBatch.activeConstraintCount++;
    }

    // *** 关键修改 4: 停用逻辑 (Swap and Pop) ***
    private void DeactivatePin(int slotIndex)
    {
        // 1. 从追踪字典中移除要停用的粒子
        particleToBatchIndex.Remove(dynamicPinBatch.particleIndices[slotIndex]);

        // 2. 将活动数量减一
        dynamicPinBatch.activeConstraintCount--;
        
        // 3. 如果被停用的不是最后一个元素，则用最后一个元素的数据覆盖它
        int lastActiveIndex = dynamicPinBatch.activeConstraintCount;
        if (slotIndex < lastActiveIndex)
        {
            // 复制批处理数据
            dynamicPinBatch.particleIndices[slotIndex] = dynamicPinBatch.particleIndices[lastActiveIndex];
            dynamicPinBatch.pinBodies[slotIndex] = dynamicPinBatch.pinBodies[lastActiveIndex];
            dynamicPinBatch.colliderIndices[slotIndex] = dynamicPinBatch.colliderIndices[lastActiveIndex];
            dynamicPinBatch.offsets[slotIndex] = dynamicPinBatch.offsets[lastActiveIndex];
            dynamicPinBatch.stiffnesses[slotIndex * 2] = dynamicPinBatch.stiffnesses[lastActiveIndex * 2];
            dynamicPinBatch.stiffnesses[slotIndex * 2 + 1] = dynamicPinBatch.stiffnesses[lastActiveIndex * 2 + 1];

            // 复制C#追踪信息
            pinInfos[slotIndex].Collider = pinInfos[lastActiveIndex].Collider;
            pinInfos[slotIndex].LocalOffset = pinInfos[lastActiveIndex].LocalOffset;

            // 4. 更新被移动的粒子在字典中的索引
            particleToBatchIndex[dynamicPinBatch.particleIndices[slotIndex]] = slotIndex;
        }
    }
    
    // --- 颜色和辅助方法 ---
    private void UpdateColors()
    {
        if (!enableVisualization || solver == null) return;
        RestoreAllParticleColors();
        for(int i = 0; i < dynamicPinBatch.activeConstraintCount; ++i)
        {
            int solverIndex = dynamicPinBatch.particleIndices[i];
            if(!originalParticleColors.ContainsKey(solverIndex))
            {
                originalParticleColors[solverIndex] = solver.colors[solverIndex];
            }
            solver.colors[solverIndex] = pinnedParticleColor;
        }
        solver.colors.Upload();
    }
    
    private void RestoreAllParticleColors()
    {
        if(originalParticleColors.Count > 0 && solver != null)
        {
            foreach(var p in originalParticleColors)
            {
                if(p.Key >= 0 && p.Key < solver.colors.count) solver.colors[p.Key] = p.Value;
            }
            solver.colors.Upload();
        }
        originalParticleColors.Clear();
    }
    
    private int GetParticleSolverIndexFromContact(Oni.Contact contact) { if(IsParticleFromOurSoftbody(contact.bodyA)) return contact.bodyA; if(IsParticleFromOurSoftbody(contact.bodyB)) return contact.bodyB; return -1; }
    private int GetColliderIndexFromContact(Oni.Contact contact) { return IsParticleFromOurSoftbody(contact.bodyA) ? contact.bodyB : contact.bodyA; }
    private bool IsParticleFromOurSoftbody(int particleSolverIndex) { if(solver == null || particleSolverIndex < 0 || particleSolverIndex >= solver.particleToActor.Length) return false; var p = solver.particleToActor[particleSolverIndex]; return p != null && p.actor == softbody; }
}