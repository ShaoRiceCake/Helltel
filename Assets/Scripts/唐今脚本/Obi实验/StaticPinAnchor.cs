using UnityEngine;
using Obi;
using System.Collections.Generic;

/// <summary>
/// 通过将碰撞粒子“静态化”（invMass=0）并每帧强制其位置，来实现最强的防穿透效果。
/// (V10.1: 修正了获取原始逆质量时的索引错误)
/// </summary>
[RequireComponent(typeof(ObiSoftbody))]
public class StaticPinAnchor : MonoBehaviour
{
    [Header("核心配置")]
    [Tooltip("只对拥有此Tag的物体加强碰撞。留空则对所有碰撞生效。")]
    public string colliderTag;
    [Tooltip("是否同时固定粒子的朝向。")]
    public bool constrainOrientation = true;

    [Header("可视化")]
    public bool enableVisualization = true;
    public Color anchoredParticleColor = Color.magenta;

    
    // --- 内部状态 ---
    private class AnchorInfo
    {
        public ObiColliderBase AttachedCollider;
        public Vector3 LocalPositionOffset;
        public Quaternion LocalRotationOffset;
        // 存储原始信息以便恢复
        public float OriginalInvMass;
        public float OriginalInvRotationalMass;
    }
    private readonly Dictionary<int, AnchorInfo> anchoredParticles = new Dictionary<int, AnchorInfo>();

    private ObiSoftbody softbody;
    private ObiSolver solver;
    private ObiActorBlueprint blueprint;

    // --- 生命周期 ---
    void Start()
    {
        softbody = GetComponent<ObiSoftbody>();
        softbody.OnBlueprintLoaded += OnBlueprintLoaded;
        if (softbody.isLoaded) OnBlueprintLoaded(softbody, softbody.sourceBlueprint);
    }

    void OnDisable()
    {
        softbody.OnBlueprintLoaded -= OnBlueprintLoaded;
        if (solver != null) solver.OnCollision -= Solver_OnCollision;
        RestoreAllParticles();
    }
    
    private void OnBlueprintLoaded(ObiActor actor, ObiActorBlueprint blueprint)
    {
        // 使用 sharedBlueprint 以确保我们获取的是当前 actor 正在使用的版本
        this.blueprint = softbody.sharedBlueprint; 
        SubscribeToSolver();
    }

    private void SubscribeToSolver()
    {
        if (solver != null) solver.OnCollision -= Solver_OnCollision;
        solver = softbody.solver;
        if (solver != null) solver.OnCollision += Solver_OnCollision;
    }

    // --- 核心逻辑 ---

    void LateUpdate()
    {
        if (!softbody.isLoaded || anchoredParticles.Count == 0) return;

        bool needsPropertyUpdate = false;

        foreach (var pair in anchoredParticles)
        {
            int solverIndex = pair.Key;
            var anchorInfo = pair.Value;

            if (anchorInfo.AttachedCollider == null || !anchorInfo.AttachedCollider.gameObject.activeInHierarchy)
                continue;

            if (solver.invMasses[solverIndex] != 0)
            {
                solver.invMasses[solverIndex] = 0;
                needsPropertyUpdate = true;
            }
            if (constrainOrientation && softbody.usesOrientedParticles && solver.invRotationalMasses[solverIndex] != 0)
            {
                solver.invRotationalMasses[solverIndex] = 0;
                needsPropertyUpdate = true;
            }

            Transform targetTransform = anchorInfo.AttachedCollider.transform;
            Matrix4x4 attachmentMatrix = solver.transform.worldToLocalMatrix * targetTransform.localToWorldMatrix;

            solver.velocities[solverIndex] = Vector3.zero;
            solver.positions[solverIndex] = attachmentMatrix.MultiplyPoint3x4(anchorInfo.LocalPositionOffset);
            
            if (constrainOrientation && softbody.usesOrientedParticles)
            {
                solver.angularVelocities[solverIndex] = Vector3.zero;
                solver.orientations[solverIndex] = attachmentMatrix.rotation * anchorInfo.LocalRotationOffset;
            }
        }

        if (needsPropertyUpdate)
        {
            softbody.UpdateParticleProperties();
        }
    }

