using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ItemSausage : ActiveItem
{
    [Header("Recovery Settings")]
    [SerializeField] private int amount = 25; // 总恢复量
    protected override void Awake()
    {
        base.Awake();
        OnUseStart.AddListener(StartUseProcess);
    }

    private void StartUseProcess()
    {
        EventBus<LifeChangedEvent>.Publish(
            new LifeChangedEvent(amount));
    }

    private void OnDestroy()
    {
        OnUseStart.RemoveListener(StartUseProcess);
    }
}
