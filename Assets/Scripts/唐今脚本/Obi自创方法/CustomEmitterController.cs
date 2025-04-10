using System;
using UnityEngine;
using Obi;

[RequireComponent(typeof(ObiEmitter))]
public class CustomEmitterController : MonoBehaviour
{
    private ObiEmitter _emitter;
    private bool _hasEmitted;
    private bool _isFinished = false;

    private void Awake()
    {
        _emitter = GetComponent<ObiEmitter>();

        if (!_emitter)
        {
            Debug.LogError("No ObiEmitter component found");
        }
        
        _emitter.enabled = false;
    }

    private void Update()
    {
        if (_isFinished) return;
        
        var allEmitted = _emitter.activeParticleCount == _emitter.particleCount;

        if (!allEmitted) return;
        _emitter.emissionMethod = ObiEmitter.EmissionMethod.MANUAL;
        _isFinished = true;
    }

    public void Emit()
    {
        if (_hasEmitted) return;
        
        EmitAllParticles();
        _hasEmitted = true;
    }

    private void EmitAllParticles()
    {
        _emitter.enabled = true;
    }
}
