using UnityEngine;
using UnityEngine.Events;

public class ActiveItem : ItemBase, IUsable
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

    public override UnityEvent OnGrabbed { get; set; }
    public override UnityEvent OnReleased { get; set; }
    public override bool IsGrabbed { get; set; }
}