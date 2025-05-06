using Obi;
using UnityEngine;
using System;
using System.Collections.Generic;

public class FluidParticleCounter : MonoBehaviour
{
    [Tooltip("The ObiSolver that handles the physics")]
    public ObiSolver solver;
    
    public List<ObiEmitter> emitters = new List<ObiEmitter>();
    public List<ObiColliderBase> cleanableColliders = new List<ObiColliderBase>();

    private bool _canPlaySound = true;          // 控制音效播放的标记

    public event Action<int> OnParticleDestroyed;

    private void Start()
    {
        FindAllCleanableObjects();
        FindChildEmitters();
        
        EventBus<BloodSprayEvent>.Subscribe(OnNewEmitterCreated, this);
    }

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
        
        EventBus<BloodSprayEvent>.UnsubscribeAll(this);
    }

    private void OnNewEmitterCreated(BloodSprayEvent sprayEvent)
    {
        StartCoroutine(CheckForNewEmittersNextFrame());
    }

    private System.Collections.IEnumerator CheckForNewEmittersNextFrame()
    {
        yield return null;
        FindChildEmitters();
    }

    private void FindAllCleanableObjects()
    {
        cleanableColliders.Clear();
        var cleanableObjects = FindObjectsOfType<MonoBehaviour>();
        foreach (var obj in cleanableObjects)
        {
            if (obj is not ICleanable cleanable) continue;
            var obiCollider = cleanable.GetObiCollider();
            if (obiCollider != null)
            {
                cleanableColliders.Add(obiCollider);
            }
        }
    }

    private void FindChildEmitters()
    {
        if (!solver) return;

        var childEmitters = solver.GetComponentsInChildren<ObiEmitter>(true);
        
        foreach (var emitter in childEmitters)
        {
            if (!emitter || emitters.Contains(emitter)) continue;
            emitters.Add(emitter);
        }
        
        for (var i = emitters.Count - 1; i >= 0; i--)
        {
            if (!emitters[i] || emitters[i].transform.parent != solver.transform)
            {
                emitters.RemoveAt(i);
            }
        }
    }

    private void OnSolverCollision(object sender, ObiNativeContactList contacts)
    {
        if (emitters.Count == 0 || cleanableColliders.Count == 0) return;

        var colliderWorld = ObiColliderWorld.GetInstance();

        for (var i = 0; i < contacts.count; ++i)
        {
            if (!(contacts[i].distance < 0.01f)) continue;
            
            var obiColliderHandle = colliderWorld.colliderHandles[contacts[i].bodyB];
            var obiCollider = obiColliderHandle.owner;
            
            if (obiCollider == null || !cleanableColliders.Contains(obiCollider)) continue;
            
            var solverParticleIndex = solver.simplices[contacts[i].bodyA];
            
            foreach (var emitter in emitters)
            {
                if (!emitter) continue;
                
                var actorIndex = FindActorIndex(emitter, solverParticleIndex);
                if (actorIndex < 0) continue;
                StartCoroutine(DeactivateNextFrame(emitter, actorIndex));
                break;
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
        var lastCleanPosition = solver.transform.TransformPoint(solver.positions[solver.simplices[actorIndex]]);

        if (_canPlaySound)
        {
            StartCoroutine(PlayCleanSound(lastCleanPosition));
        }
    }

    private System.Collections.IEnumerator PlayCleanSound(Vector3 position)
    {
        _canPlaySound = false;
    
        // 播放音效
        AudioManager.Instance.Play("擦地", position, 1f);
        yield return new WaitForSeconds(1f);
        _canPlaySound = true;
    }
    
    public void RefreshCleanableObjects()
    {
        FindAllCleanableObjects();
    }
}