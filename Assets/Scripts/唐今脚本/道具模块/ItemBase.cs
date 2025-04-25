using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public interface IInteractable
{
    string ItemName { get; }
    EItemState CurrentState { get; }
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
    
    bool RequestStateChange(EItemState newState, int toolInstanceId = -1, ulong playerId = ulong.MaxValue);
}

public enum EItemState
{
    NotSelected,    // 未被选择（默认状态）
    Selected,       // 被选择（进入玩家检测范围）
    ReadyToGrab,    // 待抓取（手球体激活且最近）
    Grabbed         // 被抓取（玩家已抓取）
}

// 状态基类（保持不变）
public abstract class ItemState
{
    protected readonly ItemBase Item;
    protected EItemState stateType;

    public EItemState StateType => stateType;

    protected ItemState(ItemBase item) => this.Item = item;
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
}

// 各具体状态类实现（保持不变）
public class NotSelectedState : ItemState
{
    public NotSelectedState(ItemBase item) : base(item) => stateType = EItemState.NotSelected;
    public override void Enter() => Item.gameObject.layer = Item.OriginalLayer;
}

public class SelectedState : ItemState
{
    public SelectedState(ItemBase item) : base(item) => stateType = EItemState.Selected;
    public override void Enter() => Item.gameObject.layer = Item.OriginalLayer;
}

public class ReadyToGrabState : ItemState
{
    public ReadyToGrabState(ItemBase item) : base(item) => stateType = EItemState.ReadyToGrab;
    public override void Enter() => Item.gameObject.layer = Item.TargetLayer;
}

public class GrabbedState : ItemState
{
    public GrabbedState(ItemBase item) : base(item) => stateType = EItemState.Grabbed;
    public override void Enter()
    {
        Item.OnGrabbed?.Invoke();
        Item.gameObject.layer = Item.OriginalLayer;
    }
    public override void Exit()
    {
        if (Item)
        {
            Item.ClearGrabPermission();
        }
        Item.OnReleased?.Invoke();
    }}

[RequireComponent(typeof(NetworkObject))]
public abstract class ItemBase : NetworkBehaviour, IInteractable, IGrabbable
{
    [SerializeField] protected string itemName;
    public string ItemName => itemName;
    
    // 权限验证字段
    private ulong _currentGrabbingPlayerId = ulong.MaxValue;
    private int _currentGrabbingToolId = -1;
    private bool IsBeingGrabbed => _currentGrabbingToolId != -1;

    public abstract UnityEvent OnGrabbed { get; set; }
    public abstract UnityEvent OnReleased { get; set; }
    
    // 状态机相关
    private readonly Stack<ItemState> _stateStack = new();
    private readonly Dictionary<EItemState, ItemState> _stateDictionary = new();
    public EItemState CurrentState => _stateStack.Count > 0 ? _stateStack.Peek().StateType : EItemState.NotSelected;
    
    // 层级管理
    public int OriginalLayer { get; private set; }
    public int TargetLayer { get; private set; }
    
    protected virtual void Awake()
    {
        OriginalLayer = gameObject.layer;
        TargetLayer = LayerMask.NameToLayer("Item");
        
        // 初始化状态机
        _stateDictionary.Add(EItemState.NotSelected, new NotSelectedState(this));
        _stateDictionary.Add(EItemState.Selected, new SelectedState(this));
        _stateDictionary.Add(EItemState.ReadyToGrab, new ReadyToGrabState(this));
        _stateDictionary.Add(EItemState.Grabbed, new GrabbedState(this));
        
        // 初始状态
        ForceSetState(EItemState.NotSelected);
    }

    public bool RequestStateChange(EItemState newState, int toolInstanceId = -1, ulong playerId = ulong.MaxValue)
    {
        // 权限验证（抓取/释放）
        if (newState == EItemState.Grabbed && !TryAcquireGrabPermission(toolInstanceId, playerId))
            return false;
        
        if (CurrentState == EItemState.Grabbed && newState != EItemState.Grabbed && 
            !IsValidPermissionHolder(toolInstanceId, playerId))
            return false;

        // 状态转换规则
        switch (CurrentState)
        {
            case EItemState.NotSelected:
                if (newState == EItemState.Selected) PushState(newState);
                else return false;
                break;

            case EItemState.Selected:
                if (newState == EItemState.NotSelected) PopState();
                else if (newState == EItemState.ReadyToGrab) PushState(newState);
                else return false;
                break;

            case EItemState.ReadyToGrab:
                if (newState == EItemState.Selected) PopState();
                else if (newState == EItemState.Grabbed) PushState(newState);
                else return false;
                break;

            case EItemState.Grabbed:
                if (newState == EItemState.ReadyToGrab) PopState();
                else return false;
                break;
        }

        return true;
    }

    private void PushState(EItemState newState)
    {
        if (_stateStack.Count > 0) _stateStack.Peek().Exit();
        if (_stateDictionary.TryGetValue(newState, out var state))
        {
            _stateStack.Push(state);
            state.Enter();
        }
    }

    private void PopState()
    {
        if (_stateStack.Count == 0) return;
        _stateStack.Pop().Exit();
        if (_stateStack.Count > 0) _stateStack.Peek().Enter();
        else ForceSetState(EItemState.NotSelected);
    }

    private void ForceSetState(EItemState newState)
    {
        while (_stateStack.Count > 0) _stateStack.Pop().Exit();
        PushState(newState);
    }

    private bool TryAcquireGrabPermission(int toolInstanceId, ulong playerId)
    {
        if (IsBeingGrabbed)
            return _currentGrabbingToolId == toolInstanceId && 
                   _currentGrabbingPlayerId == playerId;

        _currentGrabbingToolId = toolInstanceId;
        _currentGrabbingPlayerId = playerId;
        return true;
    }

    private bool IsValidPermissionHolder(int toolId, ulong playerId)
    {
        return _currentGrabbingToolId == toolId && 
               _currentGrabbingPlayerId == playerId;
    }

    protected virtual void Update()
    {
        _stateStack.Peek().Update();
        Debug.Log($"[ItemState] {itemName}: {CurrentState} | OwnerID: {_currentGrabbingPlayerId}, HandID: {_currentGrabbingToolId}");
    }

    public void ClearGrabPermission()
    {
        _currentGrabbingToolId = -1;
        _currentGrabbingPlayerId = ulong.MaxValue;
    }
    
    // 便捷查询方法
    public bool IsNotSelected => CurrentState == EItemState.NotSelected;
    public bool IsSelected => CurrentState == EItemState.Selected;
    public bool IsReadyToGrab => CurrentState == EItemState.ReadyToGrab;
    public bool IsGrabbed => CurrentState == EItemState.Grabbed;
}