using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerControlInformationProcess : MonoBehaviour
{
    // 控制模式枚举
    public enum ControlMode
    {
        LegControl, // 腿部控制
        HandControl // 手部控制
    }

    // 当前控制模式
    private ControlMode m_currentControlMode = ControlMode.LegControl;

    // 鼠标控制组件
    private MouseControl m_mouseControl;

    // 封装后的事件
    public UnityEvent onLiftLeftLeg; // 抬起左腿
    public UnityEvent onLiftRightLeg; // 抬起右腿
    public UnityEvent onReleaseLeftLeg; // 释放左腿
    public UnityEvent onReleaseRightLeg; // 释放右腿
    public UnityEvent onCancelLegGrab; // 取消腿部抓取

    public UnityEvent onLiftLeftHand; // 抬起左手
    public UnityEvent onLiftRightHand; // 抬起右手
    public UnityEvent onReleaseLeftHand; // 释放左手
    public UnityEvent onReleaseRightHand; // 释放右手
    public UnityEvent onCancelHandGrab; // 取消手部抓取

    public UnityEvent onSwitchControlMode; // 切换手-腿控制
    public UnityEvent<Vector2> onMouseMove; // 鼠标相对位移
    public UnityEvent onDefaultMode; // 默认模式

    void Awake()
    {
        // 初始化所有事件
        if (onLiftLeftLeg == null) onLiftLeftLeg = new UnityEvent();
        if (onLiftRightLeg == null) onLiftRightLeg = new UnityEvent();
        if (onReleaseLeftLeg == null) onReleaseLeftLeg = new UnityEvent();
        if (onReleaseRightLeg == null) onReleaseRightLeg = new UnityEvent();
        if (onCancelLegGrab == null) onCancelLegGrab = new UnityEvent();

        if (onLiftLeftHand == null) onLiftLeftHand = new UnityEvent();
        if (onLiftRightHand == null) onLiftRightHand = new UnityEvent();
        if (onReleaseLeftHand == null) onReleaseLeftHand = new UnityEvent();
        if (onReleaseRightHand == null) onReleaseRightHand = new UnityEvent();
        if (onCancelHandGrab == null) onCancelHandGrab = new UnityEvent();

        if (onSwitchControlMode == null) onSwitchControlMode = new UnityEvent();
        if (onMouseMove == null) onMouseMove = new UnityEvent<Vector2>();
        if (onDefaultMode == null) onDefaultMode = new UnityEvent();

        // 自动添加 MouseControl 组件
        m_mouseControl = gameObject.AddComponent<MouseControl>();

        // 添加事件监听
        m_mouseControl.onLeftMouseDown.AddListener(OnLeftMouseDown);
        m_mouseControl.onRightMouseDown.AddListener(OnRightMouseDown);
        m_mouseControl.onLeftMouseUp.AddListener(OnLeftMouseUp);
        m_mouseControl.onRightMouseUp.AddListener(OnRightMouseUp);
        m_mouseControl.onBothMouseButtonsDown.AddListener(OnBothMouseButtonsDown);
        m_mouseControl.onMiddleMouseDown.AddListener(OnMiddleMouseDown);
        m_mouseControl.onMouseMove.AddListener(OnMouseMove);
        m_mouseControl.onNoMouseButtonDown.AddListener(OnNoMouseButtonDown);
    }

    void OnDestroy()
    {
        // 取消订阅事件
        if (m_mouseControl != null)
        {
            m_mouseControl.onLeftMouseDown.RemoveListener(OnLeftMouseDown);
            m_mouseControl.onRightMouseDown.RemoveListener(OnRightMouseDown);
            m_mouseControl.onLeftMouseUp.RemoveListener(OnLeftMouseUp);
            m_mouseControl.onRightMouseUp.RemoveListener(OnRightMouseUp);
            m_mouseControl.onBothMouseButtonsDown.RemoveListener(OnBothMouseButtonsDown);
            m_mouseControl.onMiddleMouseDown.RemoveListener(OnMiddleMouseDown);
            m_mouseControl.onMouseMove.RemoveListener(OnMouseMove);
            m_mouseControl.onNoMouseButtonDown.RemoveListener(OnNoMouseButtonDown);
        }
    }

    // 鼠标左键按下事件处理
    private void OnLeftMouseDown()
    {
        if (m_currentControlMode == ControlMode.LegControl)
        {
            onLiftLeftLeg?.Invoke(); // 抬起左腿
        }
        else if (m_currentControlMode == ControlMode.HandControl)
        {
            onLiftLeftHand?.Invoke(); // 抬起左手
        }
    }

    // 鼠标右键按下事件处理
    private void OnRightMouseDown()
    {
        if (m_currentControlMode == ControlMode.LegControl)
        {
            onLiftRightLeg?.Invoke(); // 抬起右腿
        }
        else if (m_currentControlMode == ControlMode.HandControl)
        {
            onLiftRightHand?.Invoke(); // 抬起右手
        }
    }

    // 鼠标左键抬起事件处理
    private void OnLeftMouseUp()
    {
        if (m_currentControlMode == ControlMode.LegControl)
        {
            onReleaseLeftLeg?.Invoke(); // 释放左腿
        }
        else if (m_currentControlMode == ControlMode.HandControl)
        {
            onReleaseLeftHand?.Invoke(); // 释放左手
        }
    }

    // 鼠标右键抬起事件处理
    private void OnRightMouseUp()
    {
        if (m_currentControlMode == ControlMode.LegControl)
        {
            onReleaseRightLeg?.Invoke(); // 释放右腿
        }
        else if (m_currentControlMode == ControlMode.HandControl)
        {
            onReleaseRightHand?.Invoke(); // 释放右手
        }
    }

    // 鼠标左右键同时按下事件处理
    private void OnBothMouseButtonsDown()
    {
        if (m_currentControlMode == ControlMode.LegControl)
        {
            onCancelLegGrab?.Invoke(); // 取消腿部抓取
        }
        else if (m_currentControlMode == ControlMode.HandControl)
        {
            onCancelHandGrab?.Invoke(); // 取消手部抓取
        }
    }

    // 鼠标中键按下事件处理（切换控制模式）
    private void OnMiddleMouseDown()
    {
        // 切换控制模式
        m_currentControlMode = (m_currentControlMode == ControlMode.LegControl) ? ControlMode.HandControl : ControlMode.LegControl;
        onSwitchControlMode?.Invoke(); // 发布切换控制模式事件
    }

    // 鼠标移动事件处理
    private void OnMouseMove(Vector2 mouseDelta)
    {
        onMouseMove?.Invoke(mouseDelta); // 发布鼠标移动量

    }

    // 鼠标无按键操作事件处理
    private void OnNoMouseButtonDown()
    {
        onDefaultMode?.Invoke(); // 进入默认模式
    }

    public void SetSensitivity(float newSensitivity)
    {
        if (m_mouseControl)
        {
            m_mouseControl.MouseSensitivity *= newSensitivity;
        }
    }
}