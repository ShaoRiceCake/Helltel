using System.Collections;
using System.Collections.Generic;
using Helltal.Gelercat;
using UnityEngine;
using NPBehave;
public class MothController : GuestBase, IHurtable
{
    
    public MothGroupController belongToGroup; // 所属的虫群

    private Root behaviorTree; 

    protected override void Start()
    {
        base.Start();
        behaviorTree = GetBehaviorTree();
#if UNITY_EDITOR
        Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
        debugger.BehaviorTree = behaviorTree;
#endif
    }

    




    // 实现 IHurtable 接口
    public void TakeDamage(int damage)
    {
        // 处理伤害逻辑
        Debug.Log("Take Damage: " + damage);
    }


}
