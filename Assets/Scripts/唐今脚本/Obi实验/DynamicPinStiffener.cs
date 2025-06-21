using UnityEngine;
using Obi;
using System.Collections.Generic;

/// <summary>
/// (V12: 混合模式 - 使用 Pin 约束定义硬度，并手动强制定位以确保跟随)
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
    public bool logPinStatus = false;

    // --- 内部状态 ---
    private class PinSlot
    {
        public bool IsActive = false;
        public int ParticleSolverIndex = -1;
        public ObiColliderBase Collider = null;
        public Vector3 LocalOffset;
    }
    private PinSlot[] pinSlots;
    private int activePinCount = 0;

    private ObiSoftbody softbody;
    private ObiSolver solver;
    private ObiPinConstraintsData pinConstraintsData;
    private ObiPinConstraintsBatch dynamicPinBatch;

    // --- 生命周期 (与V11.1完全相同) ---
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
            Debug.LogError($"[{this.name}] ...", this);
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
        activePinCount = 0;
        dynamicPinBatch = null;
        pinConstraintsData = null;
    }

    // --- 核心逻辑 ---

    void LateUpdate()
    {
        if (!softbody.isLoaded || activePinCount == 0) return;

        // *** 核心修正：手动强制定位 ***
        // 遍历所有激活的 Pin
        for (int i = 0; i < pinPoolSize; ++i)
        {
            if (pinSlots[i].IsActive)
            {
                var slot = pinSlots[i];
                var targetCollider = slot.Collider;

                if (targetCollider == null || !targetCollider.gameObject.activeInHierarchy)
                    continue;

                // 1. 计算出粒子“应该在”的世界位置 (在求解器空间中)
                Matrix4x4 attachmentMatrix = solver.transform.worldToLocalMatrix * targetCollider.transform.localToWorldMatrix;
                Vector3 targetPosition = attachmentMatrix.MultiplyPoint3x4(slot.LocalOffset);

                // 2. 强制将粒子位置和速度设置为目标状态
                // 注意：我们只设置位置，让求解器在下一次迭代中自己计算速度，这样更自然。
                // 如果需要更强的“粘滞”，可以同时设置速度为0。
                solver.positions[slot.ParticleSolverIndex] = targetPosition;
                // solver.velocities[slot.ParticleSolverIndex] = Vector3.zero; // 可选：如果需要完全静止
            }
        }

        // 调试日志
        if (logPinStatus && Time.frameCount % 120 == 0)
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
            UpdateColors(); 
        }
    }

    private void ActivatePin(int slotIndex, int particleSolverIndex, ObiColliderBase collider, Vector3 offset)
    {
        // 更新我们自己的状态跟踪器
        var slot = pinSlots[slotIndex];
        slot.IsActive = true;
        slot.ParticleSolverIndex = particleSolverIndex;
        slot.Collider = collider;
        slot.LocalOffset = offset;
        activePinCount++;

        // 更新 Batch 数据（仍然需要，以定义硬度）
        dynamicPinBatch.particleIndices[slotIndex] = particleSolverIndex;
        dynamicPinBatch.pinBodies[slotIndex] = collider.Handle;
        dynamicPinBatch.colliderIndices[slotIndex] = collider.Handle.index;
        dynamicPinBatch.offsets[slotIndex] = offset;
        dynamicPinBatch.stiffnesses[slotIndex * 2] = 1f - pinStiffness; 
        dynamicPinBatch.stiffnesses[slotIndex * 2 + 1] = 1f;           

        if (logPinStatus)
        {
            Debug.Log($"<color=lime>[{this.name}] Pin Activated! Slot: {slotIndex}</color>\n" +
                      $"- Particle (Solver Index): {particleSolverIndex}, Target Collider: {collider.name}, Local Offset: {offset.ToString("F4")}");
        }
    }
    
    // ... (FindFreePinSlot, IsParticleAlreadyPinned, LogCurrentStatus, UpdateColors, RestoreAllParticleColors, Get...FromContact, IsParticleFromOurSoftbody 与V11.1相同)
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
                if (loggedPins >= 5) { status += "  - ... (and more)\n"; break; }
            }
        }
        if (activePinCount == 0) status += "  No active pins.";
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