using Obi;
using UnityEngine;
using System;

public class SoftbodyCollisionDetector : MonoBehaviour
{
    [Tooltip("Layers that trigger collision detection")]
    public LayerMask triggerLayers;
    
    [Tooltip("The ObiSolver that handles the physics")]
    private ObiSolver _solver;
    
    [Tooltip("The ObiSoftbody whose collisions we want to detect")]
    private ObiSoftbody _softbody;
    
    private ObiCollider _obiCollider;

    private void Awake()
    {
        _obiCollider = GetComponent<ObiCollider>();
        if (_obiCollider == null)
        {
            Debug.LogError("BalloonCollisionDetector requires an ObiCollider component!");
            enabled = false;
            return;
        }

        FindPlayerComponents();
    }
    
    private void FindPlayerComponents()
    {
        var playerSolverObj = GameObject.Find("PlayerSolver");
        if (playerSolverObj == null)
        {
            Debug.LogError("找不到 PlayerSolver 对象！请确保玩家 Solver 在场景中。");
            enabled = false;
            return;
        }

        _solver = playerSolverObj.GetComponent<ObiSolver>();
        if (_solver == null)
        {
            Debug.LogError("PlayerSolver 对象上没有 ObiSolver 组件！");
            enabled = false;
            return;
        }

        _softbody = playerSolverObj.GetComponentInChildren<ObiSoftbody>();
        if (_softbody == null)
        {
            Debug.LogError("PlayerSolver 的子对象中没有 ObiSoftbody 组件！");
            enabled = false;
            return;
        }

        Debug.Log("成功找到玩家 Solver 和 Softbody！");
    }
    
    private void OnEnable()
    {
        if (_solver != null)
        {
            _solver.OnCollision += OnSolverCollision;
        }
    }

    private void OnDisable()
    {
        if (_solver != null)
        {
            _solver.OnCollision -= OnSolverCollision;
        }
    }

    private void OnSolverCollision(object sender, Obi.ObiNativeContactList contacts)
    {
        if (_obiCollider == null || _solver == null) return;

        var colliderWorld = ObiColliderWorld.GetInstance();

        for (var i = 0; i < contacts.count; i++)
        {
            if (contacts[i].distance >= 0) continue;

            var otherColliderHandle = contacts[i].bodyB;
            var otherCollider = colliderWorld.colliderHandles[otherColliderHandle].owner;

            if (otherCollider != _obiCollider) continue;

            var particleIndex = contacts[i].bodyA;
            var particleObject = _solver.particleToActor[particleIndex].actor?.gameObject;

            if (particleObject == null || (triggerLayers.value & (1 << particleObject.layer)) == 0)
                continue;
            
            Debug.Log($"与玩家碰撞！");
        }
    }
}