using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlInformationManager : MonoBehaviour
{
    private PlayerControlInformationProcess _controlProcess;

    private void Awake()
    {
        _controlProcess = GetComponent<PlayerControlInformationProcess>();
        
        // 订阅事件
        EventBus<ControlModeChangedEvent>.Subscribe(OnControlModeChanged, this);
        EventBus<ControlLockEvent>.Subscribe(OnControlLocked, this);
        EventBus<UnlockAllControlsEvent>.Subscribe(OnAllControlsUnlocked, this);
    }

    private void OnDestroy()
    {
        // 取消订阅
        EventBus<ControlModeChangedEvent>.UnsubscribeAll(this);
        EventBus<ControlLockEvent>.UnsubscribeAll(this);
        EventBus<UnlockAllControlsEvent>.UnsubscribeAll(this);
    }

    private void OnControlModeChanged(ControlModeChangedEvent e)
    {
        _controlProcess.mCurrentControlMode = e.NewMode;
    }

    private void OnControlLocked(ControlLockEvent e)
    {
        if (e.LockAll)
        {
            _controlProcess.stopPlayerControl = true;
        }
        else
        {
            _controlProcess.EnableLegControl(!e.LockLegs);
            _controlProcess.EnableHandControl(!e.LockHands);
        }
    }

    private void OnAllControlsUnlocked(UnlockAllControlsEvent _)
    {
        _controlProcess.stopPlayerControl = false;
        _controlProcess.EnableLegControl(true);
        _controlProcess.EnableHandControl(true);
    }
}
