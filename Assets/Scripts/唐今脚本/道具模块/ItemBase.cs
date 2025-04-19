using UnityEngine;

public interface IInteractable
{
    string ItemName { get; }
    void OnHighlight(bool enable);
    void Release();
}

public interface IUsable
{
    void UseStart();
    void UseEnd();
}

public enum HandType { None, Left, Right }

public abstract class ItemBase : MonoBehaviour, IInteractable
{
    [SerializeField] protected string itemName;
    protected bool isGrabbed;

    public string ItemName => itemName;

    public virtual void OnHighlight(bool enable)
    {
    }

    public virtual void Grab()
    {

    }

    public virtual void Release()
    {

    }
}