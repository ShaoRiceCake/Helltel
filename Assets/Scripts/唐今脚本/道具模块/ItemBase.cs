using Unity.Netcode;
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

public abstract class ItemBase : NetworkBehaviour, IInteractable, IGrabbable 
{
    [SerializeField] protected string itemName;
    public string ItemName => itemName;
    public abstract UnityEvent OnGrabbed { get; set; }
    public abstract UnityEvent OnReleased { get; set; }

    private int _originalLayer;
    private int _targetLayer;

    public EGraspingState CurrentState { get; private set; } = EGraspingState.NotSelected;

    protected virtual void Awake()
    {
        _originalLayer = gameObject.layer;
        _targetLayer = LayerMask.NameToLayer("Item");
    }

    public virtual void UpdateGraspingState(EGraspingState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        UpdateLayerBasedOnState();
    }

    private void UpdateLayerBasedOnState()
    {
        // if (!IsLocalPlayer) return;

        switch (CurrentState)
        {
            case EGraspingState.OnSelected:
                gameObject.layer = _targetLayer;
                break;
            case EGraspingState.OnCaught:
            case EGraspingState.NotSelected:
            default:
                gameObject.layer = _originalLayer;
                break;
        }
    }

    protected virtual void Update()
    {
        // Debug.Log($"{ItemName} current state: {CurrentState}");
    }
}