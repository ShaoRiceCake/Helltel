using UnityEngine;
using Obi;
using System.Collections.Generic;

/// <summary>
/// (V14: 纯粹动态约束版 - 移除强制定位，让 Pin 刚度回归)
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
    
    // 内部状态与生命周期方法与之前版本类似，但有细微调整
    private class PinSlot { public bool IsActive = false; public int ParticleSolverIndex = -1; public ObiColliderBase Collider = null; public Vector3 LocalOffset; }
    private PinSlot[] pinSlots;
    private ObiSoftbody softbody;
    private ObiSolver solver;
    private ObiPinConstraintsData pinConstraintsData;
    private ObiPinConstraintsBatch dynamicPinBatch;

    void OnEnable() { softbody = GetComponent<ObiSoftbody>(); softbody.OnBlueprintLoaded += OnBlueprintLoaded; if (softbody.isLoaded) OnBlueprintLoaded(softbody, softbody.sourceBlueprint); }
    void OnDisable() { softbody.OnBlueprintLoaded -= OnBlueprintLoaded; RemoveDynamicBatch(); RestoreAllParticleColors(); }
    private void OnBlueprintLoaded(ObiActor actor, ObiActorBlueprint blueprint) { SetupDynamicBatch(); SubscribeToSolver(); }
    private void SubscribeToSolver() { if (solver != null) solver.OnCollision -= Solver_OnCollision; solver = softbody.solver; if (solver != null) solver.OnCollision += Solver_OnCollision; }
    private void SetupDynamicBatch() { /* ...与V11.1/V12相同... */ pinConstraintsData = softbody.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiPinConstraintsData; if (pinConstraintsData == null) { Debug.LogError($"[{this.name}] ...", this); enabled = false; return; } if (dynamicPinBatch == null) { dynamicPinBatch = pinConstraintsData.CreateBatch(); pinSlots = new PinSlot[pinPoolSize]; for (int i = 0; i < pinPoolSize; ++i) { pinSlots[i] = new PinSlot(); dynamicPinBatch.AddConstraint(-1, null, Vector3.zero, Quaternion.identity, 1f, 1f); } dynamicPinBatch.activeConstraintCount = pinPoolSize; } }
    private void RemoveDynamicBatch() { /* ...与V11.1/V12相同... */ if (solver != null) solver.OnCollision -= Solver_OnCollision; solver = null; if (pinConstraintsData != null && dynamicPinBatch != null) { pinConstraintsData.RemoveBatch(dynamicPinBatch); if (softbody.isLoaded) softbody.SetConstraintsDirty(Oni.ConstraintType.Pin); } dynamicPinBatch = null; pinConstraintsData = null; }

    // --- 核心逻辑 ---

    void LateUpdate()
    {
        if (!softbody.isLoaded || dynamicPinBatch == null || !enableDisengagement) return;

        // *** 核心：柔化挣脱逻辑 ***
        bool needsUpdate = false;
        for (int i = 0; i < pinPoolSize; ++i)
        {
            if (pinSlots[i].IsActive)
            {
                var slot = pinSlots[i];
                var targetCollider = slot.Collider;

                if (targetCollider == null || !targetCollider.gameObject.activeInHierarchy)
                {
                    DeactivatePin(i);
                    needsUpdate = true;
                    continue;
                }
                
                Vector3 currentParticlePos = solver.positions[slot.ParticleSolverIndex];
                Vector3 pinWorldPos = targetCollider.transform.TransformPoint(slot.LocalOffset);

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
            UpdateColors(); // 需要在颜色上反映挣脱
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
        var slot = pinSlots[slotIndex];
        slot.IsActive = true;
        slot.ParticleSolverIndex = particleSolverIndex;
        slot.Collider = collider;
        slot.LocalOffset = offset;

        dynamicPinBatch.particleIndices[slotIndex] = particleSolverIndex;
        dynamicPinBatch.pinBodies[slotIndex] = collider.Handle;
        dynamicPinBatch.colliderIndices[slotIndex] = collider.Handle.index;
        dynamicPinBatch.offsets[slotIndex] = offset;
        dynamicPinBatch.stiffnesses[slotIndex * 2] = 1f - pinStiffness; 
        dynamicPinBatch.stiffnesses[slotIndex * 2 + 1] = 1f;
    }

    private void DeactivatePin(int slotIndex)
    {
        var slot = pinSlots[slotIndex];
        slot.IsActive = false;
        slot.ParticleSolverIndex = -1;
        slot.Collider = null;

        dynamicPinBatch.stiffnesses[slotIndex * 2] = 1f;
        dynamicPinBatch.stiffnesses[slotIndex * 2 + 1] = 1f;
    }
    
    // 其他所有辅助方法 (FindFreePinSlot, IsParticleAlreadyPinned, Log, Colors, Get...FromContact, IsParticleFromOurSoftbody) 都与 V12 完全相同
    private int FindFreePinSlot(){for (int i=0; i<pinPoolSize; i++){if (!pinSlots[i].IsActive) return i;} return -1;}
    private bool IsParticleAlreadyPinned(int particleSolverIndex){for (int i=0; i<pinPoolSize; i++){if (pinSlots[i].IsActive && pinSlots[i].ParticleSolverIndex == particleSolverIndex) return true;} return false;}
    private readonly Dictionary<int, Color> originalParticleColors = new Dictionary<int, Color>();
    private void UpdateColors(){if (!enableVisualization) return; for(int i=0;i<pinPoolSize;++i){if(pinSlots[i].IsActive){int sI=pinSlots[i].ParticleSolverIndex; if(sI>=0&&!originalParticleColors.ContainsKey(sI)){originalParticleColors[sI]=solver.colors[sI]; solver.colors[sI]=pinnedParticleColor;}}} solver.colors.Upload();}
    private void RestoreAllParticleColors(){if(originalParticleColors.Count>0&&solver!=null){foreach(var p in originalParticleColors){if(p.Key>=0&&p.Key<solver.colors.count)solver.colors[p.Key]=p.Value;}solver.colors.Upload();}originalParticleColors.Clear();}
    private int GetParticleSolverIndexFromContact(Oni.Contact contact){if(IsParticleFromOurSoftbody(contact.bodyA))return contact.bodyA; if(IsParticleFromOurSoftbody(contact.bodyB))return contact.bodyB; return-1;}
    private int GetColliderIndexFromContact(Oni.Contact contact){return IsParticleFromOurSoftbody(contact.bodyA)?contact.bodyB:contact.bodyA;}
    private bool IsParticleFromOurSoftbody(int particleSolverIndex){if(solver==null||particleSolverIndex<0||particleSolverIndex>=solver.particleToActor.Length)return false; var p=solver.particleToActor[particleSolverIndex]; return p!=null&&p.actor==softbody;}
}