using System;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
using System.Collections;
using UnityEngine.Serialization;

public class PlayerControlInformationProcess : MonoBehaviour
{
    public enum ControlMode
    {
        LegControl,
        HandControl
    }
    
    public bool isTest = false;

    public ControlMode mCurrentControlMode = ControlMode.LegControl;
    private MouseControl _mMouseControl;

    // Control permission flags
    [Header("Control Permissions")]
    public bool legControlEnabled = true;
    public bool handControlEnabled = true;
    
    [Header("Events")]
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
    public UnityEvent onCameraControl;
    public UnityEvent onStopCameraControl;
    
    public bool isCameraControlActive;
    public bool stopPlayerControl;

    private void Start()
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
        onCameraControl ??= new UnityEvent(); 
        onStopCameraControl ??= new UnityEvent(); 
        
        _mMouseControl = GetComponent<MouseControl>();
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

    public void OnDestroy()
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
    }

    private void Update()
    {
        if (stopPlayerControl) return;
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCameraControlActive = true;
            onCameraControl?.Invoke();
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            onStopCameraControl?.Invoke();
            isCameraControlActive = false;
        }
    }

    private void OnLeftMouseDown()
    {
        if (stopPlayerControl || isCameraControlActive) return;
        
        switch (mCurrentControlMode)
        {
            case ControlMode.LegControl:
                if (legControlEnabled) onLiftLeftLeg?.Invoke();
                break;
            case ControlMode.HandControl:
                if (handControlEnabled) onLiftLeftHand?.Invoke();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnRightMouseDown()
    {
        if (stopPlayerControl || isCameraControlActive) return;
        
        switch (mCurrentControlMode)
        {
            case ControlMode.LegControl:
                if (legControlEnabled) onLiftRightLeg?.Invoke();
                break;
            case ControlMode.HandControl:
                if (handControlEnabled) onLiftRightHand?.Invoke();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnLeftMouseUp()
    {
        if (stopPlayerControl || isCameraControlActive) return;
        
        switch (mCurrentControlMode)
        {
            case ControlMode.LegControl:
                if (legControlEnabled) onReleaseLeftLeg?.Invoke();
                break;
            case ControlMode.HandControl:
                if (handControlEnabled) onReleaseLeftHand?.Invoke();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnRightMouseUp()
    {
        if (stopPlayerControl || isCameraControlActive) return;
        
        switch (mCurrentControlMode)
        {
            case ControlMode.LegControl:
                if (legControlEnabled) onReleaseRightLeg?.Invoke();
                break;
            case ControlMode.HandControl:
                if (handControlEnabled) onReleaseRightHand?.Invoke();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnBothMouseButtonsDown()
    {
        if (stopPlayerControl || isCameraControlActive) return;
        
        switch (mCurrentControlMode)
        {
            case ControlMode.LegControl:
                if (legControlEnabled) onCancelLegGrab?.Invoke();
                break;
            case ControlMode.HandControl:
                if (handControlEnabled) onCancelHandGrab?.Invoke();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

// 在 OnMiddleMouseDown 方法中添加控制粒子显示的代码
    private void OnMiddleMouseDown()
    {
        if (stopPlayerControl || isCameraControlActive) return;
    
        mCurrentControlMode = (mCurrentControlMode == ControlMode.LegControl) ? ControlMode.HandControl : ControlMode.LegControl;
    
        // 切换控制模式时更新所有肢体的粒子显示状态
        var footControls = GetComponentsInChildren<PlayerControl_FootControl>();
        var handControls = GetComponentsInChildren<PlayerControl_HandControl>();
    
        foreach (var foot in footControls)
        {
            foot.shouldShowParticles = (mCurrentControlMode == ControlMode.LegControl);
        }
    
        foreach (var hand in handControls)
        {
            hand.shouldShowParticles = (mCurrentControlMode == ControlMode.HandControl);
        }
    
        onSwitchControlMode?.Invoke();
        AudioManager.Instance.Play("玩家手-腿控制切换",transform.position,1f);
    }

    private void OnMouseMoveFixedUpdate(Vector2 mouseDelta)
    {
        if (stopPlayerControl) return;
        onMouseMoveFixedUpdate?.Invoke(mouseDelta);
    }

    private void OnMouseMoveUpdate(Vector2 mouseDelta)
    {
        if (stopPlayerControl) return;
        onMouseMoveUpdate?.Invoke(mouseDelta);
    }

    private void OnNoMouseButtonDown()
    {
        if (stopPlayerControl) return;
        onDefaultMode?.Invoke();
    }

    public void SetSensitivity(float newSensitivity)
    {
        if (_mMouseControl)
        {
            _mMouseControl.MouseSensitivity *= newSensitivity;
        }
    }

    // Public methods to control permissions
    public void EnableLegControl(bool enable)
    {
        legControlEnabled = enable;
    }

    public void EnableHandControl(bool enable)
    {
        handControlEnabled = enable;
    }

    // Temporary disable functionality
    public void TemporarilyDisableControl(float duration)
    {
        StartCoroutine(TemporaryDisableRoutine(duration));
    }

    private IEnumerator TemporaryDisableRoutine(float duration)
    {
        var originalLegState = legControlEnabled;
        var originalHandState = handControlEnabled;
        var originalStopState = stopPlayerControl;

        // Disable all controls
        legControlEnabled = false;
        handControlEnabled = false;
        stopPlayerControl = true;

        yield return new WaitForSeconds(duration);

        // Restore original states
        legControlEnabled = originalLegState;
        handControlEnabled = originalHandState;
        stopPlayerControl = originalStopState;
    }
}