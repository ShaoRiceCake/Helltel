using Obi;
using UnityEngine;
using System;

public class SoftbodyCollisionDetector : MonoBehaviour
{
    // 只有这个层级的对象会产生玩家粒子碰撞检测，目前设定是PlayerDetect
    [Tooltip("Layers that trigger collision detection")]
    public LayerMask triggerLayers;
    
    [Tooltip("The ObiSolver that handles the physics")]
    public ObiSolver solver;
    
    [Tooltip("The ObiSoftbody whose collisions we want to detect")]
    public ObiSoftbody softbody;

    // 订阅这个事件，获取玩家对象粒子碰撞位置
    public event Action<Vector3> OnSoftbodyCollision;

    private void OnEnable()
    {
        if (solver != null)
        {
            solver.OnCollision += OnSolverCollision;
        }
    }

    private void OnDisable()
    {
        if (solver != null)
        {
            solver.OnCollision -= OnSolverCollision;
        }
    }

    private void OnSolverCollision(object sender, ObiNativeContactList contacts)
    {
        if (!softbody) return;

        var colliderWorld = ObiColliderWorld.GetInstance();

        for (var i = 0; i < contacts.count; ++i)
        {
            if (!(contacts[i].distance < 0.01f)) continue;
            
            var col = colliderWorld.colliderHandles[contacts[i].bodyB].owner;
            if (!col || triggerLayers != (triggerLayers | (1 << col.gameObject.layer))) continue;
            
            var solverParticleIndex = solver.simplices[contacts[i].bodyA];
            
            var actorIndex = FindActorIndex(softbody, solverParticleIndex);
            if (actorIndex < 0) continue;
            
            var particlePosition = solver.transform.TransformPoint(solver.positions[solverParticleIndex]);
            
            OnSoftbodyCollision?.Invoke(particlePosition);
            
            // Debug.Log("particlePosition:"+particlePosition);
            
            break;
        }
    }

    private static int FindActorIndex(ObiSoftbody softbody, int solverParticleIndex)
    {
        for (var i = 0; i < softbody.solverIndices.count; i++)
        {
            if (softbody.solverIndices[i] == solverParticleIndex)
                return i;
        }
        return -1;
    }
}