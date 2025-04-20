using UnityEngine;
using UnityEngine.Events;

public abstract class ActiveItem : ItemBase, IUsable
{
    public virtual void UseStart()
    {
    }

    public virtual void UseEnd()
    {
    }

    protected virtual void ExecuteUse()
    {
        Destroy(gameObject);
    }

    public override UnityEvent OnGrabbed { get; set; } = new UnityEvent();
    public override UnityEvent OnReleased { get; set; } = new UnityEvent();
}