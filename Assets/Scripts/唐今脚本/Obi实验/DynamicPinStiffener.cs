using UnityEngine;
using Obi;
using System.Collections.Generic;

/// <summary>
/// (V13: Batch重建版 - 彻底模拟 ObiParticleAttachment 的初始化流程)
/// </summary>
[RequireComponent(typeof(ObiSoftbody))]
public class DynamicPinStiffener : MonoBehaviour
{
    [Header("核心配置")]
    [Tooltip("只对拥有此Tag的物体加强碰撞。留空则对所有碰撞生效。")]
    public string colliderTag;

    [Header("固定模式与强度")]
    [Tooltip("Pin约束的硬度 (0-1)。")]
    [Range(0f, 1f)]
    public float pinStiffness = 1f;

    [Header("可视化与调试")]
    public bool enableVisualization = true;
    public Color pinnedParticleColor = Color.cyan;
    public bool logPinStatus = false;

    // --- 内部状态 ---
    private struct PinInfo
    {
        public int ParticleSolverIndex;
        public ObiColliderBase Collider;
        public Vector3 LocalOffset;
    }
    // 我们不再使用“池”，而是用一个字典来维持当前所有激活的Pin的状态
    // Key: particle solver index, Value: PinInfo
    private readonly Dictionary<int, PinInfo> activePins = new Dictionary<int, PinInfo>();

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

        // 使用 new 创建我们自己的 batch，就像 ObiParticleAttachment 一样
        if (dynamicPinBatch == null)
        {
            dynamicPinBatch = new ObiPinConstraintsBatch();
            pinConstraintsData.AddBatch(dynamicPinBatch);
        }
    }

    private void RemoveDynamicBatch()
    {
        if (solver != null) solver.OnCollision -= Solver_OnCollision;
        if (pinConstraintsData != null && dynamicPinBatch != null)
        {
            pinConstraintsData.RemoveBatch(dynamicPinBatch);
            if (softbody.isLoaded) softbody.SetConstraintsDirty(Oni.ConstraintType.Pin);
        }
        activePins.Clear();
        dynamicPinBatch = null;
        pinConstraintsData = null;
    }

    // --- 核心逻辑 ---

    void LateUpdate()
    {
        if (!softbody.isLoaded || dynamicPinBatch == null) return;

        // ** 核心逻辑：每帧都清空并重建我们的 Batch **
        dynamicPinBatch.Clear();

        if (activePins.Count > 0)
        {
            float compliance = 1f - pinStiffness;
            foreach (var pair in activePins)
            {
                var pin = pair.Value;

                // 检查碰撞体是否有效，无效则从激活列表中移除
                if (pin.Collider == null || !pin.Collider.gameObject.activeInHierarchy)
                {
                    // activePins.Remove(pair.Key); // 在遍历中修改字典是危险的，暂时跳过
                    continue;
                }

                dynamicPinBatch.AddConstraint(pin.ParticleSolverIndex, pin.Collider, pin.LocalOffset, Quaternion.identity, compliance, 1f);
            }
        }
        
        // 即使 batch 为空，也要通知 solver，因为它可能上一帧不为空
        softbody.SetConstraintsDirty(Oni.ConstraintType.Pin);
        
        UpdateColors();

        if (logPinStatus)
        {
            LogStatus();
        }
    }

    private void Solver_OnCollision(ObiSolver solver, ObiNativeContactList contacts)
    {
        if (contacts.count == 0) return;

        for (int i = 0; i < contacts.count; ++i)
        {
            Oni.Contact contact = contacts[i];
            
            int particleSolverIndex = GetParticleSolverIndexFromContact(contact);
            // 如果粒子已存在，则不处理。这是“只加不减”逻辑
            if (particleSolverIndex == -1 || activePins.ContainsKey(particleSolverIndex)) continue;
            
            var otherCollider = ObiColliderWorld.GetInstance().colliderHandles[GetColliderIndexFromContact(contact)].owner;
            if (otherCollider == null || !otherCollider.gameObject.activeInHierarchy) continue;
            if (!string.IsNullOrEmpty(colliderTag) && !otherCollider.CompareTag(colliderTag)) continue;
            
            Vector3 pinOffset = otherCollider.transform.InverseTransformPoint(contact.pointB);
            if (float.IsNaN(pinOffset.x)) continue;

            // 添加到我们的激活列表
            activePins[particleSolverIndex] = new PinInfo
            {
                ParticleSolverIndex = particleSolverIndex,
                Collider = otherCollider,
                LocalOffset = pinOffset
            };
        }
    }

    // --- 调试与可视化 ---
    
    private readonly Dictionary<int, Color> originalParticleColors = new Dictionary<int, Color>();

    private void UpdateColors()
    {
        RestoreAllParticleColors();
        if (!enableVisualization || solver == null) return;

        foreach (var pair in activePins)
        {
            int solverIndex = pair.Key;
            if (!originalParticleColors.ContainsKey(solverIndex))
                originalParticleColors[solverIndex] = solver.colors[solverIndex];
            solver.colors[solverIndex] = pinnedParticleColor;
        }

        if (originalParticleColors.Count > 0)
            solver.colors.Upload();
    }

    private void RestoreAllParticleColors()
    {
        if (originalParticleColors.Count > 0 && solver != null)
        {
            // 创建一个副本进行遍历，因为我们可能会修改原始字典
            var keys = new List<int>(originalParticleColors.Keys);
            foreach (int solverIndex in keys)
            {
                // 如果一个粒子不再被钉住，恢复它的颜色
                if (!activePins.ContainsKey(solverIndex))
                {
                    if (solverIndex >= 0 && solverIndex < solver.colors.count)
                       solver.colors[solverIndex] = originalParticleColors[solverIndex];
                    originalParticleColors.Remove(solverIndex);
                }
            }
            solver.colors.Upload();
        }
    }

    private void LogStatus()
    {
        if (Time.frameCount % 60 == 0 && dynamicPinBatch.activeConstraintCount > 0) // 每60帧打印一次
        {
            var pin = activePins[dynamicPinBatch.particleIndices[0]]; // 只打印第一个pin的信息作为样本
            var colliderHandle = dynamicPinBatch.pinBodies[0];
            Debug.Log($"[Obi Pin Debug] Frame: {Time.frameCount}\n" +
                      $"Active Pins: {activePins.Count}, Batch Constraints: {dynamicPinBatch.activeConstraintCount}\n" +
                      $"Sample Pin (Particle {pin.ParticleSolverIndex}):\n" +
                      $"- Target Collider: {pin.Collider.name} (Handle Index: {colliderHandle.index})\n" +
                      $"- Local Offset: {pin.LocalOffset.ToString("F4")}\n" +
                      $"- Stiffness: {pinStiffness} (Compliance: {1f - pinStiffness})");
        }
    }

    // --- 辅助方法 ---
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