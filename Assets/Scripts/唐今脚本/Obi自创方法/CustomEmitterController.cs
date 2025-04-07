using UnityEngine;
using Obi;

[RequireComponent(typeof(ObiEmitter))]
public class CustomEmitterController : MonoBehaviour
{
    private ObiEmitter _emitter;
    private bool _hasEmitted;

    private void Awake()
    {
        _emitter = GetComponent<ObiEmitter>();
        
        _emitter.emissionMethod = ObiEmitter.EmissionMethod.MANUAL;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Space) || _hasEmitted) return;
        EmitAllParticles();
        _hasEmitted = true;
    }

    private void EmitAllParticles()
    {
        if (!_emitter || !_emitter.isLoaded) return;

        var particlesToEmit = _emitter.particleCount - _emitter.activeParticleCount;
        
        for (var i = 0; i < particlesToEmit; i++)
        {
            _emitter.EmitParticle(0);
        }
        
        _emitter.SimulationStart(Time.deltaTime, Time.deltaTime);
    }
}