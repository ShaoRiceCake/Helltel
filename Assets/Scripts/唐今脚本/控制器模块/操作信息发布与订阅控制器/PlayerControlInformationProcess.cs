using System;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
public class PlayerControlInformationProcess : NetworkBehaviour
{
    public enum ControlMode
    {
        LegControl,
        HandControl
    }

    public ControlMode mCurrentControlMode = ControlMode.LegControl;
    private MouseControl _mMouseControl;

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
    public UnityEvent<Vector2> onMouseMoveFixedUpdate; 
    public UnityEvent<Vector2> onMouseMoveUpdate;   
    public UnityEvent onDefaultMode;

    void Start()
    {
        onLiftLeftLeg ??= new UnityEvent();
        onLiftRightLeg ??= new UnityEvent();
        onReleaseLeftLeg ??= new UnityEvent();
        onReleaseRightLeg ??= new UnityEvent();
        onCancelLegGrab ??= new UnityEvent();

        onLiftLeftHand ??= new UnityEvent();
        onLiftRightHand ??= new UnityEvent();
        onReleaseLeftHand ??= new UnityEvent();
        onReleaseRightHand ??= new UnityEvent();
        onCancelHandGrab ??= new UnityEvent();

        onSwitchControlMode ??= new UnityEvent();
        onMouseMoveFixedUpdate ??= new UnityEvent<Vector2>();
        onMouseMoveUpdate ??= new UnityEvent<Vector2>();
        onDefaultMode ??= new UnityEvent();

        _mMouseControl = gameObject.AddComponent<MouseControl>();

        _mMouseControl.onLeftMouseDown.AddListener(OnLeftMouseDown);
        _mMouseControl.onRightMouseDown.AddListener(OnRightMouseDown);
        _mMouseControl.onLeftMouseUp.AddListener(OnLeftMouseUp);
        _mMouseControl.onRightMouseUp.AddListener(OnRightMouseUp);
        _mMouseControl.onBothMouseButtonsDown.AddListener(OnBothMouseButtonsDown);
        _mMouseControl.onMiddleMouseDown.AddListener(OnMiddleMouseDown);
        _mMouseControl.onMouseMoveFixedUpdate.AddListener(OnMouseMoveFixedUpdate);
        _mMouseControl.onMouseMoveUpdate.AddListener(OnMouseMoveUpdate);
        _mMouseControl.onNoMouseButtonDown.AddListener(OnNoMouseButtonDown);
    }

    public override void OnDestroy()
    {
        if (_mMouseControl == null) return;
        _mMouseControl.onLeftMouseDown.RemoveListener(OnLeftMouseDown);
        _mMouseControl.onRightMouseDown.RemoveListener(OnRightMouseDown);
        _mMouseControl.onLeftMouseUp.RemoveListener(OnLeftMouseUp);
        _mMouseControl.onRightMouseUp.RemoveListener(OnRightMouseUp);
        _mMouseControl.onBothMouseButtonsDown.RemoveListener(OnBothMouseButtonsDown);
        _mMouseControl.onMiddleMouseDown.RemoveListener(OnMiddleMouseDown);
        _mMouseControl.onMouseMoveFixedUpdate.RemoveListener(OnMouseMoveFixedUpdate);
        _mMouseControl.onMouseMoveUpdate.RemoveListener(OnMouseMoveUpdate);
        _mMouseControl.onNoMouseButtonDown.RemoveListener(OnNoMouseButtonDown);

        base.OnDestroy();
    }

    private void OnLeftMouseDown()
    {
        switch (mCurrentControlMode)
        {
            case ControlMode.LegControl:
                onLiftLeftLeg?.Invoke();
                break;
            case ControlMode.HandControl:
                onLiftLeftHand?.Invoke();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnRightMouseDown()
    {
        switch (mCurrentControlMode)
        {
            case ControlMode.LegControl:
                onLiftRightLeg?.Invoke();
                break;
            case ControlMode.HandControl:
                onLiftRightHand?.Invoke();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnLeftMouseUp()
    {
        switch (mCurrentControlMode)
        {
            case ControlMode.LegControl:
                onReleaseLeftLeg?.Invoke();
                break;
            case ControlMode.HandControl:
                onReleaseLeftHand?.Invoke();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnRightMouseUp()
    {
        switch (mCurrentControlMode)
        {
            case ControlMode.LegControl:
                onReleaseRightLeg?.Invoke();
                break;
            case ControlMode.HandControl:
                onReleaseRightHand?.Invoke();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnBothMouseButtonsDown()
    {
        switch (mCurrentControlMode)
        {
            case ControlMode.LegControl:
                onCancelLegGrab?.Invoke();
                break;
            case ControlMode.HandControl:
                onCancelHandGrab?.Invoke();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnMiddleMouseDown()
    {
        mCurrentControlMode = (mCurrentControlMode == ControlMode.LegControl) ? ControlMode.HandControl : ControlMode.LegControl;
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
        if (_mMouseControl)
        {
            _mMouseControl.MouseSensitivity *= newSensitivity;
        }
    }
}