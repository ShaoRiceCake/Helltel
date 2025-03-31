using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerControlInformationProcess : MonoBehaviour
{
    public enum ControlMode
    {
        LegControl,
        HandControl
    }

    public ControlMode m_currentControlMode = ControlMode.LegControl;
    private MouseControl m_mouseControl;

    public UnityEvent onLiftLeftLeg;
    public UnityEvent onLiftRightLeg;
    public UnityEvent onReleaseLeftLeg;
    public UnityEvent onReleaseRightLeg;
    public UnityEvent onCancelLegGrab;

    public UnityEvent onLiftLeftHand;
    public UnityEvent onLiftRightHand;
    public UnityEvent onReleaseLeftHand;
    public UnityEvent onReleaseRightHand;
    public UnityEvent onCancelHandGrab;

    public UnityEvent onSwitchControlMode;
    public UnityEvent<Vector2> onMouseMoveFixedUpdate; // 固定时间步长相对运动
    public UnityEvent<Vector2> onMouseMoveUpdate;     // 每帧相对运动
    public UnityEvent onDefaultMode;

    void Awake()
    {
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
        if (onMouseMoveFixedUpdate == null) onMouseMoveFixedUpdate = new UnityEvent<Vector2>();
        if (onMouseMoveUpdate == null) onMouseMoveUpdate = new UnityEvent<Vector2>();
        if (onDefaultMode == null) onDefaultMode = new UnityEvent();

        m_mouseControl = gameObject.AddComponent<MouseControl>();

        m_mouseControl.onLeftMouseDown.AddListener(OnLeftMouseDown);
        m_mouseControl.onRightMouseDown.AddListener(OnRightMouseDown);
        m_mouseControl.onLeftMouseUp.AddListener(OnLeftMouseUp);
        m_mouseControl.onRightMouseUp.AddListener(OnRightMouseUp);
        m_mouseControl.onBothMouseButtonsDown.AddListener(OnBothMouseButtonsDown);
        m_mouseControl.onMiddleMouseDown.AddListener(OnMiddleMouseDown);
        m_mouseControl.onMouseMoveFixedUpdate.AddListener(OnMouseMoveFixedUpdate);
        m_mouseControl.onMouseMoveUpdate.AddListener(OnMouseMoveUpdate);
        m_mouseControl.onNoMouseButtonDown.AddListener(OnNoMouseButtonDown);
    }

    void OnDestroy()
    {
        if (m_mouseControl != null)
        {
            m_mouseControl.onLeftMouseDown.RemoveListener(OnLeftMouseDown);
            m_mouseControl.onRightMouseDown.RemoveListener(OnRightMouseDown);
            m_mouseControl.onLeftMouseUp.RemoveListener(OnLeftMouseUp);
            m_mouseControl.onRightMouseUp.RemoveListener(OnRightMouseUp);
            m_mouseControl.onBothMouseButtonsDown.RemoveListener(OnBothMouseButtonsDown);
            m_mouseControl.onMiddleMouseDown.RemoveListener(OnMiddleMouseDown);
            m_mouseControl.onMouseMoveFixedUpdate.RemoveListener(OnMouseMoveFixedUpdate);
            m_mouseControl.onMouseMoveUpdate.RemoveListener(OnMouseMoveUpdate);
            m_mouseControl.onNoMouseButtonDown.RemoveListener(OnNoMouseButtonDown);
        }
    }

    private void OnLeftMouseDown()
    {
        if (m_currentControlMode == ControlMode.LegControl)
        {
            onLiftLeftLeg?.Invoke();
        }
        else if (m_currentControlMode == ControlMode.HandControl)
        {
            onLiftLeftHand?.Invoke();
        }
    }

    private void OnRightMouseDown()
    {
        if (m_currentControlMode == ControlMode.LegControl)
        {
            onLiftRightLeg?.Invoke();
        }
        else if (m_currentControlMode == ControlMode.HandControl)
        {
            onLiftRightHand?.Invoke();
        }
    }

    private void OnLeftMouseUp()
    {
        if (m_currentControlMode == ControlMode.LegControl)
        {
            onReleaseLeftLeg?.Invoke();
        }
        else if (m_currentControlMode == ControlMode.HandControl)
        {
            onReleaseLeftHand?.Invoke();
        }
    }

    private void OnRightMouseUp()
    {
        if (m_currentControlMode == ControlMode.LegControl)
        {
            onReleaseRightLeg?.Invoke();
        }
        else if (m_currentControlMode == ControlMode.HandControl)
        {
            onReleaseRightHand?.Invoke();
        }
    }

    private void OnBothMouseButtonsDown()
    {
        if (m_currentControlMode == ControlMode.LegControl)
        {
            onCancelLegGrab?.Invoke();
        }
        else if (m_currentControlMode == ControlMode.HandControl)
        {
            onCancelHandGrab?.Invoke();
        }
    }

    private void OnMiddleMouseDown()
    {
        m_currentControlMode = (m_currentControlMode == ControlMode.LegControl) ? ControlMode.HandControl : ControlMode.LegControl;
        onSwitchControlMode?.Invoke();
    }

    private void OnMouseMoveFixedUpdate(Vector2 mouseDelta)
    {
        onMouseMoveFixedUpdate?.Invoke(mouseDelta);
    }

    private void OnMouseMoveUpdate(Vector2 mouseDelta)
    {
        onMouseMoveUpdate?.Invoke(mouseDelta);
    }

    private void OnNoMouseButtonDown()
    {
        onDefaultMode?.Invoke();
    }

    public void SetSensitivity(float newSensitivity)
    {
        if (m_mouseControl)
        {
            m_mouseControl.MouseSensitivity *= newSensitivity;
        }
    }
}