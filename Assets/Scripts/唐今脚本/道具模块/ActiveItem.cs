using UnityEngine;

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
}