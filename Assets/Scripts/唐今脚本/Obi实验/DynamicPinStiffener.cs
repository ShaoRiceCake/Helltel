using UnityEngine;
using Obi;
using System.Collections.Generic;

/// <summary>
/// (V11.1: 稳定框架 + 深度监控版 - 基于V11，增加详细日志)
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

    [Header("可视化与调试")]
    public bool enableVisualization = true;
    public Color pinnedParticleColor = Color.cyan;
    [Tooltip("勾选后，将持续在控制台打印当前激活的Pin约束状态。")]
    public bool logPinStatus = true;


    // --- 内部状态 ---
    private class PinSlot
    {
        public bool IsActive = false;
        public int ParticleSolverIndex = -1;
    }
    private PinSlot[] pinSlots;
    private int activePinCount = 0;

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
            Debug.LogError($"[{this.name}] DynamicPinStiffener: 软体上必须启用 'Pin Constraints' 才能工作。", this);
            enabled = false; return;
        }

        if (dynamicPinBatch == null)
        {
            dynamicPinBatch = pinConstraintsData.CreateBatch();
            pinSlots = new PinSlot[pinPoolSize];
            for (int i = 0; i < pinPoolSize; ++i)
            {
                pinSlots[i] = new PinSlot();
                // 使用 AddConstraint 来正确初始化 Batch 内部列表的大小
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
        activePinCount = 0;
        dynamicPinBatch = null;
        pinConstraintsData = null;
    }

    // --- 核心逻辑 ---

    void LateUpdate()
    {
        // V11.1 中 LateUpdate 保持为空，因为我们是“只加不减”
        
        // ** DEBUG LOGGING **
        if (logPinStatus && Time.frameCount % 120 == 0) // 每 2 秒打印一次状态
        {
            LogCurrentStatus();
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
                if (logPinStatus) Debug.LogWarning($"[{this.name}] Pin pool is full! Cannot create more pins.");
                break; 
            }

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
            UpdateColors(); 
        }
    }

    // --- Pin 管理 ---

    private void ActivatePin(int slotIndex, int particleSolverIndex, ObiColliderBase collider, Vector3 offset)
    {
        pinSlots[slotIndex].IsActive = true;
        pinSlots[slotIndex].ParticleSolverIndex = particleSolverIndex;
        activePinCount++;

        dynamicPinBatch.particleIndices[slotIndex] = particleSolverIndex;
        dynamicPinBatch.pinBodies[slotIndex] = collider.Handle;
        dynamicPinBatch.colliderIndices[slotIndex] = collider.Handle.index;
        dynamicPinBatch.offsets[slotIndex] = offset;
        dynamicPinBatch.stiffnesses[slotIndex * 2] = 1f - pinStiffness; 
        dynamicPinBatch.stiffnesses[slotIndex * 2 + 1] = 1f;           

        // ** DEBUG LOGGING **
        if (logPinStatus)
        {
            Debug.Log($"<color=lime>[{this.name}] Pin Activated! Slot: {slotIndex}</color>\n" +
                      $"- Particle (Solver Index): {particleSolverIndex}\n" +
                      $"- Target Collider: {collider.name} (Handle Index: {collider.Handle.index})\n" +
                      $"- Local Offset: {offset.ToString("F4")}\n" +
                      $"- Stiffness/Compliance: {pinStiffness} / {1f-pinStiffness}\n" +
                      $"- Total Active Pins: {activePinCount}");
        }
    }
    
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

    // --- 调试与可视化 ---
    
    private void LogCurrentStatus()
    {
        string status = $"<color=yellow>[{this.name}] Status Update. Total Active Pins: {activePinCount}</color>\n";
        int loggedPins = 0;
        for (int i = 0; i < pinPoolSize; i++)
        {
            if (pinSlots[i].IsActive)
            {
                status += $"  - Slot {i}: Particle {dynamicPinBatch.particleIndices[i]}, Target Handle: {dynamicPinBatch.pinBodies[i].index}, Compliance: {dynamicPinBatch.stiffnesses[i * 2]:F2}\n";
                loggedPins++;
                if (loggedPins >= 5) // 最多打印5个，防止刷屏
                {
                    status += "  - ... (and more)\n";
                    break;
                }
            }
        }
        if (activePinCount == 0)
        {
            status += "  No active pins.";
        }
        Debug.Log(status);
    }
    
    private readonly Dictionary<int, Color> originalParticleColors = new Dictionary<int, Color>();

    private void UpdateColors()
    {
        if (!enableVisualization) return;
        
        for (int i = 0; i < pinPoolSize; ++i)
        {
            if (pinSlots[i].IsActive)
            {
                int solverIndex = pinSlots[i].ParticleSolverIndex;
                if (solverIndex >= 0 && !originalParticleColors.ContainsKey(solverIndex))
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