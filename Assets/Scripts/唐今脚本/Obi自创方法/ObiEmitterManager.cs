using Obi;
using UnityEngine;

[RequireComponent(typeof(ObiEmitter))]
public class ObiEmitterManager : MonoBehaviour
{
    private ObiEmitter _obiEmitter;
    private bool _isSubscribed;

    public bool forceManualEmission = true;

    private void Start()
    {
        _obiEmitter = GetComponent<ObiEmitter>();
        if (_obiEmitter == null)
        {
            Debug.LogError("ObiEmitterController requires an ObiEmitter component");
            return;
        }

        if (!forceManualEmission) return;
        _obiEmitter.emissionMethod = ObiEmitter.EmissionMethod.MANUAL;
        _obiEmitter.minPoolSize = 0.98f;
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (_isSubscribed) return;
        EventBus<BloodSprayEvent>.Subscribe(HandleBloodSprayEvent, this);
        _isSubscribed = true;
    }

    private void UnsubscribeFromEvents()
    {
        if (!_isSubscribed) return;
        EventBus<BloodSprayEvent>.UnsubscribeAll(this);
        _isSubscribed = false;
    }

    private void HandleBloodSprayEvent(BloodSprayEvent sprayEvent)
    {
        if (!_obiEmitter || !isActiveAndEnabled) return;

        transform.position = sprayEvent.spawnPosition;
        transform.rotation = sprayEvent.spawnRotation;

        _obiEmitter.speed = sprayEvent.emissionSpeed;
        _obiEmitter.randomDirection = sprayEvent.emissionRandomness;

        TriggerBurst(sprayEvent.emitterCount);

    }

    private System.Collections.IEnumerator EmitParticlesOverTime(int totalCount)
    {
        var batchSize = Mathf.Min(10, totalCount);
        var emitted = 0;

        while(emitted < totalCount)
        {
            var toEmit = Mathf.Min(batchSize, totalCount - emitted);
            for(var i = 0; i < toEmit; i++)
            {
                _obiEmitter.EmitParticle(0);
            }
            emitted += toEmit;
            yield return null; 
        }
    }

    private void TriggerBurst(int burstCount)
    {
        _obiEmitter.emissionMethod = ObiEmitter.EmissionMethod.BURST;
        StartCoroutine(ExecuteBurst(burstCount));
    }

    private System.Collections.IEnumerator ExecuteBurst(int count)
    {
        var i = 0;
        for (; i < count; i++)
        {
            _obiEmitter.EmitParticle(0);
            yield return null; // 等待一帧避免卡顿
        }
        _obiEmitter.emissionMethod = ObiEmitter.EmissionMethod.MANUAL; // 切回手动模式
    }

    public void EmitAtPosition(Vector3 position, Quaternion rotation, int count, float speed, float randomness)
    {
        var sprayEvent = new BloodSprayEvent(position, rotation, count, speed, randomness);
        HandleBloodSprayEvent(sprayEvent);
    }
    
    public void KillAllParticles()
    {
        if(_obiEmitter != null)
            _obiEmitter.KillAll();
    }
}