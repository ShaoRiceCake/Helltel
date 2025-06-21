using UnityEngine;
using Obi;
using System.Collections.Generic;

/// <summary>
/// 通过动态创建 Pin 约束来防止穿透，并暂时移除自动脱离逻辑以观察最大承压效果。
/// (V11: 动态约束“只加不减”版 - 基于V9，移除自动脱离检测)
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
    [Tooltip("Pin约束的硬度 (0-1)。")]
    [Range(0f, 1f)]
    public float pinStiffness = 1f;

    // [Header("脱离检测 (暂时禁用)")]
    // [Tooltip("当粒子与其锚定点的距离超过此值时，释放Pin约束。")]
    // public float disengagementDistance = 0.1f;

    [Header("可视化")]
    public bool enableVisualization = true;
    public Color pinnedParticleColor = Color.cyan;

    // --- 内部状态 ---
    private class PinSlot
    {
        public bool IsActive = false;
        public int ParticleSolverIndex = -1;
        // public ObiColliderBase Collider = null; // 在这个版本中暂时不需要存储 Collider
    }
    private PinSlot[] pinSlots;

    private ObiSoftbody softbody;
    private ObiSolver solver;
    private ObiPinConstraintsData pinConstraintsData;
    private ObiPinConstraintsBatch dynamicPinBatch;

    // --- 生命周期 ---
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
        pinConstraintsData = softbody.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiPinConstraintsData;

        if (pinConstraintsData == null)
        {
            Debug.LogError("DynamicPinStiffener: 软体上必须启用 'Pin Constraints' 才能工作。", this);
            enabled = false; return;
        }

        if (dynamicPinBatch == null)
        {
            dynamicPinBatch = pinConstraintsData.CreateBatch();
            pinSlots = new PinSlot[pinPoolSize];
            for (int i = 0; i < pinPoolSize; ++i)
            {
                pinSlots[i] = new PinSlot();
                dynamicPinBatch.AddConstraint(-1, null, Vector3.zero, Quaternion.identity, 1f, 1f);
            }
            dynamicPinBatch.activeConstraintCount = pinPoolSize;
        }
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

    // --- 核心逻辑 ---

    void LateUpdate()
    {
        // *** 核心修改：移除了 LateUpdate 中的所有逻辑 ***
        // 因为我们现在是“只加不减”，所以不需要在 LateUpdate 中做任何事情。
        // 颜色更新在 OnCollision 中触发即可。
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
            if (freeSlotIndex == -1) break; 

            var otherCollider = ObiColliderWorld.GetInstance().colliderHandles[GetColliderIndexFromContact(contact)].owner;
            if (otherCollider == null || !otherCollider.gameObject.activeInHierarchy) continue;
            if (!string.IsNullOrEmpty(colliderTag) && !otherCollider.CompareTag(colliderTag)) continue;
            
            Vector3 pinOffset = otherCollider.transform.InverseTransformPoint(contact.pointB);
            if (float.IsNaN(pinOffset.x)) continue;

            ActivatePin(freeSlotIndex, particleSolverIndex, otherCollider, pinOffset);
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            softbody.SetConstraintsDirty(Oni.ConstraintType.Pin);
            UpdateColors(); // 只在有新 Pin 添加时更新颜色
        }
    }

    // --- Pin 管理 ---

    private void ActivatePin(int slotIndex, int particleSolverIndex, ObiColliderBase collider, Vector3 offset)
    {
        pinSlots[slotIndex].IsActive = true;
        pinSlots[slotIndex].ParticleSolverIndex = particleSolverIndex;

        dynamicPinBatch.particleIndices[slotIndex] = particleSolverIndex;
        dynamicPinBatch.pinBodies[slotIndex] = collider.Handle;
        dynamicPinBatch.colliderIndices[slotIndex] = collider.Handle.index;
        dynamicPinBatch.offsets[slotIndex] = offset;
        dynamicPinBatch.stiffnesses[slotIndex * 2] = 1f - pinStiffness; 
        dynamicPinBatch.stiffnesses[slotIndex * 2 + 1] = 1f;           
    }
    
    // DeactivatePin 在这个版本中不被调用
    // private void DeactivatePin(int slotIndex) { ... }
    
    private int FindFreePinSlot()
    {
        for (int i = 0; i < pinPoolSize; i++)
        {
            if (!pinSlots[i].IsActive) return i;
        }
        return -1;
    }

    private bool IsParticleAlreadyPinned(int particleSolverIndex)
    {
        for (int i = 0; i < pinPoolSize; i++)
        {
            if (pinSlots[i].IsActive && pinSlots[i].ParticleSolverIndex == particleSolverIndex)
                return true;
        }
        return false;
    }

    // --- 辅助与可视化 ---
    
    private readonly Dictionary<int, Color> originalParticleColors = new Dictionary<int, Color>();

    private void UpdateColors()
    {
        if (!enableVisualization) return;
        
        // 我们只添加颜色，不恢复，因为 Pin 约束现在是永久的
        for (int i = 0; i < pinPoolSize; ++i)
        {
            if (pinSlots[i].IsActive)
            {
                int solverIndex = pinSlots[i].ParticleSolverIndex;
                if (!originalParticleColors.ContainsKey(solverIndex))
                {
                    originalParticleColors[solverIndex] = solver.colors[solverIndex];
                    solver.colors[solverIndex] = pinnedParticleColor;
                }
            }
        }
        solver.colors.Upload();
    }

    private void RestoreAllParticleColors()
    {
        if (originalParticleColors.Count > 0 && solver != null)
        {
            foreach (var pair in originalParticleColors)
            {
                if (pair.Key >= 0 && pair.Key < solver.colors.count)
                   solver.colors[pair.Key] = pair.Value;
            }
            solver.colors.Upload();
        }
        originalParticleColors.Clear();
    }

    private int GetParticleSolverIndexFromContact(Oni.Contact contact)
    {
        if (IsParticleFromOurSoftbody(contact.bodyA)) return contact.bodyA;
        if (IsParticleFromOurSoftbody(contact.bodyB)) return contact.bodyB;
        return -1;
    }

    private int GetColliderIndexFromContact(Oni.Contact contact)
    {
        return IsParticleFromOurSoftbody(contact.bodyA) ? contact.bodyB : contact.bodyA;
    }
    
    private bool IsParticleFromOurSoftbody(int particleSolverIndex)
    {
        if (solver == null || particleSolverIndex < 0 || particleSolverIndex >= solver.particleToActor.Length) return false;
        var pInActor = solver.particleToActor[particleSolverIndex];
        return pInActor != null && pInActor.actor == softbody;
    }
}