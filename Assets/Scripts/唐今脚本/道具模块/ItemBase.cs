using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

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
    bool RequestStateChange(EItemState newState, int toolInstanceId, ulong playerId);
}

public enum EItemState
{
    NotSelected,    // 未被选择（默认状态）
    Selected,       // 被选择（进入玩家检测范围）
    ReadyToGrab,    // 待抓取（手球体激活且最近）
    Grabbed         // 被抓取（玩家已抓取）
}

// 状态基类
public abstract class ItemState
{
    protected readonly ItemBase Item;
    protected EItemState stateType;

    public EItemState StateType => stateType;

    protected ItemState(ItemBase item)
    {
        this.Item = item;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
}

// 未被选择状态
public class NotSelectedState : ItemState
{
    public NotSelectedState(ItemBase item) : base(item)
    {
        stateType = EItemState.NotSelected;
    }

    public override void Enter()
    {
        Item.gameObject.layer = Item.OriginalLayer;
    }
}

// 被选择状态
public class SelectedState : ItemState
{
    public SelectedState(ItemBase item) : base(item)
    {
        stateType = EItemState.Selected;
    }

    public override void Enter()
    {
        Item.gameObject.layer = Item.OriginalLayer;

    }
}

// 待抓取状态
public class ReadyToGrabState : ItemState
{
    public ReadyToGrabState(ItemBase item) : base(item)
    {
        stateType = EItemState.ReadyToGrab;
    }

    public override void Enter()
    {
        Item.gameObject.layer = Item.TargetLayer;
    }
}

// 被抓取状态
public class GrabbedState : ItemState
{
    public GrabbedState(ItemBase item) : base(item)
    {
        stateType = EItemState.Grabbed;
    }

    public override void Enter()
    {
        Item.OnGrabbed?.Invoke();
        Item.gameObject.layer = Item.OriginalLayer;
    }

    public override void Exit()
    {
        Item.OnReleased?.Invoke();
    }
}

[RequireComponent(typeof(NetworkObject))]
public abstract class ItemBase : NetworkBehaviour, IInteractable, IGrabbable
{
    [SerializeField] protected string itemName;
    public string ItemName => itemName;
    
    // 抓取权限验证字段
    private ulong _currentGrabbingPlayerId = ulong.MaxValue;
    
    // 当前抓取状态（只读属性）
    private bool IsBeingGrabbed => CurrentGrabbingToolId != -1;
    private int CurrentGrabbingToolId { get; set; } = -1;

    public abstract UnityEvent OnGrabbed { get; set; }
    public abstract UnityEvent OnReleased { get; set; }
    
    // 状态机相关
    private readonly Stack<ItemState> _stateStack = new Stack<ItemState>();
    private readonly Dictionary<EItemState, ItemState> _stateDictionary = new Dictionary<EItemState, ItemState>();
    
    public EItemState CurrentState => _stateStack.Count > 0 ? _stateStack.Peek().StateType : EItemState.NotSelected;
    
    // 自动回退状态机协程
    private CancellationTokenSource _autoDeselectToken;
    [SerializeField] private float autoDeselectDelay = 0.5f; // 可配置的超时时间
    
    // 层级管理（高亮逻辑专用）
    public int OriginalLayer { get; private set; }
    public int TargetLayer { get; private set; }
    
    // 状态实例
    private NotSelectedState _notSelectedState;
    private SelectedState _selectedState;
    private ReadyToGrabState _readyToGrabState;
    private GrabbedState _grabbedState;

    protected virtual void Awake()
    {
        OriginalLayer = gameObject.layer;
        TargetLayer = LayerMask.NameToLayer("Item");
        
        // 初始化所有状态
        _notSelectedState = new NotSelectedState(this);
        _selectedState = new SelectedState(this);
        _readyToGrabState = new ReadyToGrabState(this);
        _grabbedState = new GrabbedState(this);
        
        // 添加到状态字典
        _stateDictionary.Add(EItemState.NotSelected, _notSelectedState);
        _stateDictionary.Add(EItemState.Selected, _selectedState);
        _stateDictionary.Add(EItemState.ReadyToGrab, _readyToGrabState);
        _stateDictionary.Add(EItemState.Grabbed, _grabbedState);
        
        // 初始状态
        PushState(EItemState.NotSelected);
    }
    
