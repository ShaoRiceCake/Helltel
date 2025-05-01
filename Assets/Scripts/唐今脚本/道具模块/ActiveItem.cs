using UnityEngine;
using UnityEngine.Events;

public abstract class ActiveItem : ItemBase, IUsable
{
    public override UnityEvent OnGrabbed { get; set; } = new UnityEvent();
    public override UnityEvent OnReleased { get; set; } = new UnityEvent();
    public UnityEvent OnUseStart { get; set; } = new UnityEvent();
    public UnityEvent OnUseEnd { get; set; } = new UnityEvent();
    
    public bool IsExhaust { get; set; }
    
    protected virtual void ExecuteUse()
    {
        Destroy(gameObject);
    }
    
}