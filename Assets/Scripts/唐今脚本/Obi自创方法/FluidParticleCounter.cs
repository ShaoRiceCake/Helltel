using Obi;
using UnityEngine;
using System;
using System.Collections.Generic; // Added for List

public class FluidParticleCounter : MonoBehaviour
{
    [Tooltip("Layers that trigger particle deactivation")]
    public LayerMask triggerLayers;
    
    [Tooltip("The ObiSolver that handles the physics")]
    public ObiSolver solver;
    
    [Tooltip("List of emitters whose particles should be tracked")]
    public List<ObiEmitter> emitters = new List<ObiEmitter>();

    // Define a public event that others can subscribe to
    public event Action<int> OnParticleDestroyed;

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
        if (emitters.Count == 0) return;

        var colliderWorld = ObiColliderWorld.GetInstance();

        for (var i = 0; i < contacts.count; ++i)
        {
            if (!(contacts[i].distance < 0.01f)) continue;
            
            var col = colliderWorld.colliderHandles[contacts[i].bodyB].owner;
            if (!col || triggerLayers != (triggerLayers | (1 << col.gameObject.layer))) continue;
            
            var solverParticleIndex = solver.simplices[contacts[i].bodyA];
            
            foreach (var emitter in emitters)
            {
                if (!emitter) continue;
                
                var actorIndex = FindActorIndex(emitter, solverParticleIndex);
                if (actorIndex < 0) continue;
                StartCoroutine(DeactivateNextFrame(emitter, actorIndex));
                break; // Particle can only belong to one emitter
            }
        }
    }

    private static int FindActorIndex(ObiEmitter emitter, int solverParticleIndex)
    {
        for (var i = 0; i < emitter.solverIndices.count; i++)
        {
            if (emitter.solverIndices[i] == solverParticleIndex)
                return i;
        }
        return -1;
    }
    
    private System.Collections.IEnumerator DeactivateNextFrame(ObiEmitter emitter, int actorIndex)
    {
        yield return null; 
        emitter.DeactivateParticle(actorIndex);
        
        OnParticleDestroyed?.Invoke(actorIndex);
    }

    // Helper methods to manage emitters
    public void AddEmitter(ObiEmitter emitter)
    {
        if (emitter != null && !emitters.Contains(emitter))
        {
            emitters.Add(emitter);
        }
    }

    public void RemoveEmitter(ObiEmitter emitter)
    {
        if (emitter != null && emitters.Contains(emitter))
        {
            emitters.Remove(emitter);
        }
    }
}