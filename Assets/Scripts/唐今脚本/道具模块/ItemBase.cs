using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;
using UnityEngine.Serialization;

// 可交互物品基础接口
public interface IInteractable
{
    string ItemName { get; }           // 物品唯一标识
    EItemState CurrentState { get; }    // 当前状态查询
}

// 可使用物品接口（事件驱动）
public interface IUsable
{
    UnityEvent OnUseStart { get; set; }  // 使用开始事件
    UnityEvent OnUseEnd { get; set; }     // 使用结束事件
}

// 可抓取物品接口（含权限管理）
public interface IGrabbable
{
    UnityEvent OnGrabbed { get; set; }   // 抓取成功事件
    UnityEvent OnReleased { get; set; }  // 释放成功事件
    
    // 状态变更请求（含权限验证）
    bool RequestStateChange(EItemState newState, int toolInstanceId = -1, ulong playerId = ulong.MaxValue);
}

/* 物品状态枚举 */
public enum EItemState
{
    NotSelected,    // 默认状态：未被玩家检测到
    Selected,       // 高亮状态：进入玩家检测范围
    ReadyToGrab,    // 可交互状态：满足抓取条件（最近且朝向正确）
    Grabbed         // 被抓持状态：已绑定到玩家控制器
}

/* 状态模式实现 */
// 抽象状态基类（模板方法模式）
public abstract class ItemState
{
    protected readonly ItemBase Item;    // 绑定物品实例
    protected EItemState stateType;      // 状态类型标识

    public EItemState StateType => stateType;  // 状态类型访问器

    protected ItemState(ItemBase item) => this.Item = item;  // 构造注入
    
    // 状态生命周期方法（空实现允许子类选择性重写）
    public virtual void Enter() { }   // 状态进入时调用
    public virtual void Exit() { }    // 状态退出时调用  
    public virtual void Update() { } // 每帧更新调用
}

// 具体状态实现类
public class NotSelectedState : ItemState
{
    public NotSelectedState(ItemBase item) : base(item) => stateType = EItemState.NotSelected;
    public override void Enter() => Item.gameObject.layer = Item.OriginalLayer; // 恢复原始层级
}

public class SelectedState : ItemState
{
    public SelectedState(ItemBase item) : base(item) => stateType = EItemState.Selected;
    public override void Enter() => Item.gameObject.layer = Item.OriginalLayer; 

    // 保持原始层级（可扩展高亮逻辑）
}

public class ReadyToGrabState : ItemState
{
    public ReadyToGrabState(ItemBase item) : base(item) => stateType = EItemState.ReadyToGrab;
    public override void Enter() => Item.gameObject.layer = Item.TargetLayer; // 设置交互专用层级
}

public class GrabbedState : ItemState
{
    public GrabbedState(ItemBase item) : base(item) => stateType = EItemState.Grabbed;
    public override void Enter()
    {
        Item.OnGrabbed?.Invoke();                // 触发抓取事件
        Item.gameObject.layer = Item.OriginalLayer; // 恢复层级避免重复检测
    }
    public override void Exit()
    {
        if (Item) Item.ClearGrabPermission();    // 清理权限信息
        Item.OnReleased?.Invoke();               // 触发释放事件
    }
}

/* 物品基类实现 */
public abstract class ItemBase : MonoBehaviour, IInteractable, IGrabbable
{
    // 配置字段
    [SerializeField] protected string itemName;  // 物品标识（需在Inspector设置）
    public string ItemName => itemName;          // 接口实现
    
    // 权限管理字段
    private ulong _currentGrabbingPlayerId = ulong.MaxValue; // 当前持有玩家ID
    private int _currentGrabbingToolId = -1;                 // 当前使用工具ID
    private bool IsBeingGrabbed => _currentGrabbingToolId != -1; // 抓取状态判断

    // 事件系统
    public abstract UnityEvent OnGrabbed { get; set; }   // 抓取事件实例
    public abstract UnityEvent OnReleased { get; set; }  // 释放事件实例

    // 状态机系统
    private readonly Stack<ItemState> _stateStack = new();          // 状态堆栈（支持状态嵌套）
    private readonly Dictionary<EItemState, ItemState> _stateDictionary = new(); // 状态注册表
    public EItemState CurrentState => _stateStack.Count > 0 ? _stateStack.Peek().StateType : EItemState.NotSelected; // 当前状态查询

    // 层级管理
    public int OriginalLayer { get; private set; }  // 物品原始层级（自动获取）
    public int TargetLayer { get; private set; }    // 交互专用层级（通常设置为"Item"层）

    // [Header("Grab Settings")]
    // [SerializeField] private Transform grabOffsetPoint; // 在Inspector中指定抓取偏移点（如尾部）
    //
    //
    // public (Vector3 position, Quaternion rotation) GetGrabPose()
    // {
    //     return grabOffsetPoint ?
    //         // 直接返回偏移点的世界坐标位置和旋转
    //         (grabOffsetPoint.position, grabOffsetPoint.rotation) :
    //         // 默认返回物体自身的世界坐标
    //         (transform.position, transform.rotation);
    // }
    //
    // // 返回抓取点的局部偏移（相对于物体自身坐标系）
    // public Vector3 GetLocalGrabOffsetPosition()
    // {
    //     return grabOffsetPoint != null ?
    //         // 计算偏移点相对于物体自身的局部位置
    //         transform.InverseTransformPoint(grabOffsetPoint.position) : Vector3.zero; // 默认无偏移
    // }
    //
    // // 返回抓取点的局部旋转偏移（可选）
    // public Quaternion GetLocalGrabOffsetRotation()
    // {
    //     if (grabOffsetPoint != null)
    //     {
    //         // 计算偏移点相对于物体自身的局部旋转
    //         return Quaternion.Inverse(transform.rotation) * grabOffsetPoint.rotation;
    //     }
    //     return Quaternion.identity;
    // }
    
