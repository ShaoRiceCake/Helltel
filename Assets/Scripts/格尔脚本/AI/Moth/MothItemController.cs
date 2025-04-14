using System.Collections;
using System.Collections.Generic;
using Helltal.Gelercat;
using UnityEngine;
using NPBehave;
public class MothItemController : GuestBase
{
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

}
