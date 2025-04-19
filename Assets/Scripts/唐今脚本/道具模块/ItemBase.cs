using UnityEngine;
using UnityEngine.Events;

public interface IInteractable
{
    string ItemName { get; }
    void OnHighlight(bool enable);
}

public interface IUsable
{
    void UseStart();
    void UseEnd();
}

public interface IGrabbable
{
    UnityEvent OnGrabbed { get; set;}
    UnityEvent OnReleased { get; set;}
    bool IsGrabbed { get; set; }
}

public abstract class ItemBase : MonoBehaviour, IInteractable, IGrabbable
{
    [SerializeField] protected string itemName;
    public string ItemName => itemName;

    public virtual void OnHighlight(bool enable)
    {
    }
    
    public abstract UnityEvent OnGrabbed { get; set; }
    public abstract UnityEvent OnReleased { get; set; }
    public abstract bool IsGrabbed { get; set; }
}