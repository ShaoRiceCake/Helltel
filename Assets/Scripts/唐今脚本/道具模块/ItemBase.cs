using UnityEngine;
using UnityEngine.Events;

public interface IInteractable
{
    string ItemName { get; }
    EGraspingState CurrentState { get; }
}

public interface IUsable
{
    void UseStart();
    void UseEnd();
}

public interface IGrabbable
{
    UnityEvent OnGrabbed { get; set; }
    UnityEvent OnReleased { get; set; }
    void UpdateGraspingState(EGraspingState newState);
}

public enum EGraspingState
{
    OnCaught,
    OnSelected,
    NotSelected
}

public abstract class ItemBase : MonoBehaviour, IInteractable, IGrabbable 
{
    [SerializeField] protected string itemName;
    public string ItemName => itemName;
    public abstract UnityEvent OnGrabbed { get; set; }
    public abstract UnityEvent OnReleased { get; set; }

    public EGraspingState CurrentState { get; private set; } = EGraspingState.NotSelected;

    public virtual void UpdateGraspingState(EGraspingState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        // Debug.Log($"{ItemName} state changed to: {CurrentState}");
    }
    
    protected virtual void Update()
    {
        // Debug.Log($"{ItemName} current state: {CurrentState}");
    }

    protected virtual void Awake(){}
}