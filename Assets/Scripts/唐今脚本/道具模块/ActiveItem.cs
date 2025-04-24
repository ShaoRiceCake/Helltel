// ActiveItem.cs
using UnityEngine;
using UnityEngine.Events;

public abstract class ActiveItem : ItemBase, IUsable
{
    private Rigidbody rb;
    private bool originalFreezeRotation;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            originalFreezeRotation = rb.freezeRotation;
        }

        OnGrabbed.AddListener(HandleGrabbed);
        OnReleased.AddListener(HandleReleased);
    }

    private void HandleGrabbed()
    {
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    private void HandleReleased()
    {
        if (rb != null)
        {
            rb.freezeRotation = originalFreezeRotation;
        }
    }

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