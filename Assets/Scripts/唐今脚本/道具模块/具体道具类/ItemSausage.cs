using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ItemSausage : ActiveItem
{
    [Header("Recovery Settings")]
    [SerializeField] private int amount = 25; // 总恢复量
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject destroyEffectPrefab; // 摧毁时播放的特效预制体
    [SerializeField] private float destroyEffectDuration = 0.5f; // 摧毁特效持续时间
    protected override void Awake()
    {
        base.Awake();
        OnUseStart.AddListener(StartUseProcess);
    }

    private void StartUseProcess()
    {

        EventBus<LifeChangedEvent>.Publish(new LifeChangedEvent(amount));
        
        StartCoroutine(DestroyProcess());
    }

    private IEnumerator DestroyProcess()
    {
        if(TryGetComponent(out Renderer renderer1))
        {
            renderer1.enabled = false;
        }
        
        var effect = PlayEffect(destroyEffectPrefab, transform.position);
        if(destroyEffectPrefab)
        {
            yield return new WaitForSeconds(destroyEffectDuration);
            
            if(effect)
            {
                Destroy(effect);
            }
        }
        
        GameController.Instance.AddMaxHealth(amount);
        
        SelfDetach(true);
    }

    private static GameObject PlayEffect(GameObject effectPrefab, Vector3 position)
    {
        var effectInstance = Instantiate(
            effectPrefab, 
            position, 
            Quaternion.identity
        );
        
        Destroy(effectInstance, effectInstance.TryGetComponent(out ParticleSystem ps) ? ps.main.duration : 2f); 

        return effectInstance;
    }

    private void OnDestroy()
    {
        OnUseStart.RemoveListener(StartUseProcess);
    }
}