    [Header("Orbit Settings")]
    [SerializeField]
    protected bool enableOrbit = false; // 环绕开关
    [SerializeField] protected Transform orbitCenter;    // 环绕中心点
    [SerializeField] protected float orbitRotationSpeed = 90f; // 旋转速度（度/秒）
    [SerializeField] protected Vector3 manualForwardDirection = Vector3.forward; // 手动设置的正朝向


    protected Vector3 _currentOrbitDirection; // 当前朝向向量
    
    /* 生命周期方法 */
    protected virtual void Awake()
    {
        // 层级初始化
        OriginalLayer = gameObject.layer;
        TargetLayer = LayerMask.NameToLayer("Item");
        
        // 状态机初始化
        _stateDictionary.Add(EItemState.NotSelected, new NotSelectedState(this));
        _stateDictionary.Add(EItemState.Selected, new SelectedState(this));
        _stateDictionary.Add(EItemState.ReadyToGrab, new ReadyToGrabState(this));
        _stateDictionary.Add(EItemState.Grabbed, new GrabbedState(this));

        ForceSetState(EItemState.NotSelected); // 初始状态设置
    }

    private void FixedUpdate()
    {
        if (enableOrbit)
        {
            UpdateOrbitRotation();
        }
    }

    protected virtual void UpdateOrbitRotation()
    {
        // 计算从中心点到物体的方向向量
        _currentOrbitDirection = transform.position - orbitCenter.position;

        // 如果方向向量长度接近0，跳过旋转
        if (_currentOrbitDirection.sqrMagnitude < 0.001f) return;

        // 计算目标旋转（考虑手动设置的正朝向）
        var targetRotation = Quaternion.LookRotation(-_currentOrbitDirection) * 
                             Quaternion.FromToRotation(Vector3.forward, manualForwardDirection);

        // 直接旋转Transform
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, 
            targetRotation, 
            orbitRotationSpeed * Time.fixedDeltaTime);
    }

    // 新增方法：手动设置正朝向
    public void SetManualForwardDirection(Vector3 newForward)
    {
        manualForwardDirection = newForward.normalized;
    }

    // 新增方法：获取当前正朝向
    public Vector3 GetCurrentForwardDirection()
    {
        return transform.TransformDirection(manualForwardDirection);
    }

    /* 核心状态转换方法 */
    public bool RequestStateChange(EItemState newState, int toolInstanceId = -1, ulong playerId = ulong.MaxValue)
    {
        // 抓取权限验证
        if (newState == EItemState.Grabbed && !TryAcquireGrabPermission(toolInstanceId, playerId))
            return false;
        
        // 释放权限验证
        if (CurrentState == EItemState.Grabbed && newState != EItemState.Grabbed && 
            !IsValidPermissionHolder(toolInstanceId, playerId))
            return false;

        // 状态转换规则验证（有限状态机）
        switch (CurrentState)
        {
            case EItemState.NotSelected:
                if (newState == EItemState.Selected) PushState(newState);
                else return false;
                break;

            case EItemState.Selected:
                switch (newState)
                {
                    case EItemState.NotSelected: PopState(); break;
                    case EItemState.ReadyToGrab: PushState(newState); break;
                    default: return false;
                }
                break;

            case EItemState.ReadyToGrab:
                switch (newState)
                {
                    case EItemState.Selected: PopState(); break;
                    case EItemState.Grabbed: PushState(newState); break;
                    default: return false;
                }
                break;

            case EItemState.Grabbed:
                if (newState == EItemState.ReadyToGrab) PopState();
                else return false;
                break;
        }
        return true;
    }

    /* 状态机内部方法 */
    private void PushState(EItemState newState)
    {
        if (_stateStack.Count > 0) _stateStack.Peek().Exit();
        if (!_stateDictionary.TryGetValue(newState, out var state)) return;
        _stateStack.Push(state);
        state.Enter();
    }

    private void PopState()
    {
        if (_stateStack.Count == 0) return;
        _stateStack.Pop().Exit();
        if (_stateStack.Count > 0) _stateStack.Peek().Enter();
        else ForceSetState(EItemState.NotSelected); // 空栈保护
    }

    private void ForceSetState(EItemState newState)
    {
        while (_stateStack.Count > 0) _stateStack.Pop().Exit();
        PushState(newState);
    }

    /* 权限管理方法 */
    private bool TryAcquireGrabPermission(int toolInstanceId, ulong playerId)
    {
        // 已持有权限时的验证
        if (IsBeingGrabbed)
            return _currentGrabbingToolId == toolInstanceId && 
                   _currentGrabbingPlayerId == playerId;

        // 首次获取权限
        _currentGrabbingToolId = toolInstanceId;
        _currentGrabbingPlayerId = playerId;
        return true;
    }

    private bool IsValidPermissionHolder(int toolId, ulong playerId)
    {
        return _currentGrabbingToolId == toolId && 
               _currentGrabbingPlayerId == playerId;
    }

    /* 更新循环 */
    protected virtual void Update()
    {
        _stateStack.Peek().Update(); // 驱动当前状态更新
    }

    // 权限清理方法
    public void ClearGrabPermission()
    {
        _currentGrabbingToolId = -1;
        _currentGrabbingPlayerId = ulong.MaxValue;
    }
    
    /* 状态查询属性 */
    public bool IsNotSelected => CurrentState == EItemState.NotSelected;
    public bool IsSelected => CurrentState == EItemState.Selected;
    public bool IsReadyToGrab => CurrentState == EItemState.ReadyToGrab;
    public bool IsGrabbed => CurrentState == EItemState.Grabbed;
}