    private void Solver_OnCollision(ObiSolver solver, ObiNativeContactList contacts)
    {
        if (contacts.count == 0 || blueprint == null) return;

        bool anchoredNewParticle = false;

        for (int i = 0; i < contacts.count; ++i)
        {
            Oni.Contact contact = contacts[i];
            
            int particleSolverIndex = GetParticleSolverIndexFromContact(contact);
            if (particleSolverIndex == -1 || anchoredParticles.ContainsKey(particleSolverIndex)) continue;

            var otherCollider = ObiColliderWorld.GetInstance().colliderHandles[GetColliderIndexFromContact(contact)].owner;
            if (otherCollider == null || !otherCollider.gameObject.activeInHierarchy) continue;
            if (!string.IsNullOrEmpty(colliderTag) && !otherCollider.CompareTag(colliderTag)) continue;
            
            AnchorParticle(particleSolverIndex, otherCollider);
            anchoredNewParticle = true;
        }

        if (anchoredNewParticle && enableVisualization)
        {
            UpdateColors();
        }
    }

    private void AnchorParticle(int solverIndex, ObiColliderBase collider)
    {
        Transform targetTransform = collider.transform;
        Matrix4x4 bindMatrix = targetTransform.worldToLocalMatrix * solver.transform.localToWorldMatrix;
        
        Vector3 localPosOffset = bindMatrix.MultiplyPoint3x4(solver.positions[solverIndex]);
        Quaternion localRotOffset = softbody.usesOrientedParticles ? (bindMatrix.rotation * solver.orientations[solverIndex]) : Quaternion.identity;

        // *** 核心修正：使用正确的反向映射来获取 actorBlueprintIndex ***
        int actorBlueprintIndex = solver.particleToActor[solverIndex].indexInActor;

        var anchorInfo = new AnchorInfo
        {
            AttachedCollider = collider,
            LocalPositionOffset = localPosOffset,
            LocalRotationOffset = localRotOffset,
            OriginalInvMass = blueprint.invMasses[actorBlueprintIndex],
            OriginalInvRotationalMass = (softbody.usesOrientedParticles && actorBlueprintIndex < blueprint.invRotationalMasses.Length) ? blueprint.invRotationalMasses[actorBlueprintIndex] : 0
        };
        anchoredParticles[solverIndex] = anchorInfo;

        solver.invMasses[solverIndex] = 0;
        if (constrainOrientation && softbody.usesOrientedParticles)
            solver.invRotationalMasses[solverIndex] = 0;
        
        softbody.UpdateParticleProperties();
    }

    // --- 恢复与可视化 ---
    private readonly Dictionary<int, Color> originalParticleColors = new Dictionary<int, Color>();

    private void UpdateColors()
    {
        foreach (var pair in anchoredParticles)
        {
            int solverIndex = pair.Key;
            if (!originalParticleColors.ContainsKey(solverIndex))
            {
                originalParticleColors[solverIndex] = solver.colors[solverIndex];
                solver.colors[solverIndex] = anchoredParticleColor;
            }
        }
        if (originalParticleColors.Count > 0)
            solver.colors.Upload();
    }

    private void RestoreAllParticles()
    {
        if (solver == null || anchoredParticles.Count == 0) return;

        bool needsPropertyUpdate = false;
        foreach (var pair in anchoredParticles)
        {
            int solverIndex = pair.Key;
            var anchorInfo = pair.Value;
            
            if (solverIndex < solver.invMasses.count)
            {
                solver.invMasses[solverIndex] = anchorInfo.OriginalInvMass;
                needsPropertyUpdate = true;
            }
            if (constrainOrientation && softbody.usesOrientedParticles && solverIndex < solver.invRotationalMasses.count)
            {
                solver.invRotationalMasses[solverIndex] = anchorInfo.OriginalInvRotationalMass;
                needsPropertyUpdate = true;
            }
            
            if (originalParticleColors.TryGetValue(solverIndex, out Color originalColor))
            {
                 if (solverIndex < solver.colors.count)
                    solver.colors[solverIndex] = originalColor;
            }
        }
        
        if (needsPropertyUpdate) softbody.UpdateParticleProperties();
        if (originalParticleColors.Count > 0) solver.colors.Upload();

        anchoredParticles.Clear();
        originalParticleColors.Clear();
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