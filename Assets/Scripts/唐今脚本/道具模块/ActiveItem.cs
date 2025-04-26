using UnityEngine;
using UnityEngine.Events;

public abstract class ActiveItem : ItemBase, IUsable
{
    public override UnityEvent OnGrabbed { get; set; } = new UnityEvent();
    public override UnityEvent OnReleased { get; set; } = new UnityEvent();
    public UnityEvent OnUseStart { get; set; } = new UnityEvent();
    public UnityEvent OnUseEnd { get; set; } = new UnityEvent();

    protected virtual void ExecuteUse()
    {
        Destroy(gameObject);
    }
}