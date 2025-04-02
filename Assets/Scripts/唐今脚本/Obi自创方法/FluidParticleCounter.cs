using Obi;
using UnityEngine;

public class FluidParticleCounter : MonoBehaviour
{
    public LayerMask triggerLayers;
    public ObiSolver solver;
    public ObiEmitter emitter;

    private void OnEnable()
    {
        solver.OnCollision += OnSolverCollision;
    }

    private void OnDisable()
    {
        solver.OnCollision -= OnSolverCollision;
    }

    private void OnSolverCollision(object sender, ObiNativeContactList contacts)
    {
        var colliderWorld = ObiColliderWorld.GetInstance();

        for (var i = 0; i < contacts.count; ++i)
        {
            if (!(contacts[i].distance < 0.01f)) continue;
            
            var col = colliderWorld.colliderHandles[contacts[i].bodyB].owner;
            if (!col || triggerLayers != (triggerLayers | (1 << col.gameObject.layer))) continue;
            
            var solverParticleIndex = solver.simplices[contacts[i].bodyA];
            
            var actorIndex = FindActorIndex(solverParticleIndex);
            if (actorIndex >= 0)
            {
                StartCoroutine(DeactivateNextFrame(actorIndex));
            }
        }
    }

    private int FindActorIndex(int solverParticleIndex)
    {
        for (var i = 0; i < emitter.solverIndices.count; i++)
        {
            if (emitter.solverIndices[i] == solverParticleIndex)
                return i;
        }
        return -1;
    }

    private System.Collections.IEnumerator DeactivateNextFrame(int actorIndex)
    {
        yield return null; 
        emitter.DeactivateParticle(actorIndex);
    }
}