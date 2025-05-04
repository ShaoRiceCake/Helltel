using System;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
using System.Collections;

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
    
    private bool _isCameraControlActive;
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
            _isCameraControlActive = true;
            onCameraControl?.Invoke();
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            onStopCameraControl?.Invoke();
            _isCameraControlActive = false;
        }
    }

    private void OnLeftMouseDown()
    {
        if (stopPlayerControl || _isCameraControlActive) return;
        
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
        if (stopPlayerControl || _isCameraControlActive) return;
        
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
        if (stopPlayerControl || _isCameraControlActive) return;
        
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
        if (stopPlayerControl || _isCameraControlActive) return;
        
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
        if (stopPlayerControl || _isCameraControlActive) return;
        
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

    private void OnMiddleMouseDown()
    {
        if (stopPlayerControl || _isCameraControlActive) return;
        
        mCurrentControlMode = (mCurrentControlMode == ControlMode.LegControl) ? ControlMode.HandControl : ControlMode.LegControl;
        onSwitchControlMode?.Invoke();
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