    /// <summary>
    /// 带权限验证的状态切换请求（核心方法）
    /// </summary>
    /// <param name="newState">目标状态</param>
    /// <param name="toolInstanceId">请求者的抓取工具实例ID</param>
    /// <param name="playerId">请求者的玩家网络ID</param>
    /// <returns>是否成功切换状态</returns>
    public bool RequestStateChange(EItemState newState, int toolInstanceId = -1, ulong playerId = ulong.MaxValue)
    {
        // ===== 1. 状态权限验证 =====
        // 抓取权限验证
        if (newState == EItemState.Grabbed)
        {
            if (!TryAcquireGrabPermission(toolInstanceId, playerId))
            {
                Debug.LogWarning($"抓取权限被拒绝！工具ID:{toolInstanceId} 玩家ID:{playerId}");
                return false;
            }
        }
        // 释放权限验证
        else if (CurrentState == EItemState.Grabbed && newState == EItemState.ReadyToGrab)
        {
            if (!TryReleaseGrabPermission(toolInstanceId, playerId))
            {
                Debug.LogWarning($"释放权限被拒绝！工具ID:{toolInstanceId} 玩家ID:{playerId}");
                return false;
            }
        }

        // ===== 2. 状态流规则验证 =====
        var stateChanged = false;
        
        switch (CurrentState)
        {
            case EItemState.NotSelected:
                if (newState == EItemState.Selected)
                {
                    PushState(EItemState.Selected);
                    stateChanged = true;
                }
                break;

            case EItemState.Selected:
                switch (newState)
                {
                    case EItemState.NotSelected:
                        PopState();
                        stateChanged = true;
                        break;
                    case EItemState.ReadyToGrab:
                        PushState(EItemState.ReadyToGrab);
                        stateChanged = true;
                        break;
                }
                break;

            case EItemState.ReadyToGrab:
                switch (newState)
                {
                    case EItemState.Selected:
                        PopState();
                        stateChanged = true;
                        break;
                    case EItemState.Grabbed:
                        PushState(EItemState.Grabbed);
                        stateChanged = true;
                        break;
                }
                break;

            case EItemState.Grabbed:
                if (newState == EItemState.ReadyToGrab)
                {
                    PopState();
                    stateChanged = true;
                }
                break;
        }

        // ===== 3. 自动回退管理 =====
        if (stateChanged)
        {
            // 进入Selected状态时启动超时协程
            if (newState == EItemState.Selected)
            {
                _autoDeselectToken?.Cancel();
                _autoDeselectToken = new CancellationTokenSource();
                StartCoroutine(AutoDeselectRoutine(_autoDeselectToken.Token));
            }
            // 离开Selected状态时取消协程
            else if (CurrentState == EItemState.Selected)
            {
                _autoDeselectToken?.Cancel();
            }
        }
        else
        {
            Debug.LogWarning($"非法状态转换：{CurrentState} → {newState}");
        }

        return stateChanged;
    }
    
    // 推入新状态
    private void PushState(EItemState newState)
    {
        if (_stateStack.Count > 0)
        {
            _stateStack.Peek().Exit();
        }

        if (!_stateDictionary.TryGetValue(newState, out var state)) return;
        _stateStack.Push(state);
        state.Enter();
    }
    
    // 弹出当前状态
    private void PopState()
    {
        if (_stateStack.Count <= 0) return;
        _stateStack.Pop().Exit();
            
        if (_stateStack.Count > 0)
        {
            _stateStack.Peek().Enter();
        }
        else
        {
            // 确保总是有一个状态
            PushState(EItemState.NotSelected);
        }
    }
    
    // 强制设置状态（慎用，仅用于特殊情况下重置状态）
    protected void ForceSetState(EItemState newState)
    {
        while (_stateStack.Count > 0)
        {
            _stateStack.Pop().Exit();
        }
        
        PushState(newState);
    }
    
    protected virtual void Update()
    {
        if (_stateStack.Count > 0)
        {
            _stateStack.Peek().Update();
        }
    }
    
    /// <summary>
    /// 尝试获取抓取权限
    /// </summary>
    /// <returns>true表示获取权限成功</returns>
    private bool TryAcquireGrabPermission(int toolInstanceId, ulong playerId)
    {
        // 如果道具未被抓取，直接获得权限
        if (IsBeingGrabbed)
            return CurrentGrabbingToolId == toolInstanceId &&
                   _currentGrabbingPlayerId == playerId;
        CurrentGrabbingToolId = toolInstanceId;
        _currentGrabbingPlayerId = playerId;
        return true;

        // 如果已经被同一个玩家的同一只手抓取，允许保持状态
        // 其他情况都拒绝
    }

    /// <summary>
    /// 释放抓取权限（必须验证权限）
    /// </summary>
    private bool TryReleaseGrabPermission(int toolInstanceId, ulong playerId)
    {
        // 只有当前持有者才能释放
        if (CurrentGrabbingToolId != toolInstanceId ||
            _currentGrabbingPlayerId != playerId) return false;
        CurrentGrabbingToolId = -1;
        _currentGrabbingPlayerId = ulong.MaxValue;
        return true;
    }
    
    
    /// <summary>
    /// 智能管理自动回退协程
    /// </summary>
    private void HandleAutoDeselect(EItemState newState)
    {
        // 取消之前的回退计时
        _autoDeselectToken?.Cancel();
        
        // 进入Selected状态时启动新计时
        if (newState != EItemState.Selected) return;
        _autoDeselectToken = new CancellationTokenSource();
        StartCoroutine(AutoDeselectRoutine(_autoDeselectToken.Token));
    }

    /// <summary>
    /// 自动回退协程（替代Update检测）
    /// </summary>
    private IEnumerator AutoDeselectRoutine(CancellationToken token)
    {
        yield return new WaitForSeconds(autoDeselectDelay);
        
        // 如果未被取消且仍处于Selected状态
        if (!token.IsCancellationRequested && 
            CurrentState == EItemState.Selected)
        {
            ForceSetState(EItemState.NotSelected);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        // 清理资源
        _autoDeselectToken?.Cancel();
        _autoDeselectToken?.Dispose();
    }
    
    // 便捷查询方法
    public bool IsNotSelected => CurrentState == EItemState.NotSelected;
    public bool IsSelected => CurrentState == EItemState.Selected;
    public bool IsReadyToGrab => CurrentState == EItemState.ReadyToGrab;
    public bool IsGrabbed => CurrentState == EItemState.Grabbed;
}