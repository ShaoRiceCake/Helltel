using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sausage : ActiveItem
{
    [Header("Recovery Settings")]
    [SerializeField] private int Amount = 25; // 总恢复量
    protected override void Awake()
    {
        base.Awake();
        
        
        OnUseStart.AddListener(StartUseProcess);
       
    }
    void StartUseProcess()
    {
        EventBus<LifeChangedEvent>.Publish(
            new LifeChangedEvent(Amount));
    }
}
